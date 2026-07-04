---
name: resolving-merge-conflicts
description: "مهارة متخصصة في resolving-merge-conflicts لتحسين سير العمل وتطوير المشروع بكفاءة."
---

# Resolving Merge Conflicts — FinalLabSystem

> حل تعارضات git merge أو rebase بوعي بنية المشروع. الأمثلة أدناه مأخوذة من سيناريوهات حقيقية في FinalLabSystem.

## الخطوات العامة (5 خطوات)

### 1. اعرف الحالة الحالية للـ merge/rebase

```bash
# حالة الـ merge
git status

# السجل الذي أدى للتعارض
git log --oneline --graph --all -20

# قائمة الملفات المتعارضة
git diff --name-only --diff-filter=U

# عرض تعارضات ملف واحد بوضوح
git diff --check <path>
```

**اقرأ رسالة الـ merge** (تظهر في `.git/MERGE_MSG`) لتعرف **الهدف من الدمج**. هذا يحدد أي طرف يحتفظ به عند التعارضات الحقيقية.

### 2. حدد المصادر الأولية لكل تعارض

قبل تعديل أي سطر:

```bash
# تعارض في Audit logic؟ ابحث في التاريخ
git log --all --oneline -- "Services/*Service.cs"
git log --all --oneline -- "Models/*.cs" | head -20

# اقرأ commit messages المرتبطة
git show <commit-hash> --stat
```

**اسأل نفسك:**
- لماذا تم تغيير هذا السطر في الفرع A؟
- لماذا تم تغييره في الفرع B؟
- هل التغييران متناقضان فعلاً، أم يمكن دمجهما؟

### 3. حل كل تعارض مع الحفاظ على النوايا

**القاعدة الذهبية:** **لا تخترع سلوكاً جديداً** أثناء حل التعارض. احتفظ بالنوايا الأصلية أو اختر طرفاً واحداً ووثّق التنازل.

#### ✅ علامات التعارض وكيفية حلها

```
<<<<<<< HEAD (الفرع الحالي)
    كود من الفرع الحالي
=======
    كود من الفرع المدمج
>>>>>>> branch-name (الفرع الوارد)
```

**عند الحل:**
1. احذف `<<<<<<< HEAD` و `=======` و `>>>>>>> branch-name`.
2. احتفظ بالكود الصحيح (أو ادمج الاثنين إن أمكن).
3. تحقق من أن الكود يحقق **هدف الـ merge**.

### 4. اكتشف الفحوصات الآلية للمشروع وشغّلها

```bash
# 1. بناء نظيف
dotnet build --nologo 2>&1 | tee build.log

# 2. تشغيل الاختبارات
dotnet test --nologo --logger "console;verbosity=normal"

# 3. التحقق من لا توجد migrations معلّقة
dotnet ef migrations list --project FinalLabSystem.csproj

# 4. التحقق من DI
grep -n "AddScoped\|AddTransient\|AddSingleton" App.xaml.cs
```

> **إذا فشل أي فحص:** أصلحه قبل المتابعة. لا تُنهِ الـ merge بكود مكسور.

### 5. أنهِ الـ merge/rebase

```bash
# بعد حل كل التعارضات
git add .
git commit   # لإنهاء merge
# أو
git rebase --continue   # لإنهاء rebase
```

**⚠️ ممنوع:** `git merge --abort` أو `git rebase --abort` إلا بعد التشاور مع المستخدم. الـ abort يلغي كل العمل.

---

## أمثلة واقعية من FinalLabSystem

### المثال 1: تعارض في Audit Logic

**السياق:** فرع `feature/blood-bank` أضاف استدعاء `_audit.LogActionAsync` في `BloodBankService.CreateAsync`. في نفس الوقت، فرع `feature/improved-audit` غيّر توقيع الدالة في `IAuditService` من `(string action, int staffId)` إلى `(AuditAction action, int staffId, string? notes)`.

**التعارض في `BloodBankService.cs`:**

```csharp
public async Task<int> CreateAsync(BloodBankDto dto, int staffId)
{
    var entity = new BloodBank { /* ... */ };
    _db.BloodBanks.Add(entity);
    await _db.SaveChangesAsync();
<<<<<<< HEAD
    await _audit.LogActionAsync(nameof(BloodBank), entity.Id, "INSERT", staffId);
    return entity.Id;
=======
    await _audit.LogActionAsync(AuditAction.Insert, entity.Id, staffId, "BloodBank created");
    return entity.Id;
>>>>>>> feature/improved-audit
}
```

**الحل:**

```csharp
public async Task<int> CreateAsync(BloodBankDto dto, int staffId)
{
    var entity = new BloodBank { /* ... */ };
    _db.BloodBanks.Add(entity);
    await _db.SaveChangesAsync();
    // ✅ اعتمدنا توقيع feature/improved-audit (أحدث وأدق)
    await _audit.LogActionAsync(AuditAction.Insert, entity.Id, staffId, $"BloodBank {entity.Code} created");
    return entity.Id;
}
```

**التحقق بعد الحل:**
```bash
# تأكد أن كل استدعاءات _audit.LogActionAsync تستخدم التوقيع الجديد
grep -rn "_audit.LogActionAsync" Services/ | grep -v "AuditAction\."
# (يجب أن يكون فارغاً)
```

**راجع:** `audit-trail-conventions` للتوقيع الصحيح.

---

### المثال 2: تعارض في ViewModel (ObservableObject vs ViewModelBase)

**السياق:** فرع قديم من contributor خارجي أنشأ ViewModel يرث `ObservableObject`، في حين أن معايير FinalLabSystem تمنع ذلك.

**التعارض:**

```csharp
<<<<<<< HEAD
public partial class PatientsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _searchText = string.Empty;

    [RelayCommand]
    private async Task SearchAsync() { /* ... */ }
=======
public sealed class PatientsViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public AsyncRelayCommand SearchAsyncCommand { get; }
>>>>>>> feature/mvvm-conventions
}
```

**الحل:** احتفظ دائماً بفرع `feature/mvvm-conventions` (معايير المشروع):

```csharp
public sealed class PatientsViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public AsyncRelayCommand SearchAsyncCommand { get; }
    // ... constructor + commands
}
```

**⚠️ إذا كان الفرع الآخر أحدث في الـ logic (مثلاً شروط CanExecute):** انقل المنطق إلى النسخة الصحيحة.

**راجع:** `wpf-mvvm-conventions` للهيكل الصحيح.

---

### المثال 3: تعارض في EF Core Migration

**السياق:** فرعان يضيفان عمود `IsActive` لجدولين مختلفين. الـ auto-generated migrations أسماء متشابهة.

**التعارض في `Migrations/`:**

```bash
$ git status
Unmerged paths:
  both modified:   Migrations/20260701_AddIsActiveToPatients.cs
  both modified:   Migrations/20260702_AddIsActiveToVisits.cs
```

**الحل:**

```bash
# 1. اقرأ كلا الـ migrations
cat Migrations/20260701_AddIsActiveToPatients.cs
cat Migrations/20260702_AddIsActiveToVisits.cs

# 2. عادةً ما يكون التعارض في اسم الـ migration أو في ترتيب الـ operations
# 3. احتفظ بـ Up/Down methods لكلا الجدولين (لا تحذف أحدهما)
```

**مثال على الحل:**

```csharp
// ✅ في AddIsActiveToPatients.cs — احتفظ بعمليات Patients فقط
public partial class AddIsActiveToPatients : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Patients",
            type: "bit",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Patients");
    }
}
```

```csharp
// ✅ في AddIsActiveToVisits.cs — احتفظ بعمليات Visits فقط
public partial class AddIsActiveToVisits : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "Visits",
            type: "bit",
            nullable: false,
            defaultValue: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IsActive",
            table: "Visits");
    }
}
```

**⚠️ بعد الحل، حدّث `Migrations/FinalLabDbContextModelSnapshot.cs` يدوياً إن لزم** (عادة EF يحدثه عند البناء التالي).

**راجع:** `ef-core-migration-safety` للقواعد الكاملة.

---

### المثال 4: تعارض في DI Registration

**السياق:** فرعان يضيفان تسجيل خدمات في `App.xaml.cs.ConfigureServices`. التعارض في ترتيب أو موقع الكود.

**التعارض في `App.xaml.cs`:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... registrations ...

<<<<<<< HEAD
    services.AddScoped<IBloodBankService, BloodBankService>();
    services.AddTransient<BloodBankWindowViewModel>();
=======
    services.AddScoped<IVaccinationService, VaccinationService>();
    services.AddTransient<VaccinationWindowViewModel>();
>>>>>>> feature/vaccination
}
```

**الحل:**

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // ... registrations ...

    // ✅ كلا التسجيلين معاً، مرتبين أبجدياً (حسب قاعدة المشروع)
    services.AddScoped<IBloodBankService, BloodBankService>();
    services.AddScoped<IVaccinationService, VaccinationService>();
    services.AddTransient<BloodBankWindowViewModel>();
    services.AddTransient<VaccinationWindowViewModel>();
}
```

**القاعدة:** الترتيب أبجدي حسب نوع الـ Service/VM/Window.

**راجع:** `di-and-navigation-registration`.

---

### المثال 5: تعارض في ملف `FinalLabDbContextModelSnapshot.cs`

**السياق:** هذا الملف يُولَّد تلقائياً ويصعب دمجه يدوياً.

**الحل الموصى به:**

```bash
# 1. احتفظ بأحد الطرفين (عادة الأحدث تاريخاً)
git checkout --theirs Migrations/FinalLabDbContextModelSnapshot.cs
# أو
git checkout --ours Migrations/FinalLabDbContextModelSnapshot.cs

# 2. أعد توليده
dotnet ef migrations add TempRegenerate --project FinalLabSystem.csproj

# 3. احذف الـ migration المؤقت
dotnet ef migrations remove

# 4. ابنِ المشروع
dotnet build

# 5. إن ظهرت أخطاء، أضف العمليات المفقودة يدوياً من الطرف الآخر
```

**⚠️ هذا الملف حساس:** أي خطأ فيه يمنع تشغيل الـ migrations على production.

---

## نصائح خاصة بـ FinalLabSystem

### ✅ افعل

- ✅ اقرأ commit messages المرتبطة قبل الحل.
- ✅ احترم معايير المشروع (ViewModelBase، Audit calls، DI Registration).
- ✅ شغّل `dotnet build` و `dotnet test` كاملاً بعد كل حل.
- ✅ وثّق القرارات المعمارية في رسالة الـ commit.
- ✅ تحقق من `Migrations/FinalLabDbContextModelSnapshot.cs` بعد كل دمج.

### ❌ لا تفعل

- ❌ `git merge --abort` أو `git rebase --abort` بدون استشارة.
- ❌ اختراع سلوك جديد أثناء الحل.
- ❌ تجاهل الـ Validation Gates (dotnet build/test).
- ❌ حذف استدعاءات `_audit.LogActionAsync`.
- ❌ استبدال `ViewModelBase` بـ `ObservableObject` لحل سريع.
- ❌ تعديل Migration methods يدوياً إلا للضرورة (الأفضل إعادة توليدها).

---

## قائمة المراجع

| الموضوع | المهارة |
|---------|---------|
| Audit calls صحيحة | `audit-trail-conventions` |
| ViewModel صحيح | `wpf-mvvm-conventions` |
| Migrations آمنة | `ef-core-migration-safety` |
| DI Registration | `di-and-navigation-registration` |
| Validation بعد الدمج | `qa` أو `review` |
| اختبارات بعد الدمج | `lis-xunit-testing-conventions` |