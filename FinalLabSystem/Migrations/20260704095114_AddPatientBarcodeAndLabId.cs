using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientBarcodeAndLabId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabId",
                table: "Patient",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "BranchNumber",
                table: "LabSettings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateTable(
                name: "PatientBarcode",
                columns: table => new
                {
                    PatientBarcodeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    VisitId = table.Column<int>(type: "int", nullable: true),
                    CodeType = table.Column<byte>(type: "tinyint", nullable: false),
                    BarcodeValue = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SortOrdinal = table.Column<int>(type: "int", nullable: false),
                    BranchNumber = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientBarcode", x => x.PatientBarcodeId);
                    table.ForeignKey(
                        name: "FK_PatientBarcode_Patient_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patient",
                        principalColumn: "patient_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientBarcode_Staff_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Staff",
                        principalColumn: "staff_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PatientBarcode_Visit_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visit",
                        principalColumn: "visit_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patient_LabId",
                table: "Patient",
                column: "LabId",
                unique: true,
                filter: "[LabId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PatientBarcode_BarcodeValue",
                table: "PatientBarcode",
                column: "BarcodeValue",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientBarcode_CreatedBy",
                table: "PatientBarcode",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PatientBarcode_PatientId_CodeType",
                table: "PatientBarcode",
                columns: new[] { "PatientId", "CodeType" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientBarcode_VisitId",
                table: "PatientBarcode",
                column: "VisitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientBarcode");

            migrationBuilder.DropIndex(
                name: "IX_Patient_LabId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "LabId",
                table: "Patient");

            migrationBuilder.DropColumn(
                name: "BranchNumber",
                table: "LabSettings");
        }
    }
}
