using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalLabSystem.Migrations
{
    /// <summary>
    /// [VS-01 - القرار 12] فصل PaymentMethod / BillingType / ReferringEntityCategory.
    /// - إضافة عمود billing_type إلى Visit (افتراضي Individual).
    /// - إضافة عمود category إلى ReferralSource (افتراضي ReferringDoctor).
    /// - ترحيل قيم Payment.payment_method القائمة:
    ///     * Contract → Cash (وتُعيَّن BillingType.LabToLab للزيارات المرتبطة).
    ///     * Cash / Insurance / Other تبقى كما هي.
    ///     * قيم Card و Check تصبح متاحة للاستخدام.
    /// - توسيع عرض عمود payment_method لاستيعاب القيم الجديدة.
    /// ⚠️ ملاحظة: هذا الترحيل لا يُطبَّق تلقائيًا — يجب أخذ نسخة احتياطية قبل Update-Database.
    /// </summary>
    /// <inheritdoc />
    public partial class VS01_SeparateBillingTypeAndPaymentMethodAndReferringCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // 1) إضافة عمود billing_type إلى جدول Visit.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.AddColumn<string>(
                name: "billing_type",
                table: "Visit",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Individual");

            // ═══════════════════════════════════════════════════════════════════
            // 2) إضافة عمود category إلى جدول ReferralSource.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "ReferralSource",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "ReferringDoctor");

            // ═══════════════════════════════════════════════════════════════════
            // 3) ترحيل بيانات BillingType للزيارات المرتبطة بجهات تعاقد (Company)
            //    أو التي كان لها Payment بقيمة "Contract" — تُصنَّف كـ LabToLab.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                UPDATE v
                SET v.[billing_type] = N'LabToLab'
                FROM [Visit] v
                WHERE v.[company_id] IS NOT NULL
                   OR EXISTS (
                        SELECT 1
                        FROM [Payment] p
                        WHERE p.[visit_id] = v.[visit_id]
                          AND p.[payment_method] = N'Contract'
                   );
            ");

            // ═══════════════════════════════════════════════════════════════════
            // 4) تحديث default constraint وعرض عمود payment_method قبل ترحيل القيم.
            //    نُسقط default constraint القديم ثم نوسع النوع ثم نُعيد الإنشاء.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                DECLARE @dfName sysname;
                SELECT @dfName = [d].[name]
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c]
                    ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
                WHERE [d].[parent_object_id] = OBJECT_ID(N'[Payment]')
                  AND [c].[name] = N'payment_method';
                IF @dfName IS NOT NULL
                    EXEC(N'ALTER TABLE [Payment] DROP CONSTRAINT [' + @dfName + N']');
            ");

            migrationBuilder.AlterColumn<string>(
                name: "payment_method",
                table: "Payment",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Cash",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Cash");

            // ═══════════════════════════════════════════════════════════════════
            // 5) ترحيل قيم Payment.payment_method:
            //    Contract → Cash (المفهوم الأساسي "استُلم نقدًا في سطر دفع").
            //    Cash / Insurance / Other تبقى كما هي.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                UPDATE [Payment]
                SET [payment_method] = N'Cash'
                WHERE [payment_method] = N'Contract';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ═══════════════════════════════════════════════════════════════════
            // 1) استرجاع قيمة "Contract" في Payment لأي سطر لزيارة LabToLab.
            //    (تقدير أفضل — الترحيل غير عكسي بالكامل بسبب فقدان القيم الأصلية.)
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.[payment_method] = N'Contract'
                FROM [Payment] p
                INNER JOIN [Visit] v ON v.[visit_id] = p.[visit_id]
                WHERE v.[billing_type] = N'LabToLab'
                  AND p.[payment_method] = N'Cash';
            ");

            // ═══════════════════════════════════════════════════════════════════
            // 2) إسقاط أعمدة billing_type و category.
            // ═══════════════════════════════════════════════════════════════════
            migrationBuilder.DropColumn(
                name: "billing_type",
                table: "Visit");

            migrationBuilder.DropColumn(
                name: "category",
                table: "ReferralSource");
        }
    }
}
