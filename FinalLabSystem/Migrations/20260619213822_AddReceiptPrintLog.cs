using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptPrintLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptPrintLog",
                columns: table => new
                {
                    log_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    visit_id = table.Column<int>(type: "int", nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    printed_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false, defaultValueSql: "(sysdatetime())"),
                    format = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    show_breakdown = table.Column<bool>(type: "bit", nullable: false),
                    subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_after_discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_paid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    balance_due = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptPrintLog", x => x.log_id);
                    table.ForeignKey(
                        name: "FK_ReceiptPrintLog_Staff",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                    table.ForeignKey(
                        name: "FK_ReceiptPrintLog_Visit",
                        column: x => x.visit_id,
                        principalTable: "Visit",
                        principalColumn: "visit_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrintLog_PrintedAt",
                table: "ReceiptPrintLog",
                column: "printed_at");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrintLog_staff_id",
                table: "ReceiptPrintLog",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptPrintLog_VisitId",
                table: "ReceiptPrintLog",
                column: "visit_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptPrintLog");
        }
    }
}
