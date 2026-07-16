# تقرير التحقق التشخيصي — أعطال قاتلة مزعومة في FinalLabSystem

| البند | القيمة |
|---|---|
| **الفرع المُحلَّل** | `before-prd` |
| **ال-commit** | `a0f6a1f23e5564cd91b89be80a3f26d6c46287c6` |
| **طبيعة المهمة** | تحقق تشخيصي فقط — بدون مقارنة مع نظام مرجعي، وبدون أي تعديل كود |
| **النتيجة النهائية** | **الأعطال الخمسة كلها مؤكَّدة بالكود الفعلي** ✅ |

---

## العطل رقم 1: تعارض قيد الخصم يُفشل أي حفظ فيه خصم

- **الحالة:** مؤكَّد ✅

- **الدليل:**

  **تعريف القيد في DbContext:**
  `FinalLabSystem/Data/FinalLabDbContext.cs:1871`
  ```csharp
  tb.HasCheckConstraint("CK_Visit_DiscountExclusivity",
      "NOT (discount_amount > 0 AND discount_percent > 0)");
  ```

  **كيف تُبنى القيمتان في VisitService:**
  `FinalLabSystem/Services/Implementations/VisitService.cs:157-158`
  ```csharp
  visit.DiscountAmount = Math.Clamp(visit.DiscountAmount, 0, subtotal);
  visit.DiscountPercent = subtotal <= 0 ? 0
      : Math.Round(visit.DiscountAmount / subtotal * 100, 2);
  ```

  **كيف تُبنى القيمتان في FinancialViewModel:**
  `FinalLabSystem/ViewModels/Patients/FinancialViewModel.cs:73-76`
  ```csharp
  if (SetProperty(ref _discountAmount, Math.Clamp(value, 0, Subtotal)) && !_isUpdating)
  {
      _isUpdating = true;
      DiscountPercent = Subtotal <= 0 ? 0
          : Math.Round(DiscountAmount / Subtotal * 100, 2);
  ```

- **الشرح:**
  القيد `CK_Visit_DiscountExclusivity` يمنع أن يكون `discount_amount > 0` **و** `discount_percent > 0` في نفس الوقت.
  لكن `VisitService.SavePatientVisitAsync` و `FinancialViewModel` يُحوِّلان `DiscountAmount` إلى `DiscountPercent` تلقائياً via القسمة على `subtotal`.
  عند إدخال `DiscountAmount > 0` على زيارة بـ `subtotal > 0`، ينتج `DiscountPercent` موجب أيضاً.
  النتيجة: كلا الحقلين موجب في آن واحد → انتهاك القيد → `SaveChangesAsync` ترمي استثناء `SqlException`.

- **الأثر الفعلي المُقدَّر:**
  يحدث في **كل مرة** يُدخَل فيها خصم على زيارة. لا يمكن لأي مستخدم تطبيق خصم على زيارة ما دام `subtotal > 0`. الخصمة وظيفياً **معطّلة بالكامل**.

---

## العطل رقم 2: تصادم الترتيب اليومي للباركود (Daily Ordinal Collision)

- **الحالة:** مؤكَّد ✅

- **الدليل:**

  **منطق GetDailyOrdinalAsync:**
  `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs:101-111`
  ```csharp
  private async Task<int> GetDailyOrdinalAsync(int patientId, DateTime date)
  {
      var dayStart = date.Date;
      var dayEnd = dayStart.AddDays(1);
      return await _context.PatientBarcodes
          .Where(pb => pb.PatientId == patientId        // ترتيب لكل مريض على حدة
                    && pb.IssueDate >= dayStart
                    && pb.IssueDate < dayEnd
                    && pb.CodeType == BarcodeCodeType.Case)
          .CountAsync() + 1;
  }
  ```

  **هيكل الباركود (لا يحتوي PatientId):**
  `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs:91-99`
  ```csharp
  internal string BuildBarcodeValue(byte typeDigit, DateTime date, int ordinal)
  {
      var datePart = date.ToString("yyMMdd");
      var weekday = (int)date.DayOfWeek + 1;
      var ordinalPart = ordinal.ToString("D3");
      var partial = $"{weekday}{datePart}{ordinalPart}{typeDigit}";
      var checkDigit = CalculateLuhnCheckDigit(partial);
      return $"{partial}{checkDigit}";
  }
  ```

  **القيد الفريد على BarcodeValue:**
  `FinalLabSystem/Data/FinalLabDbContext.cs:2484-2485`
  ```csharp
  entity.HasIndex(e => e.BarcodeValue).IsUnique();
  ```

- **الشرح:**
  `GetDailyOrdinalAsync` يعدّ الترتيب لكل مريض على حدة (باستثناء `PatientId` من الاستعلام).
  لكن `BuildBarcodeValue` لا يُدرج PatientId في نص الباركود — فقط weekday + date + ordinal + type + luhn.
  مريضان مختلفان في نفس اليوم يحصلان على ordinal=1، فينتجان **نفس الباركود بالضبط** (مثلاً `22607160011X`).
  عند محاولة حفظ `PatientBarcode` الثاني، يصطدم Unique Index بـ `BarcodeValue` المكرر → استثناء فوري.

- **الأثر الفعلي المُقدَّر:**
  يحدث في **كل مرة** يُسجَّل فيها مريضان أو أكثر في نفس اليوم.
  أول مريض ينجح، ثاني مريض يفشل. **توليد الباركود معطّل من ثاني مريض يومياً.**

---

## العطل رقم 3: إعادة إدراج Lab ID تُفشل نافذة الباركود للمريض العائد

- **الحالة:** مؤكَّد ✅

- **الدليل:**

  **GetOrCreateLabIdAsync يعيد القيمة القديمة:**
  `FinalLabSystem/Services/Implementations/BarcodeGenerator.cs:31-52`
  ```csharp
  public async Task<string> GetOrCreateLabIdAsync(int patientId)
  {
      var patient = await _context.Patients.FindAsync(patientId);
      if (patient == null)
          throw new ArgumentException($"Patient {patientId} not found");

      if (!string.IsNullOrEmpty(patient.LabId))
          return patient.LabId;   // يُعيد القيمة القديمة دون إنشاء جديد

      var visit = await _context.Visits
          .Where(v => v.PatientId == patientId)
          .OrderBy(v => v.VisitDate)
          .FirstOrDefaultAsync();

      var visitDate = visit?.VisitDate ?? DateTime.Today;
      var labId = await GenerateLabIdAsync(patientId, visitDate);

      patient.LabId = labId;
      await _context.SaveChangesAsync();
      return labId;
  }
  ```

  **GenerateBarcodesForVisitAsync يُنشئ صف جديد بنفس القيمة:**
  `FinalLabSystem/Services/Implementations/SampleTrackingService.cs:51,79-88`
  ```csharp
  var labId = await _barcodeGenerator.GetOrCreateLabIdAsync(patientId);
  // ...
  _context.PatientBarcodes.AddRange(
      // ...
      new PatientBarcode
      {
          PatientId = patientId,
          VisitId = null,
          CodeType = BarcodeCodeType.Lab,
          BarcodeValue = labId,       // نفس القيمة القديمة
          IssueDate = DateTime.Now,
          SortOrdinal = 0,
          CreatedBy = staffId
      }
  );
  ```

- **الشرح:**
  للمريض العائد، `GetOrCreateLabIdAsync` يقرأ `patient.LabId` المحفوظ مسبقاً ويعيده كما هو.
  ثم `GenerateBarcodesForVisitAsync` يُنشئ صف `PatientBarcode` جديد بالقيمة نفسها.
  قيد `BarcodeValue` الفريد في `PatientBarcode` يمنع تكرار القيمة → `SaveChangesAsync` ترمي استثناء.
  النافذة تفشل عند مريض له زيارة ثانية.

- **الأثر الفعلي المُقدَّر:**
  يحدث في **كل مرة** يُفتح فيها نافذة الباركود لمريض له زيارة سابقة.
  المريض الأولي ينجح، المريض العائد يفشل دائماً.

---

## العطل رقم 4: خطأ في حساب LOrH يُعلِّم القيم الطبيعية كمنخفضة

- **الحالة:** مؤكَّد ✅

- **الدليل:**

  `FinalLabSystem/Models/DTOs/TestComponentResultDto.cs:80-95`
  ```csharp
  public string LOrH
  {
      get
      {
          if (ResultNumeric == null || SnapLowNormal == null || SnapHighNormal == null)
              return string.Empty;
          var val = (double)ResultNumeric.Value;
          var low = SnapLowNormal.Value;
          var high = SnapHighNormal.Value;
          if (val >= high * 2) return "HH";    // المفروض: val >= high_critical
          if (val <= low * 2) return "LL";     // <-- الخطأ: المفروض val <= low_critical
          if (val > high) return "H";
          if (val < low) return "L";
          return string.Empty;
      }
  }
  ```

- **الشرح والاختبار اليدوي:**
  الخاصية تقارن بـ `high * 2` و `low * 2` بدلاً من الحدود مباشرة.
  **مثال:** مدى طبيعي لـ Hemoglobin: 3.8–5.2 g/dL. القيمة المُدخلة = **5.0** (ضمن النطاق الطبيعي):

  | الشرط | الحساب | النتيجة |
  |---|---|---|
  | `5.0 >= 5.2 * 2` | `5.0 >= 10.4` | false |
  | `5.0 <= 3.8 * 2` | `5.0 <= 7.6` | **true → تُرجع "LL"** |

  القيمة 5.0 طبيعية تماماً، لكن الخاصية تُرجع **"LL" (منخفض جداً)**.

  **مثال آخر:** Hematocrit مدى طبيعي 36–46%. القيمة = **42%** (طبيعية):
  - `42 <= 36 * 2` → `42 <= 72` → **true → "LL"** — خطأ同样.

- **الأثر الفعلي المُقدَّر:**
  يحدث **لكل قيمة طبيعية** تنخفض عن `low * 2` (أي أقل من ضعف الحد الأدنى).
  في الممارسة: **معظم القيم الطبيعية** تُصنَّف خطأً كـ "LL" أو "HH".
  الأعمدة المُصابة: LOrH فقط — لا تؤثر على القيم الرقمية الفعلية للتحليل، لكن تُظهر تنبيهات خاطئة في الواجهة والتقرير.

---

## العطل رقم 5: حذف فعلي (Hard Delete) للزيارات بلا فحص صلاحية ولا أثر تدقيقي

- **الحالة:** مؤكَّد ✅

- **الدليل:**

  `FinalLabSystem/Services/Implementations/VisitService.cs:373-404`
  ```csharp
  public async Task<bool> CancelVisitAsync(int visitId)
  {
      using var transaction = await _context.Database.BeginTransactionAsync();

      try
      {
          var visit = await _context.Visits
              .Include(v => v.VisitTests)
              .Include(v => v.Payments)
              .Include(v => v.SampleTubes)
              .Include(v => v.VisitCharges)
              .FirstOrDefaultAsync(v => v.VisitId == visitId);

          if (visit is null)
              return false;

          _context.VisitTests.RemoveRange(visit.VisitTests);    // حذف فعلي
          _context.Payments.RemoveRange(visit.Payments);        // حذف فعلي
          _context.SampleTubes.RemoveRange(visit.SampleTubes);  // حذف فعلي
          _context.VisitCharges.RemoveRange(visit.VisitCharges); // حذف فعلي
          _context.Visits.Remove(visit);                         // حذف فعلي

          await _context.SaveChangesAsync();
          await transaction.CommitAsync();
          return true;
      }
      catch
      {
          await transaction.RollbackAsync();
          throw;
      }
  }
  ```

- **الشرح:**
  الدالة تستخدم `Remove` و `RemoveRange` — حذف فعلي من قاعدة البيانات، **ليس** تحديث حالة إلى `Cancelled`.
  رغم وجود `VisitStatus.Cancelled` في النموذج (`Visit.cs:94`)، إلا أنه غير مستخدم كخيار إلغاء ناعم.
  **لا يوجد فحص صلاحية:** لا يوجد `ICurrentUserSession`، لا يوجد `[Authorize]`، لا يوجد أي فحص لصلاحيات المستخدم.
  **لا يوجد AuditLog:** الـ `SaveChangesAsync` المُستدعى داخل المعاملة يتجاوز آلية التدقيق الأوتوماتيكية في DbContext (التي تُستخدم للمعاملات العادية فقط).

- **الأثر الفعلي المُقدَّر:**
  يحدث **في كل مرة** يُنفَّذ فيها إلغاء زيارة.
  أي مستخدم له صلاحية الوصول للدالة يمكنه حذف زيارة ومدفوعات وأنابيب عيّنة بالكامل من قاعدة البيانات بشكل لا رجعة فيه، وبدون أي أثر تدقيقي.

---

## ملخص حالة الأعطال

| رقم | العطل | الحالة |
|---|---|---|
| 1 | تعارض قيد الخصم | **مؤكَّد ✅** |
| 2 | تصادم الترتيب اليومي للباركود | **مؤكَّد ✅** |
| 3 | إعادة إدراج Lab ID للمريض العائد | **مؤكَّد ✅** |
| 4 | خطأ حساب LOrH | **مؤكَّد ✅** |
| 5 | Hard Delete بلا صلاحية/Audit | **مؤكَّد ✅** |

**النتيجة النهائية:** الأعطال الخمسة المزعومة كلها **موجودة فعلياً** في الكود المُحلَّل.
