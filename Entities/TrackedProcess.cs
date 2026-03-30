using System.Diagnostics;

namespace UsageTracker.Entities;

public sealed class TrackedProcess
{
    private readonly Stopwatch _stopwatch = new();

    public string ProcessName { get; }
    public bool IsActive { get; private set; }
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public TrackedProcess(string processName)
    {
        ProcessName = processName ?? throw new ArgumentNullException(nameof(processName));
    }

    public void Start()
    {
        IsActive = true;
        _stopwatch.Start();
    }

    public void Stop()
    {
        IsActive = false;
        if (_stopwatch.IsRunning)
            _stopwatch.Stop();
    }

    public void Reset()
    {
        Stop();
        _stopwatch.Reset();
    }
}
