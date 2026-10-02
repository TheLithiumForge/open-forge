using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal static class InstallEntrypointPreservation
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static byte[] Preserve(byte[] entrypoint, byte[]? existingOverwrite)
    {
        var text = StrictUtf8.GetString(entrypoint);
        var document = new MarkdownDocumentParser().Parse(text);
        if (document.GeneratedRegion is
            { State: MarkdownGeneratedRegionState.Complete, EntriesBlock: { } entries, RegionSpan: { } region })
        {
            var heading = document.Headings.Single(value => value.Span.Start == region.Start);
            // Only generated navigation and its heading are omitted. Authored prose
            // before, inside, or after the section retains its original text.
            text = text.Remove(entries.Span.Start, entries.Span.Length)
                .Remove(heading.Span.Start, heading.Span.Length);
        }

        var preserved = StrictUtf8.GetBytes(text);
        if (existingOverwrite is null || existingOverwrite.Length == 0)
        {
            return preserved;
        }

        // Keep the existing customization last so its established precedence survives.
        _ = StrictUtf8.GetString(existingOverwrite);
        var separator = text.Contains("\r\n", StringComparison.Ordinal) ? "\r\n\r\n"u8 : "\n\n"u8;
        return [.. preserved, .. separator, .. existingOverwrite];
    }
}
