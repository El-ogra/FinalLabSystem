using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string Username { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public string? DisplayNameAr { get; set; }

    public string PasswordHash { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public bool IsActive { get; set; }

    public string? JobTitle { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public double DiscountLimit { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<CrossMatchTest> CrossMatchTests { get; set; } = new List<CrossMatchTest>();

    public virtual ICollection<LabSetting> LabSettings { get; set; } = new List<LabSetting>();

    public virtual ICollection<MicrobiologyCulture> MicrobiologyCultureInoculatedByNavigations { get; set; } = new List<MicrobiologyCulture>();

    public virtual ICollection<MicrobiologyCulture> MicrobiologyCultureReadByNavigations { get; set; } = new List<MicrobiologyCulture>();

    public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplateCreatedByNavigations { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<ReportCommentTemplate> ReportCommentTemplateModifiedByNavigations { get; set; } = new List<ReportCommentTemplate>();

    public virtual ICollection<SampleTube> SampleTubeCollectedByNavigations { get; set; } = new List<SampleTube>();

    public virtual ICollection<SampleTube> SampleTubePrintedByNavigations { get; set; } = new List<SampleTube>();

    public virtual ICollection<SemenAnalysis> SemenAnalyses { get; set; } = new List<SemenAnalysis>();

    public virtual ICollection<StaffPermission> StaffPermissionGrantedByNavigations { get; set; } = new List<StaffPermission>();

    public virtual ICollection<StaffPermission> StaffPermissionStaffs { get; set; } = new List<StaffPermission>();

    public virtual ICollection<TestResult> TestResultEnteredByNavigations { get; set; } = new List<TestResult>();

    public virtual ICollection<TestResult> TestResultLastModifiedByNavigations { get; set; } = new List<TestResult>();

    public virtual ICollection<TestWorkflow> TestWorkflows { get; set; } = new List<TestWorkflow>();

    public virtual ICollection<VisitTest> VisitTestAddedByNavigations { get; set; } = new List<VisitTest>();

    public virtual ICollection<VisitTest> VisitTestOutsourceSentByNavigations { get; set; } = new List<VisitTest>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();

    // V4.0 New Navigation Properties
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<Attendance> AttendanceCreatedByNavigations { get; set; } = new List<Attendance>();

    public virtual ICollection<ContractInvoice> ContractInvoices { get; set; } = new List<ContractInvoice>();

    public virtual ICollection<ExternalShipment> ExternalShipments { get; set; } = new List<ExternalShipment>();

    public virtual ICollection<PatientMedicalHistory> PatientMedicalHistories { get; set; } = new List<PatientMedicalHistory>();

    public virtual ICollection<TestProfile> TestProfiles { get; set; } = new List<TestProfile>();

    public virtual ICollection<VisitCharge> VisitCharges { get; set; } = new List<VisitCharge>();
}
