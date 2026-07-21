using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using FinalLabSystem.Infrastructure.Navigation;
using FinalLabSystem.Infrastructure.Session;
using FinalLabSystem.Models.DTOs;
using FinalLabSystem.Services.Interfaces;
using FinalLabSystem.ViewModels.Patients;

namespace FinalLabSystem.Services.Implementations;

public sealed class ResultEntryDialogService : IResultEntryDialogService
{
    private readonly IServiceProvider _serviceProvider;

    public ResultEntryDialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                                ObservableCollection<TestComponentResultDto> components,
                                int patientAgeDays, string patientGender, bool isPregnant)
    {
        var tcs = new TaskCompletionSource<bool>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var routineResultService = (IRoutineResultService)_serviceProvider.GetService(typeof(IRoutineResultService))!;
            var visitService = (IVisitService)_serviceProvider.GetService(typeof(IVisitService))!;
            var auditService = (IAuditService)_serviceProvider.GetService(typeof(IAuditService))!;
            var currentUserSession = (ICurrentUserSession)_serviceProvider.GetService(typeof(ICurrentUserSession))!;
            var dialogService = (IDialogService)_serviceProvider.GetService(typeof(IDialogService))!;
            var commentTemplateService = (IReportCommentTemplateService)_serviceProvider.GetService(typeof(IReportCommentTemplateService))!;
            var printService = (IPrintService)_serviceProvider.GetService(typeof(IPrintService))!;
            var reportingService = (IReportingService)_serviceProvider.GetService(typeof(IReportingService))!;
            var navigationService = (INavigationService)_serviceProvider.GetService(typeof(INavigationService))!;

            var vm = new ResultEntryViewModel(
                routineResultService,
                visitService,
                auditService,
                currentUserSession,
                dialogService,
                commentTemplateService,
                printService,
                reportingService,
                navigationService,
                visitTestId,
                patientId,
                testTypeName,
                components,
                patientAgeDays,
                patientGender,
                isPregnant);

            var window = new Views.Patients.ResultEntryWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.RequestClose = () =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsLoaded)
                    {
                        window.DialogResult = tcs.Task.IsCompleted ? null : false;
                        window.Close();
                    }
                });
            };

            vm.SaveCompleted += (_, _) =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    tcs.TrySetResult(true);
                });
            };

            window.Closed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(false);
            };

            window.ShowDialog();
        });

        return tcs.Task;
    }

    public Task<bool> OpenCultureAsync(int visitTestId, int patientId,
                                       bool isPregnant, int patientAgeDays, string patientGender)
    {
        var tcs = new TaskCompletionSource<bool>();

        Application.Current.Dispatcher.Invoke(() =>
        {
            var cultureService = (ICultureResultService)_serviceProvider.GetService(typeof(ICultureResultService))!;
            var dialogService = (IDialogService)_serviceProvider.GetService(typeof(IDialogService))!;
            var context = (Data.FinalLabDbContext)_serviceProvider.GetService(typeof(Data.FinalLabDbContext))!;

            var vm = new CultureEntryViewModel(
                cultureService,
                dialogService,
                context,
                visitTestId,
                patientId,
                isPregnant,
                patientAgeDays);

            var window = new Views.Patients.CultureEntryWindow
            {
                DataContext = vm,
                Owner = Application.Current.MainWindow
            };

            vm.RequestClose = () =>
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsLoaded)
                    {
                        window.DialogResult = tcs.Task.IsCompleted ? null : false;
                        window.Close();
                    }
                });
            };

            window.Closed += (_, _) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.TrySetResult(false);
            };

            window.Loaded += async (_, _) =>
            {
                await vm.LoadAsync();
            };

            window.ShowDialog();
        });

        return tcs.Task;
    }
}
