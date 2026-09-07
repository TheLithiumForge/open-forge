using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Planning;

internal sealed record RepairProposalInput
{
    internal RepairProposalInput(
        RepairProposal proposal,
        RepairOccurrenceState occurrenceState,
        string observedDestination)
    {
        ArgumentNullException.ThrowIfNull(proposal);
        ArgumentNullException.ThrowIfNull(occurrenceState);
        ArgumentException.ThrowIfNullOrWhiteSpace(observedDestination);
        if (!string.Equals(
                proposal.SourceCanonicalPath,
                occurrenceState.SourceCanonicalPath,
                StringComparison.Ordinal)
            || proposal.Occurrence != occurrenceState.Occurrence)
        {
            throw new ArgumentException(
                "A Repair proposal input must retain one exact source occurrence.",
                nameof(occurrenceState));
        }

        Proposal = proposal;
        OccurrenceState = occurrenceState;
        ObservedDestination = observedDestination;
    }

    internal RepairProposal Proposal { get; }

    internal RepairOccurrenceState OccurrenceState { get; }

    internal string ObservedDestination { get; }

    internal static RepairProposalInput From(RepairableReferenceInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var proposal = input.CatalogueMember == RepairCatalogueMember.MissingTargetRelink
            ? new RepairProposal(
                input.CatalogueMember,
                input.SourceCanonicalPath,
                input.Occurrence,
                input.ExpectedDestination,
                new RepairCandidateSet([new RepairCandidate(input.Target, input.Target.CandidateProvenance)]))
            : new RepairProposal(
                input.CatalogueMember,
                input.SourceCanonicalPath,
                input.Occurrence,
                input.ExpectedDestination,
                input.IntendedDestination,
                input.Target);
        return new RepairProposalInput(proposal, input.OccurrenceState, input.ObservedDestination);
    }
}
