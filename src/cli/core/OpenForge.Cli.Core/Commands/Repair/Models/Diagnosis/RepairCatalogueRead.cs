using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;

internal sealed record RepairCatalogueRead(
    IReadOnlyList<RepairProposalInput> Proposals,
    IReadOnlyList<RepairFinding> Findings);
