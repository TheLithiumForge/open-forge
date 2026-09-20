using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairSelectionPlanner
{
    internal static RepairPlanningSelection Resolve(
        RepairRequest request,
        IReadOnlyList<RepairProposalInput> inputs,
        IReadOnlyList<RepairRelinkRequest>? promptRelinks = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inputs);
        var entries = inputs
            .GroupBy(OccurrenceIdentity)
            .Select(Combine)
            .OrderBy(entry => entry.Input.Proposal.SourceCanonicalPath, StringComparer.Ordinal)
            .ThenBy(entry => entry.Input.Proposal.Occurrence.ByteOffset)
            .ToArray();
        var selected = new List<RepairPlanningSelectionEntry>();
        var unselected = new List<RepairProposal>();
        var conflicts = entries.SelectMany(entry => entry.Conflicts).ToList();

        var relinks = promptRelinks ?? request.Relinks;
        foreach (var entry in entries)
        {
            var explicitRelink = relinks.SingleOrDefault(relink =>
                MatchesOccurrence(relink, entry.Input.Proposal));
            var selection = Select(request, entry.Input, explicitRelink, promptRelinks is not null);
            if (entry.Conflicts.Count != 0 || selection is null)
            {
                unselected.Add(entry.Input.Proposal);
                continue;
            }

            selected.Add(selection);
        }

        foreach (var relink in relinks.Where(relink =>
            !selected.Any(entry => MatchesOccurrence(relink, entry.Selected.Proposal))))
        {
            var proposal = entries
                .Where(entry => MatchesOccurrence(relink, entry.Input.Proposal))
                .Select(entry => entry.Input.Proposal)
                .SingleOrDefault();
            conflicts.Add(new RepairConflict(
                RepairConflictKind.SelectionUnmatched,
                proposal?.SourceCanonicalPath ?? relink.SourceCanonicalPath,
                proposal?.Occurrence,
                "The explicit relink does not match one current admitted Repair proposal."));
        }

        return new RepairPlanningSelection(
            new RepairSelection(
                request.SelectionMode,
                selected.Select(entry => entry.Selected),
                unselected,
                RepairLibrarySelection.Empty),
            selected,
            conflicts);
    }

    private static RepairPlanningSelectionEntry? Select(
        RepairRequest request,
        RepairProposalInput input,
        RepairRelinkRequest? relink,
        bool promptAnswered)
    {
        var proposal = input.Proposal;
        var exactRelink = relink is not null
            && string.Equals(relink.ExpectedDestination, proposal.ExpectedDestination, StringComparison.Ordinal);
        var resolution = proposal.Resolution;
        if (proposal.Candidates is { } candidates && exactRelink && relink is not null)
        {
            var candidate = candidates.Items.SingleOrDefault(value => MatchesTarget(value.Target, relink.Target));
            resolution = candidate is null
                ? null
                : new RepairProposalResolution(
                    RepairDestinationFormatter.Format(proposal.SourceCanonicalPath, candidate.Target), candidate.Target);
        }

        if (resolution is null)
        {
            return null;
        }

        if (relink is not null && (!exactRelink || !MatchesTarget(resolution.Target, relink.Target)))
        {
            return null;
        }

        var origins = new List<RepairSelectionOrigin>();
        if (request.Automatic && proposal.IsSafeExact)
        {
            origins.Add(RepairSelectionOrigin.Automatic);
        }

        if (exactRelink && relink is not null && MatchesTarget(resolution.Target, relink.Target))
        {
            origins.Add(promptAnswered ? RepairSelectionOrigin.Prompt : RepairSelectionOrigin.ExplicitRelink);
        }
        else if (!promptAnswered && request.SelectionMode == RepairSelectionMode.InteractivePrompt && proposal.IsSafeExact)
        {
            origins.Add(RepairSelectionOrigin.Prompt);
        }

        if (origins.Count == 0)
        {
            return null;
        }

        var selected = new RepairSelectedProposal(proposal, resolution, origins);
        var planningInput = new RepairableReferenceInput(
            input.OccurrenceState,
            new RepairDestinationTransition(proposal.ExpectedDestination, input.ObservedDestination, resolution.IntendedDestination),
            proposal.Member,
            origins[0],
            resolution.Target);
        return new RepairPlanningSelectionEntry(planningInput, selected);
    }

    private static RepairSelectionEntry Combine(
        IGrouping<RepairOccurrenceIdentity, RepairProposalInput> group)
    {
        var values = group.ToArray();
        var first = values[0];
        var conflicts = values.Skip(1).All(value => Equivalent(first, value))
            ? Array.Empty<RepairConflict>()
            :
            [
                new RepairConflict(
                    RepairConflictKind.TargetIdentityMismatch,
                    first.Proposal.SourceCanonicalPath,
                    first.Proposal.Occurrence,
                    "The current Repair facts disagree for one source occurrence."),
            ];
        return new RepairSelectionEntry(first, conflicts);
    }

    private static bool Equivalent(RepairProposalInput left, RepairProposalInput right)
        => left.Proposal == right.Proposal
            && left.OccurrenceState.ObservedFileState.Expectation
                == right.OccurrenceState.ObservedFileState.Expectation
            && string.Equals(left.ObservedDestination, right.ObservedDestination, StringComparison.Ordinal);

    private static bool MatchesOccurrence(RepairRelinkRequest relink, RepairProposal proposal)
        => string.Equals(relink.SourceCanonicalPath, proposal.SourceCanonicalPath, StringComparison.Ordinal)
            && relink.Line == proposal.Occurrence.Line
            && relink.Column == proposal.Occurrence.Column;

    private static bool MatchesTarget(RepairTargetSelection left, RepairTargetSelection right)
        => string.Equals(left.CanonicalTargetPath, right.CanonicalTargetPath, StringComparison.Ordinal)
            && string.Equals(left.TargetFragment, right.TargetFragment, StringComparison.Ordinal);

    private static RepairOccurrenceIdentity OccurrenceIdentity(RepairProposalInput input)
        => new(
            input.Proposal.SourceCanonicalPath,
            input.Proposal.Occurrence.Line,
            input.Proposal.Occurrence.Column,
            input.Proposal.Occurrence.ByteOffset,
            input.Proposal.Occurrence.ByteLength);

}
