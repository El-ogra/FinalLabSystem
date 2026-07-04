---
name: medical-lab-domain-glossary
description: "مهارة متخصصة في medical-lab-domain-glossary لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Medical Lab Domain Glossary — FinalLabSystem

## Core Domain Entities (من `Models/`)

### Patient (المريض)

| الحقل | المعنى | قاعدة العمل |
|---|---|---|
| `PatientId` | مفتاح أساسي | identity |
| `PatientCode` | كود المريض الفريد | UNIQUE، الشكل: `P{YYYY}{6digits}` |
| `FullNameAr` / `FullNameEn` | الاسم ثنائي اللغة | عربي إلزامي، إنجليزي اختياري |
| `NationalId` | الهوية الوطنية | UNIQUE، يُستخدم للبحث عن مريض موجود |
| `Phone` / `Phone2` | هاتف | أساسي + ثانوي |
| `Sex` | الجنس | `M` / `F` (حرف واحد، fixed-length) |
| `DateOfBirth` | تاريخ الميلاد | إن لم يُعرف، استخدم `ApproxAge + ApproxAgeUnit` |
| `BloodType` | فصيلة الدم | `A+`, `B-`, `O+`, ..., أو NULL |
| `PatientType` | نوع المريض | `Individual` / `Company` / `Insurance` / `VIP` |
| `IsVip` | VIP flag | true → أولوية في المعالجة، banner ذهبي في الواجهة |

**قاعدة:** المريض في FinalLabSystem هو **مستوى هوية فقط**. لا توجد زيارات هنا — الزيارات في `Visit`.

### Visit (الزيارة)

| الحقل | المعنى | قاعدة العمل |
|---|---|---|
| `VisitId` | مفتاح أساسي | identity |
| `PatientId` | FK لمريض | many visits : one patient |
| `VisitCode` | كود الزيارة | الشكل: `V{YYYYMMDD}-{seq}`، يستخدم في barcode |
| `VisitDate` | تاريخ الزيارة | UTC |
| `Status` (VisitStatus enum) | حالة الزيارة | `Open` / `InProgress` / `Completed` / `Cancelled` |
| `ReferralId` | FK لمصدر الإحالة (طبيب، مستشفى) | NULL يعني زيارة مباشرة |
| `CompanyId` | FK لشركة (عقد) | للزيارات المؤسسية فقط |
| `DiscountPercent` / `DiscountAmount` | خصم | من صلاحيات المحاسبة |
| `Notes` / `ClinicalNotes` | ملاحظات | تنبيهات للمختبر |

**قاعدة:** الزيارة **هي وحدة العمل المركزية**. كل شيء (tests، samples، payments، charges) يتعلق بـ Visit معيّن.

### VisitTest (اختبار في الزيارة)

| الحقل | المعنى | قاعدة العمل |
|---|---|---|
| `VisitTestId` | مفتاح أساسي | identity |
| `VisitId` | FK للزيارة | many : one visit |
| `TestTypeId` | FK لنوع الاختبار | مرجع إلى `TestType` |
| `Stage` (TestStage enum) | مرحلة سير العمل | `Pending` / `InProgress` / `ResultEntered` / `Validated` / `Released` / `SentOut` / `Cancelled` |
| `ResultEnteredBy` / `ResultEnteredAt` | من أدخل النتيجة | NULL حتى تُكتب |
| `IsPrinted` / `PrintedAt` / `PrintedBy` | هل طُبعت النتيجة | للتقرير |
| `Price` | السعر النهائي | محسوب من PricingEngine حسب الـ PriceScheme |

**قاعدة:** `VisitTest` ليس نتيجة — هو **طلب اختبار** بحالة. النتيجة الفعلية في `TestResult`.

### TestResult (نتيجة الاختبار)

| الحقل | المعنى | قاعدة العمل |
|---|---|---|
| `ResultId` | مفتاح أساسي | identity |
| `VisitTestId` | FK للاختبار | one : one |
| `ComponentId` | FK لمكوّن التحليل | material (e.g. glucose, hemoglobin) |
| `ResultValueNumeric` | قيمة عددية | NULL إذا كان النص فقط |
| `ResultValueText` | قيمة نصية | للـ microbiology, semen, cross-match |
| `ValidationStatus` (ResultValidationStatus enum) | حالة التحقق | `Entered` (0) / `Reviewed` (1) / `Validated` (2) / `Released` (3) |
| `EnteredBy` / `EnteredAt` | من أدخل القيمة | |
| `ReviewedBy` / `ReviewedAt` | من راجع | senior فقط |
| `ValidatedBy` / `ValidatedAt` | من اعتمد | pathologist فقط |
| `ReleasedBy` / `ReleasedAt` | من أصدر | للطباعة |

**قاعدة:** الـ ValidationStatus تصاعدي. لا يمكن أن تتخطى مرحلة. الإصدار (`Released`) يحتاج `Validated` أولاً.

### TestType (نوع الاختبار)

| الحقل | المعنى | قاعدة العمل |
|---|---|---|
| `TestTypeId` | مفتاح أساسي | identity |
| `TestCode` | كود الاختبار | UNIQUE، مثل `CBC`, `HbA1c`, `LIPID` |
| `NameAr` / `NameEn` | اسم ثنائي اللغة | |
| `CategoryId` | FK للفئة | مثل Chemistry, Hematology, Microbiology |
| `GroupId` | FK للمجموعة | للـ profiles |
| `IsProfile` | هل هو profile (يجمع عدة tests) | |
| `Unit` | الوحدة الافتراضية | مثل mg/dL، g/dL |
| `IsActive` | نشط | إلغاء ناعم |

### TestComponent (مكوّن التحليل / Analyte)

| الحقل | المعنى |
|---|---|
| `ComponentId` | مفتاح |
| `ComponentCode` | كود المكوّن | مثل `GLU`, `HGB`, `WBC` |
| `NameAr` / `NameEn` | اسم |
| `Unit` | الوحدة | mg/dL، 10^3/µL |
| `SortOrder` | ترتيب العرض | في التقرير والـ DataGrid |
| `DecimalPlaces` | عدد الكسور | 1 أو 2 افتراضي |

**قاعدة:** TestComponent = المادة (glucose)؛ TestType = اللوحة (Lipid Panel تحوي GLU, CHO, TRI, HDL, LDL).

### NormalRange (النطاق المرجعي)

| الحقل | المعنى |
|---|---|
| `RangeId` | مفتاح |
| `ComponentId` | FK للمكوّن |
| `LowNormal` / `HighNormal` | الحدود الطبيعية |
| `LowCritical` / `HighCritical` | الحدود الحرجة (panic value) |
| `Unit` | الوحدة (تطابق TestComponent.Unit) |
| `Sex` | `M` / `F` / `B` (both) |
| `AgeUnit` / `AgeFromValue` / `AgeToValue` / `AgeFromDays` / `AgeToDays` | النطاق العمري |
| `FastingState` | `R` / `F` / `A` (random / fasting / any) |
| `ForPregnantOnly` | فقط للحوامل |
| `LowFlag` / `HighFlag` | الرمز على التقرير (L/H) |
| `LowComment` / `HighComment` / `CriticalComment` | تعليقات تظهر على التقرير |
| `Version` | الإصدار (immutable versioning) |
| `IsActive` / `SupersededById` | للتحكم الإصداري |

**قاعدة:** النطاقات لا تُحذف أبداً. القديم يصبح IsActive=false ويُشار إلى الجديد بـ SupersededById. كل المراجع التاريخية يجب أن تظل قابلة للقراءة.

### SampleTube (عيّنة)

| الحقل | المعنى |
|---|---|
| `TubeId` | مفتاح |
| `BarcodeValue` | الباركود | UNIQUE، الشكل `LAB-{visitId:D6}-{yyyyMMdd}` |
| `TubeType` / `TubeColor` | نوع ولون الأنبوب | Lavender, Red, Green, ... |
| `CollectedAt` / `CollectedBy` | وقت ومن جمع |
| `PrintedAt` / `PrintedBy` | متى طُبعت الملصق |
| `Notes` | ملاحظات |
| `VisitId` | FK للزيارة |

**قاعدة:** كل Tube مرتبط بـ VisitTest واحد على الأقل. TestType قد يحتاج عدة أنابيب (type → tubes mapping في `TestTypeSampleTube`).

### VisitCharge (التكلفة في الزيارة)

| الحقل | المعنى |
|---|---|
| `VisitChargeId` | مفتاح |
| `VisitId` | FK للزيارة |
| `TestTypeId` | FK للاختبار |
| `UnitPrice` / `Quantity` / `DiscountAmount` | حساب |
| `NetAmount` | الإجمالي المحسوب |

**قاعدة:** NetAmount = (UnitPrice × Quantity) - DiscountAmount. لا تحرر Net يدويًا — أعِد الحساب.

### Payment (الدفعة)

| الحقل | المعنى |
|---|---|
| `PaymentId` | مفتاح |
| `VisitId` | FK |
| `Amount` | المبلغ |
| `PaymentMethod` (enum) | `Cash` / `Card` / `Transfer` / `Check` / `Insurance` |
| `PaymentType` | `PAYMENT` / `REFUND` |
| `ReceivedBy` / `ReceivedAt` / `ReferenceNumber` | |

**قاعدة:** الدفعات جزئية (يمكن دفع 50 من 100). الباقي (outstanding) محسوب عبر trigger `TR_Payment_SyncBalance`.

### PaymentMethod enum (الأرقام في الجدول)

| Value | Code عربي | Code إنجليزي |
|---|---|---|
| `Cash` | نقدي | Cash |
| `Card` | بطاقة | Card |
| `Transfer` | تحويل بنكي | Transfer |
| `Check` | شيك | Check |
| `Insurance` | تأمين | Insurance |

## Stage-Gating Entities

### TestStage enum

```csharp
public enum TestStage { Pending, InProgress, ResultEntered, Validated, Released, SentOut, Cancelled }
```

### ResultValidationStatus enum

```csharp
public enum ResultValidationStatus { Entered = 0, Reviewed = 1, Validated = 2, Released = 3 }
```

### ResultClinicalStatus enum (للتقارير فقط، ليس للتدقيق)

```csharp
public enum ResultClinicalStatus { Normal, Low, High, CriticalLow, CriticalHigh, Abnormal }
```

## DTOs (نماذج للـ UI)

| DTO | يحوي | يُستخدم في |
|---|---|---|
| `VisitFullDto` | كل بيانات الزيارة + Patient + Tests + Payments | نافذة الزيارة الكاملة |
| `TestComponentResultDto` | مكوّن + قيمة + نطاق + flag + unit | تقرير FlowDocument |
| `TodayPatientDto` | قائمة المرضى اليوم | شاشة الاستقبال |
| `PrintQueueItemDto` | queued print job | لوحة الطباعة المؤجلة |
| `BackupMetadataDto` | backup metadata | ملف JSON sidecar |
| `ReportLayoutDto` | إعدادات التقرير من LabSetting | تمرير إلى FlowDocument builder |
| `CashDrawerSummaryDto` / `CashDrawerFilterDto` | تقرير الصندوق | |
| `OutstandingBalanceReportRow` | تقرير المستحقات | |
| `CommissionReportRow` | تقرير العمولات | |
| `SelectedTestDto` | نتائج اختبار مختار في قائمة | |
| `VisitTestItemDto` | عنصر في قائمة زيارات | |

## Business Rules (المُلزمة)

### BR-001: النطاق المرجعي إلزامي

كل TestResult يجب أن يقترن بـ NormalRange صالحة في وقت الإدخال. إذا لم توجد:
- النظام يستخدم default بناءً على ComponentId فقط (إن وُجدت).
- إذا لا، يُسجَّل كـ `NormalRangeText = "—"` على التقرير.

### BR-002: الاختبار لا يُحرر قبل التحقق

`TestResult.ResultValueNumeric` يدخله فني (`ValidationStatus = Entered`)، ولا يُحرر إلا من راجع (`Reviewed`) أو اعتمد (`Validated`).

### BR-003: الزيارة لا تُلغى إذا طُبعت النتائج

إذا `VisitTests.Any(IsPrinted == true)` و `Visit.Status = Cancelled`، اطلب تأكيدًا صريحًا. لأنه قد يكون التقرير نُسخ للمريض.

### BR-004: الدفعات الجزئية لا تكمل الزيارة

`Visit.Status = Completed` لا يتحقق إلا إذا `TotalPayments >= VisitCharges`. إن لم يكن، تبقى `InProgress`.

### BR-005: لا يُحذف Patient أبدًا

Patient يُعطَّل عبر `IsActive = false` على مستوى Patient أو عبر إضافة "سبب مغلق" — لا `DELETE FROM Patient`. لأن زيارات تاريخية (audit، تقارير مطبوعة) مرتبطة به.

### BR-006: TestType مع IsProfile = true يحوي VisitTests متعددة

إذا Profile يحوي 5 components، فإن `VisitTest` واحد يتولد لكل component. أو VisitTest واحد مع VisitTestComponents مرتبطة — يعتمد على التنفيذ. تحقق من `VisitTest` في الكود.

### BR-007: الإصدار (Release) يتطلب Validation أولاً

`TestResult.ReleasedAt` لا يُملأ إلا إذا `ValidationStatus >= Validated`. راجع `Infrastructure/ResultStageRules.CanPrint`.

### BR-008: BarcodeValue ثابت ولا يُعاد توليده

إذا طُبع ملصق ثم تطلب إعادة طباعة، يُعاد استخدام نفس الـ BarcodeValue. النظام `IsPrinted` يُحدَّث، الـ BarcodeValue لا يتغير. هذا للربط بين الملصق والنتيجة في المختبر.

### BR-009: AuditLog لا يُعدَّل يدويًا

لا Service عنده صلاحية UPDATE على AuditLog. إذا لزم تصحيح، أضف audit row جديد بـ Notes = "Correction: ...".

### BR-010: Inbound vs Outbound

الـ tests نوعان:
- **Routine**: تُنفذ في نفس المختبر (chemistry, hematology).
- **SentOut (External)**: تُرسل لمختبر خارجي (مثل PCR متخصص). تُسجَّل في `ExternalShipment` + `ExternalShipmentItem`، لها tracking خاص.

## المصطلحات التجارية

| المصطلح | المعنى في النظام | مرادفات |
|---|---|---|
| **عميل** | Patient (شخص طبيعي) أو Company (مؤسسة) | الزبون |
| **زيارة** | Visit — كل ما يحدث في يوم واحد | Episode, Encounter |
| **طلب فحص** | VisitTest | Order, Requisition |
| **تحليل** | TestComponent (المادة) + TestType (اللوحة) | Analyte, Assay |
| **عيّنة** | SampleTube | Specimen |
| **نتيجة** | TestResult | Finding |
| **مدى مرجعي** | NormalRange | Reference Range, RI |
| **قيمة حرجة (Panic)** | NormalRange.LowCritical / HighCritical | Critical Value |
| **تحصيل** | Payment | Receipt |
| **عقد** | Company عقد تحصيل شهري | Contract |
| **طباعة** | `WpfLabelPrintService` (ملصق) أو `WpfFlowDocumentPrintService` (تقرير) | — |
| **تدقيق** | AuditLog | Audit trail |
| **دور** | (لا يوجد كيان صريح) — مجموعة PermissionCodes على Staff | Role |
| **مرحلة** | TestStage | Workflow state |

## المصطلحات الإنجليزية المرادفة

| Ar | En (DB column) | ملاحظة |
|---|---|---|
| `patient_code` | PatientCode | |
| `national_id` | NationalId | |
| `visit_id` | VisitId | |
| `result_value_numeric` | ResultValueNumeric | nullable |
| `validation_status` | ValidationStatus | enum |
| `barcode_value` | BarcodeValue | |
| `staff_id` | StaffId | مفتاح الموظف |
| `changed_by` | ChangedBy | من سجّل التغيير في AuditLog |
| `field_name` | FieldName | اسم العمود المُعدَّل |

## فهرسة سريعة لأي agent

عندما ترى مصطلح في طلب المستخدم ولا تعرف ما يعنيه في الـ DB، ابحث هنا أولاً. ثم افتح الكيان نفسه في `Models/<Entity>.cs` للتفاصيل.

```
Patient → Models/Patient.cs
Visit → Models/Visit.cs
VisitTest → Models/VisitTest.cs
TestResult → Models/TestResult.cs
TestType → Models/TestType.cs
TestComponent → Models/TestComponent.cs
NormalRange → Models/NormalRange.cs
SampleTube → Models/SampleTube.cs
VisitCharge → Models/VisitCharge.cs
Payment → Models/Payment.cs
Staff → Models/Staff.cs
Permission → Models/Permission.cs
StaffPermission → Models/StaffPermission.cs
LabSetting → Models/LabSetting.cs (settings runtime — يشمل backup_schedule_hour)
Company → Models/Company.cs (عقود الشركات)
ReferralSource → Models/ReferralSource.cs (مصادر الإحالة)
ExternalLab → Models/ExternalLab.cs
ExternalShipment → Models/ExternalShipment.cs
TestWorkflow → Models/TestWorkflow.cs
AuditLog → Models/AuditLog.cs
ReportCommentTemplate → Models/ReportCommentTemplate.cs
MicrobiologyCulture → Models/MicrobiologyCulture.cs
SemenAnalysis → Models/SemenAnalysis.cs
CrossMatchTest → Models/CrossMatchTest.cs
```

## ملاحظة للوكلاء

هذا القاموس **مرجع للقراءة فقط**. لا تستبدل قواعد العمل هنا بأفكارك الخاصة. إذا اكتشفت قاعدة عمل لا تظهر في `Models/`، أضفها هنا بصراحة مع ذكر الـ source (ملف + أرقام سطور).
