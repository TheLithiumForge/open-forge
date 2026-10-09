using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Shared;

internal static class RouteSharedPhrases
{
    internal static string FormatBefore(string beforeText)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.BeforeHash(beforeText);

    internal static string FormatAfter(string afterText)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.AfterHash(afterText);

    // @OpenForgeText route.shared.phrase.the-extension
    internal static string FormatTheExtension(string ownerText)
        => $"the {ownerText} Extension";

    // @OpenForgeText route.shared.phrase.matches-sources-which-one
    internal static string InspectSelection(string reference, int count)
        => string.Create(CultureInfo.InvariantCulture, $"\"{reference}\" matches {count} paths. Choose the one to inspect.");

    // @OpenForgeText route.shared.phrase.matches-paths-to-move
    internal static string MoveSelection(string reference, int count)
        => string.Create(CultureInfo.InvariantCulture, $"\"{reference}\" matches {count} paths. Choose the one to move.");

    // @OpenForgeText route.shared.phrase.matches-paths-to-remove
    internal static string RemoveSelection(string reference, int count)
        => string.Create(CultureInfo.InvariantCulture, $"\"{reference}\" matches {count} paths. Choose the one to remove.");

    // @OpenForgeText route.shared.phrase.matches-paths-to-update
    internal static string UpdateSelection(string reference, int count)
        => string.Create(CultureInfo.InvariantCulture, $"\"{reference}\" matches {count} paths. Choose the one to update.");
}
