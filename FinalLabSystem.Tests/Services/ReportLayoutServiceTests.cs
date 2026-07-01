using System;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public class ReportLayoutServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static ReportLayoutService CreateService(FinalLabDbContext ctx, ICurrentUserSession? session = null)
    {
        var logger = new Mock<ILogger<ReportLayoutService>>();
        var auditService = new Mock<IAuditService>();
        var userSession = session ?? new Mock<ICurrentUserSession>().Object;
        return new ReportLayoutService(ctx, userSession, auditService.Object, logger.Object);
    }

    [Fact]
    public async Task GetCurrentLayoutAsync_NoSettings_ReturnsDefaults()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetCurrentLayoutAsync_NoSettings_ReturnsDefaults)));
        var service = CreateService(ctx);

        var result = await service.GetCurrentLayoutAsync();

        Assert.Equal("#000000", result.PrimaryColor);
        Assert.Equal("Segoe UI", result.FontFamily);
        Assert.Equal(12, result.FontSize);
        Assert.True(result.ShowHeader);
        Assert.Equal("Portrait", result.PageOrientation);
        Assert.Equal("A4", result.PaperSize);
    }

    [Fact]
    public async Task SaveLayoutAsync_PersistsAllFields()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(SaveLayoutAsync_PersistsAllFields)));
        var service = CreateService(ctx);

        var dto = new ReportLayoutDto
        {
            LabNameAr = "مختبر الأمل",
            LabNameEn = "Al-Amal Lab",
            LogoPath = @"C:\logo.png",
            LogoWidth = 5.5m,
            LogoHeight = 3.2m,
            PrimaryColor = "#FF0000",
            SecondaryColor = "#00FF00",
            FontFamily = "Arial",
            FontSize = 14,
            HeaderFontSize = 18,
            FooterFontSize = 8,
            MarginTop = 3,
            MarginBottom = 1.5m,
            MarginLeft = 2.5m,
            MarginRight = 2.5m,
            ShowHeader = false,
            ShowFooter = false,
            ShowStamp = true,
            HeaderText = "تقرير خاص",
            FooterText = "صفحة @p",
            PageOrientation = "Landscape",
            PaperSize = "Letter"
        };

        await service.SaveLayoutAsync(dto, 1);

        var loaded = await service.GetCurrentLayoutAsync();
        Assert.Equal("مختبر الأمل", loaded.LabNameAr);
        Assert.Equal("Al-Amal Lab", loaded.LabNameEn);
        Assert.Equal(@"C:\logo.png", loaded.LogoPath);
        Assert.Equal(5.5m, loaded.LogoWidth);
        Assert.Equal(3.2m, loaded.LogoHeight);
        Assert.Equal("#FF0000", loaded.PrimaryColor);
        Assert.Equal("#00FF00", loaded.SecondaryColor);
        Assert.Equal("Arial", loaded.FontFamily);
        Assert.Equal(14, loaded.FontSize);
        Assert.Equal(18, loaded.HeaderFontSize);
        Assert.Equal(8, loaded.FooterFontSize);
        Assert.Equal(3, loaded.MarginTop);
        Assert.Equal(1.5m, loaded.MarginBottom);
        Assert.Equal(2.5m, loaded.MarginLeft);
        Assert.Equal(2.5m, loaded.MarginRight);
        Assert.False(loaded.ShowHeader);
        Assert.False(loaded.ShowFooter);
        Assert.True(loaded.ShowStamp);
        Assert.Equal("تقرير خاص", loaded.HeaderText);
        Assert.Equal("صفحة @p", loaded.FooterText);
        Assert.Equal("Landscape", loaded.PageOrientation);
        Assert.Equal("Letter", loaded.PaperSize);
    }

    [Fact]
    public async Task GetCurrentLayoutAsync_PartialSettings_MergesWithDefaults()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetCurrentLayoutAsync_PartialSettings_MergesWithDefaults)));
        var setting = new LabSetting
        {
            SettingKey = "default",
            ReportFontFamily = "Tahoma",
            ReportFontSize = 20,
            ReportPrimaryColor = "#0000FF"
        };
        ctx.LabSettings.Add(setting);
        await ctx.SaveChangesAsync();

        var service = CreateService(ctx);
        var result = await service.GetCurrentLayoutAsync();

        Assert.Equal("Tahoma", result.FontFamily);
        Assert.Equal(20, result.FontSize);
        Assert.Equal("#0000FF", result.PrimaryColor);
        // Defaults for untouched fields
        Assert.Equal(16, result.HeaderFontSize);
        Assert.True(result.ShowHeader);
        Assert.Equal("Portrait", result.PageOrientation);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ResetsAllReportFields()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(ResetToDefaultsAsync_ResetsAllReportFields)));
        var service = CreateService(ctx);

        var custom = new ReportLayoutDto
        {
            FontFamily = "Courier",
            FontSize = 8,
            ShowHeader = false
        };
        await service.SaveLayoutAsync(custom, 1);

        await service.ResetToDefaultsAsync();

        var result = await service.GetCurrentLayoutAsync();
        Assert.Equal("Segoe UI", result.FontFamily);
        Assert.Equal(12, result.FontSize);
        Assert.True(result.ShowHeader);
    }

    [Fact]
    public void GetDefaults_ReturnsValidDto()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(GetDefaults_ReturnsValidDto)));
        var service = CreateService(ctx);

        var defaults = service.GetDefaults();

        Assert.NotNull(defaults);
        Assert.Equal(12, defaults.FontSize);
        Assert.Equal(16, defaults.HeaderFontSize);
        Assert.Equal(10, defaults.FooterFontSize);
        Assert.Equal(2, defaults.MarginTop);
        Assert.Equal(2, defaults.MarginBottom);
        Assert.Equal(2, defaults.MarginLeft);
        Assert.Equal(2, defaults.MarginRight);
        Assert.True(defaults.ShowHeader);
        Assert.True(defaults.ShowFooter);
        Assert.False(defaults.ShowStamp);
        Assert.Equal("Portrait", defaults.PageOrientation);
        Assert.Equal("A4", defaults.PaperSize);
    }

    [Fact]
    public async Task SaveLayoutAsync_LogsAuditEvent()
    {
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(SaveLayoutAsync_LogsAuditEvent)));
        var logger = new Mock<ILogger<ReportLayoutService>>();
        var auditService = new Mock<IAuditService>();
        var userSession = new Mock<ICurrentUserSession>();
        var service = new ReportLayoutService(ctx, userSession.Object, auditService.Object, logger.Object);

        var dto = new ReportLayoutDto();
        await service.SaveLayoutAsync(dto, 5);

        auditService.Verify(a => a.LogActionAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.Is<int>(id => id == 5),
            It.IsAny<string?>()), Times.Once);
    }
}
