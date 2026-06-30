using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBackupAndSmtpFieldsToLabSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "smtp_host",
                table: "LabSettings",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "smtp_port",
                table: "LabSettings",
                type: "int",
                nullable: true,
                defaultValue: 587);

            migrationBuilder.AddColumn<string>(
                name: "smtp_username",
                table: "LabSettings",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "smtp_password_encrypted",
                table: "LabSettings",
                type: "nvarchar(500)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "smtp_enable_ssl",
                table: "LabSettings",
                type: "bit",
                nullable: true,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "backup_schedule_hour",
                table: "LabSettings",
                type: "int",
                nullable: true,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "backup_retention_days",
                table: "LabSettings",
                type: "int",
                nullable: true,
                defaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "backup_output_folder",
                table: "LabSettings",
                type: "nvarchar(500)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "backup_output_folder",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "backup_retention_days",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "backup_schedule_hour",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "smtp_enable_ssl",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "smtp_password_encrypted",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "smtp_username",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "smtp_port",
                table: "LabSettings");

            migrationBuilder.DropColumn(
                name: "smtp_host",
                table: "LabSettings");
        }
    }
}
