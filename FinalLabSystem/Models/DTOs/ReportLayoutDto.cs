namespace FinalLabSystem.Models.DTOs;

public class ReportLayoutDto
{
    // Branding
    public string? LabNameAr { get; set; }
    public string? LabNameEn { get; set; }
    public string? LogoPath { get; set; }

    // Logo sizing (cm)
    public decimal? LogoWidth { get; set; }
    public decimal? LogoHeight { get; set; }

    // Colors (hex: "#RRGGBB")
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }

    // Typography
    public string? FontFamily { get; set; }
    public double FontSize { get; set; } = 12;
    public double HeaderFontSize { get; set; } = 16;
    public double FooterFontSize { get; set; } = 10;

    // Page margins (cm)
    public decimal MarginTop { get; set; } = 2;
    public decimal MarginBottom { get; set; } = 2;
    public decimal MarginLeft { get; set; } = 2;
    public decimal MarginRight { get; set; } = 2;

    // Section visibility
    public bool ShowHeader { get; set; } = true;
    public bool ShowFooter { get; set; } = true;
    public bool ShowStamp { get; set; } = false;

    // Section text
    public string? HeaderText { get; set; }
    public string? FooterText { get; set; }

    // Page setup
    public string PageOrientation { get; set; } = "Portrait";
    public string PaperSize { get; set; } = "A4";
}
