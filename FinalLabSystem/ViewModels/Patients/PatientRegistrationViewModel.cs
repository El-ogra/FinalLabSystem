using System.Windows;
using System.Windows.Input;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PatientRegistrationViewModel : ViewModelBase
{
    private readonly IVisitService _visitService;
    private readonly IPatientService _patientService;
    private readonly ISampleTrackingService _sampleTrackingService;
    private readonly INavigationService _navigationService;
    private readonly ICurrentUserSession _currentUserSession;
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
        ICurrentUserSession currentUserSession)
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

        TestSelection.TestsChanged += (_, _) =>
        {
            Financial.RecalculateFromTests(TestSelection.GetSelectedPrices());
            HasUnsavedChanges = true;
        };

        AddNewCommand = new AsyncRelayCommand(AddNewAsync);
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        EditCommand = new RelayCommand(_ => IsEditMode = true);
        DeleteCommand = new RelayCommand(_ => MessageBox.Show("الحذف يحتاج اختيار مريض محفوظ وسياسة حذف نهائية.", "حذف", MessageBoxButton.OK, MessageBoxImage.Information));
        BarcodeCommand = new AsyncRelayCommand(BarcodeAsync, () => CurrentVisitId > 0);
        ReceiptCommand = new RelayCommand(_ => MessageBox.Show("الإيصال متاح بعد اكتمال قالب الطباعة النهائي.", "الإيصال", MessageBoxButton.OK, MessageBoxImage.Information), _ => CurrentVisitId > 0);
        ReturnToMainCommand = new RelayCommand(_ => ReturnToMain());
        LoadTodayPatientsCommand = new AsyncRelayCommand(LoadTodayPatientsAsync);
        _ = AddNewAsync();
    }

    public PatientInfoViewModel PatientInfo { get; }

    public ReferralViewModel Referral { get; }

    public MedicalHistoryViewModel MedicalHistory { get; }

    public TestSelectionViewModel TestSelection { get; }

    public FinancialViewModel Financial { get; }

    public int CurrentPatientId
    {
        get => _currentPatientId;
        private set => SetProperty(ref _currentPatientId, value);
    }

    public int CurrentVisitId
    {
        get => _currentVisitId;
        private set => SetProperty(ref _currentVisitId, value);
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
        PatientInfo.LoadPatient(new Patient
        {
            PatientCode = string.Empty,
            FullNameAr = string.Empty,
            Sex = "U",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        });
        await PatientInfo.GenerateCodeAsync();
        Financial.LoadFinancials(0, 0, 0, 0);
        HasUnsavedChanges = false;
    }

    private async Task SaveAsync()
    {
        if (PatientInfo.HasErrors)
        {
            MessageBox.Show("يجب إدخال اسم المريض وتحديد النوع.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedTestIds = TestSelection.GetSelectedTestTypeIds();
        if (selectedTestIds.Count == 0)
        {
            MessageBox.Show("يجب اختيار تحليل واحد على الأقل.", "تحقق", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var patient = PatientInfo.ToPatient();
        patient.PatientId = CurrentPatientId;
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;
        patient.CreatedBy = staffId;

        var visit = new Visit
        {
            VisitId = CurrentVisitId,
            VisitCode = CurrentVisitId == 0 ? await _visitService.GenerateVisitCodeAsync() : $"V{CurrentVisitId}",
            PatientId = CurrentPatientId,
            VisitDate = EntryDate,
            ExpectedReady = ExpectedReady,
            IsPregnant = MedicalHistory.IsPregnant,
            IsFasting = MedicalHistory.IsFasting,
            FastingHours = MedicalHistory.FastingHours,
            ReferralId = Referral.SelectedReferral?.ReferralId,
            Subtotal = Convert.ToDouble(Financial.Subtotal),
            DiscountAmount = Convert.ToDouble(Financial.DiscountAmount),
            DiscountPercent = Convert.ToDouble(Financial.DiscountPercent),
            TotalAfterDiscount = Convert.ToDouble(Financial.TotalAfterDiscount),
            TotalPaid = Convert.ToDouble(Financial.AmountPaid),
            BalanceDue = Convert.ToDouble(Financial.BalanceDue),
            PaymentStatus = Financial.BalanceDue <= 0 ? "PAID" : Financial.AmountPaid > 0 ? "PARTIAL" : "PENDING",
            VisitStatus = "OPEN",
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
            Convert.ToDouble(Financial.AmountPaid),
            staffId,
            MedicalHistory.ToMedicalHistoryList(),
            referralToSave);

        CurrentPatientId = savedVisit.PatientId;
        CurrentVisitId = savedVisit.VisitId;
        HasUnsavedChanges = false;
        MessageBox.Show("تم حفظ بيانات المريض والزيارة.", "حفظ", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task BarcodeAsync()
    {
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 1;
        await _sampleTrackingService.GenerateBarcodesForVisitAsync(CurrentVisitId, staffId);
        MessageBox.Show("تم تجهيز باركود الأنابيب للزيارة الحالية.", "الباركود", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task LoadTodayPatientsAsync()
    {
        var visits = await _patientService.GetTodayPatientsAsync();
        MessageBox.Show($"عدد زيارات اليوم: {visits.Count}", "مرضى اليوم", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReturnToMain()
    {
        if (HasUnsavedChanges)
        {
            var result = MessageBox.Show("توجد تغييرات غير محفوظة. هل تريد العودة؟", "تنبيه", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        _navigationService.ReturnToMain();
    }
}
