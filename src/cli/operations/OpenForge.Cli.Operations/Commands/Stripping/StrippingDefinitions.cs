using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Stripping;

/// <summary>
/// Input vocabulary shared by uninstall and export. The plain-docs Interface contract
/// defines the selection flags and part names for both commands.
/// </summary>
internal static class StrippingDefinitions
{
    internal const string UninstallCommandIdentity = "uninstall";
    internal const string ExportCommandIdentity = "export";
    internal const char PartSeparator = ',';
    internal const string StripOption = "--strip";
    internal const string FlattenOption = "--flatten";
    internal const string RemoveEditedOption = "--remove-edited";
    internal const string DryRunOption = "--dry-run";
    internal const string AutomaticOption = "--automatic";

    internal static readonly CliOptionDefinition<string[]> Strip = new(StripOption, "Strip metadata, frontmatter, entrypoints or links from the documents that stay. Repeat or use commas.", CliOptionArity.ExactlyOne, [], "parts");
    internal static readonly CliOptionDefinition<string?> Flatten = new(FlattenOption, "Move documents below this folder's subfolders into the folder itself.", CliOptionArity.ExactlyOne, null, "folder");
    internal static readonly CliOptionDefinition<string[]> RemoveEdited = new(RemoveEditedOption, "Remove this managed file or section even though you edited it. Repeat for more.", CliOptionArity.ExactlyOne, [], "path");
    internal static readonly CliOptionDefinition<bool> DryRun = new(DryRunOption, "List every delete, rewrite and move with hashes, without writing.", CliOptionArity.None, false);

    internal static string CommandIdentity(StripCommandKind kind)
        => kind switch
        {
            StripCommandKind.Uninstall => UninstallCommandIdentity,
            StripCommandKind.Export => ExportCommandIdentity,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The plan command kind is not defined."),
        };

    // The shared message families use the command name at the start of a sentence.
    internal static string CommandTitle(StripCommandKind kind)
    {
        var identity = CommandIdentity(kind);
        return char.ToUpperInvariant(identity[0]) + identity[1..];
    }

    internal static string PartList()
        => string.Join(", ", Enum.GetValues<StripPart>().SkipLast(1).Select(PartName)) + " or " + PartName(Enum.GetValues<StripPart>()[^1]);

    internal static string PartName(StripPart part)
        => part switch
        {
            StripPart.Metadata => "metadata",
            StripPart.Frontmatter => "frontmatter",
            StripPart.Entrypoints => "entrypoints",
            StripPart.Links => "links",
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, "The strip part is not defined."),
        };
}
