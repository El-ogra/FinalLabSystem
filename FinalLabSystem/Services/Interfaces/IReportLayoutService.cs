using FinalLabSystem.Models.DTOs;
using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportLayoutService
{
    Task<ReportLayoutDto> GetCurrentLayoutAsync();
    Task SaveLayoutAsync(ReportLayoutDto layout, int staffId);
    Task ResetToDefaultsAsync();
    ReportLayoutDto GetDefaults();
}
