using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalLabSystem.Tests.Services;

public class ReportCommentTemplateServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static ReportCommentTemplateService CreateService(FinalLabDbContext context)
        => new ReportCommentTemplateService(context);

    private static async Task<ReportCommentTemplate> SeedTemplateAsync(
        FinalLabDbContext context,
        string triggerCondition = "Low",
        int? testtypeId = null,
        int? componentId = null,
        string commentText = "Test comment")
    {
        var template = new ReportCommentTemplate
        {
            Title = "Test Template",
            CommentText = commentText,
            CommentLang = "EN",
            TriggerCondition = triggerCondition,
            TesttypeId = testtypeId,
            ComponentId = componentId,
            IsActive = true,
            SortOrder = 1,
            CreatedAt = DateTime.UtcNow
        };
        context.ReportCommentTemplates.Add(template);
        await context.SaveChangesAsync();
        return template;
    }

    [Fact]
    public async Task CreateTemplateAsync_CreatesTemplate()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var template = new ReportCommentTemplate
        {
            Title = "New Template",
            CommentText = "New comment",
            CommentLang = "EN",
            TriggerCondition = "High",
            IsActive = true,
            SortOrder = 1
        };

        var created = await service.CreateTemplateAsync(template);

        Assert.True(created.TemplateId > 0);
        Assert.Equal("New Template", created.Title);
        Assert.True(created.IsActive);
    }

    [Fact]
    public async Task GetActiveTemplatesAsync_ReturnsOnlyActive()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await SeedTemplateAsync(context, triggerCondition: "Low");
        await SeedTemplateAsync(context, triggerCondition: "High");

        var inactive = new ReportCommentTemplate
        {
            Title = "Inactive",
            CommentText = "Inactive comment",
            CommentLang = "EN",
            TriggerCondition = "Critical",
            IsActive = false,
            SortOrder = 2,
            CreatedAt = DateTime.UtcNow
        };
        context.ReportCommentTemplates.Add(inactive);
        await context.SaveChangesAsync();

        var templates = await service.GetActiveTemplatesAsync();

        Assert.Equal(2, templates.Count);
        Assert.All(templates, t => Assert.True(t.IsActive));
    }

    [Fact]
    public async Task GetTemplatesByTriggerAsync_ReturnsMatchingTrigger()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await SeedTemplateAsync(context, triggerCondition: "Low");
        await SeedTemplateAsync(context, triggerCondition: "High");
        await SeedTemplateAsync(context, triggerCondition: "Low");

        var lowTemplates = await service.GetTemplatesByTriggerAsync(ReportCommentTrigger.Low);

        Assert.Equal(2, lowTemplates.Count);
        Assert.All(lowTemplates, t => Assert.Equal("Low", t.TriggerCondition));
    }

    [Fact]
    public async Task FindMatchingTemplateAsync_ComponentSpecific_Matches()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await SeedTemplateAsync(context, triggerCondition: "Low", componentId: 5);

        var match = await service.FindMatchingTemplateAsync(10, 5, ReportCommentTrigger.Low);

        Assert.NotNull(match);
        Assert.Equal(5, match.ComponentId);
    }

    [Fact]
    public async Task FindMatchingTemplateAsync_TestTypeLevel_MatchesWhenNoComponentMatch()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await SeedTemplateAsync(context, triggerCondition: "High", testtypeId: 10);

        var match = await service.FindMatchingTemplateAsync(10, null, ReportCommentTrigger.High);

        Assert.NotNull(match);
        Assert.Equal(10, match.TesttypeId);
    }

    [Fact]
    public async Task FindMatchingTemplateAsync_GlobalFallback_MatchesWhenNothingElse()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await SeedTemplateAsync(context, triggerCondition: "Critical");

        var match = await service.FindMatchingTemplateAsync(null, null, ReportCommentTrigger.Critical);

        Assert.NotNull(match);
        Assert.Null(match.TesttypeId);
        Assert.Null(match.ComponentId);
    }

    [Fact]
    public async Task FindMatchingTemplateAsync_NoMatch_ReturnsNull()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var match = await service.FindMatchingTemplateAsync(1, 1, ReportCommentTrigger.Low);

        Assert.Null(match);
    }

    [Fact]
    public async Task UpdateTemplateAsync_UpdatesFields()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var template = await SeedTemplateAsync(context);
        template.Title = "Updated Title";
        template.CommentText = "Updated comment";

        await service.UpdateTemplateAsync(template);

        var updated = await context.ReportCommentTemplates.FindAsync(template.TemplateId);
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated comment", updated.CommentText);
        Assert.NotNull(updated.ModifiedAt);
    }

    [Fact]
    public async Task UpdateTemplateAsync_NotFound_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var template = new ReportCommentTemplate { TemplateId = 999 };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateTemplateAsync(template));
    }

    [Fact]
    public async Task DeleteTemplateAsync_SoftDeletes()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        var template = await SeedTemplateAsync(context);

        await service.DeleteTemplateAsync(template.TemplateId);

        var deleted = await context.ReportCommentTemplates.FindAsync(template.TemplateId);
        Assert.NotNull(deleted);
        Assert.False(deleted.IsActive);
        Assert.NotNull(deleted.ModifiedAt);
    }

    [Fact]
    public async Task DeleteTemplateAsync_NotFound_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteTemplateAsync(999));
    }
}
