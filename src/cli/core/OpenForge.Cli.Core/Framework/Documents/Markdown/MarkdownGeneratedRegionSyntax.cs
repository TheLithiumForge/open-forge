namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal static class MarkdownGeneratedRegionSyntax
{
    internal const string EntriesHeadingText = "Entries";
    internal const string EntriesHeadingLine = "## " + EntriesHeadingText;
    internal const string MarkerPrefix = "open-forge:generated-index:";
    internal const string StartMarker = "<!-- " + MarkerPrefix + "start -->";
    internal const string EndMarker = "<!-- " + MarkerPrefix + "end -->";
}
