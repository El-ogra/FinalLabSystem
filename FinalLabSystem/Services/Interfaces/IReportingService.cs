using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportingService
{
    Task<List<VPendingTest>> GetPendingWorksheetsAsync();
    Task<List<VOutstandingBalance>> GetDefaultersListAsync();
    Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end);
    Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId);
    Task<object> GetDashboardMetricsAsync(DateTime date);
}
