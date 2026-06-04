using System;
using System.Collections.Generic;

namespace FinalLabSystem.Models;

public partial class SemenAnalysis
{
    public int SemenId { get; set; }

    public int VisitTestId { get; set; }

    public string? CollectionMethod { get; set; }

    public byte? AbstinenceDays { get; set; }

    public DateTime? CollectionTime { get; set; }

    public DateTime? AnalysisTime { get; set; }

    public short? LiquefactionTimeMin { get; set; }

    public double? VolumeMl { get; set; }

    public string? Appearance { get; set; }

    public double? PhValue { get; set; }

    public string? Viscosity { get; set; }

    public double? ProgressiveAPct { get; set; }

    public double? ProgressiveBPct { get; set; }

    public double? NonProgressiveCPct { get; set; }

    public double? ImmotileDPct { get; set; }

    public double? TotalMotilityPct { get; set; }

    public double? ProgressiveMotilityPct { get; set; }

    public double? ConcentrationMPerMl { get; set; }

    public double? TotalCountM { get; set; }

    public double? VitalityPct { get; set; }

    public double? NormalMorphologyPct { get; set; }

    public double? HeadDefectsPct { get; set; }

    public double? MidpieceDefectsPct { get; set; }

    public double? TailDefectsPct { get; set; }

    public string? WbcPerHpf { get; set; }

    public string? RbcPerHpf { get; set; }

    public string? EpithelialCells { get; set; }

    public bool BacteriaNoted { get; set; }

    public string? Agglutination { get; set; }

    public string? Interpretation { get; set; }

    public int? AnalyzedBy { get; set; }

    public string? AnalysisNotes { get; set; }

    public virtual Staff? AnalyzedByNavigation { get; set; }

    public virtual VisitTest VisitTest { get; set; } = null!;
}
