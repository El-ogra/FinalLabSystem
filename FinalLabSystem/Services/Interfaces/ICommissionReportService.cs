using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface ICommissionReportService
{
    Task<List<CommissionReportRow>> GetCommissionReportAsync(DateTime startDate, DateTime endDate);
}
