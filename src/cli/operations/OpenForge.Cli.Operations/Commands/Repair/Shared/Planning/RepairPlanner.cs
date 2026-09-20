using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal static class RepairPlanner
{
    internal static RepairPlan Build(
        RepairRequest request,
        IEnumerable<RepairableReferenceInput> references)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(references);

        var inputs = references
            .Select(reference => reference ?? throw new ArgumentException(
                "Repair planning inputs cannot contain null members.",
                nameof(references)))
            .Select(RepairProposalInput.From)
            .ToArray();
        return Build(request, inputs);
    }

    internal static RepairPlan Build(
        RepairRequest request,
        IReadOnlyList<RepairProposalInput> references)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(references);
        var inputs = references
            .OrderBy(reference => reference.Proposal.SourceCanonicalPath, StringComparer.Ordinal)
            .ThenBy(reference => reference.Proposal.Occurrence.ByteOffset)
            .ThenBy(reference => reference.Proposal.Occurrence.ByteLength)
            .ToArray();
        var selection = RepairSelectionPlanner.Resolve(request, inputs);
        return RepairEffectPlanner.Build(request, selection);
    }

    internal static RepairPlan Build(
        RepairRequest request,
        IReadOnlyList<RepairProposalInput> references,
        IReadOnlyList<RepairRelinkRequest> promptRelinks)
        => RepairEffectPlanner.Build(request, RepairSelectionPlanner.Resolve(request, references, promptRelinks));

}
