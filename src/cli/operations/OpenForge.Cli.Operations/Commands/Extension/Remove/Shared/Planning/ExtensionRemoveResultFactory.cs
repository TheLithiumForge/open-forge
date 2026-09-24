using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal static class ExtensionRemoveResultFactory
{
    internal static ExtensionRemoveResult Create(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection? selection,
        ExtensionRemoveDependencyPlan? dependencies,
        IReadOnlyList<ExtensionRemovePathPlan> paths,
        ExtensionRemoveGeneratedNavigation? navigation,
        IReadOnlyList<ExtensionRemoveEffect> effects,
        ExtensionRemoveLifecycle lifecycle,
        ExtensionRemoveRecovery recovery,
        ExtensionRemoveVerification verification,
        IReadOnlyList<ExtensionRemoveFinding> findings,
        ExtensionRemoveEffect? settingsEffect = null)
        => new(new ExtensionRemoveResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Automatic = request.Automatic,
            Facts = new ExtensionRemoveResultFacts
            {
                Selection = selection,
                Dependencies = dependencies,
                Paths = paths,
                GeneratedNavigation = navigation,
                Effects = settingsEffect is null ? effects : [.. effects, settingsEffect],
                Lifecycle = lifecycle,
                Recovery = recovery,
                Verification = verification,
                PackageSourceUnchanged = true,
            },
            Findings = findings,
        });

    internal static ExtensionRemoveResult CreateBoundary(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection? selection,
        ExtensionRemoveDependencyPlan? dependencies,
        params ExtensionRemoveFinding[] findings)
        => Create(
            request,
            selection,
            dependencies,
            [],
            navigation: null,
            [],
            new ExtensionRemoveLifecycle(
                ReadTrust(findings),
                ReadCoverage(findings),
                ExtensionRemoveLifecycleAction.None,
                ExtensionRemoveLifecycleOutcome.NotRequested),
            new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.NotRequired,
                [],
                residualPath: null),
            new ExtensionRemoveVerification(
                ExtensionRemoveVerificationState.NotRequested,
                ExtensionRemoveVerificationState.NotRequested,
                ExtensionRemoveVerificationState.NotRequested),
            findings);

    internal static ExtensionRemoveResult PlanBoundary(
        ExtensionRemovePlan plan,
        ExtensionRemoveResult planned,
        ExtensionRemoveFindingCode code,
        string cause,
        ExtensionRemoveRecovery? recovery = null)
        => Create(
            plan.Request,
            plan.Selection,
            plan.Dependencies,
            planned.Paths,
            planned.GeneratedNavigation,
            ExtensionRemoveApplicationResultFactory.NotStartedEffects(plan),
            ExtensionRemoveApplicationResultFactory.Lifecycle(
                plan,
                plan.OwnershipChange is null
                    ? ExtensionRemoveLifecycleOutcome.AlreadyCurrent
                    : ExtensionRemoveLifecycleOutcome.NotStarted),
            recovery ?? new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.NotCreated,
                [],
                residualPath: null),
            new ExtensionRemoveVerification(
                ExtensionRemoveVerificationState.NotRequested,
                ExtensionRemoveVerificationState.NotRequested,
                ExtensionRemoveVerificationState.NotRequested),
            planned.Findings.Append(new ExtensionRemoveFinding(code, cause)).ToArray(),
            plan.SettingsEffect is { } settingsEffect
                ? ExtensionRemoveApplicationResultFactory.WithOutcome(
                    settingsEffect,
                    ExtensionRemoveEffectOutcome.NotStarted)
                : null);

    private static ExtensionRemoveLifecycleTrust ReadTrust(
        IReadOnlyList<ExtensionRemoveFinding> findings)
    {
        if (findings.Any(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleBlocked))
        {
            return ExtensionRemoveLifecycleTrust.Blocked;
        }

        if (findings.Any(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleUnavailable))
        {
            return ExtensionRemoveLifecycleTrust.Unavailable;
        }

        return ExtensionRemoveLifecycleTrust.NotRequested;
    }

    private static ExtensionRemoveLifecycleCoverage ReadCoverage(
        IReadOnlyList<ExtensionRemoveFinding> findings)
    {
        if (findings.Any(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleBlocked))
        {
            return ExtensionRemoveLifecycleCoverage.Blocked;
        }

        if (findings.Any(finding => finding.Code == ExtensionRemoveFindingCode.LifecycleUnavailable))
        {
            return ExtensionRemoveLifecycleCoverage.Incomplete;
        }

        return ExtensionRemoveLifecycleCoverage.NotRequested;
    }
}
