using OpenForge.Cli.Core.Commands.Stripping.Models.Request;

namespace OpenForge.Cli.Core.Commands.Stripping;

/// <summary>
/// Input vocabulary shared by strip and uninstall. The strip Interface contract
/// defines the command identities, options and part names for both commands.
/// </summary>
internal static class StrippingDefinitions
{
    internal const string StripCommandIdentity = "strip";
    internal const string UninstallCommandIdentity = "uninstall";
    internal const char PartSeparator = ',';
    internal const string StripOption = "--strip";
    internal const string FlattenOption = "--flatten";
    internal const string RemoveEditedOption = "--remove-edited";
    internal const string DryRunOption = "--dry-run";
    internal const string AutomaticOption = "--automatic";

    internal static string CommandIdentity(StripCommandKind kind)
        => kind switch
        {
            StripCommandKind.Strip => StripCommandIdentity,
            StripCommandKind.Uninstall => UninstallCommandIdentity,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The strip command kind is not defined."),
        };

    internal static string PartName(StripPart part)
        => part switch
        {
            StripPart.Metadata => "metadata",
            StripPart.Entries => "entries",
            StripPart.Links => "links",
            _ => throw new ArgumentOutOfRangeException(nameof(part), part, "The strip part is not defined."),
        };
}
