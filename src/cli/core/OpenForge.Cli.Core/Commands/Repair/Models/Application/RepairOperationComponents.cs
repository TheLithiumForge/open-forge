using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairOperationComponents(
    DoctorDiagnosisReader DiagnosisReader,
    RepairCatalogueReader CatalogueReader,
    RepairApplicationOperation Application,
    CliInteractiveSession InteractiveSession);
