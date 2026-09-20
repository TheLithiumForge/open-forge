using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;

internal static class StatusLifecycleFindingAggregator
{
    internal static void Add(
        StatusFindingCollector findings,
        StatusObservationSet observations,
        StatusLifecycle lifecycle)
    {
        if (lifecycle.Framework.State != StatusLifecycleState.Absent)
        {
            AddFrameworkView(findings, observations.FrameworkLifecycle.State);
        }

        if (lifecycle.Extensions.State != StatusLifecycleState.Absent)
        {
            AddExtensionView(findings, observations.ExtensionLifecycle.State);
        }
        if (observations.FrameworkLifecycle.OwnershipObservation is { } frameworkObservation)
        {
            findings.Add(StatusFindingCode.FrameworkOwnershipObservation, null, frameworkObservation);
        }
        else
        {
            AddFrameworkLifecycle(findings, lifecycle.Framework);
        }
        if (observations.ExtensionLifecycle.OwnershipObservation is { } extensionObservation)
        {
            findings.Add(StatusFindingCode.ExtensionOwnershipObservation, null, extensionObservation);
        }
        else
        {
            AddExtensionLifecycle(findings, lifecycle.Extensions);
        }
    }

    private static void AddFrameworkView(StatusFindingCollector findings, OperationalViewState state)
    {
        switch (state)
        {
            case OperationalViewState.Complete:
                break;
            case OperationalViewState.Incomplete:
                findings.Add(StatusFindingCode.FrameworkLifecycleIncomplete, null,
                    "The Framework lifecycle observation is incomplete.");
                break;
            case OperationalViewState.Blocked:
                findings.Add(StatusFindingCode.FrameworkLifecycleBlocked, null,
                    "The Framework lifecycle observation is blocked.");
                break;
            case OperationalViewState.Interrupted:
                findings.Add(StatusFindingCode.Interrupted, null,
                    "The Framework lifecycle observation was interrupted.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state,
                    "The Framework lifecycle view state is not defined.");
        }
    }

    private static void AddExtensionView(StatusFindingCollector findings, OperationalViewState state)
    {
        switch (state)
        {
            case OperationalViewState.Complete:
                break;
            case OperationalViewState.Incomplete:
                findings.Add(StatusFindingCode.ExtensionLifecycleIncomplete, null,
                    "The Extension lifecycle observation is incomplete.");
                break;
            case OperationalViewState.Blocked:
                findings.Add(StatusFindingCode.ExtensionLifecycleBlocked, null,
                    "The Extension lifecycle observation is blocked.");
                break;
            case OperationalViewState.Interrupted:
                findings.Add(StatusFindingCode.Interrupted, null,
                    "The Extension lifecycle observation was interrupted.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state,
                    "The Extension lifecycle view state is not defined.");
        }
    }

    private static void AddFrameworkLifecycle(
        StatusFindingCollector findings,
        StatusFrameworkLifecycle lifecycle)
    {
        switch (lifecycle.State)
        {
            case StatusLifecycleState.Absent:
            case StatusLifecycleState.Trusted:
                break;
            case StatusLifecycleState.Untrusted:
                findings.Add(StatusFindingCode.FrameworkLifecycleUntrusted, null,
                    "The Framework lifecycle document is untrusted.");
                break;
            case StatusLifecycleState.Incomplete:
                findings.Add(StatusFindingCode.FrameworkLifecycleIncomplete, null,
                    "The Framework lifecycle document is incomplete.");
                break;
            case StatusLifecycleState.Blocked:
                findings.Add(StatusFindingCode.FrameworkLifecycleBlocked, null,
                    "The Framework lifecycle document is blocked.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.State,
                    "The Framework lifecycle state is not defined.");
        }

        foreach (var target in lifecycle.Targets)
        {
            AddTarget(findings, target.Path, target.State, framework: true, cause: target.Cause);
        }
    }

    private static void AddExtensionLifecycle(
        StatusFindingCollector findings,
        StatusExtensionLifecycle lifecycle)
    {
        switch (lifecycle.State)
        {
            case StatusLifecycleState.Absent:
            case StatusLifecycleState.Trusted:
                break;
            case StatusLifecycleState.Untrusted:
                findings.Add(StatusFindingCode.ExtensionLifecycleUntrusted, null,
                    "The Extension lifecycle document is untrusted.");
                break;
            case StatusLifecycleState.Incomplete:
                findings.Add(StatusFindingCode.ExtensionLifecycleIncomplete, null,
                    "The Extension lifecycle document is incomplete.");
                break;
            case StatusLifecycleState.Blocked:
                findings.Add(StatusFindingCode.ExtensionLifecycleBlocked, null,
                    "The Extension lifecycle document is blocked.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.State,
                    "The Extension lifecycle state is not defined.");
        }

        var unavailableSources = lifecycle.Installed
            .Where(extension => extension.SourceAvailability == StatusSourceAvailability.Unavailable)
            .OrderBy(extension => extension.Id, StringComparer.Ordinal)
            .ToArray();
        if (unavailableSources.Length > 0)
        {
            foreach (var extension in unavailableSources)
            {
                findings.Add(
                    StatusFindingCode.ExtensionSourceUnavailable,
                    extension.Id,
                    extension.SourceCause ?? "The installed Extension source is unavailable.",
                    source: extension.Source);
            }
        }
        else if (lifecycle.SourceAvailability == StatusSourceAvailability.Unavailable)
        {
            findings.Add(StatusFindingCode.ExtensionSourceUnavailable, null,
                "The installed Extension source is unavailable.");
        }

        foreach (var target in lifecycle.ManagedFiles.Targets)
        {
            AddTarget(findings, target.Path, target.State, framework: false, owners: target.Owners, cause: target.Cause);
        }
    }

    private static void AddTarget(
        StatusFindingCollector findings,
        string path,
        StatusTargetState state,
        bool framework,
        IReadOnlyList<string>? owners = null,
        string? cause = null)
    {
        StatusFindingCode? code = (framework, state) switch
        {
            (_, StatusTargetState.Current) => null,
            (true, StatusTargetState.Changed) => StatusFindingCode.FrameworkTargetChanged,
            (true, StatusTargetState.Missing) => StatusFindingCode.FrameworkTargetMissing,
            (true, StatusTargetState.Unavailable) => StatusFindingCode.FrameworkTargetUnavailable,
            (true, StatusTargetState.Blocked) => StatusFindingCode.FrameworkTargetBlocked,
            (false, StatusTargetState.Changed) => StatusFindingCode.ExtensionTargetChanged,
            (false, StatusTargetState.Missing) => StatusFindingCode.ExtensionTargetMissing,
            (false, StatusTargetState.Unavailable) => StatusFindingCode.ExtensionTargetUnavailable,
            (false, StatusTargetState.Blocked) => StatusFindingCode.ExtensionTargetBlocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state,
                "The managed target state is not defined."),
        };
        if (code.HasValue)
        {
            if (framework || owners is null || owners.Count == 0)
            {
                findings.Add(code.Value, path, cause ?? "The managed lifecycle target is not current.");
            }
            else
            {
                foreach (var owner in owners.Order(StringComparer.Ordinal))
                {
                    findings.Add(code.Value, path, cause ?? "The managed lifecycle target is not current.", owner);
                }
            }
        }
    }
}
