using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using NSubstitute;
using ReactiveUI;
using UsageTracker.Entities;
using UsageTracker.Services;
using UsageTracker.ViewModels;
using Xunit;

namespace UsageTracker.Tests;

/// <summary>
/// Tests for TrackingViewModel tree insertion logic (title parsing, hierarchy, deduplication).
///
/// RxApp.MainThreadScheduler is replaced with a TestScheduler per test instance so that
/// ObserveOn callbacks execute synchronously via scheduler.AdvanceBy(1).
/// Tests in this class are isolated — xUnit creates a new instance per test.
/// </summary>
public class TrackingViewModelTests : IDisposable
{
    private readonly IProcessTrackingService _trackingService;
    private readonly IWindowMonitor _windowMonitor;
    private readonly Subject<string?> _windowSubject = new();
    private readonly Subject<Unit> _changedSubject = new();
    private readonly TestScheduler _scheduler = new();
    private readonly IScheduler _originalScheduler;
    private IReadOnlyList<TrackedProcess> _trackedProcesses = [];

    public TrackingViewModelTests()
    {
        _originalScheduler = RxApp.MainThreadScheduler;
        RxApp.MainThreadScheduler = _scheduler;

        _trackingService = Substitute.For<IProcessTrackingService>();
        _windowMonitor = Substitute.For<IWindowMonitor>();

        _windowMonitor.ActiveWindowChanged.Returns(_windowSubject);
        _trackingService.Changed.Returns(_changedSubject);
        _trackingService.TrackedProcesses.Returns(_ => _trackedProcesses);
        _trackingService.ActiveProcess.Returns((TrackedProcess?)null);
        _trackingService.SessionElapsed.Returns(TimeSpan.Zero);
    }

    public void Dispose()
    {
        RxApp.MainThreadScheduler = _originalScheduler;
        _windowSubject.Dispose();
        _changedSubject.Dispose();
    }

    private TrackingViewModel CreateAndActivate(out IDisposable activation)
    {
        var sut = new TrackingViewModel(_trackingService, _windowMonitor);
        activation = sut.Activator.Activate();
        return sut;
    }

    /// Fire Changed and advance the scheduler by one tick to flush ObserveOn queues.
    private void FireChangedAndFlush()
    {
        _changedSubject.OnNext(Unit.Default);
        _scheduler.AdvanceBy(1);
    }

    // ── Title parsing / hierarchy insertion ───────────────────────────────────

    [Fact]
    public void SyncNewProcesses_SingleSegmentTitle_AddsOneRootGroup()
    {
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("Notepad")];
            FireChangedAndFlush();

            Assert.Single(sut.Groups.Where(g => g.Name == "Notepad"));
        }
    }

    [Fact]
    public void SyncNewProcesses_SingleSegmentTitle_LeafHasOwnProcess()
    {
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("Notepad")];
            FireChangedAndFlush();

            var root = Assert.Single(sut.Groups);
            Assert.NotNull(root.OwnProcess);
            Assert.Equal("Notepad", root.OwnProcess!.ProcessName);
        }
    }

    [Fact]
    public void SyncNewProcesses_TwoSegmentTitle_CreatesRootWithChild()
    {
        // "File - Notepad" → root: "Notepad" → child: "File"
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("File - Notepad")];
            FireChangedAndFlush();

            var root = Assert.Single(sut.Groups.Where(g => g.Name == "Notepad"));
            Assert.Single(root.Children.Where(c => c.Name == "File"));
        }
    }

    [Fact]
    public void SyncNewProcesses_ThreeSegmentTitle_CreatesDeepHierarchy()
    {
        // "xQc - Twitch - Google Chrome" → Chrome → Twitch → xQc
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("xQc - Twitch - Google Chrome")];
            FireChangedAndFlush();

            var chrome = Assert.Single(sut.Groups.Where(g => g.Name == "Google Chrome"));
            var twitch = Assert.Single(chrome.Children.Where(c => c.Name == "Twitch"));
            Assert.Single(twitch.Children.Where(c => c.Name == "xQc"));
        }
    }

    [Fact]
    public void SyncNewProcesses_IntermediateNode_HasNoOwnProcess()
    {
        // "Google Chrome" root when it has children is a group node, not a leaf
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("xQc - Twitch - Google Chrome")];
            FireChangedAndFlush();

            var chrome = sut.Groups.Single(g => g.Name == "Google Chrome");
            Assert.Null(chrome.OwnProcess);
        }
    }

    [Fact]
    public void SyncNewProcesses_LeafNode_HasOwnProcess()
    {
        // "xQc" is the deepest node and should carry the TrackedProcess
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("xQc - Twitch - Google Chrome")];
            FireChangedAndFlush();

            var xqc = sut.Groups.Single().Children.Single().Children.Single();
            Assert.NotNull(xqc.OwnProcess);
            Assert.Equal("xQc - Twitch - Google Chrome", xqc.OwnProcess!.ProcessName);
        }
    }

    // ── Tree reuse / deduplication ─────────────────────────────────────────────

    [Fact]
    public void SyncNewProcesses_TwoDifferentChildrenSameRoot_ReusesRootNode()
    {
        // "Page1 - Chrome" then "Page2 - Chrome" → one Chrome root with two children
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("Page1 - Chrome")];
            FireChangedAndFlush();

            _trackedProcesses = [new TrackedProcess("Page1 - Chrome"), new TrackedProcess("Page2 - Chrome")];
            FireChangedAndFlush();

            Assert.Single(sut.Groups.Where(g => g.Name == "Chrome"));
            Assert.Equal(2, sut.Groups.Single().Children.Count);
        }
    }

    [Fact]
    public void SyncNewProcesses_SameTitleFiredTwice_DoesNotDuplicateNode()
    {
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("Notepad")];
            FireChangedAndFlush();
            FireChangedAndFlush(); // second Changed with identical list

            Assert.Single(sut.Groups.Where(g => g.Name == "Notepad"));
        }
    }

    [Fact]
    public void SyncNewProcesses_TwoIndependentApps_AddsTwoRootGroups()
    {
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _trackedProcesses = [new TrackedProcess("Notepad")];
            FireChangedAndFlush();

            _trackedProcesses = [new TrackedProcess("Notepad"), new TrackedProcess("Visual Studio")];
            FireChangedAndFlush();

            Assert.Equal(2, sut.Groups.Count);
        }
    }

    // ── Tick / session time ────────────────────────────────────────────────────

    [Fact]
    public void TickUpdate_AfterOneSecond_SessionTimeReflectsServiceElapsed()
    {
        _trackingService.SessionElapsed.Returns(TimeSpan.FromSeconds(42));
        var sut = CreateAndActivate(out var activation);
        using (activation)
        {
            _scheduler.AdvanceBy(TimeSpan.FromSeconds(1).Ticks);

            Assert.Equal("00:00:42", sut.SessionTime);
        }
    }
}
