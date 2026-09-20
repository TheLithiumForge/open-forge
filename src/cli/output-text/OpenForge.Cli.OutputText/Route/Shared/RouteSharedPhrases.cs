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
    internal static string FormatMatchesSourcesWhichOne(string requestedReferenceText, string candidateCountText)
        => $"{requestedReferenceText} matches {candidateCountText} sources. Which one?";
}
