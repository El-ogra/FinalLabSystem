using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportingService
{
    /// <summary>
    /// Gets pending worksheet rows for laboratory processing.
    /// </summary>
    /// <returns>The pending worksheet rows.</returns>
    Task<List<VPendingTest>> GetPendingWorksheetsAsync();

    /// <summary>
    /// Gets patients or visits with outstanding balances.
    /// </summary>
    /// <returns>The outstanding balance rows.</returns>
    Task<List<VOutstandingBalance>> GetDefaultersListAsync();

    /// <summary>
    /// Gets referral commission report rows for a date range.
    /// </summary>
    /// <param name="start">The inclusive start date.</param>
    /// <param name="end">The inclusive end date.</param>
    /// <returns>The referral commission report rows.</returns>
    Task<List<VReferralCommissionReport>> GetCommissionsAsync(DateTime start, DateTime end);

    /// <summary>
    /// Gets historical result comparisons for a patient and test type.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="testTypeId">The test type identifier.</param>
    /// <returns>The patient history rows for the selected test.</returns>
    Task<List<VPatientHistory>> GetHistoricalComparisonsAsync(int patientId, int testTypeId);

    /// <summary>
    /// Gets dashboard metrics for a date.
    /// </summary>
    /// <param name="date">The dashboard date.</param>
    /// <returns>An object containing dashboard metric values.</returns>
    Task<object> GetDashboardMetricsAsync(DateTime date);
}
