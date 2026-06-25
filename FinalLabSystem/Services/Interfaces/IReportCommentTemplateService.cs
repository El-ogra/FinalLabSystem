using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportCommentTemplateService
{
    Task<List<ReportCommentTemplate>> GetActiveTemplatesAsync();

    Task<List<ReportCommentTemplate>> GetTemplatesByTriggerAsync(ReportCommentTrigger trigger);

    Task<ReportCommentTemplate?> FindMatchingTemplateAsync(int? testtypeId, int? componentId, ReportCommentTrigger trigger);

    Task<ReportCommentTemplate> CreateTemplateAsync(ReportCommentTemplate template);

    Task UpdateTemplateAsync(ReportCommentTemplate template);

    Task DeleteTemplateAsync(int templateId);
}
