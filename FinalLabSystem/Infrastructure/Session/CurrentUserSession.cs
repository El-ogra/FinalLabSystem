using FinalLabSystem.Models;

namespace FinalLabSystem.Infrastructure.Session;

public interface ICurrentUserSession
{
    Staff? CurrentUser { get; }

    bool IsAuthenticated { get; }

    void SignIn(Staff staff);

    void SignOut();
}

public sealed class CurrentUserSession : ICurrentUserSession
{
    public Staff? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public void SignIn(Staff staff)
    {
        CurrentUser = staff ?? throw new ArgumentNullException(nameof(staff));
    }

    public void SignOut()
    {
        CurrentUser = null;
    }
}
