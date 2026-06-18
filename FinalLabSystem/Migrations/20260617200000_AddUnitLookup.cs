using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    public partial class AddUnitLookup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    unit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    unit_name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    unit_name_ar = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    abbreviation = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Unit__3B4F0E4A12345678", x => x.unit_id);
                });

            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "sec", 1, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "mg/dL", 2, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "mmol/L", 3, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "g/dL", 4, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "U/L", 5, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "IU/L", 6, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "%", 7, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "fl", 8, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "pg", 9, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "cells/µL", 10, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "mEq/L", 11, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "mg/g", 12, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "µg/L", 13, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "ng/mL", 14, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "IU/mL", 15, true });
            migrationBuilder.InsertData("Unit", new[] { "unit_name", "sort_order", "is_active" }, new object[] { "copies/mL", 16, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Unit");
        }
    }
}
