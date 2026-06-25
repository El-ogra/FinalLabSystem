using System;
using System.Threading.Tasks;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public sealed class ReportCommentEngine : IReportCommentEngine
{
    private readonly IReportCommentTemplateService _templateService;

    public ReportCommentEngine(IReportCommentTemplateService templateService)
    {
        _templateService = templateService;
    }

    public async Task ApplyAutoCommentAsync(TestResult result, int? testtypeId)
    {
        if (!string.IsNullOrWhiteSpace(result.Comment))
            return;

        if (string.IsNullOrWhiteSpace(result.ResultStatus))
            return;

        var trigger = MapStatusToTrigger(result.ResultStatus);
        if (trigger == null)
            return;

        var template = await _templateService.FindMatchingTemplateAsync(
            testtypeId, result.ComponentId, trigger.Value);

        if (template != null)
            result.Comment = template.CommentText;
    }

    private static ReportCommentTrigger? MapStatusToTrigger(string resultStatus)
    {
        return resultStatus.ToUpperInvariant() switch
        {
            "LOW" => ReportCommentTrigger.Low,
            "HIGH" => ReportCommentTrigger.High,
            "LOW_CRITICAL" => ReportCommentTrigger.Critical,
            "HIGH_CRITICAL" => ReportCommentTrigger.Critical,
            _ => null
        };
    }
}
