using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface ISqlServerRestoreExecutor
{
    Task SetSingleUserAsync(string databaseName);
    Task RestoreAsync(string databaseName, string sourcePath);
    Task SetMultiUserAsync(string databaseName);
}
