ممتاز. سأقدّم خطه العمل النهائية الآن.

PHASE 4 — خطه العمل التنفيذية النهائية
Billing & Contracts — النسخة المُحسّنة
مبنية على:

✅ grill-with-docs — 8 أسئلة استجوابية + 8 قرارات معمارية
✅ domain-modeling — 9 مصطلحات معرّفة
✅ codebase-design — تحليل Depth/Leverage/Deletion Test
✅ to-issues — 6 Vertical Slices
✅ improve-codebase-architecture — 4 فرص تعميق مُعتمدّة
أولاً: القرارات المعمارية المُوثّقة
#	القرار	السبب
1	Company = Contract — لا كيان مستقل	المستخدم أكّد: كل شركة لها عقد واحد
2	ContractPeriod = Dual — بعض الشركات لها تواريخ وبعضها مفتوح	يدعم الفواتير التلقائية واليدوية
3	SchemeId من الزيارة الحالية — لا اختيار يدوي	الأسهل وأدق
4	External results = TestResult عادية — لا حقول إضافية	أبسط تنفيذاً
5	Invoice discount = TotalAfterDiscount مباشرة — لا خصم إضافي	الخصم محسوب عند التسجيل
6	Pricing fallback = سعر 0 + تنبيه — لا DefaultPrice	يُجبر على التحديد اليدوي
7	Payment status = تحديث تلقائي	يمنع الأخطاء
8	Duplicate invoice = لا حماية	بسيط — tech debt محتمل
9	تقسيم IContractService → ICompanyService + IInvoiceService	عمق أعمق
10	تقسيم IExternalLabService → IExternalLabRegistryService + IExternalShipmentService	فصل domains
11	استخراج TestPricingEngine	فصل pricing عن UI
12	إضافة BillingPeriodicity على Company	يدعم الفواتير التلقائية
ثانياً: Vertical Slices (التقسيم التنفيذي)
Slice 1: DI + Company Management (Foundation)
Blocked by: لا شيء Duration: 2 يوم

الخطوة	الملف	الإجراء
1.1	App.xaml.cs	تسجيل ICompanyService + IInvoiceService + IExternalLabRegistryService + IExternalShipmentService + IPricingService + TestPricingEngine
1.2	Models/Company.cs	إضافة ContractStartDate?, ContractEndDate?, BillingPeriodicity?
1.3	Data/FinalLabDbContext.cs	Fluent API للحقول الجديدة
1.4	Migrations/	هجرة جديدة AddCompanyContractFields
1.5	Services/Interfaces/ICompanyService.cs	إنشاء: GetAll, Create, Update
1.6	Services/Implementations/CompanyService.cs	تنفيذ
1.7	ViewModels/Settings/CompaniesWindowViewModel.cs	إنشاء
1.8	Views/Settings/CompaniesWindow.xaml + .cs	إنشاء
1.9	App.xaml.cs	تسجيل VM + Window في Navigation
1.10	Tests	8 اختبارات
Validation: dotnet build + dotnet test — كل اختبارات Phase 1-3 تنجح + 8 جديدة.

Slice 2: Pricing Integration
Blocked by: Slice 1 (يحتاج ICompanyService + TestPricingEngine) Duration: 1.5 يوم

الخطوة	الملف	الإجراء
2.1	Services/Interfaces/IPricingService.cs	إضافة CreateSchemeAsync, UpdateSchemeAsync, GetSchemeByIdAsync
2.2	Services/Implementations/PricingService.cs	تنفيذ
2.3	Infrastructure/TestPricingEngine.cs	إنشاء: ResolvePriceAsync, GetPricingSummaryAsync
2.4	ViewModels/Patients/TestSelectionViewModel.cs	حقن TestPricingEngine + تعديل InitializeAsync لتمرير schemeId
2.5	ViewModels/Settings/PriceSchemeWindowViewModel.cs	إنشاء
2.6	Views/Settings/PriceSchemeWindow.xaml + .cs	إنشاء
2.7	Tests	10 اختبارات
Validation:atient مع شركة لها scheme → الأسعار تختلف عن DefaultPrice.

Slice 3: Invoice Workflow
Blocked by: Slice 1 (يحتاج IInvoiceService) Duration: 2.5 يوم

الخطوة	الملف	الإجراء
3.1	Services/Interfaces/IInvoiceService.cs	GenerateInvoice, RecordPayment, GetInvoices, GetPayments, UpdateStatus
3.2	Services/Implementations/InvoiceService.cs	تنفيذ + تحديث PaidAmount/Status تلقائياً
3.3	ViewModels/Settings/ContractInvoiceWindowViewModel.cs	إنشاء
3.4	Views/Settings/ContractInvoiceWindow.xaml + .cs	إنشاء
3.5	ViewModels/Settings/InvoiceRowViewModel.cs	إنشاء
3.6	ViewModels/Settings/PaymentRowViewModel.cs	إنشاء
3.7	Tests	14 اختبارات
Validation: فاتورة تُولَّد → PaidAmount = 0 → دفعة → PaidAmount يتحدث → Status يتحول.

Slice 4: External Labs
Blocked by: Slice 1 (يحتاج IExternalLabRegistryService + IExternalShipmentService) Duration: 2 يوم

الخطوة	الملف	الإجراء
4.1	Services/Interfaces/IExternalLabRegistryService.cs	GetAll, Create, Update
4.2	Services/Implementations/ExternalLabRegistryService.cs	تنفيذ
4.3	Services/Interfaces/IExternalShipmentService.cs	CreateManifest, ReceiveResults, GetShipments
4.4	Services/Implementations/ExternalShipmentService.cs	تنفيذ
4.5	ViewModels/Settings/ExternalLabsWindowViewModel.cs	إنشاء
4.6	Views/Settings/ExternalLabsWindow.xaml + .cs	إنشاء
4.7	ViewModels/Settings/ExternalLabRowViewModel.cs	إنشاء
4.8	ViewModels/Settings/ShipmentRowViewModel.cs	إنشاء
4.9	Tests	12 اختبارات
Validation: معامل → شحنة → استلام نتائج → TestResult تظهر في DB.

Slice 5: Menu Activation + F7 Wire-up
Blocked by: Slices 1-4 (تحتاج النوافذ جاهزة) Duration: 1 يوم

الخطوة	الملف	الإجراء
5.1	ViewModels/Menu/AccountsMenuViewModel.cs	استبدال Placeholder بـ NavigateToCompaniesCommand + NavigateToPricingCommand + NavigateToInvoicesCommand
5.2	ViewModels/Menu/ExternalSamplesMenuViewModel.cs	استبدال Placeholder بـ NavigateToExternalLabsCommand
5.3	Views/Patients/TestResultsWindow.xaml	التحقق من F7 binding
5.4	Tests	6 اختبارات
Validation: F7 → ExternalLabsWindow. Accounts menu → 3 أوامر حقيقية.

Slice 6: Test Suite + Final Regression
Blocked by: كل Slices السابقة Duration: 3 يوم

الملف	السيناريو	العدد
Tests/Services/CompanyServiceTests.cs	Company CRUD	8
Tests/Services/InvoiceServiceTests.cs	Invoice generation, payment, status	12
Tests/Services/ExternalLabRegistryServiceTests.cs	Lab CRUD	6
Tests/Services/ExternalShipmentServiceTests.cs	Manifest, receive results	8
Tests/Services/PricingServiceTests.cs	Scheme CRUD, price lookup	8
Tests/Infrastructure/TestPricingEngineTests.cs	Resolve price, fallback 0	6
Tests/ViewModels/Settings/CompaniesWindowViewModelTests.cs	Company list, add/edit	6
Tests/ViewModels/Settings/PriceSchemeWindowViewModelTests.cs	Scheme list, price editing	6
Tests/ViewModels/Settings/ContractInvoiceWindowViewModelTests.cs	Invoice generation, payment	8
Tests/ViewModels/Settings/ExternalLabsWindowViewModelTests.cs	Lab management, shipments	6
Tests/ViewModels/Patients/TestSelectionViewModelPricingTests.cs	Scheme vs DefaultPrice	6
Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs	Real submenu	4
الإجمالي		~84
Validation: dotnet test — كل 347 + ~84 = ~431 اختبار تنجح.

ثالثاً: Affected Areas
ملفات للتعديل (~10)
الملف	التغيير
App.xaml.cs	تسجيل 6 خدمات جديدة + VMs/Windows
Models/Company.cs	إضافة 3 حقول اختيارية
Data/FinalLabDbContext.cs	Fluent API للحقول الجديدة
ViewModels/Menu/AccountsMenuViewModel.cs	استبدال Placeholder
ViewModels/Menu/ExternalSamplesMenuViewModel.cs	استبدال Placeholder
ViewModels/Patients/TestSelectionViewModel.cs	حقن TestPricingEngine
Services/Interfaces/IPricingService.cs	إضافة 3 methods
Services/Implementations/PricingService.cs	تنفيذ
Services/Implementations/ContractService.cs	نسخ logics → InvoiceService
Services/Implementations/ExternalLabService.cs	نسخ logics → ShipmentService
Docs/PRDs/IMPLEMENTATION_STATUS.md	تحديث Phase 4
ملفات لإنشاء (~35)
الفئة	الملفات
Services	ICompanyService.cs, CompanyService.cs, IInvoiceService.cs, InvoiceService.cs, IExternalLabRegistryService.cs, ExternalLabRegistryService.cs, IExternalShipmentService.cs, ExternalShipmentService.cs, TestPricingEngine.cs
ViewModels	CompaniesWindowViewModel.cs, PriceSchemeWindowViewModel.cs, ContractInvoiceWindowViewModel.cs, ExternalLabsWindowViewModel.cs, CompanyRowViewModel.cs, InvoiceRowViewModel.cs, PaymentRowViewModel.cs, ExternalLabRowViewModel.cs, ShipmentRowViewModel.cs
Views	CompaniesWindow.xaml+.cs, PriceSchemeWindow.xaml+.cs, ContractInvoiceWindow.xaml+.cs, ExternalLabsWindow.xaml+.cs
Tests	12 ملف (~84 اختبار)
Migration	1 هجرة AddCompanyContractFields
ملفات محتمل حذفها
الملف	السبب
Services/Implementations/ContractService.cs	يُستبدل بـ CompanyService + InvoiceService
Services/Interfaces/IContractService.cs	يُستبدل بـ ICompanyService + IInvoiceService
Services/Implementations/ExternalLabService.cs	يُستبدل بـ ExternalLabRegistryService + ExternalShipmentService
Services/Interfaces/IExternalLabService.cs	يُستبدل بـ IExternalLabRegistryService + IExternalShipmentService
رابعاً: المخاطر المعمارية
المخاطر	الخطورة	التخفيف
تحديث PaidAmount/Status تلقائياً قد يفشل	🔴 High	Transaction + تحقق من PaidAmount قبل Status
Company.DiscountRate هو double لا decimal	🟡 Medium	قبول كـ tech debt، لا أدخل double جديد
لا حماية من فواتير مزدوجة	🟡 Medium	توثيق كـ tech debt
تغيير IContractService يكسر DI registrations	🟠 High	تحديث App.xaml.cs بعد كل تقسيم
BillingPeriodicity migration على بيانات حقيقية	🟡 Medium	defaultValue null — لا يكسر بيانات
خامساً: Validation Gates
Gate	الشرط	الطريقة
G1	Build succeeds	dotnet build — 0 errors
G2	DI resolves all services	dotnet test — DI registration tests
G3	Company CRUD works	Manual test + unit tests
G4	Pricing integration works	Test with company+scheme vs without
G5	Invoice generation correct	Unit tests with multiple companies
G6	Payment auto-update works	Unit tests: Pending→Partial→Paid
G7	External shipment workflow	Unit tests: create→send→receive
G8	F7 opens ExternalLabsWindow	Manual test
G9	Full regression	dotnet test — ~431 tests pass
سادساً: معايير الإكمال
Phase 4 يُعتبر مكتملاً عندما:

✅ 6 خدمات جديدة مسجّلة في DI (ICompanyService, IInvoiceService, IExternalLabRegistryService, IExternalShipmentService, IPricingService موسّعة, TestPricingEngine)
✅ Company تحوي ContractStartDate?, ContractEndDate?, BillingPeriodicity?
✅ CompaniesWindow يعرض ويُعدّل الشركات
✅ PriceSchemeWindow يعرض ويُعدّل تشكيلات الأسعار
✅ TestSelectionViewModel يستخدم TestPricingEngine (لا DefaultPrice)
✅ ContractInvoiceWindow يولّد فواتير ويعرضها
✅ RecordPaymentAsync يحدّث PaidAmount و Status تلقائياً
✅ ExternalLabsWindow يدير المعامل والشحنات
✅ ReceiveResults يكتب TestResult عادية
✅ F7 يفتح ExternalLabsWindow
✅ AccountsMenuViewModel يعرض أوامر حقيقية
✅ ~431 اختبار تنجح (347 + ~84)
✅ IMPLEMENTATION_STATUS.md محدّث
تقدير المدة: 12 يوم عمل مستوى الخطورة: 🔴 Critical (مال + شركات + التزامات قانونية)

