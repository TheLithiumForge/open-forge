using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.List;

internal static class RouteListPhrases
{
    // @OpenForgeText route.list.phrase.rename-to
    internal static string FormatRenameTo(string compatibilityNameText)
        => $"rename to _{compatibilityNameText}";

    // @OpenForgeText route.list.phrase.listed
    internal static string FormatListed(string countText, string pluralText)
        => $"{countText} {pluralText} listed.";

    // @OpenForgeText route.list.phrase.no-routes-under
    internal static string FormatNoRoutesUnder(string idText)
        => $"No routes under {idText}.";

    // @OpenForgeText route.list.phrase.listed-with-warnings
    internal static string FormatListedWithWarnings(string countText, string pluralText)
        => $"{countText} {pluralText} listed with warnings.";

    // @OpenForgeText route.list.phrase.cannot-list-routes
    internal static string FormatCannotListRoutes(string trimSentenceText)
        => $"Cannot list routes: {trimSentenceText}.";

    // @OpenForgeText route.list.phrase.route-list-stopped-because-of-an-unexpected-error
    internal static string FormatRouteListStoppedBecauseOfAnUnexpectedError(string trimSentenceText)
        => $"Route list stopped because of an unexpected error: {trimSentenceText}.";

    // @OpenForgeText route.list.phrase.the-frontmatter-of-could-not-be-read
    internal static string FormatTheFrontmatterOfCouldNotBeRead(string pathText, string trimSentenceText)
        => $"The frontmatter of {pathText} could not be read: {trimSentenceText}.";

    // @OpenForgeText route.list.phrase.to-depth-deeper-routes-open-forge-route-list-depth-all
    internal static string FormatToDepthDeeperRoutesOpenForgeRouteListDepthAll(string countText, string routeWordText, string machineValueText)
        => $"{countText} {routeWordText} to depth {machineValueText}. Deeper routes: open-forge route list --depth=all";
}
