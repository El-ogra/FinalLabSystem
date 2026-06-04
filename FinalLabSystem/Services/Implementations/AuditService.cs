using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class AuditService : IAuditService
{
    private readonly FinalLabDbContext _context;

    public AuditService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId)
    {
        var resultIds = await _context.TestResults
            .Where(tr => tr.VisitTestId == visitTestId)
            .Select(tr => tr.ResultId)
            .ToListAsync();

        return await _context.VResultAuditTrails
            .AsNoTracking()
            .Where(t => resultIds.Contains(t.ResultId))
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(al => al.TableName == tableName && al.RecordId == recordId)
            .OrderByDescending(al => al.ChangedAt)
            .ToListAsync();
    }
}
