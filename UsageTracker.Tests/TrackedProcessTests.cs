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
        Assert.True(process.IsActive);
    }

    [Fact]
    public void Stop_SetsIsActiveFalse()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        process.Stop();
        Assert.False(process.IsActive);
    }

    [Fact]
    public void Stop_WhenNotRunning_DoesNotThrow()
    {
        var process = new TrackedProcess("TestApp");
        var ex = Record.Exception(() => process.Stop());
        Assert.Null(ex);
    }

    [Fact]
    public async Task Elapsed_AfterStartAndStop_IsPositive()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        await Task.Delay(50);
        process.Stop();
        Assert.True(process.Elapsed > TimeSpan.Zero);
    }

    [Fact]
    public async Task Reset_ClearsElapsed()
    {
        var process = new TrackedProcess("TestApp");
        process.Start();
        await Task.Delay(50);
        process.Reset();
        Assert.Equal(TimeSpan.Zero, process.Elapsed);
    }

    [Fact]
    public void Constructor_NullName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new TrackedProcess(null!));
    }
}
