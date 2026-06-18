using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    public partial class AddTubeMaterialMaster : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TubeMaterial",
                columns: table => new
                {
                    tube_material_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    material_name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    material_name_ar = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    tube_color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TubeMaterial", x => x.tube_material_id);
                });

            // Seed default tube materials matching the original hardcoded list
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "Serum", 1, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "Plasma", 2, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "EDTA Blood", 3, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "Citrate Blood", 4, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "Urine", 5, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "CSF", 6, true });
            migrationBuilder.InsertData("TubeMaterial", new[] { "material_name", "sort_order", "is_active" }, new object[] { "Other", 7, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "TubeMaterial");
        }
    }
}
