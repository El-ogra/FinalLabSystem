using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultValidationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "validated_at",
                table: "TestResult",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "validated_by_staff_id",
                table: "TestResult",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "validation_status",
                table: "TestResult",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_validated_by_staff_id",
                table: "TestResult",
                column: "validated_by_staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_ValidatedBy",
                table: "TestResult",
                column: "validated_by_staff_id",
                principalTable: "Staff",
                principalColumn: "staff_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Result_ValidatedBy",
                table: "TestResult");

            migrationBuilder.DropIndex(
                name: "IX_TestResult_validated_by_staff_id",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "validated_at",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "validated_by_staff_id",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "validation_status",
                table: "TestResult");
        }
    }
}
