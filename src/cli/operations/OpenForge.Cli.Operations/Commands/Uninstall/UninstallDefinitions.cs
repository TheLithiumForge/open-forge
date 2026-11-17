using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Uninstall;

internal static class UninstallDefinitions
{
    internal const string CommandIdentity = StrippingDefinitions.UninstallCommandIdentity;

    internal static readonly CliSyntaxDefinition Command = new(CommandIdentity, "Remove Open Forge from this workspace and keep the files you wrote.");
    internal static readonly CliOptionDefinition<string[]> Strip = new(StrippingDefinitions.StripOption, "Also strip metadata, frontmatter, entrypoints or links from the documents that stay. Repeat or use commas.", CliOptionArity.ExactlyOne, [], "parts");
    internal static readonly CliOptionDefinition<string?> Flatten = new(StrippingDefinitions.FlattenOption, "Move documents below this folder's subfolders into the folder itself.", CliOptionArity.ExactlyOne, null, "folder");
    internal static readonly CliOptionDefinition<string[]> RemoveEdited = new(StrippingDefinitions.RemoveEditedOption, "Remove this managed file or section even though you edited it. Repeat for more.", CliOptionArity.ExactlyOne, [], "path");
    internal static readonly CliOptionDefinition<bool> DryRun = new(StrippingDefinitions.DryRunOption, "List every delete, rewrite and move with hashes, without writing.", CliOptionArity.None, false);
    internal static readonly CliOptionDefinition<bool> Automatic = new(StrippingDefinitions.AutomaticOption, "Apply the plan without a confirmation prompt.", CliOptionArity.None, false);
}
