using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface ICompanyService
{
    Task<List<Company>> GetAllAsync();

    Task<Company?> GetByIdAsync(int id);

    Task<Company> CreateAsync(Company company);

    Task UpdateAsync(Company company);
}
