using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Settings;
using Moq;

namespace FinalLabSystem.Tests.ViewModels.Settings;

public class NormalRangeDetailViewModelTests
{
    private readonly Mock<ITestCatalogService> _catalogServiceMock = new();
    private readonly Mock<IDialogService> _dialogServiceMock = new();

    private NormalRangeDetailViewModel CreateVm()
        => new(_catalogServiceMock.Object, _dialogServiceMock.Object);

    private static NormalRange CreateSampleRange(int id = 1, int componentId = 1)
    {
        return new NormalRange
        {
            RangeId = id,
            ComponentId = componentId,
            Sex = "B",
            AgeFromDays = 0,
            AgeToDays = 36500,
            AgeUnit = "Days",
            LowNormal = 0.5,
            HighNormal = 1.5,
            LowCritical = null,
            HighCritical = null,
            NormalRangeText = "0.5 - 1.5",
            FastingState = "A",
            Unit = "mg/dL",
            ForPregnantOnly = false,
            LowFlag = "Low",
            HighFlag = "High",
            LowComment = "Below normal",
            HighComment = "Above normal",
            CriticalRangeText = null,
            CriticalFlag = null,
            CriticalComment = null,
            AgeDescription = null,
            RangeNote = null,
            IsActive = true,
            Version = 1
        };
    }

    [Fact]
    public void Load_WithNullRange_CreatesEmptyWithUnit()
    {
        var vm = CreateVm();

        vm.Load(null, "mg/dL");

        Assert.NotNull(vm.EditableRange);
        Assert.Equal("mg/dL", vm.EditableRange.Unit);
        Assert.Equal("Both", vm.EditableRange.Sex);
        Assert.Equal("A", vm.EditableRange.FastingState);
        Assert.Equal(0, vm.EditableRange.AgeFromDays);
        Assert.Equal(36500, vm.EditableRange.AgeToDays);
        Assert.Equal(RangeSex.B, vm.Sex);
        Assert.False(vm.ForPregnantOnly);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public void Load_WithRange_MapsAllFields()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();

        vm.Load(range, "mg/dL");

        Assert.Equal(range.RangeId, vm.EditableRange.RangeId);
        Assert.Equal(range.ComponentId, vm.EditableRange.ComponentId);
        Assert.Equal(range.Sex, vm.EditableRange.Sex);
        Assert.Equal(range.AgeFromDays, vm.AgeFromDays);
        Assert.Equal(range.AgeToDays, vm.AgeToDays);
        Assert.Equal(range.LowNormal, vm.LowNormal);
        Assert.Equal(range.HighNormal, vm.HighNormal);
        Assert.Equal(range.LowCritical, vm.LowCritical);
        Assert.Equal(range.HighCritical, vm.HighCritical);
        Assert.Equal(range.NormalRangeText, vm.NormalRangeText);
        Assert.Equal(range.FastingState, vm.FastingState);
        Assert.Equal(range.Unit, vm.Unit);
        Assert.Equal(range.AgeUnit, vm.AgeUnit);
        Assert.Equal(range.LowFlag, vm.LowFlag);
        Assert.Equal(range.HighFlag, vm.HighFlag);
        Assert.Equal(range.LowComment, vm.LowComment);
        Assert.Equal(range.HighComment, vm.HighComment);
        Assert.Equal(range.CriticalRangeText, vm.CriticalRangeText);
        Assert.Equal(range.CriticalFlag, vm.CriticalFlag);
        Assert.Equal(range.CriticalComment, vm.CriticalComment);
        Assert.Equal(range.AgeDescription, vm.AgeDescription);
        Assert.Equal(range.RangeNote, vm.RangeNote);
        Assert.Equal(range.ForPregnantOnly ?? false, vm.ForPregnantOnly);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public void Load_SetsSexEnumCorrectly_Both()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        range.Sex = "B";

        vm.Load(range, "mg/dL");

        Assert.Equal(RangeSex.B, vm.Sex);
        Assert.True(vm.IsSexBoth);
        Assert.False(vm.IsSexMale);
        Assert.False(vm.IsSexFemale);
    }

    [Fact]
    public void Load_SetsSexEnumCorrectly_Male()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        range.Sex = "M";

        vm.Load(range, "mg/dL");

        Assert.Equal(RangeSex.M, vm.Sex);
        Assert.True(vm.IsSexMale);
        Assert.False(vm.IsSexBoth);
        Assert.False(vm.IsSexFemale);
    }

    [Fact]
    public void Load_SetsSexEnumCorrectly_Female()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        range.Sex = "F";

        vm.Load(range, "mg/dL");

        Assert.Equal(RangeSex.F, vm.Sex);
        Assert.True(vm.IsSexFemale);
        Assert.False(vm.IsSexBoth);
        Assert.False(vm.IsSexMale);
    }

    [Fact]
    public void Save_WithLowGreaterThanHigh_ShowsWarning()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.LowNormal = 10.0;
        vm.HighNormal = 5.0;

        vm.SaveCommand.Execute(null);

        _dialogServiceMock.Verify(d => d.ShowWarning(It.Is<string>(s => s.Contains("Low Normal")), It.IsAny<string>()), Times.Once);
        _catalogServiceMock.Verify(s => s.SaveRangeAsync(It.IsAny<NormalRange>()), Times.Never);
    }

    [Fact]
    public void Save_WithLowCriticalGreaterThanHighCritical_ShowsWarning()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.LowCritical = 10.0;
        vm.HighCritical = 5.0;

        vm.SaveCommand.Execute(null);

        _dialogServiceMock.Verify(d => d.ShowWarning(It.Is<string>(s => s.Contains("Low Critical")), It.IsAny<string>()), Times.Once);
        _catalogServiceMock.Verify(s => s.SaveRangeAsync(It.IsAny<NormalRange>()), Times.Never);
    }

    [Fact]
    public void Save_WithValidRange_CallsService()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        var savedRange = CreateSampleRange();
        _catalogServiceMock.Setup(s => s.SaveRangeAsync(It.IsAny<NormalRange>())).ReturnsAsync(savedRange);

        vm.SaveCommand.Execute(null);

        _catalogServiceMock.Verify(s => s.SaveRangeAsync(It.IsAny<NormalRange>()), Times.Once);
    }

    [Fact]
    public void SexRoundTrip_BMapsToBoth()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        range.Sex = "B";
        var savedRange = CreateSampleRange();
        savedRange.Sex = "B";
        _catalogServiceMock.Setup(s => s.SaveRangeAsync(It.IsAny<NormalRange>())).ReturnsAsync(savedRange);

        vm.Load(range, "mg/dL");
        Assert.Equal(RangeSex.B, vm.Sex);

        vm.SaveCommand.Execute(null);

        Assert.Equal("Both", vm.EditableRange.Sex);
    }

    [Fact]
    public void SexRoundTrip_MMapsToMale()
    {
        var vm = CreateVm();
        vm.Sex = RangeSex.M;
        Assert.Equal(RangeSex.M, vm.Sex);
        Assert.True(vm.IsSexMale);
    }

    [Fact]
    public void SexRoundTrip_FMapsToFemale()
    {
        var vm = CreateVm();
        vm.Sex = RangeSex.F;
        Assert.Equal(RangeSex.F, vm.Sex);
        Assert.True(vm.IsSexFemale);
    }

    [Fact]
    public void Load_WithForPregnantOnlyTrue_SetsForPregnantOnly()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        range.ForPregnantOnly = true;

        vm.Load(range, "mg/dL");

        Assert.True(vm.ForPregnantOnly);
    }

    [Fact]
    public void Cancel_RevertsToLastLoadedRange()
    {
        var vm = CreateVm();
        var range = CreateSampleRange();
        vm.Load(range, "mg/dL");

        vm.LowNormal = 99.0;

        vm.CancelCommand.Execute(null);

        Assert.Equal(0.5, vm.LowNormal);
        Assert.False(vm.IsDirty);
    }

    [Fact]
    public void AgeUnitOptions_ContainsExpectedValues()
    {
        var vm = CreateVm();

        Assert.Equal(new[] { "Days", "Months", "Years" }, vm.AgeUnitOptions);
    }

    [Fact]
    public void FastingStateAny_SetsStateToA()
    {
        var vm = CreateVm();
        vm.FastingState = "F";
        vm.IsFastingAny = true;
        Assert.Equal("A", vm.FastingState);
    }

    [Fact]
    public void FastingStateFasting_SetsStateToF()
    {
        var vm = CreateVm();
        vm.IsFasting = true;
        Assert.Equal("F", vm.FastingState);
    }

    [Fact]
    public void LowCritical_RoundTrip()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.LowCritical = 2.5;

        Assert.Equal(2.5, vm.EditableRange.LowCritical);
    }

    [Fact]
    public void HighCritical_RoundTrip()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.HighCritical = 20.0;

        Assert.Equal(20.0, vm.EditableRange.HighCritical);
    }

    [Fact]
    public void CriticalRangeText_RoundTrip()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.CriticalRangeText = "Critical: < 2.5 or > 20.0";

        Assert.Equal("Critical: < 2.5 or > 20.0", vm.EditableRange.CriticalRangeText);
    }

    [Fact]
    public void AgeDescription_RoundTrip()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.AgeDescription = "Adults 18-60";

        Assert.Equal("Adults 18-60", vm.EditableRange.AgeDescription);
    }

    [Fact]
    public void RangeNote_RoundTrip()
    {
        var vm = CreateVm();
        vm.Load(CreateSampleRange(), "mg/dL");

        vm.RangeNote = "Repeat test if outside range";

        Assert.Equal("Repeat test if outside range", vm.EditableRange.RangeNote);
    }
}
