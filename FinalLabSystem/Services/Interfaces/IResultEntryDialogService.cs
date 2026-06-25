using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FinalLabSystem.Models.DTOs;

namespace FinalLabSystem.Services.Interfaces;

public interface IResultEntryDialogService
{
    Task<bool> OpenAsync(int visitTestId, int patientId, string testTypeName,
                         ObservableCollection<TestComponentResultDto> components);
}
