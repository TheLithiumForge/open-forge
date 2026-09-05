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
        if (lifecycle.Framework.State != OperationalLifecycleState.Absent)
        {
            AddFrameworkView(findings, observations.FrameworkLifecycle.State);
        }

        if (lifecycle.Extensions.State != OperationalLifecycleState.Absent)
        {
            AddExtensionView(findings, observations.ExtensionLifecycle.State);
        }
        AddFrameworkLifecycle(findings, lifecycle.Framework);
        AddExtensionLifecycle(findings, lifecycle.Extensions);
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
            case OperationalLifecycleState.Absent:
            case OperationalLifecycleState.Trusted:
                break;
            case OperationalLifecycleState.Untrusted:
                findings.Add(StatusFindingCode.FrameworkLifecycleUntrusted, null,
                    "The Framework lifecycle document is untrusted.");
                break;
            case OperationalLifecycleState.Incomplete:
                findings.Add(StatusFindingCode.FrameworkLifecycleIncomplete, null,
                    "The Framework lifecycle document is incomplete.");
                break;
            case OperationalLifecycleState.Blocked:
                findings.Add(StatusFindingCode.FrameworkLifecycleBlocked, null,
                    "The Framework lifecycle document is blocked.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.State,
                    "The Framework lifecycle state is not defined.");
        }

        foreach (var target in lifecycle.Targets)
        {
            AddTarget(findings, target.Path, target.State, framework: true);
        }
    }

    private static void AddExtensionLifecycle(
        StatusFindingCollector findings,
        StatusExtensionLifecycle lifecycle)
    {
        switch (lifecycle.State)
        {
            case OperationalLifecycleState.Absent:
            case OperationalLifecycleState.Trusted:
                break;
            case OperationalLifecycleState.Untrusted:
                findings.Add(StatusFindingCode.ExtensionLifecycleUntrusted, null,
                    "The Extension lifecycle document is untrusted.");
                break;
            case OperationalLifecycleState.Incomplete:
                findings.Add(StatusFindingCode.ExtensionLifecycleIncomplete, null,
                    "The Extension lifecycle document is incomplete.");
                break;
            case OperationalLifecycleState.Blocked:
                findings.Add(StatusFindingCode.ExtensionLifecycleBlocked, null,
                    "The Extension lifecycle document is blocked.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(lifecycle), lifecycle.State,
                    "The Extension lifecycle state is not defined.");
        }

        if (lifecycle.SourceAvailability == OperationalSourceAvailability.Unavailable)
        {
            findings.Add(StatusFindingCode.ExtensionSourceUnavailable, null,
                "The installed Extension source is unavailable.");
        }

        foreach (var target in lifecycle.ManagedFiles.Targets)
        {
            AddTarget(findings, target.Path, target.State, framework: false);
        }
    }

    private static void AddTarget(
        StatusFindingCollector findings,
        string path,
        OperationalTargetState state,
        bool framework)
    {
        StatusFindingCode? code = (framework, state) switch
        {
            (_, OperationalTargetState.Current) => null,
            (true, OperationalTargetState.Changed) => StatusFindingCode.FrameworkTargetChanged,
            (true, OperationalTargetState.Missing) => StatusFindingCode.FrameworkTargetMissing,
            (true, OperationalTargetState.Unavailable) => StatusFindingCode.FrameworkTargetUnavailable,
            (true, OperationalTargetState.Blocked) => StatusFindingCode.FrameworkTargetBlocked,
            (false, OperationalTargetState.Changed) => StatusFindingCode.ExtensionTargetChanged,
            (false, OperationalTargetState.Missing) => StatusFindingCode.ExtensionTargetMissing,
            (false, OperationalTargetState.Unavailable) => StatusFindingCode.ExtensionTargetUnavailable,
            (false, OperationalTargetState.Blocked) => StatusFindingCode.ExtensionTargetBlocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state,
                "The managed target state is not defined."),
        };
        if (code.HasValue)
        {
            findings.Add(code.Value, path, "The managed lifecycle target is not current.");
        }
    }
}
