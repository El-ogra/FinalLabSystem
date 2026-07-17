using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddLabelPrintOffsetToLabSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LabelPrintOffsetXmm",
                table: "LabSettings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "LabelPrintOffsetYmm",
                table: "LabSettings",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LabelPrintOffsetXmm",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "LabelPrintOffsetYmm",
                table: "LabSettings");
        }
    }
}
