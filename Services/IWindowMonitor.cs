namespace UsageTracker.Services;

public interface IWindowMonitor
{
    /// <summary>
    /// Emits the active window title whenever it changes.
    /// Emits <c>null</c> when transitioning to "no real window" (desktop, lock screen,
    /// all windows minimized) — subscribers should pause tracking on null.
    /// </summary>
    IObservable<string?> ActiveWindowChanged { get; }

    void Start();
    void Stop();
}
