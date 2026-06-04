using System.Collections.Generic;
using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IBloodBankService
{
    Task SaveCrossMatchResultAsync(CrossMatchTest test, List<CrossMatchDonor> donors, int staffId);
    Task<CrossMatchTest?> GetCrossMatchDetailsAsync(int visitTestId);
}
