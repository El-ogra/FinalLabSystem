---
name: handoff
description: Compact the current conversation into a handoff document for another agent to pick up. Tailored for FinalLabSystem — includes work plan progress, confirmed facts, and implementation state.
argument-hint: "What will the next session be used for?"
disable-model-invocation: true
---

# Handoff — FinalLabSystem

اكتب ملف Handoff يضغط المحادثة الحالية حتى يتمكن وكيل جديد من الاستكمال بدون إعادة تحليل أو إعادة فحص الملفات.

احفظ الملف في: `FinalLabSystem/FinalLabSystem/Docs/PRDs/Handoff-{اسم-الشريحة-أو-المهمة}.md`

---

## ما يجب تضمينه دائماً

### 1. الحالة الحالية للمشروع

```markdown
## حالة المشروع
- آخر migration مطبّقة: {اسم الـ migration}
- عدد الاختبارات الحالي: {N} / {N} ناجح
- Build: ✅ 0 errors, 0 warnings
- الفرع الحالي: before-prd
```

### 2. تقدم خطة العمل (إذا كنا في منتصف تنفيذ شرائح)

```markdown
## تقدم خطة العمل
- الشريحة 6.0: ✅ مكتملة (N اختبار)
- الشريحة 6.1: ✅ مكتملة (N اختبار)
- الشريحة 6.2: 🔄 في التنفيذ — أُنشئ Service وAES Helper، المتبقي: Tests
- الشريحة 6.3: ⏳ لم تبدأ
```

### 3. الحقائق المؤكدة (لا تُعاد إعادة فحصها)

```markdown
## حقائق مؤكدة — لا تُعد فحصها
- اسم جدول LabSetting في DB: "LabSettings" (جمع)
- توقيع Audit: LogActionAsync("Table", 0, "C"/"U"/"D", staffId, notes)
- Action column: HasMaxLength(1) — حرف واحد فقط
- {أي حقيقة أخرى تم التحقق منها}
```

### 4. القرارات المتخذة في هذه الجلسة

```markdown
## قرارات متخذة
- قررنا استخدام JSON بدلاً من T-SQL BACKUP — السبب: لا صلاحيات sysadmin
- قررنا IModel.GetEntityTypes() بدلاً من القراءة اليدوية — السبب: 44 جدول
- {أي قرار آخر}
```

### 5. الملفات التي تم لمسها في هذه الجلسة

```markdown
## الملفات المعدّلة في هذه الجلسة
- `Models/LabSetting.cs` — أُضيفت 8 حقول جديدة
- `Data/FinalLabDbContext.cs` — Fluent mappings للحقول الجديدة
- `Migrations/AddBackupFields.cs` — تم إنشاؤه وتطبيقه
```

### 6. المهمة التالية المباشرة

```markdown
## المهمة التالية
ابدأ من: `Tests/Services/BackupServiceTests.cs`
أنشئ 20 اختباراً بالفئات التالية:
- Category A: AES/Encryption (10 اختبارات)
- Category B: Migration Validation (4 اختبارات)
...
```

### 7. تعليمات صريحة لوكيل التنفيذ

```markdown
## تعليمات للوكيل التالي
هذا الملف نتاج تحليل مكتمل ونهائي.
**لا تُعد فحص أو قراءة الملفات المذكورة أعلاه** — كل الحقائق موثوقة هنا.
ابدأ التنفيذ مباشرة من "المهمة التالية".
```

---

## ما لا يجب تكراره

- لا تنسخ محتوى ملفات موجودة — أشر إليها بمسارها
- لا تكرر كود موجود في الملفات المذكورة — المسار يكفي
- لا تُضمّن PRDs أو Work Plans كاملة — أشر إليها بمسارها في `Docs/PRDs/`

---

## اقتراحات مهارات للجلسة التالية

أضف في نهاية الملف:

```markdown
## مهارات مقترحة للجلسة التالية
- `wpf-mvvm-conventions` — إذا كانت المهمة تشمل XAML أو ViewModel
- `ef-core-migration-safety` — إذا كانت المهمة تشمل migration
- `di-and-navigation-registration` — إذا كانت المهمة تشمل خدمة أو نافذة جديدة
- `csharp-testing` — إذا كانت المهمة تشمل كتابة اختبارات
```

