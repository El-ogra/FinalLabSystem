---
name: csharp-testing
description: C# and .NET testing patterns with xUnit and Moq for WPF/MVVM desktop applications. Covers unit tests, EF Core InMemory integration tests, and DI registration tests, tailored to FinalLabSystem's conventions.
metadata:
  origin: ECC
---

# C# Testing Patterns

> ⚠️ **تحذير مهم — خاص بمشروع FinalLabSystem:**
> هذا المشروع يستخدم **xUnit + Moq + EF Core InMemory** حصراً.
> - ❌ لا تستخدم **NSubstitute** — غير مثبّت ولن يعمل
> - ❌ لا تستخدم **Testcontainers** — مشروع WPF لا يحتاجه
> - ❌ لا تستخدم **WebApplicationFactory** — لا يوجد ASP.NET Core
> - ❌ لا تستخدم **FluentAssertions** — استخدم xUnit assertions العادية
> - ✅ استخدم **Moq** للـ mocking فقط
> - ✅ استخدم **EF Core InMemory** لاختبارات قاعدة البيانات

> **مهارة أعمق:** للمرجعية الشاملة الخاصة بـ FinalLabSystem (قوالب InMemoryDbContextFactory، Seed Data لكل الكيانات، الأنماط الممنوعة صراحة)، استخدم `lis-xunit-testing-conventions`. هذه المهارة (`csharp-testing`) تبقى المرجع العام للغة C#.

---

## قواعد الاختبار الإلزامية في FinalLabSystem

| القاعدة | الصحيح | الخطأ |
|---------|--------|-------|
| التواريخ | `DateTime.UtcNow` | `DateTime.Now` أو `DateTime.Today` |
| بيانات المريض | `Sex = "M"` إلزامياً | حذف حقل Sex |
| Moq اختياري | `It.IsAny<string>()` | تمرير قيمة ثابتة لمعامل غير مهم |
| DbContext | `UseInMemoryDatabase(...)` + `ConfigureWarnings(...)` | Mock للـ DbContext |
| الخدمات | `public` إلزامياً | `internal` — يسبب فشل الاختبار |
| Seed Staff | `IsActive = true` + `PasswordHash` | نسيان الحقول الإلزامية |
| اسم Patient | `FullNameAr` (ليس `FullName`) | استخدام `FullName` غير موجود |

---

## هيكل الاختبار القياسي في المشروع

> **القالب التالي مأخوذ حرفيًا من `FinalLabSystem.Tests/Services/BackupServiceTests.cs` و`AttendanceServiceTests.cs` — لا تخترع قالبًا مختلفًا.**

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;   // ⚠️ ضروري لـ InMemoryEventId
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public sealed class PatientServiceTests
{
    // 1) Factory لـ DbContextOptions — مع ConfigureWarnings لتفادي warning في dotnet build
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    // 2) SUT Factory — يُرجع tuple فيه الـ context والـ mocks للاستخدام في Verify
    private static (PatientService sut, FinalLabDbContext ctx, Mock<IAuditService> audit)
        CreateSut(string dbName, Staff? user = null)
    {
        var ctx = new FinalLabDbContext(CreateOptions(dbName));

        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(user ?? SeedAdmin());
        session.Setup(s => s.IsAuthenticated).Returns(user != null);

        var audit = new Mock<IAuditService>();
        var logger = Mock.Of<ILogger<PatientService>>();

        var sut = new PatientService(ctx, session.Object, audit.Object, logger);
        return (sut, ctx, audit);
    }

    // 3) Seed helpers
    private static Staff SeedAdmin() => new()
    {
        StaffId = 1, Username = "admin", DisplayName = "Admin",
        IsAdmin = true, IsActive = true, PasswordHash = "hash",
        Sex = "M", CreatedAt = DateTime.UtcNow
    };

    private static Staff SeedRegularUser() => new()
    {
        StaffId = 2, Username = "user", DisplayName = "User",
        IsAdmin = false, IsActive = true, PasswordHash = "hash",
        Sex = "M", CreatedAt = DateTime.UtcNow
    };

    // 4) الاختبار — النمط: MethodName_ExpectedResult_WhenCondition
    [Fact]
    public async Task GetPatientAsync_ReturnsNull_WhenPatientNotFound()
    {
        var (sut, ctx, _) = CreateSut(nameof(GetPatientAsync_ReturnsNull_WhenPatientNotFound));

        var result = await sut.GetPatientAsync(999);

        Assert.Null(result);
    }
}
```

---

## Seed Data — القالب القياسي

### Patient — انتبه للأسماء الفعلية للحقول

```csharp
// ⚠️ اسم الحقل FullNameAr وليس FullName
// الحقول FullNameAr و FullNameEn منفصلة (عربي/إنجليزي)
var patient = new Patient
{
    PatientId   = 1,
    PatientCode = "P001",
    FullNameAr  = "أحمد محمد",     // ⚠️ FullNameAr إلزامي (Required, MaxLength 200)
    FullNameEn  = "Ahmad M.",
    Sex         = "M",             // ⚠️ إلزامي — HasMaxLength(1) IsFixedLength
    DateOfBirth = new DateOnly(1990, 1, 1),
    CreatedAt   = DateTime.UtcNow, // ⚠️ UtcNow دائماً
    IsActive    = true
};
```

### Staff — لا تنسَ `IsActive = true`

```csharp
var staff = new Staff
{
    StaffId      = 1,
    Username     = "test_user",
    DisplayName  = "موظف تجريبي",
    FullName     = "موظف تجريبي",
    IsAdmin      = true,
    IsActive     = true,           // ⚠️ إلزامي — يُستخدم في Where(IsActive)
    PasswordHash = "hash",
    Sex          = "M",
    CreatedAt    = DateTime.UtcNow
};
```

### Visit — يربط Patient بـ Staff

```csharp
var visit = new Visit
{
    VisitId   = 1,
    PatientId = 1,
    StaffId   = 1,
    VisitDate = DateTime.UtcNow,                           // ⚠️ UtcNow دائماً
    VisitCode = $"V{DateTime.UtcNow:yyyyMMdd}-0001"        // ⚠️ صيغة موصى بها
};
```

---

## Moq — الأنماط المعتمدة

### Mocking لـ `IAuditService`

**التوقيع الفعلي (من `Services/Interfaces/IAuditService.cs`):**
```csharp
Task LogActionAsync(string tableName, int recordId, string action, int staffId, string? notes = null);
```

```csharp
// Setup — كل المعاملات بـ It.IsAny ما عدا tableName (نتحقق منه في Verify)
_audit.Setup(a => a.LogActionAsync(
        It.IsAny<string>(),   // tableName
        It.IsAny<int>(),      // recordId
        It.IsAny<string>(),   // action (حرف واحد: "C","U","D","B","R")
        It.IsAny<int>(),      // staffId
        It.IsAny<string>()))  // notes (nullable)
     .Returns(Task.CompletedTask);

// Verify — تحقق أن الدالة استُدعيت مرة واحدة بالقيم المتوقعة
_audit.Verify(a => a.LogActionAsync(
        "Patient", patient.PatientId, "C",                 // القيم الفعلية
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Once);

// Verify — تحقق أنها لم تُستدعَ
_audit.Verify(a => a.LogActionAsync(
        It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Never);
```

### Mocking لـ `ICurrentUserSession`

**كلا `CurrentUser` و `IsAuthenticated` يجب إعدادهما معًا:**

```csharp
// مستخدم مسجل (admin)
var session = new Mock<ICurrentUserSession>();
session.Setup(s => s.CurrentUser).Returns(SeedAdmin());
session.Setup(s => s.IsAuthenticated).Returns(true);

// مستخدم غير مسجل
var emptySession = new Mock<ICurrentUserSession>();
emptySession.Setup(s => s.CurrentUser).Returns((Staff?)null);
emptySession.Setup(s => s.IsAuthenticated).Returns(false);
```

### Mocking لـ `ILogger<T>`

```csharp
// بدون setup (الأبسط)
var logger = Mock.Of<ILogger<MyService>>();

// مع تحقق من تسجيل رسالة معينة
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

---

## الاختبارات الثلاثة الإلزامية لكل Service جديدة

كل Service جديدة في المشروع تحتاج **3 ملفات اختبار على الأقل**:

```
Tests/
├── Services/
│   ├── {Name}ServiceTests.cs                        ← unit tests للمنطق
│   └── {Name}ServiceRegistrationTests.cs            ← DI registration test
└── Integration/
    └── {Name}{Workflow}EndToEndTests.cs             ← E2E مع InMemory DB
```

> **ملاحظة حول اسم ملف E2E:** في المشروع الحالي يوجد نمطان:
> - `{Name}ServiceIntegrationTests.cs` (مثل `BackupServiceIntegrationTests.cs`)
> - `{Name}{Workflow}EndToEndTests.cs` (مثل `AttendanceWorkflowEndToEndTests.cs`)
>
> **اختر النمط الأقرب لطبيعة الـ Workflow.** إن كان الـ Workflow اختبار ميزة معينة (مثل Backup)، استخدم `{Name}ServiceIntegrationTests.cs`. وإن كان اختبار سير عمل كامل، استخدم `{Name}{Workflow}EndToEndTests.cs`.

### DI Registration Test — النمط القياسي

```csharp
using FinalLabSystem.Data;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public sealed class BackupServiceRegistrationTests
{
    [Fact]
    public void IBackupService_IsRegisteredInDI()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDbContext<FinalLabDbContext>(o =>
            o.UseInMemoryDatabase("reg-test"));
        services.AddLogging();
        services.AddScoped<IBackupService, BackupService>();

        // Act
        var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<IBackupService>();   // ⚠️ GetRequiredService وليس GetService

        // Assert
        Assert.NotNull(resolved);
        Assert.IsType<BackupService>(resolved);
    }

    [Fact]
    public void BackupService_Lifetime_IsScoped()
    {
        // تأكد أن الـ service Scoped وليس Singleton
        var services = new ServiceCollection();
        services.AddScoped<IBackupService, BackupService>();
        var descriptor = services.First(d => d.ServiceType == typeof(IBackupService));

        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
```

---

## تسمية الاختبارات — المعيار المعتمد

```
MethodName_ExpectedResult_WhenCondition
```

**القاعدة:** يجب أن يكون اسم الاختبار ذاتي الشرح:
1. ما هو الـ method المختبَر (`MethodName`)
2. ما هو السلوك المتوقع (`ExpectedResult`)
3. تحت أي شرط (`WhenCondition`)

### ✅ أمثلة صحيحة من المشروع

```
CreateBackupAsync_ThrowsUnauthorized_WhenUserIsNotAdmin
GetPatientAsync_ReturnsNull_WhenPatientNotFound
RestoreBackupAsync_CreatesPreRestoreBackup_BeforeRestoring
RecordClockInAsync_CreatesRecordWithCorrectLateMinutes
EndToEnd_ClockInAndClockOut_Success
```

### ❌ أمثلة خاطئة

```
Test1
BackupTest
ShouldCreateBackup
CreateBackup
```

---

## الاختبارات المُعلَّمة (Theory + InlineData)

```csharp
[Theory]
[InlineData("admin", true)]
[InlineData("user", false)]
public async Task IsAdmin_ReturnsCorrectValue_WhenUserIsProvided(string username, bool expected)
{
    var (sut, _, _) = CreateSut(nameof(IsAdmin_ReturnsCorrectValue_WhenUserIsProvided) + username);
    var staff = new Staff { Username = username, IsAdmin = expected, IsActive = true };

    // ملاحظة: المثال للتوضيح — كودك الفعلي يختلف
    var result = staff.IsAdmin;

    Assert.Equal(expected, result);
}
```

> **تحذير:** لا تفرط في استخدام `[Theory]` لاختبارات معقدة. استخدمه للمدخلات البسيطة فقط (validation، enum checks).

---

## التعامل مع الوقت في الاختبارات

**المشكلة:** اختبارات تستخدم `DateTime.UtcNow` تفشل أحيانًا بسبب فرق التوقيت.

**الحل:**

```csharp
// ✅ صحيح: قارن مع هامش تسامح
Assert.True(Math.Abs((result.CreatedAt - expectedDate).TotalSeconds) < 5);

// ❌ خطأ: مقارنة دقيقة
Assert.Equal(expectedDate, result.CreatedAt);   // سيفشل بسبب clock skew
```

أو الأفضل: استخدم `DateOnly.FromDateTime(DateTime.UtcNow)` في الـ assertion:

```csharp
Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), result.AttendanceDate);
```

---

## الأنماط المحظورة في هذا المشروع

| المحظور | البديل الصحيح |
|---------|--------------|
| `Substitute.For<T>()` (NSubstitute) | `new Mock<T>()` (Moq) |
| `DateTime.Now` / `DateTime.Today` في الاختبارات | `DateTime.UtcNow` |
| `Mock<FinalLabDbContext>()` | `UseInMemoryDatabase(...)` |
| `WebApplicationFactory` | لا يوجد ASP.NET هنا |
| `PostgreSqlContainer` / `MsSqlContainer` (Testcontainers) | `UseInMemoryDatabase(...)` |
| اختبار code-behind في XAML | اختبر الـ ViewModel فقط |
| `Thread.Sleep` في الاختبارات | `Task.Delay` بحذر — يبطّئ الاختبارات |
| `result.Should().Be(expected)` (FluentAssertions) | `Assert.Equal(expected, result)` |
| `GetService<T>()` بدل `GetRequiredService<T>()` | `GetRequiredService<T>()` يفشل سريعًا إذا غير مسجّل |
| نسيان `.ConfigureWarnings(w => w.Ignore(...))` | أضفها دائمًا لتفادي warning في `dotnet build` |
| استخدام `FullName` بدلاً من `FullNameAr` | الحقل اسمه `FullNameAr` في كيان `Patient` |

---

## ملخص القالب الجاهز (Copy-Paste)

```csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace FinalLabSystem.Tests.Services;

public sealed class TemplateServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static Staff SeedAdmin() => new()
    {
        StaffId = 1, Username = "admin", DisplayName = "Admin",
        IsAdmin = true, IsActive = true, PasswordHash = "hash",
        Sex = "M", CreatedAt = DateTime.UtcNow
    };

    private static (TemplateService sut, FinalLabDbContext ctx, Mock<IAuditService> audit)
        CreateSut(string dbName, Staff? user = null)
    {
        var ctx = new FinalLabDbContext(CreateOptions(dbName));
        var session = new Mock<ICurrentUserSession>();
        session.Setup(s => s.CurrentUser).Returns(user ?? SeedAdmin());
        session.Setup(s => s.IsAuthenticated).Returns(user != null);
        var audit = new Mock<IAuditService>();
        var logger = Mock.Of<ILogger<TemplateService>>();
        var sut = new TemplateService(ctx, session.Object, audit.Object, logger);
        return (sut, ctx, audit);
    }

    [Fact]
    public async Task DoSomethingAsync_ReturnsExpected_WhenInputIsValid()
    {
        var (sut, ctx, audit) = CreateSut(nameof(DoSomethingAsync_ReturnsExpected_WhenInputIsValid));

        var result = await sut.DoSomethingAsync();

        Assert.NotNull(result);
    }
}
```

---

## المراجع

- `FinalLabSystem.Tests/Services/BackupServiceTests.cs` — نموذج ممتاز لخدمة معقدة بـ audit
- `FinalLabSystem.Tests/Services/AttendanceServiceTests.cs` — نموذج لخدمة بسيطة
- `FinalLabSystem.Tests/Services/AuthServiceTests.cs` — نموذج لخدمة بـ authorization
- `FinalLabSystem.Tests/Integration/AttendanceWorkflowEndToEndTests.cs` — نموذج E2E
- `AgentSkills/lis-xunit-testing-conventions/SKILL.md` — المهارة الأعمق الخاصة بالمشروع
- `AgentSkills/audit-trail-conventions/SKILL.md` — كيف يعمل Audit التلقائي (لفهم الـ Verify)