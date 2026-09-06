using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Binding;

internal static class UpdateInvalidResultFactory
{
    internal static UpdateResult WorkspaceUnavailable(UpdateBindingInput input)
        => Create(
            input,
            new UpdateFinding(
                UpdateFindingCode.WorkspaceUnavailable,
                target: null,
                "The selected workspace is unavailable."));

    internal static UpdateResult Invalid(
        UpdateBindingInput input,
        string cause)
        => Create(
            input,
            new UpdateFinding(
                UpdateFindingCode.InvalidInput,
                target: null,
                cause));

    private static UpdateResult Create(
        UpdateBindingInput input,
        UpdateFinding finding)
        => new(new UpdateResultFormation
        {
            Workspace = null,
            Mode = input.Mode,
            Force = input.Force,
            Prune = input.Prune,
            Automatic = input.Automatic,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [finding],
        });
}
