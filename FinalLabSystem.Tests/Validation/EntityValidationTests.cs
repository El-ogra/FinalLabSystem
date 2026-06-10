using System.ComponentModel.DataAnnotations;
using FinalLabSystem.Models;

namespace FinalLabSystem.Tests.Validation;

public class EntityValidationTests
{
    [Fact]
    public void Patient_WithEmptyName_FailsDataAnnotationValidation()
    {
        var patient = new Patient
        {
            PatientCode = "P001",
            FullNameAr = "",
            Sex = "M",
            PatientType = "Individual",
            CreatedAt = DateTime.UtcNow
        };

        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(patient);
        var isValid = Validator.TryValidateObject(patient, ctx, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Patient.FullNameAr)));
    }

    [Fact]
    public void TestType_WithNegativePrice_FailsDataAnnotationValidation()
    {
        var testType = new TestType
        {
            TypeCode = "T001",
            TypeNameEn = "Test",
            DefaultPrice = -1,
            GroupId = 1,
            SortOrder = 1,
            TurnaroundHours = 24,
            IsActive = true
        };

        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(testType);
        var isValid = Validator.TryValidateObject(testType, ctx, results, true);

        Assert.False(isValid);
    }

    [Fact]
    public void Staff_WithEmptyUsername_FailsDataAnnotationValidation()
    {
        var staff = new Staff
        {
            Username = "",
            DisplayName = "Test",
            PasswordHash = "hash",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var results = new List<ValidationResult>();
        var ctx = new ValidationContext(staff);
        var isValid = Validator.TryValidateObject(staff, ctx, results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Staff.Username)));
    }
}
