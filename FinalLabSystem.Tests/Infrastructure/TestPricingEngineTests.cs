using FinalLabSystem.Infrastructure;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using Moq;

namespace FinalLabSystem.Tests.Infrastructure;

public class TestPricingEngineTests
{
    [Fact]
    public async Task ResolvePriceAsync_WithSchemeId_ReturnsSchemePrice()
    {
        var mockPricing = new Mock<IPricingService>();
        mockPricing.Setup(p => p.GetTestPriceAsync(1, 10)).ReturnsAsync(50m);
        var engine = new TestPricingEngine(mockPricing.Object);

        var result = await engine.ResolvePriceAsync(1, 10);

        Assert.Equal(50m, result.Price);
        Assert.False(result.IsFallback);
        Assert.Equal(1, result.TestTypeId);
    }

    [Fact]
    public async Task ResolvePriceAsync_WithNullSchemeId_ReturnsFallback()
    {
        var mockPricing = new Mock<IPricingService>();
        var engine = new TestPricingEngine(mockPricing.Object);

        var result = await engine.ResolvePriceAsync(1, null);

        Assert.Equal(0m, result.Price);
        Assert.True(result.IsFallback);
        mockPricing.Verify(p => p.GetTestPriceAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ResolvePriceAsync_WhenPriceIsZero_ReturnsFallback()
    {
        var mockPricing = new Mock<IPricingService>();
        mockPricing.Setup(p => p.GetTestPriceAsync(1, 10)).ReturnsAsync(0m);
        var engine = new TestPricingEngine(mockPricing.Object);

        var result = await engine.ResolvePriceAsync(1, 10);

        Assert.Equal(0m, result.Price);
        Assert.True(result.IsFallback);
    }

    [Fact]
    public async Task GetPricingSummaryAsync_ReturnsResultsForAllTests()
    {
        var mockPricing = new Mock<IPricingService>();
        mockPricing.Setup(p => p.GetTestPriceAsync(1, 10)).ReturnsAsync(50m);
        mockPricing.Setup(p => p.GetTestPriceAsync(2, 10)).ReturnsAsync(30m);
        mockPricing.Setup(p => p.GetTestPriceAsync(3, 10)).ReturnsAsync(0m);
        var engine = new TestPricingEngine(mockPricing.Object);

        var result = await engine.GetPricingSummaryAsync(new[] { 1, 2, 3 }, 10);

        Assert.Equal(3, result.Count);
        Assert.False(result[0].IsFallback);
        Assert.False(result[1].IsFallback);
        Assert.True(result[2].IsFallback);
    }

    [Fact]
    public async Task GetPricingSummaryAsync_WithNullSchemeId_AllFallback()
    {
        var mockPricing = new Mock<IPricingService>();
        var engine = new TestPricingEngine(mockPricing.Object);

        var result = await engine.GetPricingSummaryAsync(new[] { 1, 2, 3 }, null);

        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.True(r.IsFallback));
        Assert.All(result, r => Assert.Equal(0m, r.Price));
    }

    [Fact]
    public async Task ResolvePriceAsync_PassesCorrectParameters()
    {
        var mockPricing = new Mock<IPricingService>();
        mockPricing.Setup(p => p.GetTestPriceAsync(42, 7)).ReturnsAsync(99.99m);
        var engine = new TestPricingEngine(mockPricing.Object);

        await engine.ResolvePriceAsync(42, 7);

        mockPricing.Verify(p => p.GetTestPriceAsync(42, 7), Times.Once);
    }
}
