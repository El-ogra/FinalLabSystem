using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemeIdToVisitAndReferralSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "scheme_id",
                table: "ReferralSource",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visit_scheme_id",
                table: "Visit",
                column: "scheme_id");

            migrationBuilder.CreateIndex(
                name: "IX_ReferralSource_scheme_id",
                table: "ReferralSource",
                column: "scheme_id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReferralSource_PriceScheme",
                table: "ReferralSource",
                column: "scheme_id",
                principalTable: "PriceScheme",
                principalColumn: "scheme_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReferralSource_PriceScheme",
                table: "ReferralSource");

            migrationBuilder.DropIndex(
                name: "IX_Visit_scheme_id",
                table: "Visit");

            migrationBuilder.DropIndex(
                name: "IX_ReferralSource_scheme_id",
                table: "ReferralSource");

            migrationBuilder.DropColumn(
                name: "scheme_id",
                table: "ReferralSource");
        }
    }
}
