---
name: lis-session-handoff
description: إنشاء ملف Handoff منظّم بين جلسة تخطيط وجلسة تنفيذ في FinalLabSystem. يضغط حالة المشروع، تقدّم خطة العمل بالشرائح، الحقائق المؤكدة، القرارات، الملفات المعدّلة، والمهمة التالية بتفاصيل كافية ليبدأ وكيل جديد فورًا دون إعادة التحليل. يستدعى عند قول المستخدم: «جهّز handoff للجلسة التالية»، «اكتب ملخص لما أنجزناه»، «أنا بوقف هنا، كمّل في الجلسة الجاية»، «wrap up الجلسة».
disable-model-invocation: true
argument-hint: "اسم الشريحة أو المهمة القادمة + (اختياري) ملاحظات إضافية"
---

# LIS Session Handoff — FinalLabSystem

> **هدف المهارة الوحيد:** إنتاج ملف Handoff قابل للقراءة البشرية والآلية، يضغط محادثة طويلة (تخطيط أو تنفيذ) في وثيقة واحدة يستطيع وكيل جديد من خلالها المتابعة فورًا **بدون** إعادة فحص الملفات أو إعادة اتخاذ القرارات.

---

## متى تستدعى هذه المهارة

| الموقف | استدعاء المهارة؟ |
|--------|------------------|
| انتهاء جلسة تخطيط موديول قبل بدء التنفيذ | ✅ نعم |
| تنفيذ شريحة واحدة والانتقال للشريحة التالية في جلسة جديدة | ✅ نعم |
| اكتشاف قرار معماري يحتاج تسليمه للجلسة القادمة | ✅ نعم |
| انقطاع متوقع (انتهاء وقت، إجازة، حدود توكن) | ✅ نعم |
| إنهاء المشروع ككل | ❌ لا — استخدم ملخصًا أبسط في PR description |

---

## المخرج النهائي — ملف واحد دائمًا

**مسار الحفظ:** `FinalLabSystem/FinalLabSystem/Docs/PRDs/Handoff-<اسم-الشريحة-أو-المهمة>.md`

**لماذا هذا المسار بالذات؟**
- داخل مشروع WPF الفعلي (`FinalLabSystem/FinalLabSystem/`)، وليس في جذر المستودع.
- تحت `Docs/PRDs/` لأنها وثائق مرجعية طويلة الأمد، ليست نتائج اختبارات.
- اسم الملف يبدأ بـ `Handoff-` ليسهل البحث والفلترة.

---

## هيكل ملف Handoff الكامل

```markdown
# Handoff: <اسم الشريحة أو المهمة>

> **تاريخ:** <YYYY-MM-DD HH:MM>
> **الفرع:** `before-prd`
> **آخر commit:** `<SHA>` — "<رسالة الـ commit>"
> **الجلسة السابقة:** <اسم الجلسة أو وصف مختصر>
> **الجلسة التالية المتوقعة:** <متى وكيف ستُكمل>

## 0. ملخّص تنفيذي (30 ثانية قراءة)

<فقرة 2-4 أسطر تصف: أين نحن الآن، ماذا أنجزنا، ماذا بقي، ما الخطوة التالية الفورية>

## 1. حالة المشروع

| العنصر | القيمة |
|--------|--------|
| آخر migration مطبَّقة | `20260610021544_AddUnitToNormalRange` (أو الأحدث) |
| عدد الـ migrations الكلي | 33 |
| عدد الاختبارات الحالي | <N> ناجح / <N> إجمالي |
| Build | ✅ 0 errors, 0 warnings |
| الفرع الحالي | `before-prd` |
| آخر commit SHA | `cab1f46a...` |

**أوامر التحقق السريعة:**
```bash
git log --oneline -5
dotnet build
dotnet test --no-build
```

## 2. تقدّم خطة العمل

> اشرح في جملة ما هي المرحلة الحالية وكم قطعنا منها.

**مثال:**

المرحلة الحالية: **المرحلة 6 — Backup & Restore**
- الشريحة 6.0: ✅ مكتملة (3 اختبارات)
- الشريحة 6.1: ✅ مكتملة (10 اختبارات — AES + Audit)
- الشريحة 6.2: 🔄 **في التنفيذ — أنت هنا** (الخدمة جاهزة، الاختبارات لم تكتمل)
- الشريحة 6.3: ⏳ لم تبدأ

**تفاصيل الشريحة الجارية (6.2):**
- ✅ `IBackupService` و `BackupService` مكتوبان
- ✅ `_auditService.LogActionAsync(...)` مُستدعَى في كل عملية حساسة
- ⏳ الاختبارات: كُتب 4 من أصل 8 (ناقص: Category B و C)
- ⏳ Migration أُنشئت لكن لم تُطبَّق على InMemory بعد

## 3. الحقائق المؤكدة (لا تُعد فحصها أبدًا)

> هذه حقائق تم التحقق منها في هذه الجلسة. **اعتبرها موثوقة 100%.**
> لا تعيد قراءة الكود الذي تثبتّ هذه الحقائق منه.

### 3.1 بنية قاعدة البيانات

- اسم جدول `LabSetting` في DB: **`LabSettings`** (جمع، ليس `LabSetting`)
- جدول `AuditLog`: العمود `Action` طوله `HasMaxLength(1) IsFixedLength` — حرف واحد فقط: `C`, `U`, `D`, `B`, `R`
- كيان `Patient` الحقول الإلزامية: `PatientCode` (30)، `FullNameAr` (200، Required)، `Sex` (1، Required)
- كيان `Staff` الحقول الإلزامية: `Username`, `DisplayName`, `PasswordHash`, `IsActive = true`

### 3.2 توقيعات الخدمات (Contracts)

- `IAuditService.LogActionAsync(string tableName, int recordId, string action, int staffId, string? notes = null)` — توقيع ثابت، لا تغيير
- `ICurrentUserSession.CurrentUser` يرجع `Staff?` — `null` يعني غير مسجل
- `ICurrentUserSession.IsAuthenticated` يجب أن يكون متزامنًا مع `CurrentUser != null`

### 3.3 اتفاقيات الكود

- كل التواريخ في DB والكود: `DateTime.UtcNow` (لا `DateTime.Now` أبدًا)
- كل بيانات المرضى في seed: `Sex = "M"` إلزامي (العمود NOT NULL)
- كل VMs ترث `Infrastructure.ViewModelBase` (لا `ObservableObject` من CommunityToolkit)
- كل الخدمات والـ VMs والـ Windows: `public` (لا `internal`)
- Audit يعمل تلقائيًا عبر `override SaveChangesAsync` على DbContext (لا حاجة لاستدعاء يدوي)

### 3.4 أكواد وصلنا إليها

- تشفير AES في `Infrastructure/Security/AesEncryptionHelper.cs` — استخدمه بدل أي مكتبة خارجية
- `Infrastructure/Session/CurrentUserSession.cs` يحقن `Staff` الحالي
- `App.xaml.cs` يفعّل `db.Database.MigrateAsync()` عند الإقلاع — لا حاجة لتشغيل migration يدويًا في التطوير

## 4. القرارات المتخذة في هذه الجلسة

> القرارات التي يجب ألا تُتخذ من جديد في الجلسة التالية.

| # | القرار | السبب | البدائل المرفوضة |
|---|--------|-------|------------------|
| 1 | استخدام JSON + AES للتخزين بدل T-SQL BACKUP | لا نملك صلاحيات sysadmin على SQL Server | T-SQL BACKUP, RESTORE — مستبعد لصلاحيات |
| 2 | استخدام `IModel.GetEntityTypes()` لإنشاء نسخة احتياطية كاملة | 44 جدول، القراءة اليدوية ستأخذ ساعات | قراءة كل جدول يدويًا |
| 3 | إضافة `[Auditable]` لـ `LabBackup` رغم أنها ليست بيانات طبية | لأن حذف/إنشاء Backup يجب أن يُسجَّل قانونيًا | عدم التدقيق — مستبعد |
| 4 | إنشاء pre-restore backup تلقائيًا قبل أي restore | منع فقدان البيانات | السماح بالاستعادة المباشرة — مستبعد |

## 5. الملفات التي تم لمسها في هذه الجلسة

> **روابط بمسار كامل.** لا تنسخ محتوى الملفات هنا أبدًا.

### 5.1 ملفات جديدة

| المسار | الوصف |
|--------|-------|
| `FinalLabSystem/Models/LabBackup.cs` | كيان النسخة الاحتياطية |
| `FinalLabSystem/Migrations/20260703_AddLabBackup.cs` | migration إنشاء الجدول |
| `FinalLabSystem/Services/Interfaces/IBackupService.cs` | واجهة الخدمة |
| `FinalLabSystem/Services/Implementations/BackupService.cs` | تنفيذ الخدمة (246 سطر) |
| `FinalLabSystem.Tests/Services/BackupServiceTests.cs` | 8 اختبارات وحدة (4 منها مكتملة) |

### 5.2 ملفات معدّلة

| المسار | التعديل |
|--------|---------|
| `FinalLabSystem/Data/FinalLabDbContext.cs` | أضيف `modelBuilder.Entity<LabBackup>(...)` |
| `FinalLabSystem/App.xaml.cs` | أضيف `services.AddScoped<IBackupService, BackupService>()` |

## 6. المهمة التالية المباشرة (ابدأ فورًا)

> **هذا القسم هو أهم شيء في الملف.** يجب أن يكون محددًا لدرجة أن الوكيل الجديد يبدأ العمل بدون أي سؤال.

### 6.1 المهمة

أكمل كتابة اختبارات `BackupService` في `FinalLabSystem.Tests/Services/BackupServiceTests.cs`.

### 6.2 الاختبارات المطلوبة (4 متبقية من 8)

| الفئة | اسم الاختبار | الحالة | التفاصيل |
|-------|-------------|--------|----------|
| B | `RestoreBackupAsync_DecryptsFile_WithCorrectPassword` | ⏳ لم يبدأ | استخدم `AesEncryptionHelper.Encrypt(...)` لإنشاء ملف ثم `RestoreBackupAsync` |
| B | `RestoreBackupAsync_ReturnsFalse_WhenPasswordIsWrong` | ⏳ لم يبدأ | أنشئ ملف بكلمة مرور مختلفة، استدعِ `RestoreBackupAsync` بكلمة أخرى |
| C | `CreateBackupAsync_CallsAudit_WithAction_B` | ⏳ لم يبدأ | تحقق: `audit.Verify(... "B" ...)` |
| C | `RestoreBackupAsync_CreatesPreRestoreBackup_BeforeRestoring` | ⏳ لم يبدأ | تحقق: عدد النسخ الاحتياطية في `LabBackups` يزيد 1 بعد الاستعادة |

### 6.3 القيود الصارمة

- ⚠️ **استخدم `InMemoryDbContextFactory.Create(nameof(...))`** لإنشاء Context لكل اختبار
- ⚠️ **`Sex = "M"`** إلزامي في كل `Staff` و `Patient` في seed
- ⚠️ **`DateTime.UtcNow`** لكل التواريخ
- ⚠️ **Moq فقط** (لا NSubstitute، لا FluentAssertions)

### 6.4 معايير القبول (Definition of Done)

- [ ] عدد الاختبارات في `BackupServiceTests.cs` = **8 بالضبط**
- [ ] `dotnet build` بدون warnings
- [ ] `dotnet test --filter "FullyQualifiedName~BackupServiceTests"` جميعها ✅
- [ ] لا كود مكرر مع `BackupServiceIntegrationTests.cs`

### 6.5 المخاطر المتوقعة

- ⚠️ `RestoreBackupAsync` يعتمد على الـ file system. **استخدم `Path.Combine(Path.GetTempPath(), "BackupTests_" + Guid.NewGuid())`** كما في `CreateBackupService` factory.
- ⚠️ إذا فشل الاختبار بسبب `TransactionIgnoredWarning`، تأكد أن `ConfigureWarnings(w => w.Ignore(...))` موجود في الـ factory.

## 7. تعليمة صريحة للوكيل التالي

> **هذا الملف نتاج تحليل مكتمل ونهائي.**

```
⚠️ لا تُعد فحص أو قراءة الملفات المذكورة في القسم 5.
   كل الحقائق في القسم 3 موثوقة 100%.
   كل القرارات في القسم 4 نهائية.

✅ ابدأ فورًا من القسم 6 ("المهمة التالية المباشرة").
   نفّذ الاختبارات الأربعة المتبقية بالترتيب.

✅ عند الانتهاء، أنشئ Handoff جديد للشريحة التالية (6.3).
```

## 8. المراجع السريعة

> أشر إلى الملفات الأخرى بدون نسخ محتواها.

- **وثيقة المتطلبات:** `FinalLabSystem/Docs/PRDs/Backup/03-Requirements-Work-Plan.md`
- **Work Plan الكامل:** `FinalLabSystem/Docs/PRDs/Backup/WorkPlan-Backup.md`
- **تحليل الفجوات:** `FinalLabSystem/Docs/PRDs/Backup/02-Gap-Analysis.md`
- **معايير الاختبارات:** `AgentSkills/lis-xunit-testing-conventions/SKILL.md`
- **معايير DI:** `AgentSkills/di-and-navigation-registration/SKILL.md`
- **معايير Audit:** `AgentSkills/audit-trail-conventions/SKILL.md`

## 9. ملاحظات إضافية (اختياري)

<أي ملاحظات لا تندرج في الأقسام أعلاه — مثل "المطوّر غير متأكد من X"، أو "القرار يتوقف على رد العميل Y">
```

---

## قواعد كتابة Handoff ناجح

### 1. اختبر قابلية «القفز فوق»

**القاعدة:** لو قرأ وكيل جديد فقط **القسم 0 + القسم 6**، يجب أن يستطيع البدء.

| ❌ لا تبدأ من هنا | ✅ ابدأ من هنا |
|-------------------|---------------|
| "أكمل الشريحة 6.2 من خطة العمل" | "أنشئ 4 اختبارات في `BackupServiceTests.cs` — الأسماء في الجدول بقسم 6.2" |
| "بعد مراجعة الكود" | "الكود موجود في `BackupService.cs`، الـ 4 اختبارات الناقصة مذكورة أعلاه" |

### 2. الحقائق المؤكدة: لا تنسَ أبدًا

**القاعدة:** أي شيء تحققت منه لأول مرة في هذه الجلسة يدخل القسم 3.

**لماذا؟** لأن الجلسة التالية قد لا تملك السياق، وستضيع ساعات في إعادة التحقق.

### 3. القرارات: اذكر البدائل المرفوضة

**القاعدة:** ليس كافيًا أن تقول "اخترنا X"، بل "اخترنا X لأن Y، ورفضنا Z لأن W".

**لماذا؟** حتى لا يعيد الوكيل التالي فتح النقاش.

### 4. الملفات: مسارات كاملة فقط

**القاعدة:** `FinalLabSystem/Services/Implementations/BackupService.cs` وليس `BackupService.cs`.

### 5. التعليمة الصريحة للوكيل التالي

**القاعدة:** يجب أن تكون في قسم 7 منفصل، بصيغة تحذيرية.

| ❌ ضعيف | ✅ قوي |
|--------|--------|
| "تابع من النقطة التي توقفنا عندها" | "ابدأ فورًا من القسم 6 — لا تقرأ أي ملف آخر" |

---

## ما لا يجب تكراره في Handoff

- ❌ محتوى الكود الكامل (أشر بالمسار)
- ❌ PRD كامل أو Work Plan كامل (أشر بالمسار)
- ❌ رسائل الـ commits السابقة (أشر بالـ SHA فقط)
- ❌ قائمة بجميع الاختبارات الناجحة (اذكر العدد فقط)

---

## أمثلة واقعية

### مثال 1: Handoff قصير (شريحة بسيطة)

```markdown
# Handoff: الشريحة 6.0 — جدول LabBackup

## 0. ملخص
الشريحة 6.0 مكتملة. تم إنشاء كيان `LabBackup` + migration. الخطوة التالية: الشريحة 6.1 (خدمة BackupService).

## 1. حالة المشروع
- Migrations: 34 (الأحدث: `20260703_AddLabBackup`)
- اختبارات: 109 ناجح
- Build: ✅

## 6. المهمة التالية
ابدأ من `Services/Interfaces/IBackupService.cs`. عرّف 5 دوال: `CreateBackupAsync`, `RestoreBackupAsync`, `GetBackupHistoryAsync`, `DeleteBackupAsync`, `GetBackupByIdAsync`. استخدم `JsonSerializer` للتسلسل.
```

### مثال 2: Handoff طويل (شريحة معقدة)

```markdown
# Handoff: الشريحة 7.3 — Microbiology Culture Workflow

## 0. ملخص
الشريحة 7.3 في منتصف التنفيذ. خدمة `IMicrobiologyService` جاهزة، الـ ViewModel `MicrobiologyEntryViewModel` جاهز، لكن اختبار E2E `MicrobiologyWorkflowEndToEndTests.cs` يفشل في حالة `MicrobiologyCulture_WithContaminatedSample_ReturnsRejected`. السبب قيد التحقيق. الخطوة التالية: إصلاح الاختبار + إكمال 6 اختبارات وحدة متبقية.

## 1. حالة المشروع
... (جدول كامل)

## 2. تقدّم خطة العمل
... (تفاصيل كل شريحة)

## 3. الحقائق المؤكدة
... (15 حقيقة)

## 4. القرارات
... (5 قرارات ببدائلها)

## 5. الملفات
... (12 ملف)

## 6. المهمة التالية
... (تفاصيل دقيقة)
```

---

## بعد كتابة Handoff

1. **احفظ الملف** في المسار المحدد.
2. **اعرضه على المطوّر** للمراجعة — خاصة قسم "المهمة التالية".
3. **اعمل commit** بالملف + رسالة واضحة:
   ```bash
   git add "FinalLabSystem/Docs/PRDs/Handoff-<Name>.md"
   git commit -m "handoff: <module> slice <N>"
   git push origin before-prd
   ```
4. **ابدأ الجلسة التالية** بقراءة هذا الملف فقط.

---

## قائمة مراجعة سريعة

- [ ] **القسم 0:** ملخص تنفيذي ≤ 4 أسطر؟
- [ ] **القسم 1:** حالة المشروع محدّثة (آخر migration، عدد الاختبارات، build)؟
- [ ] **القسم 2:** كل الشرائح مع علامة حالة (✅/🔄/⏳)؟
- [ ] **القسم 3:** حقائق مؤكدة كافية لتجنّب إعادة الفحص؟
- [ ] **القسم 4:** قرارات مع البدائل المرفوضة؟
- [ ] **القسم 5:** مسارات كاملة فقط، بدون محتوى؟
- [ ] **القسم 6:** المهمة التالية محددة لدرجة أن الوكيل الجديد يبدأ فورًا؟
- [ ] **القسم 7:** تعليمة صريحة بـ "لا تُعد الفحص"؟
- [ ] **القسم 8:** مراجع بمسارات صحيحة؟