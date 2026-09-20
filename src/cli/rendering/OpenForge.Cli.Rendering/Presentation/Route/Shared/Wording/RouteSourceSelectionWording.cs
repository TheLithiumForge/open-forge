using System.Globalization;

namespace OpenForge.Cli.Core.Presentation.Route.Shared.Wording;

internal static class RouteSourceSelectionWording
{
    internal static string SourceSelection(string requestedReference, int candidateCount)
        => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedPhrases.FormatMatchesSourcesWhichOne(string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{requestedReference}"), string.Create(global::System.Globalization.CultureInfo.InvariantCulture, $"{candidateCount}"));
}
