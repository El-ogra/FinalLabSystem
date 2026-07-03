---
name: csharp-testing
description: C# and .NET testing patterns with xUnit and Moq for WPF/MVVM desktop applications. Covers unit tests, EF Core InMemory integration tests, and DI registration tests.
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

---

## قواعد الاختبار الإلزامية في FinalLabSystem

| القاعدة | الصحيح | الخطأ |
|---------|--------|-------|
| التواريخ | `DateTime.UtcNow` | `DateTime.Now` أو `DateTime.Today` |
| بيانات المريض | `Sex = "M"` إلزامياً | حذف حقل Sex |
| Moq اختياري | `It.IsAny<string>()` | تمرير قيمة ثابتة لمعامل غير مهم |
| DbContext | `UseInMemoryDatabase(Guid.NewGuid().ToString())` | Mock للـ DbContext |
| الخدمات | `public` إلزامياً | `internal` — يسبب فشل الاختبار |

---

## هيكل الاختبار القياسي في المشروع

```csharp
public sealed class PatientServiceTests : IDisposable
{
    private readonly FinalLabDbContext _db;
    private readonly Mock<IAuditService> _audit = new();
    private readonly Mock<ICurrentUserSession> _session = new();
    private readonly PatientService _sut;

    public PatientServiceTests()
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new FinalLabDbContext(options);
        _session.Setup(s => s.CurrentUser)
                .Returns(new Staff { StaffId = 1, IsAdmin = true, Sex = "M" });
        _sut = new PatientService(_db, _audit.Object, _session.Object);
    }

    public void Dispose() => _db.Dispose();
}
```

---

## Seed Data — القالب القياسي

```csharp
// Patient — Sex = "M" إلزامي دائماً
var patient = new Patient
{
    PatientId   = 1,
    FullName    = "أحمد محمد",
    Sex         = "M",          // ⚠️ إلزامي — لا تحذفه
    DateOfBirth = new DateTime(1990, 1, 1),
    CreatedAt   = DateTime.UtcNow   // ⚠️ UtcNow دائماً
};

// Staff
var staff = new Staff
{
    StaffId      = 1,
    FullName     = "موظف تجريبي",
    Sex          = "M",
    IsAdmin      = true,
    PasswordHash = "hash",
    Username     = "test_user"
};

// Visit
var visit = new Visit
{
    VisitId    = 1,
    PatientId  = 1,
    StaffId    = 1,
    VisitDate  = DateTime.UtcNow,   // ⚠️ UtcNow دائماً
    VisitCode  = "V20260703-0001"
};
```

---

## Moq — الأنماط المعتمدة

```csharp
// Setup
_audit.Setup(a => a.LogActionAsync(
        It.IsAny<string>(),   // tableName
        It.IsAny<int>(),      // recordId
        It.IsAny<string>(),   // action (حرف واحد: "C","U","D","B","R")
        It.IsAny<int>(),      // staffId
        It.IsAny<string>()))  // notes
     .Returns(Task.CompletedTask);

// Verify — تحقق أن الدالة استُدعيت مرة واحدة
_audit.Verify(a => a.LogActionAsync(
        "Patient", patient.PatientId, "C",
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Once);

// Verify — تحقق أنها لم تُستدعَ
_audit.Verify(a => a.LogActionAsync(
        It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(),
        It.IsAny<int>(), It.IsAny<string>()),
    Times.Never);
```

---

## الاختبارات الثلاثة الإلزامية لكل Service جديدة

كل Service جديدة في المشروع تحتاج **3 ملفات اختبار**:

```
Tests/
  Services/
    {Name}ServiceTests.cs          ← unit tests للمنطق
    {Name}ServiceRegistrationTests.cs  ← DI registration test
  Integration/
    {Name}EndToEndTests.cs         ← E2E مع InMemory DB
```

### DI Registration Test — النمط القياسي

```csharp
public sealed class BackupServiceRegistrationTests
{
    [Fact]
    public void IBackupService_IsRegistered_InDI()
    {
        var services = new ServiceCollection();
        // أضف ما يحتاجه الـ service
        services.AddDbContext<FinalLabDbContext>(o =>
            o.UseInMemoryDatabase("reg-test"));
        services.AddScoped<IBackupService, BackupService>();

        var provider = services.BuildServiceProvider();
        var resolved = provider.GetService<IBackupService>();

        Assert.NotNull(resolved);
    }

    [Fact]
    public void BackupService_LifeTime_IsScoped()
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

أمثلة صحيحة من المشروع:
```
CreateBackupAsync_ThrowsUnauthorized_WhenUserIsNotAdmin
GetPatientAsync_ReturnsNull_WhenPatientNotFound
RestoreBackupAsync_CreatesPreRestoreBackup_BeforeRestoring
```

---

## الأنماط المحظورة في هذا المشروع

| المحظور | البديل الصحيح |
|---------|--------------|
| `Substitute.For<T>()` (NSubstitute) | `new Mock<T>()` (Moq) |
| `DateTime.Now` في الاختبارات | `DateTime.UtcNow` |
| `Mock<FinalLabDbContext>()` | `UseInMemoryDatabase(...)` |
| `WebApplicationFactory` | لا يوجد ASP.NET هنا |
| `PostgreSqlContainer` (Testcontainers) | `UseInMemoryDatabase(...)` |
| اختبار code-behind في XAML | اختبر الـ ViewModel فقط |
| `Thread.Sleep` في الاختبارات | `Task.Delay` |

