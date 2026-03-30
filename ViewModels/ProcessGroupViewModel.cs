using System.Collections.ObjectModel;
using ReactiveUI;
using UsageTracker.Entities;

namespace UsageTracker.ViewModels;

public sealed class ProcessGroupViewModel : ReactiveObject
{
    private TimeSpan _totalElapsed;
    private int _progressPercent;
    private string _elapsedText = "00:00:00";
    private bool _isExpanded;

    public string Name { get; }

    /// <summary>
    /// The underlying tracked process. Non-null only for leaf nodes (unique window titles).
    /// Group nodes that purely aggregate children have this as null.
    /// </summary>
    public TrackedProcess? OwnProcess { get; }

    /// <summary>Full original window title for display in the active card.</summary>
    public string FullTitle => OwnProcess?.ProcessName ?? Name;

    public ObservableCollection<ProcessGroupViewModel> Children { get; } = new();
    public bool HasChildren => Children.Count > 0;

    public TimeSpan TotalElapsed
    {
        get => _totalElapsed;
        private set => this.RaiseAndSetIfChanged(ref _totalElapsed, value);
    }

    public int ProgressPercent
    {
        get => _progressPercent;
        set => this.RaiseAndSetIfChanged(ref _progressPercent, value);
    }

    public string ElapsedText
    {
        get => _elapsedText;
        private set => this.RaiseAndSetIfChanged(ref _elapsedText, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public ProcessGroupViewModel(string name, TrackedProcess? ownProcess = null)
    {
        Name = name;
        OwnProcess = ownProcess;
    }

    /// <summary>
    /// Phase 1 of the tick: recalculate TotalElapsed bottom-up from leaf stopwatches.
    /// Must be called before UpdateProgress.
    /// </summary>
    public void RecalculateTotals()
    {
        foreach (var child in Children)
            child.RecalculateTotals();

        TotalElapsed = (OwnProcess?.Elapsed ?? TimeSpan.Zero)
                     + Children.Aggregate(TimeSpan.Zero, (acc, c) => acc + c.TotalElapsed);

        var e = TotalElapsed;
        ElapsedText = $"{e.Hours:00}:{e.Minutes:00}:{e.Seconds:00}";
    }

    /// <summary>
    /// Bubble sort direct children by TotalElapsed descending, then recurse.
    /// Called after RecalculateTotals so TotalElapsed values are current.
    /// </summary>
    public void Sort()
    {
        int n = Children.Count;
        for (int i = 0; i < n - 1; i++)
        {
            if (Children[i].TotalElapsed < Children[i + 1].TotalElapsed)
                Children.Move(i + 1, i);
        }
        foreach (var child in Children)
            child.Sort();
    }

    /// <summary>
    /// Phase 2 of the tick: update progress bars top-down.
    /// Each level's bars are relative to parentTotalMs (the parent's TotalElapsed).
    /// Children recurse with this node's own TotalElapsed as their reference.
    /// </summary>
    public void UpdateProgress(double parentTotalMs)
    {
        ProgressPercent = parentTotalMs > 0
            ? Math.Min(100, (int)(TotalElapsed.TotalMilliseconds / parentTotalMs * 100))
            : 100;

        var ownMs = TotalElapsed.TotalMilliseconds;
        foreach (var child in Children)
            child.UpdateProgress(ownMs);
    }
}
