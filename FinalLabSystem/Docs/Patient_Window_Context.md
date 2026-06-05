# Patient Window Context

تم إنشاء هذا الملف تلقائياً من ملفات المشروع الحالية في: `C:\Users\LAP LINK\source\repos\FinalLabSystem\FinalLabSystem`.
الهدف: جمع سياق نافذة إضافة/تعديل بيانات المرضى دون تعديل أي كود موجود.

---
## الجزء الأول: قاعدة البيانات والنماذج (Models)

### 1. البنية الكاملة لجدول المرضى (Patients table)
- الجدول الموجود باسم `Patient` وليس `Patients` حسب `DbContext`.
- يوجد `PatientId` في الكلاس ويقابله العمود `patient_id` في قاعدة البيانات.
- لا يوجد عمود باسم `LabId` في `Patient.cs` أو تكوين `Patient`.
- يوجد `PatientCode` ويقابله `patient_code` مع فهرس Unique حسب الـ Snapshot/DbContext.
- `PatientId` مولد تلقائياً من SQL Server عبر `ValueGeneratedOnAdd()` و`UseIdentityColumn()` في الـ Snapshot.
- التاريخ المرضي ليس عموداً داخل `Patient`; يوجد جدول مستقل باسم `PatientMedicalHistory` مرتبط بـ `patient_id`.

#### جدول Patient
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.Patient", b =>
                {
                    b.Property<int>("PatientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("patient_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PatientId"));

                    b.Property<string>("Address")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)")
                        .HasColumnName("address");

                    b.Property<double?>("ApproxAge")
                        .HasColumnType("float")
                        .HasColumnName("approx_age");

                    b.Property<string>("ApproxAgeUnit")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasDefaultValue("Years")
                        .HasColumnName("approx_age_unit");

                    b.Property<string>("BloodType")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)")
                        .HasColumnName("blood_type");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<DateOnly?>("DateOfBirth")
                        .HasColumnType("date")
                        .HasColumnName("date_of_birth");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("email");

                    b.Property<string>("FullNameAr")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("full_name_ar");

                    b.Property<string>("FullNameEn")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("full_name_en");

                    b.Property<string>("NationalId")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("national_id");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("notes");

                    b.Property<string>("PatientCode")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("patient_code");

                    b.Property<string>("Phone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("phone");

                    b.Property<string>("Phone2")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("phone2");

                    b.Property<string>("Sex")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("nchar(1)")
                        .HasColumnName("sex")
                        .IsFixedLength();

                    b.Property<string>("Title")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("title");

                    b.HasKey("PatientId")
                        .HasName("PK__Patient__4D5CE47686F206AD");

                    b.HasIndex("CreatedBy");

                    b.HasIndex(new[] { "FullNameAr" }, "IX_Patient_Name");

                    b.HasIndex(new[] { "Phone" }, "IX_Patient_Phone");

                    b.HasIndex(new[] { "PatientCode" }, "UQ__Patient__58D46F1F4F7F8E9D")
                        .IsUnique();

                    b.ToTable("Patient", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PatientId).HasName("PK__Patient__4D5CE47686F206AD");

            entity.ToTable("Patient");

            entity.HasIndex(e => e.FullNameAr, "IX_Patient_Name");

            entity.HasIndex(e => e.Phone, "IX_Patient_Phone");

            entity.HasIndex(e => e.PatientCode, "UQ__Patient__58D46F1F4F7F8E9D").IsUnique();

            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.ApproxAge).HasColumnName("approx_age");
            entity.Property(e => e.ApproxAgeUnit)
                .HasMaxLength(10)
                .HasDefaultValue("Years")
                .HasColumnName("approx_age_unit");
            entity.Property(e => e.BloodType)
                .HasMaxLength(5)
                .HasColumnName("blood_type");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullNameAr)
                .HasMaxLength(150)
                .HasColumnName("full_name_ar");
            entity.Property(e => e.FullNameEn)
                .HasMaxLength(150)
                .HasColumnName("full_name_en");
            entity.Property(e => e.NationalId)
                .HasMaxLength(20)
                .HasColumnName("national_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientCode)
                .HasMaxLength(30)
                .HasColumnName("patient_code");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Phone2)
                .HasMaxLength(20)
                .HasColumnName("phone2");
            entity.Property(e => e.Sex)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("sex");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("title");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Patient_CreatedBy");
        });
````

#### جدول PatientMedicalHistory
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.PatientMedicalHistory", b =>
                {
                    b.Property<int>("MedicalHistoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("medical_history_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MedicalHistoryId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("created_at");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("end_date");

                    b.Property<string>("HistoryType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("history_type");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit")
                        .HasColumnName("is_active");

                    b.Property<int>("PatientId")
                        .HasColumnType("int")
                        .HasColumnName("patient_id");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("start_date");

                    b.HasKey("MedicalHistoryId")
                        .HasName("PK_PatientMedicalHistory");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("PatientId");

                    b.ToTable("PatientMedicalHistory", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<PatientMedicalHistory>(entity =>
        {
            entity.HasKey(e => e.MedicalHistoryId).HasName("PK_PatientMedicalHistory");

            entity.ToTable("PatientMedicalHistory");

            entity.Property(e => e.MedicalHistoryId).HasColumnName("medical_history_id");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.HistoryType).HasColumnName("history_type");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasOne(d => d.Patient).WithMany(p => p.PatientMedicalHistories)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PatientMedicalHistory_Patient");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PatientMedicalHistories)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_PatientMedicalHistory_Staff");
        });
````

### 2. البنية الكاملة لجداول جهات التحويل
- جهات التحويل مخزنة في جدول مستقل باسم `ReferralSource`.
- الأعمدة تشمل: `referral_id`, `source_type`, `title`, `source_name`, بيانات الاتصال، `commission_rate`, `is_active`, `notes`, `created_at`, و`scheme_id`.

#### جدول ReferralSource
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.ReferralSource", b =>
                {
                    b.Property<int>("ReferralId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("referral_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReferralId"));

                    b.Property<string>("Address")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)")
                        .HasColumnName("address");

                    b.Property<double>("CommissionRate")
                        .HasColumnType("float")
                        .HasColumnName("commission_rate");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("email");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("is_active");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("notes");

                    b.Property<string>("Phone")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("phone");

                    b.Property<string>("Phone2")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("phone2");

                    b.Property<int?>("SchemeId")
                        .HasColumnType("int")
                        .HasColumnName("scheme_id");

                    b.Property<string>("SourceName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("source_name");

                    b.Property<string>("SourceType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("DOCTOR")
                        .HasColumnName("source_type");

                    b.Property<string>("Title")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("title");

                    b.HasKey("ReferralId")
                        .HasName("PK__Referral__62BC1805CCD8D512");

                    b.HasIndex("SchemeId");

                    b.ToTable("ReferralSource", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<ReferralSource>(entity =>
        {
            entity.HasKey(e => e.ReferralId).HasName("PK__Referral__62BC1805CCD8D512");

            entity.ToTable("ReferralSource");

            entity.Property(e => e.ReferralId).HasColumnName("referral_id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Phone2)
                .HasMaxLength(20)
                .HasColumnName("phone2");
            entity.Property(e => e.SourceName)
                .HasMaxLength(150)
                .HasColumnName("source_name");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .HasDefaultValue("DOCTOR")
                .HasColumnName("source_type");
            entity.Property(e => e.Title)
                .HasMaxLength(20)
                .HasColumnName("title");
        });
````

### 3. البنية الكاملة لجداول التحاليل
- التحاليل الفردية مخزنة في `TestType`، ومكوناتها في `TestComponent`.
- الباقات/المجموعات المركبة موجودة باسم `TestProfile` مع جدول وسيط `TestProfileItem`.
- الفئات الرئيسية موجودة باسم `TestCategory`، والمجموعات الفرعية باسم `TestGroup`.
- ربط الزيارة/المريض بالتحاليل المطلوبة يتم عبر `VisitTest`; المريض يرتبط عبر `Visit.patient_id` ثم `VisitTest.visit_id`.

#### جدول TestCategory
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("category_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("category_code");

                    b.Property<string>("CategoryNameAr")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("category_name_ar");

                    b.Property<string>("CategoryNameEn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("category_name_en");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("is_active");

                    b.Property<short>("SortOrder")
                        .HasColumnType("smallint")
                        .HasColumnName("sort_order");

                    b.HasKey("CategoryId")
                        .HasName("PK__TestCate__D54EE9B457D2B89D");

                    b.HasIndex(new[] { "CategoryCode" }, "UQ__TestCate__BC9D1E7C823E4CF2")
                        .IsUnique();

                    b.ToTable("TestCategory", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__TestCate__D54EE9B457D2B89D");

            entity.ToTable("TestCategory");

            entity.HasIndex(e => e.CategoryCode, "UQ__TestCate__BC9D1E7C823E4CF2").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryCode)
                .HasMaxLength(20)
                .HasColumnName("category_code");
            entity.Property(e => e.CategoryNameAr)
                .HasMaxLength(100)
                .HasColumnName("category_name_ar");
            entity.Property(e => e.CategoryNameEn)
                .HasMaxLength(100)
                .HasColumnName("category_name_en");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });
````

#### جدول TestGroup
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestGroup", b =>
                {
                    b.Property<int>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("group_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("GroupId"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("int")
                        .HasColumnName("category_id");

                    b.Property<string>("GroupCode")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasColumnName("group_code");

                    b.Property<string>("GroupNameAr")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("group_name_ar");

                    b.Property<string>("GroupNameEn")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("group_name_en");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("is_active");

                    b.Property<short>("SortOrder")
                        .HasColumnType("smallint")
                        .HasColumnName("sort_order");

                    b.HasKey("GroupId")
                        .HasName("PK__TestGrou__D57795A03A3FA260");

                    b.HasIndex("CategoryId");

                    b.HasIndex(new[] { "GroupCode" }, "UQ__TestGrou__3180DCD1BE8C3F6E")
                        .IsUnique();

                    b.ToTable("TestGroup", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestGroup>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__TestGrou__D57795A03A3FA260");

            entity.ToTable("TestGroup");

            entity.HasIndex(e => e.GroupCode, "UQ__TestGrou__3180DCD1BE8C3F6E").IsUnique();

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.GroupCode)
                .HasMaxLength(20)
                .HasColumnName("group_code");
            entity.Property(e => e.GroupNameAr)
                .HasMaxLength(100)
                .HasColumnName("group_name_ar");
            entity.Property(e => e.GroupNameEn)
                .HasMaxLength(100)
                .HasColumnName("group_name_en");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Category).WithMany(p => p.TestGroups)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestGroup_Category");
        });
````

#### جدول TestType
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestType", b =>
                {
                    b.Property<int>("TesttypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("testtype_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TesttypeId"));

                    b.Property<double>("DefaultPrice")
                        .HasColumnType("float")
                        .HasColumnName("default_price");

                    b.Property<string>("DefaultTubeColor")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("default_tube_color");

                    b.Property<string>("DefaultTubeType")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("default_tube_type");

                    b.Property<int>("GroupId")
                        .HasColumnType("int")
                        .HasColumnName("group_id");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("is_active");

                    b.Property<bool>("IsOutsourceable")
                        .HasColumnType("bit")
                        .HasColumnName("is_outsourceable");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("notes");

                    b.Property<string>("SampleType")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("sample_type");

                    b.Property<short>("SortOrder")
                        .HasColumnType("smallint")
                        .HasColumnName("sort_order");

                    b.Property<string>("SpecialType")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("STANDARD")
                        .HasColumnName("special_type");

                    b.Property<short>("TurnaroundHours")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("smallint")
                        .HasDefaultValue((short)24)
                        .HasColumnName("turnaround_hours");

                    b.Property<string>("TypeAbbrev")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("type_abbrev");

                    b.Property<string>("TypeCode")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("type_code");

                    b.Property<string>("TypeNameAr")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("type_name_ar");

                    b.Property<string>("TypeNameEn")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("type_name_en");

                    b.HasKey("TesttypeId")
                        .HasName("PK__TestType__8547CC22615C1CAA");

                    b.HasIndex("GroupId");

                    b.HasIndex(new[] { "TypeCode" }, "UQ__TestType__2CB4DBF5D148A91E")
                        .IsUnique();

                    b.ToTable("TestType", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TesttypeId).HasName("PK__TestType__8547CC22615C1CAA");

            entity.ToTable("TestType");

            entity.HasIndex(e => e.TypeCode, "UQ__TestType__2CB4DBF5D148A91E").IsUnique();

            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");
            entity.Property(e => e.DefaultPrice).HasColumnName("default_price");
            entity.Property(e => e.DefaultTubeColor)
                .HasMaxLength(30)
                .HasColumnName("default_tube_color");
            entity.Property(e => e.DefaultTubeType)
                .HasMaxLength(50)
                .HasColumnName("default_tube_type");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsOutsourceable).HasColumnName("is_outsourceable");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.SampleType)
                .HasMaxLength(50)
                .HasColumnName("sample_type");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.SpecialType)
                .HasMaxLength(20)
                .HasDefaultValue("STANDARD")
                .HasColumnName("special_type");
            entity.Property(e => e.TurnaroundHours)
                .HasDefaultValue((short)24)
                .HasColumnName("turnaround_hours");
            entity.Property(e => e.TypeAbbrev)
                .HasMaxLength(30)
                .HasColumnName("type_abbrev");
            entity.Property(e => e.TypeCode)
                .HasMaxLength(30)
                .HasColumnName("type_code");
            entity.Property(e => e.TypeNameAr)
                .HasMaxLength(150)
                .HasColumnName("type_name_ar");
            entity.Property(e => e.TypeNameEn)
                .HasMaxLength(150)
                .HasColumnName("type_name_en");

            entity.HasOne(d => d.Group).WithMany(p => p.TestTypes)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestType_Group");
        });
````

#### جدول TestComponent
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestComponent", b =>
                {
                    b.Property<int>("ComponentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("component_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ComponentId"));

                    b.Property<string>("ComponentCode")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("component_code");

                    b.Property<string>("ComponentNameAr")
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("component_name_ar");

                    b.Property<string>("ComponentNameEn")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("component_name_en");

                    b.Property<byte>("DecimalPlaces")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint")
                        .HasDefaultValue((byte)2)
                        .HasColumnName("decimal_places");

                    b.Property<bool>("IsActive")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true)
                        .HasColumnName("is_active");

                    b.Property<string>("ResultType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)")
                        .HasDefaultValue("NUMERIC")
                        .HasColumnName("result_type");

                    b.Property<short>("SortOrder")
                        .HasColumnType("smallint")
                        .HasColumnName("sort_order");

                    b.Property<int>("TesttypeId")
                        .HasColumnType("int")
                        .HasColumnName("testtype_id");

                    b.Property<string>("Unit")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("unit");

                    b.HasKey("ComponentId")
                        .HasName("PK__TestComp__AEB1DA59AF0CE64E");

                    b.HasIndex(new[] { "TesttypeId", "ComponentCode" }, "UQ_Component_Code")
                        .IsUnique();

                    b.ToTable("TestComponent", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestComponent>(entity =>
        {
            entity.HasKey(e => e.ComponentId).HasName("PK__TestComp__AEB1DA59AF0CE64E");

            entity.ToTable("TestComponent");

            entity.HasIndex(e => new { e.TesttypeId, e.ComponentCode }, "UQ_Component_Code").IsUnique();

            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.ComponentCode)
                .HasMaxLength(30)
                .HasColumnName("component_code");
            entity.Property(e => e.ComponentNameAr)
                .HasMaxLength(150)
                .HasColumnName("component_name_ar");
            entity.Property(e => e.ComponentNameEn)
                .HasMaxLength(150)
                .HasColumnName("component_name_en");
            entity.Property(e => e.DecimalPlaces)
                .HasDefaultValue((byte)2)
                .HasColumnName("decimal_places");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ResultType)
                .HasMaxLength(15)
                .HasDefaultValue("NUMERIC")
                .HasColumnName("result_type");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");
            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .HasColumnName("unit");

            entity.HasOne(d => d.Testtype).WithMany(p => p.TestComponents)
                .HasForeignKey(d => d.TesttypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Component_Type");
        });
````

#### جدول TestProfile
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestProfile", b =>
                {
                    b.Property<int>("ProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("profile_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProfileId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("created_at");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("description");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit")
                        .HasColumnName("is_active");

                    b.Property<string>("ProfileNameAr")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("profile_name_ar");

                    b.Property<string>("ProfileNameEn")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("profile_name_en");

                    b.HasKey("ProfileId")
                        .HasName("PK_TestProfile");

                    b.HasIndex("CreatedBy");

                    b.ToTable("TestProfile", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestProfile>(entity =>
        {
            entity.HasKey(e => e.ProfileId).HasName("PK_TestProfile");

            entity.ToTable("TestProfile");

            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.ProfileNameAr).HasColumnName("profile_name_ar");
            entity.Property(e => e.ProfileNameEn).HasColumnName("profile_name_en");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.TestProfiles)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_TestProfile_Staff");
        });
````

#### جدول TestProfileItem
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.TestProfileItem", b =>
                {
                    b.Property<int>("ProfileItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("profile_item_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProfileItemId"));

                    b.Property<int>("ProfileId")
                        .HasColumnType("int")
                        .HasColumnName("profile_id");

                    b.Property<int?>("SortOrder")
                        .HasColumnType("int")
                        .HasColumnName("sort_order");

                    b.Property<int>("TestTypeId")
                        .HasColumnType("int")
                        .HasColumnName("testtype_id");

                    b.HasKey("ProfileItemId")
                        .HasName("PK_TestProfileItem");

                    b.HasIndex("ProfileId");

                    b.HasIndex("TestTypeId");

                    b.ToTable("TestProfileItem", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<TestProfileItem>(entity =>
        {
            entity.HasKey(e => e.ProfileItemId).HasName("PK_TestProfileItem");

            entity.ToTable("TestProfileItem");

            entity.Property(e => e.ProfileItemId).HasColumnName("profile_item_id");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.TestTypeId).HasColumnName("testtype_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Profile).WithMany(p => p.TestProfileItems)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestProfileItem_TestProfile");

            entity.HasOne(d => d.TestType).WithMany(p => p.TestProfileItems)
                .HasForeignKey(d => d.TestTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestProfileItem_TestType");
        });
````

#### جدول VisitTest
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.VisitTest", b =>
                {
                    b.Property<int>("VisitTestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("visit_test_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VisitTestId"));

                    b.Property<DateTime>("AddedAt")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("added_at")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<int?>("AddedBy")
                        .HasColumnType("int")
                        .HasColumnName("added_by");

                    b.Property<string>("CurrentStage")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)")
                        .HasDefaultValue("PENDING")
                        .HasColumnName("current_stage");

                    b.Property<int?>("ExternalLabId")
                        .HasColumnType("int")
                        .HasColumnName("external_lab_id");

                    b.Property<bool>("IsOutsourced")
                        .HasColumnType("bit")
                        .HasColumnName("is_outsourced");

                    b.Property<double?>("OutsourceCost")
                        .HasColumnType("float")
                        .HasColumnName("outsource_cost");

                    b.Property<DateTime?>("OutsourceResultReceivedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("outsource_result_received_at");

                    b.Property<DateTime?>("OutsourceSentAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("outsource_sent_at");

                    b.Property<int?>("OutsourceSentBy")
                        .HasColumnType("int")
                        .HasColumnName("outsource_sent_by");

                    b.Property<double>("PriceCharged")
                        .HasColumnType("float")
                        .HasColumnName("price_charged");

                    b.Property<int>("TesttypeId")
                        .HasColumnType("int")
                        .HasColumnName("testtype_id");

                    b.Property<int?>("TubeId")
                        .HasColumnType("int")
                        .HasColumnName("tube_id");

                    b.Property<int>("VisitId")
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    b.HasKey("VisitTestId")
                        .HasName("PK__VisitTes__D6ECAC167778287D");

                    b.HasIndex("AddedBy");

                    b.HasIndex("ExternalLabId");

                    b.HasIndex("OutsourceSentBy");

                    b.HasIndex("TesttypeId");

                    b.HasIndex("TubeId");

                    b.HasIndex(new[] { "VisitId", "TesttypeId" }, "UQ_VisitTest")
                        .IsUnique();

                    b.ToTable("VisitTest", null, t =>
                        {
                            t.HasTrigger("TR_VisitTest_SyncBalance");
                        });

                    b.HasAnnotation("SqlServer:UseSqlOutputClause", false);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<VisitTest>(entity =>
        {
            entity.HasKey(e => e.VisitTestId).HasName("PK__VisitTes__D6ECAC167778287D");

            entity.ToTable("VisitTest", tb => tb.HasTrigger("TR_VisitTest_SyncBalance"));

            entity.HasIndex(e => new { e.VisitId, e.TesttypeId }, "UQ_VisitTest").IsUnique();

            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");
            entity.Property(e => e.AddedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("added_at");
            entity.Property(e => e.AddedBy).HasColumnName("added_by");
            entity.Property(e => e.CurrentStage)
                .HasMaxLength(15)
                .HasDefaultValue("PENDING")
                .HasColumnName("current_stage");
            entity.Property(e => e.ExternalLabId).HasColumnName("external_lab_id");
            entity.Property(e => e.IsOutsourced).HasColumnName("is_outsourced");
            entity.Property(e => e.OutsourceCost).HasColumnName("outsource_cost");
            entity.Property(e => e.OutsourceResultReceivedAt)
                .HasPrecision(0)
                .HasColumnName("outsource_result_received_at");
            entity.Property(e => e.OutsourceSentAt)
                .HasPrecision(0)
                .HasColumnName("outsource_sent_at");
            entity.Property(e => e.OutsourceSentBy).HasColumnName("outsource_sent_by");
            entity.Property(e => e.PriceCharged).HasColumnName("price_charged");
            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");
            entity.Property(e => e.TubeId).HasColumnName("tube_id");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.AddedByNavigation).WithMany(p => p.VisitTestAddedByNavigations)
                .HasForeignKey(d => d.AddedBy)
                .HasConstraintName("FK_VT_AddedBy");

            entity.HasOne(d => d.ExternalLab).WithMany(p => p.VisitTests)
                .HasForeignKey(d => d.ExternalLabId)
                .HasConstraintName("FK_VT_ExtLab");

            entity.HasOne(d => d.OutsourceSentByNavigation).WithMany(p => p.VisitTestOutsourceSentByNavigations)
                .HasForeignKey(d => d.OutsourceSentBy)
                .HasConstraintName("FK_VT_OutsourcedBy");

            entity.HasOne(d => d.Testtype).WithMany(p => p.VisitTests)
                .HasForeignKey(d => d.TesttypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VT_TestType");

            entity.HasOne(d => d.Tube).WithMany(p => p.VisitTests)
                .HasForeignKey(d => d.TubeId)
                .HasConstraintName("FK_VT_Tube");

            entity.HasOne(d => d.Visit).WithMany(p => p.VisitTests)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VT_Visit");
        });
````

### 4. البنية الكاملة لجداول الماليات
- إجماليات الزيارة المالية مخزنة في جدول `Visit`: `subtotal`, `discount_amount`, `discount_percent`, `total_after_discount`, `total_paid`, `balance_due`, `payment_status`.
- المدفوعات التفصيلية مخزنة في جدول مستقل باسم `Payment` مرتبط بـ `visit_id`.
- الرسوم الإضافية مخزنة في `VisitCharge`.
- توجد فواتير تعاقدية للشركات باسم `ContractInvoice` ومدفوعاتها في `ContractPayment`، وليست باسم `Invoice.cs` أو `Bill.cs`.
- الخصم محفوظ كرقم وقيمة نسبية معاً في `Visit`: `DiscountAmount` و`DiscountPercent`.

#### جدول Visit
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.Visit", b =>
                {
                    b.Property<int>("VisitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("VisitId"));

                    b.Property<double>("BalanceDue")
                        .HasColumnType("float")
                        .HasColumnName("balance_due");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int")
                        .HasColumnName("company_id");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("created_at")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<double>("DiscountAmount")
                        .HasColumnType("float")
                        .HasColumnName("discount_amount");

                    b.Property<double>("DiscountPercent")
                        .HasColumnType("float")
                        .HasColumnName("discount_percent");

                    b.Property<DateTime?>("ExpectedReady")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("expected_ready");

                    b.Property<bool>("IsFasting")
                        .HasColumnType("bit")
                        .HasColumnName("is_fasting");

                    b.Property<bool>("IsPregnant")
                        .HasColumnType("bit")
                        .HasColumnName("is_pregnant");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("notes");

                    b.Property<int>("PatientId")
                        .HasColumnType("int")
                        .HasColumnName("patient_id");

                    b.Property<string>("PaymentStatus")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasDefaultValue("PENDING")
                        .HasColumnName("payment_status");

                    b.Property<int?>("ReceptionistId")
                        .HasColumnType("int")
                        .HasColumnName("receptionist_id");

                    b.Property<int?>("ReferralId")
                        .HasColumnType("int")
                        .HasColumnName("referral_id");

                    b.Property<int?>("SchemeId")
                        .HasColumnType("int")
                        .HasColumnName("scheme_id");

                    b.Property<double>("Subtotal")
                        .HasColumnType("float")
                        .HasColumnName("subtotal");

                    b.Property<double>("TotalAfterDiscount")
                        .HasColumnType("float")
                        .HasColumnName("total_after_discount");

                    b.Property<double>("TotalPaid")
                        .HasColumnType("float")
                        .HasColumnName("total_paid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("updated_at");

                    b.Property<string>("VisitCode")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("visit_code");

                    b.Property<DateTime>("VisitDate")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("visit_date")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<string>("VisitStatus")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)")
                        .HasDefaultValue("OPEN")
                        .HasColumnName("visit_status");

                    b.HasKey("VisitId")
                        .HasName("PK__Visit__375A75E1798EC5C4");

                    b.HasIndex("ReceptionistId");

                    b.HasIndex("SchemeId");

                    b.HasIndex(new[] { "CompanyId" }, "IX_Visit_Company");

                    b.HasIndex(new[] { "VisitDate" }, "IX_Visit_Date");

                    b.HasIndex(new[] { "PatientId" }, "IX_Visit_Patient");

                    b.HasIndex(new[] { "ReferralId" }, "IX_Visit_Referral");

                    b.HasIndex(new[] { "VisitStatus", "PaymentStatus" }, "IX_Visit_Status");

                    b.HasIndex(new[] { "VisitCode" }, "UQ__Visit__6B282A41CE8E7529")
                        .IsUnique();

                    b.ToTable("Visit", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visit__375A75E1798EC5C4");

            entity.ToTable("Visit");

            entity.HasIndex(e => e.CompanyId, "IX_Visit_Company");

            entity.HasIndex(e => e.VisitDate, "IX_Visit_Date");

            entity.HasIndex(e => e.PatientId, "IX_Visit_Patient");

            entity.HasIndex(e => e.ReferralId, "IX_Visit_Referral");

            entity.HasIndex(e => new { e.VisitStatus, e.PaymentStatus }, "IX_Visit_Status");

            entity.HasIndex(e => e.VisitCode, "UQ__Visit__6B282A41CE8E7529").IsUnique();

            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.BalanceDue).HasColumnName("balance_due");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.ExpectedReady)
                .HasPrecision(0)
                .HasColumnName("expected_ready");
            entity.Property(e => e.IsFasting).HasColumnName("is_fasting");
            entity.Property(e => e.IsPregnant).HasColumnName("is_pregnant");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(10)
                .HasDefaultValue("PENDING")
                .HasColumnName("payment_status");
            entity.Property(e => e.ReceptionistId).HasColumnName("receptionist_id");
            entity.Property(e => e.ReferralId).HasColumnName("referral_id");
            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.TotalAfterDiscount).HasColumnName("total_after_discount");
            entity.Property(e => e.TotalPaid).HasColumnName("total_paid");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasColumnName("updated_at");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("visit_date");
            entity.Property(e => e.VisitStatus)
                .HasMaxLength(15)
                .HasDefaultValue("OPEN")
                .HasColumnName("visit_status");

            entity.HasOne(d => d.Company).WithMany(p => p.Visits)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Visit_Company");

            entity.HasOne(d => d.Patient).WithMany(p => p.Visits)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visit_Patient");

            entity.HasOne(d => d.Receptionist).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ReceptionistId)
                .HasConstraintName("FK_Visit_Receptionist");

            entity.HasOne(d => d.Referral).WithMany(p => p.Visits)
                .HasForeignKey(d => d.ReferralId)
                .HasConstraintName("FK_Visit_Referral");

            entity.HasOne(d => d.Scheme).WithMany(p => p.Visits)
                .HasForeignKey(d => d.SchemeId)
                .HasConstraintName("FK_Visit_Scheme");
        });
````

#### جدول Payment
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("payment_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PaymentId"));

                    b.Property<double>("Amount")
                        .HasColumnType("float")
                        .HasColumnName("amount");

                    b.Property<string>("Notes")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("notes");

                    b.Property<DateTime>("PaymentDate")
                        .ValueGeneratedOnAdd()
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("payment_date")
                        .HasDefaultValueSql("(sysdatetime())");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)")
                        .HasDefaultValue("CASH")
                        .HasColumnName("payment_method");

                    b.Property<string>("PaymentType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasDefaultValue("PAYMENT")
                        .HasColumnName("payment_type");

                    b.Property<int>("ReceivedBy")
                        .HasColumnType("int")
                        .HasColumnName("received_by");

                    b.Property<string>("ReferenceNumber")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("reference_number");

                    b.Property<int>("VisitId")
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    b.HasKey("PaymentId")
                        .HasName("PK__Payment__ED1FC9EAD5166C32");

                    b.HasIndex("ReceivedBy");

                    b.HasIndex("VisitId");

                    b.ToTable("Payment", null, t =>
                        {
                            t.HasTrigger("TR_Payment_SyncBalance");
                        });

                    b.HasAnnotation("SqlServer:UseSqlOutputClause", false);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__ED1FC9EAD5166C32");

            entity.ToTable("Payment", tb => tb.HasTrigger("TR_Payment_SyncBalance"));

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Notes)
                .HasMaxLength(200)
                .HasColumnName("notes");
            entity.Property(e => e.PaymentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasDefaultValue("CASH")
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentType)
                .HasMaxLength(10)
                .HasDefaultValue("PAYMENT")
                .HasColumnName("payment_type");
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(100)
                .HasColumnName("reference_number");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.Payments)
                .HasForeignKey(d => d.ReceivedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Staff");

            entity.HasOne(d => d.Visit).WithMany(p => p.Payments)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payment_Visit");
        });
````

#### جدول VisitCharge
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.VisitCharge", b =>
                {
                    b.Property<int>("ChargeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("charge_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ChargeId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("amount");

                    b.Property<string>("ChargeDescription")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("charge_description");

                    b.Property<string>("ChargeType")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("charge_type");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("created_at");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<int>("VisitId")
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    b.HasKey("ChargeId")
                        .HasName("PK_VisitCharge");

                    b.HasIndex("CreatedBy");

                    b.HasIndex("VisitId");

                    b.ToTable("VisitCharge", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<VisitCharge>(entity =>
        {
            entity.HasKey(e => e.ChargeId).HasName("PK_VisitCharge");

            entity.ToTable("VisitCharge");

            entity.Property(e => e.ChargeId).HasColumnName("charge_id");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.ChargeDescription).HasColumnName("charge_description");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.ChargeType).HasColumnName("charge_type");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.Visit).WithMany(p => p.VisitCharges)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VisitCharge_Visit");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VisitCharges)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_VisitCharge_Staff");
        });
````

#### جدول ContractInvoice
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.ContractInvoice", b =>
                {
                    b.Property<int>("ContractInvoiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("contract_invoice_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ContractInvoiceId"));

                    b.Property<int>("CompanyId")
                        .HasColumnType("int")
                        .HasColumnName("company_id");

                    b.Property<int?>("CreatedBy")
                        .HasColumnType("int")
                        .HasColumnName("created_by");

                    b.Property<DateTime>("InvoiceDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("invoice_date");

                    b.Property<decimal>("PaidAmount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("paid_amount");

                    b.Property<DateOnly>("PeriodEnd")
                        .HasColumnType("date")
                        .HasColumnName("period_end");

                    b.Property<DateOnly>("PeriodStart")
                        .HasColumnType("date")
                        .HasColumnName("period_start");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("status");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("total_amount");

                    b.HasKey("ContractInvoiceId")
                        .HasName("PK_ContractInvoice");

                    b.HasIndex("CompanyId");

                    b.HasIndex("CreatedBy");

                    b.ToTable("ContractInvoice", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<ContractInvoice>(entity =>
        {
            entity.HasKey(e => e.ContractInvoiceId).HasName("PK_ContractInvoice");

            entity.ToTable("ContractInvoice");

            entity.Property(e => e.ContractInvoiceId).HasColumnName("contract_invoice_id");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.InvoiceDate).HasColumnName("invoice_date");
            entity.Property(e => e.PeriodStart).HasColumnName("period_start");
            entity.Property(e => e.PeriodEnd).HasColumnName("period_end");
            entity.Property(e => e.TotalAmount).HasColumnName("total_amount");
            entity.Property(e => e.PaidAmount).HasColumnName("paid_amount");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");

            entity.HasOne(d => d.Company).WithMany(p => p.ContractInvoices)
                .HasForeignKey(d => d.CompanyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContractInvoice_Company");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ContractInvoices)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ContractInvoice_Staff");
        });
````

#### جدول ContractPayment
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.ContractPayment", b =>
                {
                    b.Property<int>("ContractPaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("contract_payment_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ContractPaymentId"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18,2)")
                        .HasColumnName("amount");

                    b.Property<int>("ContractInvoiceId")
                        .HasColumnType("int")
                        .HasColumnName("contract_invoice_id");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("payment_date");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("payment_method");

                    b.Property<string>("ReferenceNumber")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("reference_number");

                    b.HasKey("ContractPaymentId")
                        .HasName("PK_ContractPayment");

                    b.HasIndex("ContractInvoiceId");

                    b.ToTable("ContractPayment", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<ContractPayment>(entity =>
        {
            entity.HasKey(e => e.ContractPaymentId).HasName("PK_ContractPayment");

            entity.ToTable("ContractPayment");

            entity.Property(e => e.ContractPaymentId).HasColumnName("contract_payment_id");
            entity.Property(e => e.ContractInvoiceId).HasColumnName("contract_invoice_id");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.ReferenceNumber).HasColumnName("reference_number");

            entity.HasOne(d => d.ContractInvoice).WithMany(p => p.ContractPayments)
                .HasForeignKey(d => d.ContractInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContractPayment_ContractInvoice");
        });
````

### 5. البنية الكاملة لجدول الباركود إذا كان موجوداً
- لا يوجد جدول باسم `Barcode` مستقل.
- الباركود مخزن داخل جدول `SampleTube` في العمود `barcode_value` مع فهرس Unique.
- توجد View باسم `V_SampleTubeStatus` ممثلة بالكلاس `VSampleTubeStatus`.

#### جدول SampleTube
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.SampleTube", b =>
                {
                    b.Property<int>("TubeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("tube_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TubeId"));

                    b.Property<string>("BarcodeValue")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("barcode_value");

                    b.Property<DateTime?>("CollectedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("collected_at");

                    b.Property<int?>("CollectedBy")
                        .HasColumnType("int")
                        .HasColumnName("collected_by");

                    b.Property<string>("Notes")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("notes");

                    b.Property<DateTime?>("PrintedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("printed_at");

                    b.Property<int?>("PrintedBy")
                        .HasColumnType("int")
                        .HasColumnName("printed_by");

                    b.Property<string>("TubeColor")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("tube_color");

                    b.Property<string>("TubeType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("tube_type");

                    b.Property<int>("VisitId")
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    b.HasKey("TubeId")
                        .HasName("PK__SampleTu__ABB3DFACE8E18792");

                    b.HasIndex("CollectedBy");

                    b.HasIndex("PrintedBy");

                    b.HasIndex("VisitId");

                    b.HasIndex(new[] { "BarcodeValue" }, "UQ__SampleTu__6932170B90D6E91B")
                        .IsUnique();

                    b.ToTable("SampleTube", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<SampleTube>(entity =>
        {
            entity.HasKey(e => e.TubeId).HasName("PK__SampleTu__ABB3DFACE8E18792");

            entity.ToTable("SampleTube");

            entity.HasIndex(e => e.BarcodeValue, "UQ__SampleTu__6932170B90D6E91B").IsUnique();

            entity.Property(e => e.TubeId).HasColumnName("tube_id");
            entity.Property(e => e.BarcodeValue)
                .HasMaxLength(100)
                .HasColumnName("barcode_value");
            entity.Property(e => e.CollectedAt)
                .HasPrecision(0)
                .HasColumnName("collected_at");
            entity.Property(e => e.CollectedBy).HasColumnName("collected_by");
            entity.Property(e => e.Notes)
                .HasMaxLength(200)
                .HasColumnName("notes");
            entity.Property(e => e.PrintedAt)
                .HasPrecision(0)
                .HasColumnName("printed_at");
            entity.Property(e => e.PrintedBy).HasColumnName("printed_by");
            entity.Property(e => e.TubeColor)
                .HasMaxLength(30)
                .HasColumnName("tube_color");
            entity.Property(e => e.TubeType)
                .HasMaxLength(50)
                .HasColumnName("tube_type");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");

            entity.HasOne(d => d.CollectedByNavigation).WithMany(p => p.SampleTubeCollectedByNavigations)
                .HasForeignKey(d => d.CollectedBy)
                .HasConstraintName("FK_Tube_CollectedBy");

            entity.HasOne(d => d.PrintedByNavigation).WithMany(p => p.SampleTubePrintedByNavigations)
                .HasForeignKey(d => d.PrintedBy)
                .HasConstraintName("FK_Tube_PrintedBy");

            entity.HasOne(d => d.Visit).WithMany(p => p.SampleTubes)
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tube_Visit");
        });
````

#### View V_SampleTubeStatus
بنية الأعمدة/المفاتيح/الفهارس كما تظهر في `FinalLabDbContextModelSnapshot.cs`:
````csharp
modelBuilder.Entity("FinalLabSystem.Models.VSampleTubeStatus", b =>
                {
                    b.Property<string>("BarcodeValue")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("barcode_value");

                    b.Property<DateTime?>("CollectedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("collected_at");

                    b.Property<string>("CollectedByName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("collected_by_name");

                    b.Property<string>("PatientName")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)")
                        .HasColumnName("patient_name");

                    b.Property<DateTime?>("PrintedAt")
                        .HasPrecision(0)
                        .HasColumnType("datetime2(0)")
                        .HasColumnName("printed_at");

                    b.Property<string>("PrintedByName")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("printed_by_name");

                    b.Property<int?>("TestsOnThisTube")
                        .HasColumnType("int")
                        .HasColumnName("tests_on_this_tube");

                    b.Property<string>("TubeColor")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("tube_color");

                    b.Property<int>("TubeId")
                        .HasColumnType("int")
                        .HasColumnName("tube_id");

                    b.Property<string>("TubeType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("tube_type");

                    b.Property<string>("VisitCode")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)")
                        .HasColumnName("visit_code");

                    b.Property<int>("VisitId")
                        .HasColumnType("int")
                        .HasColumnName("visit_id");

                    b.ToTable((string)null);

                    b.ToView("V_SampleTubeStatus", (string)null);
                });
````
تكوين EF Fluent API كما يظهر في `FinalLabDbContext.cs`:
````csharp
modelBuilder.Entity<VSampleTubeStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_SampleTubeStatus");

            entity.Property(e => e.BarcodeValue)
                .HasMaxLength(100)
                .HasColumnName("barcode_value");
            entity.Property(e => e.CollectedAt)
                .HasPrecision(0)
                .HasColumnName("collected_at");
            entity.Property(e => e.CollectedByName)
                .HasMaxLength(100)
                .HasColumnName("collected_by_name");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.PrintedAt)
                .HasPrecision(0)
                .HasColumnName("printed_at");
            entity.Property(e => e.PrintedByName)
                .HasMaxLength(100)
                .HasColumnName("printed_by_name");
            entity.Property(e => e.TestsOnThisTube).HasColumnName("tests_on_this_tube");
            entity.Property(e => e.TubeColor)
                .HasMaxLength(30)
                .HasColumnName("tube_color");
            entity.Property(e => e.TubeId).HasColumnName("tube_id");
            entity.Property(e => e.TubeType)
                .HasMaxLength(50)
                .HasColumnName("tube_type");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
        });
````

---
## الجزء الثاني: طبقة النماذج (Model Layer)

### 6. الكود الكامل للكلاسات المطلوبة إذا كانت موجودة
### Patient.cs
المسار: `Models/Patient.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public string PatientCode { get; set; } = null!;

    public string? NationalId { get; set; }

    public string? Title { get; set; }

    public string FullNameAr { get; set; } = null!;

    public string? FullNameEn { get; set; }

    public string Sex { get; set; } = null!;

    public DateOnly? DateOfBirth { get; set; }

    public double? ApproxAge { get; set; }

    public string? ApproxAgeUnit { get; set; }

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? BloodType { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    public virtual ICollection<PatientMedicalHistory> PatientMedicalHistories { get; set; } = new List<PatientMedicalHistory>();
}
````

### ReferralSource.cs
المسار: `Models/ReferralSource.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class ReferralSource
{
    public int ReferralId { get; set; }

    public string SourceType { get; set; } = null!;

    public string? Title { get; set; }

    public string SourceName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Phone2 { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public double CommissionRate { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? SchemeId { get; set; }

    // Navigation properties
    public virtual PriceScheme? Scheme { get; set; }

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
````

### Doctor.cs
[غير موجود حالياً] Models/Doctor.cs

### Test.cs
[غير موجود حالياً] Models/Test.cs

### TestItem.cs
[غير موجود حالياً] Models/TestItem.cs

### TestType.cs - المقابل الحالي للتحاليل الفردية
المسار: `Models/TestType.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestType
{
    public int TesttypeId { get; set; }

    public int GroupId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeNameEn { get; set; } = null!;

    public string? TypeNameAr { get; set; }

    public string? TypeAbbrev { get; set; }

    public double DefaultPrice { get; set; }

    public string? SampleType { get; set; }

    public string? DefaultTubeType { get; set; }

    public string? DefaultTubeColor { get; set; }

    public short TurnaroundHours { get; set; }

    public bool IsOutsourceable { get; set; }

    public string? SpecialType { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public string? Notes { get; set; }

    public virtual TestGroup Group { get; set; } = null!;

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestComponent> TestComponents { get; set; } = new List<TestComponent>();

    public virtual ICollection<TestTypePrice> TestTypePrices { get; set; } = new List<TestTypePrice>();

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<TestProfileItem> TestProfileItems { get; set; } = new List<TestProfileItem>();
}
````

### TestComponent.cs - مكونات التحليل
المسار: `Models/TestComponent.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestComponent
{
    public int ComponentId { get; set; }

    public int TesttypeId { get; set; }

    public string ComponentCode { get; set; } = null!;

    public string ComponentNameEn { get; set; } = null!;

    public string? ComponentNameAr { get; set; }

    public string? Unit { get; set; }

    public string ResultType { get; set; } = null!;

    public byte DecimalPlaces { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<NormalRange> NormalRanges { get; set; } = new List<NormalRange>();

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual TestType Testtype { get; set; } = null!;
}
````

### Panel.cs
[غير موجود حالياً] Models/Panel.cs

### TestPackage.cs
[غير موجود حالياً] Models/TestPackage.cs

### TestProfile.cs - المقابل الحالي للباقات
المسار: `Models/TestProfile.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Test Profile/Package - Groups of predefined tests that can be added to a visit with a single click
/// V4.0 New Table
/// </summary>
public partial class TestProfile
{
    public int ProfileId { get; set; }

    public string ProfileNameAr { get; set; } = null!;

    public string? ProfileNameEn { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<TestProfileItem> TestProfileItems { get; set; } = new List<TestProfileItem>();
}
````

### TestProfileItem.cs - عناصر الباقات
المسار: `Models/TestProfileItem.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Test Profile Item - Maps individual tests to test profiles/packages
/// V4.0 New Table
/// </summary>
public partial class TestProfileItem
{
    public int ProfileItemId { get; set; }

    public int ProfileId { get; set; }

    public int TestTypeId { get; set; }

    public int? SortOrder { get; set; }

    // Navigation properties
    public virtual TestProfile Profile { get; set; } = null!;

    public virtual TestType TestType { get; set; } = null!;
}
````

### TestCategory.cs
المسار: `Models/TestCategory.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestCategory
{
    public int CategoryId { get; set; }

    public string CategoryCode { get; set; } = null!;

    public string CategoryNameEn { get; set; } = null!;

    public string? CategoryNameAr { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplates { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<TestGroup> TestGroups { get; set; } = new List<TestGroup>();
}
````

### TestGroup.cs
المسار: `Models/TestGroup.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class TestGroup
{
    public int GroupId { get; set; }

    public int CategoryId { get; set; }

    public string GroupCode { get; set; } = null!;

    public string GroupNameEn { get; set; } = null!;

    public string? GroupNameAr { get; set; }

    public short SortOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual TestCategory Category { get; set; } = null!;

    public virtual ICollection<TestType> TestTypes { get; set; } = new List<TestType>();
}
````

### PatientTest.cs
[غير موجود حالياً] Models/PatientTest.cs

### OrderItem.cs
[غير موجود حالياً] Models/OrderItem.cs

### VisitTest.cs - المقابل الحالي لربط الزيارة بالتحاليل
المسار: `Models/VisitTest.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class VisitTest
{
    public int VisitTestId { get; set; }

    public int VisitId { get; set; }

    public int TesttypeId { get; set; }

    public int? TubeId { get; set; }

    public double PriceCharged { get; set; }

    public string CurrentStage { get; set; } = null!;

    public bool IsOutsourced { get; set; }

    public int? ExternalLabId { get; set; }

    public double? OutsourceCost { get; set; }

    public DateTime? OutsourceSentAt { get; set; }

    public int? OutsourceSentBy { get; set; }

    public DateTime? OutsourceResultReceivedAt { get; set; }

    public DateTime AddedAt { get; set; }

    public int? AddedBy { get; set; }

    public virtual Staff? AddedByNavigation { get; set; }

    public virtual CrossMatchTest? CrossMatchTest { get; set; }

    public virtual ExternalLab? ExternalLab { get; set; }

    public virtual MicrobiologyCulture? MicrobiologyCulture { get; set; }

    public virtual Staff? OutsourceSentByNavigation { get; set; }

    public virtual SemenAnalysis? SemenAnalysis { get; set; }

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();

    public virtual ICollection<TestWorkflow> TestWorkflows { get; set; } = new List<TestWorkflow>();

    public virtual TestType Testtype { get; set; } = null!;

    public virtual SampleTube? Tube { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual ICollection<ExternalShipmentItem> ExternalShipmentItems { get; set; } = new List<ExternalShipmentItem>();
}
````

### Invoice.cs
[غير موجود حالياً] Models/Invoice.cs

### Bill.cs
[غير موجود حالياً] Models/Bill.cs

### Visit.cs - يحتوي إجماليات الفاتورة للزيارة
المسار: `Models/Visit.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Visit
{
    public int VisitId { get; set; }

    public string VisitCode { get; set; } = null!;

    public int PatientId { get; set; }

    public DateTime VisitDate { get; set; }

    public DateTime? ExpectedReady { get; set; }

    public bool IsPregnant { get; set; }

    public bool IsFasting { get; set; }

    public int? ReferralId { get; set; }

    public int? CompanyId { get; set; }

    public int? SchemeId { get; set; }

    public double Subtotal { get; set; }

    public double DiscountAmount { get; set; }

    public double DiscountPercent { get; set; }

    public double TotalAfterDiscount { get; set; }

    public double TotalPaid { get; set; }

    public double BalanceDue { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public string VisitStatus { get; set; } = null!;

    public int? ReceptionistId { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Company? Company { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Staff? Receptionist { get; set; }

    public virtual ReferralSource? Referral { get; set; }

    public virtual ICollection<SampleTube> SampleTubes { get; set; } = new List<SampleTube>();

    public virtual PriceScheme? Scheme { get; set; }

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();

    public virtual ICollection<VisitCharge> VisitCharges { get; set; } = new List<VisitCharge>();
}
````

### Payment.cs - المدفوعات
المسار: `Models/Payment.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int VisitId { get; set; }

    public DateTime PaymentDate { get; set; }

    public double Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentType { get; set; } = null!;

    public int ReceivedBy { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public virtual Staff ReceivedByNavigation { get; set; } = null!;

    public virtual Visit Visit { get; set; } = null!;
}
````

### VisitCharge.cs - الرسوم الإضافية
المسار: `Models/VisitCharge.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Visit Charge - Additional charges beyond tests (e.g., home collection, rush fee)
/// V4.0 New Table
/// </summary>
public partial class VisitCharge
{
    public int ChargeId { get; set; }

    public int VisitId { get; set; }

    public string ChargeDescription { get; set; } = null!;

    public decimal Amount { get; set; }

    public string? ChargeType { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Visit Visit { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }
}
````

### ContractInvoice.cs - فواتير الشركات
المسار: `Models/ContractInvoice.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Contract Invoice - Monthly aggregated invoices to corporate clients
/// V4.0 New Table
/// </summary>
public partial class ContractInvoice
{
    public int ContractInvoiceId { get; set; }

    public int CompanyId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateOnly PeriodStart { get; set; }

    public DateOnly PeriodEnd { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; } = 0;

    public string Status { get; set; } = "DRAFT"; // DRAFT, ISSUED, PAID

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Company Company { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }

    public virtual ICollection<ContractPayment> ContractPayments { get; set; } = new List<ContractPayment>();
}
````

### ContractPayment.cs - مدفوعات فواتير الشركات
المسار: `Models/ContractPayment.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Contract Payment - Payment records for contract invoices
/// V4.0 New Table
/// </summary>
public partial class ContractPayment
{
    public int ContractPaymentId { get; set; }

    public int ContractInvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? ReferenceNumber { get; set; }

    // Navigation properties
    public virtual ContractInvoice ContractInvoice { get; set; } = null!;
}
````

### PatientMedicalHistory.cs
المسار: `Models/PatientMedicalHistory.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

/// <summary>
/// Patient Medical History - Structured medical history record (diseases, allergies, medications, surgeries)
/// V4.0 New Table
/// </summary>
public partial class PatientMedicalHistory
{
    public int MedicalHistoryId { get; set; }

    public int PatientId { get; set; }

    public string HistoryType { get; set; } = null!; // DISEASE, MEDICATION, ALLERGY, SURGERY, OTHER

    public string Description { get; set; } = null!;

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    // Navigation properties
    public virtual Patient Patient { get; set; } = null!;

    public virtual Staff? CreatedByNavigation { get; set; }
}
````

### SampleTube.cs - الباركود
المسار: `Models/SampleTube.cs`
````csharp
using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class SampleTube
{
    public int TubeId { get; set; }

    public int VisitId { get; set; }

    public string TubeType { get; set; } = null!;

    public string? TubeColor { get; set; }

    public string BarcodeValue { get; set; } = null!;

    public DateTime? CollectedAt { get; set; }

    public int? CollectedBy { get; set; }

    public DateTime? PrintedAt { get; set; }

    public int? PrintedBy { get; set; }

    public string? Notes { get; set; }

    public virtual Staff? CollectedByNavigation { get; set; }

    public virtual Staff? PrintedByNavigation { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual ICollection<VisitTest> VisitTests { get; set; } = new List<VisitTest>();
}
````

---
## الجزء الثالث: طبقة الخدمات (Service Layer)

### 7. قائمة بجميع ملفات الـ Services الموجودة حالياً مع أسماء الدوال
#### AndrologyService.cs
المسار: `Services/Implementations/AndrologyService.cs`
الدوال:
- `public AndrologyService(FinalLabDbContext context)`
- `public async Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId)`

#### AttendanceService.cs
المسار: `Services/Implementations/AttendanceService.cs`
الدوال:
- `public AttendanceService(FinalLabDbContext context)`
- `public async Task<List<WorkShift>> GetActiveShiftsAsync()`
- `public async Task RecordClockInAsync(int staffId, int shiftId)`
- `public async Task RecordClockOutAsync(int staffId)`

#### AuditService.cs
المسار: `Services/Implementations/AuditService.cs`
الدوال:
- `public AuditService(FinalLabDbContext context)`
- `public async Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId)`
- `public async Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId)`

#### AuthService.cs
المسار: `Services/Implementations/AuthService.cs`
الدوال:
- `public AuthService(FinalLabDbContext context)`
- `public async Task<Staff?> LoginAsync(string username, string password)`
- `public async Task<bool> HasPermissionAsync(int staffId, string permissionCode)`
- `public async Task UpdateLastLoginAsync(int staffId)`
- `public async Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds)`
- `public Task<bool> HasAnyAdministratorAsync()`
- `public async Task<Staff> CreateInitialAdministratorAsync(string username, string password, string? displayName = null)`
- `throw new ArgumentException("Username is required.", nameof(username));`
- `throw new ArgumentException("Password is required.", nameof(password));`
- `throw new InvalidOperationException("An administrator account already exists.");`

#### BloodBankService.cs
المسار: `Services/Implementations/BloodBankService.cs`
الدوال:
- `public BloodBankService(FinalLabDbContext context)`
- `public async Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId)`
- `public async Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId)`

#### ContractService.cs
المسار: `Services/Implementations/ContractService.cs`
الدوال:
- `public ContractService(FinalLabDbContext context)`
- `public async Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId)`
- `public async Task RecordCorporatePaymentAsync(ContractPayment payment)`

#### CultureResultService.cs
المسار: `Services/Implementations/CultureResultService.cs`
الدوال:
- `public CultureResultService(FinalLabDbContext context)`
- `public async Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild)`
- `public async Task SaveCultureAsync(MicrobiologyCulture culture)`
- `public async Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms)`

#### ExternalLabService.cs
المسار: `Services/Implementations/ExternalLabService.cs`
الدوال:
- `public ExternalLabService(FinalLabDbContext context)`
- `public async Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId)`
- `public async Task UpdateShipmentStatusAsync(int shipmentId, string status)`

#### FinancialService.cs
المسار: `Services/Implementations/FinancialService.cs`
الدوال:
- `public FinancialService(FinalLabDbContext context)`
- `public async Task RecordPatientPaymentAsync(Payment payment)`
- `public async Task ApplyDiscountAsync(int visitId, double discount, int staffId)`

#### PatientService.cs
المسار: `Services/Implementations/PatientService.cs`
الدوال:
- `public PatientService(FinalLabDbContext context)`
- `public async Task<Patient> RegisterPatientAsync(Patient patient)`
- `public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)`
- `public async Task AddMedicalHistoryAsync(PatientMedicalHistory history)`
- `public async Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId)`

#### PricingService.cs
المسار: `Services/Implementations/PricingService.cs`
الدوال:
- `public PricingService(FinalLabDbContext context)`
- `public async Task<List<PriceScheme>> GetAllSchemesAsync()`
- `public async Task<double> GetTestPriceAsync(int testTypeId, int schemeId)`
- `public async Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices)`

#### ReferralService.cs
المسار: `Services/Implementations/ReferralService.cs`
الدوال:
- `public ReferralService(FinalLabDbContext context)`
- `public async Task<ReferralSource> AddReferralSourceAsync(ReferralSource source)`
- `public async Task LinkReferralToSchemeAsync(int referralId, int schemeId)`

#### ReportingService.cs
المسار: `Services/Implementations/ReportingService.cs`
الدوال:
- `public ReportingService(FinalLabDbContext context)`
- `public async Task<List<VPendingTest>> GetPendingWorksheetsAsync()`
- `public async Task<List<VOutstandingBalance>> GetDefaultersListAsync()`
- `public async Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end)`
- `public async Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId)`
- `public async Task<object> GetDashboardMetricsAsync(DateTime date)`

#### RoutineResultService.cs
المسار: `Services/Implementations/RoutineResultService.cs`
الدوال:
- `public RoutineResultService(FinalLabDbContext context)`
- `public async Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId)`
- `else if (matchingRange.LowCritical.HasValue && value < matchingRange.LowCritical.Value)`
- `else if (matchingRange.HighNormal.HasValue && value > matchingRange.HighNormal.Value)`
- `else if (matchingRange.LowNormal.HasValue && value < matchingRange.LowNormal.Value)`
- `public async Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId)`

#### SampleTrackingService.cs
المسار: `Services/Implementations/SampleTrackingService.cs`
الدوال:
- `public SampleTrackingService(FinalLabDbContext context)`
- `public async Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId)`
- `public async Task UpdateTestStageAsync(int visitTestId, string newStage, int staffId)`

#### SettingsService.cs
المسار: `Services/Implementations/SettingsService.cs`
الدوال:
- `public SettingsService(FinalLabDbContext context)`
- `public async Task<string?> GetSettingValueAsync(string key)`
- `public async Task UpsertSettingAsync(LabSetting setting, int staffId)`
- `public async Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName)`

#### TestCatalogService.cs
المسار: `Services/Implementations/TestCatalogService.cs`
الدوال:
- `public TestCatalogService(FinalLabDbContext context)`
- `public async Task<List<TestCategory>> GetFullHierarchyAsync()`
- `public async Task<TestType?> GetTestTypeDetailsAsync(int testTypeId)`
- `public async Task<List<TestProfile>> GetActiveProfilesAsync()`
- `public async Task<List<TestType>> GetProfileTestsAsync(int profileId)`

#### VisitService.cs
المسار: `Services/Implementations/VisitService.cs`
الدوال:
- `public VisitService(FinalLabDbContext context)`
- `public async Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges)`
- `public async Task CancelVisitTestAsync(int visitTestId)`
- `public async Task<Visit?> GetVisitSummaryAsync(int visitId)`

#### IAndrologyService.cs
المسار: `Services/Interfaces/IAndrologyService.cs`
الدوال:
- `Task SaveSemenAnalysisAsync(SemenAnalysis analysis, int staffId);`

#### IAttendanceService.cs
المسار: `Services/Interfaces/IAttendanceService.cs`
الدوال:
- `Task<List<WorkShift>> GetActiveShiftsAsync();`
- `Task RecordClockInAsync(int staffId, int shiftId);`
- `Task RecordClockOutAsync(int staffId);`

#### IAuditService.cs
المسار: `Services/Interfaces/IAuditService.cs`
الدوال:
- `Task<List<VResultAuditTrail>> GetResultModificationsAsync(int visitTestId);`
- `Task<List<AuditLog>> GetTableAuditHistoryAsync(string tableName, int recordId);`

#### IAuthService.cs
المسار: `Services/Interfaces/IAuthService.cs`
الدوال:
- `Task<Staff?> LoginAsync(string username, string password);`
- `Task<bool> HasPermissionAsync(int staffId, string permissionCode);`
- `Task UpdateLastLoginAsync(int staffId);`
- `Task<Staff> CreateUserAsync(Staff staff, List<int> permissionIds);`
- `Task<bool> HasAnyAdministratorAsync();`
- `Task<Staff> CreateInitialAdministratorAsync(string username, string password, string? displayName = null);`

#### IBloodBankService.cs
المسار: `Services/Interfaces/IBloodBankService.cs`
الدوال:
- `Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId);`
- `Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId);`

#### IContractService.cs
المسار: `Services/Interfaces/IContractService.cs`
الدوال:
- `Task<ContractInvoice> GenerateMonthlyCorporateInvoiceAsync(int companyId, DateTime start, DateTime end, int staffId);`
- `Task RecordCorporatePaymentAsync(ContractPayment payment);`

#### ICultureResultService.cs
المسار: `Services/Interfaces/ICultureResultService.cs`
الدوال:
- `Task<List<AntibioticCatalog>> GetSafeAntibioticsAsync(bool isPregnant, bool isChild);`
- `Task SaveCultureAsync(MicrobiologyCulture culture);`
- `Task AddOrganismsAndSensitivitiesAsync(int cultureId, List<MicrobiologyOrganism> organisms);`

#### IExternalLabService.cs
المسار: `Services/Interfaces/IExternalLabService.cs`
الدوال:
- `Task<ExternalShipment> CreateOutsourceManifestAsync(int externalLabId, List<int> visitTestIds, int staffId);`
- `Task UpdateShipmentStatusAsync(int shipmentId, string status);`

#### IFinancialService.cs
المسار: `Services/Interfaces/IFinancialService.cs`
الدوال:
- `Task RecordPatientPaymentAsync(Payment payment);`
- `Task ApplyDiscountAsync(int visitId, double discount, int staffId);`

#### IPatientService.cs
المسار: `Services/Interfaces/IPatientService.cs`
الدوال:
- `Task<Patient> RegisterPatientAsync(Patient patient);`
- `Task<List<Patient>> SearchPatientsAsync(string searchTerm);`
- `Task AddMedicalHistoryAsync(PatientMedicalHistory history);`
- `Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);`

#### IPricingService.cs
المسار: `Services/Interfaces/IPricingService.cs`
الدوال:
- `Task<List<PriceScheme>> GetAllSchemesAsync();`
- `Task<double> GetTestPriceAsync(int testTypeId, int schemeId);`
- `Task UpdateSchemePricesAsync(int schemeId, List<TestTypePrice> prices);`

#### IReferralService.cs
المسار: `Services/Interfaces/IReferralService.cs`
الدوال:
- `Task<ReferralSource> AddReferralSourceAsync(ReferralSource source);`
- `Task LinkReferralToSchemeAsync(int referralId, int schemeId);`

#### IReportingService.cs
المسار: `Services/Interfaces/IReportingService.cs`
الدوال:
- `Task<List<VPendingTest>> GetPendingWorksheetsAsync();`
- `Task<List<VOutstandingBalance>> GetDefaultersListAsync();`
- `Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end);`
- `Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId);`
- `Task<object> GetDashboardMetricsAsync(DateTime date);`

#### IRoutineResultService.cs
المسار: `Services/Interfaces/IRoutineResultService.cs`
الدوال:
- `Task SaveNumericOrTextResultsAsync(List<TestResult> results, int patientId, int staffId);`
- `Task<List<TestResult>> GetResultsByVisitTestAsync(int visitTestId);`

#### ISampleTrackingService.cs
المسار: `Services/Interfaces/ISampleTrackingService.cs`
الدوال:
- `Task<List<SampleTube>> GenerateBarcodesForVisitAsync(int visitId, int staffId);`
- `Task UpdateTestStageAsync(int visitTestId, string newStage, int staffId);`

#### ISettingsService.cs
المسار: `Services/Interfaces/ISettingsService.cs`
الدوال:
- `Task<string?> GetSettingValueAsync(string key);`
- `Task UpsertSettingAsync(LabSetting setting, int staffId);`
- `Task<Dictionary<string, string>> GetSettingsByGroupAsync(string groupName);`

#### ITestCatalogService.cs
المسار: `Services/Interfaces/ITestCatalogService.cs`
الدوال:
- `Task<List<TestCategory>> GetFullHierarchyAsync();`
- `Task<TestType?> GetTestTypeDetailsAsync(int testTypeId);`
- `Task<List<TestProfile>> GetActiveProfilesAsync();`
- `Task<List<TestType>> GetProfileTestsAsync(int profileId);`

#### IVisitService.cs
المسار: `Services/Interfaces/IVisitService.cs`
الدوال:
- `Task<Visit> CreateVisitAsync(Visit visit, List<int> testIds, List<int> profileIds, List<VisitCharge> charges);`
- `Task CancelVisitTestAsync(int visitTestId);`
- `Task<Visit?> GetVisitSummaryAsync(int visitId);`

### 8. الكود الكامل للـ Services المطلوبة إذا كانت موجودة
### IPatientService.cs
المسار: `Services/Interfaces/IPatientService.cs`
````csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IPatientService
{
    Task<Patient> RegisterPatientAsync(Patient patient);
    Task<List<Patient>> SearchPatientsAsync(string searchTerm);
    Task AddMedicalHistoryAsync(PatientMedicalHistory history);
    Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId);
}
````

### PatientService.cs
المسار: `Services/Implementations/PatientService.cs`
````csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly FinalLabDbContext _context;

    public PatientService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<Patient> RegisterPatientAsync(Patient patient)
    {
        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();
        return patient;
    }

    public async Task<List<Patient>> SearchPatientsAsync(string searchTerm)
    {
        return await _context.Patients
            .Where(p => p.FullNameAr.Contains(searchTerm)
                || (p.FullNameEn != null && p.FullNameEn.Contains(searchTerm))
                || p.PatientCode.Contains(searchTerm)
                || (p.Phone != null && p.Phone.Contains(searchTerm)))
            .ToListAsync();
    }

    public async Task AddMedicalHistoryAsync(PatientMedicalHistory history)
    {
        _context.PatientMedicalHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    public async Task<List<PatientMedicalHistory>> GetActiveHistoryAsync(int patientId)
    {
        return await _context.PatientMedicalHistories
            .Where(h => h.PatientId == patientId && h.IsActive)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync();
    }
}
````

### ITestService.cs
[غير موجود حالياً] Services/Interfaces/ITestService.cs

### TestService.cs
[غير موجود حالياً] Services/Implementations/TestService.cs

### ITestCatalogService.cs - المقابل الموجود
المسار: `Services/Interfaces/ITestCatalogService.cs`
````csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ITestCatalogService
{
    Task<List<TestCategory>> GetFullHierarchyAsync();
    Task<TestType?> GetTestTypeDetailsAsync(int testTypeId);
    Task<List<TestProfile>> GetActiveProfilesAsync();
    Task<List<TestType>> GetProfileTestsAsync(int profileId);
}
````

### TestCatalogService.cs - المقابل الموجود
المسار: `Services/Implementations/TestCatalogService.cs`
````csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class TestCatalogService : ITestCatalogService
{
    private readonly FinalLabDbContext _context;

    public TestCatalogService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<TestCategory>> GetFullHierarchyAsync()
    {
        return await _context.TestCategories
            .Include(c => c.TestGroups)
                .ThenInclude(g => g.TestTypes)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<TestType?> GetTestTypeDetailsAsync(int testTypeId)
    {
        return await _context.TestTypes
            .Include(tt => tt.TestComponents)
                .ThenInclude(tc => tc.NormalRanges)
            .FirstOrDefaultAsync(tt => tt.TesttypeId == testTypeId);
    }

    public async Task<List<TestProfile>> GetActiveProfilesAsync()
    {
        return await _context.TestProfiles
            .Include(p => p.TestProfileItems)
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<List<TestType>> GetProfileTestsAsync(int profileId)
    {
        var profile = await _context.TestProfiles
            .Include(p => p.TestProfileItems)
                .ThenInclude(tpi => tpi.TestType)
            .FirstOrDefaultAsync(p => p.ProfileId == profileId);

        return profile?.TestProfileItems
            .Select(tpi => tpi.TestType)
            .ToList() ?? new List<TestType>();
    }
}
````

### IReferralService.cs
المسار: `Services/Interfaces/IReferralService.cs`
````csharp
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReferralService
{
    Task<ReferralSource> AddReferralSourceAsync(ReferralSource source);
    Task LinkReferralToSchemeAsync(int referralId, int schemeId);
}
````

### ReferralService.cs
المسار: `Services/Implementations/ReferralService.cs`
````csharp
using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class ReferralService : IReferralService
{
    private readonly FinalLabDbContext _context;

    public ReferralService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<ReferralSource> AddReferralSourceAsync(ReferralSource source)
    {
        _context.ReferralSources.Add(source);
        await _context.SaveChangesAsync();
        return source;
    }

    public async Task LinkReferralToSchemeAsync(int referralId, int schemeId)
    {
        var referral = await _context.ReferralSources.FindAsync(referralId);
        if (referral == null)
            throw new InvalidOperationException($"ReferralSource with Id {referralId} not found.");

        var scheme = await _context.PriceSchemes.FindAsync(schemeId);
        if (scheme == null)
            throw new InvalidOperationException($"PriceScheme with Id {schemeId} not found.");

        referral.SchemeId = schemeId;
        await _context.SaveChangesAsync();
    }
}
````

### IFinancialService.cs
المسار: `Services/Interfaces/IFinancialService.cs`
````csharp
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IFinancialService
{
    Task RecordPatientPaymentAsync(Payment payment);
    Task ApplyDiscountAsync(int visitId, double discount, int staffId);
}
````

### FinancialService.cs
المسار: `Services/Implementations/FinancialService.cs`
````csharp
using System;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class FinancialService : IFinancialService
{
    private readonly FinalLabDbContext _context;

    public FinancialService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task RecordPatientPaymentAsync(Payment payment)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            payment.PaymentDate = DateTime.UtcNow;
            payment.PaymentType = "PAYMENT";

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var visit = await _context.Visits.FindAsync(payment.VisitId);
            if (visit != null)
                await _context.Entry(visit).ReloadAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ApplyDiscountAsync(int visitId, double discount, int staffId)
    {
        var visit = await _context.Visits.FindAsync(visitId);
        if (visit == null)
            throw new InvalidOperationException($"Visit with ID {visitId} not found.");

        visit.DiscountPercent = discount;
        visit.DiscountAmount = visit.Subtotal * discount / 100.0;
        visit.TotalAfterDiscount = visit.Subtotal - visit.DiscountAmount;
        visit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}
````

---
## الجزء الرابع: البنية المعمارية الحالية للتطبيق

### 9. الكود الكامل للملفات المطلوبة
### App.xaml.cs
المسار: `App.xaml.cs`
````csharp
using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FinalLabSystem;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var navigation = ServiceProvider.GetRequiredService<INavigationService>();

        bool hasAdmin;
        using (var scope = ServiceProvider.CreateScope())
        {
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            hasAdmin = await auth.HasAnyAdministratorAsync();
        }

        if (hasAdmin)
            navigation.ShowLogin();
        else
            navigation.ShowFirstRunSetup();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=FinalLab;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IUserSettingsService, JsonUserSettingsService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<LoginWindow>();

        services.AddTransient<FirstRunSetupViewModel>();
        services.AddTransient<FirstRunSetupView>();
        services.AddTransient<FirstRunSetupWindow>();

        services.AddTransient<MainWindow>();
    }
}
````

### MainWindow.xaml
المسار: `MainWindow.xaml`
````xml
<Window x:Class="FinalLabSystem.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:FinalLabSystem"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Grid>

    </Grid>
</Window>
````

### MainWindow.xaml.cs
المسار: `MainWindow.xaml.cs`
````csharp
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalLabSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
````

### MainViewModel.cs
[غير موجود حالياً] ViewModels/MainViewModel.cs

### 10. Base Class للـ ViewModels
يوجد Base Class باسم `ViewModelBase`.
### ViewModelBase.cs
المسار: `Infrastructure/ViewModelBase.cs`
````csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FinalLabSystem.Infrastructure;

public class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
````

### 11. نظام Navigation الحالي وكيف يعمل
يوجد نظام Navigation عبر خدمة `INavigationService`/`NavigationService`. الكود التالي هو المصدر الفعلي لطريقة العمل:
### INavigationService.cs
المسار: `Infrastructure/Navigation/INavigationService.cs`
````csharp
namespace FinalLabSystem.Infrastructure.Navigation;

public interface INavigationService
{
    void ShowLogin();

    void ShowFirstRunSetup();

    void ShowMain();

    void OpenTaskWindow<TViewModel>() where TViewModel : class;

    void ReturnToMain();

    void Shutdown();
}
````

### NavigationService.cs
المسار: `Infrastructure/Navigation/NavigationService.cs`
````csharp
using System.Collections.Generic;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace FinalLabSystem.Infrastructure.Navigation;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _viewModelToWindowMap = new();

    private Window? _bootstrapWindow;
    private Window? _mainWindow;
    private Window? _activeTaskWindow;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void RegisterWindow<TViewModel, TWindow>()
        where TViewModel : class
        where TWindow : Window
    {
        _viewModelToWindowMap[typeof(TViewModel)] = typeof(TWindow);
    }

    public void ShowLogin()
    {
        SwapBootstrapWindow(_serviceProvider.GetRequiredService<Views.LoginWindow>());
    }

    public void ShowFirstRunSetup()
    {
        SwapBootstrapWindow(_serviceProvider.GetRequiredService<Views.FirstRunSetupWindow>());
    }

    public void ShowMain()
    {
        _mainWindow ??= _serviceProvider.GetRequiredService<MainWindow>();

        Application.Current.MainWindow = _mainWindow;
        _mainWindow.Show();
        _mainWindow.Activate();

        if (_bootstrapWindow is not null)
        {
            _bootstrapWindow.Close();
            _bootstrapWindow = null;
        }
    }

    public void OpenTaskWindow<TViewModel>() where TViewModel : class
    {
        if (!_viewModelToWindowMap.TryGetValue(typeof(TViewModel), out var windowType))
            throw new InvalidOperationException(
                $"No window is registered for ViewModel '{typeof(TViewModel).Name}'.");

        if (_serviceProvider.GetService(windowType) is not Window window)
            throw new InvalidOperationException(
                $"Window '{windowType.Name}' is not registered in the DI container.");

        _mainWindow?.Hide();
        _activeTaskWindow = window;
        window.Closed += OnTaskWindowClosed;
        window.Show();
        window.Activate();
    }

    public void ReturnToMain()
    {
        if (_activeTaskWindow is not null)
        {
            _activeTaskWindow.Closed -= OnTaskWindowClosed;
            _activeTaskWindow.Close();
            _activeTaskWindow = null;
        }

        if (_mainWindow is not null)
        {
            _mainWindow.Show();
            _mainWindow.Activate();
        }
    }

    public void Shutdown() => Application.Current.Shutdown();

    private void SwapBootstrapWindow(Window window)
    {
        var previous = _bootstrapWindow;
        _bootstrapWindow = window;
        Application.Current.MainWindow = window;
        window.Show();
        previous?.Close();
    }

    private void OnTaskWindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window closed)
            closed.Closed -= OnTaskWindowClosed;

        _activeTaskWindow = null;
        _mainWindow?.Show();
        _mainWindow?.Activate();
    }
}
````

### 12. RelayCommand أو DelegateCommand
يوجد `RelayCommand`.
يوجد أيضاً `AsyncRelayCommand`.
### RelayCommand.cs
المسار: `Infrastructure/RelayCommand.cs`
````csharp
using System.Windows.Input;

namespace FinalLabSystem.Infrastructure;

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecute?.Invoke(parameter) ?? true;
    }

    public void Execute(object? parameter)
    {
        _execute(parameter);
    }
}
````

### AsyncRelayCommand.cs
المسار: `Infrastructure/AsyncRelayCommand.cs`
````csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FinalLabSystem.Infrastructure;

public class AsyncRelayCommand : ICommand, INotifyPropertyChanged
{
    private readonly Func<object?, Task> _execute;
    private readonly Func<object?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute is null ? null : new Func<object?, bool>(_ => canExecute()))
    {
    }

    public AsyncRelayCommand(Func<object?, Task> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting != value)
            {
                _isExecuting = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanExecute(object? parameter)
        => !IsExecuting && (_canExecute?.Invoke(parameter) ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        IsExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(parameter);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        finally
        {
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class AsyncRelayCommand<T> : ICommand, INotifyPropertyChanged
{
    private readonly Func<T?, Task> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<T?, Task> execute, Func<T?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<Exception>? ErrorOccurred;

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool IsExecuting
    {
        get => _isExecuting;
        private set
        {
            if (_isExecuting != value)
            {
                _isExecuting = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanExecute(object? parameter)
        => !IsExecuting && (_canExecute?.Invoke(Cast(parameter)) ?? true);

    public async void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
            return;

        IsExecuting = true;
        RaiseCanExecuteChanged();

        try
        {
            await _execute(Cast(parameter));
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, ex);
        }
        finally
        {
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

    private static T? Cast(object? parameter)
        => parameter is T typed ? typed : default;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
````

### DelegateCommand.cs
[غير موجود حالياً] Infrastructure/DelegateCommand.cs

### 13. الـ IoC Container المستخدم
الـ IoC Container المستخدم هو `Microsoft.Extensions.DependencyInjection` حسب `App.xaml.cs` و/أو ملف المشروع.
راجع الكود الكامل لـ `App.xaml.cs` في السؤال 9 لتفاصيل التسجيل والاستخدام.

---
## الجزء الخامس: الواجهات الرسومية الموجودة

### 14. الكود الكامل لملف LoginView.xaml
### LoginView.xaml
المسار: `Views/LoginView.xaml`
````xml
<UserControl x:Class="FinalLabSystem.Views.LoginView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:infra="clr-namespace:FinalLabSystem.Infrastructure"
             xmlns:vm="clr-namespace:FinalLabSystem.ViewModels"
             mc:Ignorable="d"
             d:DesignHeight="450" d:DesignWidth="400">
    <UserControl.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </UserControl.Resources>
    <Grid Background="#F0F2F5">
        <Border Background="White"
                Width="350"
                Height="420"
                CornerRadius="8"
                HorizontalAlignment="Center"
                VerticalAlignment="Center">
            <Border.Effect>
                <DropShadowEffect BlurRadius="15" Opacity="0.15" ShadowDepth="2"/>
            </Border.Effect>
            <StackPanel Margin="30,20">
                <TextBlock Text="FinalLab"
                           FontSize="28"
                           FontWeight="Bold"
                           Foreground="#1976D2"
                           HorizontalAlignment="Center"
                           Margin="0,0,0,5"/>
                <TextBlock Text="Laboratory Management System"
                           FontSize="13"
                           Foreground="#888"
                           HorizontalAlignment="Center"
                           Margin="0,0,0,25"/>

                <TextBlock Text="Username"
                           FontSize="12"
                           Foreground="#555"
                           Margin="0,0,0,4"/>
                <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
                         Height="40"
                         FontSize="14"
                         Padding="10,4"
                         BorderBrush="#CCC"
                         BorderThickness="1"/>

                <TextBlock Text="Password"
                           FontSize="12"
                           Foreground="#555"
                           Margin="0,14,0,4"/>
                <Grid Height="40">
                    <PasswordBox infra:PasswordBoxHelper.BoundPassword="{Binding Password, UpdateSourceTrigger=PropertyChanged}"
                                 FontSize="14"
                                 Padding="10,4"
                                 BorderBrush="#CCC"
                                 BorderThickness="1">
                        <PasswordBox.Style>
                            <Style TargetType="PasswordBox">
                                <Setter Property="Visibility" Value="Visible"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsPasswordVisible}" Value="True">
                                        <Setter Property="Visibility" Value="Collapsed"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </PasswordBox.Style>
                    </PasswordBox>
                    <TextBox Text="{Binding Password, UpdateSourceTrigger=PropertyChanged}"
                             FontSize="14"
                             Padding="10,4"
                             BorderBrush="#CCC"
                             BorderThickness="1">
                        <TextBox.Style>
                            <Style TargetType="TextBox">
                                <Setter Property="Visibility" Value="Collapsed"/>
                                <Style.Triggers>
                                    <DataTrigger Binding="{Binding IsPasswordVisible}" Value="True">
                                        <Setter Property="Visibility" Value="Visible"/>
                                    </DataTrigger>
                                </Style.Triggers>
                            </Style>
                        </TextBox.Style>
                    </TextBox>
                    <Button Content="👁"
                            Command="{Binding TogglePasswordVisibilityCommand}"
                            Width="32"
                            Height="32"
                            Background="Transparent"
                            BorderThickness="0"
                            FontSize="18"
                            Cursor="Hand"
                            HorizontalAlignment="Right"
                            VerticalAlignment="Center"
                            Margin="0,0,4,0"/>
                </Grid>

                <CheckBox Content="Remember Me"
                          IsChecked="{Binding IsRememberMe}"
                          Margin="0,12,0,0"
                          FontSize="13"
                          Foreground="#555"/>

                <TextBlock Text="{Binding ErrorMessage}"
                           Foreground="#D32F2F"
                           FontSize="13"
                           Margin="0,10,0,0"
                           TextWrapping="Wrap">
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding ErrorMessage}" Value="{x:Null}">
                                    <Setter Property="Visibility" Value="Collapsed"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding ErrorMessage}" Value="">
                                    <Setter Property="Visibility" Value="Collapsed"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>

                <Button Content="Login"
                        Command="{Binding LoginCommand}"
                        Height="42"
                        FontSize="16"
                        FontWeight="Bold"
                        Background="#1976D2"
                        Foreground="White"
                        Margin="0,16,0,0"
                        Cursor="Hand">
                    <Button.Style>
                        <Style TargetType="Button">
                            <Setter Property="Background" Value="#1976D2"/>
                            <Style.Triggers>
                                <Trigger Property="IsEnabled" Value="False">
                                    <Setter Property="Background" Value="#90CAF9"/>
                                </Trigger>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#1565C0"/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>

                <ProgressBar IsIndeterminate="True"
                             Height="4"
                             Margin="0,10,0,0"
                             Foreground="#1976D2"
                             Visibility="{Binding IsBusy, Converter={StaticResource BoolToVis}}"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
````

### 15. ResourceDictionary مشترك للألوان والأنماط
ملفات XAML التي تحتوي موارد/أنماط أو ResourceDictionary:
- `App.xaml`
- `Views/FirstRunSetupView.xaml`
- `Views/LoginView.xaml`

### App.xaml - موارد التطبيق إن وجدت
المسار: `App.xaml`
````xml
<Application x:Class="FinalLabSystem.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:FinalLabSystem"
             ShutdownMode="OnLastWindowClose">
    <Application.Resources>

    </Application.Resources>
</Application>
````

### Views/FirstRunSetupView.xaml
المسار: `Views/FirstRunSetupView.xaml`
````xml
<UserControl x:Class="FinalLabSystem.Views.FirstRunSetupView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:infra="clr-namespace:FinalLabSystem.Infrastructure"
             mc:Ignorable="d"
             d:DesignHeight="560" d:DesignWidth="440">
    <UserControl.Resources>
        <BooleanToVisibilityConverter x:Key="BoolToVis"/>
    </UserControl.Resources>
    <Grid Background="#F0F2F5">
        <Border Background="White"
                Width="400"
                CornerRadius="8"
                HorizontalAlignment="Center"
                VerticalAlignment="Center"
                Padding="32,24">
            <Border.Effect>
                <DropShadowEffect BlurRadius="15" Opacity="0.15" ShadowDepth="2"/>
            </Border.Effect>
            <StackPanel>
                <TextBlock Text="Welcome to FinalLab"
                           FontSize="22"
                           FontWeight="Bold"
                           Foreground="#1976D2"
                           HorizontalAlignment="Center"/>
                <TextBlock Text="Create the initial administrator account"
                           FontSize="13"
                           Foreground="#666"
                           HorizontalAlignment="Center"
                           Margin="0,4,0,20"
                           TextWrapping="Wrap"
                           TextAlignment="Center"/>

                <TextBlock Text="Administrator Username *" FontSize="12" Foreground="#555" Margin="0,0,0,4"/>
                <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged, ValidatesOnNotifyDataErrors=True}"
                         Height="36" FontSize="14" Padding="10,4"
                         BorderBrush="#CCC" BorderThickness="1"/>
                <TextBlock Text="{Binding (Validation.Errors)[0].ErrorContent, RelativeSource={RelativeSource PreviousData}}"
                           Foreground="#D32F2F" FontSize="11" Margin="0,2,0,0"/>

                <TextBlock Text="Display Name (optional)" FontSize="12" Foreground="#555" Margin="0,12,0,4"/>
                <TextBox Text="{Binding DisplayName, UpdateSourceTrigger=PropertyChanged}"
                         Height="36" FontSize="14" Padding="10,4"
                         BorderBrush="#CCC" BorderThickness="1"/>

                <TextBlock Text="Password *" FontSize="12" Foreground="#555" Margin="0,12,0,4"/>
                <PasswordBox infra:PasswordBoxHelper.BoundPassword="{Binding Password, UpdateSourceTrigger=PropertyChanged, ValidatesOnNotifyDataErrors=True}"
                             Height="36" FontSize="14" Padding="10,4"
                             BorderBrush="#CCC" BorderThickness="1"/>
                <TextBlock Text="{Binding (Validation.Errors)[0].ErrorContent, RelativeSource={RelativeSource PreviousData}}"
                           Foreground="#D32F2F" FontSize="11" Margin="0,2,0,0"/>

                <TextBlock Text="Confirm Password *" FontSize="12" Foreground="#555" Margin="0,12,0,4"/>
                <PasswordBox infra:PasswordBoxHelper.BoundPassword="{Binding ConfirmPassword, UpdateSourceTrigger=PropertyChanged, ValidatesOnNotifyDataErrors=True}"
                             Height="36" FontSize="14" Padding="10,4"
                             BorderBrush="#CCC" BorderThickness="1"/>
                <TextBlock Text="{Binding (Validation.Errors)[0].ErrorContent, RelativeSource={RelativeSource PreviousData}}"
                           Foreground="#D32F2F" FontSize="11" Margin="0,2,0,0"/>

                <TextBlock Text="{Binding StatusMessage}"
                           Foreground="#D32F2F" FontSize="13" Margin="0,12,0,0"
                           TextWrapping="Wrap">
                    <TextBlock.Style>
                        <Style TargetType="TextBlock">
                            <Style.Triggers>
                                <DataTrigger Binding="{Binding StatusMessage}" Value="{x:Null}">
                                    <Setter Property="Visibility" Value="Collapsed"/>
                                </DataTrigger>
                                <DataTrigger Binding="{Binding StatusMessage}" Value="">
                                    <Setter Property="Visibility" Value="Collapsed"/>
                                </DataTrigger>
                            </Style.Triggers>
                        </Style>
                    </TextBlock.Style>
                </TextBlock>

                <Button Content="Create Administrator"
                        Command="{Binding CreateAdministratorCommand}"
                        Height="42" FontSize="15" FontWeight="Bold"
                        Background="#1976D2" Foreground="White"
                        Margin="0,18,0,0" Cursor="Hand">
                    <Button.Style>
                        <Style TargetType="Button">
                            <Setter Property="Background" Value="#1976D2"/>
                            <Style.Triggers>
                                <Trigger Property="IsEnabled" Value="False">
                                    <Setter Property="Background" Value="#90CAF9"/>
                                </Trigger>
                                <Trigger Property="IsMouseOver" Value="True">
                                    <Setter Property="Background" Value="#1565C0"/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Button.Style>
                </Button>

                <ProgressBar IsIndeterminate="True" Height="4" Margin="0,10,0,0"
                             Foreground="#1976D2"
                             Visibility="{Binding IsBusy, Converter={StaticResource BoolToVis}}"/>

                <TextBlock Text="This window appears only on first run. The administrator created here will have full system access."
                           FontSize="11" Foreground="#888"
                           Margin="0,16,0,0" TextWrapping="Wrap" TextAlignment="Center"/>
            </StackPanel>
        </Border>
    </Grid>
</UserControl>
````


