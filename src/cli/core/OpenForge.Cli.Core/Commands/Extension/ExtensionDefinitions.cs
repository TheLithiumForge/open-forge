using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension;

internal static class ExtensionDefinitions
{
    internal static readonly CliSyntaxDefinition ExtensionGroup = new(
        name: "extension",
        description: "Inspect and manage optional Open Forge Extension packages.");
}
