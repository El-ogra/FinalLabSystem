using FinalLabSystem.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations;

[DbContext(typeof(FinalLabDbContext))]
[Migration("20260605000200_AddVisitMedicalHistoryAndOutsideSamples")]
public partial class AddVisitMedicalHistoryAndOutsideSamples : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "taken_outside_lab",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "outside_urine",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "outside_stool",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "outside_blood",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "outside_semen",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "outside_csf",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_diabetes",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_anemia",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_bleeding_disorder",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_thyroid",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_joint_disease",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_viral_infection",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "on_anticoagulant",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_hypertension",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_liver_disease",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_kidney_disease",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "has_lupus",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<bool>(
            name: "had_xray_contrast",
            table: "Visit",
            type: "bit",
            nullable: false,
            defaultValue: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "taken_outside_lab", table: "Visit");
        migrationBuilder.DropColumn(name: "outside_urine", table: "Visit");
        migrationBuilder.DropColumn(name: "outside_stool", table: "Visit");
        migrationBuilder.DropColumn(name: "outside_blood", table: "Visit");
        migrationBuilder.DropColumn(name: "outside_semen", table: "Visit");
        migrationBuilder.DropColumn(name: "outside_csf", table: "Visit");
        migrationBuilder.DropColumn(name: "has_diabetes", table: "Visit");
        migrationBuilder.DropColumn(name: "has_anemia", table: "Visit");
        migrationBuilder.DropColumn(name: "has_bleeding_disorder", table: "Visit");
        migrationBuilder.DropColumn(name: "has_thyroid", table: "Visit");
        migrationBuilder.DropColumn(name: "has_joint_disease", table: "Visit");
        migrationBuilder.DropColumn(name: "has_viral_infection", table: "Visit");
        migrationBuilder.DropColumn(name: "on_anticoagulant", table: "Visit");
        migrationBuilder.DropColumn(name: "has_hypertension", table: "Visit");
        migrationBuilder.DropColumn(name: "has_liver_disease", table: "Visit");
        migrationBuilder.DropColumn(name: "has_kidney_disease", table: "Visit");
        migrationBuilder.DropColumn(name: "has_lupus", table: "Visit");
        migrationBuilder.DropColumn(name: "had_xray_contrast", table: "Visit");
    }
}
