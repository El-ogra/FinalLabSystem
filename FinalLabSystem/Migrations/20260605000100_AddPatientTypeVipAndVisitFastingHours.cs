using FinalLabSystem.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations;

[DbContext(typeof(FinalLabDbContext))]
[Migration("20260605000100_AddPatientTypeVipAndVisitFastingHours")]
public partial class AddPatientTypeVipAndVisitFastingHours : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "is_vip",
            table: "Patient",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "patient_type",
            table: "Patient",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Individual");

        migrationBuilder.AddColumn<short>(
            name: "fasting_hours",
            table: "Visit",
            type: "smallint",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "is_vip",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "patient_type",
            table: "Patient");

        migrationBuilder.DropColumn(
            name: "fasting_hours",
            table: "Visit");
    }
}
