using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusContextFindingAggregator
{
    internal static void Add(
        StatusFindingCollector findings,
        StatusObservationSet observations,
        StatusFacts facts)
    {
        AddView(findings, observations);
        if (facts.Installation.State != StatusInstallationState.Uninstalled)
        {
            AddMeasurements(findings, observations, facts);
        }

        foreach (var target in facts.Structure.GeneratedNavigation)
        {
            var observation = observations.Routes.GeneratedNavigation
                .SingleOrDefault(value => string.Equals(value.Path, target.Path, StringComparison.Ordinal));
            AddGenerated(findings, target, observation);
        }
    }

    private static void AddView(StatusFindingCollector findings, StatusObservationSet observations)
    {
        switch (observations.WorkspaceEntry.State)
        {
            case OperationalViewState.Complete:
                break;
            case OperationalViewState.Incomplete:
                findings.Add(StatusFindingCode.EntryUnavailable, observations.WorkspaceEntry.EntryPath,
                    "The workspace entry observation is incomplete.");
                break;
            case OperationalViewState.Blocked:
                findings.Add(StatusFindingCode.WorkspaceUnsafe, null,
                    "The workspace entry boundary is blocked.");
                break;
            case OperationalViewState.Interrupted:
                findings.Add(StatusFindingCode.Interrupted, null,
                    "The workspace entry observation was interrupted.");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(observations),
                    observations.WorkspaceEntry.State,
                    "The workspace-entry view state is not defined.");
        }

        switch (observations.Routes.State)
        {
            case OperationalViewState.Complete:
                break;
            case OperationalViewState.Incomplete:
                findings.Add(StatusFindingCode.ContextInventoryIncomplete,
                    observations.Routes.ContextUnavailablePath,
                    observations.Routes.ContextUnavailableCause
                        ?? "The context inventory observation is incomplete.");
                break;
            case OperationalViewState.Blocked:
                findings.Add(StatusFindingCode.WorkspaceUnsafe, null,
                    "The route observation is blocked by the workspace boundary.");
                break;
            case OperationalViewState.Interrupted:
                findings.Add(StatusFindingCode.Interrupted, null,
                    "The route observation was interrupted.");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(observations),
                    observations.Routes.State,
                    "The route view state is not defined.");
        }
    }

    private static void AddMeasurements(
        StatusFindingCollector findings,
        StatusObservationSet observations,
        StatusFacts facts)
    {
        if (Unavailable(facts.Context.Startup.Initial))
        {
            findings.Add(StatusFindingCode.EmbeddedFrameworkUnavailable, null,
                "The embedded Framework startup measurement is unavailable.");
        }

        if (Unavailable(facts.Context.TotalAvailable)
            && observations.Routes.ContextUnavailablePath is null)
        {
            findings.Add(StatusFindingCode.ContextInventoryIncomplete, null,
                "The total available context measurement is incomplete.");
        }

        if (Unavailable(facts.Context.Startup.Current))
        {
            findings.Add(StatusFindingCode.StartupContextUnavailable, null,
                "The current startup context measurement is unavailable.");
        }

        if (Unavailable(facts.Context.Continuity))
        {
            findings.Add(StatusFindingCode.ContinuityContextUnavailable, null,
                "The continuity context measurement is unavailable.");
        }

        if (facts.Structure.RootCategories.Count.State == StatusValueState.Unavailable)
        {
            findings.Add(StatusFindingCode.RootCategoriesUnavailable, null,
                "The current root-category observation is unavailable.");
        }
    }

    private static bool Unavailable(StatusMeasurement measurement)
        => measurement.Files.State == StatusValueState.Unavailable
            || measurement.Characters.State == StatusValueState.Unavailable
            || measurement.Utf8Bytes.State == StatusValueState.Unavailable
            || measurement.EstimatedTokens.State == StatusValueState.Unavailable;

    private static void AddGenerated(
        StatusFindingCollector findings,
        StatusGeneratedNavigation target,
        GeneratedNavigationTargetObservation? observation)
    {
        if (target.State == StatusGeneratedNavigationState.Unavailable
            && observation?.MetadataIssues is { Count: > 0 } metadataIssues)
        {
            foreach (var issue in metadataIssues)
            {
                findings.Add(
                    StatusFindingCode.GeneratedNavigationMetadataInvalid,
                    issue.Path,
                    issue.Cause,
                    source: target.Path);
            }

            return;
        }

        StatusFindingCode? code = target.State switch
        {
            StatusGeneratedNavigationState.Current => null,
            StatusGeneratedNavigationState.NotApplicable => null,
            StatusGeneratedNavigationState.Changed => StatusFindingCode.GeneratedNavigationChanged,
            StatusGeneratedNavigationState.Missing => StatusFindingCode.GeneratedNavigationMissing,
            StatusGeneratedNavigationState.Unavailable => StatusFindingCode.GeneratedNavigationUnavailable,
            StatusGeneratedNavigationState.Blocked => StatusFindingCode.GeneratedNavigationBlocked,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target.State,
                "The generated-navigation state is not defined."),
        };
        if (code.HasValue)
        {
            findings.Add(code.Value, target.Path, "The generated navigation target is not current.");
        }
    }
}
