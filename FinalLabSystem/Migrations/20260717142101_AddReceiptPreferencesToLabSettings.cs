using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReceiptPreferencesToLabSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoPrintReceiptAfterSave",
                table: "LabSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTestBreakdownInReceipt",
                table: "LabSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoPrintReceiptAfterSave",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "ShowTestBreakdownInReceipt",
                table: "LabSettings");
        }
    }
}
