using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;

internal sealed record RepairCatalogueRead(
    IReadOnlyList<RepairProposalInput> Proposals,
    IReadOnlyList<RepairFinding> Findings)
{
    internal WorkspaceLayoutRead? LayoutRead { get; init; }
    internal RenameEvidenceResult? RenameEvidence { get; init; }
    internal SourceCatalogue? Catalogue { get; init; }
    internal IReadOnlyDictionary<string, MarkdownDocumentFacts> Documents { get; init; } = new Dictionary<string, MarkdownDocumentFacts>();
    internal IReadOnlyList<LocalReferenceObservation> MissingTargetReferences { get; init; } = [];
}
