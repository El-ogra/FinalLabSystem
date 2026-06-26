using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyContractFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "billing_periodicity",
                table: "Company",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "contract_end_date",
                table: "Company",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "contract_start_date",
                table: "Company",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "billing_periodicity",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "contract_end_date",
                table: "Company");

            migrationBuilder.DropColumn(
                name: "contract_start_date",
                table: "Company");
        }
    }
}
