using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveLinkScopePlanner
{
    internal static PlannedFileChange? Build(RouteMoveResolvedDestination destination)
    {
        if (destination.Inventory.Settings?.Snapshot is not { } snapshot) return null;
        var layers = destination.Subject.Layers.Select(layer => (Source: layer.SourcePath, Destination: layer.DestinationPath)).ToArray();
        (string Source, string Destination)? folderMove = null;
        if (destination.Subject.Kind == RouteMoveSubjectKind.Category)
            folderMove = (SourceLogicalPath.ReadParent(destination.Inventory.Subject.Source.Path
                ?? throw new InvalidOperationException("A move subject requires its path.")),
                SourceLogicalPath.ReadParent(destination.Destination.Path
                ?? throw new InvalidOperationException("A move destination requires its path.")));
        var bytes = WorkspaceSettingsCodec.MoveLinkScope(snapshot.Bytes.ToArray(), layers, folderMove);
        return bytes is null ? null : PlannedFileChange.Replace(snapshot.Expectation, bytes);
    }
}
