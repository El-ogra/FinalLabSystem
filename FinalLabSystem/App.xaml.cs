using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace FinalLabSystem;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        var navigation = ServiceProvider.GetRequiredService<INavigationService>();

        bool hasAdmin;
        using (var scope = ServiceProvider.CreateScope())
        {
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            hasAdmin = await auth.HasAnyAdministratorAsync();
        }

        if (hasAdmin)
            navigation.ShowLogin();
        else
            navigation.ShowFirstRunSetup();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseSqlServer(
                "Server=.\\SQLEXPRESS;Database=FinalLab;Trusted_Connection=True;TrustServerCertificate=True;"));

        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<IUserSettingsService, JsonUserSettingsService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<LoginWindow>();

        services.AddTransient<FirstRunSetupViewModel>();
        services.AddTransient<FirstRunSetupView>();
        services.AddTransient<FirstRunSetupWindow>();

        services.AddTransient<MainWindow>();
    }
}
