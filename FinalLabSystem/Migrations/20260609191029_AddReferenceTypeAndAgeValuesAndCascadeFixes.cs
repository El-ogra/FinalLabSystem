using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddReferenceTypeAndAgeValuesAndCascadeFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NormalRange_Component",
                table: "NormalRange");

            migrationBuilder.DropForeignKey(
                name: "FK_Component_Type",
                table: "TestComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Component",
                table: "TestResult");

            migrationBuilder.DropForeignKey(
                name: "FK_TestType_Group",
                table: "TestType");

            migrationBuilder.DropForeignKey(
                name: "FK_TestTypePrice_Type",
                table: "TestTypePrice");

            migrationBuilder.RenameColumn(
                name: "applies_to_pregnant",
                table: "NormalRange",
                newName: "for_pregnant_only");

            migrationBuilder.AddColumn<string>(
                name: "reference_type",
                table: "TestType",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "age_from_value",
                table: "NormalRange",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "age_to_value",
                table: "NormalRange",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_NormalRange_Component",
                table: "NormalRange",
                column: "component_id",
                principalTable: "TestComponent",
                principalColumn: "component_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Component_Type",
                table: "TestComponent",
                column: "testtype_id",
                principalTable: "TestType",
                principalColumn: "testtype_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Component",
                table: "TestResult",
                column: "component_id",
                principalTable: "TestComponent",
                principalColumn: "component_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestType_Group",
                table: "TestType",
                column: "group_id",
                principalTable: "TestGroup",
                principalColumn: "group_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TestTypePrice_Type",
                table: "TestTypePrice",
                column: "testtype_id",
                principalTable: "TestType",
                principalColumn: "testtype_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NormalRange_Component",
                table: "NormalRange");

            migrationBuilder.DropForeignKey(
                name: "FK_Component_Type",
                table: "TestComponent");

            migrationBuilder.DropForeignKey(
                name: "FK_Result_Component",
                table: "TestResult");

            migrationBuilder.DropForeignKey(
                name: "FK_TestType_Group",
                table: "TestType");

            migrationBuilder.DropForeignKey(
                name: "FK_TestTypePrice_Type",
                table: "TestTypePrice");

            migrationBuilder.DropColumn(
                name: "reference_type",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "age_from_value",
                table: "NormalRange");

            migrationBuilder.DropColumn(
                name: "age_to_value",
                table: "NormalRange");

            migrationBuilder.RenameColumn(
                name: "for_pregnant_only",
                table: "NormalRange",
                newName: "applies_to_pregnant");

            migrationBuilder.AddForeignKey(
                name: "FK_NormalRange_Component",
                table: "NormalRange",
                column: "component_id",
                principalTable: "TestComponent",
                principalColumn: "component_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Component_Type",
                table: "TestComponent",
                column: "testtype_id",
                principalTable: "TestType",
                principalColumn: "testtype_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Result_Component",
                table: "TestResult",
                column: "component_id",
                principalTable: "TestComponent",
                principalColumn: "component_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestType_Group",
                table: "TestType",
                column: "group_id",
                principalTable: "TestGroup",
                principalColumn: "group_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestTypePrice_Type",
                table: "TestTypePrice",
                column: "testtype_id",
                principalTable: "TestType",
                principalColumn: "testtype_id");
        }
    }
}
