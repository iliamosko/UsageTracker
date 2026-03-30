using UsageTracker.Entities;

namespace UsageTracker.Services;

public interface IProcessTrackingService
{
    IReadOnlyList<TrackedProcess> TrackedProcesses { get; }
    TrackedProcess? ActiveProcess { get; }

    /// <summary>Fires whenever the tracked process list or active process changes.</summary>
    IObservable<System.Reactive.Unit> Changed { get; }

    /// <summary>
    /// Called when the foreground window changes.
    /// Pass <c>null</c> to pause tracking (desktop, lock screen, all windows closed).
    /// </summary>
    void OnWindowChanged(string? windowTitle);
    void StartSession();
    void StopSession();
    TimeSpan SessionElapsed { get; }
}
