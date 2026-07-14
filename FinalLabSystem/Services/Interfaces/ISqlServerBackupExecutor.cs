using System.Threading.Tasks;

namespace FinalLabSystem.Services.Interfaces;

public interface ISqlServerBackupExecutor
{
    Task FullBackupAsync(string databaseName, string targetPath);
    Task DifferentialBackupAsync(string databaseName, string targetPath);
    Task TransactionLogBackupAsync(string databaseName, string targetPath);
}
