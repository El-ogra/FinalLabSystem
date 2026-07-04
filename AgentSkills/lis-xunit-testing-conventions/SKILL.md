---
name: lis-xunit-testing-conventions
description: معايير كتابة الاختبارات في FinalLabSystem تحديدًا — xUnit + Moq + EF Core InMemory فقط. قوالب جاهزة لـ InMemoryDbContextFactory، أنماط Moq المعتمدة، Seed Data القياسي، هيكل الاختبارات الثلاثة الإلزامية لكل Service، والأنماط الممنوعة. يستدعى عند قول المستخدم: «اكتب اختبارات لـ...»، «أضف tests للـ service»، «كيف أكتب اختبارًا في هذا المشروع؟».
disable-model-invocation: true
argument-hint: "اسم الـ Service أو الـ Component المستهدف + (اختياري) نوع الاختبار المطلوب"
---

# LIS xUnit Testing Conventions — FinalLabSystem

> **هدف المهارة الوحيد:** توحيد كتابة الاختبارات في FinalLabSystem بحيث يكون كل اختبار متوافقًا معايير المشروع، يبني بدون warnings، ويُلتقط به مشاكل حقيقية.

---

## مبادئ التأسيس (اقرأ أول مرة فقط)

1. **الحزمة المعتمدة حصرًا:** `xUnit 2.5.3` + `Moq 4.20.70` + `Microsoft.EntityFrameworkCore.InMemory 8.0.0`.
2. **ممنوع نهائيًا:**
   - ❌ `NSubstitute` — غير مثبّت
   - ❌ `FluentAssertions` — استخدم `Assert.*` العادية من xUnit
   - ❌ `Testcontainers` — مشروع WPF لا يحتاجه
   - ❌ `WebApplicationFactory` — لا يوجد ASP.NET Core
   - ❌ `Bogus` / `AutoFixture` — اكتب seed يدويًا (أبسط وأسرع)
   - ❌ `DateTime.Now` / `DateTime.Today` — استخدم `DateTime.UtcNow` فقط
   - ❌ `Mock<FinalLabDbContext>` — استخدم `UseInMemoryDatabase(...)` بدلًا من ذلك
3. **كل اختبارات الـ Service يجب أن تكون في `FinalLabSystem.Tests/Services/`** — دائمًا.
4. **اختبارات التكامل (E2E) في `FinalLabSystem.Tests/Integration/`** — دائمًا.

---

## 1. InMemoryDbContextFactory — القالب الكامل (احفظه وانسخه)

**هذا القالب موجود حرفيًا في `FinalLabSystem.Tests/Services/BackupServiceTests.cs` و`AttendanceServiceTests.cs` وغيرهما. لا تخترع قالبًا آخر.**

```csharp
using FinalLabSystem.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace FinalLabSystem.Tests.Services;

internal static class InMemoryDbContextFactory
{
    public static DbContextOptions<FinalLabDbContext> Create(string dbName)
    {
        return new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w
                .Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }

    public static FinalLabDbContext CreateContext(string dbName)
        => new(Create(dbName));
}
```

**⚠️ أخطاء شائعة:**

| ❌ الخطأ | ✅ الصواب |
|---------|----------|
| بدون `.ConfigureWarnings(...)` | دائمًا أضفها — وإلا تظهر warnings في `dotnet build` |
| استخدام `Mock<FinalLabDbContext>` | استخدم `UseInMemoryDatabase` |
| نسيان `using` لـ `Interventions` | أضف `using Microsoft.EntityFrameworkCore.Diagnostics;` |

**استخدام القالب داخل الاختبار:**

```csharp
[Fact]
public async Task SomeMethod_DoesExpected_WhenCondition()
{
    // Arrange
    using var ctx = new FinalLabDbContext(InMemoryDbContextFactory.Create(nameof(SomeMethod_DoesExpected_WhenCondition)));
    // ... باقي الاختبار
}
```

---

## 2. Seed Data — القوالب القياسية لكل كيان

### 2.1 Patient — إلزامي `Sex = "M"`

```csharp
private static Patient SeedPatient(int id = 1, string code = "P001") => new()
{
    PatientId   = id,
    PatientCode = code,
    FullNameAr  = "أحمد محمد",        // ⚠️ اسم الحقل FullNameAr وليس FullName
    FullNameEn  = "Ahmad M.",
    Sex         = "M",                // ⚠️ إلزامي — العمود NOT NULL و HasMaxLength(1)
    DateOfBirth = new DateOnly(1990, 1, 1),
    CreatedAt   = DateTime.UtcNow,    // ⚠️ UtcNow دائمًا
    IsActive    = true
};
```

### 2.2 Staff — يجب أن يكون `IsActive = true`

```csharp
private static Staff SeedAdmin(int id = 1) => new()
{
    StaffId      = id,
    Username     = "admin",
    DisplayName  = "Admin User",
    FullName     = "Admin User",
    IsAdmin      = true,
    IsActive     = true,
    PasswordHash = "hash",
    Sex          = "M",
    CreatedAt    = DateTime.UtcNow
};

private static Staff SeedRegularUser(int id = 2) => new()
{
    StaffId      = id,
    Username     = "user",
    DisplayName  = "Regular User",
    IsAdmin      = false,
    IsActive     = true,
    PasswordHash = "hash",
    Sex          = "M",
    CreatedAt    = DateTime.UtcNow
};
```

### 2.3 Visit — يربط Patient بـ Staff

```csharp
private static Visit SeedVisit(int id = 1, int patientId = 1, int staffId = 1) => new()
{
    VisitId   = id,
    PatientId = patientId,
    StaffId   = staffId,
    VisitDate = DateTime.UtcNow,
    VisitCode = $"V{DateTime.UtcNow:yyyyMMdd}-{id:D4}"
};
```

### 2.4 VisitTest — ربط اختبار بزيارة

```csharp
private static VisitTest SeedVisitTest(int id = 1, int visitId = 1, int testTypeId = 1) => new()
{
    VisitTestId = id,
    VisitId     = visitId,
    TestTypeId  = testTypeId,
    Status      = "Pending"
};
```

### 2.5 TestType — تحليل في الكتالوج

```csharp
private static TestType SeedTestType(int id = 1, string code = "CBC") => new()
{
    TestTypeId  = id,
    Code        = code,
    NameAr      = "صورة دم كاملة",
    NameEn      = "Complete Blood Count",
    IsActive    = true,
    Price       = 50m,
    CreatedAt   = DateTime.UtcNow
};
```

---

## 3. Moq — الأنماط المعتمدة

### 3.1 Mocking لـ `IAuditService`

**التوقيع الفعلي:** `Task LogActionAsync(string tableName, int recordId, string action, int staffId, string? notes = null)`

```csharp
// Setup
var audit = new Mock<IAuditService>();
audit.Setup(a => a.LogActionAsync(
        It.IsAny<string>(),   // tableName
        It.IsAny<int>(),      // recordId
        It.IsAny<string>(),   // action (C/U/D/B/R)
        It.IsAny<int>(),      // staffId
        It.IsAny<string>()))  // notes
     .Returns(Task.CompletedTask);

// Verify — استُدعيت مرة واحدة
audit.Verify(a => a.LogActionAsync(
        "Patient", patient.PatientId, "C",
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Once);

// Verify — لم تُستدعَ
audit.Verify(a => a.LogActionAsync(
        It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Never);
```

### 3.2 Mocking لـ `ICurrentUserSession`

```csharp
var session = new Mock<ICurrentUserSession>();
session.Setup(s => s.CurrentUser).Returns(SeedAdmin());
session.Setup(s => s.IsAuthenticated).Returns(true);

// سيناريو مستخدم غير مسجل
var emptySession = new Mock<ICurrentUserSession>();
emptySession.Setup(s => s.CurrentUser).Returns((Staff?)null);
emptySession.Setup(s => s.IsAuthenticated).Returns(false);
```

### 3.3 Mocking لـ `ILogger<T>`

```csharp
// الطريقة 1: بدون setup (لا تفعل شيئًا)
var logger = Mock.Of<ILogger<MyService>>();

// الطريقة 2: مع التحقق من أن رسالة معينة سُجّلت
var logger = new Mock<ILogger<MyService>>();
logger.Verify(
    l => l.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.IsAny<It.IsAnyType>(),
        It.IsAny<Exception?>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
    Times.Once);
```

### 3.4 أنماط Moq الممنوعة

| ❌ ممنوع | السبب | ✅ البديل |
|---------|------|----------|
| `Substitute.For<T>()` (NSubstitute) | غير مثبّت | `new Mock<T>()` |
| `Mock<FinalLabDbContext>()` | الـ DbContext ليس واجهة | `UseInMemoryDatabase(...)` |
| `It.IsAny<int>()` لقيمة مهمة في Verify | لا تتحقق من القيمة | مرّر القيمة الفعلية |
| `Times.AtLeastOnce` بدون داعٍ | يفشل الاختبار لو لم يُستدعَ | `Times.Once` أو `Times.Never` |

---

## 4. هيكل الاختبارات الثلاثة الإلزامية لكل Service جديدة

### 4.1 الملفات الثلاثة

```
Tests/
├── Services/
│   ├── {Name}ServiceTests.cs              ← اختبارات الوحدة (unit tests)
│   └── {Name}ServiceRegistrationTests.cs  ← اختبار تسجيل DI فقط
└── Integration/
    └── {Name}{Workflow}EndToEndTests.cs   ← اختبار E2E مع InMemory DB
```

> **ملاحظة:** اسم ملف E2E يتبع النمط `{Name}{Workflow}EndToEndTests.cs` مثل `BackupServiceIntegrationTests.cs` أو `AttendanceWorkflowEndToEndTests.cs`. طابق النمط الموجود في كل حالة.

### 4.2 `{Name}ServiceTests.cs` — اختبارات الوحدة

**القالب الكامل:**

```csharp
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public sealed class MyServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => InMemoryDbContextFactory.Create(dbName);

    private static Staff SeedAdmin() => new()
    {
        StaffId = 1, Username = "admin", DisplayName = "Admin",
        IsAdmin = true, IsActive = true, PasswordHash = "hash",
        Sex = "M", CreatedAt = DateTime.UtcNow
    };

    private static (MyService service, FinalLabDbContext ctx, Mock<IAuditService> audit)
        CreateSut(string dbName, Staff? user = null)
    {
        var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(user ?? SeedAdmin());
        session.Setup(s => s.IsAuthenticated).Returns(user != null);
        var audit = new Mock<IAuditService>();
        var logger = Mock.Of<ILogger<MyService>>();
        var service = new MyService(ctx, session.Object, audit.Object, logger);
        return (service, ctx, audit);
    }

    [Fact]
    public async Task DoSomethingAsync_ReturnsExpected_WhenInputIsValid()
    {
        // Arrange
        var (sut, ctx, audit) = CreateSut(nameof(DoSomethingAsync_ReturnsExpected_WhenInputIsValid));
        var input = new SomeInput { /* ... */ };

        // Act
        var result = await sut.DoSomethingAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("expected", result.SomeProperty);
        audit.Verify(a => a.LogActionAsync(
            "SomeTable", It.IsAny<int>(), "C",
            It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DoSomethingAsync_ThrowsUnauthorized_WhenUserIsNotAdmin()
    {
        // Arrange
        var (sut, _, _) = CreateSut(nameof(DoSomethingAsync_ThrowsUnauthorized_WhenUserIsNotAdmin),
            user: SeedRegularUser());

        // Act + Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => sut.DoSomethingAsync(new SomeInput()));
    }
}
```

### 4.3 `{Name}ServiceRegistrationTests.cs` — اختبار تسجيل DI

```csharp
using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public sealed class MyServiceRegistrationTests
{
    [Fact]
    public void IMyService_IsRegisteredInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(opt =>
            opt.UseInMemoryDatabase("DI_Test_MyService"));
        services.AddLogging();
        services.AddScoped<IMyService, MyService>();

        // Act
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<IMyService>();

        // Assert
        Assert.NotNull(service);
        Assert.IsType<MyService>(service);
    }
}
```

### 4.4 `{Name}{Workflow}EndToEndTests.cs` — اختبار E2E

```csharp
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLabSystem.Tests.Integration;

public sealed class MyFeatureEndToEndTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => InMemoryDbContextFactory.Create(dbName);

    [Fact]
    public async Task EndToEnd_FullWorkflow_Succeeds()
    {
        // Arrange — UseInMemoryDatabase جديد لكل اختبار
        using var ctx = new FinalLabDbContext(CreateOptions(nameof(EndToEnd_FullWorkflow_Succeeds)));
        var patient = SeedPatient();
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        var service = new MyService(ctx, /* deps */);

        // Act
        var result = await service.FullWorkflowAsync(patient.PatientId);

        // Assert
        Assert.True(result.IsSuccess);
        var fromDb = await ctx.SomeTable.FirstOrDefaultAsync();
        Assert.NotNull(fromDb);
        Assert.Equal(patient.PatientId, fromDb.PatientId);
    }
}
```

---

## 5. تسمية الاختبارات — المعيار المعتمد

**القاعدة:** `MethodName_ExpectedResult_WhenCondition`

### ✅ أمثلة صحيحة من المشروع

```
CreateBackupAsync_ThrowsUnauthorized_WhenUserIsNotAdmin
GetPatientAsync_ReturnsNull_WhenPatientNotFound
RestoreBackupAsync_CreatesPreRestoreBackup_BeforeRestoring
RecordClockInAsync_CreatesRecordWithCorrectLateMinutes
```

### ❌ أمثلة خاطئة

```
Test1
BackupTest
ShouldCreateBackup
CreateBackup
```

**القاعدة الذهبية:** لو قرأت اسم الاختبار وحده، يجب أن تعرف:
1. ما هو الـ method المختبَر
2. ما هو السلوك المتوقع
3. تحت أي شرط

---

## 6. الأنماط الممنوعة صراحة

| المحظور | البديل الصحيح |
|---------|---------------|
| `Substitute.For<T>()` (NSubstitute) | `new Mock<T>()` |
| `DateTime.Now` / `DateTime.Today` | `DateTime.UtcNow` |
| `Mock<FinalLabDbContext>()` | `UseInMemoryDatabase(...)` |
| `WebApplicationFactory<>` | غير قابل للتطبيق (WPF) |
| `PostgreSqlContainer` / `MsSqlContainer` | `UseInMemoryDatabase(...)` |
| `await Task.Delay(...)` للانتظار | استخدم قيم ثابتة |
| `Thread.Sleep(...)` | ممنوع نهائيًا |
| اختبار XAML / code-behind | اختبر ViewModel فقط |
| `FluentAssertions` (`.Should().Be()`) | `Assert.Equal(expected, actual)` |
| `internal class` للخدمة المختبَرة | `public class` إلزامي |

---

## 7. قواعد خاصة بـ `dotnet build`

**لكي يمر `dotnet build` بدون warnings:**

1. **كل الـ `using` المستخدمة:** أزل غير المستخدمة (الـ default analyzer يحذّر).
2. **لا تكرار namespaces:** `using FinalLabSystem.Models;` + `using FinalLabSystem.Models.Enums;` بترتيب أبجدي.
3. **تسمية المتغيرات:** `camelCase` للمتغيرات المحلية، `PascalCase` للـ properties.
4. **التعليقات العربية:** مقبولة للشرح، لكن لا تُترجم رسائل الخطأ المُولَّدة.
5. **لا `async void`:** استخدم `async Task` دائمًا.

---

## 8. قائمة مراجعة سريعة قبل commit

استخدم هذه القائمة لكل ملف اختبار جديد قبل `git commit`:

- [ ] **3 ملفات موجودة؟** `{Name}ServiceTests.cs` + `{Name}ServiceRegistrationTests.cs` + `{Name}...EndToEndTests.cs`
- [ ] **التسمية صحيحة؟** `MethodName_ExpectedResult_WhenCondition`
- [ ] **InMemory factory موحّد؟** يستخدم `InMemoryDbContextFactory.Create(...)` أو نمط موحّد مشابه
- [ ] **Seed Data كامل؟** كل من `Patient` و `Staff` و `Visit` يحوي الحقول الإلزامية (`Sex = "M"`, `CreatedAt = DateTime.UtcNow`, ...)
- [ ] **`dotnet build` نظيف؟** 0 warnings, 0 errors
- [ ] **`dotnet test` يمر؟** كل الاختبارات الجديدة ✅
- [ ] **عدد الاختبارات ≥ الحد الأدنى؟** (CRUD بسيط: 5، CRUD مع منطق: 8، Workflow: 12+)
- [ ] **Moq patterns صحيحة؟** `Setup` و `Verify` بـ `It.IsAny<>` للمعاملات غير المهمة، قيم محددة للمعاملات المهمة
- [ ] **لا أنماط ممنوعة؟** لا NSubstitute، لا FluentAssertions، لا Testcontainers، لا DateTime.Now

---

## 9. المراجع

- `FinalLabSystem.Tests/Services/BackupServiceTests.cs` — نموذج ممتاز لخدمة معقدة
- `FinalLabSystem.Tests/Services/AttendanceServiceTests.cs` — نموذج لخدمة بسيطة
- `FinalLabSystem.Tests/Services/AuthServiceTests.cs` — نموذج لخدمة بـ authorization
- `FinalLabSystem.Tests/Integration/AttendanceWorkflowEndToEndTests.cs` — نموذج E2E
- `AgentSkills/csharp-testing-improved/SKILL.md` — المهارة العامة (تمّ تصحيح الإشارة من `csharp-testing` إلى `csharp-testing-improved` — المجلد الفعلي في المشروع)
- `AgentSkills/audit-trail-conventions/SKILL.md` — كيف يعمل Audit التلقائي (لفهم الـ Verify)