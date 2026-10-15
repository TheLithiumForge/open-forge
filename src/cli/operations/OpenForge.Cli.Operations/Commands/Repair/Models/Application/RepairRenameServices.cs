using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairRenameServices(RepairRenameRead Read, IRoutedMaintenanceRenameEvidence Scaffolding)
{
    internal RepairRenameReferenceRead? ReadReferences { get; init; }
    internal RenameEvidenceReader? DiagnosisReader { get; init; }
}
