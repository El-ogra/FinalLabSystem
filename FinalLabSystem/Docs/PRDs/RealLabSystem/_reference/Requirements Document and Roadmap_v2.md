📘 وثيقة المتطلبات وخارطة الطريق — النسخة المحسّنة (v2)
مشروع FinalLabSystem — مقارنة تفصيلية مع Real Lab System
مرجع الكود: El-ogra/FinalLabSystem @ commit 2a6caa314efd5c029e4e75675dbeadfec47df7be (branch before-prd) رسالة الـ commit: "التحضير لدورة إستكمال المتطلبات الثالثة" مرجع الاستهداف: Real_Lab_System_Unified_Reference_FINAL.md (23 قسمًا وظيفيًا) — باستثناء صريح للقسم 15 (NATIGH.COM والأندرويد) حالة المستودع الحالي: 66 EF Core Migration، ~230 ملف مصدر C#/XAML، بنية MVVM ناضجة مع Dependency Injection كامل و Serilog، مشروع اختبارات مصاحب FinalLabSystem.Tests.

📋 ملخص القرارات المنتجية الـ23 المُدمجة
#	القرار	أثره الرئيسي في هذه الوثيقة
1	إلغاء قيد مسار D:\real lab system\Data	G9 — لا يظهر كقيد
2	First-Run Setup تحسين متعمد	G1 — يبقى ويُوثَّق كتحسين
3	قائمة منسدلة لأسماء المستخدمين	G1 — شريحة تعديل XAML
4	لا توحيد لنص "خادم قواعد البيانات غير معرف"	G10 — مُلغى
5	Responsive Layout بدلاً من فحص أبعاد الشاشة	G1, G10 — معيار جودة عام
6	لا Splash Screen	G1 — مُلغى
7	لا رسالة "اضغط أي مفتاح"	G10 — مُلغى
8	كلمات مرور منفصلة لكل شاشة حساسة	G2, G9 — تصميم توزيعي
9	استبدال JSON Backup بـ SQL Server BACKUP الأصلي	G9 — إعادة معمارية كبرى
10	تسمية بمنطق سياقي (اسم + تاريخ)	G9 — مرن
11	5 أعلام مستقلة → 7 حالات بصرية	G6, G7 — تصحيح جوهري
12	فصل ثلاثي: BillingType / PaymentMethod / ReferringEntityCategory + ثنائية تسعير	G5 — إعادة هيكلة
13	Max Discount Percent كضابط صلاحية	G2, G5
14	واجهة "إضافة مبلغ إضافي" صريحة	G5
15	تدفق أوضح بدل "Enter مرتين"	G5
16	لا سلوك تعديل حساب بالنقر	G5 — استبعاد
17	باركود 13 رقمًا يطابق ترتيب المرجع	G7
18	Luhn تحسين متعمد فوق المرجع	G7
19	لا حد على عدد النطاقات الطبيعية	G3 — تصحيح للوثيقة الموحدة
20	تبسيط "6 مرات" إلى نطاق واحد "أي/أي"	G3
21	TurnaroundHours بالساعات (لا تغيير)	G3
22	تسميات الحساسية الحرفية: HighlyFor/ModerateFor/LowFor/ResistantFor	G6 — تصحيح للوثيقة الموحدة
23	إزالة Multi-Branch نهائيًا	جميع المجموعات + المخاطر
G1 — الإقلاع، تسجيل الدخول، والأمان الأولي
المصادر المرجعية: [Ref/Q1, Q2, Q4] · [Learn/Ch1] · [Show] القرارات المُدمجة: 2, 3, 5, 6, 7

G1.1 حالة المطابقة الحالية
البند	المرجع	الحالة في الكود @ commit	التصنيف
نافذة تسجيل دخول عند الإقلاع	مطلوب	LoginView.xaml + LoginViewModel.cs (135 سطر) موجودة ومربوطة بـ Navigation Service	✅ مطابق بالكامل
First-Run Setup بدل admin/admin	تحسين متعمد يتجاوز المرجع (القرار 2)	FirstRunSetupViewModel.cs (202 سطر) + FirstRunSetupView.xaml — تحقق طول ≥3/6 وتأكيد كلمة المرور	✅ مطابق بالكامل (كتحسين)
القائمة المنسدلة لأسماء المستخدمين	مطلوب (القرار 3)	LoginView.xaml يستخدم TextBox نصية لاسم المستخدم، لا يوجد ComboBox يعرض Staff النشطين	⚠️ يحتاج عمل
إخفاء/إظهار كلمة المرور	ذكاء واجهة	IsPasswordVisible DataTrigger على PasswordBox/TextBox — موجود ✅	✅ مطابق بالكامل
رسالة خطأ عند فشل الاعتماد	مطلوب	StatusMessage مربوط في LoginViewModel	✅ مطابق بالكامل
فحص أبعاد الشاشة <10 بوصة	ورد بالمرجع	مُلغى بالقرار 5 — يُستبدل بـ Responsive Layout	🚫 لا يُنفَّذ
Splash Screen	لم يرد صراحة	لا يوجد في الكود ✅ متوافق مع القرار 6	✅ مطابق (بالحذف)
G1.2 معايير قبول مُحدَّثة لهذه المجموعة
[من القرار 3] عند فتح شاشة تسجيل الدخول، يجب أن يُعرض ComboBox مربوط بمصدر بيانات Staff حيث IsActive == true، مرتبًا حسب DisplayName. المستخدم يختار اسمه ثم يُدخل كلمة المرور. TextBox النصي يبقى متاحًا كخيار ثانوي لكن التركيز الافتراضي يكون على ComboBox.
[من القرار 2] لا يُعاد أبدًا إلى سلوك admin/admin الافتراضي؛ إذا كان جدول Staff فارغًا يُوجَّه المستخدم لشاشة First-Run Setup تلقائيًا (السلوك الحالي).
[من القرار 5] كل عناصر شاشة تسجيل الدخول و First-Run Setup يجب أن تُبنى في Grid/StackPanel متجاوب — لا Width/Height ثابت على الحاويات الخارجية، مع MinWidth/MinHeight معقولة وSizeToContent="WidthAndHeight" على الـ Window.
[من القرار 6] الإقلاع مباشر: App.OnStartup ينتقل فورًا إلى LoginView بدون Window وسيط أو SplashScreen.
G1.3 سيناريو اختبار يدوي
تشغيل التطبيق مباشرةً من ملف تنفيذي على شاشة FullHD ثم على شاشة 8 بوصة — يجب أن تظهر شاشة الدخول بدون أي تشويه أو قصّ في كلتيهما (Responsive).
عند وجود Staff نشط واحد على الأقل، يجب أن تظهر القائمة المنسدلة تحتوي عليه.
عند حذف كل الـ Staff، يجب أن يُعاد التوجيه لـ First-Run Setup تلقائيًا.
G2 — إدارة المستخدمين والصلاحيات
المصادر المرجعية: [Ref/Q4] · [Learn/Ch1, Ch7] القرارات المُدمجة: 8, 13

G2.1 حالة المطابقة الحالية
البند	المرجع	الحالة في الكود	التصنيف
نموذج Staff مع Hash لكلمة المرور	مطلوب	Staff.cs يحتوي PasswordHash, IsAdmin, IsActive, JobTitle...	✅ مطابق
نظام صلاحيات دقيقة	مطلوب	Permission + StaffPermission (M:N) + IsGranted, GrantedBy, GrantedAt	✅ مطابق
الحد الأقصى للخصم لكل موظف (القرار 13)	مطلوب	Staff.DiscountLimit (double) موجود ✅ لكن لا يوجد تحقق يمنع تجاوزه في PricingService/InvoiceService	⚠️ يحتاج عمل
كلمات مرور منفصلة لكل شاشة حساسة (القرار 8)	مطلوب	يجب فحص الوجود عبر LabSetting أو جداول منفصلة	⚠️ يحتاج عمل — لا يبدو مركزيًا في LabSetting.cs
Audit Log لعمليات الحسابات الحساسة	مطلوب	AuditLog + [Auditable] attribute + AuditService	✅ مطابق
G2.2 معايير قبول
[من القرار 13] عند محاولة موظف تطبيق خصم على فاتورة، إذا DiscountPercent > Staff.DiscountLimit يجب رفض العملية مع رسالة صريحة، مع تسجيل المحاولة في AuditLog. أدمن التطبيق (IsAdmin=true) معفى من هذا التحقق.
[من القرار 8] جدول SensitiveScreenPassword (أو أعمدة مستقلة في LabSetting مثل CashDrawerPassword, DbMaintenancePassword, SettingsPassword) يجب أن يوجد كأعمدة منفصلة قابلة للتغيير مستقلًا. لا يُقبل حقل مركزي واحد يُستخدم لكل الشاشات.
جميع كلمات المرور الحساسة تُخزَّن كـ Hash (نفس PasswordHasher المستخدم مع Staff، عبر Infrastructure/Security).
G2.3 سيناريو اختبار يدوي
إنشاء موظف بـ DiscountLimit = 10%، ثم محاولة تطبيق خصم 15% على فاتورة — يجب رفض العملية.
تسجيل الدخول كأدمن وتغيير كلمة مرور Cash Drawer فقط، ثم التأكد من أن كلمة مرور Database Maintenance لم تتأثر.
G3 — إدارة التحاليل والقاموس الطبي (Test Catalog)
المصادر المرجعية: [Ref/Q5, Q6] · [Learn/Ch3, Ch4] القرارات المُدمجة: 19, 20, 21

G3.1 تصحيح هام مقابل الوثيقة الموحدة
[تصحيح مبني على تحقق مستقل — القرار 19]: الوثيقة الموحدة كانت تشير ضمنيًا لحد 6 نطاقات (3 وحدات زمنية × جنسين) كقيد. هذا التفسير غير صحيح — الرقم 6 في المرجع مجرد مثال توضيحي لا قيد مفروض. النظام يجب أن يدعم عددًا غير محدود من نطاقات المعدل الطبيعي لكل مكوّن تحليل، والكود الحالي بالفعل يدعم ذلك بطبيعة علاقة TestComponent → NormalRange (one-to-many مفتوحة). ✅

G3.2 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
جدول TestType بحقول شاملة	مطلوب	TestType.cs — 30+ حقلاً بما فيها TypeCode, TypeNameEn/Ar, TypeAbbrev, DefaultPrice, SampleType, TurnaroundHours, BarcodeName, ReportNameLine1/2 وسلوك Behavior (Flags)	✅ مطابق بشكل ممتاز
وحدة زمن ظهور النتيجة بالساعات (القرار 21)	المرجع يستخدم الأيام	short TurnaroundHours — الاحتفاظ بالساعات كما هو	✅ مطابق للقرار
TestComponent منفصل عن TestType	مطلوب	TestComponent.cs موجود ✅	✅ مطابق
NormalRange بجنس/عمر/صيام/حمل	مطلوب	NormalRange.cs يحتوي Sex, AgeFromDays/AgeToDays, AgeUnit, ForPregnantOnly, FastingState, LowNormal/HighNormal, LowCritical/HighCritical, LowFlag/HighFlag/CriticalFlag, LowComment/HighComment + نظام Versioning (Version, IsActive, SupersededById)	✅ مطابق ومتفوق على المرجع
عدد النطاقات غير محدود (القرار 19)	لا يوجد حد فعلي	لا يوجد Validator يفرض حدًا ✅	✅ مطابق للقرار — يجب توثيقه صراحة كي لا يُضاف حد بالخطأ
نطاق واحد "أي جنس/أي عمر" (القرار 20)	تبسيط للنمط القديم	ممكن حاليًا بـ Sex="Any" + AgeFromDays=0, AgeToDays=int.MaxValue — لكن لا يوجد Preset/Helper رسمي لهذا النمط	⚠️ يحتاج عمل — إضافة زر "نطاق موحد لكل الفئات" في UI
TestProfile (باقات/بروفايلات)	مطلوب	TestProfile.cs + TestProfileItem.cs ✅	✅ مطابق
SampleTube / TubeMaterial	مطلوب	موجودان + TestTypeSampleTube لعلاقة M:N ✅	✅ مطابق
G3.3 معايير قبول
[من القرار 19] لا يجب أن يوجد أي Validator أو تحقق برمجي (سواء في Domain Layer أو Application Layer) يرفض تحليلاً لديه ≥6 نطاقات. يجب أن تنجح العملية مهما كان العدد.
[من القرار 20] في NormalRangesWindow (شاشة تحرير النطاقات لتحليل مركّب) يجب إضافة زر واضح "نطاق موحد لجميع الفئات" — عند الضغط عليه ينشئ صفًا واحدًا فقط بقيم Sex=Any, AgeFromDays=0, AgeToDays=int.MaxValue, FastingState=Any, ForPregnantOnly=false ويرفض إضافة صفوف إضافية إلا بعد إلغاء هذا الوضع.
[من القرار 21] حقل زمن الجاهزية في كل شاشات الإدخال والعرض والتقارير يجب أن يظل بالساعات (TurnaroundHours) — لا تحويل تلقائي لأيام.
الـ Versioning الحالي على NormalRange (Immutable + SupersededById) يبقى — لا تعديل مباشر لصف قائم.
G3.4 سيناريو اختبار يدوي
إنشاء تحليل مركّب بـ 15 نطاقًا طبيعيًا لمكوّن واحد — يجب أن ينجح الحفظ بدون أي رفض.
إنشاء تحليل بسيط لا يتأثر بالجنس والعمر، الضغط على "نطاق موحد لجميع الفئات" — يجب إنشاء صف واحد وإخفاء أزرار إضافة نطاق حتى إلغاء الوضع.
تحرير نطاق قائم — يجب أن يُنشأ صف جديد بـ Version+1 ويُشار للصف القديم عبر SupersededById، مع بقاء الصف القديم موجودًا بـ IsActive=false.
G4 — إدارة المرضى والاستقبال (Patient Management & Reception)
المصادر المرجعية: [Ref/Q7, Q8] · [Learn/Ch2, Ch5] القرارات المُدمجة: 17 (البنية)، 11 (الأعلام تُفصَّل في G6/G7)

G4.1 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
نموذج Patient كامل	مطلوب	موجود بحقول ديموغرافية + LabId كمعرف مستمر عبر الزيارات	✅ مطابق
Visit كسجل زيارة مستقلة	مطلوب	Visit.cs — 30+ حقلاً بما فيها بيانات صيام/حمل/تاريخ مرضي + مالية	✅ مطابق
بيانات تاريخ مرضي مفصلة	مطلوب	Visit يحتوي HasDiabetes/HasAnemia/HasBleedingDisorder/HasThyroid/HasJointDisease/HasViralInfection/OnAnticoagulant/HasHypertension/HasLiverDisease/HasKidneyDisease/HasLupus/HadXrayContrast + PatientMedicalHistory جدول منفصل	✅ مطابق ومتفوق
بيانات عينات مأخوذة خارج المعمل	مطلوب	Visit.TakenOutsideLab + OutsideUrine/Stool/Blood/Semen/Csf	✅ مطابق
البحث عن مرضى قدامى بـ LabId	مطلوب	PatientService + BarcodeGenerator.GetOrCreateLabIdAsync	✅ مطابق
VPatientHistory view لتاريخ المريض	مطلوب	VPatientHistory.cs view موجودة	✅ مطابق
G4.2 معايير قبول
[من القرار 17] عند إنشاء زيارة جديدة، إنشاء الباركود LabId (إن لم يوجد للمريض) و Case Code و File Code للزيارة يجب أن يستخدم BarcodeGenerator الحالي (تفاصيله في G7).
عند تحرير أي حقل من Visit بعد الحفظ الأولي، يجب أن يُسجل في AuditLog مع الحقل السابق والجديد.
G4.3 سيناريو اختبار يدوي
تسجيل مريض جديد ثم إعادة تسجيل زيارة له لاحقًا — يجب أن يُسترجع نفس LabId وليس رقمًا جديدًا.
تسجيل زيارة لمريض حامل صائم مع تاريخ سكري — يجب حفظ الأعلام كلها بشكل صحيح والاعتماد عليها في اختيار نطاق طبيعي مناسب في G6.
G5 — الفوترة، الحساب، والدفع (Billing & Payment)
المصادر المرجعية: [Ref/Q9, Q10] · [Learn/Ch5, Ch6] القرارات المُدمجة: 12, 13, 14, 15, 16

⚠️ هذه أهم مجموعة من حيث إعادة الهيكلة — القرار 12 يفرض تفكيك خلط جوهري في الكود الحالي.

G5.1 التصحيح الجوهري — الفصل الثلاثي (القرار 12)
الحالة الحالية في الكود (فحص مباشر):

PaymentMethod enum = { Cash, Insurance, Contract, Other } — خلط واضح: Insurance و Contract مفاهيم تمييز عميل/جهة، وليست وسائل سداد. Card و Check غائبان تمامًا.
لا يوجد enum BillingType مستقل.
لا يوجد enum ReferringEntityCategory مستقل.
TestType.DefaultPrice سعر مفرد واحد — لا وجود لثنائية Patient/LabToLab.
PriceScheme + TestTypePrice تدعم قوائم أسعار مخصصة، لكنها فوق طبقة السعر الأساسية غير موجودة.
التصميم المستهدف (بعد القرار 12):

Copy┌──────────────────────────────────┐   ┌──────────────────────────────────┐
│ enum BillingType                 │   │ enum PaymentMethod  (منفصل تمامًا)│
│   Individual                     │   │   Cash                            │
│   LabToLab                       │   │   Card                            │
│   Free                           │   │   Check                           │
│                                  │   │   Insurance                       │
│ يُحدَّد مصدر التسعير على مستوى Visit │   │   Other                           │
└──────────────────────────────────┘   └──────────────────────────────────┘
                                             ↑
                                             على مستوى Payment (سطر دفع)

┌──────────────────────────────────────────────┐
│ enum ReferringEntityCategory                 │
│   ReferringDoctor                            │
│   OutsourcedSample                           │
│   ReferralOrContractEntity                   │
│ على مستوى ReferralSource — للتقارير والعمولات │
└──────────────────────────────────────────────┘

┌────────────────────────────────────────────┐
│ TestType                                   │
│   + PatientDefaultPrice   (جديد — أساسي)   │
│   + LabToLabDefaultPrice  (جديد — أساسي)   │
│   DefaultPrice ← يُهجَّر تدريجيًا لصالحهما    │
└────────────────────────────────────────────┘
        ↓ (اختياريًا فوق الطبقة الأساسية)
   PriceScheme + TestTypePrice (تسعير مخصص لجهة معينة)
G5.2 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
BillingType enum مستقل (القرار 12أ)	مطلوب	غير موجود	❌ غير موجود
PaymentMethod بالقيم الصحيحة (القرار 12ب)	Cash/Card/Check/Insurance/Other	حاليًا Cash/Insurance/Contract/Other — قيم مختلطة	⚠️ مشكلة مكتشفة تحتاج إصلاح
ReferringEntityCategory (القرار 12ج)	مطلوب	غير موجود كـ enum؛ ReferralSource موجود كنموذج لكن بدون هذا المحور	❌ غير موجود
ثنائية التسعير على مستوى TestType (القرار 12)	مطلوب	حقل واحد DefaultPrice	⚠️ يحتاج عمل
قوائم أسعار مخصصة (PriceScheme)	مطلوب فوق الأساس	PriceScheme.cs + TestTypePrice.cs موجودان ✅	✅ مطابق
نموذج Payment بسطور متعددة لكل زيارة	مطلوب	Payment.cs مع علاقة M:1 مع Visit ✅	✅ مطابق
Visit يحسب Subtotal/DiscountAmount/DiscountPercent/TotalAfterDiscount/TotalPaid/BalanceDue	مطلوب	كلها موجودة كأعمدة ✅	✅ مطابق
Max Discount Percent لكل موظف (القرار 13)	مطلوب	Staff.DiscountLimit موجود، لكن لا فرض له في InvoiceService/PricingService	⚠️ يحتاج عمل
VisitCharge لخدمات إضافية غير تحاليل (القرار 14)	مطلوب كواجهة صريحة	نموذج VisitCharge موجود ✅ كطبقة بيانات — لكن لا يوجد UI صريح حسب فحص المجلد Views/Patients	⚠️ يحتاج فحص XAML أعمق — الأرجح غير مكتمل
تدفق "حساب ثم حفظ" أوضح من Enter مرتين (القرار 15)	مطلوب	يحتاج فحص XAML للـ Invoice/Reception View	⚠️ يحتاج عمل — يُصمَّم بزرين منفصلين
إلغاء "تعديل حساب سابق بالنقر" (القرار 16)	يُستبعد	يجب التأكد أن السلوك غير مطبَّق	✅ مطابق (بالحذف) — لا نفعّله
Cash Drawer بكلمة مرور	مطلوب	CashDrawerService.cs موجود ✅	✅ مطابق
G5.3 معايير قبول مفصّلة
[من القرار 12] إنشاء ثلاث Enums مستقلة في Models/Enums/:

Copypublic enum BillingType { Individual, LabToLab, Free }
public enum PaymentMethod { Cash, Card, Check, Insurance, Other }
public enum ReferringEntityCategory { ReferringDoctor, OutsourcedSample, ReferralOrContractEntity }
إضافة BillingType BillingType على Visit (بدل الاستدلال الحالي على النوع من CompanyId/SchemeId)، وReferringEntityCategory Category على ReferralSource.

[من القرار 12] ترحيل PaymentMethod: الاحتفاظ بـ Cash وInsurance وOther، وإضافة Card وCheck، وإزالة Contract (يُنقل لطبقة BillingType وعلاقة ReferralSource).

[من القرار 12] إضافة على TestType:

Copypublic decimal PatientDefaultPrice { get; set; }
public decimal LabToLabDefaultPrice { get; set; }
مع Migration ترحيل يضبط PatientDefaultPrice = DefaultPrice وLabToLabDefaultPrice = DefaultPrice * 0.7m (قيمة ابتدائية قابلة للتعديل يدويًا لاحقًا).

[من القرار 12] خدمة PricingService.GetPriceForTestAsync(testTypeId, visitId) تُطبِّق ترتيبًا واضحًا:

إذا Visit.SchemeId موجود ⇒ ابحث في TestTypePrice لهذه القائمة (تجاوز الأساس).
وإلا: استعمل BillingType:
Individual ⇒ PatientDefaultPrice
LabToLab ⇒ LabToLabDefaultPrice
Free ⇒ 0
[من القرار 13] في InvoiceService.ApplyDiscount(visitId, discountPercent, staffId) رفض أي discountPercent > staff.DiscountLimit (إلا لو staff.IsAdmin)، وتسجيل الرفض في AuditLog.

[من القرار 14] إنشاء Views/Patients/AddExtraChargeDialog.xaml — نافذة صريحة لاختيار وصف الخدمة (Home Collection / Rush Fee / Other) وإدخال المبلغ، مع حفظ سطر VisitCharge مربوط بـ Visit. الإجمالي يُعاد حسابه فورًا.

[من القرار 15] في نموذج فاتورة الاستقبال، استبدال أي نمط "اضغط Enter مرتين" بـ:

زر "احسب" (Calculate) يعرض المبالغ بشكل معاينة (Preview) في منطقة واضحة.
زر "احفظ الفاتورة" (Save Invoice) منفصل ومعطَّل حتى يُضغط "احسب" مرة واحدة.
أي تعديل بعد الحساب يُبطل زر الحفظ حتى إعادة الحساب.
[من القرار 16] لا يجب أن يوجد أي MouseDown/Click handler على خلية إجمالي التحليل يفتح شاشة تعديل حساب سابق. إذا وُجد → يُحذف. تعديل حساب سابق يتم فقط من شاشة الفواتير القديمة الرسمية.

G5.4 سيناريو اختبار يدوي مفصَّل
[Q12 - BillingType] إنشاء تحليل CBC بسعر مريض 100 وسعر معمل-لمعمل 70. تسجيل زيارة Individual وأخرى LabToLab بنفس التحليل — يجب أن تظهر 100 و 70 على التوالي.
[Q12 - PaymentMethod] إنشاء زيارة Individual بإجمالي 500، ثم تسجيل دفعتين: 200 بـ Cash و 300 بـ Card — يجب حفظهما بوسائل سداد صحيحة والزيارة تُغلَق بـ BalanceDue=0.
[Q13] موظف بـ DiscountLimit=10% يحاول تطبيق خصم 15% — رفض + تسجيل في AuditLog + رسالة صريحة "الخصم يتجاوز الحد المسموح".
[Q14] إضافة سطر VisitCharge "خدمة السحب المنزلي 50 جنيه" — يجب أن يزيد Subtotal بـ 50 ويُعاد حساب Total.
[Q15] في شاشة الاستقبال: زر "احفظ" معطَّل، الضغط على "احسب" ⇒ عرض المعاينة ⇒ زر "احفظ" يصبح مفعَّلاً. تعديل التحاليل ⇒ زر "احفظ" يُعطَّل مجددًا.
G6 — إدخال ومراجعة النتائج (Result Entry & Review)
المصادر المرجعية: [Ref/Q11, Q12] · [Learn/Ch4] القرارات المُدمجة: 11 (نصفها هنا، النصف الآخر في G7)، 22

G6.1 التصحيح الجوهري — تسميات حساسية المزارع (القرار 22)
[تصحيح مبني على تحقق مستقل قاطع]: الوثيقة الموحدة ذكرت سابقًا تسميات ثلاثية Sensitive/Moderate/Resistant. هذا خطأ. المرجع الأصلي يستخدم أربع تسميات بلاحقة For حرفيًا:

HighlyFor — الحساسية عالية
ModerateFor — الحساسية متوسطة
LowFor — الحساسية منخفضة
ResistantFor — مقاوم
حالة الكود الحالي (فحص مباشر): AntibioticSensitivity = { Highly, Moderate, Low, Resistant } — القيم الأربع صحيحة لكن التسميات بدون لاحقة For. فجوة تسمية دقيقة يجب سدّها بإعادة تسمية الـ enum + Migration + تحديث كل مراجع الكود (OrganismAntibiotic.Sensitivity) + تحديث موارد التعريب في UI.

G6.2 التصحيح الثاني — الأعلام الخمسة والحالات السبع (القرار 11)
المرجع (بعد التحقق المستقل): 7 رموز حالة بصرية معروضة للمستخدم، مشتقة من 5 أعلام Boolean مستقلة:

#	العلم (Flag)	الوصف
F1	IsEntered	أُدخلت النتائج
F2	IsReviewed	راجعها معتمد
F3	IsPrinted	طُبعت النتائج
F4	IsDelivered	استُلمت من المريض
F5	IsFullyPaid	خالصة الحساب ( مستقل تمامًا عن F4)
7 حالات بصرية مشتقة من هذه الأعلام:

#	الرمز	الشرط	الوصف
S1	🔴 دائرة حمراء	كل F1..F4 = false	مريض جديد بلا نتائج
S2	📝 ورقة ملاحظات	F1=false ولكن الزيارة أُنشئت	نتائج لم تُكتب بعد
S3	↔️ سهمان (أخضر/أزرق)	F1=true, F2=false	نتائج لم تُراجع بعد
S4	🖨️ طابعة	F2=true, F3=false	نتائج لم تُطبع بعد
S5	🛒 عربة تسوق	F3=true, F4=false	لم يُستلم بعد
S6	£ جنيه	F4=true, F5=false	استلم كل النتائج لكن يوجد باقٍ حساب
S7	🎖️ وسام	كل F1..F5 = true	كل شيء مكتمل
حالة الكود الحالي: PatientVisitStatus enum يحتوي 7 قيم بأسماء تطابق روح المرجع، لكنه enum مسطّح يفقد الاستقلالية بين F4 و F5 (تحديدًا الفرق بين S5 و S6). التصحيح المطلوب: الاستعاضة عن الـ enum بخمسة أعلام صريحة + خاصية محسوبة (Computed) DisplayStatusIcon تشتق الحالة السبعية.

G6.3 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
نموذج TestResult بقيم رقمية/نصية	مطلوب	موجود مع ResultValue/ResultNumeric + SnapUnit/SnapLowNormal/SnapHighNormal (Snapshot للنطاق المستخدم)	✅ مطابق ومتفوق
علم النطاق المرجعي (Low/High/Critical)	مطلوب	ResultClinicalStatus { Normal, Low, High, Critical } + SnapLowFlag/SnapHighFlag	✅ مطابق
الأعلام الخمسة على Visit (القرار 11)	مطلوب	PatientVisitStatus enum مسطح — لا أعلام مستقلة	⚠️ يحتاج إعادة تصميم
مراجعة النتائج (ResultValidationStatus)	مطلوب	Entered/Reviewed/Validated/Released + ValidatedByStaffId/ValidatedAt	✅ مطابق
TestStage على مستوى VisitTest	مطلوب	موجود	✅ مطابق
حساسية المضادات (AntibioticSensitivity) بلاحقة For (القرار 22)	مطلوب	القيم صحيحة (4)، التسميات بدون For	⚠️ يحتاج إعادة تسمية
Culture / Organism / Antibiotic كنماذج منفصلة	مطلوب	MicrobiologyCulture + MicrobiologyOrganism + OrganismAntibiotic + AntibioticCatalog ✅	✅ مطابق ومتفوق
Semen Analysis / Cross-Match / Blood Bank	مطلوب	SemenAnalysis.cs + CrossMatchTest.cs + CrossMatchDonor.cs + BloodBankService.cs ✅	✅ مطابق
Auto-Comment على النتائج	مطلوب	ReportCommentTemplate + ReportCommentEngine.cs + ReportCommentTrigger ✅	✅ مطابق ومتفوق
G6.4 معايير قبول
[من القرار 22] إعادة تسمية AntibioticSensitivity إلى:

Copypublic enum AntibioticSensitivity : byte
{
    HighlyFor = 0,
    ModerateFor = 1,
    LowFor = 2,
    ResistantFor = 3
}
مع Migration للحفاظ على القيم الرقمية (لا حاجة لتغيير بيانات) + تحديث كل موارد التعريب (Arabic/English UI resources) لعرض التسميات المناسبة للمستخدم مع الاحتفاظ بالاسم الحرفي في الكود.

[من القرار 11] إضافة على Visit:

Copypublic bool IsEntered { get; set; }       // F1
public bool IsReviewed { get; set; }      // F2
public bool IsPrinted { get; set; }       // F3
public bool IsDelivered { get; set; }     // F4
public bool IsFullyPaid { get; set; }     // F5 — مستقل عن F4
مع خاصية محسوبة ([NotMapped]) VisitDisplayStatus تعود بأحد قيم enum VisitDisplayStatus السبعية:

Copypublic enum VisitDisplayStatus
{
    NewNoResults,        // S1 🔴
    ResultsNotWritten,   // S2 📝
    ResultsNotReviewed,  // S3 ↔️
    ResultsNotPrinted,   // S4 🖨️
    NotDelivered,        // S5 🛒
    DeliveredWithBalance,// S6 £
    FullyComplete        // S7 🎖️
}
[من القرار 11] ترحيل PatientVisitStatus القديم: حساب الأعلام من القيم الحالية عبر Migration، ثم إلغاء الـ enum القديم (أو الاحتفاظ به Deprecated لجيل واحد).

[من القرار 11] IsFullyPaid يُشتق من Visit.BalanceDue == 0 كتحديث تلقائي عند كل عملية Payment، لكن يظل مستقلاً عن IsDelivered (حتى لو المريض لم يستلم النتائج، يمكن أن يكون قد سدد كامل الحساب).

G6.5 سيناريو اختبار يدوي
[Q22] إدخال مزرعة، إضافة ميكروب، إضافة 3 مضادات بحساسيات متدرجة (HighlyFor, ModerateFor, LowFor) ثم مضاد رابع ResistantFor — يجب أن يُطبع تقرير المزرعة بالأربع فئات بترتيب صحيح.
[Q11] سلسلة تدفق كاملة لزيارة:
إنشاء زيارة (كل الأعلام false) → أيقونة 🔴 S1.
إدخال جزئي لنتيجة (F1=true) → 📝 S2.
إكمال الإدخال (كل النتائج) + المراجع لم يعتمد → ↔️ S3.
المراجع يعتمد (F2=true) لكن لم تُطبع → 🖨️ S4.
طباعة (F3=true) لكن لم تُسلَّم → 🛒 S5.
المريض استلم النسخة (F4=true) لكن باقٍ عليه 100 جنيه (F5=false) → £ S6.
المريض سدد الباقي (BalanceDue=0 ⇒ F5=true) → 🎖️ S7.
[Q11 - استقلالية F4/F5] سيناريو موازٍ: المريض سدد كامل الحساب لكن لم يستلم النسخة بعد (F5=true, F4=false) → يجب أن تظل الأيقونة 🛒 S5 (تنبيه للاستلام)، ليس £ S6.
قد اكتملت المجموعات G1..G6 في هذا الجزء الأول. أواصل الآن في نفس الرد بالمجموعات G7..G10 ثم خارطة الطريق ثم المخاطر المعمارية.

G7 — الطباعة والباركود والتسليم (Printing, Barcodes & Delivery)
المصادر المرجعية: [Ref/Q13] · [Learn/Ch6] · [Show] القرارات المُدمجة: 11 (النصف الثاني)، 17، 18

G7.1 تصحيح مقابل الوثيقة الأولى — بنية الباركود (القرار 17)
فحص مباشر للكود BarcodeGenerator.cs:

CopyBuildBarcodeValue(typeDigit, branchDigit, date, ordinal):
  → [Type(1)][Branch(1)][yyMMdd(6)][Weekday(1)][Ordinal-D3(3)][Luhn(1)] = 13 رقمًا
البادئات الحالية: Case=1, File=3, Lab=5.

المطلوب من القرار 17: مطابقة حرفية كاملة لترتيب حقول المرجع وبادئاته الثلاث. البنية الحالية 13 رقمًا مطابقة للطول، لكن يجب مراجعة نصية دقيقة للمرجع للتأكد من:

ترتيب المرجع للحقول (خصوصًا موضع Weekday: بعد التاريخ أم قبله).
بادئات المرجع الفعلية للأنواع الثلاثة (Case/File/Lab) — هل هي 1/3/5 حقًا أم قيم أخرى.
غموض متبقٍّ (يُوثَّق صراحة): ⚠️ لم يتم التحقق النصي المباشر من Real_Lab_System_Unified_Reference_FINAL.md القسم المخصص للباركود ضمن هذا التحليل. تُترك هذه النقطة كـ TODO للمراجعة اليدوية قبل بدء الشريحة.

G7.2 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
نموذج PatientBarcode مع نوع كود	مطلوب	موجود بـ BarcodeCodeType (Case/File/Lab) + SortOrdinal + IssueDate	✅ مطابق
باركود 13 رقمًا يطابق ترتيب المرجع (القرار 17)	مطلوب	13 رقمًا ✅، الترتيب يحتاج تحقق نصي	⚠️ يحتاج تحقق نصي من المرجع
Luhn Check Digit فوق البنية (القرار 18)	تحسين مقصود	CalculateLuhnCheckDigit() موجود ومطبَّق ✅	✅ مطابق للقرار
طباعة تقرير النتائج (FlowDocument)	مطلوب	WpfFlowDocumentPrintService.cs + ReportLayoutService.cs	✅ مطابق
طباعة ملصق العينة (Label)	مطلوب	WpfLabelPrintService.cs	✅ مطابق
Print Preview قبل الطباعة	مطلوب	PrintPreviewDialogService.cs	✅ مطابق
Print Queue (طابور طباعة)	مطلوب	PrintQueueService.cs + PrintQueueItemStatus enum ✅	✅ مطابق ومتفوق
Receipt Print Log	مطلوب	ReceiptPrintLog.cs model ✅	✅ مطابق
علم IsPrinted مستقل (F3 من القرار 11)	مطلوب	يُدار حاليًا عبر PatientVisitStatus — يُهاجَر لعلم Boolean صريح	⚠️ يُنفَّذ ضمن G6.4
علم IsDelivered مستقل (F4 من القرار 11)	مطلوب	يوجد DeliveryConfirmedAt/DeliverySignature/DeliveryOtpCode على Visit + DeliveryConfirmation جدول منفصل + DeliveryConfirmationService — لكن لا يوجد Bool صريح IsDelivered	⚠️ يُضاف كعلم صريح مع القرار 11
BranchNumber في PatientBarcode (القرار 23)	يُزال	موجود حاليًا byte BranchNumber = 1	⚠️ يُزال (تفاصيل في G10)
G7.3 معايير قبول
[من القرار 17] قبل تنفيذ الشريحة المخصصة، يُنشأ محضر تحقق نصي (Test Fixture) من المرجع الأصلي يُقارن نموذج مثال مُخرَج من BuildBarcodeValue مع نموذج من المرجع رقمًا برقم. أي اختلاف في ترتيب الحقول أو البادئات يُصحَّح في BarcodeGenerator.
[من القرار 18] Luhn Check Digit يبقى مضافًا بعد كل الحقول الأخرى مباشرةً كخانة رقمية أخيرة. يُوثَّق في تعليق فوق CalculateLuhnCheckDigit() أنه تحسين متعمد خارج المرجع الأصلي.
[من القرار 23] إزالة BranchNumber من BuildBarcodeValue بالكامل — البنية تصبح [Type(1)][yyMMdd(6)][Weekday(1)][Ordinal-D3(4)][Luhn(1)] = 13 رقمًا (بتوسيع Ordinal من 3 إلى 4 خانات للحفاظ على 13 رقمًا)، أو [Type(1)][yyMMdd(6)][Weekday(1)][Ordinal-D3(3)][Extra(1)][Luhn(1)] حسب ما يظهر من التحقق النصي للمرجع. القرار النهائي على التوزيع يُتَّخذ بعد التحقق النصي.
[من القرار 11] IsPrinted يُضبط true تلقائيًا في PrintQueueService عند نجاح طباعة تقرير النتائج (ليس ملصق العينة). IsDelivered يُضبط true عند نجاح DeliveryConfirmationService.ConfirmAsync (توقيع/OTP/موظف يشهد).
G7.4 سيناريو اختبار يدوي
[Q17] إنشاء 3 زيارات في نفس اليوم لنفس المريض — كل زيارة يجب أن تُعطى Ordinal متسلسل داخل اليوم بدون تكرار، وباركود Case مختلف بالكامل.
[Q18] إدخال باركود مُنشَأ للتو في ماسح — يجب قبوله (Luhn صحيح). إدخاله يدويًا مع تغيير رقم واحد فقط — يجب رفضه.
[Q11] بعد طباعة تقرير زيارة → علم IsPrinted=true → أيقونة الحالة تتغير من 🖨️ إلى 🛒.
G8 — التقارير والإحصائيات (Reports & Analytics)
المصادر المرجعية: [Ref/Q3] · [Learn/Ch7] القرارات المُدمجة: 12، 23

G8.1 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
تقرير مبيعات يومي/شهري	مطلوب	ReportingService.cs + FinancialService.cs	✅ مطابق
تقرير رصيد المتأخرات (Outstanding Balance)	مطلوب	OutstandingBalanceReportService.cs + VOutstandingBalance view ✅	✅ مطابق ومتفوق
تقرير عمولات المُحوِّلين	مطلوب	CommissionReportService.cs + VReferralCommissionReport view ✅	✅ مطابق
إعادة تصنيف تقارير العمولات وفق ReferringEntityCategory (القرار 12)	مطلوب	VReferralCommissionReport view يعتمد حاليًا على ReferralSource بدون هذا المحور — يحتاج تحديث بعد إضافة enum	⚠️ يحتاج عمل بعد G5
تقرير الحضور	مطلوب	AttendanceService.cs + Attendance.cs ✅	✅ مطابق
تقرير Audit Trail	مطلوب	AuditTrailDialogService.cs + VResultAuditTrail view ✅	✅ مطابق ومتفوق
Views جاهزة كطبقة استعلام	مطلوب	VPatientHistory, VPendingTest, VSampleTubeStatus, VOutstandingBalance, VReferralCommissionReport, VResultAuditTrail — 6 Views ✅	✅ مطابق ومتفوق
تقارير مقسّمة بالفرع (القرار 23)	يُزال	لا يوجد فلترة صريحة بـ BranchNumber في التقارير حاليًا — جيد	✅ متوافق مع القرار (يجب التأكد بعد إزالة الحقول)
G8.2 معايير قبول
[من القرار 12] تحديث VReferralCommissionReport بعد إضافة ReferringEntityCategory ليتضمن تجميع بحسب المحور: ReferringDoctor / OutsourcedSample / ReferralOrContractEntity. تقرير عمولات يعرض إجماليًا لكل فئة على حدة.
[من القرار 23] أي تقرير موجود لا يجب أن يقبل معامل BranchId أو BranchNumber — إن وُجد يُحذف. عنوان التقرير الرسمي "تقرير معمل [اسم المعمل]" بدون تقسيم فروع.
تقارير المبيعات الشهرية يجب أن تُميّز بين BillingType (Individual / LabToLab / Free) لعرض إجمالي كل قناة بيع.
G8.3 سيناريو اختبار يدوي
توليد تقرير المبيعات الشهري لشهر يحتوي زيارات Individual و LabToLab وFree — التقرير يعرض 3 صفوف تجميع بإجمالياتها.
توليد تقرير عمولات المُحوِّلين — 3 أقسام: أطباء محوِّلون / عينات مُستعان بها خارجيًا / جهات إحالة أو تعاقد.
البحث في كل التقارير عن أي إشارة لكلمة "فرع" — يجب ألا تظهر.
G9 — النسخ الاحتياطي وصيانة قاعدة البيانات (Backup & DB Maintenance)
المصادر المرجعية: [Ref/Q4, Q13] · [Learn/Ch7] القرارات المُدمجة: 1، 8، 9، 10

⚠️ هذه المجموعة تحتوي على أثقل قرار تنفيذي في الوثيقة — القرار 9 يفرض إعادة معمارية كاملة لخدمة النسخ الاحتياطي.

G9.1 التغيير المعماري الجوهري (القرار 9)
الحالة الحالية (فحص مباشر لـ BackupService.cs 339 سطرًا):

الآلية: JSON Serialization لكل الجداول عبر Reflection + [Auditable] filter + AES Encryption على المخرج (.bak.enc).
تسمية الملف: FinalLabSystem_yyyy-MM-dd_HHmmss.bak.enc.
الاسترجاع: فك التشفير + Deserialize JSON + إعادة إدراج للسجلات.
المشاكل المعمارية بهذا النهج:

لا يستفيد من ميزات SQL Server (Point-in-time recovery, transaction log backups, differential).
مخاطر تضارب Foreign Keys أثناء الاسترجاع.
بطيء على قواعد بيانات كبيرة (Reflection + JSON على كل جدول).
لا يشمل Views/Stored Procedures/Indexes/Constraints.
غير قياسي — لا يمكن استخدامه مع أدوات SQL Server الرسمية.
التصميم الجديد المطلوب:

CopyBackupService (إعادة بناء)
├── ISqlServerBackupExecutor      ← يشغّل T-SQL: BACKUP DATABASE ... TO DISK
│   ├── FullBackup                    (WITH INIT, COMPRESSION)
│   ├── DifferentialBackup            (WITH DIFFERENTIAL)
│   └── TransactionLogBackup          (WITH NO_TRUNCATE)
├── ISqlServerRestoreExecutor     ← يشغّل T-SQL: RESTORE DATABASE ... FROM DISK
│   ├── SetDbToSingleUser             (WITH ROLLBACK IMMEDIATE)
│   ├── Restore                       (WITH REPLACE, RECOVERY / NORECOVERY)
│   └── SetDbToMultiUser
├── IBackupFileNameStrategy       ← اسم واضح + تاريخ (القرار 10)
└── IBackupScheduler              ← يومي/أسبوعي (اختياري)
G9.2 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
نسخ احتياطي بأمر SQL Server أصلي (القرار 9)	مطلوب	JSON Serialization + AES — معماري خاطئ	❌ إعادة بناء كاملة
مسار قاعدة بيانات ثابت D:\real lab system\Data (القرار 1)	يُلغى كقيد	مسار النسخة الاحتياطية يُختار عبر targetFolder parameter — لا قيد ثابت ✅	✅ متوافق مع القرار
تسمية سياقية (اسم + تاريخ) (القرار 10)	مطلوب	FinalLabSystem_{DateTime.UtcNow:yyyy-MM-dd_HHmmss}.bak.enc — منطق التسمية سليم، يحتاج فقط تعديل الامتداد إلى .bak بعد إعادة البناء	✅ مطابق للقرار (مع تعديل امتداد)
كلمة مرور منفصلة لشاشة النسخ (القرار 8)	مطلوب	adminPassword parameter تُطلب حاليًا — لكن هي كلمة مرور الأدمن نفسها، ليست كلمة مرور شاشة صيانة قاعدة بيانات منفصلة	⚠️ يحتاج عمل — فصل كلمة مرور DB Maintenance
ضمان أن الأدمن فقط ينسخ	مطلوب	_currentUserSession.CurrentUser?.IsAdmin فحص موجود ✅	✅ مطابق
تسجيل عملية النسخ في AuditLog	مطلوب	يحتاج تحقق من _auditService — الاستدعاء موجود في الحقن	✅ مطابق (لا تفاصيل)
Compact / Repair Database	مطلوب [Ref/Q13]	ProcessService.cs (يحتاج تحقق) — لا يوجد شاشة صيانة قاعدة بيانات صريحة	⚠️ يحتاج فحص عمق
G9.3 معايير قبول
[من القرار 9] استبدال منطق BackupService.CreateBackupAsync بالكامل بـ:

CopyBACKUP DATABASE @dbName TO DISK = @targetPath
WITH INIT, COMPRESSION, CHECKSUM, STATS = 10, NAME = 'FinalLab Full Backup'
وتنفيذه عبر ExecuteSqlRawAsync على قناة DbContext منفصلة (لأن الأمر يحتاج امتيازات معينة على قاعدة master أحيانًا).

[من القرار 9] استبدال منطق الاسترجاع بـ:

CopyALTER DATABASE @dbName SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
RESTORE DATABASE @dbName FROM DISK = @sourcePath WITH REPLACE, RECOVERY;
ALTER DATABASE @dbName SET MULTI_USER;
مع تعامل قوي مع الأخطاء (اتصال مفتوح يمنع Single User) وإلغاء أي DbContext آخر خلال العملية.

[من القرار 10] تسمية ملف: FinalLab_{yyyy-MM-dd_HHmmss}_{BackupTypeShort}.bak مثال: FinalLab_2026-07-13_142530_FULL.bak. الامتداد الأصلي .bak بدون تشفير طبقة عليا — SQL Server يوفر خيار WITH PASSWORD إذا أريد تشفير النسخة (اختياري لاحقًا).

[من القرار 8] إنشاء عمود مستقل LabSetting.DbMaintenancePassword (Hashed) بمعزل عن كلمة مرور الأدمن. شاشة النسخ الاحتياطي تطلب هذه الكلمة تحديدًا. تغييرها لا يؤثر على تسجيل دخول الأدمن ولا على كلمة مرور Cash Drawer.

[من القرار 1] لا يُفرض أي مسار افتراضي D:\real lab system\Data. المسار الافتراضي يُخزَّن في LabSetting.DefaultBackupPath قابلاً للتعديل من الإعدادات، مع قيمة أولية معقولة (%APPDATA%\FinalLabSystem\Backups\).

عملية النسخ يجب أن تُسجَّل في AuditLog بحقول: Action=BackupCreated, TargetPath, BackupSizeBytes, Duration, Result (Success/Fail + رسالة الخطأ).

G9.4 سيناريو اختبار يدوي
[Q9] أدمن يفتح شاشة النسخ الاحتياطي، يُدخل كلمة مرور DB Maintenance، يختار مجلد الحفظ، ينقر "نسخ كامل" — ينتج ملف .bak مقروء من SQL Server Management Studio مباشرة يمكن استرجاعه في بيئة أخرى.
[Q9] استرجاع نسخة على قاعدة بيانات فارغة — يجب أن تعمل الواجهة بعد الاسترجاع بدون فقدان أي جدول أو Migration.
[Q8] تغيير كلمة مرور DB Maintenance من الإعدادات — كلمة مرور Cash Drawer وكلمة مرور تسجيل الدخول تبقى غير متأثرتين.
[Q1] تشغيل التطبيق أول مرة على جهاز بلا مجلد D:\real lab system\Data — التطبيق يعمل عادي دون طلب هذا المسار.
G10 — الإعدادات العامة والتكامل والواجهة (Settings & Integration)
المصادر المرجعية: [Ref/Q2, Q4] · [Learn/Ch1, Ch7] القرارات المُدمجة: 4، 5، 7، 23

G10.1 حالة المطابقة
البند	المرجع	الحالة في الكود	التصنيف
LabSetting كصف واحد للإعدادات العامة	مطلوب	LabSetting.cs موجود مع علاقات متعددة بـ Staff	✅ مطابق
اسم المعمل / عنوان / تليفون / لوجو	مطلوب	يحتاج فحص أعمدة LabSetting.cs كاملة	✅ مطابق (على الأرجح)
تعريف Connection String لـ SQL Server	مطلوب	عبر appsettings.json + SettingsService.cs	✅ مطابق
رسالة "خادم قواعد البيانات غير معرف" (القرار 4)	لا يجب توحيدها حرفيًا	حاليًا رسالة عامة عند فشل الاتصال — لا مشكلة	✅ متوافق مع القرار
فحص أبعاد الشاشة <10 بوصة (القرار 5)	يُلغى	غير موجود ✅	✅ متوافق مع القرار
Responsive Layout (القرار 5)	مطلوب	يحتاج فحص XAML لكل النوافذ للتأكد	⚠️ يحتاج فحص XAML لكل View
رسالة "اضغط أي مفتاح للمتابعة" (القرار 7)	تُستبعد	يجب التأكد بالبحث في XAML وحصر أي KeyDown handlers	⚠️ يحتاج تحقق
Feature Toggles	مطلوب لتفعيل ميزات تدريجيًا	FeatureToggleService.cs ✅	✅ مطابق ومتفوق
إزالة Multi-Branch (القرار 23)	يُزال بالكامل	LabSetting.BranchNumber + PatientBarcode.BranchNumber — بقايا يجب حذفها	⚠️ إزالة مطلوبة
External Lab (تحاليل مُرسَلة خارج)	مطلوب	ExternalLab.cs + ExternalShipment.cs + ExternalShipmentItem.cs + ExternalLabService.cs + ExternalShipmentService.cs + ExternalLabRegistryService.cs ✅	✅ مطابق ومتفوق
قسم NATIGH.COM / Android (الوثيقة الموحدة القسم 15)	مُستبعَد صراحة من هذا التحليل	إن وُجدت بقايا في الكود ⇒ توثَّق كخطر معماري فقط	🚫 خارج النطاق
G10.2 معايير قبول
[من القرار 4] أي رسالة خطأ لفشل الاتصال بقاعدة البيانات يجب أن تكون واضحة ومفيدة (تُشير إلى Server/Database/Instance) — لكن لا توحيد نصي حرفي مع صيغة المرجع القديمة "خادم قواعد البيانات غير معرف". صيغة FinalLab الحالية أفضل ويُحتفظ بها.
[من القرار 5] كل نافذة/UserControl في Views/ تُراجَع لضمان:
عدم استخدام Width/Height ثابت على الجذر (استخدام MinWidth/MinHeight فقط).
استعمال Grid مع Star sizing أو DockPanel أو Viewbox عند الضرورة.
عدم افتراض أبعاد شاشة محددة (لا SystemParameters.PrimaryScreenWidth < ...).
[من القرار 7] بحث نصي في كل XAML وكل ViewModel عن نصوص "اضغط" / "متابعة" / "Any Key" / "Press Any Key" — إن وُجدت تُحذف. أي KeyDown handler كان يخدم هذا السلوك يُلغى.
[من القرار 23] إزالة BranchNumber من:
Models/LabSetting.cs (السطر 24).
Models/PatientBarcode.cs (السطر 15).
Migration جديدة تُسقط العمودين.
تحديث BarcodeGenerator.BuildBarcodeValue لعدم استخدام Branch (تفاصيل في G7.3-3).
إزالة LabSettings من علاقة Staff.LabSettings إن كانت تُشير لعدد فروع (تحقق مطلوب).
[من القرار 23] أي شريحة، تعليق، أو Task في _bmad/ تشير إلى Multi-Branch → تُحذف من خارطة الطريق بالكامل.
G10.3 سيناريو اختبار يدوي
تشغيل التطبيق على شاشات: 800×600 / 1366×768 / 1920×1080 / 2560×1440 — كل النوافذ يجب أن تتكيف بشكل مرئي ممتاز.
بحث بصري في التطبيق كاملاً عن كلمة "الفرع" / "Branch" — يجب ألا تظهر في أي شاشة نهائية.
تشغيل التطبيق على جهاز جديد ⇒ First-Run Setup ⇒ إنشاء أدمن ⇒ الدخول ⇒ لا ظهور لأي شاشة وسيطة أو رسالة "اضغط أي مفتاح".
🗺️ خارطة الطريق — الشرائح العمودية (Vertical Slices)
ملاحظة تنظيمية: كل شريحة مستقلة قابلة للنشر (Deployable Slice) — تشمل كل الطبقات من Model حتى XAML. الترتيب يعكس الأولوية المنطقية (Foundational أولاً، ثم Value، ثم Polish). تم حذف أي شريحة كانت مرتبطة بـ Multi-Branch بالكامل — القرار 23.

المرحلة الأولى — إعادة هيكلة القاعدة (Foundational Refactors) — 5 شرائح
VS-01 — فصل PaymentMethod / BillingType / ReferringEntityCategory (القرار 12)
الأثر: G5, G8
العناصر: إنشاء 3 enums جديدة + إضافة BillingType على Visit + ReferringEntityCategory على ReferralSource + Migration بترحيل البيانات القائمة (Insurance تبقى، Contract تُخرَّج لـ BillingType.LabToLab، Cash/Other تبقى، Card وCheck قيم جديدة فارغة قابلة للاستخدام) + تحديث كل PricingService/InvoiceService/FinancialService/التقارير.
معيار قبول: كل الاختبارات القائمة في FinalLabSystem.Tests تعبر بعد الترحيل + بيانات الزيارات القديمة تُحفظ صحيحة.
سيناريو اختبار: مذكور في G5.4-1 و G5.4-2.
مخاطر: Migration بيانات إنتاجية — يحتاج نسخ احتياطي قبل التطبيق.
VS-02 — ثنائية تسعير TestType (القرار 12)
الأثر: G3, G5
العناصر: إضافة PatientDefaultPrice و LabToLabDefaultPrice على TestType + Migration ينسخ DefaultPrice إلى كليهما (مع خصم 30% افتراضي للثاني) + تحديث PricingService.GetPriceForTestAsync بمنطق الأولوية (G5.3-4) + شاشة تحرير التحاليل تعرض الحقلين + الاختبارات.
يعتمد على: VS-01.
معيار قبول: زيارة Individual تستخدم PatientDefaultPrice، وزيارة LabToLab تستخدم LabToLabDefaultPrice، ومع PriceScheme مُخصَّص يتم تجاوز الاثنين.
VS-03 — الأعلام الخمسة والحالات السبع (القرار 11)
الأثر: G6, G7, G4 (شاشة قائمة المرضى)
العناصر: إضافة 5 أعمدة Boolean على Visit + خاصية محسوبة VisitDisplayStatus + Migration ترحيل من PatientVisitStatus القديم إلى الأعلام + تحديث VisitService/ResultService/PrintQueueService/DeliveryConfirmationService ليحدثوا الأعلام تلقائيًا + Converter في XAML يعرض الأيقونة المناسبة + إضافة أيقونات SVG السبع.
يعتمد على: —
معيار قبول: سلسلة تدفق الحالة في G6.5-2 تعمل بالكامل + سيناريو الاستقلالية G6.5-3 يعمل بالكامل.
VS-04 — إعادة تسمية AntibioticSensitivity بلاحقة "For" (القرار 22)
الأثر: G6
العناصر: إعادة تسمية القيم الأربع (Highly→HighlyFor, Moderate→ModerateFor, Low→LowFor, Resistant→ResistantFor) — القيم الرقمية تبقى (Migration بدون تغيير بيانات) + تحديث كل مراجع الاستخدام في Services/, ViewModels/, Views/ + تحديث موارد التعريب لعرض الاسم المناسب للمستخدم.
يعتمد على: —
معيار قبول: تقرير المزرعة يعرض 4 فئات مضادات بترتيب HighlyFor→ResistantFor مع تسميات UI مناسبة للغة الواجهة.
VS-05 — إزالة Multi-Branch بالكامل (القرار 23)
الأثر: G7, G10 + كل التقارير + كل الشرائح المستقبلية
العناصر: Migration يُسقط LabSetting.BranchNumber و PatientBarcode.BranchNumber + تحديث BarcodeGenerator.BuildBarcodeValue لصيغة 13 رقمًا بدون Branch (توسيع Ordinal إلى 4 خانات) + إزالة أي إشارة نصية "فرع" من UI + إزالة أي Task في _bmad/ مرتبط بالفروع.
يعتمد على: VS-01 (لضمان استقرار الأساس قبل).
معيار قبول: بحث نصي عن Branch في كل الكود يعود بنتائج فقط في تعليقات "removed" أو في نصوص خارجية.
المرحلة الثانية — تعزيز الفوترة والصلاحيات — 4 شرائح
VS-06 — تفعيل Max Discount Percent (القرار 13)
الأثر: G2, G5
العناصر: تعديل InvoiceService/PricingService لفرض Staff.DiscountLimit + شاشة تحرير الموظف تعرض/تحرر الحقل + شاشة تطبيق الخصم تعرض الحد المسموح للموظف الحالي + AuditLog للمحاولات المرفوضة.
يعتمد على: —
معيار قبول: مذكور في G5.4-3.
VS-07 — واجهة إضافة مبلغ إضافي صريحة (القرار 14)
الأثر: G5
العناصر: AddExtraChargeDialog.xaml + ViewModel + ربط بـ VisitCharge القائم + زر "خدمة إضافية" في شاشة الاستقبال + إعادة حساب Total بشكل صحيح.
يعتمد على: VS-01 (لضمان استقرار Visit Total).
معيار قبول: مذكور في G5.4-4.
VS-08 — تدفق فاتورة أوضح (احسب / احفظ) (القرارات 15، 16)
الأثر: G5
العناصر: تعديل PatientReceptionView.xaml أو ما يعادلها + ReceptionViewModel — زران منفصلان + معاينة واضحة + إبطال زر الحفظ عند التعديل + حذف أي معالج MouseDown على خلية إجمالي التحليل يفتح تعديل حساب سابق.
يعتمد على: VS-06, VS-07.
معيار قبول: مذكور في G5.4-5.
VS-09 — كلمات مرور شاشات حساسة منفصلة (القرار 8)
الأثر: G2, G5, G9
العناصر: إضافة أعمدة Hashed منفصلة على LabSetting (CashDrawer / DbMaintenance / Settings) أو جدول SensitiveScreenPassword M:1 مع LabSetting + شاشة "إعدادات الأمان" لتغيير كل كلمة على حدة + تحديث CashDrawerService/BackupService لاستدعاء الكلمة المناسبة.
يعتمد على: —
معيار قبول: مذكور في G2.3-2 و G9.4-3.
المرحلة الثالثة — إعادة معمارية النسخ الاحتياطي — شريحة واحدة كبيرة
VS-10 — استبدال JSON Backup بـ SQL Server BACKUP الأصلي (القرار 9، 10، 1)
الأثر: G9
العناصر: مذكورة في G9.1 بالكامل — 4 abstractions جديدة + شاشة UI معدَّلة + Migration لإضافة DefaultBackupPath + توثيق مطوَّر + Integration Tests تستخدم LocalDB.
يعتمد على: VS-09 (كلمة مرور DB Maintenance منفصلة).
معيار قبول: مذكور في G9.4-1 و G9.4-2.
مخاطر: تحتاج امتيازات SQL على قاعدة master — يجب توثيق متطلبات النشر بوضوح.
المرحلة الرابعة — التحسينات على المستوى الوظيفي — 4 شرائح
VS-11 — قائمة منسدلة لأسماء المستخدمين (القرار 3)
الأثر: G1
العناصر: إضافة ComboBox بربطه بـ Staff.IsActive==true مرتب بـ DisplayName + بقاء TextBox كخيار ثانوي.
معيار قبول: مذكور في G1.3-2.
VS-12 — نمط "نطاق موحد لجميع الفئات" (القرار 20)
الأثر: G3
العناصر: زر جديد في NormalRangesWindow + Preset يُنشئ صفًا واحدًا فقط + تعطيل أزرار إضافة نطاقات إضافية أثناء تفعيل الوضع.
معيار قبول: مذكور في G3.4-2.
VS-13 — Responsive Layout لكل النوافذ (القرار 5)
الأثر: G10
العناصر: مراجعة يدوية لكل XAML + إزالة Width/Height ثابتة + استعمال Grid/DockPanel/Viewbox مناسبة.
معيار قبول: G10.3-1.
VS-14 — تنظيف بقايا "اضغط أي مفتاح" و Splash Screen (القرارات 6، 7)
الأثر: G1, G10
العناصر: بحث نصي + حذف + توثيق.
معيار قبول: G10.3-3.
المرحلة الخامسة — التوثيق ومطابقة الباركود — شريحتان
VS-15 — تحقق نصي من بنية الباركود ومطابقته للمرجع (القرار 17)
الأثر: G7
العناصر: قراءة نصية دقيقة لقسم الباركود في Real_Lab_System_Unified_Reference_FINAL.md + جدول مقارنة رقمًا برقم + تعديل BarcodeGenerator.BuildBarcodeValue إن لزم + Unit Tests لكل بادئة (Case/File/Lab).
يعتمد على: VS-05 (بعد إزالة Branch).
معيار قبول: مثال باركود واحد على الأقل من المرجع يُطابَق حرفيًا رقمًا برقم بواسطة الكود.
VS-16 — توثيق Luhn كتحسين متعمد (القرار 18)
الأثر: G7
العناصر: تعليق XML Doc فوق CalculateLuhnCheckDigit() + مقطع في PRD في _bmad/ يوضح أن Luhn طبقة إضافية لا تُضعف مطابقة المرجع.
معيار قبول: قارئ الكود يفهم مباشرة أن Luhn غير موجود في المرجع الأصلي.
المرحلة السادسة — تقارير محدَّثة على المحاور الجديدة — شريحة واحدة
VS-17 — تحديث تقارير العمولات وفق ReferringEntityCategory (القرار 12)
الأثر: G8
العناصر: تحديث VReferralCommissionReport view + CommissionReportService + شاشة عرض التقرير لتقسيم النتائج على 3 محاور.
يعتمد على: VS-01.
معيار قبول: مذكور في G8.3-2.
إجمالي الشرائح: 17 شريحة عمودية (بدلاً من 20 في الوثيقة الأولى — تم حذف 3 شرائح كانت مرتبطة بـ Multi-Branch أو بحد الـ6 نطاقات، ودمج شريحتين متعلقتين بالباركود).

⚠️ المخاطر المعمارية (Architectural Risks)
R-01 — Migration بيانات إنتاجية لـ PaymentMethod (خطر عالٍ)
الوصف: القرار 12 يُغيّر قيم PaymentMethod القائمة. الترحيل غير عكسي إذا فُقدت النسخة الاحتياطية. التخفيف: تنفيذ VS-10 (Backup الحقيقي) قبل VS-01 إن أمكن، أو على الأقل نسخ احتياطية يدوية موثَّقة، وإضافة اختبارات Migration integration على قاعدة بيانات مطابقة للإنتاج.

R-02 — امتيازات SQL Server لعمليات BACKUP/RESTORE (خطر متوسط)
الوصف: أوامر BACKUP DATABASE و RESTORE تحتاج امتيازات على master قد لا تكون متاحة لحساب SQL المُستخدم في appsettings.json. التخفيف: توثيق متطلبات النشر: SQL user يجب أن يكون dbcreator أو له db_backupoperator على قاعدة FinalLab. توفير سكربت SQL جاهز لمنح الصلاحيات.

R-03 — بقايا Multi-Branch في الكود بعد الحذف (خطر منخفض)
الوصف: قد تبقى مراجع مخفية في _bmad/، Docs/، تعليقات كود، Seeds، أو Migrations قديمة. التخفيف: بحث نصي شامل (grep -r) بعد VS-05 كخطوة تحقق نهائية، وإضافة اختبار CI يفشل عند اكتشاف الكلمة BranchNumber في أي ملف مصدر جديد.

R-04 — الأعلام الخمسة قد تخرج من التزامن مع الحقول المالية (خطر متوسط)
الوصف: IsFullyPaid مشتق من BalanceDue==0. إذا نُسي تحديثه بعد عملية Payment، تُعرض حالة خاطئة. التخفيف: تنفيذ التحديث كـ EF Interceptor على SaveChanges لـ Payment، مع اختبار Integration محكم.

R-05 — بقايا NATIGH.COM في الكود (خطر منخفض — خارج نطاق التنفيذ)
الوصف: المصدر يستبعد صراحة القسم 15، لكن قد توجد بقايا في الكود (Views/Services) تحتاج تنظيف مستقبلي. التخفيف: مسح مبدئي: البحث عن Natigh/natigh/نتيجة في الكود — إن وُجدت تُوثَّق في Ticket منفصل خارج هذه الوثيقة (خارج النطاق).

R-06 — تحقق نصي من الباركود لم يتم بالكامل (خطر منخفض)
الوصف: مطابقة BarcodeGenerator مع نص المرجع الأصلي لم تُنفَّذ في هذا التحليل (VS-15 مخصصة لها). التخفيف: تنفيذ VS-15 قبل أي إصدار إنتاجي يعتمد على مطابقة الباركود.

R-07 — تغيير امتداد النسخة الاحتياطية من .bak.enc إلى .bak (خطر منخفض)
الوصف: النسخ الحالية بـ AES-encrypted JSON ستكون غير قابلة للاسترجاع بعد VS-10. التخفيف: أثناء VS-10، الاحتفاظ بمنطق RestoreLegacyJsonBackup كوظيفة استرجاع للنسخ القديمة فقط، مع رسالة واضحة "هذه نسخة قديمة، بعد الاسترجاع أنشئ نسخة جديدة بالتنسيق الأصلي".

⛔ ملاحظة على مخاطر تم إلغاؤها من الوثيقة الأولى
المخاطرة الملغاة	السبب
خطر عدم فرض حد 6 نطاقات	ألغي بالقرار 19 — لا يوجد حد أصلاً في المرجع.
خطر عدم توحيد رسالة "خادم قواعد البيانات غير معرف"	ألغي بالقرار 4 — التوحيد الحرفي غير مطلوب.
خطر تنسيق التاريخ English (Canada)	ألغي بالقرار 5 — لن يُطبَّق.
خطر عدم فحص أبعاد الشاشة <10 بوصة	ألغي بالقرار 5 — يُستبدل بـ Responsive Layout.
شريحة Multi-Branch	ألغيت بالقرار 23 بالكامل.
📝 حالة إكمال العمل
المجموعة/القسم	الحالة
G1 — الإقلاع وتسجيل الدخول	✅ مكتملة
G2 — المستخدمون والصلاحيات	✅ مكتملة
G3 — التحاليل والقاموس الطبي	✅ مكتملة
G4 — المرضى والاستقبال	✅ مكتملة
G5 — الفوترة والدفع	✅ مكتملة (الأثقل — إعادة هيكلة جوهرية)
G6 — إدخال ومراجعة النتائج	✅ مكتملة (تصحيحان جوهريان)
G7 — الطباعة والباركود والتسليم	✅ مكتملة (مع غموض نصي متبقٍّ على مطابقة الباركود — R-06)
G8 — التقارير والإحصائيات	✅ مكتملة
G9 — النسخ الاحتياطي وصيانة DB	✅ مكتملة (إعادة معمارية كاملة)
G10 — الإعدادات والتكامل	✅ مكتملة
خارطة الطريق (17 شريحة)	✅ مكتملة
المخاطر المعمارية (7 مخاطر)	✅ مكتملة
نقاط الغموض المتبقية (موثَّقة صراحة كما طُلب — بلا افتراضات)
بنية الباركود الحرفية للمرجع — لم يتم فحص نصي دقيق لقسم الباركود في Real_Lab_System_Unified_Reference_FINAL.md ضمن هذا التحليل؛ يُنفَّذ ضمن VS-15 (مخاطرة R-06).
وجود واجهة UI صريحة لـ VisitCharge — النموذج موجود، لكن التحقق من وجود XAML وViewModel مخصصين تعذَّر بدون فحص أعمق لكل ملف Views/Patients/*.xaml؛ الافتراض العملي: على الأرجح غير موجود، وشريحة VS-07 تخلقه من الصفر.
موقع كلمات مرور الشاشات الحساسة الحالي — الفحص السطحي لـ LabSetting.cs لم يُظهر أعمدة مخصصة، لكن قد تكون في جدول آخر (Constants/Settings). يُنفَّذ ضمن VS-09 مع تصميم مؤكَّد.
سلوك "تعديل حساب سابق بالنقر" (القرار 16) — لم يتم تأكيد وجوده أو غيابه من فحص XAML؛ VS-08 تشمل التحقق والإزالة إن وُجد.
القرارات الـ23 — تأكيد الظهور
#	القرار	ظهر في
1	إلغاء مسار D:\ الثابت	G9.2, G9.3-5, VS-10
2	First-Run Setup كتحسين	G1.1, G1.2-2
3	القائمة المنسدلة	G1.1, G1.2-1, VS-11
4	لا توحيد رسالة الخادم	G10.2-1
5	Responsive بدل فحص شاشة	G1.2-3, G10.2-2, VS-13
6	لا Splash Screen	G1.1, G1.2-4, VS-14
7	لا "اضغط أي مفتاح"	G10.2-3, VS-14
8	كلمات مرور شاشات منفصلة	G2.2-2, G9.3-4, VS-09
9	SQL Server BACKUP الأصلي	G9.1, G9.3-1/2, VS-10
10	تسمية سياقية	G9.3-3, VS-10
11	5 أعلام / 7 حالات	G6.2, G6.4-2/3/4, G7.3-4, VS-03
12	الفصل الثلاثي + ثنائية تسعير	G5.1-الكامل, G8.2-1, VS-01, VS-02, VS-17
13	Max Discount	G2.2-1, G5.3-5, VS-06
14	واجهة مبلغ إضافي	G5.3-6, VS-07
15	تدفق أوضح	G5.3-7, VS-08
16	لا تعديل بالنقر	G5.3-8, VS-08
17	باركود 13 رقمًا مطابق	G7.3-1, VS-15
18	Luhn كتحسين	G7.3-2, VS-16
19	لا حد للنطاقات	G3.1-الكامل, G3.3-1, R-محذوف
20	نطاق موحد	G3.3-2, VS-12
21	Turnaround بالساعات	G3.2, G3.3-3
22	تسميات For	G6.1-الكامل, G6.4-1, VS-04
23	إزالة Multi-Branch	G7.2, G10.2-4/5, VS-05, R-03
جميع القرارات الـ23 مُدمجة بأثر واضح في الوثيقة.

