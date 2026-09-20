using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Extension;

internal static class ExtensionDefinitions
{
    internal static readonly CliSyntaxDefinition ExtensionGroup = new(
        name: "extension",
        description: "Manage optional Open Forge Extension packages.");
}
