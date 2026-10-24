using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;

// Slice R supplies and revalidates optional identity evidence. Absence never blocks ordinary scaffolding.
internal interface IRoutedMaintenanceRenameEvidence
{
    ValueTask<RouteScaffoldRenameProof?> ReadAsync(WorkspaceLayout layout, RouteScaffoldRenameCandidate candidate, CancellationToken cancellationToken);
    ValueTask<bool> VerifyAsync(WorkspaceLayout layout, RouteScaffoldRenameProof proof, CancellationToken cancellationToken);
}
