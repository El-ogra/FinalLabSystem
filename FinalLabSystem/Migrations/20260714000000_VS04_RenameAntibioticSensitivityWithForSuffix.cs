using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <summary>
    /// VS-04 — إعادة تسمية قيم <c>AntibioticSensitivity</c> بلاحقة "For" (القرار 22).
    /// التغيير على مستوى الكود فقط:
    /// Highly → HighlyFor (byte 0)
    /// Moderate → ModerateFor (byte 1)
    /// Low → LowFor (byte 2)
    /// Resistant → ResistantFor (byte 3)
    /// القيم الرقمية المخزَّنة في العمود <c>OrganismAntibiotic.sensitivity</c> (tinyint)
    /// لم تتغيّر مطلقاً — لا حاجة لأي DDL أو DML.
    /// هذا الترحيل يبقى فارغاً بشكل مقصود ليُحدِّث <c>__EFMigrationsHistory</c>
    /// ويُوثِّق نقطة الفصل بين إصدارَي التسمية.
    /// </summary>
    /// <inheritdoc />
    public partial class VS04_RenameAntibioticSensitivityWithForSuffix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // لا تغييرات هيكلية أو بيانية — VS-04 إعادة تسمية على مستوى الكود فقط.
            // القيم الرقمية للـ enum ثابتة، والعمود tinyint يبقى كما هو.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // لا شيء لعكسه — إعادة التسمية على مستوى الكود فقط.
        }
    }
}
