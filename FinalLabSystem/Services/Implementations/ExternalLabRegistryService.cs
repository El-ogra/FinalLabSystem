using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Data;
using FinalLabSystem.Models;
using FinalLabSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinalLabSystem.Services.Implementations;

public class ExternalLabRegistryService : IExternalLabRegistryService
{
    private readonly FinalLabDbContext _context;

    public ExternalLabRegistryService(FinalLabDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExternalLab>> GetAllAsync()
    {
        return await _context.ExternalLabs
            .Where(l => l.IsActive)
            .OrderBy(l => l.LabName)
            .ToListAsync();
    }

    public async Task<ExternalLab?> GetByIdAsync(int id)
    {
        return await _context.ExternalLabs.FindAsync(id);
    }

    public async Task<ExternalLab> CreateAsync(ExternalLab lab)
    {
        lab.IsActive = true;
        _context.ExternalLabs.Add(lab);
        await _context.SaveChangesAsync();
        return lab;
    }

    public async Task UpdateAsync(ExternalLab lab)
    {
        var existing = await _context.ExternalLabs.FindAsync(lab.ExternalLabId);
        if (existing is null) return;

        existing.LabName = lab.LabName;
        existing.ContactPerson = lab.ContactPerson;
        existing.Phone = lab.Phone;
        existing.Email = lab.Email;
        existing.Address = lab.Address;
        existing.Notes = lab.Notes;
        existing.IsActive = lab.IsActive;

        await _context.SaveChangesAsync();
    }
}
