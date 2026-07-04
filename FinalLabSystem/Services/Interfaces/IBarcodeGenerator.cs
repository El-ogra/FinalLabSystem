using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface IBarcodeGenerator
{
    Task<string> GenerateCaseCodeAsync(int visitId);
    Task<string> GenerateFileCodeAsync(int visitId);
    Task<string> GetOrCreateLabIdAsync(int patientId);
}
