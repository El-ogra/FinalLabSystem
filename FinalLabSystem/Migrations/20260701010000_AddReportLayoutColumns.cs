using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReportLayoutColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Branding
            migrationBuilder.AddColumn<string>(
                name: "ReportLabNameAr",
                table: "LabSettings",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportLabNameEn",
                table: "LabSettings",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportLogoPath",
                table: "LabSettings",
                type: "nvarchar(500)",
                nullable: true);

            // Logo sizing
            migrationBuilder.AddColumn<decimal>(
                name: "ReportLogoWidth",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportLogoHeight",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: true);

            // Colors
            migrationBuilder.AddColumn<string>(
                name: "ReportPrimaryColor",
                table: "LabSettings",
                type: "nvarchar(7)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportSecondaryColor",
                table: "LabSettings",
                type: "nvarchar(7)",
                nullable: true);

            // Typography
            migrationBuilder.AddColumn<string>(
                name: "ReportFontFamily",
                table: "LabSettings",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ReportFontSize",
                table: "LabSettings",
                type: "float",
                nullable: false,
                defaultValue: 12.0);

            migrationBuilder.AddColumn<double>(
                name: "ReportHeaderFontSize",
                table: "LabSettings",
                type: "float",
                nullable: false,
                defaultValue: 16.0);

            migrationBuilder.AddColumn<double>(
                name: "ReportFooterFontSize",
                table: "LabSettings",
                type: "float",
                nullable: false,
                defaultValue: 10.0);

            // Page margins
            migrationBuilder.AddColumn<decimal>(
                name: "ReportMarginTop",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 2m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportMarginBottom",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 2m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportMarginLeft",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 2m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportMarginRight",
                table: "LabSettings",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 2m);

            // Section visibility
            migrationBuilder.AddColumn<bool>(
                name: "ReportShowHeader",
                table: "LabSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReportShowFooter",
                table: "LabSettings",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReportShowStamp",
                table: "LabSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Section text
            migrationBuilder.AddColumn<string>(
                name: "ReportHeaderText",
                table: "LabSettings",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportFooterText",
                table: "LabSettings",
                type: "nvarchar(500)",
                nullable: true);

            // Page setup
            migrationBuilder.AddColumn<string>(
                name: "ReportPageOrientation",
                table: "LabSettings",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportPaperSize",
                table: "LabSettings",
                type: "nvarchar(20)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ReportLabNameAr", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportLabNameEn", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportLogoPath", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportLogoWidth", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportLogoHeight", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportPrimaryColor", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportSecondaryColor", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportFontFamily", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportFontSize", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportHeaderFontSize", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportFooterFontSize", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportMarginTop", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportMarginBottom", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportMarginLeft", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportMarginRight", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportShowHeader", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportShowFooter", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportShowStamp", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportHeaderText", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportFooterText", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportPageOrientation", table: "LabSettings");
            migrationBuilder.DropColumn(name: "ReportPaperSize", table: "LabSettings");
        }
    }
}
