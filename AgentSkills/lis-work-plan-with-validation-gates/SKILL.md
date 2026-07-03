---
name: lis-work-plan-with-validation-gates
description: إنشاء ملف Work Plan احترافي لأي مرحلة تطوير في FinalLabSystem. يقسّم المرحلة إلى شرائح (Slices) مرتبة منطقيًا، ولكل شريحة Pre-condition + قائمة ملفات + قائمة اختبارات + Validation Gate قابل للقياس. يستدعى عند قول المستخدم: «اكتب خطة عمل لـ...»، «قسّم هذا العمل إلى شرائح»، «أريد خطة قابلة للتنفيذ»، «احتاج work plan».
disable-model-invocation: true
argument-hint: "اسم المرحلة أو الموديول + (اختياري) PRD مرجعي"
---

# LIS Work Plan with Validation Gates — FinalLabSystem

> **هدف المهارة الوحيد:** إنتاج ملف Work Plan احترافي يحول متطلبات مرحلة ما إلى شرائح تنفيذ مرتّبة، لكل شريحة Validation Gate قابل للقياس (`dotnet build` نظيف + عدد اختبارات معلوم + 0 failures).

---

## متى تستدعى هذه المهارة

| الموقف | استدعاء المهارة؟ |
|--------|------------------|
| لديك PRD/وثيقة متطلبات وتريد تقسيمها لشرائح | ✅ نعم |
| لديك موديول كبير وتريد خطة منظمة لتنفيذه | ✅ نعم |
| تريد إضافة موديول كامل مثل Blood Bank أو Vaccination | ✅ نعم |
| المهمة تغيير صغير (إصلاح خطأ، إعادة تسمية) | ❌ لا — نفّذ مباشرة |
| المطلوب تقرير أو توثيق فقط | ❌ لا — استخدم `lis-pdf-reference-extractor` إن كان المصدر PDF |

---

## المخرج النهائي — ملف واحد دائمًا

**مسار الحفظ:** `FinalLabSystem/Docs/PRDs/<ModuleName>/WorkPlan-<ModuleName>.md`

> **إذا لم يكن المجلد موجودًا، أنشئه.** كل Work Plan يُحفظ في مجلد الموديول الخاص به مع وثيقة المتطلبات.

---

## هيكل ملف Work Plan الكامل

```markdown
# Work Plan: <ModuleName>

> **للمراجعة من المطوّر قبل البدء بالتنفيذ.**
> المصدر: <PRD أو المتطلبات المرجعية>
> تاريخ الإنشاء: <YYYY-MM-DD>
> الفرع: `before-prd`
> العدد المتوقع للاختبارات الإضافية: <N> اختبار
> العدد المتوقع للملفات الجديدة: <N> ملف

## 1. ملخص المرحلة

<فقرة 2-3 أسطر تصف ماذا نضيف ولماذا في هذه المرحلة>

## 2. معايير القبول الإجمالية للمرحلة

- [ ] جميع الشرائح مكتملة
- [ ] عدد الاختبارات الإضافية ≥ <N>
- [ ] `dotnet build` بدون warnings
- [ ] `dotnet test` جميعها ✅
- [ ] لا توجد migrations معلّقة
- [ ] لا توجد خدمات أو نوافذ غير مسجّلة في DI/Navigation

## 3. القيود الصارمة (تطبَّق على كل شريحة)

| القيد | القاعدة |
|-------|---------|
| نمط MVVM | كل VM يرث `Infrastructure.ViewModelBase` (لا `ObservableObject`) |
| الأوامر | ترث `Infrastructure.RelayCommand` أو `AsyncRelayCommand` |
| DI | كل خدمة جديدة مسجّلة في `App.xaml.cs.ConfigureServices` |
| Navigation | كل نافذة جديدة مسجّلة بـ `navigation.RegisterWindow<VM, Window>()` |
| الـ ORM | EF Core 8 + SQL Server، كل تغيير عبر migration جديد |
| الـ Testing | xUnit + Moq + EF Core InMemory (ممنوع: NSubstitute, FluentAssertions, Testcontainers) |
| التواريخ | `DateTime.UtcNow` في كل مكان (ممنوع: `DateTime.Now`, `DateTime.Today`) |
| بيانات المريض | `Sex = "M"` إلزامي في كل seed data |
| التغليف | كل الخدمات والـ ViewModels والـ Windows `public` (ممنوع: `internal`) |
| الكود العربي | رسائل الخطأ بالعربية، الـ UI بـ `FlowDirection="RightToLeft"` |

## 4. شرائح التنفيذ (مرتّبة)

### الشريحة 1: <اسم قصير>

**Pre-condition:** لا شيء (شريحة تأسيسية).

**الملفات المتوقعة:**

| النوع | المسار | المسؤولية |
|-------|--------|-----------|
| Model | `Models/X.cs` | تعريف الكيان + `[Auditable]` |
| Migration | `Migrations/<Timestamp>_AddX.cs` | إنشاء الجدول |
| Interface | `Services/Interfaces/IXService.cs` | عقد الخدمة |
| Implementation | `Services/Implementations/XService.cs` | منطق الخدمة |
| Tests | `Tests/Services/XServiceTests.cs` | اختبارات الوحدة |
| Tests | `Tests/Services/XServiceRegistrationTests.cs` | اختبار تسجيل DI |
| Tests | `Tests/Integration/XEndToEndTests.cs` | اختبار E2E |

**الاختبارات المتوقعة (8 اختبارات على الأقل):**

| الفئة | العدد | أمثلة |
|-------|-------|--------|
| DI Registration | 1 | `IXService_IsRegisteredInDI` |
| Validation | 2 | `XAsync_ThrowsArgumentNull_WhenInputIsNull` |
| Happy Path | 3 | `XAsync_ReturnsExpected_WhenValid` |
| Error Path | 2 | `XAsync_LogsError_WhenDatabaseFails` |

**Validation Gate:**

- [ ] عدد الاختبارات ≥ 8، كلها ✅
- [ ] `dotnet build` بدون errors أو warnings
- [ ] لا توجد migrations معلّقة لم تُطبَّق
- [ ] الخدمة مسجّلة في `App.xaml.cs.ConfigureServices`
- [ ] الكود يتوافق مع جميع القيود في القسم 3

---

### الشريحة 2: <اسم قصير>

**Pre-condition:** الشريحة 1 ✅ مكتملة.

**الملفات المتوقعة:** ...

**الاختبارات المتوقعة (N اختبار):**

**Validation Gate:** ...

---

### الشريحة N: ...

---

## 5. جدول ملخص الشرائح

| # | الشريحة | الحالة | عدد الملفات | عدد الاختبارات | Validation Gate |
|---|---------|--------|-------------|----------------|-----------------|
| 1 | <اسم> | ⏳ لم تبدأ | 7 | 8 | [ ] |
| 2 | <اسم> | ⏳ لم تبدأ | 5 | 6 | [ ] |
| 3 | <اسم> | ⏳ لم تبدأ | 4 | 5 | [ ] |
| **المجموع** | | | **16** | **19** | |

## 6. ترتيب التنفيذ

**يُنفَّذ بالترتيب الرقمي.** كل شريحة تبني على السابقة. لا تبدأ شريحة N قبل أن تكون الشريحة N-1 قد اجتازت Validation Gate.

**للتخطيط الموازي (نادر):** يمكن تنفيذ شريحتين متوازيتين فقط إذا:
- لا تلمسان نفس الملفات
- لا تلمسان نفس جداول قاعدة البيانات
- تم الاتفاق مع المطوّر مسبقًا

## 7. ما لا يدخل في هذه المرحلة (خارج النطاق)

- ميزة X (مؤجلة للمرحلة 7.2)
- ميزة Y (لم يطلبها المستخدم)

## 8. المخاطر المحتملة

| المخاطرة | الاحتمال | الأثر | خطة التخفيف |
|----------|----------|-------|-------------|
| Migration كبيرة تفشل على بيانات حقيقية | متوسط | عالي | اختبار على نسخة احتياطية أولًا |
| تداخل مع Audit التلقائي | منخفض | متوسط | اختبار أن `[Auditable]` يعمل على الكيان الجديد |

## 9. معالم النجاح (Definition of Done)

- [ ] جميع الشرائح مكتملة ومرّت Validation Gates
- [ ] جميع الاختبارات الجديدة مكتوبة وتمرّ
- [ ] لا انحدار (regression) في الاختبارات القديمة
- [ ] تم تحديث `Docs/FinalLab_Modules_Documentation.md` بالميزات الجديدة
- [ ] تم commit + push للفرع `before-prd`
```

---

## قواعد كتابة الشريحة الصحيحة

### 1. Pre-condition دقيق

| ❌ خطأ | ✅ صحيح |
|--------|---------|
| "بعد الكود الأساسي" | "الشريحة 1 ✅ مكتملة + Migration `AddX` مطبَّق" |
| "بعد التصميم" | "الشريحة 2 ✅ — نموذج X موجود في `Models/X.cs`" |

### 2. عدد الاختبارات واقعي

| نوع الخدمة | العدد الأدنى المقبول |
|------------|---------------------|
| CRUD بسيط (كيان واحد فقط) | 5 |
| CRUD مع منطق (تحقق، حساب) | 8 |
| خدمة معقدة (workflow متعدد الخطوات) | 12+ |

### 3. قائمة الملفات كاملة

**لا تنسَ أبدًا:**
- ملفات الـ Tests (3 ملفات لكل خدمة جديدة: Tests + Registration + Integration)
- ملفات الـ Registration في `App.xaml.cs` (لا تُحسب كملف جديد لكنها إلزامية)
- ملفات الـ Migration (واحد على الأقل لكل تغيير على DB)

### 4. Validation Gate قابل للقياس

**كل بند في Validation Gate يجب:**
- أن يكون قابلاً للتحقق الموضوعي (boolean: نعم/لا)
- أن يكون محددًا برقم أو شرط واضح
- أن يكون قابلاً للاختبار في أقل من دقيقتين

| ❌ غير قابل للقياس | ✅ قابل للقياس |
|-------------------|---------------|
| "الكود نظيف" | "0 warnings في `dotnet build`" |
| "الاختبارات كافية" | "≥ 8 اختبارات، كلها ✅" |
| "التوثيق موجود" | "تم تحديث `FinalLab_Modules_Documentation.md`" |

---

## أمثلة واقعية من المشروع

### مثال 1: شريحة بسيطة (إعداد جدول جديد)

```markdown
### الشريحة 1: جدول LabBackup

**Pre-condition:** لا شيء.

**الملفات المتوقعة:**
- `Models/LabBackup.cs` — كيان: Id, FileName, FilePath, Size, CreatedAt, CreatedBy
- `Migrations/20260704_AddLabBackup.cs` — إنشاء الجدول

**الاختبارات المتوقعة (3 اختبارات):**
- `LabBackup_CanBeInserted_WhenValid`
- `LabBackup_CanBeRetrievedById`
- `LabBackup_CanBeDeleted_WhenExists`

**Validation Gate:**
- [ ] عدد الاختبارات ≥ 3، كلها ✅
- [ ] Migration تُطبَّق بنجاح على InMemory
- [ ] `dotnet build` بدون warnings
```

### مثال 2: شريحة خدمة كاملة

```markdown
### الشريحة 2: خدمة BackupService

**Pre-condition:** الشريحة 1 ✅ — جدول `LabBackups` موجود.

**الملفات المتوقعة:**
- `Services/Interfaces/IBackupService.cs`
- `Services/Implementations/BackupService.cs`
- `Tests/Services/BackupServiceTests.cs` (8 اختبارات)
- `Tests/Services/BackupServiceRegistrationTests.cs` (2 اختبار)
- `Tests/Integration/BackupServiceIntegrationTests.cs` (4 اختبارات)

**الاختبارات المتوقعة (14 اختبار):**
| الفئة | العدد | أمثلة |
|-------|-------|--------|
| DI Registration | 2 | `IBackupService_IsRegisteredInDI`, `BackupService_Lifetime_IsScoped` |
| Authorization | 2 | `CreateBackupAsync_ThrowsUnauthorized_WhenNotAdmin` |
| Happy Path | 4 | `CreateBackupAsync_CreatesFile_WhenValid` |
| AES Encryption | 4 | `CreateBackupAsync_EncryptsFile_WithProvidedPassword` |
| Error Path | 2 | `RestoreBackupAsync_ReturnsFalse_WhenFileNotFound` |

**Validation Gate:**
- [ ] عدد الاختبارات ≥ 14، كلها ✅
- [ ] `dotnet build` بدون warnings
- [ ] `IBackupService` مسجَّل في `App.xaml.cs.ConfigureServices`
- [ ] الكود يستخدم `AesEncryptionHelper` (موجود مسبقًا) وليس مكتبة خارجية
- [ ] كل استدعاء لـ `BackupService` يُسجَّل عبر `_auditService.LogActionAsync(...)`
```

---

## قواعد ذهبية

1. **لا تضع Validation Gate بدون رقم قابل للقياس.** كل Gate إما عدد اختبارات، أو عدد warnings، أو عدد ملفات معدّلة.
2. **Pre-condition يجب أن يكون شيئًا محددًا.** "بعد التصميم" ليس Pre-condition.
3. **عدد الشرائح بين 3 و 8.** لو أكثر، قسّم المرحلة إلى مرحلتين (A, B).
4. **كل شريحة يجب أن تكون قابلة للتنفيذ في جلسة واحدة** (≤ 4 ساعات عمل للوكيل).
5. **Validation Gate للشرحة الأخيرة من المرحلة يجب أن يشمل معالم النجاح** من القسم 9.

---

## ما يجب تجنبه

| الخطأ | البديل |
|-------|--------|
| شريحة واحدة كبيرة بكل العمل | قسّمها لشرائح صغيرة |
| Validation Gate غامض ("الكود يبدو جيدًا") | Gate محدد ("8 اختبارات، كلها ✅") |
| عدم ذكر Pre-condition | أضف Pre-condition واضح لكل شريحة |
| نسيان ملفات الاختبارات | كل شريحة لها 3 ملفات اختبارات افتراضيًا |
| نسيان Migration | Migration إلزامي لكل تغيير على DB |
| نسيان تسجيل DI | كل خدمة جديدة مسجّلة في `App.xaml.cs` |
| نسيان تسجيل Navigation | كل نافذة جديدة مسجّلة في `App.OnStartup` |