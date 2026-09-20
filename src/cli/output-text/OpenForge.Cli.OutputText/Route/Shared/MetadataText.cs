using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Shared;

internal static class MetadataText
{
    // @OpenForgeText route.metadata.tags
    internal static string Tags(string tags)
        => $"tags: {tags}";
}
