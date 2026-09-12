namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceOverwritePath
{
    private const string Suffix = ".overwrite.md";

    internal static bool HasSuffix(string path)
        => path.EndsWith(Suffix, StringComparison.Ordinal);

    internal static string ReadBasePath(string overwritePath)
        => overwritePath[..^Suffix.Length] + ".md";

    internal static string ReadAdjacentPath(string basePath)
        => basePath[..^".md".Length] + Suffix;
}
