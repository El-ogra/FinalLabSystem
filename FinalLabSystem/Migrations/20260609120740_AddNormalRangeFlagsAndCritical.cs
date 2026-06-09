using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddNormalRangeFlagsAndCritical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "critical_comment",
                table: "NormalRange",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "critical_flag",
                table: "NormalRange",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "critical_range_text",
                table: "NormalRange",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "high_comment",
                table: "NormalRange",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "high_flag",
                table: "NormalRange",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "low_comment",
                table: "NormalRange",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "low_flag",
                table: "NormalRange",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "critical_comment",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "critical_flag",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "critical_range_text",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "high_comment",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "high_flag",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "low_comment",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "low_flag",
                table: "NormalRange");
        }
    }
}
