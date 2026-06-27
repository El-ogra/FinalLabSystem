using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IOutstandingBalanceReportService
{
    Task<List<OutstandingBalanceReportRow>> GetOutstandingBalancesAsync(DateTime startDate, DateTime endDate);
}
