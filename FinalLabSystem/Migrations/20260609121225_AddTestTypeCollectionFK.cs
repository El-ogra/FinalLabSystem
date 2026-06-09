using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddTestTypeCollectionFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestType_CollectionTypes_CollectionTypeId",
                table: "TestType");

            migrationBuilder.RenameColumn(
                name: "CollectionTypeId",
                table: "TestType",
                newName: "collection_type_id");

            migrationBuilder.RenameIndex(
                name: "IX_TestType_CollectionTypeId",
                table: "TestType",
                newName: "IX_TestType_collection_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestType_CollectionType",
                table: "TestType",
                column: "collection_type_id",
                principalTable: "CollectionTypes",
                principalColumn: "collection_type_id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TestType_CollectionType",
                table: "TestType");

            migrationBuilder.RenameColumn(
                name: "collection_type_id",
                table: "TestType",
                newName: "CollectionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_TestType_collection_type_id",
                table: "TestType",
                newName: "IX_TestType_CollectionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TestType_CollectionTypes_CollectionTypeId",
                table: "TestType",
                column: "CollectionTypeId",
                principalTable: "CollectionTypes",
                principalColumn: "collection_type_id");
        }
    }
}
