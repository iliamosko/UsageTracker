using UsageTracker.Entities;
using UsageTracker.ViewModels;
using Xunit;

namespace UsageTracker.Tests;

public class ProcessGroupViewModelTests
{
    // ── RecalculateTotals ──────────────────────────────────────────────────────

    [Fact]
    public void RecalculateTotals_LeafNode_ReflectsOwnProcessElapsed()
    {
        var process = new TrackedProcess("App");
        process.Start();
        Thread.Sleep(40);
        process.Stop();
        var node = new ProcessGroupViewModel("App", process);

        node.RecalculateTotals();

        Assert.Equal(process.Elapsed, node.TotalElapsed);
        Assert.True(node.TotalElapsed > TimeSpan.Zero);
    }

    [Fact]
    public void RecalculateTotals_GroupNode_AggregatesChildTotals()
    {
        var p1 = new TrackedProcess("Page1");
        p1.Start(); Thread.Sleep(30); p1.Stop();
        var p2 = new TrackedProcess("Page2");
        p2.Start(); Thread.Sleep(30); p2.Stop();

        var child1 = new ProcessGroupViewModel("Page1", p1);
        var child2 = new ProcessGroupViewModel("Page2", p2);
        var parent = new ProcessGroupViewModel("App");
        parent.Children.Add(child1);
        parent.Children.Add(child2);

        parent.RecalculateTotals();

        Assert.Equal(p1.Elapsed + p2.Elapsed, parent.TotalElapsed);
    }

    [Fact]
    public void RecalculateTotals_NoProcessNoChildren_IsZero()
    {
        var node = new ProcessGroupViewModel("Empty");

        node.RecalculateTotals();

        Assert.Equal(TimeSpan.Zero, node.TotalElapsed);
    }

    [Fact]
    public void RecalculateTotals_UpdatesElapsedText_ToFormattedString()
    {
        var node = new ProcessGroupViewModel("App");

        node.RecalculateTotals();

        Assert.Equal("00:00:00", node.ElapsedText);
    }

    [Fact]
    public void RecalculateTotals_RecursesIntoGrandchildren()
    {
        var p = new TrackedProcess("Leaf");
        p.Start(); Thread.Sleep(30); p.Stop();

        var leaf = new ProcessGroupViewModel("Leaf", p);
        var middle = new ProcessGroupViewModel("Middle");
        middle.Children.Add(leaf);
        var root = new ProcessGroupViewModel("Root");
        root.Children.Add(middle);

        root.RecalculateTotals();

        Assert.Equal(p.Elapsed, root.TotalElapsed);
        Assert.Equal(p.Elapsed, middle.TotalElapsed);
    }

    // ── UpdateProgress ─────────────────────────────────────────────────────────

    [Fact]
    public void UpdateProgress_ZeroParent_SetsFullHundred()
    {
        var node = new ProcessGroupViewModel("App");
        node.RecalculateTotals();

        node.UpdateProgress(0);

        Assert.Equal(100, node.ProgressPercent);
    }

    [Fact]
    public void UpdateProgress_HalfOfParent_SetsFiftyPercent()
    {
        var p = new TrackedProcess("App");
        p.Start(); Thread.Sleep(60); p.Stop();
        var node = new ProcessGroupViewModel("App", p);
        node.RecalculateTotals();

        node.UpdateProgress(node.TotalElapsed.TotalMilliseconds * 2);

        Assert.Equal(50, node.ProgressPercent);
    }

    [Fact]
    public void UpdateProgress_ExceedsParent_CapsAtHundred()
    {
        var p = new TrackedProcess("App");
        p.Start(); Thread.Sleep(40); p.Stop();
        var node = new ProcessGroupViewModel("App", p);
        node.RecalculateTotals();

        // parentMs is smaller than node's own total — shouldn't happen normally
        node.UpdateProgress(node.TotalElapsed.TotalMilliseconds / 2);

        Assert.Equal(100, node.ProgressPercent);
    }

    [Fact]
    public void UpdateProgress_RecursesIntoChildren()
    {
        var p = new TrackedProcess("Child");
        p.Start(); Thread.Sleep(40); p.Stop();
        var child = new ProcessGroupViewModel("Child", p);
        var parent = new ProcessGroupViewModel("Parent");
        parent.Children.Add(child);
        parent.RecalculateTotals();

        parent.UpdateProgress(parent.TotalElapsed.TotalMilliseconds);

        // child is 100% of parent
        Assert.Equal(100, child.ProgressPercent);
    }

    // ── Sort ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Sort_SortsChildrenDescendingByTotalElapsed()
    {
        var pLess = new TrackedProcess("Less");
        pLess.Start(); Thread.Sleep(20); pLess.Stop();
        var pMore = new TrackedProcess("More");
        pMore.Start(); Thread.Sleep(80); pMore.Stop();

        var less = new ProcessGroupViewModel("Less", pLess);
        var more = new ProcessGroupViewModel("More", pMore);
        var parent = new ProcessGroupViewModel("Parent");
        parent.Children.Add(less); // less-elapsed first
        parent.Children.Add(more);
        parent.RecalculateTotals();

        parent.Sort();

        Assert.Equal("More", parent.Children[0].Name);
        Assert.Equal("Less", parent.Children[1].Name);
    }

    [Fact]
    public void Sort_RecursivelySortsGrandchildren()
    {
        var pLess = new TrackedProcess("Less");
        pLess.Start(); Thread.Sleep(20); pLess.Stop();
        var pMore = new TrackedProcess("More");
        pMore.Start(); Thread.Sleep(80); pMore.Stop();

        var less = new ProcessGroupViewModel("Less", pLess);
        var more = new ProcessGroupViewModel("More", pMore);
        var middle = new ProcessGroupViewModel("Middle");
        middle.Children.Add(less);
        middle.Children.Add(more);
        var root = new ProcessGroupViewModel("Root");
        root.Children.Add(middle);
        root.RecalculateTotals();

        root.Sort();

        Assert.Equal("More", middle.Children[0].Name);
    }

    // ── HasChildren ────────────────────────────────────────────────────────────

    [Fact]
    public void HasChildren_WhenEmpty_ReturnsFalse()
    {
        Assert.False(new ProcessGroupViewModel("App").HasChildren);
    }

    [Fact]
    public void HasChildren_AfterAddingChild_ReturnsTrue()
    {
        var parent = new ProcessGroupViewModel("App");
        parent.Children.Add(new ProcessGroupViewModel("Child"));
        Assert.True(parent.HasChildren);
    }
}
