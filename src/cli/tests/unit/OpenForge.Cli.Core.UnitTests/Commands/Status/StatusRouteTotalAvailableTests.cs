using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusRouteTotalAvailableTests
{
    [Fact(DisplayName = "Status total available is zero for an empty workspace")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void MissingEntryAndSourceInventoryProduceAvailableZero()
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: false,
            FileReadState.Missing,
            entryText: null,
            OperationalViewState.Incomplete);

        Assert.Equal(OperationalViewState.Complete, result.State);
        StatusRouteTotalAvailableSupport.AssertMeasurement(
            result.Context.TotalAvailable,
            expectedFiles: 0L,
            expectedCharacters: 0L,
            expectedBytes: 0L,
            expectedTokens: 0L);
    }

    [Fact(DisplayName = "Status total available counts a readable canonical entry")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void ReadableEntryIsMeasuredExactly()
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: false,
            FileReadState.Complete,
            "é\n",
            OperationalViewState.Complete);

        Assert.Equal(OperationalViewState.Complete, result.State);
        StatusRouteTotalAvailableSupport.AssertMeasurement(
            result.Context.TotalAvailable,
            expectedFiles: 1L,
            expectedCharacters: 2L,
            expectedBytes: 3L,
            expectedTokens: 1L);
    }

    [Theory(DisplayName = "Status live context measurements retain exact ceiling token boundaries"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    [InlineData("", 0L, 0L), InlineData("x", 1L, 1L), InlineData("xxxx", 4L, 1L), InlineData("xxxxx", 5L, 2L)]
    public void LiveMeasurementsUseCeilingTokenEstimate(string text, long expectedCharacters, long expectedTokens)
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: false,
            FileReadState.Complete,
            text,
            OperationalViewState.Complete);

        Assert.Equal(OperationalViewState.Complete, result.State);
        StatusRouteTotalAvailableSupport.AssertMeasurement(
            result.Context.TotalAvailable,
            expectedFiles: 1L,
            expectedCharacters: expectedCharacters,
            expectedBytes: expectedCharacters,
            expectedTokens: expectedTokens);
    }

    [Fact(DisplayName = "Status total available is unavailable when the canonical entry is unavailable")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void UnavailableEntryProducesUnavailableMeasurement()
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: false,
            FileReadState.AccessDenied,
            entryText: null,
            OperationalViewState.Incomplete);

        Assert.Equal(OperationalViewState.Incomplete, result.State);
        StatusRouteTotalAvailableSupport.AssertUnavailable(result.Context);
        Assert.Equal(OperationalValueState.Unavailable, result.Context.TotalAvailable.EstimatedTokens.State);
    }

    [Fact(DisplayName = "Status route view is blocked when the canonical entry is unsafe")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void UnsafeEntryBlocksTheRouteView()
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: false,
            FileReadState.InputOutputFailure,
            entryText: null,
            OperationalViewState.Blocked);

        Assert.Equal(OperationalViewState.Blocked, result.State);
        StatusRouteTotalAvailableSupport.AssertUnavailable(result.Context);
    }

    [Fact(DisplayName = "Status excludes a missing canonical entry from an installed inventory")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void InstalledInventoryExcludesMissingEntryAndRemainsIncomplete()
    {
        var result = StatusRouteTotalAvailableSupport.Read(
            installed: true,
            FileReadState.Missing,
            entryText: null,
            OperationalViewState.Incomplete);

        Assert.Equal(OperationalViewState.Incomplete, result.State);
        StatusRouteTotalAvailableSupport.AssertMeasurement(
            result.Context.TotalAvailable,
            expectedFiles: 1L,
            expectedCharacters: 9L,
            expectedBytes: 9L,
            expectedTokens: 3L);
    }

}
