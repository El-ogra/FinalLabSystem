using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.Views.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PatientRegistrationViewModel : ViewModelBase, IAsyncInitializable
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly INavigationService _navigationService;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IDialogService _dialogService;
    private int _currentPatientId;
    private int _currentVisitId;
    private bool _isEditMode;
    private bool _hasUnsavedChanges;
    private DateTime _entryDate = DateTime.Now;
    private DateTime? _expectedReady = DateTime.Now.AddDays(1);

    public PatientRegistrationViewModel(
        PatientInfoViewModel patientInfo,
        ReferralViewModel referral,
        MedicalHistoryViewModel medicalHistory,
        TestSelectionViewModel testSelection,
        FinancialViewModel financial,
        IVisitService visitService,
        IPatientService patientService,
        ISampleTrackingService sampleTrackingService,
        INavigationService navigationService,
        ICurrentUserSession currentUserSession,
        IDialogService dialogService)
    {
        PatientInfo = patientInfo;
        Referral = referral;
        MedicalHistory = medicalHistory;
        TestSelection = testSelection;
        Financial = financial;
        _visitService = visitService;
        _patientService = patientService;
        _sampleTrackingService = sampleTrackingService;
        _navigationService = navigationService;
        _currentUserSession = currentUserSession;
        _dialogService = dialogService;
        TodayPatients = new ObservableCollection<TodayPatientDto>();

        TestSelection.TestsChanged += (_, _) =>
        {
            Financial.RecalculateFromTests(TestSelection.GetSelectedPrices());
            HasUnsavedChanges = true;
        };

        AddNewCommand = new AsyncRelayCommand(AddNewAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        EditCommand = new AsyncRelayCommand(EditAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => CurrentVisitId > 0);
        BarcodeCommand = new AsyncRelayCommand(BarcodeAsync, () => CurrentVisitId > 0);
        ReceiptCommand = new AsyncRelayCommand(ReceiptAsync, () => CurrentVisitId > 0);
        ReturnToMainCommand = new RelayCommand(_ => ReturnToMain());
        LoadTodayPatientsCommand = new AsyncRelayCommand(LoadTodayPatientsAsync);
    }

    public async Task InitializeAsync()
    {
        try
        {
            await PatientInfo.InitializeAsync();
            await Referral.InitializeAsync();
            await AddNewAsync();
        }
        catch
        {
            _dialogService.ShowError("حدث خطأ أثناء تهيئة النموذج.");
        }
    }

    public PatientInfoViewModel PatientInfo { get; }

    public ReferralViewModel Referral { get; }

    public MedicalHistoryViewModel MedicalHistory { get; }

    public TestSelectionViewModel TestSelection { get; }

    public FinancialViewModel Financial { get; }

    public ObservableCollection<TodayPatientDto> TodayPatients { get; }

    public int CurrentPatientId
    {
        get => _currentPatientId;
        private set => SetProperty(ref _currentPatientId, value);
    }

    public int CurrentVisitId
    {
        get => _currentVisitId;
        private set
        {
            if (SetProperty(ref _currentVisitId, value))
            {
                Financial.SetCurrentVisitId(value);
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public DateTime EntryDate
    {
        get => _entryDate;
        set
        {
            if (SetProperty(ref _entryDate, value))
                HasUnsavedChanges = true;
        }
    }

    public DateTime? ExpectedReady
    {
        get => _expectedReady;
        set
        {
            if (SetProperty(ref _expectedReady, value))
                HasUnsavedChanges = true;
        }
    }

    public ICommand AddNewCommand { get; }

    public ICommand SaveCommand { get; }

    public ICommand EditCommand { get; }

    public ICommand DeleteCommand { get; }

    public ICommand BarcodeCommand { get; }

    public ICommand ReceiptCommand { get; }

    public ICommand ReturnToMainCommand { get; }

    public ICommand LoadTodayPatientsCommand { get; }

    private async Task AddNewAsync()
    {
        CurrentPatientId = 0;
        CurrentVisitId = 0;
        IsEditMode = false;
        EntryDate = DateTime.Now;
        ExpectedReady = DateTime.Now.AddDays(1);
        PatientInfo.ClearAllFields();
        Referral.ClearAllFields();
        MedicalHistory.ClearAllFields();
        TestSelection.ClearAll();
        Financial.ClearAllFields();
        await PatientInfo.GenerateCodeAsync();
        HasUnsavedChanges = false;
    }

    private async Task SaveAsync()
    {
        if (PatientInfo.HasErrors)
        {
            _dialogService.ShowWarning("يجب إدخال اسم المريض وتحديد النوع.", "تحقق");
            return;
        }

        var selectedTestIds = TestSelection.GetSelectedTestTypeIds();
        if (selectedTestIds.Count == 0)
        {
            _dialogService.ShowWarning("يجب اختيار تحليل واحد على الأقل.", "تحقق");
            return;
        }

        var patient = PatientInfo.ToPatient();
        patient.PatientId = CurrentPatientId;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;
        patient.CreatedBy = staffId;

        var visit = new Visit
        {
            VisitId = CurrentVisitId,
            VisitCode = PatientInfo.PatientCode,
            PatientId = CurrentPatientId,
            VisitDate = EntryDate,
            ExpectedReady = ExpectedReady,
            IsPregnant = MedicalHistory.IsPregnant,
            IsFasting = MedicalHistory.IsFasting,
            FastingHours = MedicalHistory.FastingHours,
            TakenOutsideLab = MedicalHistory.TakenOutsideLab,
            OutsideUrine = MedicalHistory.OutsideUrine,
            OutsideStool = MedicalHistory.OutsideStool,
            OutsideBlood = MedicalHistory.OutsideBlood,
            OutsideSemen = MedicalHistory.OutsideSemen,
            OutsideCsf = MedicalHistory.OutsideCsf,
            HasDiabetes = MedicalHistory.HasDiabetes,
            HasAnemia = MedicalHistory.HasAnemia,
            HasBleedingDisorder = MedicalHistory.HasBleedingDisorder,
            HasThyroid = MedicalHistory.HasThyroid,
            HasJointDisease = MedicalHistory.HasJointDisease,
            HasViralInfection = MedicalHistory.HasViralInfection,
            OnAnticoagulant = MedicalHistory.OnAnticoagulant,
            HasHypertension = MedicalHistory.HasHypertension,
            HasLiverDisease = MedicalHistory.HasLiverDisease,
            HasKidneyDisease = MedicalHistory.HasKidneyDisease,
            HasLupus = MedicalHistory.HasLupus,
            HadXrayContrast = MedicalHistory.HadXrayContrast,
            ReferralId = Referral.SelectedReferral?.ReferralId,
            Subtotal = Financial.Subtotal,
            DiscountAmount = Financial.DiscountAmount,
            DiscountPercent = Financial.DiscountPercent,
            TotalAfterDiscount = Financial.TotalAfterDiscount,
            TotalPaid = Financial.AmountPaid,
            BalanceDue = Financial.BalanceDue,
            PaymentStatus = Financial.BalanceDue <= 0 ? PaymentStatus.Paid : Financial.AmountPaid > 0 ? PaymentStatus.PartiallyPaid : PaymentStatus.Pending,
            VisitStatus = VisitStatus.Open,
            ReceptionistId = staffId,
            Notes = MedicalHistory.VisitNotes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var referralToSave = Referral.ShouldSaveReferral ? Referral.ToReferralSource() : null;
        var savedVisit = await _visitService.SavePatientVisitAsync(
            patient,
            visit,
            selectedTestIds,
            Financial.AmountPaid,
            staffId,
            MedicalHistory.ToMedicalHistoryList(),
            referralToSave);

        CurrentPatientId = savedVisit.PatientId;
        CurrentVisitId = savedVisit.VisitId;
        Financial.SetCurrentVisitId(savedVisit.VisitId);
        IsEditMode = true;
        HasUnsavedChanges = false;
        _dialogService.ShowMessage("تم حفظ بيانات المريض والزيارة.", "حفظ");
    }

    private async Task EditAsync()
    {
        var viewModel = new TodayPatientsDialogViewModel(_visitService);
        var dialog = new TodayPatientsDialog(viewModel)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };

        if (dialog.ShowDialog() == true && dialog.SelectedPatient is not null)
            await LoadVisitForEditAsync(dialog.SelectedPatient.VisitId);
    }

    public async Task LoadVisitForEditAsync(int visitId)
    {
        var dto = await _visitService.GetVisitFullDataAsync(visitId);
        CurrentPatientId = dto.PatientId;
        CurrentVisitId = dto.VisitId;
        EntryDate = dto.EntryDate;
        ExpectedReady = dto.ExpectedReady;
        PatientInfo.LoadPatient(dto);
        Referral.LoadFromDto(dto);
        MedicalHistory.LoadFromVisit(dto);
        TestSelection.LoadSelectedTests(dto.SelectedTests);
        Financial.LoadFromDto(dto);
        IsEditMode = true;
        HasUnsavedChanges = false;
    }

    private async Task DeleteAsync()
    {
        if (CurrentVisitId <= 0)
            return;

        if (!_dialogService.ShowConfirmation("هل تريد حذف الزيارة الحالية؟", "حذف"))
            return;

        var deleted = await _visitService.CancelVisitAsync(CurrentVisitId);
        if (deleted)
            await AddNewAsync();
    }

    private async Task BarcodeAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;
        await _sampleTrackingService.GenerateBarcodesForVisitAsync(CurrentVisitId, staffId);
        var viewModel = App.ServiceProvider.GetRequiredService<BarcodeDialogViewModel>();
        await viewModel.LoadTubesAsync(CurrentVisitId);
        var dialog = new BarcodeDialog(viewModel)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };
        dialog.ShowDialog();
    }

    private async Task ReceiptAsync()
    {
        var dto = await _visitService.GetVisitFullDataAsync(CurrentVisitId);
        var viewModel = App.ServiceProvider.GetRequiredService<ReceiptDialogViewModel>();
        viewModel.Initialize(dto);
        var dialog = new ReceiptDialog(viewModel)
        {
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(window => window.IsActive)
        };
        dialog.ShowDialog();
    }

    private async Task LoadTodayPatientsAsync()
    {
        var patients = await _visitService.GetTodayPatientListAsync();
        TodayPatients.Clear();
        foreach (var patient in patients)
            TodayPatients.Add(patient);
    }

    private void ReturnToMain()
    {
        if (HasUnsavedChanges)
        {
            if (!_dialogService.ShowConfirmation("توجد تغييرات غير محفوظة. هل تريد العودة؟", "تنبيه"))
                return;
        }

        _navigationService.ReturnToMain();
    }
}
