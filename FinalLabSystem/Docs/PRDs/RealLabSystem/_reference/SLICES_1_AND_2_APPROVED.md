# وثيقة التنفيذ المعتمدة — الشريحتان 1 و 2 من GAP_ANALYSIS

**تاريخ التحقق:** 2026-07-04
**الكوميت الذي جرى التحقق منه:** `c8d40f0a8e1636638275669023ed7cbd7cc7f5b0` (الفرع: `before-prd`)
**المستودع:** `El-ogra/FinalLabSystem`
**المرجع الأساسي:** `Docs/PRDs/RealLabSystem/_reference/Real_Lab_System_Unified_Reference.md`
**الوثيقة قيد التحقق:** `Docs/PRDs/RealLabSystem/_reference/GAP_ANALYSIS.md`

---

## 0. ملخّص التحقق (الخطوتان 1 و 2)

### 0.1 نتيجة الخطوة 1 — التحقق من ترتيب الأولوية

**النتيجة: ترتيب الأولوية في GAP_ANALYSIS صحيح.** لا حاجة لاستبدال الشريحتين الأوليَتَين بشريحتين أخريَتَين.

**السبب المنطقي (مبني على قراءة الكود):**
- **Slice 1 (الباركود ثلاثي المستويات) هي فعلاً Foundational:** الكود الحالي في `Services/Implementations/SampleTrackingService.cs` (السطر 59) يُنتج كودًا واحدًا فقط بصيغة `{PatientCode}-{ordinal:D2}`، بينما المرجع (`BR-BC-001/002/003`) يفرض ثلاثة أكواد منفصلة (Case=1، File=3، Lab=5) بطول 13 خانة، مع Lab ID دائم. هذه الفجوة تمس **كل زيارة لكل مريض** وسير عمل التسليم (S-10 يعتمد على "قراءة كود الإيصال ثم كود الملف") وسير عمل البحث (S-11.2 يطلب حقلاً مستقلاً لكل نوع كود). أيّ شريحة أخرى تفترض ضمنيًا وجود هذه الهوية.
- **Slice 2 (نافذة إدخال المزرعة) حرجة سريريًا:** الخدمة `CultureResultService` والموديلات (`MicrobiologyCulture`, `MicrobiologyOrganism`, `OrganismAntibiotic`, `AntibioticCatalog`) موجودة فعلاً، لكن **لا نافذة WPF ولا ViewModel** لإدخال النتائج. النتيجة العملية: قسم الميكروبيولوجي معطَّل كليًا في الواجهة رغم جاهزية الطبقة الخلفية.
- **مقارنة بالشرائح الأخرى:**
  - Slice 3 (إدارة المستخدمين): الخدمات (`AuthService`, `Permission`, `StaffPermission`) موجودة ويمكن العمل مؤقتًا بـ `admin` — ليست Blocker.
  - Slice 5 (لوحة الإحصائيات): تقارير — مفقودة لكن لا تعطّل التشغيل اليومي.
  - Slice 6 (رموز الحالة السبعة): تحسين UX فقط، ليس Blocker.

**الاستنتاج:** الشريحتان الأصليتان (Slice 1 و Slice 2) هما فعلاً الأَولى بالتنفيذ، ولا توجد فجوة أخرى (مذكورة أو غير مذكورة) تسبقهما منطقيًا.

### 0.2 نتيجة الخطوة 2 — التحقق التفصيلي من دقة المحتوى

بعد قراءة الكود الفعلي في هذا الكوميت:

#### Slice 1 — دقيقة في جوهرها، مع ملاحظات

| الادعاء في GAP_ANALYSIS | حالة التحقق | التفصيل |
|---|---|---|
| `SampleTrackingService.GenerateBarcodesForVisitAsync` ينتج صيغة `{PatientCode}-{ordinal:D2}` | ✅ دقيق | `SampleTrackingService.cs:59` — مطابق حرفيًا |
| لا وجود لموديل `PatientBarcode` | ✅ دقيق | لا ملف باسمه في `Models/` |
| لا وجود لحقل `Patient.LabId` | ✅ دقيق | `Models/Patient.cs` (74 سطرًا) لا يحوي `LabId` |
| لا `BarcodeGenerator` service ولا `IBarcodeGenerator` | ✅ دقيق | لا ملفين في `Services/Interfaces` و `Services/Implementations` |
| لا `BranchNumber` في `LabSetting` | ✅ دقيق | grep على `Models/LabSetting.cs` بلا نتائج |
| `BarcodeDialogViewModel.cs` و `BarcodeDialog.xaml` موجودان (يحتاجان تحديث) | ✅ دقيق | 166 سطر VM، 55 سطر XAML |
| `PatientRegistrationViewModel.cs` موجود (يحتاج زر `PrintLabIdCommand`) | ✅ دقيق | الملف موجود بالكامل |
| `Data/FinalLabDbContext.cs` يحتاج DbSet + Fluent API جديد | ✅ دقيق | مفعّل حاليًا للجداول الموجودة فقط |
| بنية الكود المقترحة: `1-{branch}-{ddmmyy}-{weekday}-{ordinal:D3}-{caseCode}` | ⚠️ ملاحظة | المرجع نفسه (السطر 149 من `Real_Lab_System_Unified_Reference.md`) فيه **تباس داخلي** بين المثال الحرفي `1-4-100623-1-006-8` والوصف اللفظي "لا يُنظَر — ترتيب الدخول — رقم الفرع — تاريخ اليوم — يوم الأسبوع — كود الحالة". الترتيب النصي لا يطابق المثال ترتيبيًا. لذلك اقتراح GAP_ANALYSIS للبنية هو **اجتهاد معقول** لكن **يجب اعتماده صراحة معك** قبل التنفيذ. أضفت ذلك كنقطة قرار في الملف المعتمد أدناه. |
| Migration واحدة اسمها `AddPatientBarcodeAndLabId` | ⚠️ اسم مقترح دقيق منطقيًا | لا اعتراض. |
| 8 اختبارات مقترحة | ✅ منطقية ومغطية | لا اعتراض. |

#### Slice 2 — دقيقة في معظمها، مع نقص جوهري واحد

| الادعاء في GAP_ANALYSIS | حالة التحقق | التفصيل |
|---|---|---|
| `MicrobiologyCulture` بـ 11 حقلًا | ✅ دقيق | إحصاء فعلي (بدون Navigation): 11 خاصية (`CultureId, VisitTestId, SpecimenSource, SpecimenVolumeMl, ReceivedAt, InoculatedBy, CultureResult, IncubationHours, FinalReadingAt, ReadBy, FinalComment`) |
| `MicrobiologyOrganism` بحقول `ColonyCount`, `GramStain` | ✅ دقيق | كلاهما موجود |
| `OrganismAntibiotic.Sensitivity` نصي مفتوح | ✅ دقيق | `Sensitivity` من نوع `string` غير مقيد |
| `AntibioticCatalog` بـ 6 حقول + IsSafePregnancy/IsSafeChildren | ✅ دقيق | تحقق مباشر من الملف |
| `CultureResultService.GetSafeAntibioticsAsync(isPregnant, isChild)` موجود | ✅ دقيق | السطر 23 من الخدمة |
| لا `AntibioticSensitivity` enum | ✅ دقيق | غير موجود في `Models/Enums/` |
| لا `CultureEntryViewModel` ولا `CultureEntryWindow.xaml` ولا `CultureReportTemplate.cs` | ✅ دقيق | تحقق بحث الملفات |
| `Infrastructure/Navigation/NavigationService.cs` موجود لتسجيل النافذة | ✅ دقيق | الملف موجود |
| نطاق النافذة يحمل حقولاً: `Sample Source, Colony Count, Culture Condition, Incubation Hours` | ⚠️ **نقص جوهري** | حقلا `Culture Condition` و `Colony Count` (على مستوى المزرعة نفسها لا الكائن) **غير موجودَين** كأعمدة على `MicrobiologyCulture`. المرجع (السطر 306) يذكرهما صراحةً كحقول للنافذة. لذلك **يجب إضافة Migration مستقلة** تضيف عمودَي `CultureCondition` و `ColonyCount` (على مستوى العينة الكلية) على `MicrobiologyCulture`. GAP_ANALYSIS لم توضح ذلك في قسم Migrations المطلوبة، وقد ذكرت فقط `AddCultureSensitivityEnum + AddCultureTypeCatalog (اختياري)`. |
| تحويل `OrganismAntibiotic.Sensitivity` من string إلى enum | ⚠️ صحيح مع تحفّظ | ذُكرت المخاطرة في القسم 6 لكن الحل غير مُثبَت. أضفت خطة data migration واضحة في الملف المعتمد أدناه. |
| 10 اختبارات مقترحة | ✅ منطقية | لا اعتراض. |

#### ملاحظات جانبية (خارج نطاق الشريحتين لكن تستحق الإشارة)
أرقام الملخص التنفيذي في GAP_ANALYSIS (955 ملف كود، 79 موديل، 58 Migration) لم تطابق الإحصاء الفعلي (499 ملف كود بمجموع المشروعين، 53 موديل في مجلد `Models/`، 33 Migration فعلية). هذه أرقام سياقية لا تؤثر على صحة تحليل الشريحتين، لكنها تُلمّح إلى أن الوكيل السابق قد يكون قدَّر الأرقام تقديرًا لا إحصاءً. **لن يؤثر ذلك على قرار التنفيذ.**

---

## 1. الملف المعتمد للتنفيذ — Slice 1 & Slice 2

> هذا هو الملف الرسمي المعتمد للتنفيذ. مبنيّ على قراءة الكود الفعلي في الكوميت `c8d40f0`، ومصحَّح ومحسَّن مقارنة بالنسخة الأصلية في `GAP_ANALYSIS.md`.

### ▸ Slice 1 — الباركود ثلاثي المستويات + Lab ID الدائم

**المعرّف:** S-BC-01
**الأولوية:** 🔴 حرجة (Foundational)
**الجهد المقدَّر:** 3 – 4 أيام عمل فعلي
**الأثر:** هوية النظام كلها؛ يعتمد عليه سير عمل الاستقبال، البحث، التسليم، والتقارير.
**القواعد المرجعية:** `BR-BC-001` / `BR-BC-002` / `BR-BC-003` (المرجع الفصل 5.3)

#### 1.1 وصف الفجوة (بناءً على الكود الفعلي)

في الكوميت الحالي:
- `Services/Implementations/SampleTrackingService.cs:59` ينتج **كودًا واحدًا فقط** بصيغة `$"{patientCode}-{ordinal:D2}"` (مثال: `P0001-01`, `P0001-02` …).
- `Models/Patient.cs` لا يحوي أي حقل `LabId`.
- `Models/SampleTube.cs` لا يحوي `CodeType` (يوجد `TubeType` فقط الذي يشير لنوع الأنبوبة لا لنوع الكود).
- `Models/LabSetting.cs` لا يحوي `BranchNumber`.
- لا يوجد `Models/PatientBarcode.cs` ولا `IBarcodeGenerator` ولا `BarcodeGenerator`.

الفجوة كاملة مقابل المرجع الذي يفرض:
1. **ثلاثة أنواع أكواد لكل مريض** — Case Code (يبدأ بـ 1)، File Code (يبدأ بـ 3)، Lab ID (يبدأ بـ 5).
2. **كل كود بطول 13 خانة رقمية** (بنية `1-4-100623-1-006-8` وفق المثال في المرجع — انظر نقطة القرار أدناه).
3. **Lab ID دائم** يُنشأ عند أول زيارة ولا يتغيّر عبر الزيارات اللاحقة، ويُعاد طبعه من نافذة البحث عند فقدان البطاقة.
4. **زر مستقل لطباعة Lab ID فقط** في نافذة تسجيل المريض.

#### 1.2 نقطة قرار مطلوبة قبل بدء التنفيذ

المرجع `Real_Lab_System_Unified_Reference.md:149` فيه تباس داخلي بين:
- **المثال الحرفي:** `1-4-100623-1-006-8`
- **الوصف اللفظي:** "رقم لا يُنظَر إليه — ترتيب دخول المريض في يوم الشغل — رقم الفرع — تاريخ اليوم من اليمين — رقم يوم الأسبوع — كود الحالة"

عند تطبيق الوصف اللفظي حرفيًا على المثال، الترتيب لا يستقيم (مثلاً `100623` يفترض أنه رقم الفرع بينما هو تاريخ). لذلك المطلوب منك اختيار واحدة من صيغتين:

- **الخيار A (المقترح من GAP_ANALYSIS، مطابق للوصف اللفظي):**
  `{type:1}-{branch:1}-{ddmmyy:6}-{weekday:1}-{ordinal:D3}-{caseCode:1}`
  مثال: `1-4-100723-6-006-8`

- **الخيار B (مطابق للمثال الحرفي):**
  `{type:1}-{ordinal:1}-{ddmmyy:6}-{weekday:1}-{branch:3}-{caseCode:1}`
  مثال: `1-4-100623-1-006-8`

**قبل بدء تنفيذ Slice 1، يجب أن تختار الخيار A أو B وتوثّق قرارك.** أي وكيل ينفّذ الشريحة لاحقًا يجب أن يرى هذا القرار مثبَّتًا.

#### 1.3 النطاق التفصيلي

**A) الموديلات والبيانات:**
- إضافة موديل جديد `Models/PatientBarcode.cs` بحقول:
  - `PatientBarcodeId` (PK, int)
  - `PatientId` (FK → Patient)
  - `VisitId` (FK → Visit, nullable — Lab ID لا يرتبط بزيارة محدّدة)
  - `CodeType` (enum جديد: `Case`, `File`, `Lab`)
  - `BarcodeValue` (string, length=13, unique index)
  - `IssueDate` (DateTime)
  - `SortOrdinal` (int — ترتيب دخول اليوم)
  - `BranchNumber` (byte — نسخة مأخوذة وقت الإصدار)
  - `CreatedBy` (FK → Staff, nullable)
- إضافة enum `Models/Enums/BarcodeCodeType.cs` بقيم: `Case=1, File=3, Lab=5`.
- إضافة حقل `LabId` (string, length=13, nullable, unique) على `Models/Patient.cs`.
- إضافة حقل `BranchNumber` (byte, default=1) على `Models/LabSetting.cs`.

**B) الخدمات:**
- إنشاء `Services/Interfaces/IBarcodeGenerator.cs` بواجهات:
  - `Task<string> GenerateCaseCodeAsync(int visitId)`
  - `Task<string> GenerateFileCodeAsync(int visitId)`
  - `Task<string> GetOrCreateLabIdAsync(int patientId)`
- إنشاء `Services/Implementations/BarcodeGenerator.cs` تنفّذ المنطق حسب الخيار A أو B المعتمد.
- **إعادة كتابة** `SampleTrackingService.GenerateBarcodesForVisitAsync`:
  - قبل توليد `SampleTube`s، ينادي `IBarcodeGenerator` لتوليد الكودَين المطلوبَين للزيارة (Case + File) وضمان وجود Lab ID للمريض.
  - يخزّن الأكواد في جدول `PatientBarcode` الجديد.
  - يُبقي منطق `SampleTube` الحالي كما هو (لأن `SampleTube.BarcodeValue` يُستخدم على مستوى الأنبوبة الفردية، لا على مستوى المريض/الزيارة/المعمل).
  - **قرار معماري:** هل نستخدم `PatientBarcode.BarcodeValue` كمصدر وحيد ونمرّره لـ `SampleTube` (نستبدل الصيغة القديمة `{PatientCode}-{ordinal}` بصيغة تعتمد على Case Code)، أم نُبقي `SampleTube.BarcodeValue` بصيغته الحالية للاستخدام الداخلي وننشئ ملصقات منفصلة للأكواد الثلاثة؟ **الاختيار المقترح:** الإبقاء (لأن الأنابيب متعددة والكود الثلاثي على مستوى الملف/الحالة).

**C) واجهات المستخدم:**
- **تحديث** `ViewModels/Patients/BarcodeDialogViewModel.cs`:
  - عرض ثلاث مجموعات ملصقات: Case Labels (بادئة 1)، File Labels (بادئة 3)، Lab ID Label (بادئة 5 — ملصق واحد فقط).
  - أوامر منفصلة: `PrintCaseLabelsCommand`, `PrintFileLabelsCommand`, `PrintLabIdCommand`, `PrintAllCommand`.
- **تحديث** `Views/Patients/BarcodeDialog.xaml`: 3 أقسام مرئية (Tabs أو Expanders) لكل نوع كود.
- **تحديث** `ViewModels/Patients/PatientRegistrationViewModel.cs`: زر مستقل جديد `PrintLabIdCommand` مربوط بـ `IBarcodeGenerator.GetOrCreateLabIdAsync` + `WpfLabelPrintService`. الزر مفعَّل فقط بعد حفظ المريض (بمعنى: بعد أن يحصل على `PatientId`).
- **تحديث** `Views/Patients/PatientRegistrationWindow.xaml`: إضافة زر Lab ID بجوار زر الباركود الحالي.

**D) ربط قاعدة البيانات:**
- **تحديث** `Data/FinalLabDbContext.cs`:
  - إضافة `DbSet<PatientBarcode> PatientBarcodes`.
  - إضافة Fluent API لجدول `PatientBarcode` (Unique constraint على `BarcodeValue`، فهرس على `PatientId + CodeType`).
  - إضافة تحويل enum → int لعمود `CodeType`.

#### 1.4 الملفات المتأثرة (قائمة نهائية موثَّقة من قراءة المستودع)

| الملف | الحالة | الفعل |
|---|---|---|
| `FinalLabSystem/Models/PatientBarcode.cs` | 🆕 جديد | إنشاء |
| `FinalLabSystem/Models/Enums/BarcodeCodeType.cs` | 🆕 جديد | إنشاء |
| `FinalLabSystem/Models/Patient.cs` | ✏️ تعديل | إضافة `LabId` |
| `FinalLabSystem/Models/LabSetting.cs` | ✏️ تعديل | إضافة `BranchNumber` |
| `FinalLabSystem/Services/Interfaces/IBarcodeGenerator.cs` | 🆕 جديد | إنشاء |
| `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs` | 🆕 جديد | إنشاء |
| `FinalLabSystem/Services/Implementations/SampleTrackingService.cs` | ✏️ تعديل جوهري | إعادة كتابة `GenerateBarcodesForVisitAsync` (استدعاء `IBarcodeGenerator`) |
| `FinalLabSystem/ViewModels/Patients/BarcodeDialogViewModel.cs` | ✏️ تعديل جوهري | 3 أوامر طباعة منفصلة + عرض مجموعات |
| `FinalLabSystem/Views/Patients/BarcodeDialog.xaml` | ✏️ تعديل | 3 أقسام Tabs/Expanders |
| `FinalLabSystem/ViewModels/Patients/PatientRegistrationViewModel.cs` | ✏️ تعديل | إضافة `PrintLabIdCommand` |
| `FinalLabSystem/Views/Patients/PatientRegistrationWindow.xaml` | ✏️ تعديل | إضافة زر Lab ID |
| `FinalLabSystem/Data/FinalLabDbContext.cs` | ✏️ تعديل | DbSet + Fluent API |
| `FinalLabSystem/App.xaml.cs` (DI) | ✏️ تعديل | تسجيل `IBarcodeGenerator` → `BarcodeGenerator` |

#### 1.5 Migrations المطلوبة

**Migration واحدة** باسم `AddPatientBarcodeAndLabId`:
- CREATE TABLE `PatientBarcode` (كل الأعمدة أعلاه + FK constraints + unique index على `BarcodeValue` + composite index على `(PatientId, CodeType)`).
- ALTER TABLE `Patient` ADD COLUMN `LabId nvarchar(13) NULL` + unique filtered index (حيث `LabId IS NOT NULL`).
- ALTER TABLE `LabSetting` ADD COLUMN `BranchNumber tinyint NOT NULL DEFAULT 1`.

#### 1.6 الاختبارات المقترحة (8)

**Unit — `BarcodeGenerator`:**
1. `GenerateCaseCodeAsync_ReturnsValidFormat` — يتأكد من طول 13 خانة والبادئة `1`.
2. `GenerateFileCodeAsync_ReturnsValidFormat` — طول 13 خانة والبادئة `3`.
3. `GetOrCreateLabIdAsync_NewPatient_CreatesAndPersistsLabId` — أول استدعاء يحفظ Lab ID.
4. `GetOrCreateLabIdAsync_ExistingLabId_ReturnsSameValue` — الاستدعاء الثاني لنفس المريض يُرجع نفس القيمة (لا يُنشئ جديد).

**Integration — `SampleTrackingService`:**
5. `GenerateBarcodesForVisit_FirstVisit_CreatesAll3Codes` — زيارة أولى تُنتج Case + File + Lab ID.
6. `GenerateBarcodesForVisit_SecondVisit_ReusesLabIdCreatesNewCaseFile` — Lab ID القديم يُقرأ من DB، Case و File جديدَان.

**ViewModel:**
7. `BarcodeDialog_LabIdSection_ShowsOnlyOneLabel` — لا يظهر أكثر من ملصق Lab ID واحد.
8. `PatientRegistration_PrintLabIdCommand_EnabledOnlyAfterSave` — الزر معطَّل قبل حفظ المريض، مفعَّل بعده.

#### 1.7 معايير القبول

1. مريض جديد يُنتج له عند أول زيارة: Case Code (يبدأ بـ 1، 13 خانة) + File Code (يبدأ بـ 3، 13 خانة) + Lab ID (يبدأ بـ 5، 13 خانة) — الثلاثة محفوظة في جدول `PatientBarcode`.
2. زيارة ثانية لنفس المريض تُنتج Case + File جديدَين، بينما يُقرأ Lab ID القديم من `Patient.LabId` دون تعديل.
3. زر مستقل لطباعة Lab ID موجود ويعمل من `PatientRegistrationWindow`، ولا يحتاج إعادة إنشاء الكود إذا كان موجودًا.
4. جميع الاختبارات الثمانية تجتاز محليًا.
5. Migration `AddPatientBarcodeAndLabId` تُطبَّق على قاعدة موجودة دون فقدان بيانات.

#### 1.8 مخاطر واعتبارات

- **الزيارات القديمة قبل هذه الشريحة** لن يكون لها Lab ID تلقائيًا. **قرار معتمد:** لا نُهاجر الأكواد القديمة؛ كل زيارة جديدة بعد التطبيق تحصل على الصيغة الجديدة. Lab ID للمرضى الحاليين يُنشأ عند أول زيارة تالية.
- **صيغة الباركود:** انظر نقطة القرار 1.2 — يجب حسمها قبل بدء التنفيذ.
- **الفهرس الفريد على `Patient.LabId`:** يجب أن يكون Filtered Index (يستثني NULLs) لتجنّب فشل الترحيل على البيانات القائمة.

---

### ▸ Slice 2 — نافذة إدخال المزرعة والحساسية + تصنيف Enum

**المعرّف:** S-CULT-01
**الأولوية:** 🔴 حرجة (سريريًا)
**الجهد المقدَّر:** 4 – 5 أيام عمل فعلي
**الأثر:** يفكّ حصار قسم الميكروبيولوجي كاملًا — الطبقة الخلفية جاهزة والواجهة مفقودة.
**القواعد المرجعية:** `BR-CULT-001` / `BR-CULT-002` (المرجع الفصل 5)

#### 2.1 وصف الفجوة (بناءً على الكود الفعلي)

في الكوميت الحالي:
- **موجود:** موديلات كاملة (`MicrobiologyCulture` بـ 11 حقل، `MicrobiologyOrganism` بـ 7 حقول أصلية، `OrganismAntibiotic` بـ 9 حقول، `AntibioticCatalog` بـ 6 حقول)، وخدمة `CultureResultService` (3 دوال: `GetSafeAntibioticsAsync`, `SaveCultureAsync`, `AddOrganismsAndSensitivitiesAsync`)، وربط DbContext كامل، و FK إلى `AntibioticCatalog`.
- **مفقود:**
  - لا واجهة WPF (`Views/Patients/CultureEntryWindow.xaml`).
  - لا `ViewModels/Patients/CultureEntryViewModel.cs`.
  - لا `Services/Printing/CultureReportTemplate.cs`.
  - لا `Models/Enums/AntibioticSensitivity.cs`.
  - `OrganismAntibiotic.Sensitivity` من نوع `string` غير مقيَّد (يمكن كتابة أي قيمة، مما يخالف `BR-CULT-001` الذي يفرض التصنيف الرباعي Highly / Moderate / Low / Resistant).
  - **نقص لم يذكره GAP_ANALYSIS بوضوح:** المرجع (السطر 306) يذكر أن نافذة المزرعة تحتوي حقلي `Culture Condition` و `Colony Count` على مستوى العينة (لا الكائن الفردي)، لكن `MicrobiologyCulture` **لا يحوي هذين العمودين** حاليًا. `ColonyCount` موجود فقط على `MicrobiologyOrganism` (وهو ملائم لكل كائن حي على حدة، لكن ليس هو المطلوب في المرجع).

#### 2.2 النطاق التفصيلي

**A) توسيع الموديلات:**
- إضافة enum `Models/Enums/AntibioticSensitivity.cs`:
  ```csharp
  public enum AntibioticSensitivity : byte
  {
      Highly = 0,
      Moderate = 1,
      Low = 2,
      Resistant = 3
  }
  ```
- **تعديل** `Models/OrganismAntibiotic.cs`: تغيير `public string Sensitivity` إلى `public AntibioticSensitivity Sensitivity`.
- **تعديل** `Models/MicrobiologyCulture.cs`: إضافة عمودَين جديدَين مطابقَين للمرجع:
  - `public string? CultureCondition { get; set; }` (مثال: "Aerobic, 37°C, 24h")
  - `public string? ColonyCount { get; set; }` (على مستوى العينة الكلية — Semi-quantitative مثل "10^5 CFU/mL")

**B) الخدمة:**
- **تعزيز** `CultureResultService.cs`:
  - `Task<MicrobiologyCulture> GetByVisitTestIdAsync(int visitTestId)` — لتحميل نتيجة موجودة عند إعادة الفتح.
  - `Task UpdateSensitivityAsync(int antibioticResultId, AntibioticSensitivity value)` — تحديث دقيق دون إعادة كتابة الكائن كاملًا.
  - **حفظ Culture + Organisms + Antibiotics في transaction واحد** (حاليًا `AddOrganismsAndSensitivitiesAsync` منفصلة عن `SaveCultureAsync` مما يفتح الباب لحالة عدم اتساق).

**C) الواجهة الجديدة — `CultureEntryWindow.xaml`:**
- تُفتح من `ResultEntryWindow` عند التحليل من نوع Culture.
- **قسم علوي (Sample-level):** SpecimenSource, CultureCondition, ColonyCount, IncubationHours, ReceivedAt, FinalReadingAt, FinalComment.
- **قسم أوسط (Organisms):** ItemsControl يعرض 0 إلى 3 كائنات، كل صف: OrganismName, GramStain, ColonyCount, Morphology + زر حذف. زر "إضافة كائن" (مقيَّد بـ ≤3).
- **قسم سفلي (Sensitivity Grid):** DataGrid لكل كائن على حدة:
  - عمود Antibiotic Name (من `AntibioticCatalog` مفلترًا حسب حالة المريض).
  - 4 أعمدة Radio (Highly / Moderate / Low / Resistant) — كل صف يسمح باختيار واحد فقط (تحقّق UI).
- **الفلترة التلقائية:** عند فتح النافذة، يتم استدعاء `GetSafeAntibioticsAsync(visit.IsPregnant, patient.AgeInYears < 12)` وحقن النتيجة في DataGrid.

**D) ViewModel — `CultureEntryViewModel.cs`:**
- خصائص Observable لكل الحقول العلوية.
- `ObservableCollection<OrganismInputVm> Organisms` (max 3).
- `ObservableCollection<AntibioticRow> Antibiotics` لكل كائن — مربوطة بجدول الحساسية.
- Commands: `LoadCommand(int visitTestId)`, `AddOrganismCommand`, `RemoveOrganismCommand`, `SaveCommand`, `ResetCommand`.
- Validation:
  - على الأقل كائن واحد قبل الحفظ (أو `CultureResult = "No Growth"`).
  - كل صف حساسية يجب أن يكون له مستوى واحد.
- **يلتزم بقاعدتك المعمارية:** منطق الأعمال في ViewModel مباشرة (لا Domain Service إضافي).

**E) قالب التقرير:**
- إنشاء `Services/Printing/CultureReportTemplate.cs`:
  - Header: بيانات المريض + Sample + CultureCondition + ColonyCount.
  - Body: لكل كائن: OrganismName + GramStain + جدول الحساسية بأربع مجموعات (Highly for / Moderate for / Low for / Resistant for) بحسب `BR-CULT-001`.
  - Footer: FinalComment + توقيع الفني المسؤول.

**F) Navigation:**
- **تعديل** `Infrastructure/Navigation/NavigationService.cs`: تسجيل النافذة `CultureEntryWindow` لتُفتح عبر `NavigateToCultureEntryAsync(int visitTestId)`.
- **ربط في** `ResultEntryViewModel`: عند اختيار تحليل من فئة Culture، فتح `CultureEntryWindow` بدل نافذة الإدخال العددية الافتراضية.

#### 2.3 الملفات المتأثرة (قائمة نهائية)

| الملف | الحالة | الفعل |
|---|---|---|
| `FinalLabSystem/Models/Enums/AntibioticSensitivity.cs` | 🆕 جديد | إنشاء enum |
| `FinalLabSystem/Models/OrganismAntibiotic.cs` | ✏️ تعديل | تحويل `Sensitivity` من `string` إلى `AntibioticSensitivity` |
| `FinalLabSystem/Models/MicrobiologyCulture.cs` | ✏️ تعديل | إضافة `CultureCondition`, `ColonyCount` |
| `FinalLabSystem/Services/Interfaces/ICultureResultService.cs` | ✏️ تعديل | إضافة `GetByVisitTestIdAsync`, `UpdateSensitivityAsync`, `SaveFullCultureAsync` |
| `FinalLabSystem/Services/Implementations/CultureResultService.cs` | ✏️ تعديل | تنفيذ الدوال الجديدة + transaction |
| `FinalLabSystem/ViewModels/Patients/CultureEntryViewModel.cs` | 🆕 جديد | إنشاء |
| `FinalLabSystem/Views/Patients/CultureEntryWindow.xaml` (+ `.xaml.cs`) | 🆕 جديد | إنشاء |
| `FinalLabSystem/Services/Printing/CultureReportTemplate.cs` | 🆕 جديد | إنشاء قالب |
| `FinalLabSystem/Infrastructure/Navigation/NavigationService.cs` | ✏️ تعديل | تسجيل النافذة |
| `FinalLabSystem/ViewModels/Patients/ResultEntryViewModel.cs` | ✏️ تعديل | توجيه Culture إلى `CultureEntryWindow` |
| `FinalLabSystem/Data/FinalLabDbContext.cs` | ✏️ تعديل | Fluent API: تحويل enum → byte، إضافة الأعمدة الجديدة |
| `FinalLabSystem/App.xaml.cs` (DI) | ✏️ تعديل | تسجيل ViewModel + Template إن لزم |

#### 2.4 Migrations المطلوبة (مصحَّحة — نقص جوهري في GAP_ANALYSIS)

**Migration 1 — `AddCultureFieldsAndSensitivityEnum`:**
- ALTER TABLE `MicrobiologyCulture`:
  - ADD `CultureCondition nvarchar(200) NULL`
  - ADD `ColonyCount nvarchar(50) NULL`
- ALTER TABLE `OrganismAntibiotic`:
  - إضافة عمود مؤقت `Sensitivity_New tinyint NOT NULL DEFAULT 3` (Resistant كأقل ضرر افتراضًا).
  - **Data migration UPDATE:** تعبئة `Sensitivity_New` من `Sensitivity` القديم بمطابقة نصية:
    - `'Highly'` أو `'Sensitive'` أو `'S'` → 0
    - `'Moderate'` أو `'Intermediate'` أو `'I'` → 1
    - `'Low'` → 2
    - `'Resistant'` أو `'R'` أو غيرها → 3
  - DROP COLUMN `Sensitivity`.
  - RENAME `Sensitivity_New` → `Sensitivity`.
- **ملاحظة إلزامية:** يجب تدقيق البيانات القائمة في `OrganismAntibiotic.Sensitivity` قبل تطبيق الترحيل للتأكد من عدم فقدان قيم فريدة. إن كان الجدول فارغًا (أو يحوي بيانات test فقط)، يمكن تبسيط الترحيل بـ DROP + ADD مباشرة.

**Migration 2 (اختياري) — `AddCultureTypeCatalog`:**
- خارج نطاق الشريحة الحرجة. يمكن تأجيله إلى Slice 10 (Sweep). لا يمنع اكتمال Slice 2.

#### 2.5 الاختبارات المقترحة (10)

**Unit — `CultureResultService`:**
1. `GetSafeAntibioticsAsync_Pregnant_FiltersOutUnsafe`
2. `GetSafeAntibioticsAsync_Child_FiltersOutUnsafe`
3. `GetSafeAntibioticsAsync_PregnantAndChild_AppliesBothFilters`

**ViewModel — `CultureEntryViewModel`:**
4. `AddOrganism_LimitedTo3` — إضافة رابع تفشل.
5. `SaveCulture_WithNoOrganism_RequiresCultureResultText` — منع الحفظ الفارغ.
6. `LoadingPregnantPatient_TriggersAntibioticFilter` — عند فتح مريضة حامل، القائمة تُفلتر تلقائيًا.

**Integration:**
7. `FullCultureWorkflow_SaveAndReload_Roundtrip` — كتابة Culture + 3 Organisms + 20 Antibiotics ثم قراءتها.
8. `DataMigration_SensitivityStringToEnum_MapsCorrectly` — اختبار Migration على بيانات نصية موجودة.

**Report:**
9. `CultureReportTemplate_PregnantPatient_ShowsOnlySafeAntibiotics`
10. `CultureReportTemplate_GroupsBySensitivityLevel` — يظهر Highly / Moderate / Low / Resistant كأقسام منفصلة.

#### 2.6 معايير القبول

1. عند فتح مريضة حامل، تختفي المضادات غير الآمنة تلقائيًا من الجدول.
2. عند فتح طفل (<12 سنة)، تختفي المضادات غير الآمنة للأطفال.
3. حفظ نتيجة كاملة (Culture + حتى 3 كائنات + جدول حساسية) يعمل ويحفظ في DB ضمن transaction واحد.
4. طباعة `CultureReportTemplate` تُظهر تصنيف Highly / Moderate / Low / Resistant كمجموعات صريحة.
5. `OrganismAntibiotic.Sensitivity` أصبح enum ولا يقبل قيمًا نصية عشوائية.
6. `MicrobiologyCulture` يحمل `CultureCondition` و `ColonyCount` على مستوى العينة.
7. جميع الاختبارات العشرة تجتاز.

#### 2.7 مخاطر واعتبارات

- **Data Migration لعمود `Sensitivity`:** إن كان الإنتاج يحوي بيانات حقيقية بصياغات مختلفة (S/I/R، Sensitive/Intermediate/Resistant، أو بالعربية)، خريطة التحويل النصية أعلاه قد لا تغطي كل الحالات. قبل تطبيق الترحيل، يُنصح بتشغيل `SELECT DISTINCT Sensitivity FROM OrganismAntibiotic` ومراجعة القيم فعليًا.
- **التأثير على تقارير قديمة:** إن كان هناك تقارير مطبوعة قديمة تعتمد على `Sensitivity` كنص، سيتغيّر شكل قراءتها. لا نفس التقارير الجديدة سيغطي هذا.
- **الأداء:** `CultureEntryViewModel` يحمل قائمة مضادات ضخمة (قد تكون >100 مضاد). استخدم `ICollectionView` مع Virtualization في DataGrid.

---

## 2. جدول ملخّص أولوية التنفيذ

| # | الشريحة | الجهد | Migrations | اختبارات | مخاطر |
|---|---|---|---|---|---|
| 1 | S-BC-01 الباركود ثلاثي المستويات + Lab ID | 3-4 أيام | 1 (`AddPatientBarcodeAndLabId`) | 8 | نقطة قرار مطلوبة قبل البدء (صيغة 13 خانة) |
| 2 | S-CULT-01 نافذة المزرعة + Sensitivity Enum | 4-5 أيام | 1 (`AddCultureFieldsAndSensitivityEnum`) | 10 | Data migration لبيانات Sensitivity القائمة |

**إجمالي مقدَّر:** 7 – 9 أيام عمل فعلي (Slices 1 و 2 معًا).

---

## 3. قيود إلزامية للوكيل المنفّذ

1. **لا حذف** لأي كود موجود دون طلب صريح.
2. **الحفاظ على القرارات المعمارية:**
   - منطق الأعمال في ViewModels (لا Domain Layer إضافي).
   - لا Repository Pattern — استخدام `FinalLabDbContext` مباشرة داخل الخدمات.
   - `TestCatalogService` (God Class بـ 51 دالة) يبقى كما هو — أي إضافة تُضاف عليه، لا تفكيك.
3. **كل شريحة تُنفَّذ في PR مستقل.**
4. **Migrations مسمّاة بوصف صريح** (كما أعلاه).
5. **حل نقطة قرار 1.2 (صيغة الباركود) قبل بدء Slice 1** — لا يجوز للوكيل المنفّذ اختيار الصيغة من تلقاء نفسه.

---

*انتهت الوثيقة المعتمدة.*