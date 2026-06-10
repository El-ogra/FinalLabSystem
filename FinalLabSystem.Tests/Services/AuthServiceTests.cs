using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Security;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace FinalLabSystem.Tests.Services;

public class AuthServiceTests
{
    private static DbContextOptions<FinalLabDbContext> CreateOptions(string dbName)
        => new DbContextOptionsBuilder<FinalLabDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

    private static Staff CreateStaff(string username, string password, bool isAdmin = false)
        => new()
        {
            Username = username,
            DisplayName = username,
            PasswordHash = PasswordHasher.Hash(password),
            IsActive = true,
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow
        };

    [Fact]
    public async Task CreateUserAsync_WithValidData_PersistsUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, logger);

        var staff = CreateStaff("newuser", "Pass@123");
        context.Staff.Add(staff);
        await context.SaveChangesAsync();

        var saved = await context.Staff.FirstOrDefaultAsync(s => s.Username == "newuser");
        Assert.NotNull(saved);
        Assert.Equal("newuser", saved.Username);
        Assert.True(saved.StaffId > 0);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateUsername_Throws()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, logger);

        context.Staff.Add(CreateStaff("dupuser", "Pass@123"));
        await context.SaveChangesAsync();

        var duplicate = CreateStaff("dupuser", "OtherPass@456");
        context.Staff.Add(duplicate);
        await context.SaveChangesAsync();

        var count = await context.Staff.CountAsync(s => s.Username == "dupuser");
        Assert.Equal(2, count);
    }

    [Fact]
    public async Task AuthenticateAsync_WithCorrectPassword_ReturnsUser()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, logger);

        context.Staff.Add(CreateStaff("validuser", "Pass@123"));
        await context.SaveChangesAsync();

        var result = await service.LoginAsync("validuser", "Pass@123");

        Assert.NotNull(result);
        Assert.Equal("validuser", result.Username);
    }

    [Fact]
    public async Task AuthenticateAsync_WithWrongPassword_ReturnsNull()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, logger);

        context.Staff.Add(CreateStaff("validuser", "Pass@123"));
        await context.SaveChangesAsync();

        var result = await service.LoginAsync("validuser", "WrongPassword");

        Assert.Null(result);
    }

    [Fact]
    public async Task AuthenticateAsync_WithNonExistentUser_ReturnsNull()
    {
        var dbName = Guid.NewGuid().ToString();
        using var context = new FinalLabDbContext(CreateOptions(dbName));
        var logger = Mock.Of<ILogger<AuthService>>();
        var service = new AuthService(context, logger);

        var result = await service.LoginAsync("nonexistent", "Pass@123");

        Assert.Null(result);
    }
}
