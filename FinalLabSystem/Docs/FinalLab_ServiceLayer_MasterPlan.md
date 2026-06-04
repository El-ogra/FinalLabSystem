# 🏛 الخطة الهندسية الرئيسية لطبقة الخدمات (Service Layer Master Plan) — FinalLab V4.0 (DDD & SRP Edition)

## 📑 الفهرس
1. [المقدمة والمبادئ المعمارية (DDD & SRP)](#المقدمة-والمبادئ-المعمارية)
2. [المرحلة 1: خدمات الهوية والإدارة التأسيسية (Identity & Core)](#المرحلة-1)
3. [المرحلة 2: خدمات القواميس الطبية والتسعير (Dictionaries & Pricing)](#المرحلة-2)
4. [المرحلة 3: خدمات العمليات وسحب العينات (Operations & Samples)](#المرحلة-3)
5. [المرحلة 4: الخدمات الإكلينيكية والطبية (Clinical Domains)](#المرحلة-4)
6. [المرحلة 5: الخدمات المالية والتعاقدات (Finance & B2B)](#المرحلة-5)
7. [المرحلة 6: خدمات التقارير والتدقيق (Reporting & Auditing)](#المرحلة-6)
8. [قواعد التنفيذ الفنية (EF Core Rules)](#قواعد-التنفيذ)

---

## المقدمة والمبادئ المعمارية
بناءً على التوجيهات المعمارية العليا للإصدار 4.0، تم إلغاء القيود القديمة التي كانت تحصر النظام في 9 خدمات منتفخة (God Objects). تم تصميم هذه الخطة بناءً على **Domain-Driven Design (DDD)** ومبدأ **Single Responsibility Principle (SRP)**.

سيتم تقسيم النظام إلى **18 خدمة دقيقة (Granular Services)** تتعامل مع 46 نموذجاً (Entity & View)، مما يضمن قابلية الصيانة، التوسع، والاختبار المستقل لكل مجال عمل (Domain).

**المتطلبات التقنية الصارمة:**
- حقن التبعيات (Dependency Injection) حصري عبر `FinalLabDbContext`.
- الإرجاع غير المتزامن 100% `Task<T>`.
- استخدام `IDbContextTransaction` في العمليات الموزعة.
- التضمين المسبق (Eager Loading `.Include()`) لتقليل طلبات قاعدة البيانات.

---

## المرحلة 1: خدمات الهوية والإدارة التأسيسية
*(التركيز على الوصول الأساسي للنظام وشؤون الموظفين والإعدادات)*

### 1. `IAuthService` (خدمة المصادقة والصلاحيات)
* **الجداول:** `Staff`، `Permission`، `StaffPermission`.
* **التواقيع:**
  ```csharp
  Task<Staff?> LoginAsync(string username, string password);
  Task<bool> HasPermissionAsync(int staffId, string permissionCode);
  Task UpdateLastLoginAsync(int staffId);
  Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds);
  ```
* **التقنيات:** `HasPermissionAsync` يستخدم `.Include(sp => sp.Permission)`. 

### 2. `ISettingsService` (خدمة إعدادات النظام)
* **الجداول:** `LabSetting`.
* **التواقيع:**
  ```csharp
  Task<string?> GetSettingValueAsync(string key);
  Task UpsertSettingAsync(LabSetting setting, int staffId);
  Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName);
  ```

### 3. `IAttendanceService` (خدمة الحضور والورديات)
* **الجداول:** `WorkShift`، `Attendance`.
* **التواقيع:**
  ```csharp
  Task<List<WorkShift>> GetActiveShiftsAsync();
  Task RecordClockInAsync(int staffId, int shiftId);
  Task RecordClockOutAsync(int staffId);
  ```
* **التقنيات:** حساب التفاوت (LateMinutes) بين `ClockInTime` الرسمي ووقت الـ Clock-In الفعلي.

---

## المرحلة 2: خدمات القواميس الطبية والتسعير
*(التركيز على بناء البنية التحتية لكتالوج المختبر)*

### 4. `ITestCatalogService` (خدمة كتالوج التحاليل)
* **الجداول:** `TestCategory`، `TestGroup`، `TestType`، `TestComponent`، `NormalRange`، `ReportCommentTemplate`، `TestProfile`، `TestProfileItem`.
* **التواقيع:**
  ```csharp
  Task<List<TestCategory>> GetFullHierarchyAsync(); // Category -> Group -> Type
  Task<TestType?> GetTestTypeDetailsAsync(int testTypeId); // Includes Components & NormalRanges
  Task<List<TestProfile>> GetActiveProfilesAsync();
  Task<List<TestType>> GetProfileTestsAsync(int profileId);
  ```

### 5. `IPricingService` (خدمة التسعير والقوائم)
* **الجداول:** `PriceScheme`، `TestTypePrice`.
* **التواقيع:**
  ```csharp
  Task<List<PriceScheme>> GetAllSchemesAsync();
  Task<double> GetTestPriceAsync(int testTypeId, int schemeId);
  Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices);
  ```

---

## المرحلة 3: خدمات العمليات وسحب العينات
*(التركيز على مسار المريض داخل المعمل)*

### 6. `IPatientService` (خدمة المرضى)
* **الجداول:** `Patient`، `PatientMedicalHistory`.
* **التواقيع:**
  ```csharp
  Task<Patient> RegisterPatientAsync(Patient patient);
  Task<List<Patient>> SearchPatientsAsync(string searchTerm);
  Task AddMedicalHistoryAsync(PatientMedicalHistory history);
  Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);
  ```

### 7. `IVisitService` (خدمة الزيارات والفواتير التشغيلية)
* **الجداول:** `Visit`، `VisitTest`، `VisitCharge`.
* **التواقيع:**
  ```csharp
  Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges);
  Task CancelVisitTestAsync(int visitTestId);
  Task<Visit?> GetVisitSummaryAsync(int visitId);
  ```
* **التقنيات:** **`IDbContextTransaction` إلزامي** لحقن الفحوصات الفردية، وتفكيك الفحوصات داخل الـ Profiles، وحقن الرسوم الإضافية `VisitCharge`. التحديث المالي يتم عبر الـ DB Triggers.

### 8. `ISampleTrackingService` (خدمة تتبع العينات)
* **الجداول:** `SampleTube`، `TestWorkflow`.
* **التواقيع:**
  ```csharp
  Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId);
  Task UpdateTestStageAsync(int visitTestId, string newStage, int staffId);
  ```
* **التقنيات:** تجميع التحاليل المتشابهة لونياً في `SampleTube` واحد. وإضافة سجل في `TestWorkflow` مع تحديث `CurrentStage` في `VisitTest`.

---

## المرحلة 4: الخدمات الإكلينيكية والطبية
*(تم فصل التخصصات الطبية لاحترام مبدأ SRP وتجنب تضخم خدمة النتائج)*

### 9. `IResultService` (خدمة النتائج القياسية)
* **الجداول:** `TestResult`.
* **التواقيع:**
  ```csharp
  Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId);
  Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId);
  ```
* **التقنيات:** فحص تلقائي לـ `ResultNumeric` ضد `NormalRange` بناءً على عمر وجنس المريض، وتحديد حالة `ResultStatus` كـ NORMAL/HIGH/LOW.

### 10. `IMicrobiologyService` (خدمة المزارع والميكروبيولوجي)
* **الجداول:** `MicrobiologyCulture`، `MicrobiologyOrganism`، `OrganismAntibiotic`، `AntibioticCatalog`.
* **التواقيع:**
  ```csharp
  Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);
  Task SaveCultureAsync(MicrobiologyCulture culture);
  Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms);
  ```
* **التقنيات:** إدارة السلسلة الثلاثية المترابطة (Culture -> Organism -> Antibiotics).

### 11. `IBloodBankService` (خدمة بنك الدم والتوافق)
* **الجداول:** `CrossMatchTest`، `CrossMatchDonor`.
* **التواقيع:**
  ```csharp
  Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId);
  Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId);
  ```

### 12. `IAndrologyService` (خدمة تحاليل الذكورة)
* **الجداول:** `SemenAnalysis`.
* **التواقيع:**
  ```csharp
  Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId);
  ```

---

## المرحلة 5: الخدمات المالية والتعاقدات
*(إدارة B2B والماليات)*

### 13. `IFinancialService` (الخدمة المالية للمرضى)
* **الجداول:** `Payment`.
* **التواقيع:**
  ```csharp
  Task RecordPatientPaymentAsync(Payment payment);
  Task ApplyDiscountAsync(int visitId, double discount, int staffId);
  ```
* **التقنيات:** الاعتماد على الـ DB Trigger `TR_Payment_SyncBalance` في إعادة حساب المجاميع.

### 14. `IContractService` (خدمة الشركات والمطالبات)
* **الجداول:** `Company`، `ContractInvoice`، `ContractPayment`.
* **التواقيع:**
  ```csharp
  Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId);
  Task RecordCorporatePaymentAsync(ContractPayment payment);
  ```
* **التقنيات:** تجميع `TotalAfterDiscount` للزيارات غير المسددة المرتبطة بالشركة لإنشاء الفاتورة.

### 15. `IReferralService` (خدمة الإحالات والأطباء)
* **الجداول:** `ReferralSource`.
* **التواقيع:**
  ```csharp
  Task<ReferralSource> AddReferralSourceAsync(ReferralSource source);
  Task LinkReferralToSchemeAsync(int referralId, int schemeId);
  ```

### 16. `IExternalLabService` (خدمة المعامل الخارجية)
* **الجداول:** `ExternalLab`، `ExternalShipment`، `ExternalShipmentItem`.
* **التواقيع:**
  ```csharp
  Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId);
  Task UpdateShipmentStatusAsync(int shipmentId, string status);
  ```
* **التقنيات:** تعديل `IsOutsourced = true` للتحاليل المرسلة.

---

## المرحلة 6: خدمات التقارير والتدقيق
*(قراءة فقط - High Performance Views)*

### 17. `IReportingService` (خدمة الإحصاء والتقارير)
* **الـ Views المرتبطة:** `VOutstandingBalance`، `VPatientHistory`، `VPendingTest`، `VReferralCommissionReport`، `VSampleTubeStatus`.
* **التواقيع:**
  ```csharp
  Task<List<VPendingTest>> GetPendingWorksheetsAsync();
  Task<List<VOutstandingBalance>> GetDefaultersListAsync();
  Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end);
  Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId);
  Task<object> GetDashboardMetricsAsync(DateTime date);
  ```

### 18. `IAuditService` (خدمة تتبع التدقيق)
* **الجداول والمرئيات:** `AuditLog`، `VResultAuditTrail`.
* **التواقيع:**
  ```csharp
  Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId);
  Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId);
  ```

---

## قواعد التنفيذ الفنية (EF Core Rules)

1. **لا لـ God Objects:** تم تقسيم النظام إلى 18 واجهة. يجب على الوكيل البرمجي تنفيذ واجهة واحدة في كل مرة بدقة.
2. **منع `new FinalLabDbContext()`:** الاعتماد 100% على الـ Dependency Injection في المُنشئ (`constructor`).
3. **التزامن (Concurrency):** الإرجاع يجب أن يكون `Task` عبر وظائف `.ToListAsync()`، `.FirstOrDefaultAsync()`. لا تستخدم `.Result` أو `.Wait()` لمنع إغلاق واجهة WPF.
4. **عزل الـ UI:** لا يوجد أي كود مخصص لـ `MessageBox` أو غيره. تُدار الأخطاء عبر `Exceptions` مخصصة. 

---
**تمت الموافقة الهندسية. هذه هي النسخة النهائية والملزمة لدورة حياة التطوير (SDLC) للإصدار 4.0.**
