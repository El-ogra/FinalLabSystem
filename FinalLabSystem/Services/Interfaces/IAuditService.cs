using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAuditService
{
    Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId);
    Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId);
}
