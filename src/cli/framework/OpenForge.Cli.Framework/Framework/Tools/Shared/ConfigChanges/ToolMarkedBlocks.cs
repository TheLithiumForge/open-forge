using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal static class ToolMarkedBlocks
{
    internal static ToolEntrySpan? Find(byte[] bytes, string startMarker, string endMarker)
    {
        RequireMarker(startMarker);
        RequireMarker(endMarker);
        if (startMarker == endMarker || startMarker.Contains(endMarker, StringComparison.Ordinal) || endMarker.Contains(startMarker, StringComparison.Ordinal))
            throw new InvalidDataException("The block markers must be distinct.");
        var source = ToolConfigBytes.Utf8.GetString(bytes);
        var lines = ToolConfigBytes.Lines(source);
        var starts = lines.Where(line => line.Content.TrimStart('\uFEFF').Contains(startMarker, StringComparison.Ordinal)).ToArray();
        var ends = lines.Where(line => line.Content.Contains(endMarker, StringComparison.Ordinal)).ToArray();
        if (starts.Length == 0 && ends.Length == 0) return null;
        if (starts.Length != 1 || ends.Length != 1 || starts[0].Content.TrimStart('\uFEFF') != startMarker
            || ends[0].Content != endMarker || starts[0].End > ends[0].Start)
            throw new InvalidDataException("The block markers are malformed, duplicated, incomplete or reversed.");
        var start = starts[0].Start;
        if (start == 0 && source.StartsWith('\uFEFF')) start++;
        return ToolConfigBytes.Span(source, start, ends[0].End, string.Empty);
    }

    internal static byte[] Render(string startMarker, string endMarker, string content)
    {
        RequireMarker(startMarker);
        RequireMarker(endMarker);
        if (content.Contains(startMarker, StringComparison.Ordinal) || content.Contains(endMarker, StringComparison.Ordinal))
            throw new InvalidDataException("The block content contains a reserved marker.");
        content = content.ReplaceLineEndings("\n");
        if (content.Length > 0 && !content.EndsWith('\n')) content += "\n";
        var bytes = ToolConfigBytes.Utf8.GetBytes($"{startMarker}\n{content}{endMarker}\n");
        _ = Find(bytes, startMarker, endMarker);
        return bytes;
    }

    internal static byte[] Add(byte[] source, byte[] block, bool preferEnd = false)
    {
        if (preferEnd && (source.Length == 0 || source[^1] == (byte)'\n'))
            return ToolConfigBytes.Replace(source, [new(source.Length, source.Length, block)]);
        // Prepending keeps a last line without a newline byte-for-byte after removal.
        var offset = source.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }) ? 3 : 0;
        return ToolConfigBytes.Replace(source, [new(offset, offset, block)]);
    }

    private static void RequireMarker(string marker)
    {
        if (string.IsNullOrWhiteSpace(marker) || marker.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new InvalidDataException("A block marker must be one nonempty line.");
    }
}
