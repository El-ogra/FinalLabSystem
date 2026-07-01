using FinalLabSystem.Data;
using FinalLabSystem.Infrastructure;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Infrastructure.Settings;
using FinalLabSystem.Services.Implementations;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels;
using FinalLabSystem.ViewModels.Patients;
using FinalLabSystem.ViewModels.Patients.Delivery;
using FinalLabSystem.ViewModels.Patients.Search;
using FinalLabSystem.ViewModels.Menu;
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
            navigation.RegisterWindow<AuditTrailViewModel, AuditTrailWindow>();
            navigation.RegisterWindow<ResultEntryViewModel, ResultEntryWindow>();
            navigation.RegisterWindow<ReportCommentTemplateViewModel, ReportCommentTemplateWindow>();
            navigation.RegisterWindow<TestProfileWindowViewModel, TestProfileWindow>();
            navigation.RegisterWindow<CompaniesWindowViewModel, CompaniesWindow>();
            navigation.RegisterWindow<PriceSchemeWindowViewModel, PriceSchemeWindow>();
            navigation.RegisterWindow<ContractInvoiceWindowViewModel, ContractInvoiceWindow>();
            navigation.RegisterWindow<ExternalLabsWindowViewModel, ExternalLabsWindow>();
            navigation.RegisterWindow<AttendanceWindowViewModel, AttendanceWindow>();
            navigation.RegisterWindow<CashDrawerWindowViewModel, CashDrawerWindow>();
            navigation.RegisterWindow<InventoryWindowViewModel, InventoryWindow>();
            navigation.RegisterWindow<CommissionReportWindowViewModel, CommissionReportWindow>();
            navigation.RegisterWindow<OutstandingBalanceWindowViewModel, OutstandingBalanceWindow>();

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

        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IFeatureToggleService, FeatureToggleService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPatientService, PatientService>();
        services.AddScoped<IReferralService, ReferralService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IFinancialService, FinancialService>();
        services.AddScoped<ITestCatalogService, TestCatalogService>();
        services.AddScoped<ISampleTrackingService, SampleTrackingService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ITestCatalogSeeder, TestCatalogSeeder>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IRoutineResultService, RoutineResultService>();
        services.AddScoped<IReportingService, ReportingService>();
        services.AddScoped<IResultEditorFactory, DefaultResultEditorFactory>();
        services.AddScoped<IReportCommentTemplateService, ReportCommentTemplateService>();
        services.AddScoped<IReportCommentEngine, ReportCommentEngine>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<TestPricingEngine>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IExternalLabRegistryService, ExternalLabRegistryService>();
        services.AddScoped<IExternalShipmentService, ExternalShipmentService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<ICashDrawerService, CashDrawerService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICommissionReportService, CommissionReportService>();
        services.AddScoped<IOutstandingBalanceReportService, OutstandingBalanceReportService>();
        services.AddScoped<IBackupService, BackupService>();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        services.AddSingleton<IUserSettingsService, JsonUserSettingsService>();
        services.AddSingleton<ICurrentUserSession, CurrentUserSession>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddScoped<IPrintService, WpfFlowDocumentPrintService>();
        services.AddSingleton<ILabelPrintService, WpfLabelPrintService>();
        services.AddSingleton<IAuditTrailDialogService, AuditTrailDialogService>();
        services.AddSingleton<IResultEntryDialogService, ResultEntryDialogService>();
        services.AddSingleton<IBarcodeDialogFactory, BarcodeDialogFactory>();
        services.AddSingleton<IReceiptDialogFactory, ReceiptDialogFactory>();
        services.AddSingleton<INormalRangesWindowFactory, NormalRangesWindowFactory>();
        services.AddSingleton<IPrintPreviewDialogService, PrintPreviewDialogService>();
        services.AddSingleton<IProcessService, ProcessService>();

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
        services.AddTransient<PrintPreviewViewModel>();
        services.AddTransient<TestResultsViewModel>();
        services.AddTransient<DeliveryViewModel>();
        services.AddTransient<PatientSearchViewModel>();

        services.AddTransient<PatientRegistrationWindow>();
        services.AddTransient<TestResultsWindow>();
        services.AddTransient<DeliveryWindow>();
        services.AddTransient<PatientSearchWindow>();
        services.AddTransient<BarcodeDialog>();
        services.AddTransient<ReceiptDialog>();
        services.AddTransient<PrintPreviewWindow>();
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

        services.AddTransient<HomeMenuViewModel>();
        services.AddTransient<PatientsMenuViewModel>();
        services.AddTransient<ResultsMenuViewModel>();
        services.AddTransient<DeliveryMenuViewModel>();
        services.AddTransient<SearchMenuViewModel>();
        services.AddTransient<ExternalSamplesMenuViewModel>();
        services.AddTransient<AccountsMenuViewModel>();
        services.AddTransient<BackupMenuViewModel>();
        services.AddTransient<TestDataMenuViewModel>();
        services.AddTransient<NormalRangesMenuViewModel>();
        services.AddTransient<ReportSettingsMenuViewModel>();
        services.AddTransient<ReportCommentTemplateViewModel>();
        services.AddTransient<ReportCommentTemplateWindow>();
        services.AddTransient<TestProfileWindowViewModel>();
        services.AddTransient<TestProfileWindow>();
        services.AddTransient<SystemSettingsMenuViewModel>();

        services.AddTransient<CompaniesWindowViewModel>();
        services.AddTransient<CompaniesWindow>();

        services.AddTransient<PriceSchemeWindowViewModel>();
        services.AddTransient<PriceSchemeWindow>();

        services.AddTransient<ContractInvoiceWindowViewModel>();
        services.AddTransient<ContractInvoiceWindow>();

        services.AddTransient<ExternalLabsWindowViewModel>();
        services.AddTransient<ExternalLabsWindow>();

        services.AddTransient<AttendanceWindowViewModel>();
        services.AddTransient<AttendanceWindow>();

        services.AddTransient<CashDrawerWindowViewModel>();
        services.AddTransient<CashDrawerWindow>();

        services.AddTransient<InventoryWindowViewModel>();
        services.AddTransient<InventoryWindow>();
        services.AddTransient<StockAdjustmentDialog>();

        services.AddTransient<CommissionReportWindowViewModel>();
        services.AddTransient<CommissionReportWindow>();

        services.AddTransient<OutstandingBalanceWindowViewModel>();
        services.AddTransient<OutstandingBalanceWindow>();

        // Slice 6.3 — Backup UI
        services.AddTransient<BackupRestoreWindowViewModel>();
        services.AddTransient<BackupRestoreWindow>();
        services.AddTransient<BackupPasswordDialog>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
