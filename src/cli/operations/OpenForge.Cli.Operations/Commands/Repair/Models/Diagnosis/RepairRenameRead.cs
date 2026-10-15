using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;

internal delegate ValueTask<RenameEvidenceResult> RepairRenameRead(CliWorkspace workspace, CancellationToken cancellationToken);

internal delegate ValueTask<RenameEvidenceResult> RepairRenameReferenceRead(
    CliWorkspace workspace, LocalReferenceDoctorView references, CancellationToken cancellationToken);
