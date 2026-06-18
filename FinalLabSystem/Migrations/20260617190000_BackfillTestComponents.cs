using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    public partial class BackfillTestComponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO TestComponent (testtype_id, component_code, component_name_en, result_type, decimal_places, sort_order, is_active)
                SELECT tt.testtype_id, tt.type_code, tt.type_name_en, 'NUMERIC', 0, 1, 1
                FROM TestType tt
                WHERE NOT EXISTS (SELECT 1 FROM TestComponent tc WHERE tc.testtype_id = tt.testtype_id)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE tc
                FROM TestComponent tc
                INNER JOIN TestType tt ON tc.testtype_id = tt.testtype_id
                WHERE tc.component_code = tt.type_code
                  AND tc.component_name_en = tt.type_name_en
                  AND tc.result_type = 'NUMERIC'
                  AND tc.sort_order = 1
                  AND tc.decimal_places = 0
                  AND NOT EXISTS (
                    SELECT 1 FROM NormalRange nr WHERE nr.component_id = tc.component_id
                  )
            ");
        }
    }
}
