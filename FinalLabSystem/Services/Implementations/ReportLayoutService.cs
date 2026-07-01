using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class ReportLayoutService : IReportLayoutService
{
    private readonly FinalLabDbContext _context;
    private readonly ICurrentUserSession _currentUserSession;
    private readonly IAuditService _auditService;
    private readonly ILogger<ReportLayoutService> _logger;

    public ReportLayoutService(
        FinalLabDbContext context,
        ICurrentUserSession currentUserSession,
        IAuditService auditService,
        ILogger<ReportLayoutService> logger)
    {
        _context = context;
        _currentUserSession = currentUserSession;
        _auditService = auditService;
        _logger = logger;
    }

    public async Task<ReportLayoutDto> GetCurrentLayoutAsync()
    {
        var setting = await _context.LabSettings.FirstOrDefaultAsync();
        if (setting is null)
            return GetDefaults();

        return new ReportLayoutDto
        {
            LabNameAr = setting.ReportLabNameAr ?? null,
            LabNameEn = setting.ReportLabNameEn ?? null,
            LogoPath = setting.ReportLogoPath ?? null,
            LogoWidth = setting.ReportLogoWidth,
            LogoHeight = setting.ReportLogoHeight,
            PrimaryColor = setting.ReportPrimaryColor ?? null,
            SecondaryColor = setting.ReportSecondaryColor ?? null,
            FontFamily = setting.ReportFontFamily ?? null,
            FontSize = setting.ReportFontSize,
            HeaderFontSize = setting.ReportHeaderFontSize,
            FooterFontSize = setting.ReportFooterFontSize,
            MarginTop = setting.ReportMarginTop,
            MarginBottom = setting.ReportMarginBottom,
            MarginLeft = setting.ReportMarginLeft,
            MarginRight = setting.ReportMarginRight,
            ShowHeader = setting.ReportShowHeader,
            ShowFooter = setting.ReportShowFooter,
            ShowStamp = setting.ReportShowStamp,
            HeaderText = setting.ReportHeaderText ?? null,
            FooterText = setting.ReportFooterText ?? null,
            PageOrientation = setting.ReportPageOrientation ?? "Portrait",
            PaperSize = setting.ReportPaperSize ?? "A4"
        };
    }

    public async Task SaveLayoutAsync(ReportLayoutDto layout, int staffId)
    {
        var setting = await _context.LabSettings.FirstOrDefaultAsync();
        if (setting is null)
        {
            setting = new LabSetting { SettingKey = "report_layout_settings" };
            _context.LabSettings.Add(setting);
        }

        setting.ReportLabNameAr = layout.LabNameAr;
        setting.ReportLabNameEn = layout.LabNameEn;
        setting.ReportLogoPath = layout.LogoPath;
        setting.ReportLogoWidth = layout.LogoWidth;
        setting.ReportLogoHeight = layout.LogoHeight;
        setting.ReportPrimaryColor = layout.PrimaryColor;
        setting.ReportSecondaryColor = layout.SecondaryColor;
        setting.ReportFontFamily = layout.FontFamily;
        setting.ReportFontSize = layout.FontSize;
        setting.ReportHeaderFontSize = layout.HeaderFontSize;
        setting.ReportFooterFontSize = layout.FooterFontSize;
        setting.ReportMarginTop = layout.MarginTop;
        setting.ReportMarginBottom = layout.MarginBottom;
        setting.ReportMarginLeft = layout.MarginLeft;
        setting.ReportMarginRight = layout.MarginRight;
        setting.ReportShowHeader = layout.ShowHeader;
        setting.ReportShowFooter = layout.ShowFooter;
        setting.ReportShowStamp = layout.ShowStamp;
        setting.ReportHeaderText = layout.HeaderText;
        setting.ReportFooterText = layout.FooterText;
        setting.ReportPageOrientation = layout.PageOrientation;
        setting.ReportPaperSize = layout.PaperSize;

        await _context.SaveChangesAsync();
        await _auditService.LogActionAsync("LabSetting", 0, "ReportSettingsUpdated", staffId, "Report layout settings updated");
        _logger.LogInformation("Report layout settings saved by staff {StaffId}", staffId);
    }

    public async Task ResetToDefaultsAsync()
    {
        var defaults = GetDefaults();
        var staffId = _currentUserSession.CurrentUser?.StaffId ?? 0;
        await SaveLayoutAsync(defaults, staffId);
    }

    public ReportLayoutDto GetDefaults()
    {
        return new ReportLayoutDto
        {
            LabNameAr = null,
            LabNameEn = null,
            LogoPath = null,
            LogoWidth = null,
            LogoHeight = null,
            PrimaryColor = "#000000",
            SecondaryColor = "#FFFFFF",
            FontFamily = "Segoe UI",
            FontSize = 12,
            HeaderFontSize = 16,
            FooterFontSize = 10,
            MarginTop = 2,
            MarginBottom = 2,
            MarginLeft = 2,
            MarginRight = 2,
            ShowHeader = true,
            ShowFooter = true,
            ShowStamp = false,
            HeaderText = null,
            FooterText = null,
            PageOrientation = "Portrait",
            PaperSize = "A4"
        };
    }
}
