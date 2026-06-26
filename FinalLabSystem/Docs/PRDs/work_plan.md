الآن لدي كل المعلومات. إليك الملف الكامل المحدَّث:

---

```markdown
# PHASE 4 — خطة العمل التنفيذية النهائية
# Billing & Contracts — النسخة المُحسَّنة v2

---

## أولاً: القرارات المعمارية المُوثَّقة (مقفلة)

| # | القرار | السبب |
|---|---|---|
| D1 | Company = Contract — لا كيان مستقل | المستخدم أكّد: كل شركة لها عقد واحد |
| D2 | ContractPeriod = Dual — بعض الشركات لها تواريخ وبعضها مفتوح | يدعم الفواتير التلقائية واليدوية |
| D3 | SchemeId من الزيارة الحالية — لا اختيار يدوي | الأسهل وأدق |
| D4 | External results = TestResult عادية — لا حقول إضافية | أبسط تنفيذاً |
| D5 | Invoice discount = TotalAfterDiscount مباشرة — لا خصم إضافي | الخصم محسوب عند التسجيل |
| D6 | Pricing fallback = سعر 0 + تنبيه IsFallback=true — لا DefaultPrice | يُجبر على التحديد اليدوي |
| D7 | Payment status = تحديث تلقائي ضمن Transaction | يمنع الأخطاء |
| D8 | Duplicate invoice = لا حماية — tech debt مقبول | بسيط |
| D9 | تقسيم IContractService → ICompanyService + IInvoiceService | عمق أعمق |
| D10 | تقسيم IExternalLabService → IExternalLabRegistryService + IExternalShipmentService | فصل domains |
| D11 | استخراج TestPricingEngine كفئة مستقلة | فصل pricing عن UI |
| D12 | إضافة BillingPeriodicity على Company | يدعم الفواتير التلقائية |

---

## ثانياً: Vertical Slices (التقسيم التنفيذي)

---

### 🟦 Slice 1: Company Management (Foundation)
**Blocked by:** لا شيء — **Duration:** 2 يوم — **Risk:** 🟠 High

> ⚠️ تعديل جوهري: لا تُسجَّل في هذا الـ Slice إلا
> ICompanyService و IPricingService فقط.
> باقي الخدمات تُسجَّل كل منها في الـ Slice الذي يُنشئها.

| الخطوة | الملف | الإجراء |
|---|---|---|
| 1.1 | Models/Company.cs | إضافة ContractStartDate? و ContractEndDate? و BillingPeriodicity? (nullable string: "Monthly"/"Quarterly") |
| 1.2 | Data/FinalLabDbContext.cs | Fluent API للحقول الثلاثة بين السطر 308 و 312: HasColumnName("contract_start_date") و HasColumnName("contract_end_date") و HasColumnName("billing_periodicity") مع HasMaxLength(20) |
| 1.3 | Migrations/ | dotnet ef migrations add AddCompanyContractFields — التحقق من Up: AddColumn للحقول الثلاثة فقط، defaultValue = null |
| 1.4 | Services/Interfaces/ICompanyService.cs | إنشاء: GetAllAsync(), GetByIdAsync(int), CreateAsync(Company), UpdateAsync(Company) — لا Delete، soft delete عبر IsActive |
| 1.5 | Services/Implementations/CompanyService.cs | تنفيذ ICompanyService — يحقن FinalLabDbContext + ILogger |
| 1.6 | App.xaml.cs | تسجيل بعد السطر 160 فقط: services.AddScoped\<ICompanyService, CompanyService\>(); و services.AddScoped\<IPricingService, PricingService\>(); |
| 1.7 | ViewModels/Settings/CompaniesWindowViewModel.cs | Master-detail VM — ObservableCollection\<CompanyRowViewModel\>، SaveCommand، AddCommand، RefreshCommand. حقول التعديل: name, type, contact, phones, email, address, scheme picker, discount rate, credit limit, payment terms, ContractStartDate, ContractEndDate, BillingPeriodicity (combo)، notes، IsActive |
| 1.8 | ViewModels/Settings/CompanyRowViewModel.cs | إنشاء |
| 1.9 | Views/Settings/CompaniesWindow.xaml + .cs | إنشاء — code-behind نظيف MVVM |
| 1.10 | App.xaml.cs | تسجيل: services.AddTransient\<CompaniesWindowViewModel\>(); و services.AddTransient\<CompaniesWindow\>(); و navigation.RegisterWindow\<CompaniesWindowViewModel, CompaniesWindow\>(); |
| 1.11 | Tests/Services/CompanyServiceTests.cs | 8 اختبارات: CRUD + IsActive filter + validation |

**Validation Gate G1:**
```

dotnet build — 0 errors
dotnet test — كل اختبارات Phase 1-3 تنجح + 8 جديدة
```

---

### 🟦 Slice 2: Pricing Integration
**Blocked by:** Slice 1 — **Duration:** 1.5 يوم — **Risk:** 🟠 High

| الخطوة | الملف | الإجراء |
|---|---|---|
| 2.1 | Services/Interfaces/IPricingService.cs | إضافة: GetSchemeByIdAsync(int id)، CreateSchemeAsync(PriceScheme s)، UpdateSchemeAsync(PriceScheme s) |
| 2.2 | Services/Implementations/PricingService.cs | تنفيذ الدوال الثلاث الجديدة |
| 2.3 | Infrastructure/TestPricingEngine.cs | إنشاء فئة Scoped تحقن IPricingService. التوقيع: Task\<TestPricingResultDto\> ResolvePriceAsync(int testTypeId, int? schemeId) و Task\<List\<TestPricingResultDto\>\> GetPricingSummaryAsync(IEnumerable\<int\> testTypeIds, int? schemeId). المنطق: إن schemeId == null أو لم يوجد سعر → Price=0 + IsFallback=true (D6) |
| 2.4 | Models/DTOs/TestPricingResultDto.cs | إنشاء: int TestTypeId، decimal Price، bool IsFallback |
| 2.5 | App.xaml.cs | إضافة بعد تسجيلات Slice 1: services.AddScoped\<TestPricingEngine\>(); |
| 2.6 | ViewModels/Patients/TestSelectionViewModel.cs | حقن TestPricingEngine في constructor. في InitializeAsync تمرير schemeId من Visit.SchemeId (Models/Visit.cs سطر 70). استبدال 3 مواضع DefaultPrice (سطور 51، 71، 268) بـ await _pricingEngine.ResolvePriceAsync(...). إذا IsFallback=true أظهر تنبيهاً مرئياً في الـ row |
| 2.7 | ViewModels/Settings/PriceSchemeWindowViewModel.cs | إنشاء — Master-detail للـ schemes وأسعار التحاليل |
| 2.8 | Views/Settings/PriceSchemeWindow.xaml + .cs | إنشاء |
| 2.9 | App.xaml.cs | تسجيل VM + Window + Navigation |
| 2.10 | Tests | Tests/Services/PricingServiceTests.cs (4 اختبارات: Scheme CRUD + price lookup) و Tests/Infrastructure/TestPricingEngineTests.cs (6 اختبارات: resolve + fallback path) |

**ملاحظة:** تحديث اختبارات TestSelectionViewModelProfileApplyTests بإضافة mock للـ TestPricingEngine — السلوك لا يتغير، فقط الـ constructor signature يتوسع.

**Validation Gate G2:**
```

dotnet test — 343 + 18 = 361 اختبار ناجح
مريض مع شركة لها scheme → الأسعار تختلف عن DefaultPrice
```

---

### 🟦 Slice 3: Invoice Workflow
**Blocked by:** Slice 1 — **Duration:** 2.5 يوم — **Risk:** 🔴 Critical

> ⚠️ تعديل جوهري: توحيد قيم Status
> القيم المعتمدة: Pending / Partial / Paid
> (بدلاً من DRAFT / ISSUED / PAID الموجودة في الكود الحالي)

| الخطوة | الملف | الإجراء |
|---|---|---|
| 3.1 | Models/ContractInvoice.cs | تعديل سطر 26: Status = "Pending" بدلاً من "DRAFT". إضافة تعليق: // القيم المعتمدة: Pending / Partial / Paid |
| 3.2 | ContractService.cs | استبدال أي قيمة "DRAFT" أو "ISSUED" أو "PAID" بالقيم الجديدة |
| 3.3 | Migrations/ (اختياري) | إذا وجدت بيانات قديمة في قاعدة البيانات أنشئ Migration باسم NormalizeInvoiceStatus تحتوي: Sql("UPDATE ContractInvoices SET status='Pending' WHERE status='DRAFT'"); و Sql("UPDATE ContractInvoices SET status='Paid' WHERE status='PAID'"); |
| 3.4 | Services/Interfaces/IInvoiceService.cs | إنشاء: GenerateInvoiceAsync(int companyId, int year, int month, int staffId)، RecordPaymentAsync(int invoiceId, decimal amount, string method, string? reference, int staffId)، GetInvoicesAsync(int companyId)، GetPaymentsAsync(int invoiceId)، UpdateStatusAsync(int invoiceId, string status) |
| 3.5 | Services/Implementations/InvoiceService.cs | تنفيذ: GenerateInvoiceAsync يفلتر Visits بـ CompanyId وVisitDate ضمن الشهر، يجمع TotalAfterDiscount (D5)، يطبق Company.DiscountRate، يُنشئ ContractInvoice بـ Status="Pending". RecordPaymentAsync يُنشئ ContractPayment + يُحدِّث PaidAmount + يحسب Status الجديد ضمن Transaction (D7): إن PaidAmount < TotalAmount → "Partial"، وإلا → "Paid". ContractService.cs القديم يتحول إلى Adapter يستدعي IInvoiceService داخلياً |
| 3.6 | App.xaml.cs | services.AddScoped\<IInvoiceService, InvoiceService\>(); |
| 3.7 | Services/Printing/ContractInvoiceTemplate.cs | إنشاء — يرث من DocumentTemplateBase. يُنشئ FlowDocument يحتوي: اسم الشركة، الفترة (شهر+سنة)، إجمالي الفاتورة، المبلغ المدفوع، الرصيد المتبقي، جدول الزيارات المشمولة |
| 3.8 | ViewModels/Settings/ContractInvoiceWindowViewModel.cs | إنشاء — اختيار شركة → عرض فواتيرها، زر "توليد فاتورة الشهر الحالي"، اختيار فاتورة → عرض دفعاتها، زر "تسجيل دفعة"، PrintInvoiceCommand يستخدم IPrintService |
| 3.9 | ViewModels/Settings/InvoiceRowViewModel.cs | إنشاء |
| 3.10 | ViewModels/Settings/PaymentRowViewModel.cs | إنشاء |
| 3.11 | Views/Settings/ContractInvoiceWindow.xaml + .cs | إنشاء |
| 3.12 | App.xaml.cs | تسجيل VM + Window + Navigation |
| 3.13 | Tests | Tests/Services/InvoiceServiceTests.cs (8 اختبارات: generation، partial payment، full payment، status transitions) و Tests/ViewModels/Settings/ContractInvoiceWindowViewModelTests.cs (6 اختبارات) |

**Validation Gate G3:**
```

فاتورة تُولَّد → PaidAmount = 0 → Status = "Pending"
دفعة جزئية → Status = "Partial"
دفعة كاملة → Status = "Paid"
زر طباعة → FlowDocument يُنتج بدون استثناء
dotnet test — 361 + 14 = 375 اختبار ناجح
```

---

### 🟦 Slice 4: External Labs Management & Shipments
**Blocked by:** Slice 1 — **Duration:** 2 يوم — **Risk:** 🟠 High

| الخطوة | الملف | الإجراء |
|---|---|---|
| 4.1 | Services/Interfaces/IExternalLabRegistryService.cs | إنشاء: GetAllAsync()، GetByIdAsync(int)، CreateAsync(ExternalLab)، UpdateAsync(ExternalLab) — soft delete عبر IsActive (BR-043) |
| 4.2 | Services/Implementations/ExternalLabRegistryService.cs | تنفيذ |
| 4.3 | Services/Interfaces/IExternalShipmentService.cs | إنشاء: CreateManifestAsync(int externalLabId, List\<int\> visitTestIds, int staffId)، GetShipmentsAsync(int externalLabId)، ReceiveResultsAsync(int shipmentItemId, string resultValue, int staffId)، UpdateStatusAsync(int shipmentId, string status) |
| 4.4 | Services/Implementations/ExternalShipmentService.cs | نقل منطق CreateOutsourceManifestAsync من ExternalLabService.cs كما هو. ReceiveResultsAsync يُنشئ TestResult عادية مرتبطة بـ VisitTest الأصلية (D4)، يُحدِّث ExternalShipmentItem.Status="Received" وVisitTest.CurrentStage إلى Resulted |
| 4.5 | App.xaml.cs | services.AddScoped\<IExternalLabRegistryService, ExternalLabRegistryService\>(); و services.AddScoped\<IExternalShipmentService, ExternalShipmentService\>(); |
| 4.6 | ViewModels/Settings/ExternalLabsWindowViewModel.cs | إنشاء — تابات: Labs، Pending Shipments، Send Manifest، Receive Results |
| 4.7 | ViewModels/Settings/ExternalLabRowViewModel.cs | إنشاء |
| 4.8 | ViewModels/Settings/ShipmentRowViewModel.cs | إنشاء |
| 4.9 | Views/Settings/ExternalLabsWindow.xaml + .cs | إنشاء |
| 4.10 | App.xaml.cs | تسجيل VM + Window + Navigation |
| 4.11 | Tests | Tests/Services/ExternalLabRegistryServiceTests.cs (6 اختبارات: Lab CRUD + soft delete) و Tests/Services/ExternalShipmentServiceTests.cs (8 اختبارات: manifest + receive + Stage transitions) |

**Validation Gate G4:**
```

معامل → شحنة → استلام نتائج → TestResult تظهر في DB
dotnet test — 375 + 14 = 389 اختبار ناجح
```

---

### 🟦 Slice 5: Menu Activation + F7 Wire-up
**Blocked by:** Slices 1-4 — **Duration:** 1 يوم — **Risk:** 🟢 Low

| الخطوة | الملف | الإجراء |
|---|---|---|
| 5.1 | ViewModels/Menu/AccountsMenuViewModel.cs | حقن INavigationService. استبدال PlaceholderCommand بـ 3 أوامر حقيقية: NavigateToCompaniesCommand و NavigateToPricingCommand و NavigateToInvoicesCommand. زر "Cash Drawer" يبقى placeholder يعرض "ستتوفر في المرحلة 5" |
| 5.2 | ViewModels/Menu/ExternalSamplesMenuViewModel.cs | استبدال PlaceholderCommand بـ NavigateToExternalLabsCommand حقيقي |
| 5.3 | ViewModels/Patients/PatientRegistrationViewModel.cs | سطر 79: استبدال _dialogService.ShowMessage(...) بـ _navigationService.OpenTaskWindow\<ExternalLabsWindowViewModel\>() |
| 5.4 | ViewModels/Patients/TestResultsViewModel.cs | سطر 127: نفس التعديل. إضافة INavigationService للـ constructor إذا لم يكن موجوداً |
| 5.5 | Tests | Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs (4 اختبارات: 3 أوامر حقيقية + Cash Drawer placeholder) و Tests/ViewModels/Settings/ExternalLabsWindowViewModelTests.cs اختبارات F7 wiring (2 اختبارات) |

**Validation Gate G5:**
```

F7 في PatientRegistrationWindow → ExternalLabsWindow تفتح
F7 في TestResultsWindow → ExternalLabsWindow تفتح
Accounts menu → 3 أوامر حقيقية + Cash Drawer placeholder
dotnet test — 389 + 6 = 395 اختبار ناجح
```

---

### 🟦 Slice 6: Test Coverage + Cleanup + Status Update
**Blocked by:** Slices 1-5 — **Duration:** 3 يوم — **Risk:** 🟢 Low

| الخطوة | الملف | الإجراء |
|---|---|---|
| 6.1 | Tests/Services/CompanyServiceTests.cs | (مكتمل من Slice 1) |
| 6.2 | Tests/Services/PricingServiceTests.cs | (مكتمل من Slice 2) |
| 6.3 | Tests/Infrastructure/TestPricingEngineTests.cs | (مكتمل من Slice 2) |
| 6.4 | Tests/Services/InvoiceServiceTests.cs | (مكتمل من Slice 3) |
| 6.5 | Tests/ViewModels/Settings/ContractInvoiceWindowViewModelTests.cs | (مكتمل من Slice 3) |
| 6.6 | Tests/Services/ExternalLabRegistryServiceTests.cs | (مكتمل من Slice 4) |
| 6.7 | Tests/Services/ExternalShipmentServiceTests.cs | (مكتمل من Slice 4) |
| 6.8 | Tests/ViewModels/Menu/AccountsMenuViewModelTests.cs | (مكتمل من Slice 5) |
| 6.9 | Tests/ViewModels/Settings/CompaniesWindowViewModelTests.cs | 6 اختبارات: Company list + add/edit |
| 6.10 | Tests/ViewModels/Settings/PriceSchemeWindowViewModelTests.cs | 6 اختبارات: Scheme list + price editing |
| 6.11 | Tests/ViewModels/Settings/ExternalLabsWindowViewModelTests.cs | 6 اختبارات: Lab management + shipments |
| 6.12 | Tests/ViewModels/Patients/TestSelectionViewModelPricingTests.cs | 6 اختبارات: Scheme vs DefaultPrice |
| 6.13 | Tests/Integration/InvoiceWorkflowEndToEndTests.cs | 5 اختبارات: company→visit→invoice→payment |
| 6.14 | Tests/Integration/ExternalShipmentEndToEndTests.cs | 5 اختبارات: manifest→send→receive→TestResult |
| 6.15 | Tests/Validation/CompanyContractFieldsValidationTests.cs | 4 اختبارات: Migration + nullable behavior |
| 6.16 | Services/Interfaces/IContractService.cs | إضافة XML doc: /// Deprecated: استخدم ICompanyService و IInvoiceService — سيُحذف في Phase 7 |
| 6.17 | Services/Implementations/ContractService.cs | تحويله إلى Adapter يستدعي IInvoiceService داخلياً |
| 6.18 | Services/Interfaces/IExternalLabService.cs | إضافة XML doc: /// Deprecated: استخدم IExternalLabRegistryService و IExternalShipmentService — سيُحذف في Phase 7 |
| 6.19 | Services/Implementations/ExternalLabService.cs | تحويله إلى Adapter يستدعي IExternalShipmentService داخلياً |
| 6.20 | Docs/PRDs/IMPLEMENTATION_STATUS.md | تحديث قسم Phase 4 من "⏳ في الانتظار" إلى قسم كامل على غرار Phase 1/2/3 يشمل: إجمالي الملفات المضافة والمعدَّلة، إجمالي الاختبارات، Migration الجديدة |
| 6.21 | Docs/PRDs/IMPLEMENTATION_STATUS.md | إضافة قسم "Tech Debt من Phase 4": TD-1: لا حماية من فاتورة مكرّرة (D8 — مخالف لـ V4 سطر 856) / TD-2: إرسال الفاتورة بالإيميل مؤجّل لـ Phase 6 / TD-3: Company.DiscountRate هو double (مقبول) / TD-4: IContractService و IExternalLabService لم يُحذفا — مقرر في Phase 7 |

**Validation Gate G6 — النهائي:**
```

dotnet test — كل 347 + ~88 = ~435 اختبار ناجح
```

---

## ثالثاً: الملفات المتأثرة

### ملفات للتعديل (~12)

| الملف | التغيير |
|---|---|
| App.xaml.cs | تسجيل 6 خدمات + VMs + Windows + Navigation (موزّعة على Slices) |
| Models/Company.cs | إضافة 3 حقول nullable |
| Models/ContractInvoice.cs | توحيد Status: Pending/Partial/Paid |
| Data/FinalLabDbContext.cs | Fluent API للحقول الجديدة |
| ViewModels/Menu/AccountsMenuViewModel.cs | استبدال Placeholder بـ 3 أوامر حقيقية |
| ViewModels/Menu/ExternalSamplesMenuViewModel.cs | استبدال Placeholder |
| ViewModels/Patients/TestSelectionViewModel.cs | حقن TestPricingEngine |
| ViewModels/Patients/PatientRegistrationViewModel.cs | F7 wire-up حقيقي |
| ViewModels/Patients/TestResultsViewModel.cs | F7 wire-up حقيقي |
| Services/Interfaces/IPricingService.cs | إضافة 3 methods |
| Services/Implementations/PricingService.cs | تنفيذ |
| Services/Implementations/ContractService.cs | تحويل إلى Adapter |
| Services/Implementations/ExternalLabService.cs | تحويل إلى Adapter |
| Services/Interfaces/IContractService.cs | إضافة Deprecated doc |
| Services/Interfaces/IExternalLabService.cs | إضافة Deprecated doc |
| Docs/PRDs/IMPLEMENTATION_STATUS.md | تحديث Phase 4 + Tech Debts |

### ملفات للإنشاء (~37)

| الفئة | الملفات |
|---|---|
| Services | ICompanyService.cs، CompanyService.cs، IInvoiceService.cs، InvoiceService.cs، IExternalLabRegistryService.cs، ExternalLabRegistryService.cs، IExternalShipmentService.cs، ExternalShipmentService.cs، TestPricingEngine.cs |
| Printing | ContractInvoiceTemplate.cs |
| DTOs | TestPricingResultDto.cs |
| ViewModels | CompaniesWindowViewModel.cs، PriceSchemeWindowViewModel.cs، ContractInvoiceWindowViewModel.cs، ExternalLabsWindowViewModel.cs، CompanyRowViewModel.cs، InvoiceRowViewModel.cs، PaymentRowViewModel.cs، ExternalLabRowViewModel.cs، ShipmentRowViewModel.cs |
| Views | CompaniesWindow.xaml+.cs، PriceSchemeWindow.xaml+.cs، ContractInvoiceWindow.xaml+.cs، ExternalLabsWindow.xaml+.cs |
| Tests | 15 ملف اختبار (~88 اختبار) |
| Migrations | 1-2 هجرة (AddCompanyContractFields + NormalizeInvoiceStatus اختياري) |

---

## رابعاً: المخاطر المعمارية

| الخطر | الخطورة | التخفيف |
|---|---|---|
| تحديث PaidAmount/Status يفشل في منتصف Transaction | 🔴 High | اتباع نمط using var transaction = await _context.Database.BeginTransactionAsync() كما في ExternalLabService.cs سطر 27 |
| تسجيل DI قبل إنشاء الملفات يكسر البناء | 🔴 High | تسجيل كل خدمة في الـ Slice الذي يُنشئها فقط |
| تعارض قيم Status بين الكود القديم والجديد | 🟠 High | توحيد في Slice 3 أولاً قبل كتابة أي اختبار |
| Company.DiscountRate هو double لا decimal | 🟡 Medium | قبول كـ tech debt، لا تدخل double جديد |
| لا حماية من فواتير مزدوجة | 🟡 Medium | توثيق كـ tech debt في IMPLEMENTATION_STATUS |
| Migration على بيانات حقيقية | 🟡 Medium | defaultValue = null — لا يكسر بيانات قديمة |

---

## خامساً: Validation Gates الكاملة

| Gate | الشرط | الطريقة |
|---|---|---|
| G1 | Build + Slice 1 tests | dotnet build 0 errors + 8 اختبارات جديدة |
| G2 | Pricing integration | مريض بشركة لها scheme → أسعار مختلفة عن DefaultPrice |
| G3 | Invoice workflow | Pending → Partial → Paid + PDF يُنتج |
| G4 | External shipment workflow | manifest → send → receive → TestResult في DB |
| G5 | F7 + Menus | F7 يفتح ExternalLabsWindow، Accounts menu 3 أوامر حقيقية |
| G6 | Full regression | dotnet test — ~435 اختبار ناجح |

---

## سادساً: معايير الإكمال

Phase 4 يُعتبر مكتملاً عندما:

```

☐ ICompanyService مسجَّل في DI (Scoped)
☐ IPricingService مسجَّل في DI (Scoped)
☐ TestPricingEngine مسجَّل في DI (Scoped)
☐ IInvoiceService مسجَّل في DI (Scoped)
☐ IExternalLabRegistryService مسجَّل في DI (Scoped)
☐ IExternalShipmentService مسجَّل في DI (Scoped)
☐ Company تحوي ContractStartDate?، ContractEndDate?، BillingPeriodicity?
☐ Migration AddCompanyContractFields طُبِّقت على .\SQLEXPRESS\FinalLab
☐ قيم Status موحَّدة: Pending / Partial / Paid
☐ CompaniesWindow تعمل (إضافة/تعديل/تفعيل-تعطيل)
☐ PriceSchemeWindow تعمل (CRUD scheme + تعديل أسعار)
☐ TestSelectionViewModel يستخدم TestPricingEngine (لا DefaultPrice)
☐ ContractInvoiceWindow تولِّد فواتير وتعرضها وتقبل دفعات
☐ RecordPaymentAsync يحدِّث PaidAmount و Status تلقائياً ضمن Transaction
☐ ContractInvoiceTemplate يولِّد FlowDocument قابل للطباعة (BR-041)
☐ ExternalLabsWindow تدير المعامل + الشحنات
☐ Manifest العينات يُنشأ بـ TrackingNumber صحيح
☐ ReceiveResultsAsync يكتب TestResult عادية (D4)
☐ F7 يفتح ExternalLabsWindow في PatientRegistrationWindow و TestResultsWindow
☐ AccountsMenuViewModel يعرض 3 أوامر حقيقية + Cash Drawer placeholder للمرحلة 5
☐ ExternalSamplesMenuViewModel يعرض أمر حقيقي (لا placeholder)
☐ IContractService و IExternalLabService محوَّلان إلى Adapters (لا محذوفان)
☐ ~88 اختبار جديد ناجح في 15 ملف
☐ الاختبارات السابقة الـ 347 لا تزال ناجحة
☐ IMPLEMENTATION_STATUS.md مُحدَّث بقسم Phase 4 كامل
☐ Tech Debts الأربعة مُوثَّقة في IMPLEMENTATION_STATUS

```

---

**تقدير المدة:** 12 يوم عمل
**مستوى الخطورة:** 🔴 Critical (مال + شركات + التزامات قانونية)
```