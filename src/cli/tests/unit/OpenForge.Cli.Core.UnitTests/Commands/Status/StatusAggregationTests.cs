using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusAggregationTests
{
    [Fact(DisplayName = "Status combines ordered continuity layers and applies every deterministic result ordering"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void ContinuitySourcesCombineBaseAndOverwriteThenOrderByBytesAndSourceId()
    {
        var result = StatusResultBuilder.Build(
            StatusObservationSeeds.Request(),
            StatusAggregationObservationSeed.Create());

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(StatusResultSeeds.Available(2), result.Facts.Context.Startup.Difference.Files);
        Assert.Equal(StatusResultSeeds.Available(12), result.Facts.Context.Startup.Difference.Characters);
        Assert.Equal(StatusResultSeeds.Available(14), result.Facts.Context.Startup.Difference.Utf8Bytes);
        Assert.Equal(StatusResultSeeds.Available(3), result.Facts.Context.Startup.Difference.EstimatedTokens);
        Assert.Equal(
            ["memory/alpha", "memory/beta", "memory/zeta"],
            result.Facts.Context.ContinuitySources.Select(source => source.SourceId));
        Assert.Equal(
            [".agents/memory/alpha.md", ".agents/memory/alpha.overwrite.md"],
            result.Facts.Context.ContinuitySources[0].Layers.Select(layer => layer.Path));
        Assert.Equal([6L, 4L], result.Facts.Context.ContinuitySources[0].Layers.Select(layer => layer.Utf8Bytes));
        Assert.Equal(StatusResultSeeds.Available(3), result.Facts.Structure.RootCategories.Count);
        Assert.Equal(["custom"], result.Facts.Structure.RootCategories.Added);
        Assert.Equal(["patterns"], result.Facts.Structure.RootCategories.Removed);
        Assert.Equal(
            [".agents/alpha.md", ".agents/zeta.md"],
            result.Facts.Lifecycle.Framework.Targets.Select(target => target.Path));
        Assert.Equal(
            ["/recovery/a-draft.tmp", "/recovery/z-final.zip"],
            result.Facts.Recovery.Candidates.Select(candidate => candidate.Path));
        Assert.Equal(StatusResultSeeds.Available(1), result.Facts.Recovery.VerifiedFinals);
        Assert.Equal(StatusResultSeeds.Available(1), result.Facts.Recovery.IncompleteDrafts);
        Assert.Equal(
            [
                StatusFindingCode.ExtensionSourceUnavailable,
                StatusFindingCode.ExtensionTargetChanged,
                StatusFindingCode.FrameworkTargetMissing,
                StatusFindingCode.GeneratedNavigationChanged,
                StatusFindingCode.RecoveryCandidateVerified,
                StatusFindingCode.RecoveryDraftIncomplete,
            ],
            result.Findings.Select(finding => finding.Code));
    }

    [Fact(DisplayName = "Status deduplicates shared lifecycle targets and preserves installed facts with unavailable source"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void LifecycleProjectionDeduplicatesSharedTargetsAndRetainsInstalledFactsWhenSourceIsUnavailable()
    {
        var result = StatusResultBuilder.Build(
            StatusObservationSeeds.Request(),
            StatusAggregationObservationSeed.Create());

        Assert.Equal(
            ["alpha", "zeta"],
            result.Facts.Lifecycle.Extensions.Installed.Select(extension => extension.Id));
        var unavailableSource = result.Facts.Lifecycle.Extensions.Installed[1];
        Assert.Null(unavailableSource.Version);
        Assert.Null(unavailableSource.Source);
        Assert.Equal(OperationalSourceAvailability.Unavailable, unavailableSource.SourceAvailability);
        Assert.Equal(["alpha", "beta"], unavailableSource.Dependencies);
        Assert.Equal([".agents/shared.md", ".agents/zeta.md"], unavailableSource.Paths);

        var managed = result.Facts.Lifecycle.Extensions.ManagedFiles;
        Assert.Equal([".agents/alpha.md", ".agents/shared.md"], managed.Targets.Select(target => target.Path));
        var shared = managed.Targets[1];
        Assert.Equal(["alpha", "zeta"], shared.Owners);
        Assert.Equal(StatusResultSeeds.Available(1), managed.Counts.Current);
        Assert.Equal(StatusResultSeeds.Available(1), managed.Counts.Changed);
        Assert.Equal(StatusResultSeeds.Available(0), managed.Counts.Missing);
        Assert.Equal(StatusResultSeeds.Available(0), managed.Counts.Unavailable);
        Assert.Equal(StatusResultSeeds.Available(0), managed.Counts.Blocked);
        Assert.Contains(result.Findings, finding => finding.Code == StatusFindingCode.ExtensionSourceUnavailable);
        Assert.Contains(result.Findings, finding => finding.Code == StatusFindingCode.ExtensionTargetChanged);
    }

    [Fact(DisplayName = "Status preserves contributor-local Library cancellation as interrupted"), Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void LibraryContributorCancellationOutranksItsSyntheticUnavailableRecord()
    {
        var observations = StatusAggregationObservationSeed.Create();
        observations = observations with
        {
            Libraries = observations.Libraries with
            {
                State = OperationalViewState.Interrupted,
                Record = observations.Libraries.Record with
                {
                    State = LibrariesRecordReadState.Unavailable,
                    Snapshot = null,
                    Cause = "Library observation was interrupted.",
                },
            },
        };

        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), observations);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == StatusFindingCode.Interrupted);
        Assert.NotNull(result.Facts.Library);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Facts.Library.State);
    }

}
