using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Destinations;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;

internal static class SourceDestinationEncoder
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    // Authored local-destination spelling is shared with generated navigation.
    internal static string EncodePath(string path)
    {
        _ = StrictUtf8.GetByteCount(path);
        return string.Join('/', path.Split('/').Select(Uri.EscapeDataString));
    }

    // Source References defines the readable form alongside Index's encoded form.
    internal static string EncodeLiteralPath(string path)
    {
        _ = StrictUtf8.GetByteCount(path);
        var segments = path.Split('/');
        for (var index = 0; index < segments.Length; index++)
        {
            var segment = MarkdownDestinationEscaper.EscapeCharacterReferences(
                    segments[index].Replace("%", "%25", StringComparison.Ordinal)
                .Replace("#", "%23", StringComparison.Ordinal)
                .Replace("?", "%3F", StringComparison.Ordinal)
                .Replace("<", "%3C", StringComparison.Ordinal)
                .Replace(">", "%3E", StringComparison.Ordinal));
            segments[index] = index == 0 ? segment.Replace(":", "%3A", StringComparison.Ordinal) : segment;
        }
        return string.Join('/', segments);
    }
}
