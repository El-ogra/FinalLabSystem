---
name: ef-core-migration-safety
description: كتابة Migration جديد بشكل آمن على FinalLabSystem الذي يحوي 33 migration سابقًا. يغطي قواعد التسمية الفعلية المستخدمة في FinalLabSystem/Migrations، ترتيب التطبيق، التعامل مع الحقول الحرجة طبيًا، توليد سكربت idempotent لمراجعة الإنتاج، التعامل مع Triggers/Views/Indexes، الأخطاء الشائعة في هذا المشروع تحديدًا. لا يُستخدم لإضافة حقل وحيد لكيان موجود (ذلك جزء من نمط إضافة الموديولات في adding-new-lab-module).
---

# EF Core Migration Safety — FinalLabSystem

## لماذا هذا الـ skill منفصل

`FinalLabSystem` عندها **33 migration فعليًا** حتى الآن (في `FinalLabSystem/Migrations/`)، معظمها ينتهي بأسماء مثل `Add<Field>` أو `ConvertXToY` أو `Reconcile<Field>`. كل واحد منها إضافة فوق مئات الجداول والـ views والـ triggers. أي migration يحوي عملية مدمّرة يكلف المعمل ساعات من إصلاح بيانات المرضى/النتائج.

هذا الـ skill يفرض القاعدة الذهبية: **يجب أن يكون أي migration قابلًا للعكس، وأن يُولَّد سكربت idempotent قبل نشره على الإنتاج، وأن يحترم الاسم الموجود في الأبجدي الفعلي للمشروع.**

## ما الذي يميّز هذا المشروع تحديدًا

- قاعدة البيانات **SQL Server** مع `sysdatetime()` defaults، Triggers (مثل `TR_Payment_SyncBalance` على جدول `Payment`)، Views (مثل `VPendingTests`, `VSampleTubeStatus`).
- حقل حرجة طبيًا لا تحتمل التوقف عن العمل فيه: `TestResult.ResultValueNumeric`, `TestResult.ValidationStatus`, `Visit.Status`, `Payment.VisitId`.
- كل كيان إما عليه `[Auditable]` أو غيره؛ الـ audit يلتقط كل تغيير (راجع skill `audit-trail-conventions`).
- الكود يستخدم `Update-Database` على dev، لكن على الإنتاج يمرّ عبر سكربت idempotent عبر `dotnet ef migrations script --idempotent`.

## القاعدة 1 — التسمية الفعلية المعتمدة في المشروع

النمط الذي يحترمه كل migration موجود:

```
<YYYYMMDDHHMMSS>_<PascalCaseName>.cs
```

مثال من الواقع: `20260610042518_AddTestResultValidationWorkflow.cs`. الأسماء الفعلية المرصودة:

- `Add<Table>` أو `Add<Field>` — لإضافة جدول/حقل.
- `Convert<X>To<Y>` — لتحويل نوع أو ترميز (مثال: `ConvertStatusFieldsToEnums`).
- `Reconcile<Field>` — لتوحيد عمود بعد اكتشاف تضارب (مثال: `ReconcileResultValueNumeric`).
- `Standardize<Thing>` — لتوحيد (`StandardizeMoneyAndAgeTypes`).
- `AddF<K>` — لإضافة foreign key (`AddTestTypeCollectionFK`).

**لا تستخدم** `Initial`, `Init`, `Update`, `Fix`، أو أسماء عامة. اتبع النمط أعلاه تمامًا.

## القاعدة 2 — قبل `dotnet ef migrations add`

افحص هذه القائمة قبل إنشاء أي migration جديد. عدم التحقق منها هو الخطأ الأكثر تكلفة في هذا المشروع:

1. **هل هناك trigger أو view يعتمد على هذا العمود؟**
   - افتح `Migrations/<last>/<last>.cs` وابحث عن `migrationBuilder.Sql(...)`.
   - أي شيء موجود هناك مرتبط بالـ schema الجديدة.
2. **هل توجد Migration سابقة عدّلت نفس العمود؟** افتح:
   ```bash
   cd FinalLabSystem
   grep -rn "<PropertyName>" Migrations/ 2>/dev/null | grep -v Designer
   ```
   أي عمود له تاريخ تعديل يجب أن يكون تعديلاً تراكميًا في migration واحد، لا متفرقًا.
3. **هل الكيان الجديد عليه `[Auditable]`؟** إذا نعم، أي تغيير يأخذ audit row تلقائيًا (راجع skill `audit-trail-conventions`).
4. **هل يوجد `db.SetDbContext().ChangeTracker` يلمس هذا الجدول في `SaveChangesAsync` override؟** نعم، كل الكيانات `Auditable` تُمسك.

## القاعدة 3 — توليد الـ Migration

```bash
cd FinalLabSystem
dotnet ef migrations add <PascalCaseName> \
  --project FinalLabSystem.csproj \
  --startup-project FinalLabSystem.csproj

# اقرأ <timestamp>_<Name>.cs كاملاً — لا تنشر قبل قراءته
cat Migrations/<timestamp>_<Name>.cs
```

**ما يجب أن تفحصه في الملف المُولَّد قبل الالتزام:**

| فحص | لماذا |
|---|---|
| `UP` يحوي `DropColumn` أو `AlterColumn` يغيّر النوع؟ | هذا حاجز نشر. إذا لا بديل، اتبع القاعدة 5. |
| هل يحوي `migrationBuilder.Sql(...)` لتعديل trigger/view؟ | تأكيد أن الـ SQL idempotent ومُعلَّق بسببه. |
| هل الـ indexes الجديد لها اسم فريد؟ | SQL Server يرفض إنشاء فهرس باسم مكرر. |
| هل الـ `ForeignKey` يستخدم `OnDelete.Restrict` على جدول طبي حساس؟ | الـ cascade يحذف نتائج مرضى — ممنوع. |

## القاعدة 4 — توليد سكربت idempotent للإنتاج

هذه الخطوة **إلزامية** قبل النشر على بيئة الإنتاج:

```bash
cd FinalLabSystem
dotnet ef migrations script \
  <LastAppliedMigrationName> \
  <NewMigrationName> \
  --idempotent \
  -o Migrations/rollout-<NewMigrationName>.sql
```

السكربت يبدأ بـ:

```sql
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'<full-migration-id>')
BEGIN
    -- DDL
END
GO
```

تأكد أن هذا الـ guard موجود، وإلا الـ migration سيُطبَّق مرتين على الإنتاج (= كارثة).

## القاعدة 5 — متى تستخدم `migrationBuilder.Sql` يدويًا

في الحالات التالية لا يكفي migration المُولَّد:

- **إضافة default values لعمود NOT NULL موجود فيه بيانات** — يجب 3 خطوات:
  1. `AddColumn` كـ nullable أولاً.
  2. `UPDATE ... SET col = <default> WHERE col IS NULL`.
  3. `AlterColumn` إلى NOT NULL.
- **إنشاء أو تعديل trigger** (مثل الـ `TR_Payment_SyncBalance` على جدول Payment) — الـ EF لا يدير ذلك.
- **تحديث views** يعتمد على أعمدة الجدول (مثل `VResultAuditTrail`).
- **أعمدة رقمية حرجة طبيًا** (مثل النطاق المرجعي `NormalRange.LowNormal`/`HighNormal`) — لا تتركها nullable في الإنتاج أبدًا.

## القاعدة 6 — قواعد "لا تلمس"

| الشيء | لا تلمسه إلا بقرار صريح وموثّق |
|---|---|
| `TestResult.ResultValueNumeric` | لا تُغيّر نوعه أبدًا (يُستخدم في التقارير والـ audit trail). |
| `Visit.Status` enum | لا تحوّله إلى string؛ أنشئ enum جديد إن لزم. |
| `Payment.VisitId` FK | cascade ممنوع — يحذف دفعات مع زيارات. |
| أي view في `V*` | الـ view يُعاد حسابه من خلال trigger أو يدويًا. |
| `AuditLog` نفسه | لا تضيف columns جديدة إلا إذا راجعت skill `audit-trail-conventions`. |

## القاعدة 7 — عند الفشل في الإنتاج

1. **لا تعمل `Update-Database` مرتين.** الـ history table يحميك، لكن سكربتات idempotent مكررة قد تُدخل الـ data مرتين.
2. تحقق:
   ```sql
   SELECT TOP 1 * FROM [__EFMigrationsHistory] ORDER BY MigrationId DESC
   ```
3. إذا الـ migration فشل جزئيًا (مثلاً أُضيف العمود لكن الـ trigger فشل):
   - راجع `Migrations/rollout-<name>.sql` يدويًا.
   - اقسم الـ rollback إلى خطوات manually بنفس الترتيب العكسي.
4. لا تنشر الترميم قبل اختباره على نسخة staging فيها schema مطابق للإنتاج.

## Checklist قبل الالتزام بأي migration

```
□ Migration name يطابق النمط المعتمد (<YYYYMMDDHHMMSS>_<PascalCase>)
□ قُرئ الـ .cs كاملاً بعد التوليد؛ لا أوامر DropTable على جداول مرجعية
□ سكربت idempotent مولّد عبر --idempotent
□ مراجعة FK أن لا cascade على جداول طبية حرجة
□ التحقق من أي trigger/view يعتمد على العمود
□ إذا الـ migration يضيف NOT NULL لعمود فيه بيانات → 3 خطوات (AddColumn nullable → UPDATE → AlterColumn)
□ إذا الكيان Auditable → تأكيد أن audit logging لا تنكسر (skill audit-trail-conventions)
□ اختبار على dev بـ Update-Database، اختبار الاسترجاع بـ Update-Database <PreviousMigration>
```

## أوامر سريعة للمشروع

```bash
# عرض كل migrations المطبقة فعليًا
cd FinalLabSystem
dotnet ef migrations list --project FinalLabSystem.csproj

# عرض أي migration معلق
dotnet ef migrations list --no-connect

# إزالة آخر migration (قبل فقط، لا تعمل هذا بعد تطبيق الإنتاج)
dotnet ef migrations remove --project FinalLabSystem.csproj
```

## الأخطاء الفعلية التي يحميك منها هذا الـ skill

- **خطأ "There is already an object named 'IX_*'"** → لا يوجد index بنفس الاسم بين الـ migrations.
- **خطأ "Cannot drop the table 'Patient' because it is referenced by a foreign key"** → الـ cascade صحيح ما يصل لـ EF.
- **خطأ "Column 'result_value' cannot be null"** → نسيت خطوة الـ UPDATE للقيم الموجودة.
- **خطأ تاريخ بعد تطبيق نصف migration** → نصف الـ history record — استخدم idempotent script، لا تعِد `Update-Database`.

## عند لمسك migration فعلي

افتح دائمًا آخر migration موجود كمرجع أنماط:

```bash
cd /workspace/lab-system/FinalLabSystem/Migrations
ls -t *.cs | grep -v Designer | head -5
```

اقرأ آخر ملفين على الأقل قبل إنشاء جديد — يحتوون على أنماط الترميز المحلي الفعلي (snake_case، `[Auditable]`, الـ custom conventions) التي يجب أن يحترمها migration الجديد.
