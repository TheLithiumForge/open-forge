using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal sealed record RoutedMetadataPolicyContext
{
    public WorkspaceLayout? Layout { get; init; }
    internal SourceContentRoots Roots => Layout is { } layout ? new SourceContentRoots(layout) : SourceContentRoots.Default;
    public required WorkspaceSettingsDocument Settings { get; init; }
    public required WorkspaceOwnershipDocument Ownership { get; init; }
}
