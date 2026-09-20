using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;

namespace OpenForge.Cli.Core.Framework.Documents.Markdown;

internal sealed class MarkdownFrontmatterParser
{
    internal MarkdownFrontmatterBoundary Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (!TryReadLine(source, 0, out var openingLine)
            || !IsFence(source[openingLine.Start..openingLine.End], allowByteOrderMark: true))
        {
            return new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Missing, null, null, 0);
        }

        var yamlStart = openingLine.NextStart;
        var lineStart = yamlStart;
        while (lineStart < source.Length)
        {
            if (!TryReadLine(source, lineStart, out var line))
            {
                break;
            }

            if (IsFence(source[line.Start..line.End], allowByteOrderMark: false))
            {
                return new MarkdownFrontmatterBoundary(
                    MarkdownFrontmatterState.Complete,
                    new MarkdownTextSpan(0, line.End),
                    new MarkdownTextSpan(yamlStart, line.Start - yamlStart),
                    line.NextStart);
            }

            if (line.NextStart == lineStart)
            {
                break;
            }

            lineStart = line.NextStart;
        }

        return new MarkdownFrontmatterBoundary(MarkdownFrontmatterState.Unavailable, null, null, null);
    }

    // A frontmatter fence is `---` on its own line. Trailing whitespace is invisible and is
    // produced by ordinary editors, and a UTF-8 byte order mark precedes the opening fence
    // whenever the file was saved by a Windows editor that writes one. Rejecting either made
    // valid frontmatter undetectable and reported it as missing required metadata.
    private static bool IsFence(string line, bool allowByteOrderMark)
    {
        var value = line.AsSpan();
        if (allowByteOrderMark && value.Length != 0 && value[0] == '﻿')
        {
            value = value[1..];
        }

        return value.TrimEnd().SequenceEqual("---");
    }

    private static bool TryReadLine(string source, int start, out SourceLine line)
    {
        if (start < 0 || start > source.Length)
        {
            line = default;
            return false;
        }

        var end = start;
        while (end < source.Length && source[end] is not ('\r' or '\n'))
        {
            end++;
        }

        var nextStart = end;
        if (end < source.Length)
        {
            nextStart++;
            if (source[end] == '\r' && nextStart < source.Length && source[nextStart] == '\n')
            {
                nextStart++;
            }
        }

        line = new SourceLine(start, end, nextStart);
        return true;
    }

    private readonly record struct SourceLine(int Start, int End, int NextStart);
}
