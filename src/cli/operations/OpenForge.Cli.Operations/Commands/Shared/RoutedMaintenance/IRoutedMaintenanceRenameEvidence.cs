using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

// Slice R supplies and revalidates optional identity evidence. Absence never blocks ordinary scaffolding.
internal interface IRoutedMaintenanceRenameEvidence
{
    ValueTask<RouteScaffoldRenameProof?> ReadAsync(CliWorkspace workspace, RouteScaffoldRenameCandidate candidate, CancellationToken cancellationToken);
    ValueTask<bool> VerifyAsync(CliWorkspace workspace, RouteScaffoldRenameProof proof, CancellationToken cancellationToken);
}
