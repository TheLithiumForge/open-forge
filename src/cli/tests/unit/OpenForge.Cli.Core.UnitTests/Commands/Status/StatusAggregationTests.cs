using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Shell.Definitions;
using StatusSourceAvailability = OpenForge.Cli.Core.Commands.Status.Models.Result.StatusSourceAvailability;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusAggregationTests
{
    [Trait("Boundary", "Processing")]
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
                StatusFindingCode.ExtensionTargetChanged,
                StatusFindingCode.FrameworkTargetMissing,
                StatusFindingCode.GeneratedNavigationChanged,
                StatusFindingCode.RecoveryCandidateVerified,
                StatusFindingCode.RecoveryDraftIncomplete,
            ],
            result.Findings.Select(finding => finding.Code));
    }

    [Trait("Boundary", "Processing")]
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
        Assert.Equal(StatusSourceAvailability.Unavailable, unavailableSource.SourceAvailability);
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

    [Trait("Boundary", "Processing")]
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
                    State = LibraryRegistrationReadState.Unavailable,
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

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status promotes proven readable malformed generated metadata to attention while retaining the unavailable region"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void ProvenReadableMalformedGeneratedMetadataBecomesAttention()
    {
        const string targetPath = ".agents/patterns/_patterns.md";
        const string sourcePath = ".agents/patterns/old-note.md";
        var observations = WithGeneratedTargets(
            new GeneratedNavigationTargetObservation(
                targetPath,
                OperationalGeneratedNavigationState.Unavailable)
            {
                MetadataIssues =
                [
                    new GeneratedNavigationMetadataIssue(
                        sourcePath,
                        "The source frontmatter or Open Forge metadata shape is malformed."),
                ],
            });

        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), observations);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var finding = Assert.Single(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationMetadataInvalid);
        Assert.Equal(sourcePath, finding.Subject);
        Assert.Equal(targetPath, finding.Source);
        Assert.DoesNotContain(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationUnavailable
                && value.Subject == targetPath);
        var target = Assert.Single(
            result.Facts.Structure.GeneratedNavigation,
            value => value.Path == targetPath);
        Assert.Equal(StatusGeneratedNavigationState.Unavailable, target.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status keeps unavailable generated metadata strict when typed evidence is absent"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void AbsentOrUnreadableGeneratedMetadataEvidenceRemainsIncomplete()
    {
        const string absentPath = ".agents/patterns/_absent.md";
        const string unreadablePath = ".agents/patterns/_unreadable.md";
        var observations = WithGeneratedTargets(
            new GeneratedNavigationTargetObservation(
                absentPath,
                OperationalGeneratedNavigationState.Unavailable),
            new GeneratedNavigationTargetObservation(
                unreadablePath,
                OperationalGeneratedNavigationState.Unavailable));

        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), observations);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(
            2,
            result.Findings.Count(value => value.Code == StatusFindingCode.GeneratedNavigationUnavailable));
        Assert.DoesNotContain(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationMetadataInvalid);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status keeps blocked generated navigation blocked even if metadata evidence is supplied"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void BlockedGeneratedNavigationRemainsBlocked()
    {
        const string targetPath = ".agents/patterns/_unsafe.md";
        var observations = WithGeneratedTargets(
            new GeneratedNavigationTargetObservation(
                targetPath,
                OperationalGeneratedNavigationState.Blocked)
            {
                MetadataIssues =
                [new GeneratedNavigationMetadataIssue(
                    ".agents/patterns/old-note.md",
                    "The source frontmatter is malformed.")],
            });

        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), observations);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationBlocked
                && value.Subject == targetPath);
        Assert.DoesNotContain(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationMetadataInvalid);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Status preserves blocked precedence and known facts when readable metadata attention is mixed with a blocked region"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void MixedGeneratedNavigationErrorsPreservePrecedenceAndKnownFacts()
    {
        const string metadataTargetPath = ".agents/patterns/_patterns.md";
        const string blockedTargetPath = ".agents/skills/_skills.md";
        var observations = WithGeneratedTargets(
            new GeneratedNavigationTargetObservation(
                metadataTargetPath,
                OperationalGeneratedNavigationState.Unavailable)
            {
                MetadataIssues =
                [new GeneratedNavigationMetadataIssue(
                    ".agents/patterns/old-note.md",
                    "The source frontmatter is malformed.")],
            },
            new GeneratedNavigationTargetObservation(
                blockedTargetPath,
                OperationalGeneratedNavigationState.Blocked));

        var result = StatusResultBuilder.Build(StatusObservationSeeds.Request(), observations);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationMetadataInvalid);
        Assert.Contains(
            result.Findings,
            value => value.Code == StatusFindingCode.GeneratedNavigationBlocked
                && value.Subject == blockedTargetPath);
        Assert.Contains(
            result.Facts.Lifecycle.Framework.Targets,
            value => value.Path == ".agents/alpha.md"
                && value.State == StatusTargetState.Missing);
    }

    private static StatusObservationSet WithGeneratedTargets(
        params GeneratedNavigationTargetObservation[] targets)
    {
        var observations = StatusAggregationObservationSeed.Create();
        return observations with
        {
            RecoveryResiduals = observations.RecoveryResiduals with
            {
                Candidates = [],
            },
            Routes = observations.Routes with
            {
                GeneratedNavigation = [.. observations.Routes.GeneratedNavigation, .. targets],
            },
            ExtensionLifecycle = observations.ExtensionLifecycle with
            {
                SourceAvailability = OperationalSourceAvailability.Available,
                Installed = observations.ExtensionLifecycle.Installed
                    .Select(extension => extension with
                    {
                        Source = extension.Source ?? $"embedded:{extension.Id}",
                        SourceAvailability = OperationalSourceAvailability.Available,
                    })
                    .ToArray(),
            },
        };
    }

}
