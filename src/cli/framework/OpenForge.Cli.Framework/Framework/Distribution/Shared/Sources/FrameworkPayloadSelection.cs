namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Sources;

internal static class FrameworkPayloadSelection
{
    internal static bool IncludesPath(string path, IEnumerable<string> removedCategories)
        => !removedCategories.Any(category => !string.IsNullOrWhiteSpace(category)
            && path.StartsWith($".agents/{category}/", StringComparison.Ordinal));
}
