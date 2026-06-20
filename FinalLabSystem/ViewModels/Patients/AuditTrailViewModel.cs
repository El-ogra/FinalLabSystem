using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Patients;

public sealed class AuditTrailViewModel : ViewModelBase
{
    public AuditTrailViewModel(string title, List<AuditLog> auditEntries)
    {
        Title = title;
        Entries = new ObservableCollection<AuditLog>(auditEntries);
        EntriesView = CollectionViewSource.GetDefaultView(Entries);
    }

    public AuditTrailViewModel(string title, List<VResultAuditTrail> resultEntries)
    {
        Title = title;
        ResultEntries = new ObservableCollection<VResultAuditTrail>(resultEntries);
        ResultEntriesView = CollectionViewSource.GetDefaultView(ResultEntries);
    }

    public string Title { get; }

    public ObservableCollection<AuditLog>? Entries { get; }

    public ICollectionView? EntriesView { get; }

    public ObservableCollection<VResultAuditTrail>? ResultEntries { get; }

    public ICollectionView? ResultEntriesView { get; }
}
