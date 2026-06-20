using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPrintExportTrackingToVisitTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "exported_at",
                table: "VisitTest",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "exported_by",
                table: "VisitTest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_exported",
                table: "VisitTest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_printed",
                table: "VisitTest",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "printed_at",
                table: "VisitTest",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "printed_by",
                table: "VisitTest",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitTest_exported_by",
                table: "VisitTest",
                column: "exported_by");

            migrationBuilder.CreateIndex(
                name: "IX_VisitTest_printed_by",
                table: "VisitTest",
                column: "printed_by");

            migrationBuilder.AddForeignKey(
                name: "FK_VT_ExportedBy",
                table: "VisitTest",
                column: "exported_by",
                principalTable: "Staff",
                principalColumn: "staff_id");

            migrationBuilder.AddForeignKey(
                name: "FK_VT_PrintedBy",
                table: "VisitTest",
                column: "printed_by",
                principalTable: "Staff",
                principalColumn: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VT_ExportedBy",
                table: "VisitTest");

            migrationBuilder.DropForeignKey(
                name: "FK_VT_PrintedBy",
                table: "VisitTest");

            migrationBuilder.DropIndex(
                name: "IX_VisitTest_exported_by",
                table: "VisitTest");

            migrationBuilder.DropIndex(
                name: "IX_VisitTest_printed_by",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "exported_at",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "exported_by",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "is_exported",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "is_printed",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "printed_at",
                table: "VisitTest");

            migrationBuilder.DropColumn(
                name: "printed_by",
                table: "VisitTest");
        }
    }
}
