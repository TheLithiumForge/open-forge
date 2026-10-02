using System.Globalization;
using System.Text;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;

internal static class WorkspaceAdoptionDocumentText
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    internal static bool TryDecode(ReadOnlyMemory<byte> bytes, out string source)
    {
        try
        {
            source = StrictUtf8.GetString(bytes.Span);
            return true;
        }
        catch (DecoderFallbackException)
        {
            source = string.Empty;
            return false;
        }
    }

    internal static bool TryEncode(string source, out byte[] bytes)
    {
        try
        {
            bytes = StrictUtf8.GetBytes(source);
            return true;
        }
        catch (EncoderFallbackException)
        {
            bytes = [];
            return false;
        }
    }

    internal static string ReadPreferredNewline(string source)
    {
        for (var index = 0; index < source.Length; index++)
        {
            if (source[index] == '\r')
            {
                return index + 1 < source.Length && source[index + 1] == '\n' ? "\r\n" : "\r";
            }

            if (source[index] == '\n')
            {
                return "\n";
            }
        }

        return "\n";
    }

    internal static string AppendEntriesSection(string source)
    {
        var newline = ReadTrailingNewline(source) ?? ReadPreferredNewline(source);
        var trailingBreaks = CountTrailingLineBreaks(source);
        string separator;
        if (source.Length == 0 || trailingBreaks >= 2)
        {
            separator = string.Empty;
        }
        else if (trailingBreaks == 1)
        {
            separator = newline;
        }
        else
        {
            separator = newline + newline;
        }

        return string.Concat(
            source,
            separator,
            "## Entries",
            newline,
            newline,
            MarkdownEntriesSectionReader.EmptyEntry,
            newline);
    }

    internal static string QuoteYamlScalar(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');
        foreach (var character in value)
        {
            switch (character)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\0':
                    builder.Append("\\0");
                    break;
                case '\a':
                    builder.Append("\\a");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\v':
                    builder.Append("\\v");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\u001B':
                    builder.Append("\\e");
                    break;
                case '\u0085':
                    builder.Append("\\N");
                    break;
                case '\u00A0':
                    builder.Append("\\_");
                    break;
                case '\u2028':
                    builder.Append("\\L");
                    break;
                case '\u2029':
                    builder.Append("\\P");
                    break;
                default:
                    if (char.IsControl(character))
                    {
                        builder.Append("\\u");
                        builder.Append(((int)character).ToString("X4", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(character);
                    }

                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    internal static string EscapeMarkdownHeading(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (character <= 0x7F && char.IsPunctuation(character))
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    internal static bool TryApplyEdits(
        string source,
        IReadOnlyList<WorkspaceAdoptionTextEdit> edits,
        out string updatedSource)
    {
        var ordered = edits.OrderByDescending(edit => edit.Start).ToArray();
        var builder = new StringBuilder(source);
        var previousStart = -1;
        foreach (var edit in ordered)
        {
            if (edit.Start < 0
                || edit.Length < 0
                || edit.Start > source.Length - edit.Length
                || edit.Replacement is null
                || edit.Start == previousStart
                || previousStart >= 0 && edit.Start + edit.Length > previousStart)
            {
                updatedSource = string.Empty;
                return false;
            }

            builder.Remove(edit.Start, edit.Length);
            builder.Insert(edit.Start, edit.Replacement);
            previousStart = edit.Start;
        }

        updatedSource = builder.ToString();
        return true;
    }

    internal static bool TryReadBlockInsertionPoint(
        string source,
        int nodeStart,
        int nodeEnd,
        out int insertionPoint,
        out string newline,
        out string indentation)
    {
        insertionPoint = 0;
        newline = string.Empty;
        indentation = string.Empty;
        if (nodeStart < 0 || nodeEnd < nodeStart || nodeEnd > source.Length)
        {
            return false;
        }

        var lineStart = nodeStart;
        while (lineStart > 0 && source[lineStart - 1] is not ('\r' or '\n'))
        {
            lineStart--;
        }

        var indentationSpan = source.AsSpan(lineStart, nodeStart - lineStart);
        if (!indentationSpan.IsWhiteSpace())
        {
            return false;
        }

        indentation = indentationSpan.ToString();
        if (nodeEnd > 0 && source[nodeEnd - 1] is '\r' or '\n')
        {
            if (!TryReadTrailingLineEnding(source.AsSpan(0, nodeEnd), out newline))
            {
                return false;
            }

            insertionPoint = nodeEnd;
            return true;
        }

        var lineEnd = nodeEnd;
        while (lineEnd < source.Length && source[lineEnd] is not ('\r' or '\n'))
        {
            lineEnd++;
        }

        if (lineEnd < source.Length)
        {
            newline = ReadLineEndingAt(source, lineEnd);
            insertionPoint = lineEnd + newline.Length;
            return true;
        }

        if (TryReadTrailingLineEnding(source.AsSpan(), out newline))
        {
            insertionPoint = source.Length;
            return true;
        }

        return false;
    }

    internal static bool TryReadStandaloneNodeIndentation(
        string source,
        int nodeStart,
        int nodeEnd,
        out string indentation)
    {
        indentation = string.Empty;
        if (nodeStart < 0 || nodeEnd < nodeStart || nodeEnd > source.Length)
        {
            return false;
        }

        var lineStart = nodeStart;
        while (lineStart > 0 && source[lineStart - 1] is not ('\r' or '\n'))
        {
            lineStart--;
        }

        var lineEnd = nodeEnd;
        while (lineEnd < source.Length && source[lineEnd] is not ('\r' or '\n'))
        {
            lineEnd++;
        }

        var prefix = source.AsSpan(lineStart, nodeStart - lineStart);
        var suffix = source.AsSpan(nodeEnd, lineEnd - nodeEnd);
        if (!prefix.IsWhiteSpace() || !suffix.IsWhiteSpace())
        {
            return false;
        }

        indentation = prefix.ToString();
        return true;
    }

    internal static bool IsSimpleEmptyFlowMapping(ReadOnlySpan<char> source)
    {
        var value = source.Trim();
        return value.Length >= 2
            && value[0] == '{'
            && value[^1] == '}'
            && value[1..^1].IsWhiteSpace();
    }

    private static string? ReadTrailingNewline(string source)
    {
        if (source.EndsWith("\r\n", StringComparison.Ordinal))
        {
            return "\r\n";
        }

        if (source.EndsWith('\n'))
        {
            return "\n";
        }

        return source.EndsWith('\r') ? "\r" : null;
    }

    private static int CountTrailingLineBreaks(string source)
    {
        var count = 0;
        var index = source.Length;
        while (index > 0)
        {
            if (source[index - 1] == '\n')
            {
                index -= index > 1 && source[index - 2] == '\r' ? 2 : 1;
                count++;
            }
            else if (source[index - 1] == '\r')
            {
                index--;
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    private static bool TryReadTrailingLineEnding(ReadOnlySpan<char> source, out string newline)
    {
        if (source.EndsWith("\r\n", StringComparison.Ordinal))
        {
            newline = "\r\n";
            return true;
        }

        if (source.EndsWith("\n", StringComparison.Ordinal))
        {
            newline = "\n";
            return true;
        }

        if (source.EndsWith("\r", StringComparison.Ordinal))
        {
            newline = "\r";
            return true;
        }

        newline = string.Empty;
        return false;
    }

    private static string ReadLineEndingAt(string source, int index)
        => source[index] == '\r' && index + 1 < source.Length && source[index + 1] == '\n'
            ? "\r\n"
            : source[index].ToString();
}
