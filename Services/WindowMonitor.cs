using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace UsageTracker.Services;

[SupportedOSPlatform("windows")]
public sealed class WindowMonitor : IWindowMonitor, IDisposable
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd); // true = minimized

    private static readonly uint OwnPid = (uint)Environment.ProcessId;

    // "Program Manager" is the Windows shell desktop window — not a real app window
    private static readonly HashSet<string> PauseWindowTitles =
        new(StringComparer.OrdinalIgnoreCase) { "Program Manager" };

    private readonly Subject<string?> _subject = new();
    private IDisposable? _subscription;

    /// <summary>
    /// Last emitted window title. Null means we are currently in a "no real window" state.
    /// Kept null after a pause so the next real window always triggers a new emit,
    /// even if it is the same title as before the pause.
    /// NOT updated when skipping our own process windows.
    /// </summary>
    private string? _lastWindow;

    /// <summary>HWND of the last window we started tracking, so we can detect if it closes.</summary>
    private IntPtr _trackedHwnd = IntPtr.Zero;

    public IObservable<string?> ActiveWindowChanged => _subject.AsObservable();

    public void Start()
    {
        _subscription = Observable
            .Interval(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => Poll());
    }

    public void Stop() => _subscription?.Dispose();

    public void Dispose()
    {
        Stop();
        _subject.Dispose();
    }

    private void Poll()
    {
        // If we were tracking a window, check it hasn't been closed or minimized.
        // IsWindow() returns false when the HWND is destroyed; IsIconic() when minimized.
        if (_trackedHwnd != IntPtr.Zero && (!IsWindow(_trackedHwnd) || IsIconic(_trackedHwnd)))
        {
            _trackedHwnd = IntPtr.Zero;
            EmitPause();
            return;
        }

        var hwnd = GetForegroundWindow();

        // No foreground window at all (e.g. transitioning between desktops)
        if (hwnd == IntPtr.Zero)
        {
            EmitPause();
            return;
        }

        GetWindowThreadProcessId(hwnd, out uint pid);

        // Skip our own windows — do NOT touch _lastWindow so the previous
        // app's stopwatch keeps running uninterrupted.
        if (pid == OwnPid) return;

        // PID 0 = System Idle, 4 = System kernel — not a real app window
        if (pid <= 4)
        {
            EmitPause();
            return;
        }

        const int nChars = 256;
        var sb = new StringBuilder(nChars);

        if (GetWindowText(hwnd, sb, nChars) <= 0)
        {
            // Titleless window (desktop shell, lock screen, etc.) — pause tracking
            EmitPause();
            return;
        }

        var title = sb.ToString();

        // "Program Manager" is the Windows desktop shell — treat as no real window
        if (PauseWindowTitles.Contains(title))
        {
            EmitPause();
            return;
        }

        if (title == _lastWindow) return;

        _trackedHwnd = hwnd;
        _lastWindow = title;
        _subject.OnNext(title);
    }

    /// <summary>
    /// Emits null only on the transition from "tracking" → "no real window".
    /// Resets _lastWindow to null so the next real window always re-emits.
    /// </summary>
    private void EmitPause()
    {
        if (_lastWindow is null) return; // already paused, don't spam
        _lastWindow = null;
        _trackedHwnd = IntPtr.Zero;
        _subject.OnNext(null);
    }
}
