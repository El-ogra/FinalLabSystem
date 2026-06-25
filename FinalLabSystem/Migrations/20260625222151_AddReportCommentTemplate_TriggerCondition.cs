using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReportCommentTemplate_TriggerCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "trigger_condition",
                table: "ReportCommentTemplate",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                defaultValue: "Manual");

            migrationBuilder.Sql("UPDATE ReportCommentTemplate SET trigger_condition = 'Manual' WHERE trigger_condition IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "trigger_condition",
                table: "ReportCommentTemplate");
        }
    }
}
