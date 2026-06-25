using System.Collections.Generic;
using FinalLabSystem.Models;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Interfaces;

public interface IAuditTrailDialogService
{
    void ShowGeneralAudit(string title, List<AuditLog> entries);
    void ShowResultAudit(string title, List<VResultAuditTrail> entries);
}
