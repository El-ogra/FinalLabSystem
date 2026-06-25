using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public sealed class ReportCommentTemplateService : IReportCommentTemplateService
{
    private readonly FinalLabDbContext _context;

    public ReportCommentTemplateService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReportCommentTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.ReportCommentTemplates
            .Where(t => t.IsActive)
            .Include(t => t.Category)
            .Include(t => t.Testtype)
            .Include(t => t.Component)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<List<ReportCommentTemplate>> GetTemplatesByTriggerAsync(ReportCommentTrigger trigger)
    {
        var triggerName = trigger.ToString();
        return await _context.ReportCommentTemplates
            .Where(t => t.IsActive && t.TriggerCondition == triggerName)
            .Include(t => t.Category)
            .Include(t => t.Testtype)
            .Include(t => t.Component)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }

    public async Task<ReportCommentTemplate?> FindMatchingTemplateAsync(
        int? testtypeId, int? componentId, ReportCommentTrigger trigger)
    {
        var triggerName = trigger.ToString();

        var query = _context.ReportCommentTemplates
            .Where(t => t.IsActive && t.TriggerCondition == triggerName);

        if (componentId.HasValue)
        {
            var componentMatch = await query
                .Where(t => t.ComponentId == componentId.Value)
                .OrderByDescending(t => t.ComponentId != null)
                .ThenBy(t => t.SortOrder)
                .FirstOrDefaultAsync();

            if (componentMatch != null)
                return componentMatch;
        }

        if (testtypeId.HasValue)
        {
            var testTypeMatch = await query
                .Where(t => t.TesttypeId == testtypeId.Value && t.ComponentId == null)
                .OrderBy(t => t.SortOrder)
                .FirstOrDefaultAsync();

            if (testTypeMatch != null)
                return testTypeMatch;
        }

        return await query
            .Where(t => t.TesttypeId == null && t.ComponentId == null)
            .OrderBy(t => t.SortOrder)
            .FirstOrDefaultAsync();
    }

    public async Task<ReportCommentTemplate> CreateTemplateAsync(ReportCommentTemplate template)
    {
        template.CreatedAt = DateTime.UtcNow;
        template.IsActive = true;

        _context.ReportCommentTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task UpdateTemplateAsync(ReportCommentTemplate template)
    {
        var existing = await _context.ReportCommentTemplates.FindAsync(template.TemplateId);
        if (existing == null)
            throw new InvalidOperationException("Template not found.");

        existing.Title = template.Title;
        existing.CommentText = template.CommentText;
        existing.CommentLang = template.CommentLang;
        existing.TriggerCondition = template.TriggerCondition;
        existing.CategoryId = template.CategoryId;
        existing.TesttypeId = template.TesttypeId;
        existing.ComponentId = template.ComponentId;
        existing.SortOrder = template.SortOrder;
        existing.IsActive = template.IsActive;
        existing.ModifiedAt = DateTime.UtcNow;
        existing.ModifiedBy = template.ModifiedBy;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteTemplateAsync(int templateId)
    {
        var existing = await _context.ReportCommentTemplates.FindAsync(templateId);
        if (existing == null)
            throw new InvalidOperationException("Template not found.");

        existing.IsActive = false;
        existing.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
