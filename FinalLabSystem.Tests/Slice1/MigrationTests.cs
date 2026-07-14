using System;
using System.Linq;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FinalLabSystem.Tests.Slice1;

public class MigrationTests
{
    [Fact]
    public async Task T10_Migration_AddPatientBarcodeAndLabId_AppliesWithoutDataLoss()
    {
        var options = new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new FinalLabDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var patient = new Patient
        {
            PatientId = 1,
            PatientCode = "P0001",
            FullNameAr = "أحمد",
            Sex = "M",
            CreatedAt = DateTime.UtcNow
        };
        context.Patients.Add(patient);
        await context.SaveChangesAsync();

        var labSetting = context.LabSettings.FirstOrDefault();
        if (labSetting == null)
        {
            labSetting = new LabSetting { SettingKey = "Main" };
            context.LabSettings.Add(labSetting);
            await context.SaveChangesAsync();
        }

        patient.LabId = "5123070410012";
        await context.SaveChangesAsync();

        var updatedPatient = await context.Patients.FindAsync(1);
        Assert.NotNull(updatedPatient!.LabId);
        Assert.Equal("5123070410012", updatedPatient.LabId);

        var barcode = new PatientBarcode
        {
            PatientId = 1,
            CodeType = BarcodeCodeType.Case,
            BarcodeValue = "1260714100005",
            IssueDate = DateTime.Now,
            SortOrdinal = 1
        };
        context.PatientBarcodes.Add(barcode);
        await context.SaveChangesAsync();

        var savedBarcode = await context.PatientBarcodes.FirstOrDefaultAsync(b => b.BarcodeValue == "1260714100005");
        Assert.NotNull(savedBarcode);
        Assert.Equal(BarcodeCodeType.Case, savedBarcode.CodeType);
        Assert.Equal(1, savedBarcode.PatientId);
    }
}
