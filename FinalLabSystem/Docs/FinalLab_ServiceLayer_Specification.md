
# مواصفات وتصميم طبقة الخدمات — FinalLab System
## Service Layer Architecture Specification (Version 4.0)

---

> **الهدف:** توحيد وتوجيه بناء طبقة الخدمات (Service Layer) في مشروع FinalLabSystem اعتماداً على بنية قاعدة البيانات المحدثة (الإصدار 4.0)، وتوفير سياق برمجي صارم للوكلاء البرمجيين لتطبيق نمط MVVM وحقن التبعيات (Dependency Injection).

---

## 1. الهيكل المجلد العام داخل `Services/`

يجب الالتزام التام بفصل العقود (Interfaces) عن التنفيذ (Implementations) لضمان مرونة النظام وقابلية اختبار الكود:


```
FinalLabSystem/
└── Services/
├── Interfaces/          ← عقود الخدمات البرمجية
│   ├── IAuthService.cs
│   ├── ISettingsService.cs
│   ├── ITestCatalogService.cs
│   ├── IPatientService.cs
│   ├── IVisitService.cs
│   ├── IResultService.cs
│   ├── IFinancialService.cs
│   ├── IAttendanceService.cs
│   └── IExternalLabService.cs
│
└── Implementations/     ← التنفيذ الفعلي للخدمات باستخدام EF Core
├── AuthService.cs
├── SettingsService.cs
├── TestCatalogService.cs
├── PatientService.cs
├── VisitService.cs
├── ResultService.cs
├── FinancialService.cs
├── AttendanceService.cs
└── ExternalLabService.cs
```

---

## 2. مواصفات وتواقيع الخدمات البرمجية (Service Methods)

### 2.1 خدمات المصادقة والمستخدمين — `IAuthService`
تتعامل مع جدول `Staff` و `Permission` و `StaffPermission` لإدارة الجلسات والصلاحيات في محطات الشبكة المحلية:
* `Task<Staff?> LoginAsync(string username, string password);` -> التحقق من الهوية وتحديث حقل `last_login_at`.
* `Task<bool> HasPermissionAsync(int staffId, string permissionCode);` -> فحص الصلاحية بشكل لحظي.
* `Task<bool> ChangePasswordAsync(int staffId, string oldPasswordHash, string newPasswordHash);`

### 2.2 إعدادات النظام والمختبر — `ISettingsService`
تتعامل مع جدول `LabSettings` وتدعم المحطات المتعددة عبر الشبكة المحلية (LAN):
* `Task<string> GetSettingValueAsync(string key);` -> جلب إعداد معين (اسم المعمل، الشعار، الطابعة الافتراضية).
* `Task SetSettingValueAsync(string key, string value);` -> حفظ الإعدادات المركزية.

### 2.3 إدارة كتالوج التحاليل والباقات — `ITestCatalogService` [تحديث V4]
تتعامل مع جداول التصنيفات وباقات التحاليل وكتالوج المضادات الحيوية:
* `Task<List<TestType>> SearchTestTypesAsync(string query);` -> البحث عن تحليل في الكتالوج.
* `Task<List<TestProfile>> GetAllProfilesAsync();` -> جلب كل الباقات الجاهزة المتاحة (ق1).
* `Task<List<TestType>> GetProfileItemsAsync(int profileId);` -> جلب التحاليل التابعة لباقة معينة (ق1).
* `Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);` -> فلترة وتصفية المضادات الحيوية الآمنة طبياً حسب حالة المريض (ق4).

### 2.4 إدارة ملفات وتاريخ المرضى — `IPatientService` [تحديث V4]
تتعامل مع سجلات المرضى وجدول التاريخ الطبي المنظم:
* `Task<Patient?> GetPatientByCodeAsync(string patientCode);` -> جلب مريض بكود المختبر الفريد.
* `Task<Patient> RegisterPatientAsync(Patient patient);` -> تسجيل مريض جديد والتحقق من عدم تكرار الرقم الوطني إن وجد.
* `Task<List<PatientMedicalHistory>> GetActiveMedicalHistoryAsync(int patientId);` -> جلب الأمراض المزمنة والحساسيات النشطة للمريض لعرضها فوراً في شاشة إدخال النتائج (ق7).
* `Task AddMedicalHistoryRecordAsync(PatientMedicalHistory history);` -> إضافة مرض أو حساسية جديدة لملف المريض الطبي المنظم (ق7).

### 2.5 إدارة الزيارات والعمليات — `IVisitService` [تحديث V4]
تتعامل مع العمليات التشغيلية (الزيارات، حزم الفحص، مسار معالجة العينات، والرسوم الإضافية):
* `Task<Visit> CreateVisitAsync(Visit visit, List<int> testTypeIds, List<int> profileIds, List<VisitCharge> additionalCharges);` -> إنشاء زيارة كاملة، حقن التحاليل الفردية، تفكيك باقات التحاليل إلى تحاليل فردية وحقنها، وإضافة الرسوم الإضافية (مثل السحب المنزلي) (ق1، ق8).
* `Task UpdateWorkflowStageAsync(int visitTestId, string stage, int performedByStaffId);` -> تحديث مرحلة التحليل ودعم مرحلة `PROCESSING` الجديدة لتتبع فصل العينات في أجهزة الطرد المركزي (ق9).
* `Task<List<VisitTest>> GetPendingProcessingSamplesAsync();` -> استعلام عن العينات الجاهزة للفصل بالطرد المركزي (ق9).

### 2.6 إدخال النتائج والتدقيق الطارئ — `IResultService`
تتعامل مع جداول النتائج وتتبع التعديلات (تعتمد على جداول النتائج وسجلات التدقيق الموحدة):
* `Task SaveTestResultsAsync(List<TestResult> results, int staffId);` -> حفظ مجموعة نتائج.
* `Task<List<TestResult>> GetResultsByVisitIdAsync(int visitId);` -> جلب نتائج زيارة معينة لعرضها أو طباعتها.
* `Task<List<AuditLog>> GetResultAuditTrailAsync(int visitTestId);` -> جلب تاريخ تعديلات النتيجة لمعرفة من غير القيم ومتى للتدقيق والأمان.

### 2.7 إدارة الحسابات والفواتير المجمعة — `IFinancialService` [تحديث V4]
إدارة المدفوعات الفورية والتسويات الشهرية لشركات التأمين:
* `Task ReceivePaymentAsync(Payment payment);` -> تسجيل دفعة نقدية لزيارة مريض، مما يشغل الـ Triggers تلقائياً لتحديث الأرصدة.
* `Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime startDate, DateTime endDate);` -> تجميع كافة زيارات موظفي شركة تأمين معينة خلال الشهر وإصدار فاتورة مجمعة رسمية لها (ق6).
* `Task RecordContractPaymentAsync(ContractPayment payment);` -> تسوية ودفع جزء أو كامل قيمة الفاتورة الشهرية للشركة المتعاقدة (ق6).

### 2.8 إدارة الحضور والانصراف للورديات — `IAttendanceService` [جديد V4]
تتعامل مع تنظيم شؤون موظفي المعمل ومراقبة المحطات:
* `Task<List<WorkShift>> GetAvailableShiftsAsync();` -> جلب ورديات العمل المتاحة بالمعمل.
* `Task RecordAttendanceAsync(int staffId, string clockType);` -> تسجيل بصمة حضور (`CLOCK_IN`) أو انصراف (`CLOCK_OUT`) للموظف وربطها بورديته آلياً (ق2).
* `Task<List<Attendance>> GetStaffAttendanceReportAsync(int staffId, DateTime start, DateTime end);`

### 2.9 شحن ولوجستيات العينات الخارجية — `IExternalLabService` [جديد V4]
تتعامل مع إدارة تتبع الفحوصات المرسلة لمعامل مرجعية أخرى:
* `Task<ExternalShipment> CreateShipmentManifestAsync(int externalLabId, List<int> visitTestIds, int createdBy);` -> إنشاء وثيقة شحن رسمية مجمعة للعينات وتغيير حالتها لـ OUTSOURCED (ق5).
* `Task UpdateShipmentStatusAsync(int shipmentId, string status);` -> تتبع الشحنة حتى تسليمها للمعمل الخارجي واستلام النتائج.

---

## 3. قواعد برمجية صارمة للتنفيذ (Core Rules)

1. **حقن سياق البيانات (DbContext Injection):** لا يجوز لأي خدمة إنشاء نسخة من `FinalLabDbContext` يدوياً عبر الكلمة المفتاحية `new`. يجب استقبال السياق عبر الـ Constructor بالاعتماد على ميزة حقن التبعيات (Dependency Injection) المبنية في دوت نت.
2. **العمليات غير المتزامنة (Asynchronous Coding):** يجب استخدام الـ Keywords تبعا لـ `async` و `await` واستخدام دوال EF Core غير المتزامنة مثل `ToListAsync()` و `FirstOrDefaultAsync()` و `SaveChangesAsync()` للحفاظ على استجابة واجهة WPF وتوفير أداء متزامن لحظي على الشبكة المحلية (LAN).
3. **عزل العمليات (Decoupling):** لا تتواصل الـ ViewModels مع قاعدة البيانات مطلقاً؛ بل تستدعي الـ Interfaces الخاصة بهذه الخدمات فقط، مما يسهل استبدال كود الخدمات أو عمل Unit Testing لاحقاً.
