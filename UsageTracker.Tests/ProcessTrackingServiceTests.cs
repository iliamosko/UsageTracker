using FluentAssertions;
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
        _sut.TrackedProcesses.Should().ContainSingle(p => p.ProcessName == "Notepad");
    }

    [Fact]
    public void OnWindowChanged_SameWindowTwice_DoesNotDuplicate()
    {
        _sut.OnWindowChanged("Chrome");
        _sut.OnWindowChanged("Chrome");
        _sut.TrackedProcesses.Should().ContainSingle(p => p.ProcessName == "Chrome");
    }

    [Fact]
    public void OnWindowChanged_DifferentWindow_StopsPreviousProcess()
    {
        _sut.OnWindowChanged("Notepad");
        var notepad = _sut.ActiveProcess!;

        _sut.OnWindowChanged("Chrome");

        notepad.IsActive.Should().BeFalse();
    }

    [Fact]
    public void OnWindowChanged_DifferentWindow_StartsNewProcess()
    {
        _sut.OnWindowChanged("Notepad");
        _sut.OnWindowChanged("Chrome");

        _sut.ActiveProcess!.ProcessName.Should().Be("Chrome");
        _sut.ActiveProcess.IsActive.Should().BeTrue();
    }

    [Fact]
    public void OnWindowChanged_EmptyTitle_IsIgnored()
    {
        _sut.OnWindowChanged(string.Empty);
        _sut.TrackedProcesses.Should().BeEmpty();
        _sut.ActiveProcess.Should().BeNull();
    }

    [Fact]
    public void TrackedProcesses_SortedByElapsedDescending()
    {
        _sut.OnWindowChanged("App A");
        _sut.OnWindowChanged("App B");
        _sut.OnWindowChanged("App A"); // switch back: App A accumulates more time

        var names = _sut.TrackedProcesses.Select(p => p.ProcessName).ToList();
        names[0].Should().Be("App A");
    }

    [Fact]
    public void SessionElapsed_AfterStartSession_IsPositive()
    {
        _sut.StartSession();
        Thread.Sleep(50);
        _sut.SessionElapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public void Changed_FiresWhenWindowChanges()
    {
        var fired = false;
        _sut.Changed.Subscribe(_ => fired = true);

        _sut.OnWindowChanged("Notepad");

        fired.Should().BeTrue();
    }
}
