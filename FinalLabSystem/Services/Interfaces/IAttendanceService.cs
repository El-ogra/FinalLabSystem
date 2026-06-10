using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IAttendanceService
{
    /// <summary>
    /// Gets active work shifts.
    /// </summary>
    /// <returns>The active work shifts.</returns>
    Task<List<WorkShift>> GetActiveShiftsAsync();

    /// <summary>
    /// Records a staff clock-in event.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    /// <param name="shiftId">The work shift identifier.</param>
    Task RecordClockInAsync(int staffId, int shiftId);

    /// <summary>
    /// Records a staff clock-out event.
    /// </summary>
    /// <param name="staffId">The staff member identifier.</param>
    Task RecordClockOutAsync(int staffId);
}
