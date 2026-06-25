using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameLabSettingFeatureToggleColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnforceStageGating",
                table: "LabSettings",
                newName: "enforce_stage_gating");

            migrationBuilder.RenameColumn(
                name: "EnableServerPrinting",
                table: "LabSettings",
                newName: "enable_server_printing");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "enforce_stage_gating",
                table: "LabSettings",
                newName: "EnforceStageGating");

            migrationBuilder.RenameColumn(
                name: "enable_server_printing",
                table: "LabSettings",
                newName: "EnableServerPrinting");
        }
    }
}
