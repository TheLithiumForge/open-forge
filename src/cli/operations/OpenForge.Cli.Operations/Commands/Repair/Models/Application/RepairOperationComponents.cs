using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairOperationComponents(
    DoctorDiagnosisReader DiagnosisReader,
    RepairCatalogueReader CatalogueReader,
    RepairApplicationOperation Application,
    RepairInteraction Interaction)
{
    internal RepairMaintenancePlanner? MaintenancePlanner { get; init; }
}
