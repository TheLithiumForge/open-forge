using System.Text;
using Markdig.Helpers;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.Destinations;

internal static class MarkdownDestinationEscaper
{
    // Source References' literal form must survive the same decoding as parsed link destinations.
    internal static string EscapeCharacterReferences(string destination)
    {
        StringBuilder? escaped = null;
        var copied = 0;
        for (var index = destination.IndexOf('&'); index >= 0; index = destination.IndexOf('&', index + 1))
        {
            var length = HtmlHelper.ScanEntity(new StringSlice(destination, index, destination.Length - 1),
                out _, out var nameStart, out var nameLength);
            if (length == 0 || nameLength > 0 && EntityHelper.DecodeEntity(destination.AsSpan(nameStart, nameLength)) is null)
                continue;

            escaped ??= new StringBuilder(destination.Length);
            escaped.Append(destination.AsSpan(copied, index - copied));
            escaped.Append("%26");
            copied = index + 1;
        }
        if (escaped is null) return destination;
        escaped.Append(destination.AsSpan(copied));
        return escaped.ToString();
    }
}
