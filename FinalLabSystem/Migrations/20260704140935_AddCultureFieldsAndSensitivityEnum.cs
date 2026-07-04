using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCultureFieldsAndSensitivityEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "colony_count",
                table: "MicrobiologyCulture",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "culture_condition",
                table: "MicrobiologyCulture",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            // Drop CHECK constraint and default constraint on sensitivity before altering
            migrationBuilder.Sql(@"
                DECLARE @ckName sysname;
                SELECT @ckName = [ck].[name]
                FROM [sys].[check_constraints] [ck]
                INNER JOIN [sys].[columns] [col]
                    ON [ck].[parent_object_id] = [col].[object_id]
                   AND [ck].[parent_column_id] = [col].[column_id]
                WHERE [ck].[parent_object_id] = OBJECT_ID(N'[OrganismAntibiotic]')
                  AND [col].[name] = N'sensitivity';
                IF @ckName IS NOT NULL
                    EXEC(N'ALTER TABLE [OrganismAntibiotic] DROP CONSTRAINT [' + @ckName + N']');

                DECLARE @dfName sysname;
                SELECT @dfName = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c]
                    ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE [d].[parent_object_id] = OBJECT_ID(N'[OrganismAntibiotic]')
                  AND [c].[name] = N'sensitivity';
                IF @dfName IS NOT NULL
                    EXEC(N'ALTER TABLE [OrganismAntibiotic] DROP CONSTRAINT [' + @dfName + N']');
            ");

            // Step 1: Add temporary column
            migrationBuilder.AddColumn<byte>(
                name: "Sensitivity_New",
                table: "OrganismAntibiotic",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)3);

            // Step 2: Copy data from old column to new
            migrationBuilder.Sql(@"
                UPDATE OrganismAntibiotic
                SET Sensitivity_New = CASE Sensitivity
                    WHEN 'S' THEN 0
                    WHEN 'I' THEN 1
                    WHEN 'R' THEN 3
                    ELSE 3
                END
            ");

            // Step 3: Drop old column
            migrationBuilder.DropColumn(
                name: "Sensitivity",
                table: "OrganismAntibiotic");

            // Step 4: Rename new column
            migrationBuilder.RenameColumn(
                name: "Sensitivity_New",
                table: "OrganismAntibiotic",
                newName: "Sensitivity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "colony_count",
                table: "MicrobiologyCulture");

            migrationBuilder.DropColumn(
                name: "culture_condition",
                table: "MicrobiologyCulture");

            migrationBuilder.RenameColumn(
                name: "Sensitivity",
                table: "OrganismAntibiotic",
                newName: "Sensitivity_New");

            migrationBuilder.AddColumn<string>(
                name: "Sensitivity",
                table: "OrganismAntibiotic",
                type: "nchar(1)",
                fixedLength: true,
                maxLength: 1,
                nullable: false,
                defaultValue: "R");

            migrationBuilder.Sql(@"
                UPDATE OrganismAntibiotic
                SET Sensitivity = CASE Sensitivity_New
                    WHEN 0 THEN 'S'
                    WHEN 1 THEN 'I'
                    WHEN 3 THEN 'R'
                    ELSE 'R'
                END
            ");

            migrationBuilder.DropColumn(
                name: "Sensitivity_New",
                table: "OrganismAntibiotic");
        }
    }
}
