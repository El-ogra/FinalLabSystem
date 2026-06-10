using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.DTOs;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class PatientServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

    private static Patient CreatePatient(string code, string name)
        => new()
        {
            PatientCode = code,
            FullNameAr = name,
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task SearchPatientsAsync_WithTerm_ReturnsMatchingPatients()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<PatientService>>();
        var service = new PatientService(context, logger);

        context.Patients.Add(CreatePatient("P001", "أحمد محمد"));
        context.Patients.Add(CreatePatient("P002", "سارة علي"));
        context.Patients.Add(CreatePatient("P003", "محمد أحمد"));
        await context.SaveChangesAsync();

        var result = await service.SearchPatientsAsync("أحمد");

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task SearchPatientsAsync_WithPage2_ReturnsSecondPage()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<PatientService>>();
        var service = new PatientService(context, logger);

        for (int i = 1; i <= 5; i++)
            context.Patients.Add(CreatePatient($"P{i:D4}", $"Patient {i}"));

        await context.SaveChangesAsync();

        var result = await service.SearchPatientsAsync("", page: 2, pageSize: 2);

        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasMore);
    }

    [Fact]
    public async Task SearchPatientsAsync_WithLargePageSize_CapsAt100()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<PatientService>>();
        var service = new PatientService(context, logger);

        for (int i = 1; i <= 150; i++)
            context.Patients.Add(CreatePatient($"P{i:D4}", $"Patient {i}"));

        await context.SaveChangesAsync();

        var result = await service.SearchPatientsAsync("", page: 1, pageSize: 500);

        Assert.NotNull(result);
        Assert.Equal(100, result.Items.Count);
        Assert.Equal(150, result.TotalCount);
        Assert.True(result.HasMore);
    }
}
