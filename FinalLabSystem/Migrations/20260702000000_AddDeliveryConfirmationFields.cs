using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryConfirmationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "delivery_confirmed_at",
                table: "Visit",
                type: "datetime2(0)",
                precision: 0,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "delivery_signature",
                table: "Visit",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "delivery_otp_code",
                table: "Visit",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeliveryConfirmation",
                columns: table => new
                {
                    delivery_confirmation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    visit_id = table.Column<int>(type: "int", nullable: false),
                    method = table.Column<int>(type: "int", nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: false),
                    signature_image = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    otp_code_hash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    received_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryConfirmation", x => x.delivery_confirmation_id);
                    table.ForeignKey(
                        name: "FK_DeliveryConfirmation_Visit",
                        column: x => x.visit_id,
                        principalTable: "Visit",
                        principalColumn: "visit_id");
                    table.ForeignKey(
                        name: "FK_DeliveryConfirmation_Staff",
                        column: x => x.staff_id,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryConfirmation_VisitId",
                table: "DeliveryConfirmation",
                column: "visit_id");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryConfirmation_StaffId",
                table: "DeliveryConfirmation",
                column: "staff_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryConfirmation");

            migrationBuilder.DropColumn(
                name: "delivery_confirmed_at",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "delivery_signature",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "delivery_otp_code",
                table: "Visit");
        }
    }
}
