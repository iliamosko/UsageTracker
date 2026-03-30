using FluentAssertions;
using UsageTracker.Entities;
using Xunit;

namespace UsageTracker.Tests;

public class TrackedProcessTests
{
    [Fact]
    public void Start_SetsIsActiveTrue()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        process.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Stop_SetsIsActiveFalse()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        process.Stop();
        process.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Stop_WhenNotRunning_DoesNotThrow()
    {
        var process = new TrackedProcess("TestApp");
        var act = () => process.Stop();
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Elapsed_AfterStartAndStop_IsPositive()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        await Task.Delay(50);
        process.Stop();
        process.Elapsed.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Reset_ClearsElapsed()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        await Task.Delay(50);
        process.Reset();
        process.Elapsed.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Constructor_NullName_Throws()
    {
        var act = () => new TrackedProcess(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
