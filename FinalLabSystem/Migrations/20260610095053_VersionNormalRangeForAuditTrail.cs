using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class VersionNormalRangeForAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NormalRangeId",
                table: "TestResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "NormalRange",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "superseded_by_id",
                table: "NormalRange",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "NormalRange",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_NormalRangeId",
                table: "TestResult",
                column: "NormalRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_NormalRange_superseded_by_id",
                table: "NormalRange",
                column: "superseded_by_id");

            migrationBuilder.AddForeignKey(
                name: "FK_NormalRange_NormalRange_superseded_by_id",
                table: "NormalRange",
                column: "superseded_by_id",
                principalTable: "NormalRange",
                principalColumn: "range_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestResult_NormalRange_NormalRangeId",
                table: "TestResult",
                column: "NormalRangeId",
                principalTable: "NormalRange",
                principalColumn: "range_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NormalRange_NormalRange_superseded_by_id",
                table: "NormalRange");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResult_NormalRange_NormalRangeId",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_TestResult_NormalRangeId",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_NormalRange_superseded_by_id",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "NormalRangeId",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "superseded_by_id",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "version",
                table: "NormalRange");
        }
    }
}
