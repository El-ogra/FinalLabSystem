using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTestDataManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "add_with_group",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "bill_name_line1",
                table: "TestType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bill_name_line2",
                table: "TestType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "collection_notes",
                table: "TestType",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "history_name",
                table: "TestType",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_main_test",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_routine_test",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_send_outside",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "outside_cost_price",
                table: "TestType",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "outside_lab_name",
                table: "TestType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_question",
                table: "TestType",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "print_with_other",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "report_name_line1",
                table: "TestType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "report_name_line2",
                table: "TestType",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "see_report",
                table: "TestType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "TestTypeSampleTube",
                columns: table => new
                {
                    testtype_tube_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    testtype_id = table.Column<int>(type: "int", nullable: false),
                    tube_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tube_color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    sample_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    sort_order = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestTypeSampleTube", x => x.testtype_tube_id);
                    table.ForeignKey(
                        name: "FK_TestTypeSampleTube_TestType",
                        column: x => x.testtype_id,
                        principalTable: "TestType",
                        principalColumn: "testtype_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestType_BillNameLine1",
                table: "TestType",
                column: "bill_name_line1");

            migrationBuilder.CreateIndex(
                name: "IX_TestType_HistoryName",
                table: "TestType",
                column: "history_name");

            migrationBuilder.CreateIndex(
                name: "IX_TestTypeSampleTube_TestType_Sort",
                table: "TestTypeSampleTube",
                columns: new[] { "testtype_id", "sort_order" });

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM PriceScheme WHERE scheme_name = N'Patient Price')
    INSERT INTO PriceScheme (scheme_name, description, is_default, is_active)
    VALUES (N'Patient Price', N'Walk-in individual patient pricing', 1, 1);

IF NOT EXISTS (SELECT 1 FROM PriceScheme WHERE scheme_name = N'Lab-to-Lab Price')
    INSERT INTO PriceScheme (scheme_name, description, is_default, is_active)
    VALUES (N'Lab-to-Lab Price', N'Referred-lab pricing', 0, 1);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestTypeSampleTube");

            migrationBuilder.DropIndex(
                name: "IX_TestType_BillNameLine1",
                table: "TestType");

            migrationBuilder.DropIndex(
                name: "IX_TestType_HistoryName",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "add_with_group",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "bill_name_line1",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "bill_name_line2",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "collection_notes",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "history_name",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "is_main_test",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "is_routine_test",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "is_send_outside",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "outside_cost_price",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "outside_lab_name",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "patient_question",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "print_with_other",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "report_name_line1",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "report_name_line2",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "see_report",
                table: "TestType");
        }
    }
}
