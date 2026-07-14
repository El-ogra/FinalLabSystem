using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class VS09_AddSensitiveScreenPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SensitiveScreenPassword",
                columns: table => new
                {
                    sensitive_screen_password_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    screen_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_updated_by = table.Column<int>(type: "int", nullable: true),
                    last_updated_at = table.Column<DateTime>(type: "datetime2(0)", precision: 0, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SensitiveScreenPassword", x => x.sensitive_screen_password_id);
                    table.ForeignKey(
                        name: "FK_SensitiveScreenPassword_Staff",
                        column: x => x.last_updated_by,
                        principalTable: "Staff",
                        principalColumn: "staff_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SensitiveScreenPassword_last_updated_by",
                table: "SensitiveScreenPassword",
                column: "last_updated_by");

            migrationBuilder.CreateIndex(
                name: "UQ__SensitiveScreenPassword__ScreenType",
                table: "SensitiveScreenPassword",
                column: "screen_type",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensitiveScreenPassword");
        }
    }
}
