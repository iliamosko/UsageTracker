using UsageTracker.Services;
using Xunit;

namespace UsageTracker.Tests;

public class ProcessTrackingServiceTests
{
    private readonly ProcessTrackingService _sut = new();

    [Fact]
    public void OnWindowChanged_NewProcess_AddsToList()
    {
        _sut.OnWindowChanged("Notepad");
        Assert.Single(_sut.TrackedProcesses.Where(p => p.ProcessName == "Notepad"));
    }

    [Fact]
    public void OnWindowChanged_SameWindowTwice_DoesNotDuplicate()
    {
        _sut.OnWindowChanged("Chrome");
        _sut.OnWindowChanged("Chrome");
        Assert.Single(_sut.TrackedProcesses.Where(p => p.ProcessName == "Chrome"));
    }

    [Fact]
    public void OnWindowChanged_DifferentWindow_StopsPreviousProcess()
    {
        _sut.OnWindowChanged("Notepad");
        var notepad = _sut.ActiveProcess!;

        _sut.OnWindowChanged("Chrome");

        Assert.False(notepad.IsActive);
    }

    [Fact]
    public void OnWindowChanged_DifferentWindow_StartsNewProcess()
    {
        _sut.OnWindowChanged("Notepad");
        _sut.OnWindowChanged("Chrome");

        Assert.Equal("Chrome", _sut.ActiveProcess!.ProcessName);
        Assert.True(_sut.ActiveProcess.IsActive);
    }

    [Fact]
    public void OnWindowChanged_EmptyTitle_IsIgnored()
    {
        _sut.OnWindowChanged(string.Empty);
        Assert.Empty(_sut.TrackedProcesses);
        Assert.Null(_sut.ActiveProcess);
    }

    [Fact]
    public void TrackedProcesses_SortedByElapsedDescending()
    {
        _sut.OnWindowChanged("App A");
        _sut.OnWindowChanged("App B");
        _sut.OnWindowChanged("App A"); // switch back: App A accumulates more time

        var names = _sut.TrackedProcesses.Select(p => p.ProcessName).ToList();
        Assert.Equal("App A", names[0]);
    }

    [Fact]
    public void SessionElapsed_AfterStartSession_IsPositive()
    {
        _sut.StartSession();
        Thread.Sleep(50);
        Assert.True(_sut.SessionElapsed > TimeSpan.Zero);
    }

    [Fact]
    public void Changed_FiresWhenWindowChanges()
    {
        var fired = false;
        _sut.Changed.Subscribe(_ => fired = true);

        _sut.OnWindowChanged("Notepad");

        Assert.True(fired);
    }
}
