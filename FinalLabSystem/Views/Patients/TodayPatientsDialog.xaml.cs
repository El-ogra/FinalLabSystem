using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Views.Patients;

public partial class TodayPatientsDialog : Window
{
    private readonly ICollectionView _patientsView;

    public TodayPatientsDialog()
        : this(new ObservableCollection<TodayPatientDto>())
    {
    }

    public TodayPatientsDialog(ObservableCollection<TodayPatientDto> todayPatients)
    {
        InitializeComponent();
        _patientsView = CollectionViewSource.GetDefaultView(todayPatients);
        _patientsView.Filter = FilterPatient;
        DataContext = _patientsView;
    }

    public TodayPatientDto? SelectedPatient { get; private set; }

    private bool FilterPatient(object item)
    {
        if (item is not TodayPatientDto patient)
            return false;

        var term = SearchBox?.Text?.Trim();
        if (string.IsNullOrWhiteSpace(term))
            return true;

        return patient.PatientCode.Contains(term, StringComparison.OrdinalIgnoreCase)
            || patient.FullNameAr.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _patientsView.Refresh();
    }

    private void SelectButton_OnClick(object sender, RoutedEventArgs e)
    {
        SelectCurrentPatient();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void PatientsList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        SelectCurrentPatient();
    }

    private void SelectCurrentPatient()
    {
        if (PatientsList.SelectedItem is not TodayPatientDto patient)
            return;

        SelectedPatient = patient;
        DialogResult = true;
        Close();
    }
}
