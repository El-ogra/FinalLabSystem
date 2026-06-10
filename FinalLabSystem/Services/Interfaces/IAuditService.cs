using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAuditService
{
    /// <summary>
    /// Gets audit entries for result modifications on a visit test.
    /// </summary>
    /// <param name="visitTestId">The visit-test identifier.</param>
    /// <returns>The result audit trail entries.</returns>
    Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId);

    /// <summary>
    /// Gets audit history for a table record.
    /// </summary>
    /// <param name="tableName">The audited table name.</param>
    /// <param name="recordId">The audited record identifier.</param>
    /// <returns>The table audit log entries.</returns>
    Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId);
}
