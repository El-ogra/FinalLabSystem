# خطة عمل تطوير MainWindow ونافذة إضافة/تعديل المرضى

المشروع: `FinalLabSystem`  
التقنيات: WPF + .NET 8 + SQL Server + MVVM + Microsoft.Extensions.DependencyInjection  
نطاق هذه الوثيقة: تحليل الفجوات ووضع خطة تنفيذ مفصلة فقط، بدون كتابة كود تنفيذي.

---

## 1. ملخص الهدف

المطلوب تطوير مرحلتين مترابطتين:

1. **MainWindow + وظيفة المرضى**
   - فتح `MainWindow` بعد تسجيل الدخول.
   - إضافة Toolbar علوي يحتوي زر "المرضى".
   - عند اختيار وظيفة المرضى تظهر أربعة اختيارات مركزية:
     - إضافة وتعديل بيانات المرضى.
     - إدخال نتائج التحاليل.
     - تسليم نتائج التحاليل.
     - البحث عن المريض.
   - عند اختيار أي مهمة، يتم إخفاء `MainWindow` وفتح نافذة المهمة عبر نظام Navigation الحالي.

2. **نافذة إضافة وتعديل بيانات المرضى**
   - تسجيل المريض دائماً مع زيارة وتحاليل ومدفوعات.
   - منع بناء God ViewModel عبر تقسيم النافذة إلى ViewModels متخصصة.
   - حفظ البيانات في المسار الصحيح:
     `Patient -> Visit -> VisitTest -> Payment / Visit totals`.

---

## 2. تحليل الفجوات

### 2.1 فجوات MainWindow وNavigation

| المجال | الوضع الحالي | الفجوة | القرار المخطط |
|---|---|---|---|
| `MainWindow.xaml` | فارغ تقريباً | لا توجد واجهة رئيسية أو منطقة محتوى | بناء Shell رئيسي بسيط يحتوي Toolbar ومنطقة مركزية |
| `MainViewModel.cs` | غير موجود | لا يوجد ViewModel يحكم الواجهة الرئيسية | إنشاء `MainViewModel` |
| Navigation | موجود عبر `OpenTaskWindow<TViewModel>()` و`ReturnToMain()` | يحتاج تسجيل النوافذ الجديدة وربط أوامر MainViewModel | استخدام النظام الحالي دون تغييره جذرياً |
| نوافذ المهام الأربعة | غير موجودة أو غير مكتملة | تحتاج Views/ViewModels أولية | إنشاء نافذة المرضى كاملة، وباقي النوافذ كمسارات Navigation قابلة للتوسع |

### 2.2 فجوات Services

| الخدمة | الفجوة | الاستخدام في نافذة المرضى |
|---|---|---|
| `IPatientService` | `GetTodayPatientsAsync`, `UpdatePatientAsync`, `GetPatientByIdAsync`, `GeneratePatientCodeAsync` | إضافة/تعديل مريض، مرضى اليوم، توليد الكود |
| `IReferralService` | `SearchReferralSourcesAsync`, `GetAllReferralSourcesAsync`, `GetReferralTitlesAsync` | البحث الديناميكي وحفظ جهة التحويل عند الطلب |
| `IVisitService` | `GetTodayVisitsWithPatientsAsync`, `UpdateVisitTestsAsync`, `GenerateVisitCodeAsync` | إنشاء الزيارة، تعديل التحاليل، قائمة مرضى اليوم |
| `IFinancialService` | `ApplyFullPaymentAsync`, `RevertPaymentAsync`, `CalculateSubtotalAsync` | الحسابات، الموافقة، التراجع |
| `ISampleTrackingService` | موجودة `GenerateBarcodesForVisitAsync` | تحتاج مراجعة توافقها مع نافذة الباركود |
| خدمة الألقاب | غير موجودة | جلب ألقاب المرضى السابقة والتحقق منها |

### 2.3 فجوات ViewModels

| ViewModel | الوضع الحالي | الفجوة |
|---|---|---|
| `MainViewModel` | غير موجود | مطلوب للتحكم في MainWindow ووظيفة المرضى |
| `PatientRegistrationViewModel` | غير موجود | مطلوب كمنسق رئيسي للنافذة |
| `PatientInfoViewModel` | غير موجود | بيانات المريض، توليد الكود، ألقاب المرضى |
| `ReferralViewModel` | غير موجود | البحث والحفظ الاختياري لجهة التحويل |
| `MedicalHistoryViewModel` | غير موجود | حالات المرض والتاريخ المرضي وبيانات الزيارة الطبية |
| `TestSelectionViewModel` | غير موجود | قائمة التحاليل، الفلاتر، الباقات، الإضافة والحذف |
| `FinancialViewModel` | غير موجود | الحسابات المالية المتزامنة والتأكيد والتراجع |
| ViewModels للنوافذ المنبثقة | غير موجودة | الباركود، الإيصال، مرضى اليوم عند الحاجة |

### 2.4 فجوات Views

| View | المطلوب |
|---|---|
| `MainWindow.xaml` | Toolbar + منطقة مركزية لخيارات وظيفة المرضى |
| `PatientRegistrationWindow.xaml` | نافذة كاملة لإضافة/تعديل المرضى |
| `PatientInfoView.xaml` | جزء بيانات المريض |
| `ReferralSectionView.xaml` | جزء جهة التحويل |
| `MedicalHistorySectionView.xaml` | جزء التاريخ المرضي والحالة |
| `TestSelectionView.xaml` | جزء اختيار التحاليل بالقائمتين |
| `FinancialSectionView.xaml` | جزء الحسابات |
| `BarcodeDialog.xaml` | نافذة منبثقة لطباعة الباركود |
| `ReceiptDialog.xaml` | نافذة منبثقة للإيصال بقالبين |

### 2.5 فجوات DI Registration

يجب تسجيل العناصر التالية في `App.xaml.cs` أو مكان تكوين الخدمات الحالي:

| النوع | عناصر التسجيل المطلوبة |
|---|---|
| Services | الخدمات الجديدة أو الدوال الموسعة ضمن الخدمات الحالية |
| ViewModels | `MainViewModel` وViewModels الخاصة بنافذة المرضى |
| Views/Windows | نوافذ المهام والمنبثقات المطلوبة |
| Navigation mappings | ربط أوامر MainViewModel بفتح النوافذ الصحيحة |

---

## 3. التقسيم النهائي المقترح للـ ViewModels

### 3.1 القرار

سيتم اعتماد تقسيم مركب، بحيث يكون `PatientRegistrationViewModel` منسقاً رئيسياً وليس حاوياً لكل المنطق.

| ViewModel | المسؤولية |
|---|---|
| `PatientRegistrationViewModel` | تنسيق الحفظ، الحالة العامة، أوامر الشريط السفلي، الربط بين الأقسام |
| `PatientInfoViewModel` | بيانات `Patient`، توليد `PatientCode`، الألقاب، التحقق من حقول المريض |
| `ReferralViewModel` | بيانات `ReferralSource`، البحث الديناميكي، حفظ جهة التحويل عند تفعيل Checkbox |
| `MedicalHistoryViewModel` | `IsFasting`, `IsPregnant`, ساعات الصيام، أنواع التاريخ المرضي، ملاحظات الزيارة |
| `TestSelectionViewModel` | تحميل التحاليل والباقات، الفلاتر، الإضافة والحذف، عداد المختار |
| `FinancialViewModel` | حساب Subtotal والخصم والمدفوع والباقي والتأكيد والتراجع |
| `PatientRegistrationState` | كائن حالة داخلي اختياري غير مرئي للواجهة لتبادل البيانات بين الأقسام |

### 3.2 مبررات القرار

- يمنع تضخم `PatientRegistrationViewModel` وتحوله إلى God ViewModel.
- يجعل كل قسم قابلاً للاختبار بمعزل عن الواجهة.
- يسمح بتطوير أجزاء النافذة تدريجياً دون كسر بقية الأقسام.
- يحافظ على MVVM الصارم: لا منطق أعمال داخل XAML أو code-behind.
- يعزل الحسابات المالية عن اختيار التحاليل، مع ربطهما عبر أحداث/خصائص واضحة.

### 3.3 حدود المسؤولية

| القاعدة | التفصيل |
|---|---|
| الحفظ النهائي | يتم فقط من `PatientRegistrationViewModel` |
| الحسابات | تتم داخل `FinancialViewModel` أو `IFinancialService` |
| تحميل التحاليل | يتم داخل `TestSelectionViewModel` عبر خدمة التحاليل |
| إنشاء Patient/Visit | يتم عبر الخدمات، وليس مباشرة داخل View |
| code-behind | يقتصر على InitializeComponent وأي ضرورة WPF بحتة غير قابلة للربط |

---

## 4. خطة العمل التفصيلية

### المرحلة 1: تثبيت العقود المعمارية ونطاق البيانات

**الهدف:** تثبيت النماذج التي ستتعامل معها النافذة وتحديد حقول الحفظ والقراءة.

| البند | التفاصيل |
|---|---|
| ملفات ستُراجع | `Models/Patient.cs`, `Models/Visit.cs`, `Models/VisitTest.cs`, `Models/Payment.cs`, `Models/ReferralSource.cs`, `Models/PatientMedicalHistory.cs`, `Models/SampleTube.cs` |
| ملفات ستُنشأ | لا شيء في هذه المرحلة |
| ملفات ستُعدّل لاحقاً | لا تعديل في مرحلة التحليل، لكن التنفيذ قد يضيف DTOs |
| الاعتماديات | ملف `Patient_Window_Context.md` وقاعدة البيانات الحالية |
| Definition of Done | توثيق خريطة الحقول بين الواجهة والجداول قبل التنفيذ |
| المخاطر | وجود حقول مطلوبة في الواجهة غير موجودة في قاعدة البيانات مثل VIP، نوع المريض، ساعات الصيام، Print على التقرير |

**قرار تصميمي مبدئي:** أي حقل غير موجود في قاعدة البيانات لا يتم إضافته مباشرة قبل موافقة صاحب المشروع. يمكن تمثيله مؤقتاً داخل ViewModel فقط إذا كان غير مطلوب للحفظ.

---

### المرحلة 2: توسعة عقود الخدمات المطلوبة

**الهدف:** إضافة الدوال الناقصة إلى Interfaces قبل بناء ViewModels.

| الملف | نوع التغيير المخطط |
|---|---|
| `Services/Interfaces/IPatientService.cs` | إضافة دوال مرضى اليوم، التعديل، الجلب بالمعرف، توليد PatientCode |
| `Services/Interfaces/IReferralService.cs` | إضافة البحث الديناميكي، كل الجهات، ألقاب الجهات |
| `Services/Interfaces/IVisitService.cs` | إضافة زيارات اليوم، تعديل تحاليل الزيارة، توليد VisitCode |
| `Services/Interfaces/IFinancialService.cs` | إضافة حساب الإجمالي، تأكيد الدفع، التراجع |
| `Services/Interfaces/ISampleTrackingService.cs` | مراجعة العقد الحالي وإضافة ما يلزم لنافذة الباركود إذا احتاجت |
| `Services/Interfaces/IPatientTitleService.cs` أو `IPatientService.cs` | قرار نهائي بشأن ألقاب المرضى |

| الاعتماديات | المرحلة 1 |
| Definition of Done | كل ViewModel يجد عقد Service واضحاً دون استعلام مباشر من DbContext |
| المخاطر | تغيير Interfaces قد يتطلب تحديث Implementations واختبارات البناء |

---

### المرحلة 3: تنفيذ توسعات الخدمات

**الهدف:** تنفيذ المنطق الخلفي الذي تعتمد عليه واجهة المرضى.

| الملف | المسؤولية |
|---|---|
| `Services/Implementations/PatientService.cs` | توليد PatientCode، تعديل المريض، قائمة مرضى اليوم |
| `Services/Implementations/ReferralService.cs` | البحث، الحفظ الاختياري، جلب الألقاب |
| `Services/Implementations/VisitService.cs` | توليد VisitCode، إنشاء/تعديل VisitTests، زيارات اليوم |
| `Services/Implementations/FinancialService.cs` | حساب الإجماليات، الخصم، الدفع، التراجع |
| `Services/Implementations/SampleTrackingService.cs` | ضمان توليد باركود مناسب للنافذة |
| `Services/Implementations/PatientTitleService.cs` | إذا تم اعتماد خدمة منفصلة للألقاب |

| الاعتماديات | المرحلة 2 |
| Definition of Done | الخدمات تبني بنجاح وتعيد بيانات كافية للـ ViewModels دون منطق UI |
| المخاطر | توليد الأكواد يجب أن يكون آمناً ضد التكرار، خصوصاً مع أكثر من مستخدم |

---

### المرحلة 4: إنشاء MainViewModel وتفعيل MainWindow

**الهدف:** تحويل `MainWindow` من نافذة فارغة إلى Shell رئيسي.

| الملف | نوع التغيير المخطط |
|---|---|
| `ViewModels/MainViewModel.cs` | إنشاء ViewModel للـ Toolbar ومنطقة المرضى |
| `MainWindow.xaml` | إضافة Toolbar علوي ومنطقة مركزية لخيارات المرضى |
| `MainWindow.xaml.cs` | إبقاؤه محدوداً قدر الإمكان |
| `App.xaml.cs` | تسجيل `MainViewModel` وربطه بالـ MainWindow |

| الاعتماديات | Navigation الحالي وDI |
| Definition of Done | بعد تسجيل الدخول تظهر MainWindow وبها زر المرضى، وعند الضغط تظهر الأزرار الأربعة |
| المخاطر | مسار فتح MainWindow بعد Login قد يحتاج مراجعة تدفق Login الحالي |

---

### المرحلة 5: تسجيل نوافذ المهام في Navigation

**الهدف:** جعل الأزرار الأربعة في MainWindow تفتح النوافذ المقابلة.

| الملف | نوع التغيير المخطط |
|---|---|
| `Infrastructure/Navigation/NavigationService.cs` | استخدام النمط الحالي لفتح النوافذ |
| `Views/Patients/PatientRegistrationWindow.xaml` | نافذة إضافة/تعديل المرضى |
| `Views/Results/ResultEntryWindow.xaml` | نافذة إدخال النتائج، قد تبدأ Placeholder وظيفي |
| `Views/Results/ResultDeliveryWindow.xaml` | نافذة تسليم النتائج، قد تبدأ Placeholder وظيفي |
| `Views/Patients/PatientSearchWindow.xaml` | نافذة البحث، قد تبدأ Placeholder وظيفي |
| `App.xaml.cs` | تسجيل النوافذ والـ ViewModels في DI |

| الاعتماديات | المرحلة 4 |
| Definition of Done | الضغط على كل زر يخفي MainWindow ويفتح النافذة المقابلة، والعودة تعمل عبر `ReturnToMain()` |
| المخاطر | إنشاء Placeholder للنوافذ الثلاث غير المحورية يحتاج موافقة على مستوى الاكتمال المطلوب |

---

### المرحلة 6: بناء ViewModels الخاصة بنافذة المرضى

**الهدف:** إنشاء الهيكل البرمجي للنافذة بدون XAML معقد في البداية.

| الملف | المسؤولية |
|---|---|
| `ViewModels/Patients/PatientRegistrationViewModel.cs` | المنسق الرئيسي والحفظ والأوامر |
| `ViewModels/Patients/PatientInfoViewModel.cs` | بيانات المريض |
| `ViewModels/Patients/ReferralViewModel.cs` | جهة التحويل |
| `ViewModels/Patients/MedicalHistoryViewModel.cs` | التاريخ المرضي والحالة |
| `ViewModels/Patients/TestSelectionViewModel.cs` | اختيار التحاليل |
| `ViewModels/Patients/FinancialViewModel.cs` | الحسابات المالية |
| `ViewModels/Patients/PatientRegistrationState.cs` | حالة مشتركة اختيارية |

| الاعتماديات | المراحل 2 و3 |
| Definition of Done | كل ViewModel مستقل، له أوامر وخصائص واضحة، ولا يوجد ViewModel واحد يحمل كل منطق النافذة |
| المخاطر | تزامن `TestSelectionViewModel` مع `FinancialViewModel` يحتاج تصميم واضح لتجنب coupling زائد |

---

### المرحلة 7: بناء واجهة نافذة إضافة/تعديل المرضى

**الهدف:** بناء XAML منظم وقابل للربط بالـ ViewModels المقسمة.

| الملف | نوع التغيير المخطط |
|---|---|
| `Views/Patients/PatientRegistrationWindow.xaml` | النافذة الرئيسية والأزرار السفلية |
| `Views/Patients/PatientInfoView.xaml` | UserControl لقسم بيانات المريض |
| `Views/Patients/ReferralSectionView.xaml` | UserControl لقسم جهة التحويل |
| `Views/Patients/MedicalHistorySectionView.xaml` | UserControl لقسم التاريخ المرضي |
| `Views/Patients/TestSelectionView.xaml` | UserControl للقائمتين |
| `Views/Patients/FinancialSectionView.xaml` | UserControl للحسابات |

| الاعتماديات | المرحلة 6 |
| Definition of Done | تظهر كل المناطق المطلوبة، وكل عنصر أساسي مربوط بخاصية أو Command |
| المخاطر | كثافة النافذة عالية؛ يجب تجنب ازدحام بصري أو اعتماد Layout هش |

---

### المرحلة 8: منطق إضافة مريض وحفظ Patient + Visit + VisitTests

**الهدف:** تنفيذ workflow الحفظ الأساسي.

| العملية | التفاصيل |
|---|---|
| إضافة مريض | توليد `PatientCode` وتفعيل الإدخال |
| حفظ | إنشاء/تحديث `Patient` ثم إنشاء `Visit` ثم إضافة `VisitTest` |
| VisitCode | توليده عبر `IVisitService` |
| Referral | حفظها فقط عند تفعيل Checkbox "حفظ جهة التحويل" |
| MedicalHistory | إنشاء سجلات `PatientMedicalHistory` عند وجود اختيارات |
| Validation | منع الحفظ بدون اسم مريض أو تحاليل مختارة أو بيانات مالية غير منطقية |

| الملفات المعنية | ViewModels المرضى + Services |
| الاعتماديات | المراحل 3 و6 و7 |
| Definition of Done | يمكن إنشاء مريض جديد مع زيارة وتحاليل وحسابات مبدئية دون كسر العلاقات |
| المخاطر | يجب أن تكون العملية Transaction واحدة حتى لا يحفظ المريض دون زيارة أو تحاليل |

---

### المرحلة 9: اختيار التحاليل والباقات والفلاتر

**الهدف:** إكمال منطق القائمتين.

| العنصر | التنفيذ المخطط |
|---|---|
| Routine Tests | تحميل `TestType` الفردية |
| Profiles | تحميل `TestProfile` وتوسيعها إلى `TestProfileItem` |
| By Group | تجميع حسب `TestGroup` |
| By Category | تجميع حسب `TestCategory` |
| البحث | فلترة نصية على اسم التحليل |
| الإضافة | DoubleClick وCommand لزر Add select test |
| الحذف | حذف المحدد أو حذف الكل |
| العدّاد | يعتمد على القائمة المختارة |

| الملفات المعنية | `TestSelectionViewModel`, `TestSelectionView.xaml`, خدمات التحاليل |
| الاعتماديات | المرحلة 6 |
| Definition of Done | كل طرق الإضافة والحذف والفلاتر تعمل وتحدث الحسابات |
| المخاطر | إضافة باقة قد تسبب تكرار تحاليل؛ يجب تحديد سياسة منع التكرار |

---

### المرحلة 10: الحسابات المالية والتزامن

**الهدف:** إكمال جزء الحسابات وربطه بالتحاليل.

| العنصر | القاعدة المخططة |
|---|---|
| Subtotal | يحسب من أسعار التحاليل المختارة |
| DiscountAmount وDiscountPercent | متزامنان في الاتجاهين |
| TotalAfterDiscount | `Subtotal - DiscountAmount` |
| TotalPaid | إدخال المستخدم |
| BalanceDue | `TotalAfterDiscount - TotalPaid` |
| الباقي للمعمل | يحتاج تعريف نهائي من صاحب المشروع |
| المدفوع سابقاً | يظهر عند تعديل زيارة موجودة |
| موافقة | تثبيت الدفع عبر `ApplyFullPaymentAsync` أو حفظ Payment |
| تراجع | إلغاء الخصم/الدفع قبل التأكيد أو عبر `RevertPaymentAsync` حسب الحالة |

| الملفات المعنية | `FinancialViewModel`, `FinancialSectionView.xaml`, `IFinancialService` |
| الاعتماديات | المرحلة 9 |
| Definition of Done | الحسابات تتحدث فورياً وتحفظ في `Visit` و/أو `Payment` حسب القواعد |
| المخاطر | تعريف "الباقي للمريض" و"الباقي للمعمل" غير واضح في قاعدة البيانات الحالية |

---

### المرحلة 11: تعديل المرضى ومرضى اليوم

**الهدف:** دعم اختيار مريض/زيارة من مرضى اليوم وتعديلها.

| العملية | التفاصيل |
|---|---|
| مرضى اليوم | تحميل زيارات اليوم مع بيانات المرضى |
| تعديل | اختيار زيارة قائمة وتعبئة كل الأقسام |
| تحديث التحاليل | استخدام `UpdateVisitTestsAsync` |
| تحديث الماليات | إعادة حساب الإجمالي عند تغيير التحاليل أو الخصم |
| حفظ التعديل | تحديث Patient وVisit وVisitTests بصورة متسقة |

| الملفات المعنية | `PatientRegistrationViewModel`, `PatientInfoViewModel`, `TestSelectionViewModel`, `FinancialViewModel` |
| الاعتماديات | المرحلة 8 |
| Definition of Done | يمكن فتح مريض اليوم وتعديل بياناته وتحاليله وحساباته |
| المخاطر | تعديل زيارة مدفوعة أو مؤكدة يحتاج قيود واضحة |

---

### المرحلة 12: الباركود والإيصال

**الهدف:** دعم النوافذ المنبثقة المطلوبة بعد حفظ الزيارة.

| الملف | المسؤولية |
|---|---|
| `Views/Patients/BarcodeDialog.xaml` | عرض/طباعة باركود الأنابيب |
| `ViewModels/Patients/BarcodeDialogViewModel.cs` | تحميل/توليد الباركود |
| `Views/Patients/ReceiptDialog.xaml` | عرض الإيصال |
| `ViewModels/Patients/ReceiptDialogViewModel.cs` | اختيار قالب بتفاصيل أو بدون تفاصيل |
| `ISampleTrackingService` | توليد Barcodes من `SampleTube` |

| الاعتماديات | المرحلة 8 |
| Definition of Done | بعد حفظ زيارة يمكن فتح باركود وإيصال مرتبطين بنفس الزيارة |
| المخاطر | الطباعة الفعلية تحتاج قراراً بشأن الطابعة والقالب ومكتبة الطباعة |

---

### المرحلة 13: ربط العودة إلى القائمة الرئيسية

**الهدف:** ضمان تدفق تنقل ثابت.

| العملية | التفاصيل |
|---|---|
| القائمة الرئيسية | Command يستدعي `ReturnToMain()` |
| إغلاق نافذة المهمة | لا يترك MainWindow مخفية |
| حالة غير محفوظة | تحذير عند الخروج إذا توجد تغييرات |

| الملفات المعنية | `PatientRegistrationViewModel`, `NavigationService`, Windows |
| الاعتماديات | المرحلة 5 |
| Definition of Done | العودة تعمل من نافذة المرضى دون فقدان نافذة MainWindow |
| المخاطر | إغلاق النافذة من زر X يحتاج معالجة موحدة |

---

### المرحلة 14: التسجيل في DI والتنظيف المعماري

**الهدف:** ضمان أن كل Dependency تأتي من Container.

| الملف | التغيير المخطط |
|---|---|
| `App.xaml.cs` | تسجيل الخدمات وViewModels وWindows |
| `FinalLabSystem.csproj` | لا يغير إلا إذا ظهرت حاجة لحزمة UI أو طباعة |

| الاعتماديات | كل المراحل السابقة |
| Definition of Done | لا توجد `new Service()` داخل ViewModels، وكل النوافذ تفتح عبر DI/Navigation |
| المخاطر | التسجيل المكرر أو lifecycle غير المناسب قد يسبب احتفاظاً بحالة قديمة |

---

### المرحلة 15: التحقق والاختبار

**الهدف:** التأكد من سلامة البناء وتدفق الاستخدام الأساسي.

| نوع التحقق | المطلوب |
|---|---|
| Build | `dotnet build` ينجح |
| Navigation smoke test | Login -> MainWindow -> المرضى -> نافذة التسجيل -> العودة |
| Save workflow | حفظ مريض + زيارة + تحاليل + ماليات |
| Edit workflow | تعديل مريض من مرضى اليوم |
| Financial workflow | الخصم، الدفع، التراجع |
| Barcode workflow | توليد/عرض باركود بعد الحفظ |

| الاعتماديات | كل مراحل التنفيذ |
| Definition of Done | لا توجد أخطاء بناء، والتدفقات الأساسية تعمل يدوياً |
| المخاطر | اختبار WPF الآلي قد لا يكون متاحاً؛ سيتم الاعتماد على build واختبار يدوي منظم إن لم توجد بنية Tests |

---

## 5. الملفات المتوقعة إنشاؤها أو تعديلها

### 5.1 ملفات جديدة متوقعة

| المسار | الغرض |
|---|---|
| `ViewModels/MainViewModel.cs` | التحكم في MainWindow |
| `ViewModels/Patients/PatientRegistrationViewModel.cs` | منسق نافذة المرضى |
| `ViewModels/Patients/PatientInfoViewModel.cs` | بيانات المريض |
| `ViewModels/Patients/ReferralViewModel.cs` | جهة التحويل |
| `ViewModels/Patients/MedicalHistoryViewModel.cs` | التاريخ المرضي |
| `ViewModels/Patients/TestSelectionViewModel.cs` | اختيار التحاليل |
| `ViewModels/Patients/FinancialViewModel.cs` | الحسابات |
| `ViewModels/Patients/BarcodeDialogViewModel.cs` | الباركود |
| `ViewModels/Patients/ReceiptDialogViewModel.cs` | الإيصال |
| `Views/Patients/PatientRegistrationWindow.xaml` | نافذة المرضى |
| `Views/Patients/PatientInfoView.xaml` | قسم بيانات المريض |
| `Views/Patients/ReferralSectionView.xaml` | قسم جهة التحويل |
| `Views/Patients/MedicalHistorySectionView.xaml` | قسم التاريخ المرضي |
| `Views/Patients/TestSelectionView.xaml` | قسم التحاليل |
| `Views/Patients/FinancialSectionView.xaml` | قسم الحسابات |
| `Views/Patients/BarcodeDialog.xaml` | نافذة الباركود |
| `Views/Patients/ReceiptDialog.xaml` | نافذة الإيصال |
| `Services/Interfaces/IPatientTitleService.cs` | إذا تم اعتماد خدمة مستقلة للألقاب |
| `Services/Implementations/PatientTitleService.cs` | تنفيذ خدمة الألقاب |

### 5.2 ملفات متوقع تعديلها

| المسار | سبب التعديل |
|---|---|
| `MainWindow.xaml` | بناء الواجهة الرئيسية |
| `MainWindow.xaml.cs` | ربط محدود إن لزم |
| `App.xaml.cs` | تسجيل DI والنوافذ والـ ViewModels |
| `Infrastructure/Navigation/NavigationService.cs` | فقط إذا احتاج النظام الحالي دعماً إضافياً |
| `Services/Interfaces/IPatientService.cs` | إضافة دوال ناقصة |
| `Services/Implementations/PatientService.cs` | تنفيذ الدوال |
| `Services/Interfaces/IReferralService.cs` | إضافة دوال ناقصة |
| `Services/Implementations/ReferralService.cs` | تنفيذ الدوال |
| `Services/Interfaces/IVisitService.cs` | إضافة دوال ناقصة |
| `Services/Implementations/VisitService.cs` | تنفيذ الدوال |
| `Services/Interfaces/IFinancialService.cs` | إضافة دوال ناقصة |
| `Services/Implementations/FinancialService.cs` | تنفيذ الدوال |
| `Services/Interfaces/ISampleTrackingService.cs` | مراجعة توافق الباركود |
| `Services/Implementations/SampleTrackingService.cs` | تعديل محدود إذا لزم |

---

## 6. مخاطر عامة وقرارات تصميمية مهمة

| الخطر | الأثر | التخفيف |
|---|---|---|
| حقول غير موجودة في قاعدة البيانات | لا يمكن حفظها فعلياً | طلب موافقة قبل Migration أو حفظها في Notes مؤقتاً |
| توليد الأكواد | احتمال التكرار | توليد داخل Service مع تحقق Unique |
| حفظ جزئي | Patient بدون Visit أو Visit بدون Tests | استخدام Transaction |
| نافذة كبيرة جداً | صعوبة استخدام وصيانة | تقسيم UserControls وViewModels |
| ربط ViewModels الفرعية | Coupling زائد | منسق رئيسي + حالة مشتركة محدودة |
| الدفع والتراجع | قواعد غير واضحة بعد التأكيد | توثيق قواعد الدفع قبل التنفيذ |
| الباقات والتكرار | إضافة نفس التحليل أكثر من مرة | سياسة منع التكرار افتراضياً |

---

## 7. قرارات تحتاج موافقة

1. **Lab.ID**
   - هل المقصود به `PatientId` الداخلي؟
   - أم `PatientCode`؟
   - أم رقم آخر يجب إضافته؟

2. **VIP ونوع المريض**
   - لا تظهر كحقول واضحة في نموذج `Patient` الحالي.
   - هل نضيف أعمدة جديدة بترحيل قاعدة بيانات، أم نؤجلها، أم تحفظ داخل `Notes`؟

3. **اسم المسؤول / الطبيب في جهة التحويل**
   - `ReferralSource` الحالي يحتوي `SourceName` ولا يظهر حقل مستقل واضح للمسؤول.
   - هل يجب إضافة حقل جديد أم استخدام `SourceName`/`Notes`؟

4. **Checkbox Print لجهة التحويل**
   - لا يوجد حقل واضح في قاعدة البيانات.
   - هل المطلوب حفظه لكل زيارة، أم مجرد خيار طباعة مؤقت؟

5. **عدد ساعات الصيام**
   - لا يوجد حقل واضح في `Visit`.
   - هل يضاف كعمود، أم يحفظ في `Visit.Notes`، أم يبقى UI-only؟

6. **نوع العينة المختار في النافذة**
   - `SampleType` موجود على مستوى `TestType`.
   - هل يسمح المستخدم بتحديد نوع عينة عام للزيارة، أم يعتمد النظام على نوع عينة كل تحليل؟

7. **الباقي للمريض والباقي للمعمل**
   - `Visit` يحتوي `BalanceDue` فقط.
   - يلزم تعريف محاسبي دقيق قبل التنفيذ.

8. **نافذة إدخال النتائج وتسليم النتائج والبحث**
   - هل المطلوب في هذه الدفعة إنشاء نوافذ Placeholder فقط للتنقل؟
   - أم تنفيذها وظيفياً بالكامل لاحقاً؟

9. **تعديل زيارة تم تأكيد دفعها**
   - هل يسمح بتعديل التحاليل والخصم بعد الدفع؟
   - ما القيود المطلوبة؟

10. **الطباعة**
    - هل توجد طابعة باركود محددة؟
    - هل توجد قوالب إيصال مطلوبة؟
    - هل الطباعة Preview أولاً أم طباعة مباشرة؟

11. **خدمة ألقاب المرضى**
    - هل نعتمد خدمة مستقلة `IPatientTitleService`؟
    - أم ندمجها في `IPatientService` لتقليل عدد الخدمات؟

---

## 8. ترتيب التنفيذ الموصى به

1. تثبيت قرارات الحقول غير الموجودة في قاعدة البيانات.
2. توسعة Interfaces.
3. تنفيذ Services مع Transactions.
4. بناء MainWindow وMainViewModel.
5. تسجيل Navigation للنوافذ.
6. بناء ViewModels المقسمة لنافذة المرضى.
7. بناء XAML مقسم إلى UserControls.
8. تنفيذ حفظ Patient + Visit + VisitTests + Financials.
9. تنفيذ تعديل مرضى اليوم.
10. تنفيذ الباركود والإيصال.
11. إجراء build واختبار يدوي منظم.

---

## 9. معيار اكتمال الميزة كاملة

تعد الميزة مكتملة عندما يتحقق التالي:

- تسجيل الدخول يفتح `MainWindow`.
- زر "المرضى" يعرض الخيارات الأربعة.
- زر "إضافة وتعديل بيانات المرضى" يفتح النافذة ويخفي MainWindow.
- يمكن إضافة مريض جديد مع زيارة وتحاليل وحسابات.
- يمكن اختيار تحاليل فردية وباقات وحذفها.
- يتم حساب الإجمالي والخصم والمدفوع والباقي تلقائياً.
- يمكن حفظ السجل كوحدة واحدة دون حفظ جزئي.
- يمكن تعديل مريض/زيارة من مرضى اليوم.
- يمكن فتح نافذة الباركود ونافذة الإيصال بعد الحفظ.
- يمكن العودة إلى MainWindow.
- المشروع يبني بنجاح.

