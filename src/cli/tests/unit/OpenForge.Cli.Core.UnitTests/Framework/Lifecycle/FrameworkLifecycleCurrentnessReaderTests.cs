using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Lifecycle;

public sealed class FrameworkLifecycleCurrentnessReaderTests
{
    [Fact(DisplayName = "Framework lifecycle currentness preserves coherent state details and bounded causes"), Trait("Feature", "route-init"), Trait("Evidence", "Unit")]
    public void CurrentnessModelPreservesCoherentStateDetailsAndBoundedCauses()
    {
        var current = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.Current,
            path: null,
            cause: null);
        var cancelled = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.Cancelled,
            path: null,
            cause: null);
        var longCause = new string('x', 512);
        var sourceMismatch = new FrameworkLifecycleCurrentness(
            FrameworkLifecycleCurrentnessState.SourceMismatch,
            path: null,
            cause: longCause);

        Assert.Equal(FrameworkLifecycleCurrentnessState.Current, current.State);
        Assert.Null(current.Path);
        Assert.Null(current.Cause);
        Assert.Equal(FrameworkLifecycleCurrentnessState.Cancelled, cancelled.State);
        Assert.Null(cancelled.Path);
        Assert.Null(cancelled.Cause);
        Assert.Equal(FrameworkLifecycleCurrentnessState.SourceMismatch, sourceMismatch.State);
        Assert.Null(sourceMismatch.Path);
        Assert.Equal(longCause[..256], sourceMismatch.Cause);

        foreach (var state in new[]
        {
            FrameworkLifecycleCurrentnessState.Changed,
            FrameworkLifecycleCurrentnessState.Missing,
            FrameworkLifecycleCurrentnessState.Unavailable,
            FrameworkLifecycleCurrentnessState.Blocked,
        })
        {
            var targetObservation = new FrameworkLifecycleCurrentness(state, "target", "cause");

            Assert.Equal(state, targetObservation.State);
            Assert.Equal("target", targetObservation.Path);
            Assert.Equal("cause", targetObservation.Cause);
            Assert.Throws<ArgumentException>(() =>
                new FrameworkLifecycleCurrentness(state, null, "cause"));
            Assert.Throws<ArgumentException>(() =>
                new FrameworkLifecycleCurrentness(state, "target", null));
        }

        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.Current,
                "target",
                null));
        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.SourceMismatch,
                null,
                "   "));
        Assert.Throws<ArgumentException>(() =>
            new FrameworkLifecycleCurrentness(
                FrameworkLifecycleCurrentnessState.Cancelled,
                null,
                "cause"));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FrameworkLifecycleCurrentness(
                (FrameworkLifecycleCurrentnessState)int.MaxValue,
                null,
                null));
    }

}
