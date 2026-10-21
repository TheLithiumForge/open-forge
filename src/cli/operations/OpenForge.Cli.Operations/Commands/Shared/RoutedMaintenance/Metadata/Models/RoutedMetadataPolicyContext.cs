using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;

internal sealed record RoutedMetadataPolicyContext
{
    public required WorkspaceSettingsDocument Settings { get; init; }
    public required WorkspaceOwnershipDocument Ownership { get; init; }
}
