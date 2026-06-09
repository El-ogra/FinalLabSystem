# تقرير التحليل والتخطيط — تطوير نافذتي بيانات التحاليل والمعدلات الطبيعية

**المستودع:** El-ogra/FinalLabSystem
**الفرع:** main
**الـ Commit المستهدف:** c66f7c2 — "إكمال الخطوة الأولي لنافذة الفئات والمجموعات"
**المرجع:** Analysis_and_Normal_Range_Guide.pdf — Real Lab System v1.0
**نوع التقرير:** تحليل وتخطيط فقط (بدون أي تعديل على الكود)

---

## القسم الأول: فهم الهدف

### 1-1 نظرة شاملة على النظامين

النظام المرجعي الموصوف في ملف الـ PDF هو نظام Real Lab System ذو نضج وظيفي عالٍ، يقدم نافذتين متكاملتين:

- **نافذة بيانات التحاليل (Analysis Data Window):** تُدار من خلالها كل بيانات التحليل التعريفية مع تكامل قوي مع إعدادات شركة المعمل، باركودات الأنابيب، الفروع، ومجموعات العمل.
- **نافذة المعدلات الطبيعية (Reference Setting Window):** تتيح إدارة مدى مرجعي مفصل لكل مكوّن مع أعلام تحذير، تعليقات سريرية، ومدى حرج، إضافة إلى تخصيص للحامل.

النظام الحالي في المستودع (FinalLabSystem) قد أنجز فعلياً عند هذه النقطة:

- بنية MVVM ناضجة (نمط Coordinator/List/Detail/Row).
- نموذج بيانات شامل يحتوي على أغلب الحقول الأساسية لـ TestType، TestTypeSampleTube، TestComponent، وNormalRange.
- Service واحد موحد ITestCatalogService مع تنفيذ كامل (CRUD على Category/Group/TestType/Component/Range).
- ترحيلات EF Core مرتبة زمنياً (AddV4SystemEnhancements → AddTestDataManagementFields).

### 1-2 نافذة بيانات التحاليل: مقارنة عامة

| البُعد | النظام المرجعي (PDF) | النظام الحالي (الكود) |
|---|---|---|
| إدخال رموز خاصة (α β γ δ µ) | شريط أزرار يدرج الحرف في الحقل النشط | غير موجود إطلاقاً |
| Collection | قائمة منسدلة مع قاعدة بيانات مرجعية | TextBox حر طويل (CollectionNotes nvarchar(1000)) |
| Tube/Barcode | قسم باركود بسيط: Name + Tube1/Tube2/Tube3 كقوائم منسدلة | DataGrid قابل للتعديل بأعمدة Type/Color/Sample/Qty/Active/Notes |
| Branch | قائمة منسدلة لفرع المعمل | غير موجود في النموذج ولا في الواجهة |
| Log Group | قائمة منسدلة لمجموعة العمل (Work List) | غير موجود في النموذج ولا في الواجهة |
| Layout | لوحة على اليمين + لوحة عريضة على اليسار + شريط أزرار سفلي + لوحة باركود مستقلة | عمودان فقط (قائمة على اليمين، تفاصيل على اليسار) دون لوحة باركود مرئية بشكل مستقل |
| الأسماء (Bill/Report) | يدعم سطرين مفصولين | مدعوم بالكامل (ReportNameLine1/2, BillNameLine1/2) |
| الـ Flags | Routine, See, PrintWithOther, AddWithGroup, MainTest | موجودة بالكامل ✓ |
| ZoneD — إرسال خارجي | غير مفصل في PDF | موجود إضافي (IsSendOutside, OutsideLabName, OutsideCostPrice) |
| ZoneE — سؤال للمريض | غير موضح | موجود (PatientQuestion nvarchar(500)) |

### 1-3 نافذة المعدلات الطبيعية: مقارنة عامة

| البُعد | النظام المرجعي (PDF) | النظام الحالي (الكود) |
|---|---|---|
| Age Unit | قائمة (يوم/شهر/سنة) | يخزن AgeFromDays و AgeToDays فقط بالأيام |
| Low/High Flag | حقلا نص قصير (L, H, … إلخ) | غير موجود في النموذج ولا في الواجهة |
| Low/High Comment | تعليقات سريرية نصية | غير موجود |
| Critical Range Text | نص يطبع في التقرير في حالة الحرج | غير موجود (يوجد LowCritical/HighCritical رقمي فقط) |
| Critical Low / High | حقلان رقميان | موجودان ✓ (LowCritical, HighCritical) |
| Critical Flag | نص قصير يظهر للنتائج الحرجة | غير موجود |
| Critical Comment | تعليق سريري للحرج | غير موجود |
| For Pregnancy Only checkbox | خانة اختيار صريحة | موجود تقريباً (AppliesToPregnant bool?) ولكن بدلالة "ينطبق أيضاً" وليس "للحامل فقط" |
| Greek Buttons (α β γ µ) | شريط أزرار للأحرف الخاصة | غير موجود |
| البنية الإجمالية | قائمة مكونات + قائمة مدى + لوحة تفاصيل مفصلة بأقسام (Normal / Flag / Comment / Critical) | قائمة مكونات + جدول مدى + تفاصيل بسيطة |

### 1-4 الفجوة التلخيصية

النموذج الحالي ينقصه عمومياً:
1. كيان Branch (فرع المعمل) كجدول مرجعي + علاقة على TestType.
2. كيان LogGroup (مجموعة العمل) كجدول مرجعي + علاقة على TestType.
3. كيان CollectionType (شروط سحب العينة) كجدول مرجعي بدل النص الحر.
4. حقول Flags/Comments على NormalRange (Low/High/Critical).
5. حقول وحدة العمر AgeUnit إضافة إلى تخزين العمر الخام.
6. نص Critical Range وعلم وتعليق Critical على NormalRange.
7. سلوك "For Pregnancy Only" صريح (وليس مجرد AppliesToPregnant).
8. أداة UI مشتركة لإدراج الأحرف اليونانية وكتابة الرموز الخاصة.

---

## القسم الثاني: التحليل التفصيلي — نافذة بيانات التحاليل

### العنصر A — أزرار الأحرف الخاصة (α β γ δ µ … إلخ)

**النظام المرجعي:**
شريط أزرار صغير بجوار حقول النص (اسم التحليل، اسم التقرير، الملاحظات) لإدراج الحرف في الحقل النشط حالياً.

**الوضع الحالي طبقة طبقة:**

- **النموذج/قاعدة البيانات:** لا توجد علاقة. هذه ميزة UI خالصة لا تحتاج جدولاً.
- **الترحيلات:** لا تأثير.
- **الـ Service:** لا تأثير.
- **الـ ViewModels:** لا توجد أوامر (Commands) أو UserControl مخصص. حقول النصوص في `TestDetailViewModel` تستقبل القيمة عبر Binding ثنائي بسيط فقط.
- **الـ Views:** ملف `TestDataManagementWindow.xaml` يحتوي حقول TextBox دون أي شريط أحرف خاصة.

**ما الذي ينقص:**
- UserControl جديد (`SpecialCharsBar`) يبث حدث "أُدرج حرف" مع متابعة آخر TextBox نشط في النافذة.
- خدمة مساعدة أو سلوك (Behavior) لتتبع `FocusManager.FocusedElement` وإدراج الحرف في موضع المؤشر.
- لا يوجد في النموذج ما لم يُبنَ في الواجهة (لأن الميزة لا تخص النموذج أصلاً).

### العنصر B — Collection كقائمة منسدلة منظمة

**النظام المرجعي:**
يختار المستخدم من قائمة محددة مسبقاً (Serum Fasting، EDTA Blood، Urine 24h، CSF، Stool … إلخ). كل تحليل يربط حتى 3 أنابيب مرتبطة بهذه القائمة.

**الوضع الحالي طبقة طبقة:**

- **النموذج:** `TestType.CollectionNotes` من نوع string?‎ nvarchar(1000) — مجرد نص حر طويل.
- **الترحيلات:** ترحيل `AddTestDataManagementFields` أضاف `collection_notes` كحقل نصي مفرد. لا يوجد جدول مرجعي.
- **الـ Service:** لا يوجد أي API لاسترجاع قائمة Collections.
- **الـ ViewModel:** `TestDetailViewModel.CollectionNotes` مرتبط بـ TextBox.
- **الـ View:** `TestDataManagementWindow.xaml` يستخدم TextBox مع `Height="58" AcceptsReturn="True"` (متعدد الأسطر).

**ما الذي ينقص تحديداً في كل طبقة:**

| الطبقة | الناقص |
|---|---|
| النموذج | كيان جديد `CollectionType` (CollectionTypeId, NameAr, NameEn, IsActive, SortOrder) + خاصية CollectionTypeId اختيارية على TestType |
| الترحيل | إنشاء جدول `CollectionType` + إضافة عمود `collection_type_id` على TestType (FK اختياري) + بذر بيانات افتراضية |
| Service | إضافة `Task<List<CollectionType>> GetActiveCollectionTypesAsync()` |
| ViewModel | إضافة `ObservableCollection<CollectionType> CollectionTypes` و`int? CollectionTypeId` في `TestDetailViewModel` |
| View | استبدال TextBox الطويل بـ ComboBox مرتبط بهذه القائمة (مع إبقاء CollectionNotes كـ TextBox ملاحظات إضافي اختياري) |

**هل يوجد في النموذج ما لم يُبنَ في الواجهة؟** نعم، يوجد `TestType.SampleType` و`DefaultTubeType` و`DefaultTubeColor` معرفة في النموذج لكنها غير معروضة في الواجهة الحالية.

### العنصر C — تبسيط قسم Tube/Barcode

**النظام المرجعي:**
قسم باركود بسيط: Name، Tube 1، Tube 2، Tube 3 — قوائم منسدلة فقط مرتبطة بإعداد Collection.

**الوضع الحالي طبقة طبقة:**

- **النموذج:** كيان `TestTypeSampleTube` (TestTypeTubeId, TestTypeId, TubeType, TubeColor, SampleType, Quantity, SortOrder, IsActive, Notes) مع علاقة 1-Many من TestType.
- **الترحيل:** أُنشئ الجدول في `AddTestDataManagementFields` مع index على (testtype_id, sort_order).
- **Service:** يُحفظ ضمن `CreateTestTypeAsync` و`UpdateTestTypeAsync` كقائمة (مع RemoveRange + إعادة إضافة).
- **ViewModel:** `TestTypeSampleTubeRowViewModel` يكشف 7 خواص قابلة للتعديل، و`TestDetailViewModel.Tubes` كـ ObservableCollection.
- **View:** DataGrid قابل للتعديل بأعمدة Sort/Tube Type/Tube Color/Sample Type/Qty/Active/Notes + أزرار Add/Edit/Delete (وزر Edit حالياً لا يفعل شيئاً سوى MarkDirty).

**ما الذي ينقص:**

| الطبقة | الناقص |
|---|---|
| النموذج | لا تغيير جوهري؛ مطلوب تقييد عدد الأنابيب لـ 3 منطقياً (أو إبقاء الكثرة) + ربط TubeType بجدول مرجعي بدلاً من نص حر |
| الترحيل | تحويل tube_type إلى FK نحو CollectionType (أو جدول TubeType مستقل) — قرار معماري لاحقاً |
| Service | لا تغيير |
| ViewModel | تعديل `TestDetailViewModel` بحيث يعرض 3 ComboBox (Tube1/2/3) بدل DataGrid، يُحوّل داخلياً إلى قائمة |
| View | استبدال DataGrid الحالي بقسم باركود نظيف: Name (Sample Name) + ComboBox×3 لكل Tube، مع قائمة مرجعية واحدة |

**هل يوجد في النموذج ما لم يُبنَ في الواجهة؟** الواجهة الحالية أعقد من النموذج المطلوب (over-engineered). يجب التبسيط لا الإضافة.

### العنصر D — حقل Branch (فرع المعمل)

**النظام المرجعي:**
قائمة منسدلة تبيّن أي فرع من المعمل مسؤول عن التحليل.

**الوضع الحالي:**

- **النموذج:** **لا يوجد** كيان `Branch` ولا خاصية `BranchId` على TestType. (مرجع: TestType.cs لا تحتوي إلا على GroupId).
- **الترحيلات:** لا شيء يخص الفروع.
- **Service:** لا API.
- **ViewModel:** لا خاصية ولا ObservableCollection.
- **View:** لا ComboBox.

**ما الذي ينقص في كل طبقة:** الكيان كاملاً (انظر القسم الرابع — الترحيلات المطلوبة).

### العنصر E — حقل Log Group (مجموعة العمل/الجهاز)

**النظام المرجعي:**
قائمة منسدلة لتحديد مجموعة العمل أو الجهاز التحليلي.

**الوضع الحالي:**

- **النموذج:** **لا يوجد** كيان LogGroup ولا خاصية LogGroupId على TestType.
- **الترحيلات:** لا شيء.
- **Service:** لا API.
- **ViewModel:** لا شيء.
- **View:** لا شيء.

**ملاحظة مهمة:** يجب عدم الخلط بين `TestGroup` الموجود حالياً (وهو التصنيف الإكلينيكي، مثل وظائف الكبد) وبين `LogGroup` المطلوب (وهو مجموعة العمل/الجهاز التحليلي). هما كيانان مختلفان وظيفياً.

### العنصر F — مقارنة Layout الإجمالي

**النظام المرجعي حسب PDF:** ثلاث مناطق منطقية (هوية + باركود + إعدادات الإرسال) موزعة في لوحة يمنى عريضة، وقائمة فرعية + شريط أحرف خاصة + شريط أزرار سفلي.

**النظام الحالي:** عمودان بنسبة 40/60، اليمين قائمة DataGrid مع بحث، اليسار ScrollViewer به Identity / Report Names / Bill Names / Classification / Workflow Flags / Notes / Pricing / Zone C Tubes / Zone D Outside / Zone E Patient Question / زر القيم الطبيعية، وشريط أزرار سفلي (جديد/حفظ/حذف/تحديث/إغلاق).

**الفروقات الهيكلية:**
1. لا يوجد شريط أحرف خاصة في الأعلى.
2. لا توجد منطقة باركود مستقلة بصرياً — أُدمج Tubes داخل ScrollViewer.
3. لا حقول Branch / LogGroup / Collection (كقائمة).
4. حقل Collection معروض كـ TextBox طويل بدلاً من ComboBox.
5. زر "القيم الطبيعية" داخل التفاصيل وليس في الشريط السفلي حيث المعتاد في النظام المرجعي.

---

## القسم الثالث: التحليل التفصيلي — نافذة المعدلات الطبيعية

### العنصر A — قائمة وحدة العمر (Days / Months / Years)

**النظام المرجعي:** قائمة منسدلة تختار وحدة العمر، مع حفظ القيمة كما أدخلها المستخدم (لا تحويل صامت لأيام).

**الوضع الحالي طبقة طبقة:**

- **النموذج (NormalRange.cs):** `AgeFromDays` (int) و`AgeToDays` (int) فقط. لا حقل AgeUnit.
- **الترحيل:** `age_from_days` و`age_to_days` فقط، الأخير بافتراضي 36500.
- **Service:** يُحفظ كما هو دون أي تحويل.
- **ViewModel:** `AgeFromDays` و`AgeToDays` فقط — يفترض ضمنياً أن الإدخال بالأيام.
- **View:** حقلا نص "Age From Days" و"Age To Days" — اسم الحقل نفسه يفرض الأيام على المستخدم.

**ما الذي ينقص:**

| الطبقة | الناقص |
|---|---|
| النموذج | حقل `AgeUnit` (string، 10 char، Default "Days") + حقلا `AgeFromValue` و`AgeToValue` (أو إبقاء AgeFromDays/AgeToDays كقيم محسوبة) |
| الترحيل | عمود `age_unit` + بيانات افتراضية للسجلات القائمة |
| Service | لا تغيير API |
| ViewModel | إضافة `AgeUnits` (ObservableCollection<string>) و`AgeUnit` (string)، وتحويل القيم عند الحفظ |
| View | إضافة ComboBox بجوار حقلي العمر، وإعادة تسمية التسميات لتزيل كلمة "Days" |

### العنصر B — Low Flag / High Flag

**النظام المرجعي:** نص قصير يظهر بجوار النتيجة في التقرير عند تجاوزها للنطاق.

**الوضع الحالي:**
- **النموذج:** **غير موجود** على NormalRange. الموجود فقط الحقول الرقمية للحدود.
- **الترحيل / Service / ViewModel / View:** لا شيء.

**ما الذي ينقص في كل طبقة:**

| الطبقة | الناقص |
|---|---|
| النموذج | `LowFlag` (string?, nvarchar(20)) و`HighFlag` (string?, nvarchar(20)) |
| الترحيل | إضافة العمودين، nullable |
| Service | لا تغيير |
| ViewModel (Detail) | خاصيتان مع MarkDirty |
| View | حقلا TextBox تحت قسم Normal Range |

### العنصر C — Low Comment / High Comment

**النظام المرجعي:** تعليق سريري طويل يظهر في التقرير ضمن خلية الملاحظات عند النتائج غير الطبيعية.

**الوضع الحالي:** غير موجود في أي طبقة.

**ما الذي ينقص:**

| الطبقة | الناقص |
|---|---|
| النموذج | `LowComment` (string?, nvarchar(500)) و`HighComment` (string?, nvarchar(500)) |
| الترحيل | إضافة العمودين |
| Service | لا تغيير |
| ViewModel | خاصيتان |
| View | حقلا TextBox متعددي السطور |

### العنصر D — المعدل الحرج (Critical Range / Critical Low / Critical High / Critical Flag / Critical Comment)

**النظام المرجعي:** قسم كامل للمعدل الحرج: نص يطبع في التقرير + الحدود الرقمية + علم + تعليق.

**الوضع الحالي:**

- **النموذج:** يوجد `LowCritical` و`HighCritical` (double?) فقط.
- **الترحيل:** `low_critical` و`high_critical` فقط.
- **Service / ViewModel / View:** الحقلان الرقميان فقط معروضان.

**ما الذي ينقص:**

| الطبقة | الناقص |
|---|---|
| النموذج | `CriticalRangeText` (string?, nvarchar(200))، `CriticalFlag` (string?, nvarchar(20))، `CriticalComment` (string?, nvarchar(500)) |
| الترحيل | الأعمدة الثلاثة الجديدة |
| Service | لا تغيير |
| ViewModel | ثلاث خواص جديدة على `NormalRangeDetailViewModel` |
| View | قسم منفصل "Critical" مع جميع الحقول (الرقمية موجودة، يضاف النص، العلم، التعليق) |

### العنصر E — For Pregnancy Only checkbox

**النظام المرجعي:** خانة اختيار صريحة تعني "هذا المدى ينطبق فقط على الحوامل".

**الوضع الحالي:**

- **النموذج:** `AppliesToPregnant` (bool?) — قيمة ثلاثية (null / true / false).
- **الترحيل:** `applies_to_pregnant` bit nullable.
- **Service:** يحفظ كما هو.
- **ViewModel:** `AppliesToPregnant` كـ bool? مكشوف.
- **View:** CheckBox واحد "Applies To Pregnant".

**ملاحظة دلالية:** السلوك الحالي ثلاثي (null = غير محدد، true = ينطبق أيضاً، false = لا ينطبق)، بينما النظام المرجعي يستخدم خانتين صريحتين أو سيمانتيك مختلف "للحوامل فقط".

**ما الذي ينقص:**
- إعادة دلالة الحقل أو إضافة حقل بولياني صريح `IsForPregnancyOnly` بدلالة "تستثنى نتائج غير الحوامل من هذا المدى".
- تحديث تسمية CheckBox في الواجهة من "Applies To Pregnant" إلى "For Pregnancy Only" مع منطق فلترة مختلف عند تطبيق المدى على نتيجة.

### العنصر F — أزرار الأحرف الخاصة (نفس A في نافذة التحاليل)

**الوضع الحالي:** غير موجود.

**ما الذي ينقص:** UserControl مشترك يستخدم في كلتا النافذتين (انظر العنصر A في القسم الثاني). لا حاجة لتغيير النموذج.

### العنصر G — مقارنة البنية الإجمالية

**النظام المرجعي:** قائمة مكونات + جدول مدى + لوحة تفاصيل مقسمة لأقسام واضحة (Sex/Age + Normal + Flag + Comment + Critical + Pregnancy + Apply).

**النظام الحالي:** قائمة مكونات + جدول مدى مفصل (10 أعمدة) + لوحة تفاصيل مدمجة فيها (Sex Radio + Age + Fasting + LowN/HighN/LowC/HighC/Unit + NormalRangeText + RangeNote + Apply).

**الفروقات:**
1. لا يوجد فصل بصري واضح بين قسم Normal وقسم Critical.
2. لا توجد أعمدة جديدة في جدول المدى لـ Flag/Comment.
3. زر Apply موجود ولكن أزرار التنفيذ الرئيسية محصورة بـ Save All / Close فقط.
4. لا شريط أحرف خاصة في الأعلى.

---

## القسم الرابع: الترحيلات المطلوبة

| # | اسم الترحيل المقترح | الجداول/الأعمدة | يتضمن ترحيل بيانات؟ | الترتيب | سبب الترتيب |
|---|---|---|---|---|---|
| 1 | AddCollectionTypeLookup | جدول جديد `CollectionType` (CollectionTypeId, NameEn, NameAr, IsActive, SortOrder) + Seed لبيانات افتراضية (Serum, Serum Fasting, EDTA Blood, Citrate Plasma, Urine, Urine 24h, Stool, CSF, Semen) | نعم (Seed) | الأول | يجب أن يوجد الجدول قبل أن يشير إليه TestType |
| 2 | AddBranchLookup | جدول جديد `Branch` (BranchId, NameEn, NameAr, Address, Phone, IsActive, SortOrder) + Seed بفرع افتراضي "Main" | نعم (Seed افتراضي) | الثاني | جدول مرجعي مستقل، لا تبعية على Collection |
| 3 | AddLogGroupLookup | جدول جديد `LogGroup` (LogGroupId, NameEn, NameAr, Description, IsActive, SortOrder) + Seed افتراضي "General" | نعم (Seed افتراضي) | الثالث | مستقل |
| 4 | AddTestTypeLookupReferences | على TestType: collection_type_id (int? FK)، branch_id (int? FK)، log_group_id (int? FK)، Indexes + بدون ترحيل بيانات قسري (تبقى TestType.CollectionNotes كنص ملاحظات إضافية) | لا (الحقول nullable) | الرابع | يعتمد على وجود الجداول 1-3 |
| 5 | AddNormalRangeAgeUnit | على NormalRange: age_unit nvarchar(10) NOT NULL DEFAULT 'Days' + إضافة AgeFromValue/AgeToValue (يمكن إبقاؤها كحقول محسوبة، أو الإبقاء على AgeFromDays/AgeToDays كما هي وإضافة وحدة فقط) | نعم (Default = 'Days' للسجلات القائمة) | الخامس | يمس فقط NormalRange، مستقل عن TestType |
| 6 | AddNormalRangeFlagsAndComments | على NormalRange: low_flag nvarchar(20)?, high_flag nvarchar(20)?, low_comment nvarchar(500)?, high_comment nvarchar(500)? | لا | السادس | تابع للسابق على نفس الجدول |
| 7 | AddNormalRangeCriticalFields | على NormalRange: critical_range_text nvarchar(200)?, critical_flag nvarchar(20)?, critical_comment nvarchar(500)? | لا | السابع | يكمل قسم Critical |
| 8 | RefineAppliesToPregnant (اختياري) | إعادة تسمية أو إضافة حقل صريح `is_for_pregnancy_only` bit | يتطلب نسخ القيم من AppliesToPregnant | الثامن | لتجنب كسر الدلالة، يفضل إضافة عمود جديد ثم إهمال القديم لاحقاً |

**ملاحظات هامة:**
- جميع الأعمدة الجديدة على الجداول القائمة (TestType، NormalRange) يجب أن تكون `nullable` أو ذات `DEFAULT` لكي لا تكسر بيانات قائمة.
- لا حاجة لإعادة بناء الجداول؛ كلها AddColumn و AddForeignKey.
- يجب تحديث `FinalLabDbContextModelSnapshot.cs` تلقائياً عن طريق EF Tools (`dotnet ef migrations add`).
- العلاقات الجديدة 1-Many، مع `OnDelete = DeleteBehavior.Restrict` على TestType ↔ Branch / LogGroup / CollectionType حتى لا يُحذف فرع لا تزال تحاليله مستخدمة.

---

## القسم الخامس: خطة العمل التفصيلية

| # | اسم الخطوة | الطبقة | الملفات الجديدة (المسارات الكاملة) | الملفات المعدلة + التغيير | التبعيات | التعقيد |
|---|---|---|---|---|---|---|
| 1 | إنشاء كيان CollectionType | Model | FinalLabSystem/Models/CollectionType.cs | لا تعديل | — | بسيطة |
| 2 | إنشاء كيان Branch | Model | FinalLabSystem/Models/Branch.cs | لا تعديل | — | بسيطة |
| 3 | إنشاء كيان LogGroup | Model | FinalLabSystem/Models/LogGroup.cs | لا تعديل | — | بسيطة |
| 4 | تحديث TestType بإضافة FKs | Model | — | Models/TestType.cs: إضافة CollectionTypeId?, BranchId?, LogGroupId? + Navigation Properties | 1،2،3 | متوسطة |
| 5 | تحديث NormalRange بحقول جديدة | Model | — | Models/NormalRange.cs: AgeUnit, LowFlag, HighFlag, LowComment, HighComment, CriticalRangeText, CriticalFlag, CriticalComment | — | متوسطة |
| 6 | تسجيل DbSets وFluent API | Data | — | Data/FinalLabDbContext.cs: 3 DbSet جديدة + ‫entity.HasOne (Branch/LogGroup/CollectionType) داخل modelBuilder.Entity<TestType> + property mapping للحقول الجديدة على NormalRange | 1-5 | متوسطة |
| 7 | ترحيل AddCollectionTypeLookup | Migration | FinalLabSystem/Migrations/<timestamp>_AddCollectionTypeLookup.cs | تحديث ModelSnapshot تلقائياً | 6 | متوسطة |
| 8 | ترحيل AddBranchLookup | Migration | FinalLabSystem/Migrations/<timestamp>_AddBranchLookup.cs | تحديث ModelSnapshot | 7 | متوسطة |
| 9 | ترحيل AddLogGroupLookup | Migration | FinalLabSystem/Migrations/<timestamp>_AddLogGroupLookup.cs | تحديث ModelSnapshot | 8 | متوسطة |
| 10 | ترحيل AddTestTypeLookupReferences | Migration | FinalLabSystem/Migrations/<timestamp>_AddTestTypeLookupReferences.cs | تحديث ModelSnapshot | 9 | متوسطة |
| 11 | ترحيل AddNormalRangeAgeUnit | Migration | FinalLabSystem/Migrations/<timestamp>_AddNormalRangeAgeUnit.cs | تحديث ModelSnapshot | 10 | بسيطة |
| 12 | ترحيل AddNormalRangeFlagsAndComments | Migration | FinalLabSystem/Migrations/<timestamp>_AddNormalRangeFlagsAndComments.cs | تحديث ModelSnapshot | 11 | بسيطة |
| 13 | ترحيل AddNormalRangeCriticalFields | Migration | FinalLabSystem/Migrations/<timestamp>_AddNormalRangeCriticalFields.cs | تحديث ModelSnapshot | 12 | بسيطة |
| 14 | توسيع ITestCatalogService | Service Interface | — | Services/Interfaces/ITestCatalogService.cs: GetActiveCollectionTypesAsync, GetActiveBranchesAsync, GetActiveLogGroupsAsync | 6 | بسيطة |
| 15 | تنفيذ Service الموسّع | Service Impl | — | Services/Implementations/TestCatalogService.cs: تنفيذ الثلاث methods + تحديث Update/Create لـ TestType لإسناد FKs الجديدة، تحديث UpdateRange لتضمين الحقول الجديدة | 14 | متوسطة |
| 16 | إنشاء UserControl لشريط الأحرف الخاصة | View Shared | FinalLabSystem/Views/Shared/SpecialCharsBar.xaml + .cs | لا تعديل | — | متوسطة |
| 17 | Behavior لتتبع الـ TextBox النشط | Infrastructure | FinalLabSystem/Infrastructure/Behaviors/ActiveTextBoxTracker.cs | لا تعديل | — | متوسطة |
| 18 | توسيع TestDetailViewModel | ViewModel | — | ViewModels/Settings/TestDetailViewModel.cs: إضافة CollectionTypes/BranchOptions/LogGroupOptions + CollectionTypeId/BranchId/LogGroupId + استدعاء Service الجديد في InitializeLookupsAsync | 15 | معقدة |
| 19 | إعادة تصميم XAML لنافذة بيانات التحاليل | View | — | Views/Settings/TestDataManagementWindow.xaml: إضافة SpecialCharsBar في الأعلى + قسم Barcode منفصل بـ ComboBox×3 + 3 ComboBox جديدة (Collection/Branch/LogGroup) + تبسيط DataGrid الأنابيب أو إخفاؤه | 16، 17، 18 | معقدة |
| 20 | إنشاء NormalRangeRowViewModel ‫(إن لزم) | ViewModel | FinalLabSystem/ViewModels/Settings/NormalRangeRowViewModel.cs (اختياري لكشف الأعمدة الجديدة بدل الـ binding المباشر على Model) | — | 5 | بسيطة |
| 21 | توسيع NormalRangeDetailViewModel | ViewModel | — | ViewModels/Settings/NormalRangeDetailViewModel.cs: AgeUnit + AgeUnits[] + LowFlag/HighFlag + LowComment/HighComment + CriticalRangeText/CriticalFlag/CriticalComment + IsForPregnancyOnly | 15 | معقدة |
| 22 | إعادة تصميم XAML لنافذة المعدلات الطبيعية | View | — | Views/Settings/NormalRangesWindow.xaml: SpecialCharsBar + ComboBox لـ Age Unit + قسم Flag/Comment + قسم Critical كامل + إعادة تسمية Checkbox للحامل | 16، 17، 21 | معقدة |
| 23 | تسجيل ViewModels الجديدة في DI | DI | — | App.xaml.cs: لا حاجة لتسجيل خدمات جديدة (نفس ITestCatalogService) — فقط تسجيل SpecialCharsBar/Behaviors إن لزم | 16، 17 | بسيطة |
| 24 | اختبار End-to-End يدوي | Verification | — | تنفيذ السيناريوهات: إنشاء تحليل مع Collection/Branch/LogGroup → إضافة مكوّن → إضافة مدى مع Flags/Comments/Critical | 1-23 | متوسطة |

---

## القسم السادس: المخاطر والقرارات المعمارية

### 6-1 تغييرات قد تكسر وظائف موجودة

1. **DataGrid الأنابيب الحالي:** ‫`TestDataManagementWindow.xaml` يعتمد على DataGrid قابل للتعديل لـ TestTypeSampleTube. التحول إلى 3 ComboBox مبسّطة قد يفقد المستخدمين القدرة على إضافة أكثر من 3 أنابيب لو احتاجوا. **القرار المعماري المطلوب:** هل نسمح بإبقاء DataGrid كخيار متقدم وعرض ComboBox×3 كاختصار، أم نُجبر على 3 أنابيب فقط؟
2. **AppliesToPregnant:** تغيير الدلالة قد يغير سلوك تقارير قائمة. **القرار:** إضافة عمود جديد `is_for_pregnancy_only` بدلاً من تعديل الموجود.
3. **CollectionNotes نص حر:** البيانات القائمة قد تحتوي نصوصاً غير منظمة. **القرار:** الإبقاء على الحقل كملاحظات حرة، وإضافة CollectionTypeId جديد بجانبه (لا استبدال).

### 6-2 تغييرات تتطلب ترحيل بيانات موجودة

- **AgeUnit:** كل سجلات NormalRange القائمة ستتلقى Default = 'Days'. هذا لا يكسر شيئاً (يتطابق مع السلوك الحالي).
- **Seed Data للجداول الجديدة:** يجب أن تحتوي ترحيلات Collection/Branch/LogGroup على Seed افتراضي حتى يتسنى للسجلات القديمة الإشارة إلى قيمة افتراضية إن لزم.
- **AppliesToPregnant → IsForPregnancyOnly:** لا تحويل تلقائي مأمون، الأفضل ترك القيمة القديمة null والسماح للمستخدمين بإعادة تصنيف السجلات يدوياً.

### 6-3 قرارات معمارية يجب اتخاذها قبل التنفيذ

| القرار | الخيارات | التوصية |
|---|---|---|
| Tube عدد محدد أم مفتوح؟ | 3 ثابتة أم Collection ديناميكية | إبقاء Collection ديناميكية، عرض UI مبسّط لأول 3 ولكن مع زر "Advanced" للوصول للجدول الكامل |
| Collection: حقل واحد أم حقلان (Type + Notes)؟ | استبدال أم تجاور | تجاور: CollectionTypeId جديد + CollectionNotes الموجود للملاحظات الإضافية |
| AgeUnit: حقل واحد أم لكل من From و To؟ | موحد أم منفصل | موحد (وحدة واحدة لكامل المدى) — مطابق للنظام المرجعي |
| SpecialCharsBar: عام أم في كل نافذة؟ | UserControl مشترك أم Behavior للنوافذ | UserControl مشترك يُضمَّن في كلتا النافذتين |
| AppliesToPregnant مقابل IsForPregnancyOnly: استبدال أم تجاور؟ | استبدال أم إبقاء كلاهما | إبقاء كلاهما مرحلياً ومنعها لاحقاً عبر [Obsolete] |
| Branch / LogGroup: قابلة للتعديل من واجهة منفصلة؟ | نعم أم لا | نعم في مرحلة لاحقة، يكفي حالياً Seed افتراضي |

### 6-4 تعارضات وتناقضات محتملة

1. **TestType.SampleType / DefaultTubeType / DefaultTubeColor:** هذه الحقول الثلاثة موجودة في النموذج لكن لا تظهر في الواجهة الحالية، وتتداخل دلالياً مع TestTypeSampleTube. يجب توضيح الغرض منها: هل ستُهجر؟ أم ستُستخدم كقيم افتراضية؟
2. **TestGroup مقابل LogGroup:** الاسمان متقاربان وقد يربك المطورين. ينصح بتسمية LogGroup كـ `WorkListGroup` أو `InstrumentGroup` لتمييز واضح.
3. **`AppliesToPregnant` ثلاثي القيمة:** السلوك الحالي (null/true/false) قد يولّد بيانات غير قابلة للتنبؤ. التحول لـ bool صريح أوضح دلالياً.
4. **ResolveSchemeIdsAsync:** يلقي استثناء إذا لم تُنفّذ الترحيلات. الإضافات الجديدة (Branch/LogGroup/CollectionType) قد تحتاج نفس النمط من البذر الأولي لتجنب أخطاء وقت التشغيل.

---

## القسم السابع: الملخص التنفيذي

النظام الحالي في commit `c66f7c2` يقدم بنية MVVM ناضجة ونموذج بيانات شاملاً يغطي معظم متطلبات بيانات التحاليل، لكنه يفتقر إلى ثلاثة كيانات مرجعية أساسية (CollectionType، Branch، LogGroup) ولا يدعم أي من حقول الأعلام والتعليقات الإكلينيكية على المعدلات الطبيعية (Low/High Flag، Low/High Comment، Critical Range Text، Critical Flag، Critical Comment، Age Unit)، كما تنقصه أداة UI لإدراج الأحرف اليونانية والرموز الخاصة. أبرز التحديات هي إعادة تصميم نافذة بيانات التحاليل لإضافة قسم باركود مبسّط مع قوائم منسدلة وثلاث FKs جديدة دون كسر تكاملاتها مع TestTypeSampleTube ومنطق الأسعار، وإعادة هيكلة لوحة تفاصيل المعدلات لتفصل بصرياً بين قسم Normal وقسم Critical مع إضافة حقول Flag/Comment، وأخيراً تطوير UserControl مشترك يتعقب TextBox النشط ليبدّل الأحرف الخاصة في موضع المؤشر. حجم العمل التقديري حوالي 24 خطوة موزعة على 5 طبقات (Model/Migration/Service/ViewModel/View) مع 7 ترحيلات قاعدة بيانات صغيرة وآمنة (كلها nullable أو ذات Default). أهم التوصيات قبل البدء: (1) عدم استبدال CollectionNotes الموجود بل إضافة CollectionTypeId بجواره، (2) إضافة عمود `is_for_pregnancy_only` جديد بدل تعديل دلالة AppliesToPregnant، (3) إبقاء جدول TestTypeSampleTube ودعم 3 أنابيب من خلال UI مبسّط مع زر متقدم للوصول للجدول الكامل، (4) Seed بيانات افتراضية في كل ترحيل مرجعي حتى لا تنكسر سيناريوهات أول تشغيل.
