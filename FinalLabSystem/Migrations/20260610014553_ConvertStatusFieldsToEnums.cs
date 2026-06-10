using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStatusFieldsToEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "current_stage",
                table: "VisitTest",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldDefaultValue: "PENDING");

            migrationBuilder.AlterColumn<string>(
                name: "visit_status",
                table: "Visit",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Open",
                oldClrType: typeof(string),
                oldType: "nvarchar(15)",
                oldMaxLength: 15,
                oldDefaultValue: "OPEN");

            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "Visit",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldDefaultValue: "PENDING");

            migrationBuilder.AddColumn<string>(
                name: "BarcodeName",
                table: "TestType",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Cash",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "CASH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarcodeName",
                table: "TestType");

            migrationBuilder.AlterColumn<string>(
                name: "current_stage",
                table: "VisitTest",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "visit_status",
                table: "Visit",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "OPEN",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Open");

            migrationBuilder.AlterColumn<string>(
                name: "payment_status",
                table: "Visit",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "PENDING",
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "CASH",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Cash");
        }
    }
}
