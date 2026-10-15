using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;

internal static class ReferenceRenameDemand
{
    internal static bool RequiresEvidence(SourceCatalogue catalogue, PhysicalPathResolver paths,
        IEnumerable<RenameRecordedIdentity> records, bool hasMissingTarget, CancellationToken token)
    {
        if (hasMissingTarget || RoutedMaintenanceOldNavigationReader.Read(catalogue).Any()) return true;
        foreach (var record in records)
        {
            token.ThrowIfCancellationRequested();
            if (catalogue.FindByPath(record.Path) is not null) continue;
            var logical = SourceLogicalPath.ToLexicalPath(catalogue.Workspace.LexicalRoot, record.Path);
            if (NoFollowLeafObserver.Observe(paths, catalogue.Workspace, logical, token).State == NoFollowLeafState.Missing)
                return true;
        }
        return false;
    }
}
