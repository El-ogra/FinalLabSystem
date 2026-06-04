using System.Threading.Tasks;
using FinalLabSystem.Models;

namespace FinalLabSystem.Services.Interfaces;

public interface IFinancialService
{
    Task RecordPatientPaymentAsync(Payment payment);
    Task ApplyDiscountAsync(int visitId, double discount, int staffId);
}
