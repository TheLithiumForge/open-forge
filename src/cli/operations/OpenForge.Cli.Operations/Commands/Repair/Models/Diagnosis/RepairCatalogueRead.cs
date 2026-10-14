using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;

internal sealed record RepairCatalogueRead(
    IReadOnlyList<RepairProposalInput> Proposals,
    IReadOnlyList<RepairFinding> Findings)
{
    internal RenameEvidenceResult? RenameEvidence { get; init; }
    internal IReadOnlyList<LocalReferenceObservation> MissingTargetReferences { get; init; } = [];
}
