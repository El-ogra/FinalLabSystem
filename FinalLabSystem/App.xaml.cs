using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.Views;
using FinalLabSystem.Views.Patients;
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
        navigation.RegisterWindow<PatientRegistrationViewModel, PatientRegistrationWindow>();
        navigation.RegisterWindow<TestResultsViewModel, TestResultsWindow>();
        navigation.RegisterWindow<DeliveryViewModel, DeliveryWindow>();
        navigation.RegisterWindow<PatientSearchViewModel, PatientSearchWindow>();

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
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<ITestCatalogService, TestCatalogService>();
        services.AddScoped<ISampleTrackingService, SampleTrackingService>();

        services.AddSingleton<IUserSettingsService, JsonUserSettingsService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddSingleton<INavigationService, NavigationService>();

        services.AddTransient<LoginViewModel>();
        services.AddTransient<LoginView>();
        services.AddTransient<LoginWindow>();

        services.AddTransient<FirstRunSetupViewModel>();
        services.AddTransient<FirstRunSetupView>();
        services.AddTransient<FirstRunSetupWindow>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<MainWindow>();

        services.AddTransient<PatientRegistrationViewModel>();
        services.AddTransient<PatientInfoViewModel>();
        services.AddTransient<ReferralViewModel>();
        services.AddTransient<MedicalHistoryViewModel>();
        services.AddTransient<TestSelectionViewModel>();
        services.AddTransient<FinancialViewModel>();
        services.AddTransient<BarcodeDialogViewModel>();
        services.AddTransient<ReceiptDialogViewModel>();
        services.AddTransient<TestResultsViewModel>();
        services.AddTransient<DeliveryViewModel>();
        services.AddTransient<PatientSearchViewModel>();

        services.AddTransient<PatientRegistrationWindow>();
        services.AddTransient<TestResultsWindow>();
        services.AddTransient<DeliveryWindow>();
        services.AddTransient<PatientSearchWindow>();
        services.AddTransient<BarcodeDialog>();
        services.AddTransient<ReceiptDialog>();
    }
}
