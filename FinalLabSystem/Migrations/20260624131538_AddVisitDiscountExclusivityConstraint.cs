using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitDiscountExclusivityConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Patient",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Visit_DiscountExclusivity",
                table: "Visit",
                sql: "NOT (discount_amount > 0 AND discount_percent > 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Visit_DiscountExclusivity",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Patient");
        }
    }
}
