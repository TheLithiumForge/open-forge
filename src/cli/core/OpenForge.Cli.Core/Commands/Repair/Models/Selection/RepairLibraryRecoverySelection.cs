using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Selection;

internal sealed record RepairLibraryRecoveryProposal
{
    internal RepairLibraryRecoveryProposal(LibraryResidualEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        Evidence = evidence;
    }

    internal LibraryResidualEvidence Evidence { get; }
}

internal sealed record RepairSelectedLibraryRecovery
{
    internal RepairSelectedLibraryRecovery(RepairLibraryRecoveryProposal proposal, ImmutableArray<RepairSelectionOrigin> origins)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        if (origins.IsDefaultOrEmpty || origins.Any(origin => origin is not RepairSelectionOrigin.Automatic and not RepairSelectionOrigin.Wizard))
        {
            throw new ArgumentException("A selected Library residual requires explicit defined selection origins.", nameof(origins));
        }

        Proposal = proposal;
        Origins = origins;
    }

    internal RepairLibraryRecoveryProposal Proposal { get; }
    internal ImmutableArray<RepairSelectionOrigin> Origins { get; }
}

internal sealed record RepairLibrarySelection
{
    internal RepairLibrarySelection(
        ImmutableArray<RepairSelectedLibraryRecovery> selected,
        ImmutableArray<RepairLibraryRecoveryProposal> unselected)
    {
        if (selected.IsDefault || unselected.IsDefault || selected.Any(value => value is null) || unselected.Any(value => value is null))
        {
            throw new ArgumentException("Library selection requires initialized immutable proposal sets.", nameof(selected));
        }

        Selected = selected;
        Unselected = unselected;
    }

    internal ImmutableArray<RepairSelectedLibraryRecovery> Selected { get; }
    internal ImmutableArray<RepairLibraryRecoveryProposal> Unselected { get; }
    internal static RepairLibrarySelection Empty { get; } = new([], []);
}
