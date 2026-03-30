using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UsageTracker.Entities;

namespace UsageTracker.Services;

public sealed class ProcessTrackingService : IProcessTrackingService, IDisposable
{
    private readonly Dictionary<string, TrackedProcess> _processes = new();
    private readonly Subject<System.Reactive.Unit> _changed = new();
    private readonly Stopwatch _sessionStopwatch = new();

    public IReadOnlyList<TrackedProcess> TrackedProcesses =>
        _processes.Values.OrderByDescending(p => p.Elapsed).ToList();

    public TrackedProcess? ActiveProcess { get; private set; }

    public IObservable<System.Reactive.Unit> Changed => _changed.AsObservable();

    public TimeSpan SessionElapsed => _sessionStopwatch.Elapsed;

    public void StartSession() => _sessionStopwatch.Start();

    public void StopSession() => _sessionStopwatch.Stop();

    public void OnWindowChanged(string? windowTitle)
    {
        // null = desktop / lock screen / all windows closed → pause tracking
        if (string.IsNullOrWhiteSpace(windowTitle))
        {
            if (ActiveProcess is null) return; // already paused, no change
            ActiveProcess.Stop();
            ActiveProcess = null;
            _changed.OnNext(System.Reactive.Unit.Default);
            return;
        }

        // Stop the previously active process immediately
        ActiveProcess?.Stop();

        if (!_processes.TryGetValue(windowTitle, out var process))
        {
            process = new TrackedProcess(windowTitle);
            _processes[windowTitle] = process;
        }

        ActiveProcess = process;
        ActiveProcess.Start();
        _changed.OnNext(System.Reactive.Unit.Default);
    }

    public void Dispose() => _changed.Dispose();
}
