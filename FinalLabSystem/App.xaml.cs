using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.ViewModels.Settings;
using FinalLabSystem.Views;
using FinalLabSystem.Views.Patients;
using FinalLabSystem.Views.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace FinalLabSystem;

public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "finallabsystem.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        DispatcherUnhandledException += (s, e) =>
        {
            Log.Fatal(e.Exception, "Unhandled dispatcher exception");
            MessageBox.Show(
                "حدث خطأ غير متوقع. تم تسجيل التفاصيل في ملف السجل.",
                "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Error(e.Exception, "Unobserved task exception");
            e.SetObserved();
        };

        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            Log.Fatal(e.ExceptionObject as Exception, "AppDomain unhandled exception — app will terminate");
            Log.CloseAndFlush();
        };

        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found in configuration. Check appsettings.json.");

            var services = new ServiceCollection();
            ConfigureServices(services, connectionString);
            ServiceProvider = services.BuildServiceProvider();

            var appSettings = configuration.GetSection("AppSettings");
            var timeoutMinutes = appSettings.GetValue<int>("IdleTimeoutMinutes", 15);
            var userSession = ServiceProvider.GetRequiredService<ICurrentUserSession>();
            userSession.IdleTimeoutMinutes = timeoutMinutes;

            InputManager.Current.PreProcessInput += (s, args) =>
            {
                if (args.StagingItem.Input is MouseEventArgs or KeyEventArgs)
                    userSession.ResetIdleTimer();
            };

            var navigation = ServiceProvider.GetRequiredService<INavigationService>();
            navigation.RegisterWindow<PatientRegistrationViewModel, PatientRegistrationWindow>();
            navigation.RegisterWindow<TestResultsViewModel, TestResultsWindow>();
            navigation.RegisterWindow<DeliveryViewModel, DeliveryWindow>();
            navigation.RegisterWindow<PatientSearchViewModel, PatientSearchWindow>();
            navigation.RegisterWindow<TestDataManagementViewModel, TestDataManagementWindow>();
            navigation.RegisterWindow<NormalRangeWindowViewModel, NormalRangesWindow>();
            navigation.RegisterWindow<CategoriesGroupsViewModel, CategoriesGroupsWindow>();

            using (var scope = ServiceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<FinalLabDbContext>();
                var seeder = scope.ServiceProvider.GetRequiredService<ITestCatalogSeeder>();

                await db.Database.MigrateAsync();
                await seeder.SeedAsync();
            }

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
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error during application startup");
            Log.CloseAndFlush();
            MessageBox.Show(
                $"فشل تشغيل البرنامج:\n{ex.Message}",
                "خطأ في التشغيل", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FinalLabDbContext>(options =>
            options.UseSqlServer(connectionString),
            ServiceLifetime.Scoped,
            ServiceLifetime.Scoped);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<ITestCatalogService, TestCatalogService>();
        services.AddScoped<ISampleTrackingService, SampleTrackingService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ITestCatalogSeeder, TestCatalogSeeder>();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        services.AddSingleton<IUserSettingsService, JsonUserSettingsService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IPrintService, NullPrintService>();
        services.AddSingleton<ILabelPrintService, WpfLabelPrintService>();

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
        services.AddTransient<TodayPatientsDialogViewModel>();
        services.AddTransient<TodayPatientsDialog>();

        services.AddTransient<TestDataManagementViewModel>();
        services.AddTransient<TestListViewModel>();
        services.AddTransient<TestDetailViewModel>();
        services.AddTransient<NormalRangeWindowViewModel>();
        services.AddTransient<NormalRangeListViewModel>();
        services.AddTransient<NormalRangeDetailViewModel>();
        services.AddTransient<CategoriesGroupsViewModel>();
        services.AddTransient<CategoryListViewModel>();
        services.AddTransient<CategoryDetailViewModel>();
        services.AddTransient<GroupListViewModel>();
        services.AddTransient<GroupDetailViewModel>();
        services.AddTransient<TestDataManagementWindow>();
        services.AddTransient<NormalRangesWindow>();
        services.AddTransient<CategoriesGroupsWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
