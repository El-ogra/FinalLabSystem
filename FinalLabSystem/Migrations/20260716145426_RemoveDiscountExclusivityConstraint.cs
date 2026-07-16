using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDiscountExclusivityConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Visit_DiscountExclusivity",
                table: "Visit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Visit_DiscountExclusivity",
                table: "Visit",
                sql: "NOT (discount_amount > 0 AND discount_percent > 0)");
        }
    }
}
