---
name: implement
description: "مهارة متخصصة في implement لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Implement — FinalLabSystem

> **الهدف:** تنفيذ شريحة عمل واحدة (Slice) من PRD أو Work Plan، بحيث تبقى الجودة قابلة للقياس بعد كل شريحة، ولا تنكسر الشرائح اللاحقة.

## متى تستدعى هذه المهارة

- المستخدم قال «نفّذ الشريحة رقم X من الخطة».
- المستخدم قال «نفذ المتطلبات المذكورة في هذا PRD».
- انتهت جلسة تخطيط عبر `lis-work-plan-with-validation-gates` وحان وقت البدء.
- المستخدم قال «continue implementation» بعد Handoff سابق.

## متى لا تستدعى

- لا توجد PRD ولا Work Plan → استخدم `brainstorming` أولاً ثم `to-prd`.
- المهمة تحليل PDF مرجعي → استخدم `lis-pdf-reference-extractor`.
- المهمة إصلاح خطأ صغير خارج أي خطة → نفّذ مباشرة بدون هذه المهارة.

---

## الخطوات الملموسة (Concrete Steps)

كل تنفيذ في FinalLabSystem يمر بهذه الخطوات **بهذا الترتيب**. لا تتخطَّ خطوة.

### 1. اقرأ الـ PRD

- افتح `FinalLabSystem/Docs/PRDs/<ModuleName>/PRD-<ModuleName>.md`.
- افهم النطاق (Scope) والاستثناءات (Out of Scope).
- تأكد من فهم قاموس المصطلحات — عند الشك ارجع لـ `medical-lab-domain-glossary`.

### 2. حدد الشريحة من الـ Work Plan

- افتح `FinalLabSystem/Docs/PRDs/<ModuleName>/WorkPlan-<ModuleName>.md`.
- اختر الشريحة التالية غير المكتملة.
- اقرأ:
  - **Pre-condition** — ما الذي يجب أن يكون جاهزاً قبل البدء.
  - **قائمة الملفات المتوقع تعديلها**.
  - **قائمة الاختبارات المتوقع إضافتها**.
  - **Validation Gate** — معيار النجاح القابل للقياس (مثلاً: `dotnet build` نظيف + 15 اختبار يمر + 0 failures).

### 3. اقرأ ملف Handoff السابق (إن وُجد)

- افتح آخر ملف تسليم في `FinalLabSystem/Docs/PRDs/<ModuleName>/Handoffs/`.
- ركّز على الأقسام:
  - «آخر ما تم إنجازه».
  - «القرارات التقنية المتخذة».
  - «الشيء التالي المتفق عليه».
  - «مخاطر معلقة».
- إذا لم يوجد Handoff (شريحة أولى) — تخطَّ هذه الخطوة.

### 4. نفّذ بأسلوب TDD

- ابدأ باختبار فاشل (Red).
- اكتب أقل كود ممكن لجعله ينجح (Green).
- ارجع لإعادة الهيكلة (Refactor).
- استخدم مهارة `tdd` كمرجع تفصيلي.
- **التزم بقواعد المشروع:**
  - ViewModels ترث `Infrastructure.ViewModelBase` (وليس `ObservableObject`) — راجع `wpf-mvvm-conventions`.
  - Commands من `Infrastructure.RelayCommand` / `AsyncRelayCommand`.
  - أي جدول جديد → Migration آمن حسب `ef-core-migration-safety`.
  - أي خدمة → تسجيل في `App.xaml.cs` حسب `di-and-navigation-registration`.
  - أي كيان جديد → قواعد الـ Audit حسب `audit-trail-conventions`.

**القيود الصارمة للاختبارات (من `lis-xunit-testing-conventions`):**

| ❌ ممنوع | ✅ البديل |
|---------|----------|
| `NSubstitute` | `Moq` |
| `FluentAssertions` | xUnit assertions العادية (`Assert.Equal`, `Assert.True`...) |
| `Testcontainers` | `EF Core InMemory` |
| `DateTime.Now` / `DateTime.Today` | `DateTime.UtcNow` فقط |
| `internal class` على خدمات/VMs | `public` فقط |
| نسيان `IAuditService.LogActionAsync` | كل INSERT/UPDATE/DELETE يُسجَّل في الـ Audit |

**قائمة الاختبارات المتوقعة حسب نوع الشريحة:**

| نوع الخدمة | العدد الأدنى |
|------------|--------------|
| CRUD بسيط | 5 اختبارات |
| CRUD مع منطق | 8 اختبارات |
| Workflow معقد | 12+ اختبار |

**ملفات الاختبارات الثلاثة الإلزامية لكل خدمة جديدة:**

| الملف | المسؤولية |
|-------|-----------|
| `Tests/Services/<Name>ServiceTests.cs` | اختبارات الوحدة (happy + error path) |
| `Tests/Services/<Name>ServiceRegistrationTests.cs` | اختبار تسجيل DI |
| `Tests/Integration/<Name>EndToEndTests.cs` | اختبار E2E شامل |

### 5. اختبر (Validation Gate)

```bash
# 1. بناء المشروع — يجب أن يكون نظيفاً
dotnet build FinalLabSystem.sln --nologo

# 2. تشغيل اختبارات الشريحة فقط أولاً
dotnet test --filter "FullyQualifiedName~<ServiceName>" --nologo

# 3. تشغيل كل الاختبارات (regression check)
dotnet test --nologo

# 4. إن كانت الشريحة تتضمن migration جديدة
dotnet ef migrations add <MigrationName> --project FinalLabSystem.csproj --startup-project FinalLabSystem.csproj
```

**جدول التحقق من Validation Gate:**

| البند | كيف تتحقق |
|-------|-----------|
| عدد الاختبارات ≥ N | `dotnet test --list-tests \| grep -c "Test "` |
| `dotnet build` بدون warnings | `dotnet build 2>&1 \| grep -i warning` (يجب أن يكون فارغاً) |
| Migration مُطبَّقة | `dotnet ef database update` على DB اختبارية |
| الخدمة مسجّلة في DI | `grep "AddScoped<I.*Service" App.xaml.cs` |
| لا ملفات `internal` | `grep -rn "internal class" Services/ ViewModels/ Views/` (يجب أن يكون فارغاً) |
| Audit calls موجودة | مراجعة الكود (كل write يستدعي `IAuditService.LogActionAsync`) |

- شغّل مهارة `qa` للفحص الوظيفي اليدوي عند الحاجة.
- إذا فشلت البوابة: **لا تنتقل للخطوة 6** — عد للخطوة 4 وأصلح.

### 6. أنشئ Handoff التالي

- استدعِ مهارة `lis-session-handoff`.
- احفظ في `FinalLabSystem/Docs/PRDs/<ModuleName>/Handoffs/handoff-<YYYY-MM-DD>-<slice-number>.md`.
- سجّل:
  - ما تم إنجازه في هذه الشريحة.
  - نتيجة Validation Gate (build + tests).
  - أي قرارات تقنية جديدة.
  - الشريحة التالية.
  - أي مخاطر أو أسئلة معلقة.

### 7. Commit

- Commit إلى الفرع الحالي برسالة واضحة تبدأ باسم الموديول والشريحة:
  ```
  <Module>: slice N — <عنوان الشريحة>
  ```
- إذا كان الفرع الحالي `main` أو `master`، أنشئ فرعاً جديداً `feature/<module>-slice-<N>` أولاً.
- راجع `git-guardrails-claude-code` قبل أي `git push`.

### 8. المراجعة النهائية

- استدعِ `/review` أو مهارة `review`.
- بعد الموافقة، عد للخطوة 2 لبدء الشريحة التالية.

---

## ملخص سريع بمخطط انسيابي

```
هل يوجد PRD؟ ──── لا ──→ توقف واسأل
        │
       نعم
        ↓
هل يوجد Work Plan؟ ── لا ──→ توقف واستدعِ lis-work-plan-with-validation-gates
        │
       نعم
        ↓
هل يوجد Handoff سابق؟ ── لا (شريحة أولى) ──→ ابدأ من الصفر
        │                                  │
      نعم                                  │
        ↓                                  ↓
    اقرأ Handoff ←─────────────────────────┘
        ↓
    اقرأ Work Plan → حدد الشريحة
        ↓
    نفّذ بـ TDD (🔴 → 🟢 → 🔵)
        ↓
    dotnet build + dotnet test
        ↓
    هل Validation Gates كلها ✅؟ ── لا ──→ أصلح
        │                                  │
       نعم                                 │
        ↓                                  ↓
    استدعِ lis-session-handoff للشريحة ←───┘
        ↓
    Commit على فرع feature → review
```

---

## قواعد ذهبية

- **شريحة واحدة في المرة.** لا تنفّذ شريحتين في نفس الجلسة إلا إذا كانت الشريحة الثانية "امتداداً ميكانيكياً" (مثل إضافة اختبار نسيته).
- **لا تنتقل للشريحة التالية قبل نجاح Validation Gate.** الفشل عند البوابة يعني أن الشريحة ليست جاهزة.
- **كل شريحة تنتهي بـ Handoff.** حتى لو أكملت 3 شرائح في يوم واحد، اكتب 3 ملفات Handoff.
- **لا تخترع سلوكاً جديداً.** إذا اكتشفت متطلباً غير موجود في PRD، أوقف التنفيذ واسأل المستخدم أو حدّث الـ PRD أولاً.

---

## المهارات المرتبطة

- `lis-work-plan-with-validation-gates` — لإنشاء أو تحديث Work Plan.
- `lis-session-handoff` — لصياغة ملف Handoff بالقالب الصحيح.
- `tdd` — لتفاصيل دورة Red-Green-Refactor.
- `wpf-mvvm-conventions` — لقواعد MVVM الملزمة.
- `ef-core-migration-safety` — لأي تعديل على DB.
- `di-and-navigation-registration` — لتسجيل خدمات ونوافذ جديدة.
- `lis-xunit-testing-conventions` — لكتابة الاختبارات.
- `review` — للمراجعة بعد التنفيذ.

---

## Anti-patterns (ممنوعة)

| الممارسة الممنوعة | البديل الصحيح |
|-------------------|----------------|
| تنفيذ شريحة كاملة قبل قراءة PRD أو Work Plan | ابدأ دائماً بالخطوة 1 |
| تخطي كتابة الاختبار أولاً (No-TDD) | التزم بدورة TDD ما لم يُعفَ صراحة |
| Commit مباشر لـ `main` | استخدم فرع feature لكل شريحة |
| نسيان Handoff بحجة "أنا مستمر" | كل شريحة → Handoff، بلا استثناء |
| استخدام `ObservableObject` أو `[ObservableProperty]` | استخدم `ViewModelBase` المخصص |
| كتابة كود جديد بدون تسجيل في DI | راجع `di-and-navigation-registration` |
