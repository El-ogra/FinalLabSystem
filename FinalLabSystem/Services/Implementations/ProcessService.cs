using System.Diagnostics;
using FinalLabSystem.Services.Interfaces;

namespace FinalLabSystem.Services.Implementations;

public class ProcessService : IProcessService
{
    public void OpenFolder(string folderPath)
    {
        Process.Start(new ProcessStartInfo("explorer.exe", folderPath)
        {
            UseShellExecute = true
        });
    }
}
