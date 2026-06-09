using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CollectionTypeId",
                table: "TestType",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CollectionTypes",
                columns: table => new
                {
                    collection_type_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type_name_en = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    type_name_ar = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    sort_order = table.Column<short>(type: "smallint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionType", x => x.collection_type_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestType_CollectionTypeId",
                table: "TestType",
                column: "CollectionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestType_CollectionTypes_CollectionTypeId",
                table: "TestType",
                column: "CollectionTypeId",
                principalTable: "CollectionTypes",
                principalColumn: "collection_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestType_CollectionTypes_CollectionTypeId",
                table: "TestType");

            migrationBuilder.DropTable(
                name: "CollectionTypes");

            migrationBuilder.DropIndex(
                name: "IX_TestType_CollectionTypeId",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "CollectionTypeId",
                table: "TestType");
        }
    }
}
