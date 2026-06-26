using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Data;

public partial class FinalLabDbContext : DbContext
{
    private readonly ICurrentUserSession? _session;
    private readonly AsyncLocal<bool> _auditingFlag = new();

    public FinalLabDbContext()
    {
    }

    public FinalLabDbContext(DbContextOptions<FinalLabDbContext> options)
        : base(options)
    {
    }

    public FinalLabDbContext(DbContextOptions<FinalLabDbContext> options, ICurrentUserSession session)
        : base(options)
    {
        _session = session;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_auditingFlag.Value)
            return await base.SaveChangesAsync(cancellationToken);

        _auditingFlag.Value = true;
        try
        {
            var snapshot = ChangeTracker.Entries()
                .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                    && e.Entity.GetType().GetCustomAttributes(typeof(AuditableAttribute), false).Length > 0)
                .Select(e => new
                {
                    State = e.State,
                    TableName = e.Metadata.GetTableName() ?? e.Metadata.Name,
                    RecordId = e.Metadata.FindPrimaryKey()?.Properties
                        .Select(p => Convert.ToInt32(e.Property(p.Name).CurrentValue))
                        .FirstOrDefault() ?? 0,
                    Properties = e.Properties.Select(p => new
                    {
                        p.Metadata.Name,
                        p.IsModified,
                        OriginalValue = p.OriginalValue,
                        CurrentValue = p.CurrentValue
                    }).ToList()
                })
                .ToList();

            var result = await base.SaveChangesAsync(cancellationToken);

            if (snapshot.Count == 0 || _session is null)
                return result;

            var staffId = _session.CurrentUser?.StaffId;
            var now = DateTime.UtcNow;

            foreach (var item in snapshot)
            {
                foreach (var prop in item.Properties)
                {
                    if (!prop.IsModified && item.State == EntityState.Modified)
                        continue;

                    AuditLogs.Add(new AuditLog
                    {
                        TableName = item.TableName,
                        RecordId = item.RecordId,
                        Action = item.State switch
                        {
                            EntityState.Added => "A",
                            EntityState.Modified => "M",
                            EntityState.Deleted => "D",
                            _ => "U"
                        },
                        FieldName = prop.Name,
                        OldValue = item.State == EntityState.Added ? null : prop.OriginalValue?.ToString(),
                        NewValue = prop.CurrentValue?.ToString(),
                        ChangedBy = staffId,
                        ChangedAt = now
                    });
                }
            }

            await base.SaveChangesAsync(cancellationToken);
            return result;
        }
        finally
        {
            _auditingFlag.Value = false;
        }
    }

    public override int SaveChanges()
    {
        return SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<CrossMatchDonor> CrossMatchDonors { get; set; }

    public virtual DbSet<CrossMatchTest> CrossMatchTests { get; set; }

    public virtual DbSet<ExternalLab> ExternalLabs { get; set; }

    public virtual DbSet<LabSetting> LabSettings { get; set; }

    public virtual DbSet<MicrobiologyCulture> MicrobiologyCultures { get; set; }

    public virtual DbSet<MicrobiologyOrganism> MicrobiologyOrganisms { get; set; }

    public virtual DbSet<NormalRange> NormalRanges { get; set; }

    public virtual DbSet<OrganismAntibiotic> OrganismAntibiotics { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PriceScheme> PriceSchemes { get; set; }

    public virtual DbSet<ReferralSource> ReferralSources { get; set; }

    public virtual DbSet<ReportCommentTemplate> ReportCommentTemplates { get; set; }

    public virtual DbSet<SampleTube> SampleTubes { get; set; }

    public virtual DbSet<SemenAnalysis> SemenAnalyses { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffPermission> StaffPermissions { get; set; }

    public virtual DbSet<TestCategory> TestCategories { get; set; }

    public virtual DbSet<TestComponent> TestComponents { get; set; }

    public virtual DbSet<CollectionType> CollectionTypes { get; set; }

    public virtual DbSet<TestGroup> TestGroups { get; set; }

    public virtual DbSet<TestResult> TestResults { get; set; }

    public virtual DbSet<TestType> TestTypes { get; set; }

    public virtual DbSet<TestTypePrice> TestTypePrices { get; set; }

    public virtual DbSet<TestTypeSampleTube> TestTypeSampleTubes { get; set; }

    public virtual DbSet<TestWorkflow> TestWorkflows { get; set; }

    public virtual DbSet<VOutstandingBalance> VOutstandingBalances { get; set; }

    public virtual DbSet<VPatientHistory> VPatientHistories { get; set; }

    public virtual DbSet<VPendingTest> VPendingTests { get; set; }

    public virtual DbSet<VReferralCommissionReport> VReferralCommissionReports { get; set; }

    public virtual DbSet<VResultAuditTrail> VResultAuditTrails { get; set; }

    public virtual DbSet<VSampleTubeStatus> VSampleTubeStatuses { get; set; }

    public virtual DbSet<Visit> Visits { get; set; }

    public virtual DbSet<VisitTest> VisitTests { get; set; }

    public virtual DbSet<Unit> Units => Set<Unit>();

    public virtual DbSet<TubeMaterial> TubeMaterials => Set<TubeMaterial>();

        public virtual DbSet<ReceiptPrintLog> ReceiptPrintLogs { get; set; }

        // V4.0 New DbSets
        public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<AntibioticCatalog> AntibioticCatalogs { get; set; }

    public virtual DbSet<ContractInvoice> ContractInvoices { get; set; }

    public virtual DbSet<ContractPayment> ContractPayments { get; set; }

    public virtual DbSet<ExternalShipment> ExternalShipments { get; set; }

    public virtual DbSet<ExternalShipmentItem> ExternalShipmentItems { get; set; }

    public virtual DbSet<PatientMedicalHistory> PatientMedicalHistories { get; set; }

    public virtual DbSet<TestProfile> TestProfiles { get; set; }

    public virtual DbSet<TestProfileItem> TestProfileItems { get; set; }

    public virtual DbSet<VisitCharge> VisitCharges { get; set; }

    public virtual DbSet<WorkShift> WorkShifts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditId).HasName("PK__AuditLog__5AF33E3359BBE1E9");

            entity.ToTable("AuditLog");

            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.Action)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("action");
            entity.Property(e => e.ChangedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.RecordId).HasColumnName("record_id");
            entity.Property(e => e.SessionInfo)
                .HasMaxLength(200)
                .HasColumnName("session_info");
            entity.Property(e => e.TableName)
                .HasMaxLength(100)
                .HasColumnName("table_name");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("FK_Audit_Staff");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.CompanyId).HasName("PK__Company__3E267235E4BD64F7");

            entity.ToTable("Company");

            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(150)
                .HasColumnName("company_name");
            entity.Property(e => e.CompanyType)
                .HasMaxLength(30)
                .HasDefaultValue("CORPORATE")
                .HasColumnName("company_type");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreditLimit).HasColumnName("credit_limit");
            entity.Property(e => e.DiscountRate).HasColumnName("discount_rate");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PaymentTerms)
                .HasMaxLength(200)
                .HasColumnName("payment_terms");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Phone2)
                .HasMaxLength(20)
                .HasColumnName("phone2");
            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");
            entity.Property(e => e.ContractStartDate)
                .HasColumnName("contract_start_date");
            entity.Property(e => e.ContractEndDate)
                .HasColumnName("contract_end_date");
            entity.Property(e => e.BillingPeriodicity)
                .HasMaxLength(20)
                .HasColumnName("billing_periodicity");

            entity.HasOne(d => d.Scheme).WithMany(p => p.Companies)
                .HasForeignKey(d => d.SchemeId)
                .HasConstraintName("FK_Company_Scheme");
        });

        modelBuilder.Entity<CrossMatchDonor>(entity =>
        {
            entity.HasKey(e => e.DonorResultId).HasName("PK__CrossMat__8AF1F7821A61BE6A");

            entity.ToTable("CrossMatchDonor");

            entity.Property(e => e.DonorResultId).HasColumnName("donor_result_id");
            entity.Property(e => e.CrossmatchId).HasColumnName("crossmatch_id");
            entity.Property(e => e.DirectAntiglobulin)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("direct_antiglobulin");
            entity.Property(e => e.DonorBloodType)
                .HasMaxLength(5)
                .HasColumnName("donor_blood_type");
            entity.Property(e => e.DonorRhFactor)
                .HasMaxLength(5)
                .HasColumnName("donor_rh_factor");
            entity.Property(e => e.DonorUnitNumber)
                .HasMaxLength(50)
                .HasColumnName("donor_unit_number");
            entity.Property(e => e.IndirectAntiglobulin)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("indirect_antiglobulin");
            entity.Property(e => e.MajorCrossmatch)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("major_crossmatch");
            entity.Property(e => e.MinorCrossmatch)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("minor_crossmatch");
            entity.Property(e => e.ResultNote)
                .HasMaxLength(200)
                .HasColumnName("result_note");
            entity.Property(e => e.UnitResult)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("unit_result");

            entity.HasOne(d => d.Crossmatch).WithMany(p => p.CrossMatchDonors)
                .HasForeignKey(d => d.CrossmatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Donor_CrossMatch");
        });

        modelBuilder.Entity<CrossMatchTest>(entity =>
        {
            entity.HasKey(e => e.CrossmatchId).HasName("PK__CrossMat__B7B3586A04BFD04D");

            entity.ToTable("CrossMatchTest");

            entity.HasIndex(e => e.VisitTestId, "UQ__CrossMat__D6ECAC17BAD55465").IsUnique();

            entity.Property(e => e.CrossmatchId).HasColumnName("crossmatch_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OverallResult)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("overall_result");
            entity.Property(e => e.RecipientAntibodyScreen)
                .HasMaxLength(20)
                .HasColumnName("recipient_antibody_screen");
            entity.Property(e => e.RecipientBloodType)
                .HasMaxLength(5)
                .HasColumnName("recipient_blood_type");
            entity.Property(e => e.RecipientRhFactor)
                .HasMaxLength(5)
                .HasColumnName("recipient_rh_factor");
            entity.Property(e => e.TestedAt)
                .HasPrecision(0)
                .HasColumnName("tested_at");
            entity.Property(e => e.TestedBy).HasColumnName("tested_by");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");

            entity.HasOne(d => d.TestedByNavigation).WithMany(p => p.CrossMatchTests)
                .HasForeignKey(d => d.TestedBy)
                .HasConstraintName("FK_CrossMatch_TestedBy");

            entity.HasOne(d => d.VisitTest).WithOne(p => p.CrossMatchTest)
                .HasForeignKey<CrossMatchTest>(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CrossMatch_VisitTest");
        });

        modelBuilder.Entity<ExternalLab>(entity =>
        {
            entity.HasKey(e => e.ExternalLabId).HasName("PK__External__5CC694F2044D69F8");

            entity.ToTable("ExternalLab");

            entity.Property(e => e.ExternalLabId).HasColumnName("external_lab_id");
            entity.Property(e => e.Address)
                .HasMaxLength(250)
                .HasColumnName("address");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LabName)
                .HasMaxLength(150)
                .HasColumnName("lab_name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<LabSetting>(entity =>
        {
            entity.HasKey(e => e.SettingKey).HasName("PK__LabSetti__0DFAC426DE80F226");

            entity.Property(e => e.SettingKey)
                .HasMaxLength(100)
                .HasColumnName("setting_key");
            entity.Property(e => e.IsRequired).HasColumnName("is_required");
            entity.Property(e => e.LastUpdatedAt)
                .HasPrecision(0)
                .HasColumnName("last_updated_at");
            entity.Property(e => e.LastUpdatedBy).HasColumnName("last_updated_by");
            entity.Property(e => e.SettingDescription)
                .HasMaxLength(200)
                .HasColumnName("setting_description");
            entity.Property(e => e.SettingGroup)
                .HasMaxLength(50)
                .HasColumnName("setting_group");
            entity.Property(e => e.SettingValue).HasColumnName("setting_value");
            entity.Property(e => e.EnforceStageGating).HasColumnName("enforce_stage_gating");
            entity.Property(e => e.EnableServerPrinting).HasColumnName("enable_server_printing");

            entity.HasOne(d => d.LastUpdatedByNavigation).WithMany(p => p.LabSettings)
                .HasForeignKey(d => d.LastUpdatedBy)
                .HasConstraintName("FK_LabSettings_Staff");
        });

        modelBuilder.Entity<MicrobiologyCulture>(entity =>
        {
            entity.HasKey(e => e.CultureId).HasName("PK__Microbio__3945B0F56E09AB20");

            entity.ToTable("MicrobiologyCulture");

            entity.HasIndex(e => e.VisitTestId, "UQ__Microbio__D6ECAC17AFF09907").IsUnique();

            entity.Property(e => e.CultureId).HasColumnName("culture_id");
            entity.Property(e => e.CultureResult)
                .HasMaxLength(20)
                .HasDefaultValue("PENDING")
                .HasColumnName("culture_result");
            entity.Property(e => e.FinalComment).HasColumnName("final_comment");
            entity.Property(e => e.FinalReadingAt)
                .HasPrecision(0)
                .HasColumnName("final_reading_at");
            entity.Property(e => e.IncubationHours)
                .HasDefaultValue((short)48)
                .HasColumnName("incubation_hours");
            entity.Property(e => e.InoculatedBy).HasColumnName("inoculated_by");
            entity.Property(e => e.ReadBy).HasColumnName("read_by");
            entity.Property(e => e.ReceivedAt)
                .HasPrecision(0)
                .HasColumnName("received_at");
            entity.Property(e => e.SpecimenSource)
                .HasMaxLength(150)
                .HasColumnName("specimen_source");
            entity.Property(e => e.SpecimenVolumeMl).HasColumnName("specimen_volume_ml");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");

            entity.HasOne(d => d.InoculatedByNavigation).WithMany(p => p.MicrobiologyCultureInoculatedByNavigations)
                .HasForeignKey(d => d.InoculatedBy)
                .HasConstraintName("FK_Culture_Inoculated");

            entity.HasOne(d => d.ReadByNavigation).WithMany(p => p.MicrobiologyCultureReadByNavigations)
                .HasForeignKey(d => d.ReadBy)
                .HasConstraintName("FK_Culture_ReadBy");

            entity.HasOne(d => d.VisitTest).WithOne(p => p.MicrobiologyCulture)
                .HasForeignKey<MicrobiologyCulture>(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Culture_VisitTest");
        });

        modelBuilder.Entity<MicrobiologyOrganism>(entity =>
        {
            entity.HasKey(e => e.OrganismId).HasName("PK__Microbio__E222B151B7FA5516");

            entity.ToTable("MicrobiologyOrganism");

            entity.Property(e => e.OrganismId).HasColumnName("organism_id");
            entity.Property(e => e.ColonyCount)
                .HasMaxLength(80)
                .HasColumnName("colony_count");
            entity.Property(e => e.CultureId).HasColumnName("culture_id");
            entity.Property(e => e.GramStain)
                .HasMaxLength(30)
                .HasColumnName("gram_stain");
            entity.Property(e => e.Morphology)
                .HasMaxLength(100)
                .HasColumnName("morphology");
            entity.Property(e => e.OrganismName)
                .HasMaxLength(150)
                .HasColumnName("organism_name");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue((byte)1)
                .HasColumnName("sort_order");

            entity.HasOne(d => d.Culture).WithMany(p => p.MicrobiologyOrganisms)
                .HasForeignKey(d => d.CultureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Organism_Culture");
        });

        modelBuilder.Entity<NormalRange>(entity =>
        {
            entity.HasKey(e => e.RangeId).HasName("PK__NormalRa__3C0A88B60E81F7C4");

            entity.ToTable("NormalRange");

            entity.Property(e => e.RangeId).HasColumnName("range_id");
            entity.Property(e => e.AgeDescription)
                .HasMaxLength(50)
                .HasColumnName("age_description");
            entity.Property(e => e.AgeFromDays).HasColumnName("age_from_days");
            entity.Property(e => e.AgeToDays)
                .HasDefaultValue(36500)
                .HasColumnName("age_to_days");
            entity.Property(e => e.AgeFromValue).HasColumnName("age_from_value");
            entity.Property(e => e.AgeToValue).HasColumnName("age_to_value");
            entity.Property(e => e.ForPregnantOnly).HasColumnName("for_pregnant_only");
            entity.Property(e => e.AgeUnit)
                .HasMaxLength(10)
                .HasColumnName("age_unit");
            entity.Property(e => e.LowFlag)
                .HasMaxLength(20)
                .HasColumnName("low_flag");
            entity.Property(e => e.HighFlag)
                .HasMaxLength(20)
                .HasColumnName("high_flag");
            entity.Property(e => e.LowComment)
                .HasMaxLength(500)
                .HasColumnName("low_comment");
            entity.Property(e => e.HighComment)
                .HasMaxLength(500)
                .HasColumnName("high_comment");
            entity.Property(e => e.CriticalRangeText)
                .HasMaxLength(200)
                .HasColumnName("critical_range_text");
            entity.Property(e => e.CriticalFlag)
                .HasMaxLength(20)
                .HasColumnName("critical_flag");
            entity.Property(e => e.CriticalComment)
                .HasMaxLength(500)
                .HasColumnName("critical_comment");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.FastingState)
                .HasMaxLength(1)
                .HasDefaultValue("A")
                .IsFixedLength()
                .HasColumnName("fasting_state");
            entity.Property(e => e.HighCritical).HasColumnName("high_critical");
            entity.Property(e => e.HighNormal).HasColumnName("high_normal");
            entity.Property(e => e.LowCritical).HasColumnName("low_critical");
            entity.Property(e => e.LowNormal).HasColumnName("low_normal");
            entity.Property(e => e.NormalRangeText)
                .HasMaxLength(200)
                .HasColumnName("normal_range_text");
            entity.Property(e => e.RangeNote)
                .HasMaxLength(200)
                .HasColumnName("range_note");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");
            entity.Property(e => e.Version)
                .HasDefaultValue(1)
                .HasColumnName("version");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.SupersededById)
                .HasColumnName("superseded_by_id");
            entity.HasOne(e => e.SupersededBy)
                .WithMany()
                .HasForeignKey(e => e.SupersededById)
                .OnDelete(DeleteBehavior.NoAction);
            entity.Property(e => e.Sex)
                .HasMaxLength(1)
                .HasDefaultValue("B")
                .IsFixedLength()
                .HasColumnName("sex");

            entity.HasOne(d => d.Component).WithMany(p => p.NormalRanges)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_NormalRange_Component");
        });

        modelBuilder.Entity<OrganismAntibiotic>(entity =>
        {
            entity.HasKey(e => e.AntibioticResultId).HasName("PK__Organism__A60286E19FAF4B63");

            entity.ToTable("OrganismAntibiotic");

            entity.Property(e => e.AntibioticResultId).HasColumnName("antibiotic_result_id");
            entity.Property(e => e.AntibioticClass)
                .HasMaxLength(100)
                .HasColumnName("antibiotic_class");
            entity.Property(e => e.AntibioticName)
                .HasMaxLength(100)
                .HasColumnName("antibiotic_name");
            entity.Property(e => e.BreakpointStandard)
                .HasMaxLength(30)
                .HasDefaultValue("CLSI")
                .HasColumnName("breakpoint_standard");
            entity.Property(e => e.DiskDiffusionMm).HasColumnName("disk_diffusion_mm");
            entity.Property(e => e.MicValue)
                .HasMaxLength(30)
                .HasColumnName("mic_value");
            entity.Property(e => e.OrganismId).HasColumnName("organism_id");
            entity.Property(e => e.Sensitivity)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("sensitivity");

            entity.HasOne(d => d.Organism).WithMany(p => p.OrganismAntibiotics)
                .HasForeignKey(d => d.OrganismId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Antibiotic_Organism");
        });

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
            entity.Property(e => e.IsVip)
                .HasDefaultValue(false)
                .HasColumnName("is_vip");
            entity.Property(e => e.PatientType)
                .HasMaxLength(20)
                .HasDefaultValue("Individual")
                .HasColumnName("patient_type");
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
                .HasConversion(v => v.ToString(), v => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), v ?? "Cash", true))
                .HasMaxLength(20)
                .HasDefaultValue(PaymentMethod.Cash)
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

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__E5331AFABA9B7A47");

            entity.ToTable("Permission");

            entity.HasIndex(e => e.PermissionCode, "UQ__Permissi__A98A808EC41BBC9C").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.PermissionCode)
                .HasMaxLength(100)
                .HasColumnName("permission_code");
            entity.Property(e => e.PermissionGroup)
                .HasMaxLength(50)
                .HasColumnName("permission_group");
            entity.Property(e => e.PermissionName)
                .HasMaxLength(100)
                .HasColumnName("permission_name");
        });

        modelBuilder.Entity<PriceScheme>(entity =>
        {
            entity.HasKey(e => e.SchemeId).HasName("PK__PriceSch__8DF8FA63378D6AAE");

            entity.ToTable("PriceScheme");

            entity.HasIndex(e => e.SchemeName, "UQ__PriceSch__BB56A46B3A492F6F").IsUnique();

            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.SchemeName)
                .HasMaxLength(100)
                .HasColumnName("scheme_name");
        });

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

        modelBuilder.Entity<ReportCommentTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__ReportCo__BE44E079B3B61F28");

            entity.ToTable("ReportCommentTemplate");

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CommentLang)
                .HasMaxLength(2)
                .HasDefaultValue("AR")
                .IsFixedLength()
                .HasColumnName("comment_lang");
            entity.Property(e => e.CommentText).HasColumnName("comment_text");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ModifiedAt)
                .HasPrecision(0)
                .HasColumnName("modified_at");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");
            entity.Property(e => e.Title)
                .HasMaxLength(150)
                .HasColumnName("title");
            entity.Property(e => e.TriggerCondition)
                .HasMaxLength(20)
                .HasDefaultValue("Manual")
                .HasColumnName("trigger_condition");

            entity.HasOne(d => d.Category).WithMany(p => p.ReportCommentTemplates)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Template_Category");

            entity.HasOne(d => d.Component).WithMany(p => p.ReportCommentTemplates)
                .HasForeignKey(d => d.ComponentId)
                .HasConstraintName("FK_Template_Component");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ReportCommentTemplateCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_Template_CreatedBy");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.ReportCommentTemplateModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_Template_ModifiedBy");

            entity.HasOne(d => d.Testtype).WithMany(p => p.ReportCommentTemplates)
                .HasForeignKey(d => d.TesttypeId)
                .HasConstraintName("FK_Template_TestType");
        });

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

        modelBuilder.Entity<SemenAnalysis>(entity =>
        {
            entity.HasKey(e => e.SemenId).HasName("PK__SemenAna__9D7B8C30DAF45734");

            entity.ToTable("SemenAnalysis");

            entity.HasIndex(e => e.VisitTestId, "UQ__SemenAna__D6ECAC172DED86C9").IsUnique();

            entity.Property(e => e.SemenId).HasColumnName("semen_id");
            entity.Property(e => e.AbstinenceDays).HasColumnName("abstinence_days");
            entity.Property(e => e.Agglutination)
                .HasMaxLength(30)
                .HasColumnName("agglutination");
            entity.Property(e => e.AnalysisNotes).HasColumnName("analysis_notes");
            entity.Property(e => e.AnalysisTime)
                .HasPrecision(0)
                .HasColumnName("analysis_time");
            entity.Property(e => e.AnalyzedBy).HasColumnName("analyzed_by");
            entity.Property(e => e.Appearance)
                .HasMaxLength(50)
                .HasColumnName("appearance");
            entity.Property(e => e.BacteriaNoted).HasColumnName("bacteria_noted");
            entity.Property(e => e.CollectionMethod)
                .HasMaxLength(50)
                .HasColumnName("collection_method");
            entity.Property(e => e.CollectionTime)
                .HasPrecision(0)
                .HasColumnName("collection_time");
            entity.Property(e => e.ConcentrationMPerMl).HasColumnName("concentration_M_per_ml");
            entity.Property(e => e.EpithelialCells)
                .HasMaxLength(50)
                .HasColumnName("epithelial_cells");
            entity.Property(e => e.HeadDefectsPct).HasColumnName("head_defects_pct");
            entity.Property(e => e.ImmotileDPct).HasColumnName("immotile_d_pct");
            entity.Property(e => e.Interpretation)
                .HasMaxLength(200)
                .HasColumnName("interpretation");
            entity.Property(e => e.LiquefactionTimeMin).HasColumnName("liquefaction_time_min");
            entity.Property(e => e.MidpieceDefectsPct).HasColumnName("midpiece_defects_pct");
            entity.Property(e => e.NonProgressiveCPct).HasColumnName("non_progressive_c_pct");
            entity.Property(e => e.NormalMorphologyPct).HasColumnName("normal_morphology_pct");
            entity.Property(e => e.PhValue).HasColumnName("ph_value");
            entity.Property(e => e.ProgressiveAPct).HasColumnName("progressive_a_pct");
            entity.Property(e => e.ProgressiveBPct).HasColumnName("progressive_b_pct");
            entity.Property(e => e.ProgressiveMotilityPct).HasColumnName("progressive_motility_pct");
            entity.Property(e => e.RbcPerHpf)
                .HasMaxLength(20)
                .HasColumnName("rbc_per_hpf");
            entity.Property(e => e.TailDefectsPct).HasColumnName("tail_defects_pct");
            entity.Property(e => e.TotalCountM).HasColumnName("total_count_M");
            entity.Property(e => e.TotalMotilityPct).HasColumnName("total_motility_pct");
            entity.Property(e => e.Viscosity)
                .HasMaxLength(20)
                .HasColumnName("viscosity");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");
            entity.Property(e => e.VitalityPct).HasColumnName("vitality_pct");
            entity.Property(e => e.VolumeMl).HasColumnName("volume_ml");
            entity.Property(e => e.WbcPerHpf)
                .HasMaxLength(20)
                .HasColumnName("wbc_per_hpf");

            entity.HasOne(d => d.AnalyzedByNavigation).WithMany(p => p.SemenAnalyses)
                .HasForeignKey(d => d.AnalyzedBy)
                .HasConstraintName("FK_Semen_AnalyzedBy");

            entity.HasOne(d => d.VisitTest).WithOne(p => p.SemenAnalysis)
                .HasForeignKey<SemenAnalysis>(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Semen_VisitTest");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__1963DD9C06F90178");

            entity.HasIndex(e => e.Username, "UQ__Staff__F3DBC5720B13A882").IsUnique();

            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountLimit).HasColumnName("discount_limit");
            entity.Property(e => e.DisplayName)
                .HasMaxLength(100)
                .HasColumnName("display_name");
            entity.Property(e => e.DisplayNameAr)
                .HasMaxLength(100)
                .HasColumnName("display_name_ar");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(100)
                .HasColumnName("job_title");
            entity.Property(e => e.LastLoginAt)
                .HasPrecision(0)
                .HasColumnName("last_login_at");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
        });

        modelBuilder.Entity<StaffPermission>(entity =>
        {
            entity.HasKey(e => e.StaffPermId).HasName("PK__StaffPer__EEE93F7D719ECFFD");

            entity.ToTable("StaffPermission");

            entity.HasIndex(e => new { e.StaffId, e.PermissionId }, "UQ_StaffPermission").IsUnique();

            entity.Property(e => e.StaffPermId).HasColumnName("staff_perm_id");
            entity.Property(e => e.GrantedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("granted_at");
            entity.Property(e => e.GrantedBy).HasColumnName("granted_by");
            entity.Property(e => e.IsGranted).HasColumnName("is_granted");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.GrantedByNavigation).WithMany(p => p.StaffPermissionGrantedByNavigations)
                .HasForeignKey(d => d.GrantedBy)
                .HasConstraintName("FK_StaffPerm_Granter");

            entity.HasOne(d => d.Permission).WithMany(p => p.StaffPermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffPerm_Perm");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffPermissionStaffs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StaffPerm_Staff");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("Unit");
            entity.HasKey(e => e.UnitId);
            entity.Property(e => e.UnitId).HasColumnName("unit_id");
            entity.Property(e => e.UnitName).IsRequired().HasMaxLength(30).HasColumnName("unit_name");
            entity.Property(e => e.UnitNameAr).HasMaxLength(30).HasColumnName("unit_name_ar");
            entity.Property(e => e.Abbreviation).HasMaxLength(10).HasColumnName("abbreviation");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<TubeMaterial>(entity =>
        {
            entity.ToTable("TubeMaterial");
            entity.HasKey(e => e.TubeMaterialId);
            entity.Property(e => e.TubeMaterialId).HasColumnName("tube_material_id");
            entity.Property(e => e.MaterialName).IsRequired().HasMaxLength(50).HasColumnName("material_name");
            entity.Property(e => e.MaterialNameAr).HasMaxLength(30).HasColumnName("material_name_ar");
            entity.Property(e => e.TubeColor).HasMaxLength(20).HasColumnName("tube_color");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<CollectionType>(entity =>
        {
            entity.HasKey(e => e.CollectionTypeId).HasName("PK_CollectionType");

            entity.ToTable("CollectionTypes");

            entity.Property(e => e.CollectionTypeId).HasColumnName("collection_type_id");
            entity.Property(e => e.TypeNameEn)
                .HasMaxLength(100)
                .IsRequired()
                .HasColumnName("type_name_en");
            entity.Property(e => e.TypeNameAr)
                .HasMaxLength(100)
                .HasColumnName("type_name_ar");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
        });

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
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Component_Type");
        });

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

        modelBuilder.Entity<TestResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__TestResu__AFB3C31618CCAA39");

            entity.ToTable("TestResult", tb =>
                {
                    tb.HasTrigger("TR_TestResult_AuditInsert");
                    tb.HasTrigger("TR_TestResult_AuditUpdate");
                    tb.HasCheckConstraint(
                        "CK_TestResult_NumericSync",
                        "[result_numeric] IS NULL OR [result_value] IS NOT NULL");
                });

            entity.HasIndex(e => new { e.VisitTestId, e.ComponentId }, "UQ_TestResult").IsUnique();

            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.ComponentId).HasColumnName("component_id");
            entity.Property(e => e.EnteredAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("entered_at");
            entity.Property(e => e.EnteredBy).HasColumnName("entered_by");
            entity.Property(e => e.LastModifiedAt)
                .HasPrecision(0)
                .HasColumnName("last_modified_at");
            entity.Property(e => e.LastModifiedBy).HasColumnName("last_modified_by");
            entity.Property(e => e.ResultNumeric)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("result_numeric");
            entity.Property(e => e.ResultStatus)
                .HasMaxLength(15)
                .HasColumnName("result_status");
            entity.Property(e => e.ResultValue)
                .HasMaxLength(150)
                .HasColumnName("result_value");
            entity.Property(e => e.SnapHighCritical).HasColumnName("snap_high_critical");
            entity.Property(e => e.SnapHighNormal).HasColumnName("snap_high_normal");
            entity.Property(e => e.SnapLowCritical).HasColumnName("snap_low_critical");
            entity.Property(e => e.SnapLowNormal).HasColumnName("snap_low_normal");
            entity.Property(e => e.SnapNormalText)
                .HasMaxLength(200)
                .HasColumnName("snap_normal_text");
            entity.Property(e => e.SnapUnit)
                .HasMaxLength(30)
                .HasColumnName("snap_unit");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");

            entity.HasOne(d => d.Component).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.ComponentId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Result_Component");

            entity.HasOne(d => d.NormalRange).WithMany()
                .HasForeignKey(d => d.NormalRangeId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(d => d.EnteredByNavigation).WithMany(p => p.TestResultEnteredByNavigations)
                .HasForeignKey(d => d.EnteredBy)
                .HasConstraintName("FK_Result_EnteredBy");

            entity.HasOne(d => d.LastModifiedByNavigation).WithMany(p => p.TestResultLastModifiedByNavigations)
                .HasForeignKey(d => d.LastModifiedBy)
                .HasConstraintName("FK_Result_ModifiedBy");

            entity.Property(e => e.ValidationStatus)
                .HasConversion<string>()
                .HasMaxLength(30)
                .HasColumnName("validation_status");

            entity.Property(e => e.ValidatedByStaffId).HasColumnName("validated_by_staff_id");
            entity.Property(e => e.ValidatedAt)
                .HasPrecision(0)
                .HasColumnName("validated_at");

            entity.HasOne(d => d.ValidatedBy).WithMany(p => p.TestResultValidatedByNavigations)
                .HasForeignKey(d => d.ValidatedByStaffId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Result_ValidatedBy");

            entity.HasOne(d => d.VisitTest).WithMany(p => p.TestResults)
                .HasForeignKey(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Result_VisitTest");
        });

        modelBuilder.Entity<TestType>(entity =>
        {
            entity.HasKey(e => e.TesttypeId).HasName("PK__TestType__8547CC22615C1CAA");

            entity.ToTable("TestType");

            entity.HasIndex(e => e.TypeCode, "UQ__TestType__2CB4DBF5D148A91E").IsUnique();

            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");
            entity.Property(e => e.DefaultPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("default_price");
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
            entity.Property(e => e.ReportNameLine1)
                .HasMaxLength(200)
                .HasColumnName("report_name_line1");
            entity.Property(e => e.ReportNameLine2)
                .HasMaxLength(200)
                .HasColumnName("report_name_line2");
            entity.Property(e => e.BillNameLine1)
                .HasMaxLength(200)
                .HasColumnName("bill_name_line1");
            entity.Property(e => e.BillNameLine2)
                .HasMaxLength(200)
                .HasColumnName("bill_name_line2");
            entity.Property(e => e.HistoryName)
                .HasMaxLength(100)
                .HasColumnName("history_name");
            entity.Property(e => e.CollectionNotes)
                .HasMaxLength(1000)
                .HasColumnName("collection_notes");
            entity.Property(e => e.CollectionTypeId).HasColumnName("collection_type_id");
            entity.Property(e => e.IsRoutineTest)
                .HasDefaultValue(false)
                .HasColumnName("is_routine_test");
            entity.Property(e => e.SeeReport)
                .HasDefaultValue(false)
                .HasColumnName("see_report");
            entity.Property(e => e.PrintWithOther)
                .HasDefaultValue(true)
                .HasColumnName("print_with_other");
            entity.Property(e => e.AddWithGroup)
                .HasDefaultValue(true)
                .HasColumnName("add_with_group");
            entity.Property(e => e.IsMainTest)
                .HasDefaultValue(false)
                .HasColumnName("is_main_test");
            entity.Property(e => e.IsSendOutside)
                .HasDefaultValue(false)
                .HasColumnName("is_send_outside");
            entity.Property(e => e.Behavior)
                .HasColumnName("Behavior")
                .HasDefaultValue(TestTypeBehavior.None);
            entity.Property(e => e.OutsideLabName)
                .HasMaxLength(200)
                .HasColumnName("outside_lab_name");
            entity.Property(e => e.OutsideCostPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("outside_cost_price");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(20)
                .HasColumnName("reference_type");
            entity.Property(e => e.PatientQuestion)
                .HasMaxLength(500)
                .HasColumnName("patient_question");

            entity.HasIndex(e => e.BillNameLine1, "IX_TestType_BillNameLine1");
            entity.HasIndex(e => e.HistoryName, "IX_TestType_HistoryName");

            entity.HasOne(d => d.Group).WithMany(p => p.TestTypes)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TestType_Group");

            entity.HasOne(d => d.CollectionType).WithMany(p => p.TestTypes)
                .HasForeignKey(d => d.CollectionTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TestType_CollectionType");
        });

        modelBuilder.Entity<TestTypePrice>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__TestType__1681726DC1872DBA");

            entity.ToTable("TestTypePrice");

            entity.HasIndex(e => new { e.SchemeId, e.TesttypeId }, "UQ_SchemeTypePrice").IsUnique();

            entity.Property(e => e.PriceId).HasColumnName("price_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");
            entity.Property(e => e.TesttypeId).HasColumnName("testtype_id");

            entity.HasOne(d => d.Scheme).WithMany(p => p.TestTypePrices)
                .HasForeignKey(d => d.SchemeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TestTypePrice_Scheme");

            entity.HasOne(d => d.Testtype).WithMany(p => p.TestTypePrices)
                .HasForeignKey(d => d.TesttypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TestTypePrice_Type");
        });

        modelBuilder.Entity<TestTypeSampleTube>(entity =>
        {
            entity.HasKey(e => e.TestTypeTubeId).HasName("PK_TestTypeSampleTube");

            entity.ToTable("TestTypeSampleTube");

            entity.HasIndex(e => new { e.TestTypeId, e.SortOrder }, "IX_TestTypeSampleTube_TestType_Sort");

            entity.Property(e => e.TestTypeTubeId).HasColumnName("testtype_tube_id");
            entity.Property(e => e.TestTypeId).HasColumnName("testtype_id");
            entity.Property(e => e.TubeType)
                .HasMaxLength(50)
                .HasColumnName("tube_type");
            entity.Property(e => e.TubeColor)
                .HasMaxLength(30)
                .HasColumnName("tube_color");
            entity.Property(e => e.SampleType)
                .HasMaxLength(50)
                .HasColumnName("sample_type");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.SortOrder)
                .HasDefaultValue((short)0)
                .HasColumnName("sort_order");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");

            entity.HasOne(d => d.Testtype).WithMany(p => p.TestTypeSampleTubes)
                .HasForeignKey(d => d.TestTypeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TestTypeSampleTube_TestType");
        });

        modelBuilder.Entity<TestWorkflow>(entity =>
        {
            entity.HasKey(e => e.WorkflowId).HasName("PK__TestWork__64A76B7089A5737D");

            entity.ToTable("TestWorkflow");

            entity.Property(e => e.WorkflowId).HasColumnName("workflow_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(200)
                .HasColumnName("notes");
            entity.Property(e => e.PerformedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("performed_at");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.Stage)
                .HasMaxLength(15)
                .HasColumnName("stage");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.TestWorkflows)
                .HasForeignKey(d => d.PerformedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Workflow_Staff");

            entity.HasOne(d => d.VisitTest).WithMany(p => p.TestWorkflows)
                .HasForeignKey(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Workflow_VisitTest");
        });

        modelBuilder.Entity<VOutstandingBalance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_OutstandingBalances");

            entity.Property(e => e.BalanceDue).HasColumnName("balance_due");
            entity.Property(e => e.CompanyName)
                .HasMaxLength(150)
                .HasColumnName("company_name");
            entity.Property(e => e.DaysOverdue).HasColumnName("days_overdue");
            entity.Property(e => e.PatientCode)
                .HasMaxLength(30)
                .HasColumnName("patient_code");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(10)
                .HasColumnName("payment_status");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.TotalAfterDiscount).HasColumnName("total_after_discount");
            entity.Property(e => e.TotalPaid).HasColumnName("total_paid");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitDate)
                .HasPrecision(0)
                .HasColumnName("visit_date");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
        });

        modelBuilder.Entity<VPatientHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_PatientHistory");

            entity.Property(e => e.ComponentName)
                .HasMaxLength(150)
                .HasColumnName("component_name");
            entity.Property(e => e.EnteredAt)
                .HasPrecision(0)
                .HasColumnName("entered_at");
            entity.Property(e => e.FullNameAr)
                .HasMaxLength(150)
                .HasColumnName("full_name_ar");
            entity.Property(e => e.IsFasting).HasColumnName("is_fasting");
            entity.Property(e => e.NormalRange)
                .HasMaxLength(200)
                .HasColumnName("normal_range");
            entity.Property(e => e.PatientCode)
                .HasMaxLength(30)
                .HasColumnName("patient_code");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.ResultStatus)
                .HasMaxLength(15)
                .HasColumnName("result_status");
            entity.Property(e => e.ResultValue)
                .HasMaxLength(150)
                .HasColumnName("result_value");
            entity.Property(e => e.Sex)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("sex");
            entity.Property(e => e.SnapHighNormal).HasColumnName("snap_high_normal");
            entity.Property(e => e.SnapLowNormal).HasColumnName("snap_low_normal");
            entity.Property(e => e.TestCategory)
                .HasMaxLength(100)
                .HasColumnName("test_category");
            entity.Property(e => e.TestGroup)
                .HasMaxLength(100)
                .HasColumnName("test_group");
            entity.Property(e => e.TestType)
                .HasMaxLength(150)
                .HasColumnName("test_type");
            entity.Property(e => e.Unit)
                .HasMaxLength(30)
                .HasColumnName("unit");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitDate)
                .HasPrecision(0)
                .HasColumnName("visit_date");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
        });

        modelBuilder.Entity<VPendingTest>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_PendingTests");

            entity.Property(e => e.CurrentStage)
                .HasMaxLength(15)
                .HasColumnName("current_stage");
            entity.Property(e => e.ExternalLab)
                .HasMaxLength(150)
                .HasColumnName("external_lab");
            entity.Property(e => e.IsOutsourced).HasColumnName("is_outsourced");
            entity.Property(e => e.LastStageAt)
                .HasPrecision(0)
                .HasColumnName("last_stage_at");
            entity.Property(e => e.LastStageBy)
                .HasMaxLength(100)
                .HasColumnName("last_stage_by");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.SpecialType)
                .HasMaxLength(20)
                .HasColumnName("special_type");
            entity.Property(e => e.TestName)
                .HasMaxLength(150)
                .HasColumnName("test_name");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitDate)
                .HasPrecision(0)
                .HasColumnName("visit_date");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");
        });

        modelBuilder.Entity<VReferralCommissionReport>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_ReferralCommissionReport");

            entity.Property(e => e.CommissionDue).HasColumnName("commission_due");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.ReferralId).HasColumnName("referral_id");
            entity.Property(e => e.ReferralName)
                .HasMaxLength(171)
                .HasColumnName("referral_name");
            entity.Property(e => e.SourceType)
                .HasMaxLength(20)
                .HasColumnName("source_type");
            entity.Property(e => e.TotalPaid).HasColumnName("total_paid");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitDate)
                .HasPrecision(0)
                .HasColumnName("visit_date");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.VisitTotal).HasColumnName("visit_total");
        });

        modelBuilder.Entity<VResultAuditTrail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_ResultAuditTrail");

            entity.Property(e => e.Action)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("action");
            entity.Property(e => e.AuditId).HasColumnName("audit_id");
            entity.Property(e => e.ChangedAt)
                .HasPrecision(3)
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedByName)
                .HasMaxLength(100)
                .HasColumnName("changed_by_name");
            entity.Property(e => e.ComponentName)
                .HasMaxLength(150)
                .HasColumnName("component_name");
            entity.Property(e => e.FieldName)
                .HasMaxLength(100)
                .HasColumnName("field_name");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.PatientName)
                .HasMaxLength(150)
                .HasColumnName("patient_name");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.TestType)
                .HasMaxLength(150)
                .HasColumnName("test_type");
            entity.Property(e => e.VisitCode)
                .HasMaxLength(30)
                .HasColumnName("visit_code");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
        });

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

        modelBuilder.Entity<Visit>(entity =>
        {
            entity.HasKey(e => e.VisitId).HasName("PK__Visit__375A75E1798EC5C4");

            entity.ToTable("Visit", tb =>
                {
                    tb.HasCheckConstraint("CK_Visit_DiscountExclusivity", "NOT (discount_amount > 0 AND discount_percent > 0)");
                });

            entity.HasIndex(e => e.CompanyId, "IX_Visit_Company");

            entity.HasIndex(e => e.VisitDate, "IX_Visit_Date");

            entity.HasIndex(e => e.PatientId, "IX_Visit_Patient");

            entity.HasIndex(e => e.ReferralId, "IX_Visit_Referral");

            entity.HasIndex(e => new { e.VisitStatus, e.PaymentStatus }, "IX_Visit_Status");

            entity.HasIndex(e => e.VisitCode, "UQ__Visit__6B282A41CE8E7529").IsUnique();

            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.BalanceDue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance_due");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.DiscountPercent)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("discount_percent");
            entity.Property(e => e.ExpectedReady)
                .HasPrecision(0)
                .HasColumnName("expected_ready");
            entity.Property(e => e.IsFasting).HasColumnName("is_fasting");
            entity.Property(e => e.FastingHours).HasColumnName("fasting_hours");
            entity.Property(e => e.IsPregnant).HasColumnName("is_pregnant");
            entity.Property(e => e.TakenOutsideLab)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("taken_outside_lab");
            entity.Property(e => e.OutsideUrine)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("outside_urine");
            entity.Property(e => e.OutsideStool)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("outside_stool");
            entity.Property(e => e.OutsideBlood)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("outside_blood");
            entity.Property(e => e.OutsideSemen)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("outside_semen");
            entity.Property(e => e.OutsideCsf)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("outside_csf");
            entity.Property(e => e.HasDiabetes)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_diabetes");
            entity.Property(e => e.HasAnemia)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_anemia");
            entity.Property(e => e.HasBleedingDisorder)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_bleeding_disorder");
            entity.Property(e => e.HasThyroid)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_thyroid");
            entity.Property(e => e.HasJointDisease)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_joint_disease");
            entity.Property(e => e.HasViralInfection)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_viral_infection");
            entity.Property(e => e.OnAnticoagulant)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("on_anticoagulant");
            entity.Property(e => e.HasHypertension)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_hypertension");
            entity.Property(e => e.HasLiverDisease)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_liver_disease");
            entity.Property(e => e.HasKidneyDisease)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_kidney_disease");
            entity.Property(e => e.HasLupus)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("has_lupus");
            entity.Property(e => e.HadXrayContrast)
                .HasColumnType("bit")
                .HasDefaultValue(false)
                .HasColumnName("had_xray_contrast");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.PaymentStatus)
                .HasConversion(v => v.ToString(), v => (PaymentStatus)Enum.Parse(typeof(PaymentStatus), v ?? "Pending", true))
                .HasMaxLength(30)
                .HasDefaultValue(PaymentStatus.Pending)
                .HasColumnName("payment_status");
            entity.Property(e => e.ReceptionistId).HasColumnName("receptionist_id");
            entity.Property(e => e.ReferralId).HasColumnName("referral_id");
            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.TotalAfterDiscount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_after_discount");
            entity.Property(e => e.TotalPaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_paid");
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
                .HasConversion(v => v.ToString(), v => (VisitStatus)Enum.Parse(typeof(VisitStatus), v ?? "Open", true))
                .HasMaxLength(30)
                .HasDefaultValue(VisitStatus.Open)
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
                .HasConversion(v => v.ToString(), v => (TestStage)Enum.Parse(typeof(TestStage), v ?? "Pending", true))
                .HasMaxLength(30)
                .HasDefaultValue(TestStage.Pending)
                .HasColumnName("current_stage");
            entity.Property(e => e.ExternalLabId).HasColumnName("external_lab_id");
            entity.Property(e => e.IsOutsourced).HasColumnName("is_outsourced");
            entity.Property(e => e.OutsourceCost)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("outsource_cost");
            entity.Property(e => e.OutsourceResultReceivedAt)
                .HasPrecision(0)
                .HasColumnName("outsource_result_received_at");
            entity.Property(e => e.OutsourceSentAt)
                .HasPrecision(0)
                .HasColumnName("outsource_sent_at");
            entity.Property(e => e.OutsourceSentBy).HasColumnName("outsource_sent_by");
            entity.Property(e => e.PriceCharged)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price_charged");
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

            entity.Property(e => e.IsPrinted)
                .HasDefaultValue(false)
                .HasColumnName("is_printed");
            entity.Property(e => e.PrintedAt)
                .HasPrecision(0)
                .HasColumnName("printed_at");
            entity.Property(e => e.PrintedBy)
                .HasColumnName("printed_by");
            entity.Property(e => e.IsExported)
                .HasDefaultValue(false)
                .HasColumnName("is_exported");
            entity.Property(e => e.ExportedAt)
                .HasPrecision(0)
                .HasColumnName("exported_at");
            entity.Property(e => e.ExportedBy)
                .HasColumnName("exported_by");

            entity.HasOne(d => d.PrintedByNavigation).WithMany()
                .HasForeignKey(d => d.PrintedBy)
                .HasConstraintName("FK_VT_PrintedBy");
            entity.HasOne(d => d.ExportedByNavigation).WithMany()
                .HasForeignKey(d => d.ExportedBy)
                .HasConstraintName("FK_VT_ExportedBy");
        });

        // V4.0 New Entity Configurations
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK_Attendance");

            entity.ToTable("Attendance");

            entity.Property(e => e.AttendanceId).HasColumnName("attendance_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.ClockIn).HasColumnName("clock_in");
            entity.Property(e => e.ClockOut).HasColumnName("clock_out");
            entity.Property(e => e.LateMinutes).HasColumnName("late_minutes");
            entity.Property(e => e.AbsenceStatus).HasColumnName("absence_status");
            entity.Property(e => e.AttendanceDate).HasColumnName("attendance_date");

            entity.HasOne(d => d.Staff).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Attendance_Staff");

            entity.HasOne(d => d.Shift).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK_Attendance_WorkShift");
        });

        modelBuilder.Entity<AntibioticCatalog>(entity =>
        {
            entity.HasKey(e => e.AntibioticId).HasName("PK_AntibioticCatalog");

            entity.ToTable("AntibioticCatalog");

            entity.Property(e => e.AntibioticId).HasColumnName("antibiotic_id");
            entity.Property(e => e.AntibioticName).HasColumnName("antibiotic_name");
            entity.Property(e => e.AntibioticClass).HasColumnName("antibiotic_class");
            entity.Property(e => e.IsSafePregnancy).HasColumnName("is_safe_pregnancy");
            entity.Property(e => e.IsSafeChildren).HasColumnName("is_safe_children");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

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

        modelBuilder.Entity<ExternalShipment>(entity =>
        {
            entity.HasKey(e => e.ShipmentId).HasName("PK_ExternalShipment");

            entity.ToTable("ExternalShipment");

            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.ExternalLabId).HasColumnName("external_lab_id");
            entity.Property(e => e.ShipmentDate).HasColumnName("shipment_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.TrackingNumber).HasColumnName("tracking_number");

            entity.HasOne(d => d.ExternalLab).WithMany(p => p.ExternalShipments)
                .HasForeignKey(d => d.ExternalLabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalShipment_ExternalLab");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ExternalShipments)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_ExternalShipment_Staff");
        });

        modelBuilder.Entity<ExternalShipmentItem>(entity =>
        {
            entity.HasKey(e => e.ShipmentItemId).HasName("PK_ExternalShipmentItem");

            entity.ToTable("ExternalShipmentItem");

            entity.Property(e => e.ShipmentItemId).HasColumnName("shipment_item_id");
            entity.Property(e => e.ShipmentId).HasColumnName("shipment_id");
            entity.Property(e => e.VisitTestId).HasColumnName("visit_test_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Notes).HasColumnName("notes");

            entity.HasOne(d => d.Shipment).WithMany(p => p.ExternalShipmentItems)
                .HasForeignKey(d => d.ShipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalShipmentItem_ExternalShipment");

            entity.HasOne(d => d.VisitTest).WithMany(p => p.ExternalShipmentItems)
                .HasForeignKey(d => d.VisitTestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExternalShipmentItem_VisitTest");
        });

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

        modelBuilder.Entity<WorkShift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK_WorkShift");

            entity.ToTable("WorkShift");

            entity.Property(e => e.ShiftId).HasColumnName("shift_id");
            entity.Property(e => e.ShiftName).HasColumnName("shift_name");
            entity.Property(e => e.ClockInTime).HasColumnName("clock_in_time");
            entity.Property(e => e.ClockOutTime).HasColumnName("clock_out_time");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
        });

        modelBuilder.Entity<ReceiptPrintLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_ReceiptPrintLog");

            entity.ToTable("ReceiptPrintLog");

            entity.Property(e => e.LogId).HasColumnName("log_id");
            entity.Property(e => e.VisitId).HasColumnName("visit_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.PrintedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("printed_at");
            entity.Property(e => e.Format)
                .HasMaxLength(10)
                .HasColumnName("format");
            entity.Property(e => e.ShowBreakdown).HasColumnName("show_breakdown");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.DiscountAmount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("discount_amount");
            entity.Property(e => e.TotalAfterDiscount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_after_discount");
            entity.Property(e => e.TotalPaid)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_paid");
            entity.Property(e => e.BalanceDue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance_due");

            entity.HasOne(d => d.Visit).WithMany()
                .HasForeignKey(d => d.VisitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptPrintLog_Visit");

            entity.HasOne(d => d.Staff).WithMany()
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReceiptPrintLog_Staff");

            entity.HasIndex(e => e.VisitId, "IX_ReceiptPrintLog_VisitId");
            entity.HasIndex(e => e.PrintedAt, "IX_ReceiptPrintLog_PrintedAt");
        });

        // Update existing entities for V4.0 compatibility
        modelBuilder.Entity<ReferralSource>(entity =>
        {
            entity.Property(e => e.SchemeId).HasColumnName("scheme_id");

            entity.HasOne(d => d.Scheme).WithMany(p => p.ReferralSources)
                .HasForeignKey(d => d.SchemeId)
                .HasConstraintName("FK_ReferralSource_PriceScheme");
        });

        modelBuilder.Entity<OrganismAntibiotic>(entity =>
        {
            entity.Property(e => e.AntibioticCatalogId).HasColumnName("antibiotic_catalog_id");

            entity.HasOne(d => d.Antibiotic).WithMany(p => p.OrganismAntibiotics)
                .HasForeignKey(d => d.AntibioticCatalogId)
                .HasConstraintName("FK_OrganismAntibiotic_AntibioticCatalog");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
