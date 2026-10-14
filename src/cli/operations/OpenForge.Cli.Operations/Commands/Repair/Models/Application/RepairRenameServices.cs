using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairRenameServices(RepairRenameRead Read, IRoutedMaintenanceRenameEvidence Scaffolding);
