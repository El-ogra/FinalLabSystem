using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FinalLabSystem.Services.Implementations;

public class InventoryService : IInventoryService
{
    private readonly FinalLabDbContext _context;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(FinalLabDbContext context, ILogger<InventoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<TubeMaterial>> GetAllAsync()
    {
        return await _context.TubeMaterials
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.MaterialName)
            .ToListAsync();
    }

    public async Task<List<TubeMaterial>> GetLowStockAsync()
    {
        return await _context.TubeMaterials
            .Where(t => t.IsActive && t.MinimumStock > 0 && t.CurrentStock <= t.MinimumStock)
            .OrderBy(t => t.CurrentStock)
            .ToListAsync();
    }

    public async Task<TubeMaterial?> GetByTubeTypeAsync(string tubeType)
    {
        if (string.IsNullOrWhiteSpace(tubeType))
            return null;

        var trimmed = tubeType.Trim();

        return await _context.TubeMaterials
            .FirstOrDefaultAsync(t =>
                t.IsActive && (
                    t.MaterialNameAr != null && t.MaterialNameAr == trimmed ||
                    t.MaterialName == trimmed ||
                    t.TubeColor != null && t.TubeColor == trimmed));
    }

    public async Task<bool> IsLowStockAsync(string tubeType)
    {
        var material = await GetByTubeTypeAsync(tubeType);
        return material is not null && material.MinimumStock > 0 && material.CurrentStock <= material.MinimumStock;
    }

    public async Task<int> GetLowStockCountAsync()
    {
        return await _context.TubeMaterials
            .CountAsync(t => t.IsActive && t.MinimumStock > 0 && t.CurrentStock <= t.MinimumStock);
    }

    public async Task UpdateStockAsync(int tubeMaterialId, int adjustment, string? reason = null)
    {
        var material = await _context.TubeMaterials.FindAsync(tubeMaterialId);
        if (material is null)
            throw new InvalidOperationException($"Material with ID {tubeMaterialId} not found.");

        var newStock = material.CurrentStock + adjustment;
        if (newStock < 0)
            throw new InvalidOperationException($"Stock cannot be negative. Current: {material.CurrentStock}, Adjustment: {adjustment}");

        material.CurrentStock = newStock;
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Stock updated for {MaterialName}: {Adjustment} (New: {NewStock}). Reason: {Reason}",
            material.MaterialName, adjustment, newStock, reason ?? "N/A");
    }
}
