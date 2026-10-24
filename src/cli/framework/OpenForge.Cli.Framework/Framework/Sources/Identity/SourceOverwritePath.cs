namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceOverwritePath
{
    private const string Suffix = ".overwrite" + SourceFormClassifier.MarkdownExtension;

    internal static bool HasSuffix(string path)
        => path.EndsWith(Suffix, StringComparison.Ordinal);

    internal static string ReadBasePath(string overwritePath)
        => overwritePath[..^Suffix.Length] + SourceFormClassifier.MarkdownExtension;

    internal static string ReadAdjacentPath(string basePath)
        => basePath[..^SourceFormClassifier.MarkdownExtension.Length] + Suffix;
}
