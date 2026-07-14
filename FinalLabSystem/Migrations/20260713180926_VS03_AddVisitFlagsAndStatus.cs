using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class VS03_AddVisitFlagsAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Visit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEntered",
                table: "Visit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullyPaid",
                table: "Visit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                table: "Visit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReviewed",
                table: "Visit",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "IsEntered",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "IsFullyPaid",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "IsPrinted",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "IsReviewed",
                table: "Visit");
        }
    }
}
