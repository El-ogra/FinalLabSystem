using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IReportCommentEngine
{
    Task ApplyAutoCommentAsync(TestResult result, int? testtypeId);
}
