using System.Threading.Tasks;

namespace FinalLabSystem.Infrastructure;

public interface IAsyncInitializable
{
    Task InitializeAsync();
}
