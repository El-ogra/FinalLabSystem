using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class ReportCommentEngineTests
{
    [Fact]
    public async Task ApplyAutoComment_LowStatus_InjectsLowTemplate()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var template = new ReportCommentTemplate
        {
            TemplateId = 1,
            CommentText = "Low result comment",
            TriggerCondition = "Low"
        };
        mockTemplateService
            .Setup(s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), ReportCommentTrigger.Low))
            .ReturnsAsync(template);

        var result = new TestResult
        {
            ResultStatus = "LOW",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Equal("Low result comment", result.Comment);
    }

    [Fact]
    public async Task ApplyAutoComment_HighStatus_InjectsHighTemplate()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var template = new ReportCommentTemplate
        {
            TemplateId = 2,
            CommentText = "High result comment",
            TriggerCondition = "High"
        };
        mockTemplateService
            .Setup(s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), ReportCommentTrigger.High))
            .ReturnsAsync(template);

        var result = new TestResult
        {
            ResultStatus = "HIGH",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Equal("High result comment", result.Comment);
    }

    [Fact]
    public async Task ApplyAutoComment_HighCriticalStatus_InjectsCriticalTemplate()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var template = new ReportCommentTemplate
        {
            TemplateId = 3,
            CommentText = "Critical high comment",
            TriggerCondition = "Critical"
        };
        mockTemplateService
            .Setup(s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), ReportCommentTrigger.Critical))
            .ReturnsAsync(template);

        var result = new TestResult
        {
            ResultStatus = "HIGH_CRITICAL",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Equal("Critical high comment", result.Comment);
    }

    [Fact]
    public async Task ApplyAutoComment_LowCriticalStatus_InjectsCriticalTemplate()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var template = new ReportCommentTemplate
        {
            TemplateId = 4,
            CommentText = "Critical low comment",
            TriggerCondition = "Critical"
        };
        mockTemplateService
            .Setup(s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), ReportCommentTrigger.Critical))
            .ReturnsAsync(template);

        var result = new TestResult
        {
            ResultStatus = "LOW_CRITICAL",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Equal("Critical low comment", result.Comment);
    }

    [Fact]
    public async Task ApplyAutoComment_NormalStatus_DoesNotInjectComment()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var result = new TestResult
        {
            ResultStatus = "NORMAL",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Null(result.Comment);
        mockTemplateService.Verify(
            s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ReportCommentTrigger>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyAutoComment_ExistingComment_DoesNotOverwrite()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var result = new TestResult
        {
            ResultStatus = "LOW",
            ComponentId = 1,
            Comment = "Manual comment"
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Equal("Manual comment", result.Comment);
        mockTemplateService.Verify(
            s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ReportCommentTrigger>()),
            Times.Never);
    }

    [Fact]
    public async Task ApplyAutoComment_NoMatchingTemplate_DoesNotSetComment()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        mockTemplateService
            .Setup(s => s.FindMatchingTemplateAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<ReportCommentTrigger>()))
            .ReturnsAsync((ReportCommentTemplate?)null);

        var result = new TestResult
        {
            ResultStatus = "HIGH",
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Null(result.Comment);
    }

    [Fact]
    public async Task ApplyAutoComment_NullStatus_DoesNotInject()
    {
        var mockTemplateService = new Mock<IReportCommentTemplateService>();
        var engine = new ReportCommentEngine(mockTemplateService.Object);

        var result = new TestResult
        {
            ResultStatus = null,
            ComponentId = 1
        };

        await engine.ApplyAutoCommentAsync(result, 10);

        Assert.Null(result.Comment);
    }
}
