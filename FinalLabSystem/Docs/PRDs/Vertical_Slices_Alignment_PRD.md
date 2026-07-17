# وثيقة الشرائح التنفيذية العمودية لمطابقة الوظائف الأربع مع النظام المرجعي

**المستودع**: `https://github.com/El-ogra/El-ogra/FinalLabSystem`
**الفرع**: `before-prd`
**الهاش**: `de8b9b9731fd15dbc285f4b4ab861dccbe503cbd`
**الحالة**: وثيقة تخطيط فقط — لا يتم تعديل أي كود.

**استثناء ثابت**: باركود المريض في FinalLabSystem = 12 خانة (وليس 13). قرار معماري مؤكد ولا يُدرج في أي شريحة.

**النظام المرجعي**: Real Lab System v1.0 (ملف: `Reference_system_2077930761065820160.md`).
**التحليل المصدر**: `وثيقة المتطلبات .md` — تحليل طبقي خماسي.

**قراءة كل شريحة**:
كل شريحة تحدد ما يطلبه المرجع، ثم الوضع الفعلي (بالملف ورقم السطر ومقتطف الكود)، ثم التغيير المطلوب مقسّماً على الطبقات فقط عند الحاجة، ثم ما إذا كانت تحتاج Migration وحجم التغيير والاعتماديات.

---

## القسم الأول: إضافة بيانات مريض جديد

### الشريحة 1.1: ربط `BillingType` بالواجهة (Individual / Lab to Lab / Free)

**المرجع يطلب:**
في الخانة (3) من نافذة بيانات المرضى، اختيار نظام الحساب للمريض من ثلاث قيم: **Individual** (يحسب بأسعار مرضى المعمل)، **Lab to Lab** (يحسب بأسعار الجهة المُحالة إذا وُجدت)، **Free** (بالمجان — الإجمالي = صفر). الاختيار يجب أن يتوافق مع جهة الإحالة ويؤثر على الحساب فوراً.

**الوضع الحالي في الكود:**
- الـ enum `BillingType` مُعرَّف: `Models/Enums/BillingType.cs` — القيم `Individual = 0, LabToLab = 1, Free = 2`.
- العمود موجود في قاعدة البيانات وله قيمة افتراضية `Individual` — `Data/FinalLabDbContext.cs:2012-2015`:
  ```csharp
  entity.Property(e => e.BillingType)
      .HasConversion(v => v.ToString(), v => (BillingType)Enum.Parse(typeof(BillingType), v ?? "Individual", true))
      .HasDefaultValue(BillingType.Individual)
  ```
- الخدمة `VisitService` تستخدمه بالفعل عند الحفظ وحساب الأسعار: `Services/Implementations/VisitService.cs:78-79, 155-156, 621-707` (دالة `ResolveBasePriceForBilling`).
- **الفجوة**: في `ViewModels/Patients/PatientRegistrationViewModel.cs` أسطر 345-386 يتم تكوين كائن `Visit` بالكامل بدون تعيين `BillingType` إطلاقاً — تبقى القيمة الافتراضية `Individual` دائماً بغض النظر عن اختيار المستخدم. `grep -rn BillingType ViewModels/ Views/` لا يعيد أي نتيجة، أي أن `BillingType` غير مربوط بأي عنصر واجهة.

**التغيير المطلوب:**
- طبقة ViewModel: في `ViewModels/Patients/PatientInfoViewModel.cs` — إضافة خاصية `BillingType SelectedBillingType` (قيمة افتراضية `BillingType.Individual`) وقائمة `BillingTypes` من قيم الـ enum لعرضها كـ RadioButtons. تنقيح `LoadPatient(...)` و `ClearAllFields()` لضبط/إعادة تعيين القيمة.
- طبقة ViewModel: في `ViewModels/Patients/PatientRegistrationViewModel.cs` عند تكوين `visit` (السطر ~345) — إضافة `BillingType = PatientInfo.SelectedBillingType`. أيضاً استدعاء إعادة الحساب عند تغيّر القيمة (استدعاء `CalculateAsync` من خلال ربط `PropertyChanged` أو `Command` مباشر).
- طبقة View/XAML: في `Views/Patients/PatientInfoView.xaml` — استبدال أو توسعة الخلية `Grid.Row="2" Grid.Column="3" Grid.ColumnSpan="2"` (ComboBox `PatientType` الحالي) لإضافة صف/خانة جديدة أو StackPanel أفقي بثلاثة RadioButtons: `Individual` / `Lab to Lab` / `Free` مربوطة بـ `PatientInfo.SelectedBillingType` (عبر `IsChecked={Binding SelectedBillingType, Converter=EnumBooleanConverter, ConverterParameter=Individual}` أو مماثل).
- طبقة Service: `ResolveBasePriceForBilling` جاهزة — لا تغيير.

**يتطلب Migration؟** لا — العمود موجود بالفعل.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 1.2: اختيار "اللقب" التلقائي بناءً على الجنس

**المرجع يطلب:**
في الخانة (2): "ادخل اسم المريض ثم بناءً على اختيارك لجنس المريض سيظهر لك اللقب الذي يتوافق معه" — أي أن اختيار Sex = M/F يحدد تلقائياً لقباً متوافقاً (السيد/السيدة، الطفل/الطفلة، Mr/Mrs …)، مع إبقاء الحرية للمستخدم في تعديله.

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/PatientInfoViewModel.cs:78-91` تم ربط setter `Sex` بإطلاق `OnPropertyChanged` على `IsMale/IsFemale/IsUnknownSex` فقط — لا يوجد أي تعديل لخاصية `Title`.
- خاصية `Title` (السطر 62-66) تُحدَّث يدوياً فقط من الواجهة.
- الخدمة `PatientService.GetPatientTitlesAsync` (`Services/Implementations/PatientService.cs:119-127`) تجمع الألقاب من جدول `Patients` بدون تصنيف بحسب الجنس:
  ```csharp
  return await _context.Patients
      .Where(p => p.Title != null && p.Title != "")
      .Select(p => p.Title!)
      .Distinct().OrderBy(t => t).ToListAsync();
  ```
- في `Views/Patients/PatientInfoView.xaml:80` الـ ComboBox `Title` يقرأ من `TitleSuggestions` بدون فلترة.

**التغيير المطلوب:**
- طبقة Service: في `Services/Interfaces/IPatientService.cs` و `Services/Implementations/PatientService.cs` إضافة دالة `Task<List<string>> GetPatientTitlesBySexAsync(string sex)` تُرجِع الألقاب المناسبة لجنس محدد، بالإضافة إلى ثابت افتراضي لكل جنس (مثلاً: M → "السيد"، F → "السيدة"، U → "").
- طبقة ViewModel: في `PatientInfoViewModel.cs` — إضافة قاموس/دالة `SuggestTitleForSex(string sex) → string` تُعيد اللقب الافتراضي؛ داخل setter `Sex` (السطر 82) استدعاء الدالة وتحديث `Title` فقط إذا كان فارغاً أو مساوياً للقب افتراضي سابق (حتى لا يتم استبدال قيمة أدخلها المستخدم يدوياً). كذلك إعادة تحميل `TitleSuggestions` وفق الجنس عبر استدعاء الدالة الجديدة `GetPatientTitlesBySexAsync`.
- طبقة View/XAML: لا تغيير في `PatientInfoView.xaml` — الـ ComboBox الحالي كافٍ (سيقرأ من نفس `TitleSuggestions` المحدّثة).

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 1.3: تخزين "بدون جهة" تلقائياً عند عدم إدخال جهة إحالة

**المرجع يطلب:**
الخانة (5): "في حالة عدم إدخال اسم جهة إحالة للمريض سيسجلها النظام تلقائياً (بدون جهة)" — أي أن الزيارة يجب أن ترتبط بجهة افتراضية اسمها "بدون جهة" (أو "منزلي" حسب سياق المعمل) عند ترك حقل الجهة فارغاً.

**الوضع الحالي في الكود:**
- في `Services/Implementations/VisitService.cs:113-160` (دالة `SavePatientVisitAsync`) عندما تكون `referralToSave` = null، يتم إسقاط الشرط بالكامل، ولا يُعيَّن `visit.ReferralId` (يبقى null):
  ```csharp
  if (referralToSave is not null) {
      _context.ReferralSources.Add(referralToSave);
      await _context.SaveChangesAsync();
      visit.ReferralId = referralToSave.ReferralId;
  }
  ```
- في `ViewModels/Patients/PatientRegistrationViewModel.cs:373`: `ReferralId = Referral.SelectedReferral?.ReferralId` يبقى null إذا لم يختر المستخدم شيئاً.

**التغيير المطلوب:**
- طبقة Service: في `Services/Implementations/ReferralService.cs` (و/أو `SeedData`) — ضمان وجود صف افتراضي في `ReferralSources` بـ `SourceName = "بدون جهة"` و `ReferringEntityCategory` مناسب و `IsActive = true`. إضافة دالة `Task<ReferralSource> GetOrCreateDefaultReferralAsync()` في `IReferralService` تُنشئه إذا لم يوجد وتُعيد الكائن.
- طبقة Service: في `VisitService.SavePatientVisitAsync` — قبل التحقق من `referralToSave`، إذا كان `visit.ReferralId == null && referralToSave == null` استدعاء `_referralService.GetOrCreateDefaultReferralAsync()` وتعيين `visit.ReferralId` بمعرّفها.
- طبقة ViewModel: `PatientRegistrationViewModel.SaveAsync` لا تحتاج تغييراً — منطق الافتراضي يُنفَّذ داخل الخدمة.
- طبقة View/XAML: لا تغيير.
- طبقة Seed / Data: يمكن إضافة صف افتراضي في `Docs/SeedData` أو Migration Seed منفصلة، لكن الأنظف تركه اعتمادياً على `GetOrCreateDefaultReferralAsync` (Idempotent).

**يتطلب Migration؟** لا — الصف يُنشأ عند التشغيل من خلال الخدمة (Idempotent). اختياري: Data Migration منفصلة تُدرج الصف مرة واحدة.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 1.4: تطبيع النص العربي (إلغاء الهمزات وتحويل ة → ه) عند حفظ الاسم

**المرجع يطلب:**
تحت الخانة (2) بوضوح: "يُفضّل إلغاء أي همزة موجودة في الاسم (أحمد = احمد، إبراهيم = ابراهيم، علاء = علاء)" و "يُفضّل استبدال التاء المربوطة بهاء (مروة = مروه، هبة = هبه، سامية = ساميه)" و "يُفضّل استخدام حرف الياء في وسط الكلمة (علي وليس على، يحيى وليس يحيي)". هذا شرط بحث/توحيد للأسماء العربية.

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/PatientInfoViewModel.cs:244-263` الدالة `ToPatient()` تعتمد على `FullNameAr.Trim()` فقط — لا يوجد أي تطبيع للنص.
- لا توجد دالة `NormalizeArabic` في `Services/` أو `Infrastructure/` (نتيجة `grep -rn "NormalizeArabic" .` فارغة).
- الحقل `Patient.FullNameAr` (`Models/Patient.cs`) لا يمر بأي معالجة.

**التغيير المطلوب:**
- طبقة Infrastructure: إنشاء `Infrastructure/Text/ArabicTextNormalizer.cs` (ملف ثابت) بدالة `public static string Normalize(string input)` تُطبق:
  - إزالة الهمزات: `أ إ آ → ا`، `ؤ → و`، `ئ → ي`.
  - تحويل `ة → ه`.
  - تحويل `ى → ي`.
  - تنظيف المسافات (`Trim` + استبدال المسافات المتعددة بواحدة).
- طبقة ViewModel/Model: في `PatientInfoViewModel.ToPatient()` استدعاء `ArabicTextNormalizer.Normalize(FullNameAr)` بدل `Trim()`. يمكن أيضاً تطبيق نفس التطبيع على `Title`.
- طبقة View/XAML: لا تغيير مطلوب. اختيارياً، إضافة تلميح `ToolTip` على حقل الاسم يشرح قواعد التطبيع.

**يتطلب Migration؟** لا. اختياري: Data Migration لتطبيع الأسماء الموجودة، لكنه خارج نطاق هذه الشريحة (بيانات موجودة قد تحتاج مراجعة يدوية).

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 1.5: تحقق دلالي من قيمة السن (كسر عشري وحدود منطقية)

**المرجع يطلب:**
في الخانة (2): "في حالة أصغر من شهر يُكتب بالأيام من 1 إلى 29 يوم"، "في حالة أصغر من سنة يُكتب بالأشهر من 1 إلى 11 شهر"، "يمكن كتابة كسر في خانة السن أي 2.5 سنة معناها سنتين وستة أشهر"، و"يجب إدخال سن المريض لظهور المعدل الطبيعي".

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/PatientInfoViewModel.cs:135-145`:
  ```csharp
  public int? ApproxAge {
      get => _approxAge;
      set => SetProperty(ref _approxAge, value);
  }
  public string ApproxAgeUnit { ... }
  ```
  الخاصية `int?` — أي لا تدعم `2.5`. لا يوجد validation على القيمة (`HasErrors` في السطر 183 تتحقق فقط من الاسم والجنس).
- الحقل في الواجهة `Views/Patients/PatientInfoView.xaml:83`:
  ```xaml
  <TextBox Grid.Row="2" Grid.Column="0" Text="{Binding ApproxAge, UpdateSourceTrigger=PropertyChanged}" ... />
  ```
  ينتظر قيمة يمكن تحويلها لعدد صحيح — الإدخال `2.5` سيسبب فشل ربط صامت.

**التغيير المطلوب:**
- طبقة Model: `Models/Patient.cs` — الحقل `ApproxAge` يبقى كما هو (int?) لكن نقدّم منطقاً يفصل الجزء الصحيح والكسري:
  - إذا الوحدة = `Years` والمدخل = `2.5` → تخزين `ApproxAge = 2` و `ApproxAgeUnit = "Years"` مع حساب مساعد شهور، **أو** — الأفضل — تحويل داخلي إلى شهور (30 شهر) وتخزينها كـ `ApproxAge = 30, ApproxAgeUnit = "Months"` عند الحفظ (اختيار قرار الفريق). في وثيقة الفجوات هذا يظل مكافئاً منطقياً.
- طبقة ViewModel: في `PatientInfoViewModel` — استبدال `int?` بـ `decimal? ApproxAgeValue` (خاصية مرئية للواجهة) مع دالة `NormalizeAgeToStorage()` تنتج زوج `(int?, string)` مطابق لبنية DB. تحديث `ToPatient` و `LoadPatient` بحسب ذلك. توسيع `HasErrors` لتشمل:
  - `ApproxAgeUnit == "Years"` → قيمة > 0.
  - `ApproxAgeUnit == "Months"` → 1..11.
  - `ApproxAgeUnit == "Days"` → 1..29.
  - قيمة null أو 0 → رفع `HasErrors` (المرجع يطلب أن الحساب المرجعي يتطلب سناً).
- طبقة View/XAML: في `Views/Patients/PatientInfoView.xaml:83` تغيير `Text="{Binding ApproxAge}"` إلى `Text="{Binding ApproxAgeValue, StringFormat=N2, UpdateSourceTrigger=LostFocus, ValidatesOnDataErrors=True}"` مع إضافة `ErrorTemplate` بسيط.
- طبقة Service: `PricingService`/`RoutineResultService` يعتمدون على `ApproxAge` كوحدات مخزنة — يجب مراجعة أن التحويل الداخلي متسق (المعدلات الطبيعية تعتمد على العمر بالأيام في `ResultEntryViewModel._patientAgeDays`).

**يتطلب Migration؟** لا — العمود يبقى `int?`. أي تحويل داخلي يتم عبر التطبيق.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 1.6: إعادة تنسيق الصف الأول في `PatientInfoView` (كود المريض / Lab ID / VIP)

**المرجع يطلب:**
في الصورة المرجعية الأولى: السطر العلوي يعرض بالترتيب (من اليمين لليسار): **كود المريض** (خانة أولى بارزة)، ثم حقل **Lab ID** (كود المعمل الدائم — يقرأ باركود عودة المريض في زياراته اللاحقة كما تصفه الخانة 14)، ثم **VIP**. الفكرة أن Lab ID مستقل عن VisitId (الأول = هوية دائمة، الثاني = رقم الزيارة الحالية).

**الوضع الحالي في الكود:**
- `Views/Patients/PatientInfoView.xaml:68-73`:
  ```xaml
  <TextBox Grid.Row="0" Grid.Column="0" Text="{Binding PatientCode, Mode=OneWay}" IsReadOnly="True" .../>
  <Label  Grid.Row="0" Grid.Column="1" Content="كود المريض" .../>
  <TextBox Grid.Row="0" Grid.Column="2" Text="{Binding DataContext.CurrentVisitId, RelativeSource=..., Mode=OneWay}" IsReadOnly="True" .../>
  <Label  Grid.Row="0" Grid.Column="3" Content="Lab. ID" .../>
  <CheckBox Grid.Row="0" Grid.Column="4" IsChecked="{Binding IsVip}" ... />
  <Label  Grid.Row="0" Grid.Column="5" Content="VIP" ... />
  ```
  الحقل تحت تسمية "Lab. ID" مربوط في الواقع بـ `CurrentVisitId` (رقم الزيارة) — ليس بكود المعمل الدائم (`Patient.LabId`) الذي أضافه Migration `20260704095114_AddPatientBarcodeAndLabId`.
- خاصية `Patient.LabId` موجودة (Model + DB) لكنها غير مربوطة بأي عنصر واجهة في `PatientInfoView.xaml`. ملاحظة: الزر "طباعة Lab ID" في `PatientRegistrationWindow.xaml:96` مربوط بأمر منفصل `PrintLabIdCommand`.

**التغيير المطلوب:**
- طبقة ViewModel: في `PatientInfoViewModel` — إضافة خاصية `string? LabId` (تُملأ من `Patient.LabId` في `LoadPatient(Patient/VisitFullDto)` وتُنسخ إلى `ToPatient()`). خاصية قراءة/كتابة (لأن الخانة 14 تسمح بالبحث بلصق باركود Lab ID عند العودة).
- طبقة View/XAML: تحديث `Views/Patients/PatientInfoView.xaml:70` — تغيير الـ TextBox المسمى "Lab. ID" ليربط `Text="{Binding LabId, UpdateSourceTrigger=PropertyChanged}"` بدل `CurrentVisitId`. إذا احتاج الفريق عرض رقم الزيارة، نُضيف خانة منفصلة (مثلاً في صف آخر أو في شريط أعلى).
- طبقة Service: عند إدخال قيمة يدوياً في `LabId` (لصق باركود قديم) — يجب استدعاء دالة بحث في `PatientService` تُحمّل بيانات المريض المرتبط بهذا `LabId`. توسيع `IPatientService` بدالة `Task<Patient?> GetByLabIdAsync(string labId)`.
- طبقة ViewModel: في `PatientRegistrationViewModel` — الاستماع لتغيّر `PatientInfo.LabId` عندما يكون النموذج في وضع "إضافة" وطول القيمة = 12، ثم استدعاء `GetByLabIdAsync` وتحميل بيانات المريض.

**يتطلب Migration؟** لا — العمود موجود.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 1.7: تفعيل `F1` لعرض بيانات التحليل المحدد (بدلاً من AddNew)

**المرجع يطلب:**
في الاختصارات (تحت الخانة 14): **F1** عند وضع المؤشر في قائمة التحاليل يعرض بيانات التحليل المحدد أسفل القائمة (كما في الشكل الثاني للنافذة المرجعية).

**الوضع الحالي في الكود:**
- في `Views/Patients/PatientRegistrationWindow.xaml:164`:
  ```xaml
  <KeyBinding Key="F1"  Command="{Binding AddNewCommand}"/>
  ```
  F1 مربوط بأمر إضافة مريض جديد — ليس بعرض تفاصيل التحليل.
- في `TestSelectionViewModel.cs` لا يوجد أمر `ShowTestDetailsCommand` أو ما شابه.

**التغيير المطلوب:**
- طبقة ViewModel: في `ViewModels/Patients/TestSelectionViewModel.cs` — إضافة أمر `ShowTestDetailsCommand` (نوع `IRelayCommand`) وخاصية `SelectedTestDetails` (نص أو DTO) تُعرض عندما يتم استدعاء الأمر على `SelectedAvailableTest` أو `SelectedTest`.
- طبقة View/XAML: في `Views/Patients/TestSelectionView.xaml` — إضافة `Border` أسفل قائمة التحاليل (`Grid.Row="2"` أو صف جديد) يعرض `SelectedTestDetails` مع `Visibility` مرتبطة بحالة العرض.
- طبقة View/XAML: في `Views/Patients/PatientRegistrationWindow.xaml:164` — تغيير `Key="F1"` إلى استدعاء `TestSelection.ShowTestDetailsCommand` عبر `Binding` نسبي. `AddNewCommand` ينتقل إلى اختصار آخر (المرجع يخصص F2 لإضافة مريض عالمياً — والزر "إضافة" في الشريط السفلي يبقى كما هو).
- طبقة Service: `TestTypeService` (يجب تأكيد وجوده) — الاعتماد على البيانات المحمّلة سلفاً في `AvailableTests` كافٍ، دون استدعاء إضافي.

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 1.8: إضافة إعداد "طباعة الوصل تلقائياً بعد الحفظ" وإعداد "إظهار تفصيل التحاليل في الوصل"

**المرجع يطلب:**
في الفقرة قبل الاختصارات (النقطة قبل قائمة F1-F12): "يمكنك طباعة إيصال للمريض … و اختيار ظهور تفصيل التحاليل وأسعارها في الوصل وطباعة الوصل تلقائياً بعد حفظ المريض أم عند الضغط على زر إيصال — وذلك من نافذة الإعدادات من زر إعدادات".

**الوضع الحالي في الكود:**
- الزر "الإيصال" موجود: `Views/Patients/PatientRegistrationWindow.xaml:94` مع `ReceiptCommand`.
- الخدمة `Services/Implementations/ReceiptService.cs` موجودة (نتيجة `ls`).
- **الفجوة**: لا يوجد إعدادات قابلة للتكوين لسلوك الوصل. `Infrastructure/Settings/` يحتوي إعدادات عامة لكن لا يوجد `AutoPrintReceiptAfterSave` أو `ShowTestBreakdownInReceipt`. `Models/LabSetting.cs` لا يحتوي هذين الحقلين (يمكن التحقق بـ `grep -n "AutoPrint\|ShowTestBreakdown" Models/LabSetting.cs` — نتيجة فارغة).

**التغيير المطلوب:**
- طبقة DB: توسيع `Models/LabSetting.cs` بحقلين: `bool AutoPrintReceiptAfterSave` (افتراضي false)، `bool ShowTestBreakdownInReceipt` (افتراضي true). Migration جديدة `AddReceiptPreferencesToLabSettings`.
- طبقة Service: في `ReceiptService` — قراءة الحقلين قبل الطباعة؛ إذا `ShowTestBreakdownInReceipt=false` طي جدول التحاليل في قالب الطباعة.
- طبقة ViewModel: في `PatientRegistrationViewModel.SaveAsync` (بعد `_dialogService.ShowMessage("تم حفظ ...")` بالسطر ~404) — قراءة `AutoPrintReceiptAfterSave` من `LabSetting`؛ إذا `true` استدعاء `ReceiptCommand.Execute(null)`.
- طبقة View/XAML: في نافذة الإعدادات (`Views/Settings/`) — إضافة CheckBox للخيارين. خارج نطاق `PatientRegistrationWindow.xaml`.

**يتطلب Migration؟** نعم — Migration جديدة تضيف عمودين `bool NOT NULL DEFAULT` على `LabSettings`.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

## القسم الثاني: طباعة الباركود للمريض

### الشريحة 2.1: توحيد `BarcodeDialog` في شاشة واحدة (بدل ثلاث تبويبات)

**المرجع يطلب:**
الصورة المرجعية للباركود: **شاشة واحدة** تعرض في نفس النافذة: مصفوفة ملصقات الأنابيب (حسب نوع العينة والمادة)، بالإضافة إلى بطاقات كود الحالة/الملف/المعمل، وزر "طباعة كل الأكواد" البرتقالي في الأسفل. الأزرار في الصورة موزعة على أقسام مرقّمة (1) كود الملف، (2) كود المعمل، (3) ملصقات الأنابيب، (4) باركود إضافي، (5) منزلقات الطباعة، (6) سلة مهملات.

**الوضع الحالي في الكود:**
- `Views/Patients/BarcodeDialog.xaml:15-176`: `<TabControl>` بثلاث `<TabItem>`:
  - "ملصقات الأنابيب" (السطر 17).
  - "كود الحالة + كود الملف" (السطر 60).
  - "كود المعمل (Lab ID)" (السطر 135).
- زر "طباعة الكل" (السطر 12) موجود لكنه يطبع فقط `Labels` (الأنابيب) — ليس كل الأكواد الثلاثة معاً.

**التغيير المطلوب:**
- طبقة View/XAML: إعادة كتابة `Views/Patients/BarcodeDialog.xaml` باستبدال `TabControl` بـ `Grid` أو `ScrollViewer` رأسي واحد يحتوي على أربعة أقسام مرتبة عمودياً:
  1. مصفوفة `Labels` (Tubes) — `WrapPanel`.
  2. `CaseLabels` + `FileLabels` — `WrapPanel` مشترك أو صفَّان.
  3. `LabIdLabels` — `WrapPanel`.
  4. زر "طباعة كل الأكواد" — يشمل الجميع.
- طبقة ViewModel: `BarcodeDialogViewModel` — تعديل `PrintAllAsync` (السطر 124-131) بحيث يجمع `Labels + CaseLabels + FileLabels + LabIdLabels` (بعد تحويل الأخيرة عبر `ToBarcodeLabel()`) ويطبعها دفعة واحدة.
- طبقة Service: `WpfLabelPrintService.PrintLabelsAsync` تقبل بالفعل `IEnumerable<BarcodeLabel>` — لا تغيير.

**يتطلب Migration؟** لا.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 2.2: قسم "باركود إضافي" لحالات التبرع بالدم/التمييز

**المرجع يطلب:**
الخانة (4): "باركود إضافي وذلك لمثل حالات التبرع بالدم أو وجود تحليل عينة تود كتابة لها تمييز آخر — تكتب الاسم أو التحاليل المطلوبة في الخانة الأولى فوق زر الأسهم ثم اضغط انتر أو زر الأسهم واكتب الذي يليها، وهكذا في الخانة التي أسفل الأسهم فيكتب بها اسم الملصق (وصف العينة أو المادة المسحوب عليها)".

**الوضع الحالي في الكود:**
- لا يوجد قسم لباركود إضافي في `Views/Patients/BarcodeDialog.xaml`.
- `BarcodeDialogViewModel.cs` لا يحتوي `ExtraLabels: ObservableCollection<ExtraBarcodeLabel>` ولا أمر `AddExtraLabelCommand` (نتيجة تفحّص كامل للملف: الأوامر الموجودة فقط `PrintBarcodeCommand`, `PrintAllCommand`, `PrintCaseLabelsCommand`, `PrintFileLabelsCommand`, `PrintLabIdCommand`).

**التغيير المطلوب:**
- طبقة Model/DTO: إنشاء `record ExtraBarcodeLabel(string Header, string Description, string BarcodePayload)` في `ViewModels/Patients/BarcodeDialogViewModel.cs` أو ملف منفصل `Models/DTOs/ExtraBarcodeLabel.cs`.
- طبقة ViewModel: في `BarcodeDialogViewModel` — إضافة `ObservableCollection<ExtraBarcodeLabel> ExtraLabels` وأوامر `AddExtraLabelCommand`، `RemoveExtraLabelCommand`، `PrintExtraLabelCommand`. توليد باركود مؤقت (مثلاً `EXT-{timestamp}-{seq}`) عبر `IBarcodeGenerator` (يمكن إضافة دالة `GenerateExtraCodeAsync` جديدة تعيد سلسلة بلا حفظ في `PatientBarcodes`).
- طبقة View/XAML: في القسم الجديد الموحّد من `BarcodeDialog.xaml` (بعد الشريحة 2.1) — إضافة قسم "باركود إضافي" يتضمن:
  - قائمة `ItemsControl` مربوطة بـ `ExtraLabels` تعرض كل عنصر مع حقلي `Header` و `Description` وصورة الباركود.
  - أزرار "إضافة"/"حذف"/"طباعة" على كل عنصر.
  - حقلا نص فارغان أعلى القائمة (Header + Description) وزر "+ إضافة".
- طبقة DB: خيار — تخزين هذه الباركودات في جدول `PatientBarcode` بنوع جديد `BarcodeCodeType.Extra`. **قرار**: عدم التخزين (المرجع لا يوضح أنها دائمة، ويمكن اعتبارها طباعة عارضة).
- طبقة Service: `WpfLabelPrintService` يقبل `BarcodeLabel` — إضافة دالة `PrintExtraLabelAsync(ExtraBarcodeLabel)` أو تحويل `ExtraBarcodeLabel → BarcodeLabel` قبل الطباعة.

**يتطلب Migration؟** لا — الباركود الإضافي مؤقت وغير مخزن.

**حجم التغيير:** متوسط

**اعتماديات:** الشريحة 2.1 (يجب أن يكون الحاوي الموحّد جاهزاً أولاً).

---

### الشريحة 2.3: دعم السحب والإفلات (Drag & Drop) على الملصقات

**المرجع يطلب:**
الخانة (6): "في حالة وجود تحليل على ملصق أو في الجزء رقم (4) يمكنك بخاصية السحب والإفلات حذفه — وذلك بسحب العنصر فوق سلة المهملات وتركه"، وأيضاً "يمكن نقل تحليل من ملصق إلى آخر عن طريق خاصية السحب والإفلات (مثلاً يوجد تحليل مصل صائم ويوجد تحليل فيروسات مصل — ممكن ضم الاثنين على ملصق واحد حسب الرغبة)".

**الوضع الحالي في الكود:**
- في `Views/Patients/BarcodeDialog.xaml:26-33`: على كل `Border` للملصق يوجد فقط `MouseBinding` بـ `LeftDoubleClick` يستدعي `PrintBarcodeCommand`. لا يوجد `AllowDrop="True"` ولا `PreviewMouseLeftButtonDown` مع `DragDrop.DoDragDrop`.
- `BarcodeDialogViewModel.cs` لا يحتوي أوامر `MoveTestBetweenLabelsCommand` أو `RemoveTestFromLabelCommand` (grep على كامل الملف يؤكد).
- `SampleTube.VisitTests` (Many-to-many) موجود في النموذج ويسمح بإضافة/حذف `VisitTest` من `SampleTube`، لكن لا يوجد UI أو خدمة لتعديل هذه العلاقة تفاعلياً.

**التغيير المطلوب:**
- طبقة Service: في `Services/Interfaces/ISampleTrackingService.cs` و `Services/Implementations/SampleTrackingService.cs` — إضافة:
  - `Task MoveTestToTubeAsync(int visitTestId, int destinationTubeId)` — يزيل `VisitTest` من الأنبوب الحالي ويضيفه للأنبوب المقصود، مع إعادة توليد `BarcodeValue` إذا لزم.
  - `Task RemoveTestFromTubeAsync(int visitTestId)` — يحذف الارتباط (أو يُشار إلى الإلغاء بحقل `IsCancelled` في `TestWorkflow`).
- طبقة ViewModel: في `BarcodeDialogViewModel` — إضافة أوامر `MoveTestCommand(int visitTestId, int destTubeId)` و `RemoveTestCommand(int visitTestId)`. إعادة تحميل الملصقات (`LoadTubesAsync`) بعد كل عملية.
- طبقة View/XAML: في `Views/Patients/BarcodeDialog.xaml`:
  - جعل كل `Border` للملصق يحمل `AllowDrop="True"` مع `Drop` event handler.
  - إضافة `PreviewMouseLeftButtonDown` على العناصر التي تعرض `TestCodesLine` (تجزئتها لعناصر فرعية Chip قابلة للسحب).
  - إضافة عنصر "سلة مهملات" مرئي (`Image` أو `Border` بأيقونة) مع `AllowDrop="True"`.
- طبقة Code-behind: قد يحتاج `BarcodeDialog.xaml.cs` لكود مبسّط يمرّر أحداث السحب/الإفلات إلى ViewModel (Attached behavior أو DragDrop helper).

**يتطلب Migration؟** لا.

**حجم التغيير:** متوسط

**اعتماديات:** الشريحة 2.1.

---

### الشريحة 2.4: منزلقان أفقي/رأسي لضبط موضع الطباعة + زر "حفظ" للأبعاد

**المرجع يطلب:**
الخانة (5): "يمكن من خلال المنزلقين الأفقي والرأسي ترحيل و**تحريك** مكان طباعة الباركود على الملصق — اطبع أي ملصق وبعد الوصول للأبعاد المطلوبة اضغط زر حفظ".

**الوضع الحالي في الكود:**
- `Infrastructure/Barcoding/BarcodeFormatOptions.cs` يحتوي أبعاد ثابتة (`LabelWidthMm=38, LabelHeightMm=25`، ذكر في التحليل التفصيلي).
- لا يوجد `Slider` في `Views/Patients/BarcodeDialog.xaml` (grep على "Slider" في الملف = 0).
- لا يوجد `LabelPrintOffset` أو ما شابه في `Models/LabSetting.cs`.

**التغيير المطلوب:**
- طبقة DB: توسيع `Models/LabSetting.cs` بحقلين: `double LabelPrintOffsetXmm` (افتراضي 0)، `double LabelPrintOffsetYmm` (افتراضي 0). Migration جديدة `AddLabelPrintOffsetToLabSettings`.
- طبقة Service: في `WpfLabelPrintService` — قراءة `LabelPrintOffsetXmm/Ymm` وتطبيقها على إحداثيات الطباعة (`FixedPage.SetLeft` / `FixedPage.SetTop` مضاف إليها الإزاحة).
- طبقة ViewModel: في `BarcodeDialogViewModel` — خاصيتان `double PreviewOffsetX/Y` مربوطتان بمنزلقات، وأمر `SavePrintOffsetsCommand` يحفظ القيمتين في `LabSetting`.
- طبقة View/XAML: في `Views/Patients/BarcodeDialog.xaml` — إضافة قسم في الأسفل يحتوي:
  - `<Slider Minimum="-20" Maximum="20" Value="{Binding PreviewOffsetX}" .../>` أفقي.
  - `<Slider Minimum="-20" Maximum="20" Value="{Binding PreviewOffsetY}" Orientation="Vertical" .../>` رأسي.
  - `<Button Content="حفظ الأبعاد" Command="{Binding SavePrintOffsetsCommand}"/>`.
- طبقة View/XAML: تطبيق `TranslateTransform` على قالب معاينة الملصق لعرض الإزاحة مباشرةً.

**يتطلب Migration؟** نعم — Migration `AddLabelPrintOffsetToLabSettings` تضيف عمودَي `float NOT NULL DEFAULT 0` على `LabSettings`.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 2.5: CheckBox "طباعة كود المعمل مع كل الأكواد" بجوار زر "طباعة الكل"

**المرجع يطلب:**
الخانة (1) في نص المرجع: "ويمكنك طباعته مع الكل بالضغط على زر طباعة كل الأكواد (البرتقالي اللون) وذلك في حالة اختيار **طباعة كود الملك مع الكل**" — أي `CheckBox` قابل للتفعيل يجعل زر "طباعة كل الأكواد" يشمل كود المعمل الدائم.

**الوضع الحالي في الكود:**
- `Views/Patients/BarcodeDialog.xaml:12-14`:
  ```xaml
  <Button DockPanel.Dock="Bottom" Content="طباعة الكل"
          Command="{Binding PrintAllCommand}" Height="36" .../>
  ```
  لا يوجد CheckBox مجاور.
- `BarcodeDialogViewModel.PrintAllAsync` (السطر 124-131) يطبع فقط `Labels` (الأنابيب).

**التغيير المطلوب:**
- طبقة ViewModel: في `BarcodeDialogViewModel` — إضافة خاصية `bool IncludeLabIdInPrintAll` (افتراضي `false` أو من `LabSetting`). تعديل `PrintAllAsync` بحيث إذا `IncludeLabIdInPrintAll = true` يضيف `LabIdLabels.Select(l => l.ToBarcodeLabel())` إلى مجموعة الطباعة (بالإضافة إلى `CaseLabels` و `FileLabels` بعد تنفيذ الشريحة 2.1).
- طبقة View/XAML: بجوار زر "طباعة الكل" — إضافة:
  ```xaml
  <CheckBox Content="طباعة كود المعمل مع الكل"
            IsChecked="{Binding IncludeLabIdInPrintAll}"
            Margin="8,0" VerticalAlignment="Center"/>
  ```
  وتلوين الزر باللون البرتقالي (`Background="#F58220"`).

**يتطلب Migration؟** لا (اختياري إذا أُريد حفظ التفضيل — يمكن إضافته في نفس Migration الشريحة 1.8).

**حجم التغيير:** صغير جداً

**اعتماديات:** الشريحة 2.1 (منطق `PrintAllAsync` الموسّع).

---

## القسم الثالث: الوصول إلى نوافذ إدخال نتائج التحاليل

### الشريحة 3.1: فلاتر حالات النتائج (غير مكتوبة / غير مراجعة / غير مطبوعة) كأزرار مرئية

**المرجع يطلب:**
الجزء (7) من نافذة النتائج: "يمكنك الوصول بسهولة لباقي النتائج التي لم تُكتب أو باقي النتائج التي لم تراجَع أو تطبع، وأيضاً الوصول بسهولة للمرضى المهمين أو المستقلين أو مرضى المعامل والجهات الأخرى".

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/TestResultsViewModel.cs:168`: توجد خاصية `VisitDisplayStatus? FilterMode` مع أمر `ApplyFilter` (السطر 462-468) الذي يقبل `string statusStr` ويحوّله لـ enum.
- في `Views/Patients/TestResultsWindow.xaml:234-278`: أزرار الفلاتر الموجودة تُمثّل **نوع المريض** فقط (`Individual/LabToLak/Contracts/VIP/Free/الكل`) مربوطة بـ `SetPatientTypeCommand` — لا توجد أزرار مربوطة بـ `ApplyFilter` (فلاتر حالة النتائج).
- الـ `FilterMode` فعّال في `FilterPatient` (السطر 451-452) لكنه غير قابل للتحكم من الواجهة الحالية.

**التغيير المطلوب:**
- طبقة View/XAML: في `Views/Patients/TestResultsWindow.xaml:234` — إضافة صف/UniformGrid آخر أعلى أو أسفل أزرار "نوع المريض" الحالية يحتوي على ستة أزرار مربوطة بأمر `ApplyFilterCommand` (يجب فحصه — على الأرجح مربوط بالخاصية `ApplyFilter` — نتيجة `grep` تظهر `ApplyFilter` كدالة داخلية فقط، يحتاج تصديره كـ ICommand):
  - "غير مكتوبة" → `CommandParameter="ResultsNotWritten"`
  - "غير مراجعة" → `"ResultsNotReviewed"`
  - "غير مطبوعة" → `"ResultsNotPrinted"`
  - "لم تُسلَّم" → `"NotDelivered"`
  - "له باقي" → `"DeliveredWithBalance"`
  - "الكل" → مسح الفلتر (`ClearFilterCommand`، السطر 96 موجود).
- طبقة ViewModel: في `TestResultsViewModel.cs` — تصدير `ApplyFilterCommand` (نوع `IRelayCommand<string>`) إذا كان الدالة `ApplyFilter` غير مصدَّرة كأمر. إضافة خاصية `VisitDisplayStatus? SelectedFilter` لعرض حالة الاختيار (لتمييز الزر النشط بصرياً).
- طبقة View/XAML: تلوين الزر النشط باستخدام `StringEqualsConverter` أو `EnumBooleanConverter` مماثل لما هو مستخدم في الفلاتر الحالية (السطر 239).

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 3.2: أسهم التوسعة الزرقاء للتنقل بين الأيام

**المرجع يطلب:**
الجزء (7) وأيضاً في نهاية الفقرة: "ومن خلال سهمي التوسعة (الأزرق) يمكنك الوصول إلى مرضى يوم محدد عن طريق إدخال تاريخ ذلك اليوم (9)".

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/TestResultsViewModel.cs:283`: `NavigateDayCommand` موجود، ودالة `NavigateDayAsync` (السطر 470-477) تقبل `int days` وتضيفها لـ `SelectedDate` ثم تُعيد التحميل.
- في `Views/Patients/TestResultsWindow.xaml`: لا يوجد أي `<Button>` مربوط بـ `NavigateDayCommand`. لا يوجد `<DatePicker>` مربوط بـ `SelectedDate` (grep على "SelectedDate" في XAML = 0).

**التغيير المطلوب:**
- طبقة View/XAML: في `Views/Patients/TestResultsWindow.xaml` — إضافة شريط علوي أو مجاور للعنوان يحتوي على:
  ```xaml
  <StackPanel Orientation="Horizontal" FlowDirection="LeftToRight">
      <Button Content="◀" Command="{Binding NavigateDayCommand}" CommandParameter="-1"
              Background="#1976D2" Foreground="White" Width="30" Height="26"/>
      <DatePicker SelectedDate="{Binding SelectedDate}" Margin="4,0"/>
      <Button Content="▶" Command="{Binding NavigateDayCommand}" CommandParameter="1"
              Background="#1976D2" Foreground="White" Width="30" Height="26"/>
  </StackPanel>
  ```
- طبقة ViewModel: `NavigateDayAsync` تقبل `object? parameter` وتتحقق من `is int` — يجب تعديلها لتقبل `string` أيضاً وتحوّل عبر `int.TryParse` (لأن `CommandParameter` من XAML يمرَّر كـ string).
  ملف: `TestResultsViewModel.cs:472`. تعديل نصف سطر.

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير جداً

**اعتماديات:** لا يوجد

---

### الشريحة 3.3: نافذة "ملاحظات" منفصلة بدلاً من `MessageBox` مُنشأ ديناميكياً

**المرجع يطلب:**
الجزء (4): "يمكن أيضاً إضافة ملاحظة لباقي الأطباء أو للعاملين في الاستقبال عن هذا المريض وتعديل الملاحظة الموجودة بالضغط على زر ملاحظات". السياق يوحي بنافذة مخصصة مع منطقة نص واسعة وأزرار (ليس مربّع إدخال بسيط).

**الوضع الحالي في الكود:**
- في `ViewModels/Patients/TestResultsViewModel.cs:739-793`: دالة `AddEditNotes()` تُنشئ `Window` يدوياً في الكود عبر `ShowInputDialog`. النافذة بحجم 400×180 بمربع نص بسيط.
- لا يوجد `Views/Patients/PatientNotesDialog.xaml` مخصص.

**التغيير المطلوب:**
- طبقة View/XAML: إنشاء `Views/Patients/PatientNotesDialog.xaml` (Window + code-behind) بتصميم موافق لبقية النوافذ في التطبيق (`FlowDirection="RightToLeft"`, ألوان النظام). يحتوي:
  - عنوان "ملاحظات المريض".
  - `TextBox` كبير `AcceptsReturn` مع `MinHeight="200"`.
  - أزرار "حفظ" (`IsDefault`) و "إلغاء" (`IsCancel`).
- طبقة ViewModel: إنشاء `ViewModels/Patients/PatientNotesDialogViewModel.cs` بسيط (خاصية `Notes` وأمرا `SaveCommand`/`CancelCommand`).
- طبقة Service: في `Services/Implementations/DialogService.cs` — إضافة `string? ShowPatientNotesDialog(string current, string patientName)` تستدعي النافذة الجديدة وتُرجع النص المُعدَّل.
- طبقة ViewModel: في `TestResultsViewModel.AddEditNotes()` (السطر 739) — استبدال `ShowInputDialog(...)` بـ `_dialogService.ShowPatientNotesDialog(CurrentPatientInfo.VisitNotes ?? "", CurrentPatientInfo.FullNameAr)`. حذف `ShowInputDialog` (السطر 751-793) إن لم تُستخدم في مكان آخر.
- طبقة Service: إذا اقتضى: حفظ الملاحظات في `Visit.Notes` عبر خدمة جديدة `UpdateVisitNotesAsync(int visitId, string notes)` في `IVisitService`.

**يتطلب Migration؟** لا — الحقل `Visit.Notes` موجود.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 3.4: توسيع البحث ليشمل `LabId` و `FileCode` (كود المعمل + كود الملف)

**المرجع يطلب:**
الجزء (8): "من خلال مربع النص هذا يمكنك الوصول إلى أي مريض من خلال الكود الخاص به أو كود المعمل أو كود الفيل [الملف] وأيضاً ممكن البحث بالاسم في مرضى اليوم المحدد فقط وكذلك رقم الحضور في هذا اليوم مثلاً (5) 12 هكذا".

**الوضع الحالي في الكود:**
- في `TestResultsViewModel.FilterPatient` (السطر 435-455) — البحث النصي يقبل:
  ```csharp
  !patient.FullNameAr.Contains(term...) &&
  !patient.PatientCode.Contains(term...) &&
  !(patient.VisitCode?.Contains(term...) == true) &&
  !(int.TryParse(term, out var attNum) && patient.AttendanceNumber == attNum)
  ```
  أي البحث يشمل الاسم و PatientCode و VisitCode و AttendanceNumber، **لكن لا يشمل LabId** (الحقل الدائم) ولا **FileCode** (كود الملف من `PatientBarcode.BarcodeValue` عند `CodeType = File`).

**التغيير المطلوب:**
- طبقة Model/DTO: في `Models/DTOs/TodayPatientWithStatusDto.cs` — إضافة حقول `string? LabId` و `string? FileCode` (يُملآن من `Patient.LabId` والباركود ذي نوع File المرتبط بالزيارة).
- طبقة Service: في `Services/Implementations/VisitService.cs` — دالة `GetTodayPatientsWithStatusAsync` (السطر ~478+) تُضاف إليها استعلام مُدمج لملء `LabId` و `FileCode`.
- طبقة ViewModel: في `TestResultsViewModel.FilterPatient` (السطر 440-443) — إضافة شرطي بحث:
  ```csharp
  !(patient.LabId?.Contains(term, ...) == true) &&
  !(patient.FileCode?.Contains(term, ...) == true)
  ```
- طبقة View/XAML: لا تغيير.

**يتطلب Migration؟** لا — الحقول موجودة في DB (Patient.LabId + PatientBarcode.BarcodeValue).

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 3.5: ربط اختصارات `F8` (مراجعة) و `F9` (تمت) و `F12` (طبعت) بلوحة المفاتيح صراحةً

**المرجع يطلب:**
الاختصارات في أسفل النافذة المرجعية:
- **F8**: اعتبار النتائج مراجَعة أو غير مراجَعة.
- **F9**: اعتبار "تمت" (Manual complete) أو "لم تتم".
- **F12**: اعتبار "طبعت" أو "لم تطبع".

**الوضع الحالي في الكود:**
- في `Views/Patients/TestResultsWindow.xaml:833-848` (كتلة `Window.InputBindings`):
  ```xaml
  <KeyBinding Key="F8" Command="{Binding EditSelectedPatientCommand}"/>
  <KeyBinding Key="F12" Command="{Binding PrintReceiptCommand}"/>
  <KeyBinding Key="R" Modifiers="Ctrl" Command="{Binding ToggleReviewStatusCommand}"/>
  <KeyBinding Key="F" Modifiers="Ctrl" Command="{Binding ToggleFinishStatusCommand}"/>
  <KeyBinding Key="P" Modifiers="Ctrl" Command="{Binding TogglePrintStatusCommand}"/>
  ```
  الأوامر مكافئة موجودة (`ToggleReviewStatusCommand`, `ToggleFinishStatusCommand`, `TogglePrintStatusCommand` في `TestResultsViewModel.cs:122, 302, 307, 300`) لكنها مربوطة بـ `Ctrl+R/F/P` — وليس F8/F9/F12 كما في المرجع. `F8` مربوط بأمر مختلف (`EditSelectedPatientCommand`) و `F12` مربوط بـ `PrintReceiptCommand`.

**التغيير المطلوب:**
- طبقة View/XAML: في `Views/Patients/TestResultsWindow.xaml:840-844` — إعادة ربط:
  ```xaml
  <KeyBinding Key="F8"  Command="{Binding ToggleReviewStatusCommand}"/>
  <KeyBinding Key="F9"  Command="{Binding ToggleFinishStatusCommand}"/>
  <KeyBinding Key="F12" Command="{Binding TogglePrintStatusCommand}"/>
  ```
  إبقاء اختصار Ctrl+R/Ctrl+F/Ctrl+P خيارياً (كاختصار إضافي).
- **قرار مهم**: `EditSelectedPatientCommand` (المربوط حالياً بـ F8) و `PrintReceiptCommand` (F12) — يجب نقلهما إلى اختصار آخر أو ترك F8/F12 حصراً لأوامر المراجعة/الطباعة كما في المرجع. المرجع في نافذة **بيانات المرضى** يخصص F8 لتعديل البيانات و F12 للإيصال — لكن في نافذة **النتائج** يخصصهما لمراجعة/طبع. سياقياً هذا صحيح.
- طبقة ViewModel: لا تغيير.

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير جداً

**اعتماديات:** لا يوجد

---

## القسم الرابع: إدخال نتائج التحاليل

### الشريحة 4.1: زر "تعليم" (البرتقالي) لاختيار تعليقات محفوظة من قاموس

**المرجع يطلب:**
الجزء (4): "المكان المخصص للتعليق ويمكنك إدخال التعليق بالكتابة أو **اختياره من التعليقات المحفوظة في القاموس** عن طريق الضغط على زر تعليم (البرتقالي) ستظهر قائمة بها التعليقات — بالضغط على التعليم المراد يتم إدخاله في مكان التعليمات. ويمكنك أيضاً إدخال التعليم المخزن من لائحة المعدلات الطبيعية بالضغط على اسم التحليل، ويوجد زر للتراجع عن آخر تعليم تم إضافته".

**الوضع الحالي في الكود:**
- في `Views/Patients/ResultEntryWindow.xaml:19-35`: قسم "Comment" موجود — لكنه مجرد `TextBox` لـ `SelectedComponent.Comment` بدون أي زر لاختيار من قاموس.
- الجدول `ReportCommentTemplate` موجود في النموذج (`Models/ReportCommentTemplate.cs`) والخدمة `IReportCommentTemplateService` موجودة (`Services/Implementations/ReportCommentTemplateService.cs`) — لكنها غير مستخدمة في `ResultEntryViewModel`.
- في `ViewModels/Patients/ResultEntryViewModel.cs:32-68` (constructor) و بقية الملف: لا يوجد `LoadCommentTemplatesAsync` ولا `PickCommentCommand` ولا `UndoLastCommentCommand`.

**التغيير المطلوب:**
- طبقة ViewModel: في `ResultEntryViewModel.cs`:
  - إضافة `ObservableCollection<ReportCommentTemplate> CommentTemplates`.
  - في constructor استقبال `IReportCommentTemplateService` عبر DI (وتحديث `DefaultResultEditorFactory.cs` بحسب ذلك).
  - إضافة أوامر `LoadCommentTemplatesCommand`, `PickCommentTemplateCommand(ReportCommentTemplate)`, `UndoLastCommentCommand`.
  - إبقاء stack بسيط `Stack<string> _commentHistory` لدعم التراجع.
- طبقة View/XAML: في `Views/Patients/ResultEntryWindow.xaml` — بجوار `TextBox` Comment (السطر 28) إضافة:
  ```xaml
  <Button Content="تعليم" Background="#F58220" Foreground="White"
          Command="{Binding OpenCommentPickerCommand}"/>
  <Button Content="تراجع" Command="{Binding UndoLastCommentCommand}"/>
  ```
- طبقة View/XAML: إنشاء `Popup` أو `ContextMenu` مربوط بـ `CommentTemplates` يعرضها كقائمة قابلة للاختيار (`ListBox` بـ `MouseBinding`).
- طبقة Service: `ReportCommentTemplateService` يجب أن يُصفّي القوالب حسب `TestTypeId` (يمكن قراءة `SelectedComponent.TestTypeId` أو `VisitTestId`).

**يتطلب Migration؟** لا — الجدول والخدمة موجودان.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 4.2: مكان استعراض النتائج المُطبَّعة يجعلها للقراءة فقط (Read-only Highlight)

**المرجع يطلب:**
الجزء (3): "التحاليل التي لم تُطبع عليها علامة صح، أما التي طُبعت فستكون **مظللة بالأحمر ويجب مراجعة النتائج حتى يمكن من طبعها أو مشاهدتها**". الفكرة: بعد الطباعة، الصف لا يمكن تعديله إلا بعد إعادة المراجعة.

**الوضع الحالي في الكود:**
- في `Views/Patients/ResultEntryWindow.xaml:129-135` عمود "Print" مع `CheckBox IsChecked="{Binding IsPrintEnabled, Mode=TwoWay}"` — قابل للتحرير من المستخدم ولا يعكس حالة "طُبع في الواقع".
- في `ViewModels/Patients/ResultEntryViewModel.cs`: لا يوجد شرط يجعل الصف read-only بعد الطباعة. `SaveAsync` (السطر 171-205) يقبل حفظ أي نتيجة سواء طُبعت أم لا.
- الحقل `IsPrinted` موجود على `VisitTest` (وليس على `TestComponentResultDto`) — يحتاج تمريره إلى `TestComponentResultDto` أو تحقق مسبق في ViewModel.

**التغيير المطلوب:**
- طبقة Model/DTO: في `Models/DTOs/TestComponentResultDto.cs` — إضافة `bool IsResultLocked` (محسوبة من `VisitTest.IsPrinted && !HasPendingReview`).
- طبقة ViewModel: في `ResultEntryViewModel` — عند تحميل `Components` في constructor (السطر 62-67) ملء `IsResultLocked` لكل عنصر.
- طبقة View/XAML: في `Views/Patients/ResultEntryWindow.xaml`:
  - عمود "النتيجة" (السطر 68-76) — إضافة `Style` على `TextBox` يجعل `IsReadOnly = True` عندما `IsResultLocked = True` مع خلفية حمراء فاتحة (`Background="#FFCDD2"`).
  - إضافة `Style` على `DataGridRow` لتلوينه أحمر خفيف عندما `IsResultLocked = True`.
- طبقة ViewModel: في `SaveAsync` (السطر 171) — تصفية `results` لتستبعد المكونات التي `IsResultLocked = true` (مع تحذير للمستخدم إذا حاول تعديل نتيجة مقفلة).

**يتطلب Migration؟** لا — الحقول المستخدمة موجودة.

**حجم التغيير:** صغير

**اعتماديات:** لا يوجد

---

### الشريحة 4.3: زر "معاينة الطباعة" و زر "تاريخ مرضي" داخل نافذة إدخال النتائج

**المرجع يطلب:**
الجزء (5) — الأزرار بالترتيب: (١) حفظ، (٢) طباعة، (٣) **معاينة الطباعة**، (٤) **تاريخ مرضي** (يفتح نافذة التاريخ المرضي)، (٥) رجوع، (٦) القائمة الرئيسية.

**الوضع الحالي في الكود:**
- في `Views/Patients/ResultEntryWindow.xaml:37-48`: الأزرار الموجودة فقط:
  ```xaml
  <Button Content="حفظ و مراجعة" .../>
  <Button Content="حفظ" .../>
  <Button Content="إلغاء" IsCancel="True"/>
  ```
  لا يوجد "طباعة" ولا "معاينة" ولا "تاريخ مرضي" ولا "القائمة الرئيسية".
- في `ViewModels/Patients/ResultEntryViewModel.cs`: الأوامر الموجودة `SaveCommand`, `SaveAndReviewCommand`, `CancelCommand` فقط.

**التغيير المطلوب:**
- طبقة ViewModel: في `ResultEntryViewModel.cs`:
  - إضافة `PrintCommand` (يستدعي `_routineResultService` + `IPrintService`).
  - إضافة `PreviewPrintCommand` (يفتح نافذة `PrintPreviewWindow` بمعاينة تقرير هذا التحليل).
  - إضافة `OpenMedicalHistoryCommand` (يفتح نافذة/dialog لعرض تاريخ نفس البروفايل للمريض — يعتمد على `VPatientHistory` أو `IReportingService.GetHistoricalComparisonsAsync`).
  - إضافة `ReturnToMainCommand` (إغلاق النافذة والعودة إلى `MainWindow`).
- طبقة Service: `IReportingService.GetHistoricalComparisonsAsync` موجودة (مذكورة في وثيقة المتطلبات) — استخدامها في `OpenMedicalHistoryCommand`.
- طبقة View/XAML: في `Views/Patients/ResultEntryWindow.xaml:37` — توسيع `StackPanel` بأزرار الأوامر الجديدة بالترتيب الصحيح (Save, Print, Preview, Medical History, Cancel/Back, Main Menu) مع ألوان مطابقة لبقية النظام.

**يتطلب Migration؟** لا.

**حجم التغيير:** متوسط

**اعتماديات:** لا يوجد

---

### الشريحة 4.4: ربط اختصارات `F8` / `F11` / `F12` داخل `ResultEntryWindow`

**المرجع يطلب:**
اختصارات لوحة المفاتيح في نافذة إدخال النتائج:
- **F8**: اعتبار تحاليل المريض المختارة تمت مراجعتها.
- **F9**: فقط النتائج والتعديلات (تحديد ما سيُحفظ).
- **F11**: معاينة طباعة تقرير النتائج المُدخَلة.
- **F12**: طباعة تقرير النتائج المراد طباعتها والمدخل نتائجها والتي تمت مراجعتها.
- **Enter**: الانتقال إلى خانة النتيجة التالية، وفي آخر خانة يحفظ وينتقل إلى نافذة نتائج التحاليل.
- **Up/Down**: التنقل بين خانات النتائج.
- **Esc**: العودة لقائمة نتائج التحاليل دون حفظ.

**الوضع الحالي في الكود:**
- `Views/Patients/ResultEntryWindow.xaml:1-11` (رأس النافذة) — لا يوجد `Window.InputBindings` (grep على `InputBindings` في الملف = 0). فقط زر "إلغاء" لديه `IsCancel="True"` (يعمل مع Esc افتراضياً).
- Enter/Up/Down داخل الـ `DataGrid` يعمل بسلوك افتراضي لكن لا يوجد ربط صريح مع الأوامر.

**التغيير المطلوب:**
- طبقة View/XAML: في نهاية `Views/Patients/ResultEntryWindow.xaml` قبل `</Window>` — إضافة:
  ```xaml
  <Window.InputBindings>
      <KeyBinding Key="F8"  Command="{Binding SaveAndReviewCommand}"/>
      <KeyBinding Key="F11" Command="{Binding PreviewPrintCommand}"/>
      <KeyBinding Key="F12" Command="{Binding PrintCommand}"/>
      <KeyBinding Key="Escape" Command="{Binding CancelCommand}"/>
  </Window.InputBindings>
  ```
- طبقة ViewModel: `SaveAndReviewCommand` جاهزة (السطر 59). `PrintCommand` و `PreviewPrintCommand` تُضاف في الشريحة 4.3.
- طبقة Code-behind (`ResultEntryWindow.xaml.cs`): معالج KeyDown على `TextBox` النتيجة داخل `DataGrid` لتحويل Enter إلى `MoveNextResult()` (يمكن تنفيذه ببرمجة تتنقل بين خلايا `DataGrid` باستخدام `DataGrid.CommitEdit()` + `DataGrid.MoveFocus()`).

**يتطلب Migration؟** لا.

**حجم التغيير:** صغير

**اعتماديات:** الشريحة 4.3 (لتوفر `PrintCommand`/`PreviewPrintCommand`).

---

## ترتيب التنفيذ الموصى به

| المجموعة | تُنفَّذ بالتوازي | الشرائح | مبرر التجميع |
|:--:|:--:|--|--|
| **A** | نعم | 1.1، 1.2، 1.4، 1.5، 1.6، 1.7 | كلها في نطاق شاشة `PatientRegistration` وطبقات مستقلة (ViewModel/View/Service محلية) بدون Migration. |
| **B** | نعم | 3.1، 3.2، 3.4، 3.5 | تحسينات UI بحتة على `TestResultsWindow` — لا Migration ولا اعتماديات. |
| **C** | نعم | 4.1، 4.2 | تعديلات مستقلة على `ResultEntryWindow` — طبقات مختلفة. |
| **D** | لا (سلسلة) | 2.1 → 2.3 و 2.5 (بالتوازي بعد 2.1) | 2.1 يعيد بناء الحاوي، 2.3 و 2.5 يعتمدان على الهيكل الجديد. |
| **E** | نعم | 2.2، 2.4 | كلاهما إضافات مستقلة على `BarcodeDialog` بعد 2.1. |
| **F** | نعم | 1.3، 3.3، 4.3 | إضافات تعتمد على خدمات جاهزة (Referral, DialogService, Reporting). |
| **G** | لا | 4.4 | يعتمد على أوامر الشريحة 4.3 (`PrintCommand`, `PreviewPrintCommand`). |
| **H** | لا | 1.8، 2.4 | تحتاجان Migration منفصلة على `LabSettings` — يُفضّل تجميعهما في Migration واحدة `AddPreferencesAndPrintOffsetToLabSettings`. |

**الترتيب الموصى به**: A + B + C + F (بالتوازي) → 2.1 → (2.3 + 2.5) و (2.2 + 2.4) → 4.3 → 4.4 → H (Migration مجمّعة).

**Migration مطلوبة**: اثنتان فقط قابلتان للدمج:
1. `AddReceiptPreferencesToLabSettings` (شريحة 1.8) — عمودان `bool`.
2. `AddLabelPrintOffsetToLabSettings` (شريحة 2.4) — عمودان `float`.
يُوصى بدمجهما في Migration واحدة: `AddLabPreferencesAndPrintOffsets`.

---

## العناصر المقبولة كما هي (لا تحتاج تنفيذاً)

| العنصر المرجعي | مصدر التصنيف في وثيقة المتطلبات | المبرر |
|--|--|--|
| PatientType موسّع (Individual/Contract/Company/Insurance) | 1.2.3 — 🟡 | التوسعة إضافية لأعمال أوسع (شركات، تعاقدات) ولا تُلغي الوظائف المرجعية؛ `Individual/LabToLab/Free` مغطى بـ `BillingType` (الشريحة 1.1). |
| مربعات "Taken outside lab" كـ CheckBoxes منفصلة بدل قائمة منسدلة | 1.3 (نقطة غامضة) | نفس الوظيفة (اختيار نوع العينة الخارجية)؛ CheckBoxes أوضح ولا يُفقد أي وظيفة. |
| فلاتر `TM/TG/CG` في `TestSelectionView` بدل ComboBox "روتيني/جميع/مجموعات" | 1.2.5 — 🟡 (10) | مكافئة وظيفياً — الفلاتر الحالية تُقدّم نفس التقسيم. |
| زر "Apply Profile" الإضافي في `TestSelectionView` | 1.2.5 — 🟡 | إضافة مفيدة لا تتعارض مع المرجع. |
| Trigger `TR_Payment_SyncBalance` | 1.2.1 — 🟡 | تحسين اتساق بيانات ولا يُغيّر السلوك المرئي. |
| أعمدة `DateOfBirth`, `BloodType`, `PhotoPath` في `Patient` | 1.2.2 — 🟡 | حقول اختيارية إضافية لا تُعرض في الشاشة الأساسية ولا تُخلّ بالمرجع. |
| `TestWorkflow` و `AuditLog` و `DeliveryConfirmation` | 3.2.1 — 🟡 | بنية تحتية للتدقيق تدعم الأزرار P/T الموجودة بالفعل — تنفذها بشكل صحيح. |
| زر "إضافة للقائمة" و "قائمة الطباعة" (Print Queue) | 3.2.5 — 🟡 | إضافات إنتاجية مفيدة، غير موجودة في المرجع لكنها لا تتعارض معه. |
| `TR_Payment_SyncBalance`, `IPrintQueueService`, `IInventoryService` | متعددة | خدمات إضافية للأمان والإنتاجية دون فقدان أي وظيفة مرجعية. |
| نافذة `CultureEntryWindow` منفصلة عن `ResultEntryWindow` للتحاليل الميكروبيولوجية | 3.2.3 | تخصص مبرَّر (ثقافات ميكروبية لها UI مختلف جوهرياً). |
| Luhn check digit في نهاية الباركود | 2.2.4 — 🟡 | تحسين تكامل بيانات؛ لا يخل بتوليد المرجع (والباركود 12 خانة مقرَّر خارج نطاق الوثيقة). |
| Weekday ordering (Sunday=1..Saturday=7) | 2.2.4 | مطابق لـ `DayOfWeek` في .NET ومعطيات الخبير. |
| حقل `Company` بأعمدة تعاقدية إضافية (DiscountRate/CreditLimit/…) | 1.2.1 — 🟡 | امتداد وظيفي مفيد لا يُنافي المرجع. |

---

## ملاحظات ختامية

1. **باركود 12 خانة**: مستثنى صراحةً من هذه الوثيقة. أي شريحة تلمّح لعدد الخانات مرفوضة.
2. **رقم الفرع**: أُزيل من `BarcodeGenerator` (القرار 23) — لا تُقترح إعادة إدخاله.
3. **تنفيذ الشرائح لا يبدأ في هذه المهمة**: هذه الوثيقة هي خطة فقط. تحوّل كل شريحة إلى Story منفصلة تحمل نفس رقمها.
4. **الاختبارات**: كل شريحة تحتاج اختبار وحدة على طبقة ViewModel وسيناريو تشغيل يدوي على طبقة View. مشروع `FinalLabSystem.Tests` هو المسار المستهدف.
