using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;

namespace OpenForge.Cli.Core.Framework.Sources.Routing;

internal static class SourceLoaderDeclarationParser
{
    internal static bool TryParse(
        string line,
        [NotNullWhen(true)] out string? destination,
        [NotNullWhen(false)] out string? cause)
    {
        destination = null;
        cause = null;
        if (!MarkdownEntryRowParser.TryParse(line, out var row, out cause))
        {
            return false;
        }

        if (row.Tags.Count == 0)
        {
            cause = "A Loader declaration must contain one or more bare tags.";
            return false;
        }

        destination = row.Destination;
        return true;
    }
}
