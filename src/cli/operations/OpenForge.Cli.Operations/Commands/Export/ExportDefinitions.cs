using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Export;

// The selection flags live in StrippingDefinitions, because uninstall and export share them.
internal static class ExportDefinitions
{
    internal const string CommandIdentity = StrippingDefinitions.ExportCommandIdentity;

    internal static readonly CliSyntaxDefinition Command = new(CommandIdentity, "Write a copy of this project without Open Forge to a new folder.");
    internal static readonly CliSyntaxDefinition Destination = new("destination", "A new or empty folder for the copy. A folder outside the project is recommended.");
}
