using System.Linq;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models;

namespace FinalLabSystem.ViewModels.Settings;

public sealed class TestRowViewModel : ViewModelBase
{
    private string _outsideLabName = string.Empty;
    private decimal? _outsideCostPrice;
    private string _barcode = string.Empty;

    public TestRowViewModel(TestType testType)
    {
        TestType = testType;
        _outsideLabName = testType.OutsideLabName ?? string.Empty;
        _outsideCostPrice = testType.OutsideCostPrice;
        _barcode = testType.BarcodeName ?? string.Empty;
    }

    public TestType TestType { get; }

    public int TesttypeId => TestType.TesttypeId;

    public short SortOrder => TestType.SortOrder;

    public string TypeCode => TestType.TypeCode;

    public string TypeNameEn => TestType.TypeNameEn;

    public string? TypeNameAr => TestType.TypeNameAr;

    public string DisplayName => string.IsNullOrWhiteSpace(TypeNameAr) ? TypeNameEn : TypeNameAr!;

    public string GroupNameEn => TestType.Group?.GroupNameEn ?? string.Empty;

    public string GroupNameAr => TestType.Group?.GroupNameAr ?? TestType.Group?.GroupNameEn ?? string.Empty;

    public double PatientPrice => TestType.TestTypePrices
        .FirstOrDefault(p => p.Scheme.SchemeName == "Patient Price")?.Price ?? TestType.DefaultPrice;

    public double LabToLabPrice => TestType.TestTypePrices
        .FirstOrDefault(p => p.Scheme.SchemeName == "Lab-to-Lab Price")?.Price ?? 0d;

    public int TubeCount => TestType.TestTypeSampleTubes.Count(t => t.IsActive);

    public string OutsideLabName
    {
        get => _outsideLabName;
        set => SetProperty(ref _outsideLabName, value ?? string.Empty);
    }

    public decimal? OutsideCostPrice
    {
        get => _outsideCostPrice;
        set => SetProperty(ref _outsideCostPrice, value);
    }

    public string Barcode
    {
        get => _barcode;
        set => SetProperty(ref _barcode, value ?? string.Empty);
    }
}
