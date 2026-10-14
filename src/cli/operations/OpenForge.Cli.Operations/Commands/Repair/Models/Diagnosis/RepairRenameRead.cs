using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;

internal delegate ValueTask<RenameEvidenceResult> RepairRenameRead(CliWorkspace workspace, CancellationToken cancellationToken);
