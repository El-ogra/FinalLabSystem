using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IStaffService
{
    Task<List<Staff>> GetAllAsync();
    Task<List<Staff>> GetActiveStaffAsync();
    Task<Staff?> GetByIdAsync(int staffId);
    Task UpdateDiscountLimitAsync(int staffId, double discountLimit, int modifiedByStaffId);
    Task UpdateStaffAsync(Staff staff, int modifiedByStaffId);
}
