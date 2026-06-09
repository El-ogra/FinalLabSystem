# مرجع تحليل بيانات التحاليل والقيم الطبيعية
## Test Data Management & Normal Ranges — Reference Document

> **ملف:** `Docs/test_data_field_reference.md`  
> **تاريخ الإنشاء:** 2026-06-05  
> **المصدر:** تحليل الكود المصدري الكامل للمشروع (WPF / .NET 8)

---

# القسم الأول — نافذة بيانات التحاليل (Test Data Management Window)

## 1.1 سير العمل — إضافة تحليل جديد (Adding a New Test)

1. **الضغط على زر «جديد»** (Command: `NewCommand`):
   - `TestList.SelectedTest = null` — إلغاء تحديد أي تحليل في القائمة
   - `TestDetail.StartNew(AllTests)` — إنشاء كائن `TestType` جديد فارغ (القيم الافتراضية: `TurnaroundHours=24`, `PrintWithOther=true`, `AddWithGroup=true`, `IsActive=true`)
   - `IsDirty = true` — يصبح زر «حفظ» مفعّلًا فورًا
   - جميع الحقول تصبح فارغة أو بقيمها الافتراضية وقابلة للتعديل

2. **تعبئة الحقول الإلزامية**:
   - **Code** (`TypeCode`) — مطلوب × لا يمكن أن يكون فارغًا
   - **Test Name** (`TypeNameEn`) — مطلوب × لا يمكن أن يكون فارغًا
   - **Group** (`GroupId`) — مطلوب × يجب اختيار مجموعة من القائمة المنسدلة

3. **تعبئة الحقول الاختيارية**: Arabic Name, History Name, Report Names, Bill Names, Arrange#, Test Time Hours, Notes, Collection Notes, Prices, Workflow Flags, Outside Lab, Patient Question

4. **الضغط على زر «حفظ»** (Command: `SaveCommand`):
   - يتم استدعاء `TestDetail.Validate()`:
     - **إذا فشل التحقق**: يظهر `MessageBox` بالرسالة المناسبة (بالعربية) ويتم إلغاء الحفظ
     - **إذا نجح التحقق**: يكمل التنفيذ
   - يتم استدعاء `TestDetail.BuildEntity()`:
     - `TypeCode` → `Trim()`
     - `TypeNameEn` → `Trim()`
     - جميع النصوص الاختيارية → `NullIfWhiteSpace()` (تحويل النص الفارغ إلى `null`)
     - `OutsideLabName` و `OutsideCostPrice` → تُضبط فقط إذا كان `IsSendOutside = true`، وإلّا `null`
     - `DefaultPrice = PatientPrice`
     - `IsActive = true`
   - `entity.TesttypeId == 0` (لأنه جديد) ← يتم استدعاء:
     - `_testCatalogService.CreateTestTypeAsync(entity, PatientPrice, LabToLabPrice, BuildTubes())`
   - يتم تحميل التحليل المُنشأ حديثًا:
     - `TestDetail.LoadAsync(id, AllTests)`
     - `IsDirty = false`
   - يتم تحديث القائمة:
     - `TestList.RefreshAsync()`

5. **بعد الحفظ**: يمكن الضغط على «القيم الطبيعية» لإضافة Normal Ranges (يجب حفظ التحليل أولًا — `TesttypeId > 0`)

---

## 1.2 سير العمل — تعديل تحليل موجود (Editing an Existing Test)

1. **البحث عن التحليل في القائمة (الجهة اليسرى)**:
   - يمكن اختيار وضع البحث من `ComboBox` (قيم `SearchMode`):
     - `Code` — بحث بالكود (مطابقة تامة)
     - `Group` — بحث باسم المجموعة (يحتوي على)
     - `Name` — بحث بالاسم (يبدأ بـ)
   - كتابة النص في `TextBox` (ToolTip: "بحث")
   - الفلترة تتم تلقائيًا مع كل تغيير (`UpdateSourceTrigger=PropertyChanged`)
   - `ApplyFilter()` تقارن `SearchText` مع `AllTests` باستخدام `StringComparison.OrdinalIgnoreCase`

2. **اختيار التحليل من الـ DataGrid**:
   - `SelectedItem="{Binding TestList.SelectedTest}"` يتحول إلى `null` عند بدء تحليل جديد
   - `OnSelectedTestChanged` يُشغّل `TestDetail.LoadAsync(row.TesttypeId, AllTests)`
   - يتم استدعاء `_testCatalogService.GetTestTypeDetailsAsync(testTypeId)` لجلب كامل التفاصيل (مع `Group`, `TestComponents`, `TestTypePrices`, `TestTypeSampleTubes`)
   - `IsDirty = false` ← زر «حفظ» غير مفعّل في البداية

3. **تعديل الحقول**:
   - لا يوجد زر «Edit» صريح — الحقول دائمًا قابلة للتعديل
   - `IsDirty` يصبح `true` مع أول تغيير في أي حقل ← زر «حفظ» يصبح مفعّلًا
   - `PatientPrice` و `LabToLabPrice` لهما `MarkDirty()` منفصل في setter الخاص بهما

4. **حفظ التعديلات** (`SaveCommand`):
   - نفس `Validate()` و `BuildEntity()` كما في الإضافة
   - `entity.TesttypeId > 0` ← يتم استدعاء:
     - `_testCatalogService.UpdateTestTypeAsync(entity, PatientPrice, LabToLabPrice, BuildTubes())`
   - `TestDetail.AcceptChanges()` ← `IsDirty = false`
   - `TestList.RefreshAsync()` ← تحديث القائمة

5. **إلغاء التعديلات**:
   - لا يوجد زر «Cancel» صريح
   - لإلغاء التغييرات غير المحفوظة: يمكن اختيار تحليل آخر من القائمة (سيتم تحميل بياناته و `IsDirty = false`)
   - أو الضغط على «جديد» (يبدأ تحليلًا جديدًا و `IsDirty = true`)

6. **الضغط على «تحديث»** (`RefreshCommand`):
   - يعيد تحميل كل التحاليل من `GetAllTestTypesAsync()` ويطبق الفلتر الحالي

---

## 1.3 سير العمل — حذف تحليل (Deleting a Test)

1. **اختيار التحليل من القائمة** (أو التأكد من تحميله في `TestDetail`)
2. **الضغط على زر «حذف»** (Command: `DeleteCommand`):
   - `CanExecute` ← `TestDetail.TesttypeId > 0` (يجب أن يكون التحليل محفوظًا مسبقًا)
3. **ظهور رسالة تأكيد**: `"هل تريد حذف هذا التحليل؟"` (نعم/لا)
4. **إذا تم اختيار «نعم»**:
   - `_testCatalogService.DeleteTestTypeAsync(TestDetail.TesttypeId)`
     - إذا كان للتحليل `VisitTests` مرتبطة به ← **Soft Delete**: `IsActive = false` فقط
     - إذا لم يكن له `VisitTests` ← **Hard Delete**: `_context.TestTypes.Remove(entity)`
   - `TestList.RefreshAsync()` — تحديث القائمة
   - `TestDetail.StartNew(AllTests)` — بدء تحليل جديد في النموذج
5. **إذا تم اختيار «لا»** ← لا يحدث شيء

---

## 1.4 سير العمل — إدارة أنابيب العينات (Zone C — Sample Tubes)

1. **إضافة أنبوب جديد** (زر "Add"):
   - `AddTubeCommand`:
     - `SortOrder` = `Max(SortOrder) + 1`
     - `TubeType = "Default"`, `Quantity = 1`, `IsActive = true`
     - يُضاف إلى `Tubes` ObservableCollection
     - يُختار تلقائيًا (`SelectedTube = row`)
     - `IsDirty = true`

2. **تعديل أنبوب موجود** (زر "Edit"):
   - `EditTubeCommand`:
     - `CanExecute` ← `SelectedTube is not null`
     - `MarkDirty()` — يسمح بتحرير الخلايا في الـ DataGrid مباشرة
   - يمكن تعديل القيم داخل خلايا الـ DataGrid:
     - Sort, Tube Type, Tube Color, Sample Type, Qty, Active (CheckBox), Notes

3. **حذف أنبوب** (زر "Delete"):
   - `DeleteTubeCommand`:
     - `CanExecute` ← `SelectedTube is not null`
     - `Tubes.Remove(SelectedTube)` — إزالة من المجموعة
     - `SelectedTube = null`
     - `IsDirty = true`

4. **الحفظ**: عند الضغط على «حفظ»، يتم استدعاء `BuildTubes()`:
   - لكل أنبوب: `ToEntity()` يُحوّله إلى `TestTypeSampleTube`
   - في `UpdateTestTypeAsync`: يتم **حذف كل الأنابيب القديمة** ثم إعادة إضافة القائمة الجديدة (`RemoveRange` + `Add` لكل أنبوب)
   - في `CreateTestTypeAsync`: تُضاف الأنابيب بعد إنشاء التحليل مباشرة

---

## 1.5 جدول مرجع الحقول — نافذة بيانات التحاليل

### Zone A — قائمة التحاليل (الجهة اليسرى)

| Field Name (as shown in UI) | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Search Mode ComboBox** (غير معنون) | `TestList.SearchMode` | `SearchMode` enum | اختياري | اختيار وضع البحث | يحدد طريقة البحث: Code / Group / Name |
| **Search TextBox** (ToolTip: "بحث") | `TestList.SearchText` | `string` | اختياري | نص البحث | نص للبحث في قائمة التحاليل حسب الوضع المختار |
| **Arrange#** (Column) | `SortOrder` | `short` | - | رقم الترتيب | ترتيب ظهور التحليل في القائمة |
| **Group** (Column) | `GroupNameAr` | `string` | - | المجموعة | اسم مجموعة التحليل |
| **Code** (Column) | `TypeCode` | `string` | - | الكود | كود التحليل |
| **Test Name** (Column) | `DisplayName` | `string` | - | اسم التحليل | الاسم العربي أو الإنجليزي للعرض |
| **Patient Price** (Column) | `PatientPrice` | `double` | - | سعر المريض | سعر التحليل للمريض |
| **Lab-to-Lab** (Column) | `LabToLabPrice` | `double` | - | سعر معمل-لمعمل | سعر التحليل بين المعامل |
| **Tubes** (Column) | `TubeCount` | `int` | - | عدد الأنابيب | عدد أنابيب العينات النشطة للتحليل |

### Zone B — Identity (هوية التحليل)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Code** | `TestDetail.TypeCode` | `string` | **نعم** | الكود | كود تعريف التحليل (يجب أن يكون فريدًا) |
| **Test Name** | `TestDetail.TypeNameEn` | `string` | **نعم** | اسم التحليل بالإنجليزية | الاسم الإنجليزي للتحليل |
| **Arabic Name** | `TestDetail.TypeNameAr` | `string?` | لا | اسم التحليل بالعربية | الاسم العربي للتحليل |
| **History Name** | `TestDetail.HistoryName` | `string?` | لا | اسم التاريخ المرضي | الاسم الذي يظهر في سجل المريض |

### Zone B — Report Names (أسماء التقارير)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Line 1** | `TestDetail.ReportNameLine1` | `string?` | لا | السطر الأول في التقرير | اسم التحليل كما يظهر في التقرير — السطر الأول |
| **Line 2** | `TestDetail.ReportNameLine2` | `string?` | لا | السطر الثاني في التقرير | اسم التحليل كما يظهر في التقرير — السطر الثاني |

### Zone B — Bill Names (أسماء الفواتير)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Line 1** | `TestDetail.BillNameLine1` | `string?` | لا | السطر الأول في الفاتورة | اسم التحليل كما يظهر في الفاتورة — السطر الأول |
| **Line 2** | `TestDetail.BillNameLine2` | `string?` | لا | السطر الثاني في الفاتورة | اسم التحليل كما يظهر في الفاتورة — السطر الثاني |

### Zone B — Classification (التصنيف)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Group** | `TestDetail.GroupId` (SelectedValue) | `int` | **نعم** | المجموعة | تصنيف التحليل ضمن مجموعة (`TestGroup`). مصدر البيانات: `TestDetail.Groups` (ObservableCollection) |
| **Arrange#** | `TestDetail.SortOrder` | `short` | لا | رقم الترتيب | ترتيب التحليل داخل المجموعة |
| **Test Time Hours** | `TestDetail.TurnaroundHours` | `short` | لا | مدة التحليل (ساعات) | زمن إنجاز التحليل بالساعات (الافتراضي: 24) |

### Zone B — Workflow Flags (خيارات سير العمل)

| Field Name (CheckBox Content) | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Is Routine Test** | `TestDetail.IsRoutineTest` | `bool` | لا | تحليل روتيني | يظهر في قائمة التحاليل الروتينية اليومية |
| **See Report** | `TestDetail.SeeReport` | `bool` | لا | يظهر في التقرير | يتحكم في ظهور التحليل في تقرير المريض |
| **Print With Other** | `TestDetail.PrintWithOther` | `bool` | لا | طباعة مع الآخرين | يُطبع مع تحاليل أخرى في نفس التقرير (افتراضي: true) |
| **Add With Group** | `TestDetail.AddWithGroup` | `bool` | لا | إضافة مع المجموعة | يُضاف تلقائيًا عند اختيار المجموعة (افتراضي: true) |
| **Is Main Test** | `TestDetail.IsMainTest` | `bool` | لا | تحليل رئيسي | يعتبر تحليلًا رئيسيًا (للتجميع مع تحاليل فرعية) |

### Zone B — Notes (ملاحظات)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Collection Notes** | `TestDetail.CollectionNotes` | `string?` | لا | ملاحظات جمع العينة | تعليمات جمع العينة للمريض (AcceptsReturn) |
| **Notes** | `TestDetail.Notes` | `string?` | لا | ملاحظات عامة | ملاحظات داخلية عن التحليل (AcceptsReturn) |

### Zone B — Pricing (التسعير)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Patient Price** | `TestDetail.PatientPrice` | `double` | لا (يجب ≥ 0) | سعر المريض | السعر الذي يدفعه المريض (يُخزن في `TestTypePrice` مع Scheme="Patient Price") |
| **Lab-to-Lab Price** | `TestDetail.LabToLabPrice` | `double` | لا (يجب ≥ 0) | سعر معمل-لمعمل | سعر التحليل بين المعامل (يُخزن في `TestTypePrice` مع Scheme="Lab-to-Lab Price") |

### Zone C — Sample Tubes DataGrid (أنابيب العينات)

| Field Name (Column Header) | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Sort** | `SortOrder` | `short` | لا | ترتيب الأنبوب | ترتيب ظهور الأنبوب |
| **Tube Type** | `TubeType` | `string` | لا | نوع الأنبوب | نوع الأنبوب المستخدم (افتراضي: "Default") |
| **Tube Color** | `TubeColor` | `string?` | لا | لون الأنبوب | لون غطاء الأنبوب |
| **Sample Type** | `SampleType` | `string?` | لا | نوع العينة | نوع العينة المطلوبة (دم، بول، إلخ) |
| **Qty** | `Quantity` | `int` | لا | الكمية | كمية الأنابيب المطلوبة (الحد الأدنى: 1) |
| **Active** | `IsActive` | `bool` | لا | نشط | هل الأنبوب نشط |
| **Notes** | `Notes` | `string?` | لا | ملاحظات | ملاحظات إضافية عن الأنبوب |

### Zone C — أزرار إدارة الأنابيب

| Field Name (Button Content) | Command Binding | Arabic Description | Arabic Purpose |
|---|---|---|---|
| **Add** | `TestDetail.AddTubeCommand` | إضافة أنبوب | إضافة أنبوب جديد بالقيم الافتراضية |
| **Edit** | `TestDetail.EditTubeCommand` | تعديل أنبوب | تفعيل التعديل على الأنبوب المحدد (CanExecute: SelectedTube != null) |
| **Delete** | `TestDetail.DeleteTubeCommand` | حذف أنبوب | حذف الأنبوب المحدد من القائمة |

### Zone D — Outside Lab (معمل خارجي)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Is Send Outside** (CheckBox) | `TestDetail.IsSendOutside` | `bool` | لا | إرسال لمعمل خارجي | تفعيل إرسال التحليل إلى معمل خارجي |
| **Outside Lab Name** | `TestDetail.OutsideLabName` | `string?` | لا* | اسم المعمل الخارجي | اسم المعمل الذي سيُرسل إليه التحليل (*مطلوب فقط إذا `IsSendOutside=true`) |
| **Outside Cost Price** | `TestDetail.OutsideCostPrice` | `decimal?` | **نعم*** | سعر تكلفة المعمل الخارجي | تكلفة التحليل من المعمل الخارجي (*مطلوب إذا `IsSendOutside=true`) |

### Zone E — Patient Question (سؤال المريض)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Patient Question** (TextBlock not visible — فقط الـ TextBox) | `TestDetail.PatientQuestion` | `string?` | لا | سؤال للمريض | سؤال يظهر للمريض قبل إجراء التحليل (AcceptsReturn) |

### Zone E — زر القيم الطبيعية

| Field Name (Button Content) | Command Binding | Arabic Description | Arabic Purpose |
|---|---|---|---|
| **القيم الطبيعية** | `TestDetail.OpenNormalRangesCommand` | فتح نافذة القيم الطبيعية | يفتح `NormalRangesWindow` (CanExecute: `TesttypeId > 0`) |

### أزرار شريط الأدوات السفلي (Bottom Toolbar)

| Button Content | Command Binding | Arabic Description | Arabic Purpose |
|---|---|---|---|
| **جديد** | `NewCommand` | إنشاء تحليل جديد | مسح النموذج وبدء تحليل جديد |
| **حفظ** | `SaveCommand` | حفظ التحليل | حفظ التحليل (جديد أو تعديل) بعد التحقق من الصحة |
| **حذف** | `DeleteCommand` | حذف التحليل | حذف التحليل بعد تأكيد المستخدم |
| **تحديث** | `TestList.RefreshCommand` | تحديث القائمة | إعادة تحميل كل التحاليل من قاعدة البيانات |
| **إغلاق** | `CloseCommand` | إغلاق النافذة | العودة إلى القائمة الرئيسية (`_navigationService.ReturnToMain()`) |

---

# القسم الثاني — نافذة القيم الطبيعية (Normal Ranges Window)

## 2.1 فتح النافذة (Opening the Window)

1. **متى تُفتح**: عند الضغط على زر «القيم الطبيعية» في نافذة بيانات التحاليل
2. **الشرط المسبق**: يجب حفظ التحليل أولًا (`TesttypeId > 0`)، وإلّا تظهر رسالة: `"احفظ التحليل أولاً قبل إدخال القيم الطبيعية."`
3. **ماذا يحدث عند الفتح** (`InitializeAsync`):
   - يتم جلب تفاصيل التحليل كاملة: `_testCatalogService.GetTestTypeDetailsAsync(parentTest.TesttypeId)`
   - `Title` يُضبط إلى `"القيم الطبيعية - {TypeNameEn}"`
   - يتم جلب `ParentTest.TestComponents.ToList()`:
     - **إذا كان عدد المكونات = 0**: يتم **إنشاء مكون افتراضي تلقائيًا**:
       - `ComponentCode = ParentTest.TypeCode`
       - `ComponentNameEn = ParentTest.TypeNameEn`
       - `ResultType = "NUMERIC"`
       - `IsActive = true`, `SortOrder = 1`
       - لا يُحفظ تلقائيًا في قاعدة البيانات — يبقى في الذاكرة حتى Save All
     - **إذا كان هناك مكونات موجودة**: تُستخدم مباشرة
   - `List.LoadComponents(components)`:
     - `Components.Clear()` ثم إضافة كل مكون (منسوخ عبر `CloneComponent`)
     - `SelectedComponent = Components.FirstOrDefault()`

---

## 2.2 سير العمل — إضافة قيمة طبيعية (Adding a Normal Range)

### لحالة بسيطة (كلا الجنسين، كل الأعمار)

1. **اختيار المكوّن** من `ListBox` (الجزء العلوي الأيسر) إذا كان هناك أكثر من مكوّن
2. **الضغط على زر "Add"** (ضمن منطقة Ranges في الجزء السفلي الأيسر):
   - `AddRangeCommand` (CanExecute: `SelectedComponent != null`)
   - يُنشئ `NormalRange` جديد بالقيم الافتراضية:
     - `Sex = "Both"`, `FastingState = "Any"`
     - `AgeFromDays = 0`, `AgeToDays = 36500` (أي من 0 يوم إلى ~100 سنة)
   - يُضاف إلى `RangesForSelectedComponent` ويُختار تلقائيًا

3. **تعبئة الحقول في الجهة اليمنى**:
   - **Sex** — اختر "Both" (افتراضي)
   - **Age From Days** و **Age To Days** — اترك 0 و 36500 (كل الأعمار)
   - **Age Description** — اختياري، يمكن تركه فارغًا (سيظهر "0-36500"؟ لا — AgeDescription يُترك null حسب `NullIfWhiteSpace` الذي لا يُستخدم هنا)
   - **Fasting State** — اختر "Any" (افتراضي)
   - **Low Normal** و **High Normal** — أدخل الحدود الطبيعية الدنيا والعليا
   - **Low Critical** و **High Critical** — اختياري، اتركه فارغًا
   - **Unit** — وحدة القياس (مثل "mg/dL")
   - **Normal Range Text** — اختياري، للنص الحر
   - **Range Note** — اختياري

4. **الضغط على زر "Apply"** (`Detail.ApplyCommand`):
   - التحقق: إذا كان `LowNormal > HighNormal` ← رسالة تحذير: `"Low Normal يجب أن يكون أقل من أو يساوي High Normal."`
   - التحقق: إذا كان `LowCritical > HighCritical` ← رسالة تحذير مماثلة
   - إذا نجح التحقق: `RangeApplied` event يُشغّل مع نسخة من الـ range
   - `IsDirty = false` للـ Detail

5. **الضغط على "Save All" لحفظ كل التغييرات** (راجع القسم 2.4)

### إضافة قيمة خاصة بجنس معين

1. اختر "M" (ذكر) أو "F" (أنثى) في حقل **Sex** (RadioButton)
2. املأ باقي الحقول كما هو موضح أعلاه
3. `Sex` يُضبط إلى `"M"` أو `"F"` تلقائيًا عند اختيار الراديو بوتن

### إضافة قيمة لفئة عمرية محددة

1. **Age From Days**: أدخل اليوم الأول للفئة (مثال: للأطفال من سنة: `365`)
2. **Age To Days**: أدخل اليوم الأخير للفئة (مثال: للأطفال حتى 12 سنة: `4380`)
3. **Age Description**: نص وصفي مثل "الأطفال (1-12 سنة)"
4. **ملاحظة**: العمر يُخزن دائمًا بـ **الأيام** (عدد صحيح `int`). `AgeFromDays=0` يعني منذ الولادة، `AgeToDays=36500` يعني حتى ~100 سنة

### Normal Range Text — متى يُستخدم

- حقل نصي حر (`NormalRangeText`) بقبول أسطر متعددة (`AcceptsReturn=True`, `TextWrapping="Wrap"`)
- يُستخدم عندما لا يمكن التعبير عن القيم الطبيعية كأرقام (Low/High Normal)
- مثال: "Negative"، "Not detected"، أو وصف نصي لنتيجة نوعية
- يمكن استخدامه **بدلاً من** أو **بالإضافة إلى** الحقول الرقمية — لا يوجد شرط إلزامي لأي منهما

### الفرق بين Normal limits و Critical limits

| النوع | الحقلان | الوصف |
|---|---|---|
| **Normal limits** | `LowNormal` / `HighNormal` | الحدود الطبيعية للنتيجة — القيم بينهما تعتبر طبيعية |
| **Critical limits** | `LowCritical` / `HighCritical` | الحدود الحرجة — القيم تحت `LowCritical` أو فوق `HighCritical` تعتبر خطيرة وتتطلب تنبيهًا عاجلًا |

---

## 2.3 إدارة المكونات (Managing Components)

### ما هو `TestComponent`؟

`TestComponent` هو **مكوّن تحليل** — أي تحليل قد يتكون من مكوّن واحد أو أكثر:
- مثال: تحليل "صورة دم كاملة" (CBC) له مكوّنات متعددة: Hb, WBC, RBC, PLT, إلخ
- كل مكوّن له قيم طبيعية خاصة به (`NormalRange`) وقد تختلف حسب الجنس والعمر
- `TestComponent` له: `ComponentId`, `TesttypeId`, `ComponentCode`, `ComponentNameEn`, `ComponentNameAr`, `Unit`, `ResultType`, `DecimalPlaces`, `SortOrder`, `IsActive`

### إضافة مكوّن جديد

1. **الضغط على زر "Add"** (في منطقة Components — الجزء العلوي الأيسر):
   - `AddComponentCommand`:
     - `ComponentCode = "COMP" + next` (مثال: "COMP1")
     - `ComponentNameEn = "New Component"`
     - `ResultType = "NUMERIC"`
     - `IsActive = true`, `SortOrder = next`
   - يُضاف إلى `Components` ويُختار تلقائيًا

### متى تحتاج مكوّنات متعددة؟

- عندما يكون للتحليل **نتائج متعددة** تقاس بشكل منفصل ولكل منها قيم طبيعية مختلفة
- مثال: تحليل "وظائف الكبد" قد يشمل ALT, AST, Albumin — كل واحد `TestComponent` منفصل
- التحليل بمكون واحد فقط (مثل "Glucose") له `TestComponent` واحد — يُنشأ تلقائيًا عند فتح نافذة القيم الطبيعية

### وحدة القياس (Unit)

- تُدخل في حقل **Unit** في الجهة اليمنى
- عند الحفظ: `SelectedComponent.Unit = _detail.Unit` — تُحفظ الوحدة على مستوى `TestComponent` وليس `NormalRange`
- تُحمّل عند اختيار مكون: `_detail.Unit = SelectedComponent.Unit`

---

## 2.4 Save All and Close

### ماذا يفعل `SaveAllCommand`؟

عند الضغط على "Save All":

**المرحلة 1 — حفظ المكونات** (كل `Component` في `Components`):
- لكل مكون:
  - إذا كان `ComponentId == 0` (جديد):
    - `_testCatalogService.AddComponentAsync(component.TesttypeId, component)` ← يُرجع الـ ID الجديد
  - إذا كان `ComponentId > 0` (موجود):
    - `_testCatalogService.UpdateComponentAsync(component)`

**المرحلة 2 — حفظ القيم الطبيعية** (كل `Range` في `RangesForSelectedComponent`):
- لكل range:
  - `range.ComponentId = SelectedComponent.ComponentId` (ربط الـ range بالمكوّن الحالي)
  - إذا كان `range.RangeId == 0` (جديد):
    - `_testCatalogService.AddRangeAsync(range)` ← يُرجع الـ ID الجديد
  - إذا كان `range.RangeId > 0` (موجود):
    - `_testCatalogService.UpdateRangeAsync(range)`

**ملاحظة هامة**: يتم حفظ **جميع المكونات أولاً** ثم **جميع الـ ranges الخاصة بالمكوّن المحدد فقط**. الـ ranges الخاصة بالمكونات الأخرى غير المحددة لا تُحفظ في هذه الدورة.

### Close — ماذا يحدث؟

- `CloseCommand`:
  - `CommandParameter="{Binding RelativeSource={RelativeSource AncestorType=Window}}"`
  - يستقبل كائن `Window` ويستدعي `window.Close()`
- **لا يوجد تحقق من وجود تغييرات غير محفوظة** — إذا ضغط المستخدم Close دون Save All، تُفقد التغييرات بدون إنذار

---

## 2.5 جدول مرجع الحقول — نافذة القيم الطبيعية

### اللوحة اليسرى — قائمة المكونات (Components List)

| Field Name (as shown in UI) | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Components ListBox** (عرض `ComponentNameEn`) | `List.Components` (ItemsSource) / `List.SelectedComponent` (SelectedItem) | `ObservableCollection<TestComponent>` / `TestComponent?` | - | قائمة مكونات التحليل | قائمة بكل مكونات التحليل لاختيار المكوّن المطلوب |
| **Add** (Button) | `List.AddComponentCommand` | `ICommand` | - | إضافة مكوّن | إضافة مكوّن جديد للتحليل |
| **Delete** (Button) | `List.DeleteComponentCommand` | `ICommand` | - | حذف مكوّن | حذف المكوّن المحدد (مع ranges المرتبطة به) |

### اللوحة اليسرى — جدول القيم الطبيعية (Ranges DataGrid)

| Field Name (Column Header) | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **Sex** | `Sex` | `string` | **نعم** | الجنس | الجنس المُطبّق عليه المدى: M / F / Both |
| **AgeFrom** | `AgeFromDays` | `int` | **نعم** | العمر من (أيام) | بداية الفئة العمرية بالأيام (افتراضي: 0) |
| **AgeTo** | `AgeToDays` | `int` | **نعم** | العمر إلى (أيام) | نهاية الفئة العمرية بالأيام (افتراضي: 36500) |
| **Description** | `AgeDescription` | `string?` | لا | وصف الفئة العمرية | نص وصفي للفئة العمرية (مثال: "الأطفال") |
| **Fasting** | `FastingState` | `string` | **نعم** | حالة الصيام | حالة الصيام: Any / Fasting |
| **LowNormal** | `LowNormal` | `double?` | لا | الحد الأدنى الطبيعي | أقل قيمة طبيعية للنتيجة |
| **HighNormal** | `HighNormal` | `double?` | لا | الحد الأقصى الطبيعي | أعلى قيمة طبيعية للنتيجة |
| **LowCritical** | `LowCritical` | `double?` | لا | الحد الأدنى الحرج | أقل قيمة حرجة (اختياري) |
| **HighCritical** | `HighCritical` | `double?` | لا | الحد الأقصى الحرج | أعلى قيمة حرجة (اختياري) |
| **Text** | `NormalRangeText` | `string?` | لا | نص القيمة الطبيعية | نص وصفي بديل للقيم الطبيعية (للنتائج النوعية) |

### أزرار إدارة القيم الطبيعية (أسفل الـ DataGrid)

| Field Name (Button Content) | Command Binding | Arabic Description | Arabic Purpose |
|---|---|---|---|
| **Add** | `List.AddRangeCommand` | إضافة مدى طبيعي | إضافة سطر جديد للقيم الطبيعية |
| **Edit** | `List.EditRangeCommand` | تعديل المدى | تحميل المدى المحدد في لوحة التحرير اليمنى |
| **Delete** | `List.DeleteRangeCommand` | حذف المدى | حذف المدى المحدد |

### اللوحة اليمنى — تفاصيل القيمة الطبيعية (Range Detail)

| Field Name | Binding Property | Data Type | Required | Arabic Description | Arabic Purpose |
|---|---|---|---|---|---|
| **M** (RadioButton) | `Detail.IsSexMale` | `bool` | - | ذكر | اختيار المدى للذكور (يُضبط `Sex = "M"`) |
| **F** (RadioButton) | `Detail.IsSexFemale` | `bool` | - | أنثى | اختيار المدى للإناث (يُضبط `Sex = "F"`) |
| **Both** (RadioButton) | `Detail.IsSexBoth` | `bool` | - | كلا الجنسين | اختيار المدى لكلا الجنسين (يُضبط `Sex = "Both"`) — افتراضي |
| **Age From Days** | `Detail.AgeFromDays` | `int` | **نعم** | العمر من (أيام) | بداية العمر بالأيام (افتراضي: 0) |
| **Age To Days** | `Detail.AgeToDays` | `int` | **نعم** | العمر إلى (أيام) | نهاية العمر بالأيام (افتراضي: 36500) |
| **Age Description** | `Detail.AgeDescription` | `string?` | لا | وصف العمر | وصف نصي للفئة العمرية |
| **Any** (RadioButton) | `Detail.IsFastingAny` | `bool` | - | أي حالة | المدى ينطبق على أي حالة صيام (يُضبط `FastingState = "Any"`) — افتراضي |
| **Fasting** (RadioButton) | `Detail.IsFasting` | `bool` | - | صائم | المدى ينطبق على المريض الصائم فقط (يُضبط `FastingState = "Fasting"`) |
| **Applies To Pregnant** (CheckBox) | `Detail.AppliesToPregnant` | `bool?` | لا | ينطبق على الحوامل | هل ينطبق هذا المدى على المريضات الحوامل |
| **Low Normal** | `Detail.LowNormal` | `double?` | لا | الحد الأدنى الطبيعي | أقل قيمة طبيعية |
| **High Normal** | `Detail.HighNormal` | `double?` | لا | الحد الأقصى الطبيعي | أعلى قيمة طبيعية |
| **Low Critical** | `Detail.LowCritical` | `double?` | لا | الحد الأدنى الحرج | أقل قيمة حرجة |
| **High Critical** | `Detail.HighCritical` | `double?` | لا | الحد الأقصى الحرج | أعلى قيمة حرجة |
| **Unit** | `Detail.Unit` | `string?` | لا | وحدة القياس | وحدة قياس المكوّن (مثل mg/dL) — تُحفظ في `TestComponent.Unit` وليس في `NormalRange` |
| **Normal Range Text** | `Detail.NormalRangeText` | `string?` | لا | نص القيمة الطبيعية | نص وصفي بديل (AcceptsReturn) |
| **Range Note** | `Detail.RangeNote` | `string?` | لا | ملاحظة المدى | ملاحظة إضافية عن هذا المدى (AcceptsReturn) |
| **Apply** (Button) | `Detail.ApplyCommand` | `ICommand` | - | تطبيق | تطبيق القيم المدخلة على المدى (بعد التحقق من صحة LowNormal ≤ HighNormal) |

### أزرار شريط الأدوات السفلي

| Button Content | Command Binding | Arabic Description | Arabic Purpose |
|---|---|---|---|
| **Save All** | `SaveAllCommand` | حفظ الكل | حفظ كل المكونات وكل الـ ranges في قاعدة البيانات |
| **Close** | `CloseCommand` | إغلاق | إغلاق النافذة (بدون تحقق من وجود تغييرات غير محفوظة) |

---

# القسم الثالث — ملخص التحقق من الصحة (Validation Summary)

| # | القاعدة (Rule) | الحقل (Field(s)) | رسالة الخطأ (Error Message) | مكان التحقق |
|---|---|---|---|---|
| 1 | كود التحليل لا يمكن أن يكون فارغًا | `TypeCode` | `"كود التحليل مطلوب."` | `TestDetailViewModel.Validate()` (السطر 276) |
| 2 | كود التحليل يجب أن يكون فريدًا | `TypeCode` | `"كود التحليل مستخدم من قبل."` | `TestDetailViewModel.Validate()` (السطر 279) — مقارنة case-insensitive مع `AllTests` باستثناء نفس التحليل |
| 3 | اسم التحليل بالإنجليزية لا يمكن أن يكون فارغًا | `TypeNameEn` | `"اسم التحليل باللغة الإنجليزية مطلوب."` | `TestDetailViewModel.Validate()` (السطر 282) |
| 4 | يجب اختيار مجموعة للتحليل | `GroupId` | `"مجموعة التحليل مطلوبة."` | `TestDetailViewModel.Validate()` (السطر 285) — `GroupId <= 0` |
| 5 | الأسعار لا يمكن أن تكون سالبة | `PatientPrice`, `LabToLabPrice` | `"الأسعار لا يمكن أن تكون سالبة."` | `TestDetailViewModel.Validate()` (السطر 287) |
| 6 | سعر تكلفة المعمل الخارجي مطلوب عند تفعيل الإرسال للخارج | `OutsideCostPrice` (عند `IsSendOutside=true`) | `"سعر تكلفة المعمل الخارجي مطلوب عند إرسال التحليل للخارج."` | `TestDetailViewModel.Validate()` (السطر 289) |
| 7 | `LowNormal` يجب أن يكون ≤ `HighNormal` | `LowNormal`, `HighNormal` | `"Low Normal يجب أن يكون أقل من أو يساوي High Normal."` | `NormalRangeDetailViewModel.Apply()` (السطر 157) |
| 8 | `LowCritical` يجب أن يكون ≤ `HighCritical` | `LowCritical`, `HighCritical` | `"Low Critical يجب أن يكون أقل من أو يساوي High Critical."` | `NormalRangeDetailViewModel.Apply()` (السطر 163) |
| 9 | يجب حفظ التحليل قبل فتح القيم الطبيعية | `TesttypeId` | `"احفظ التحليل أولاً قبل إدخال القيم الطبيعية."` | `TestDataManagementViewModel.OnOpenNormalRangesRequested()` (السطر 117) |
| 10 | الكمية الدنيا للأنبوب هي 1 | `Quantity` (في `TestTypeSampleTubeRowViewModel`) | يتم ضبط القيمة إلى 1 إذا كانت أقل (بدون رسالة) | `TestTypeSampleTubeRowViewModel.Quantity` setter (Clamp) |

---

# القسم الرابع — ملخص طبقة الخدمات (Service Layer Summary)

## `ITestCatalogService` — جميع الدوال المستخدمة من النافذتين

| # | اسم الدالة | التوقيع (Signature) | الوصف بالعربية | متى تُستدعى |
|---|---|---|---|---|
| 1 | `GetAllTestTypesAsync` | `Task<List<TestType>>()` | جلب كل التحاليل مع الأسعار والمكونات والأنابيب | عند فتح نافذة بيانات التحاليل وعند الضغط على «تحديث» |
| 2 | `GetTestTypeDetailsAsync` | `Task<TestType?>(int testTypeId)` | جلب تفاصيل تحليل واحد كامل مع المجموعة والمكونات والأسعار والأنابيب | عند اختيار تحليل من القائمة وعند فتح نافذة القيم الطبيعية |
| 3 | `GetActiveGroupsAsync` | `Task<List<TestGroup>>()` | جلب قائمة المجموعات النشطة | عند تحميص `InitializeLookupsAsync()` — لملء ComboBox المجموعات |
| 4 | `CreateTestTypeAsync` | `Task<int>(TestType, double, double, IReadOnlyList<TestTypeSampleTube>)` | إنشاء تحليل جديد مع الأسعار والأنابيب | عند الضغط على «حفظ» لتحليل جديد (إنشاء) |
| 5 | `UpdateTestTypeAsync` | `Task(TestType, double, double, IReadOnlyList<TestTypeSampleTube>)` | تحديث تحليل موجود مع الأسعار والأنابيب (حذف وإعادة إضافة الأنابيب) | عند الضغط على «حفظ» لتحليل موجود (تعديل) |
| 6 | `DeleteTestTypeAsync` | `Task(int testTypeId)` | حذف تحليل (Soft إذا له زيارات، Hard إن لم يكن) | عند الضغط على «حذف» بعد التأكيد |
| 7 | `GetRangesForComponentAsync` | `Task<List<NormalRange>>(int componentId)` | جلب كل القيم الطبيعية لمكوّن معين مرتبة حسب Sex ثم AgeFromDays | عند اختيار مكوّن مختلف في نافذة القيم الطبيعية |
| 8 | `AddComponentAsync` | `Task<int>(int testTypeId, TestComponent)` | إضافة مكوّن جديد لتحليل معين | عند الضغط على Save All — للمكونات الجديدة (`ComponentId == 0`) |
| 9 | `UpdateComponentAsync` | `Task(TestComponent)` | تحديث مكوّن موجود | عند الضغط على Save All — للمكونات الموجودة (`ComponentId > 0`) |
| 10 | `DeleteComponentAsync` | `Task(int componentId)` | حذف مكوّن مع جميع الـ ranges المرتبطة به (Hard Delete) | عند الضغط على Delete Component في نافذة القيم الطبيعية |
| 11 | `AddRangeAsync` | `Task<int>(NormalRange)` | إضافة قيمة طبيعية جديدة | عند الضغط على Save All — للـ ranges الجديدة (`RangeId == 0`) |
| 12 | `UpdateRangeAsync` | `Task(NormalRange)` | تحديث قيمة طبيعية موجودة | عند الضغط على Save All — للـ ranges الموجودة (`RangeId > 0`) |
| 13 | `DeleteRangeAsync` | `Task(int rangeId)` | حذف قيمة طبيعية (Hard Delete) | عند الضغط على Delete Range في نافذة القيم الطبيعية |

---

# نماذج البيانات (Data Models) — ملخص سريع

## `TestType` (Models/TestType.cs)

| Property | Type | Nullable | ملاحظات |
|---|---|---|---|
| `TesttypeId` | `int` | لا | PK |
| `GroupId` | `int` | لا | FK → TestGroup |
| `TypeCode` | `string` | لا (`= null!`) | كود التحليل — فريد |
| `TypeNameEn` | `string` | لا (`= null!`) | الاسم بالإنجليزية |
| `TypeNameAr` | `string?` | نعم | الاسم بالعربية |
| `DefaultPrice` | `double` | لا | السعر الافتراضي |
| `TurnaroundHours` | `short` | لا | مدة الإنجاز (افتراضي 24) |
| `SortOrder` | `short` | لا | ترتيب العرض |
| `IsActive` | `bool` | لا | نشط / Soft-delete flag |
| `IsRoutineTest` | `bool` | لا | تحليل روتيني |
| `SeeReport` | `bool` | لا | يظهر في التقرير |
| `PrintWithOther` | `bool` | لا | طباعة مع آخرين (افتراضي true) |
| `AddWithGroup` | `bool` | لا | إضافة مع المجموعة (افتراضي true) |
| `IsMainTest` | `bool` | لا | تحليل رئيسي |
| `IsSendOutside` | `bool` | لا | يُرسَل لمعمل خارجي |
| `OutsideCostPrice` | `decimal?` | نعم | تكلفة المعمل الخارجي |
| `OutsideLabName` | `string?` | نعم | اسم المعمل الخارجي |
| `PatientQuestion` | `string?` | نعم | سؤال للمريض |
| `HistoryName` | `string?` | نعم | اسم التاريخ المرضي |
| `ReportNameLine1/2` | `string?` | نعم | أسماء التقارير |
| `BillNameLine1/2` | `string?` | نعم | أسماء الفواتير |
| `CollectionNotes` | `string?` | نعم | ملاحظات جمع العينة |
| `Notes` | `string?` | نعم | ملاحظات عامة |

**Navigation Properties**: `Group`, `TestComponents` (1:N), `TestTypePrices` (1:N), `TestTypeSampleTubes` (1:N), `VisitTests` (1:N), `TestProfileItems` (1:N), `ReportCommentTemplates` (1:N)

## `TestComponent` (Models/TestComponent.cs)

| Property | Type | Nullable |
|---|---|---|
| `ComponentId` | `int` | لا (PK) |
| `TesttypeId` | `int` | لا (FK → TestType) |
| `ComponentCode` | `string` | لا |
| `ComponentNameEn` | `string` | لا |
| `ComponentNameAr` | `string?` | نعم |
| `Unit` | `string?` | نعم |
| `ResultType` | `string` | لا (مثال: "NUMERIC") |
| `DecimalPlaces` | `byte` | لا |
| `SortOrder` | `short` | لا |
| `IsActive` | `bool` | لا |

**Navigation**: `NormalRanges` (1:N), `Testtype`, `TestResults`, `ReportCommentTemplates`

## `NormalRange` (Models/NormalRange.cs)

| Property | Type | Nullable |
|---|---|---|
| `RangeId` | `int` | لا (PK) |
| `ComponentId` | `int` | لا (FK → TestComponent) |
| `Sex` | `string` | لا (M / F / Both) |
| `AgeFromDays` | `int` | لا |
| `AgeToDays` | `int` | لا |
| `AgeDescription` | `string?` | نعم |
| `AppliesToPregnant` | `bool?` | نعم |
| `FastingState` | `string` | لا (Any / Fasting) |
| `LowNormal` | `double?` | نعم |
| `HighNormal` | `double?` | نعم |
| `LowCritical` | `double?` | نعم |
| `HighCritical` | `double?` | نعم |
| `NormalRangeText` | `string?` | نعم |
| `RangeNote` | `string?` | نعم |

**Navigation**: `Component` → `TestComponent`

## `TestTypeSampleTube` (Models/TestTypeSampleTube.cs)

| Property | Type | Nullable |
|---|---|---|
| `TestTypeTubeId` | `int` | لا (PK) |
| `TestTypeId` | `int` | لا (FK → TestType) |
| `TubeType` | `string` | لا |
| `TubeColor` | `string?` | نعم |
| `SampleType` | `string?` | نعم |
| `Quantity` | `int` | لا |
| `SortOrder` | `short` | لا |
| `IsActive` | `bool` | لا |
| `Notes` | `string?` | نعم |

---

## نظام الأسعار (Pricing System)

- `TestTypePrice` (كيان غير مُقرأ مباشرةً، لكن يُستخدم في الـ service):
  - `TesttypeId` (FK → TestType)
  - `SchemeId` (FK → PriceScheme)
  - `Price` (double)
- `PriceScheme`:
  - `SchemeId` (PK)
  - `SchemeName` (string) — قيمتان مُثبّتتان: `"Patient Price"` و `"Lab-to-Lab Price"`
- عند إنشاء تحليل: يُضاف سجلّي `TestTypePrice` (واحد لكل Scheme)
- عند التحديث: يُبحث عن السجلات الموجودة، يُحدّث السعر أو يُنشأ سجل جديد إذا كان مفقودًا

---

## خريطة التنقل بين الصفحات

```
MainWindow
  └─ SystemSettingsMenuView (DataTemplate في MainWindow.xaml)
       └─ Button "بيانات التحاليل" → NavigateToTestDataCommand
            └─ TestDataManagementWindow (نافذة بيانات التحاليل)
                 └─ زر "القيم الطبيعية" → OpenNormalRangesCommand
                      └─ NormalRangesWindow (نافذة القيم الطبيعية)
```

---

*تم إعداد هذا التقرير بناءً على تحليل كامل للكود المصدري للمشروع.*
*جميع الإشارات إلى أسماء الحقول والإجراءات تستند إلى النصوص الفعلية في ملفات XAML و C#.*
