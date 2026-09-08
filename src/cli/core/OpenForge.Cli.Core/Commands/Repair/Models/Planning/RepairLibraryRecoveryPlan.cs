using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairLibraryPlanningInput
{
    public required RepairRequest Request { get; init; }
    public required ImmutableArray<RepairProposalInput> References { get; init; }
    public required ImmutableArray<RepairLibraryRecoveryProposal> Libraries { get; init; }
    public required ImmutableArray<RepairRelinkRequest> WizardRelinks { get; init; }
    public required ImmutableArray<RepairLibraryRecoveryProposal>? WizardLibraries { get; init; }
}

internal sealed record RepairLibraryRecoveryEffect
{
    internal RepairLibraryRecoveryEffect(RepairSelectedLibraryRecovery selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        Selection = selection;
    }

    internal RepairSelectedLibraryRecovery Selection { get; }

    internal RecoveryEntry Entry => Selection.Proposal.Evidence.Entry.Input.Context.Entry;
}

internal sealed record RepairLibraryRecoveryStep
{
    public required int Ordinal { get; init; }
    public required RepairSelectedLibraryRecovery Selection { get; init; }
    public required RepairDependency Dependency { get; init; }
    public required RepairVerificationRequirement Verification { get; init; }
    public required RepairLibraryRecoveryEffect? Effect { get; init; }
    public required RepairStepOutcome Outcome { get; init; }
}
