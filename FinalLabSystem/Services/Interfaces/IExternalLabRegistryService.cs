using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IExternalLabRegistryService
{
    Task<List<ExternalLab>> GetAllAsync();

    Task<ExternalLab?> GetByIdAsync(int id);

    Task<ExternalLab> CreateAsync(ExternalLab lab);

    Task UpdateAsync(ExternalLab lab);
}
