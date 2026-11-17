using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Strip;

internal static class StripDefinitions
{
    internal const string CommandIdentity = StrippingDefinitions.StripCommandIdentity;

    internal static readonly CliSyntaxDefinition Command = new(CommandIdentity, "Strip Open Forge metadata, frontmatter, entrypoints or links from routed documents, or flatten them into one folder.");
    internal static readonly CliSyntaxDefinition Parts = new("part", "Select metadata, frontmatter, entrypoints or links. Separate several with spaces or commas.");
    internal static readonly CliOptionDefinition<string?> Flatten = new(StrippingDefinitions.FlattenOption, "Move documents below this folder's subfolders into the folder itself.", CliOptionArity.ExactlyOne, null, "folder");
    internal static readonly CliOptionDefinition<bool> DryRun = new(StrippingDefinitions.DryRunOption, "List every delete, rewrite and move with hashes, without writing.", CliOptionArity.None, false);
    internal static readonly CliOptionDefinition<bool> Automatic = new(StrippingDefinitions.AutomaticOption, "Apply the plan without a confirmation prompt.", CliOptionArity.None, false);
}
