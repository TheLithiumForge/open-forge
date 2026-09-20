using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal static class ExtensionUpdateResultFormationFactory
{
    internal static ExtensionUpdateResult Create(
        ExtensionUpdateRequest request,
        ExtensionUpdateResultFacts facts,
        IReadOnlyList<ExtensionUpdateFinding> findings)
        => new(new ExtensionUpdateResultFormation
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Force = request.Force,
            Prune = request.Prune,
            Automatic = request.Automatic,
            Facts = facts,
            Findings = findings,
        });

    internal static ExtensionUpdateResultFacts EmptyFacts(
        ExtensionUpdateSelection? selection,
        ExtensionUpdateSource? source)
        => new()
        {
            Selection = selection,
            Source = source,
            Packages = [],
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new ExtensionUpdateLifecycle(
                ExtensionUpdateLifecycleTrust.NotRequested,
                ExtensionUpdateLifecycleCoverage.NotRequested,
                ExtensionUpdateLifecycleAction.None,
                ExtensionUpdateLifecycleOutcome.NotRequested),
            Recovery = new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.NotRequired,
                [],
                residualPath: null),
            Verification = new ExtensionUpdateVerification(
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested,
                ExtensionUpdateVerificationState.NotRequested),
        };

    internal static ExtensionUpdateResultFacts Facts(
        ExtensionUpdateResultFactsInput input)
        => new()
        {
            Selection = input.Selection,
            Source = input.Source,
            Packages = input.Packages,
            Comparisons = input.Comparisons,
            GeneratedNavigation = new ExtensionUpdateGeneratedNavigation(input.Topology.Regions),
            Effects = input.Effects,
            Lifecycle = new ExtensionUpdateLifecycle(
                ExtensionUpdateLifecycleTrust.Trusted,
                ExtensionUpdateLifecycleCoverage.Complete,
                input.LifecycleAction,
                input.LifecycleOutcome),
            Recovery = new ExtensionUpdateRecovery(input.RecoveryState, [], residualPath: null),
            Verification = new ExtensionUpdateVerification(
                input.Verification,
                input.Verification,
                input.Verification),
        };
}
