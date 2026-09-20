using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class ContentPartsWording
{
    // @OpenForgeText shared.wording.included-because
    internal static string IncludedBecause(string reason)
        => $"included because {reason}";

    // @OpenForgeText shared.wording.route
    internal static string Route(string route)
        => $"route: {route}";

    // @OpenForgeText shared.wording.scope
    internal static string Scope(string scope)
        => $"scope: {scope}";

    // @OpenForgeText shared.wording.order
    internal static string Order(int order)
        => string.Create(CultureInfo.InvariantCulture, $"order: {order}");

    // @OpenForgeText shared.wording.layer
    internal static string Layer(string layer)
        => $"layer: {layer}";
}
