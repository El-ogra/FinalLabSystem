using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <summary>
    /// [VS-02 - القرار 12] ثنائية تسعير TestType.
    /// - إضافة عمود patient_default_price إلى TestType (افتراضي 0).
    /// - إضافة عمود lab_to_lab_default_price إلى TestType (افتراضي 0).
    /// - ترحيل بيانات: نسخ DefaultPrice إلى PatientDefaultPrice (1:1)،
    ///   وإلى LabToLabDefaultPrice مضروبًا × 0.7 كخصم افتراضي 30% قابل للتعديل يدويًا لاحقًا.
    /// ⚠️ ملاحظة: هذا الترحيل لا يُطبَّق تلقائيًا — يجب أخذ نسخة احتياطية قبل Update-Database.
    /// </summary>
    /// <inheritdoc />
    public partial class VS02_AddDualPricingToTestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // 1) إضافة عمود patient_default_price إلى جدول TestType.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.AddColumn<decimal>(
                name: "patient_default_price",
                table: "TestType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // ═══════════════════════════════════════════════════════════════════
            // 2) إضافة عمود lab_to_lab_default_price إلى جدول TestType.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.AddColumn<decimal>(
                name: "lab_to_lab_default_price",
                table: "TestType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // ═══════════════════════════════════════════════════════════════════
            // 3) ترحيل البيانات: نسخ DefaultPrice إلى الحقلين الجديدين
            //    مع تطبيق خصم افتراضي 30% على سعر LabToLab.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                UPDATE [TestType]
                SET [patient_default_price]  = [default_price],
                    [lab_to_lab_default_price] = CAST([default_price] * 0.7 AS decimal(18,2));
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // إسقاط العمودين — العودة إلى نموذج السعر المفرد DefaultPrice.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.DropColumn(
                name: "patient_default_price",
                table: "TestType");

            migrationBuilder.DropColumn(
                name: "lab_to_lab_default_price",
                table: "TestType");
        }
    }
}
