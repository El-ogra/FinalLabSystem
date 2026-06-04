using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAttendanceService
{
    Task<List<WorkShift>> GetActiveShiftsAsync();
    Task RecordClockInAsync(int staffId, int shiftId);
    Task RecordClockOutAsync(int staffId);
}
