using System.Windows.Input;
using FinalLabSystem.Infrastructure;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class PatientNotesDialogViewModel : ViewModelBase
{
    private string _notes = string.Empty;

    public PatientNotesDialogViewModel(string currentNotes)
    {
        _notes = currentNotes;
        SaveCommand = new RelayCommand(_ => Save());
        CancelCommand = new RelayCommand(_ => Cancel());
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    public bool DialogResult { get; private set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void Save()
    {
        DialogResult = true;
    }

    private void Cancel()
    {
        DialogResult = false;
    }
}