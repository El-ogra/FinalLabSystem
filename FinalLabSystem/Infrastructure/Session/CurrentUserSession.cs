using System;
using System.Windows.Input;
using System.Windows.Threading;
using FinalLabSystem.Models;

namespace FinalLabSystem.Infrastructure.Session;

public interface ICurrentUserSession
{
    Staff? CurrentUser { get; }

    bool IsAuthenticated { get; }

    int IdleTimeoutMinutes { get; set; }

    void SignIn(Staff staff);

    void SignOut();

    void StartIdleTimer(Action onTimeout);

    void ResetIdleTimer();

    void StopIdleTimer();
}

public sealed class CurrentUserSession : ICurrentUserSession
{
    /// <summary>
    /// Thread-safe singleton session. All reads/writes to _currentUser
    /// are guarded by _lockObject because async continuations and
    /// background operations may access the session from non-UI threads.
    /// </summary>
    private static readonly object _lockObject = new();
    private Staff? _currentUser;
    private DispatcherTimer? _idleTimer;

    public Staff? CurrentUser
    {
        get { lock (_lockObject) { return _currentUser; } }
        private set { lock (_lockObject) { _currentUser = value; } }
    }

    public bool IsAuthenticated
    {
        get { lock (_lockObject) { return _currentUser is not null; } }
    }

    public int IdleTimeoutMinutes { get; set; } = 15;

    public void SignIn(Staff staff)
    {
        ArgumentNullException.ThrowIfNull(staff);
        lock (_lockObject) { _currentUser = staff; }
    }

    public void SignOut()
    {
        lock (_lockObject) { _currentUser = null; }
    }

    public void StartIdleTimer(Action onTimeout)
    {
        StopIdleTimer();
        _idleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(IdleTimeoutMinutes)
        };
        _idleTimer.Tick += (s, e) =>
        {
            _idleTimer.Stop();
            onTimeout();
        };
        _idleTimer.Start();
    }

    public void ResetIdleTimer()
    {
        _idleTimer?.Stop();
        _idleTimer?.Start();
    }

    public void StopIdleTimer()
    {
        _idleTimer?.Stop();
        _idleTimer = null;
    }
}
