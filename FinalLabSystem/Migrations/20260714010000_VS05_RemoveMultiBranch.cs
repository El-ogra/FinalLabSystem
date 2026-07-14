using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <summary>
    /// VS-05 — إزالة Multi-Branch بالكامل (القرار 23).
    /// يُسقط العمود BranchNumber من جدولَي LabSettings و PatientBarcode.
    /// </summary>
    /// <inheritdoc />
    public partial class VS05_RemoveMultiBranch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchNumber",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "BranchNumber",
                table: "PatientBarcode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "BranchNumber",
                table: "LabSettings",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.AddColumn<byte>(
                name: "BranchNumber",
                table: "PatientBarcode",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)1);
        }
    }
}
