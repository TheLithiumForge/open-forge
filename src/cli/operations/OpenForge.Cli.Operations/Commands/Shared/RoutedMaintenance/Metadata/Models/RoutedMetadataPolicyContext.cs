using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal sealed record RoutedMetadataPolicyContext
{
    public required WorkspaceLayout Layout { get; init; }
    public WorkspaceLayoutRead? LayoutRead { get; init; }
    internal SourceContentRoots Roots => new(Layout);
    public required WorkspaceSettingsDocument Settings { get; init; }
    public required WorkspaceOwnershipDocument Ownership { get; init; }

    internal WorkspaceOutsideRouteRead? AvailableOutsideBoundary(WorkspaceMaintenanceBoundary boundary)
    {
        if (LayoutRead is not { State: WorkspaceLayoutReadState.Ready or WorkspaceLayoutReadState.Uninstalled } observation
            || !ReferenceEquals(observation.Layout, Layout)) return null;
        return observation.OutsideRoutes.SingleOrDefault(route => route.State == WorkspaceOutsideRouteReadState.Available
            && route.Descriptor.DirectoryPath == boundary.DirectoryPath && route.Descriptor.CataloguePath == boundary.AnchorPath
            && route.PhysicalDirectory is not null);
    }
}
