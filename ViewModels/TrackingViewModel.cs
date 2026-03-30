using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using UsageTracker.Entities;
using UsageTracker.Services;

namespace UsageTracker.ViewModels;

public sealed class TrackingViewModel : ReactiveObject, IActivatableViewModel
{
    public ViewModelActivator Activator { get; } = new();

    private readonly IProcessTrackingService _trackingService;
    private readonly IWindowMonitor _windowMonitor;

    // Index of root groups for O(1) lookup when inserting new processes
    private readonly Dictionary<string, ProcessGroupViewModel> _rootGroups = new();

    // Tracks which raw window titles have already been inserted into the tree
    private readonly HashSet<string> _trackedTitles = new();

    private string _sessionTime = "00:00:00";
    private ProcessGroupViewModel? _activeProcess;

    public string SessionTime
    {
        get => _sessionTime;
        private set => this.RaiseAndSetIfChanged(ref _sessionTime, value);
    }

    /// <summary>Top-level process groups, sorted by TotalElapsed descending.</summary>
    public ObservableCollection<ProcessGroupViewModel> Groups { get; } = new();

    /// <summary>The leaf node currently in the foreground window.</summary>
    public ProcessGroupViewModel? ActiveProcess
    {
        get => _activeProcess;
        private set => this.RaiseAndSetIfChanged(ref _activeProcess, value);
    }

    public ReactiveCommand<Unit, Unit> MinimizeCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public TrackingViewModel(IProcessTrackingService trackingService, IWindowMonitor windowMonitor)
    {
        _trackingService = trackingService;
        _windowMonitor = windowMonitor;

        MinimizeCommand = ReactiveCommand.Create(() => { });
        CloseCommand = ReactiveCommand.Create(() => { });

        this.WhenActivated(disposables =>
        {
            _trackingService.StartSession();
            _windowMonitor.Start();

            _windowMonitor.ActiveWindowChanged
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(title => _trackingService.OnWindowChanged(title)) // null = pause
                .DisposeWith(disposables);

            // On window switch: add any new process to the tree, update active pointer
            _trackingService.Changed
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => SyncNewProcesses())
                .DisposeWith(disposables);

            // Every second: recalculate totals and resort
            Observable.Interval(TimeSpan.FromSeconds(1), RxApp.MainThreadScheduler)
                .Subscribe(_ => TickUpdate())
                .DisposeWith(disposables);

            Disposable.Create(() =>
            {
                _windowMonitor.Stop();
                _trackingService.StopSession();
            }).DisposeWith(disposables);
        });
    }

    private void SyncNewProcesses()
    {
        foreach (var process in _trackingService.TrackedProcesses)
        {
            if (_trackedTitles.Add(process.ProcessName))
                InsertProcess(process);
        }
        UpdateActiveProcess();
    }

    /// <summary>
    /// Parses a window title into a path from root (app) to leaf (content).
    /// "xQc - Twitch - Google Chrome" → ["Google Chrome", "Twitch", "xQc"]
    /// "Usage Tracker" → ["Usage Tracker"]
    /// </summary>
    private static string[] ParseWindowTitle(string title)
    {
        var parts = title.Split(new[] { " - " }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length <= 1) return new[] { title };

        var path = new string[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            path[i] = parts[parts.Length - 1 - i].Trim();
        return path;
    }

    private void InsertProcess(TrackedProcess process)
    {
        var path = ParseWindowTitle(process.ProcessName);

        // Get or create the root group (e.g. "Google Chrome")
        if (!_rootGroups.TryGetValue(path[0], out var rootGroup))
        {
            rootGroup = new ProcessGroupViewModel(path[0], path.Length == 1 ? process : null);
            _rootGroups[path[0]] = rootGroup;
            Groups.Add(rootGroup);
        }

        if (path.Length == 1) return;

        // Walk / create intermediate and leaf nodes
        var current = rootGroup;
        for (int i = 1; i < path.Length; i++)
        {
            var segmentName = path[i];
            var child = current.Children.FirstOrDefault(c => c.Name == segmentName);
            if (child == null)
            {
                bool isLeaf = i == path.Length - 1;
                child = new ProcessGroupViewModel(segmentName, isLeaf ? process : null);
                current.Children.Add(child);
            }
            current = child;
        }
    }

    private void TickUpdate()
    {
        var elapsed = _trackingService.SessionElapsed;
        SessionTime = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";

        // Phase 1: recalculate all totals bottom-up from live stopwatches
        foreach (var group in Groups)
            group.RecalculateTotals();

        // Phase 2: update progress bars top-down
        // Root-level bars are relative to the highest-total root group
        var maxRootMs = Groups.Count > 0 ? Groups.Max(g => g.TotalElapsed.TotalMilliseconds) : 0;
        foreach (var group in Groups)
            group.UpdateProgress(maxRootMs);

        // Sort root groups by total elapsed, descending
        BubbleSortGroups();
    }

    private void BubbleSortGroups()
    {
        int n = Groups.Count;
        for (int i = 0; i < n - 1; i++)
        {
            if (Groups[i].TotalElapsed < Groups[i + 1].TotalElapsed)
                Groups.Move(i + 1, i);
        }
        foreach (var group in Groups)
            group.Sort();
    }

    private void UpdateActiveProcess()
    {
        if (_trackingService.ActiveProcess is null)
        {
            ActiveProcess = null;
            return;
        }
        ActiveProcess = FindNodeByTitle(_trackingService.ActiveProcess.ProcessName, Groups);
    }

    private static ProcessGroupViewModel? FindNodeByTitle(string title, IEnumerable<ProcessGroupViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.OwnProcess?.ProcessName == title) return node;
            var found = FindNodeByTitle(title, node.Children);
            if (found != null) return found;
        }
        return null;
    }
}
