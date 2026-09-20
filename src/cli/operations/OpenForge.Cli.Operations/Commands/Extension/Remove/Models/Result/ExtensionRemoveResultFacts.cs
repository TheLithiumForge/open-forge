using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

internal sealed record ExtensionRemoveGeneratedRegion
{
    internal ExtensionRemoveGeneratedRegion(
        string path,
        ExtensionRemoveGeneratedRegionState state)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension Remove generated-region state is not defined.");
        }

        Path = path;
        State = state;
    }

    internal string Path { get; }

    internal ExtensionRemoveGeneratedRegionState State { get; }
}

internal sealed record ExtensionRemoveGeneratedNavigation
{
    internal ExtensionRemoveGeneratedNavigation(
        IEnumerable<ExtensionRemoveGeneratedRegion> regions)
    {
        ArgumentNullException.ThrowIfNull(regions);
        Regions = new ReadOnlyCollection<ExtensionRemoveGeneratedRegion>([.. regions
            .Select(value => value ?? throw new ArgumentException(
                "Extension Remove generated-navigation regions cannot contain null members.",
                nameof(regions)))]);
    }

    internal IReadOnlyList<ExtensionRemoveGeneratedRegion> Regions { get; }
}

internal sealed record ExtensionRemoveLifecycle
{
    internal ExtensionRemoveLifecycle(
        ExtensionRemoveLifecycleTrust trust,
        ExtensionRemoveLifecycleCoverage coverage,
        ExtensionRemoveLifecycleAction action,
        ExtensionRemoveLifecycleOutcome outcome)
    {
        if (!Enum.IsDefined(trust))
        {
            throw new ArgumentOutOfRangeException(
                nameof(trust),
                trust,
                "The Extension Remove lifecycle trust is not defined.");
        }

        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(coverage),
                coverage,
                "The Extension Remove lifecycle coverage is not defined.");
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Extension Remove lifecycle action is not defined.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Extension Remove lifecycle outcome is not defined.");
        }

        Trust = trust;
        Coverage = coverage;
        Action = action;
        Outcome = outcome;
    }

    internal ExtensionRemoveLifecycleTrust Trust { get; }

    internal ExtensionRemoveLifecycleCoverage Coverage { get; }

    internal ExtensionRemoveLifecycleAction Action { get; }

    internal ExtensionRemoveLifecycleOutcome Outcome { get; }
}

internal sealed record ExtensionRemoveRecovery
{
    internal ExtensionRemoveRecovery(
        ExtensionRemoveRecoveryState state,
        IEnumerable<string> protectedPaths,
        string? residualPath)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Extension Remove recovery state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(protectedPaths);
        var paths = protectedPaths
            .Select(value => value ?? throw new ArgumentException(
                "Extension Remove recovery paths cannot contain null members.",
                nameof(protectedPaths)))
            .ToArray();
        if (paths.Any(string.IsNullOrWhiteSpace)
            || paths.Distinct(StringComparer.Ordinal).Count() != paths.Length)
        {
            throw new ArgumentException(
                "Extension Remove recovery paths must be unique and non-empty.",
                nameof(protectedPaths));
        }

        if (state == ExtensionRemoveRecoveryState.Retained
            && string.IsNullOrWhiteSpace(residualPath))
        {
            throw new ArgumentException(
                "A retained Extension Remove recovery bundle requires its residual path.",
                nameof(residualPath));
        }

        if ((state is ExtensionRemoveRecoveryState.NotRequired
                or ExtensionRemoveRecoveryState.NotCreated
                or ExtensionRemoveRecoveryState.Removed)
            && residualPath is not null)
        {
            throw new ArgumentException(
                "A non-retained Extension Remove recovery state cannot expose a residual path.",
                nameof(residualPath));
        }

        State = state;
        ProtectedPaths = new ReadOnlyCollection<string>(paths);
        ResidualPath = residualPath;
    }

    internal ExtensionRemoveRecoveryState State { get; }

    internal IReadOnlyList<string> ProtectedPaths { get; }

    internal string? ResidualPath { get; }
}

internal sealed record ExtensionRemoveVerification
{
    internal ExtensionRemoveVerification(
        ExtensionRemoveVerificationState targets,
        ExtensionRemoveVerificationState topology,
        ExtensionRemoveVerificationState extensionsLifecycle)
    {
        if (!Enum.IsDefined(targets))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targets),
                targets,
                "The Extension Remove target verification state is not defined.");
        }

        if (!Enum.IsDefined(topology))
        {
            throw new ArgumentOutOfRangeException(
                nameof(topology),
                topology,
                "The Extension Remove topology verification state is not defined.");
        }

        if (!Enum.IsDefined(extensionsLifecycle))
        {
            throw new ArgumentOutOfRangeException(
                nameof(extensionsLifecycle),
                extensionsLifecycle,
                "The Extension Remove lifecycle verification state is not defined.");
        }

        Targets = targets;
        Topology = topology;
        ExtensionsLifecycle = extensionsLifecycle;
    }

    internal ExtensionRemoveVerificationState Targets { get; }

    internal ExtensionRemoveVerificationState Topology { get; }

    internal ExtensionRemoveVerificationState ExtensionsLifecycle { get; }
}

internal sealed record ExtensionRemoveFinding
{
    internal ExtensionRemoveFinding(
        ExtensionRemoveFindingCode code,
        string cause,
        string? target = null)
    {
        if (!Enum.IsDefined(code))
        {
            throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The Extension Remove finding code is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (target is not null && target.Length == 0)
        {
            throw new ArgumentException("An Extension Remove finding target cannot be empty.", nameof(target));
        }

        Code = code;
        Status = ExtensionRemoveDefinitions.ReadStatus(code);
        Target = target;
        Cause = cause;
    }

    internal ExtensionRemoveFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string? Target { get; }

    internal string Cause { get; }
}

internal sealed record ExtensionRemoveResultFacts
{
    internal required ExtensionRemoveSelection? Selection { get; init; }

    internal required ExtensionRemoveDependencyPlan? Dependencies { get; init; }

    internal required IReadOnlyList<ExtensionRemovePathPlan> Paths { get; init; }

    internal required ExtensionRemoveGeneratedNavigation? GeneratedNavigation { get; init; }

    internal required IReadOnlyList<ExtensionRemoveEffect> Effects { get; init; }

    internal WorkspacePermissionResult Permissions { get; init; } = WorkspacePermissionResult.NotEvaluated;

    internal required ExtensionRemoveLifecycle Lifecycle { get; init; }

    internal required ExtensionRemoveRecovery Recovery { get; init; }

    internal required ExtensionRemoveVerification Verification { get; init; }

    internal required bool PackageSourceUnchanged { get; init; }
}

internal sealed record ExtensionRemoveResultFormation
{
    internal required CliWorkspace? Workspace { get; init; }

    internal required ExtensionRemoveMode Mode { get; init; }


    internal required bool Automatic { get; init; }

    internal required ExtensionRemoveResultFacts Facts { get; init; }

    internal required IReadOnlyList<ExtensionRemoveFinding> Findings { get; init; }
}
