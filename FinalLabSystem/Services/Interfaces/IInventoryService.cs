using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IInventoryService
{
    Task<List<TubeMaterial>> GetAllAsync();
    Task<List<TubeMaterial>> GetLowStockAsync();
    Task<TubeMaterial?> GetByTubeTypeAsync(string tubeType);
    Task<bool> IsLowStockAsync(string tubeType);
    Task<int> GetLowStockCountAsync();
    Task UpdateStockAsync(int tubeMaterialId, int adjustment, string? reason = null);
}
