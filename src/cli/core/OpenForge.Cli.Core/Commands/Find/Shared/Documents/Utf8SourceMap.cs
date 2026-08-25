using System.Text;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Documents;

internal sealed class Utf8SourceMap
{
    private static readonly Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private readonly string _source;

    internal Utf8SourceMap(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateUtf16(source);
        _source = source;
    }

    internal FindSourceLocation Map(MarkdownTextSpan span)
    {
        ArgumentNullException.ThrowIfNull(span);
        if (span.End > _source.Length)
        {
            throw new ArgumentException("The Markdown span must be contained by the mapped source.", nameof(span));
        }

        ValidateBoundary(span.Start, nameof(span));
        ValidateBoundary(span.End, nameof(span));

        var (line, column) = GetLineAndColumn(span.Start);
        var byteOffset = Utf8.GetByteCount(_source.AsSpan(0, span.Start));
        var byteLength = Utf8.GetByteCount(_source.AsSpan(span.Start, span.Length));
        return new FindSourceLocation(line, column, byteOffset, byteLength);
    }

    private static void ValidateUtf16(string source)
    {
        for (var index = 0; index < source.Length; index++)
        {
            if (char.IsHighSurrogate(source[index]))
            {
                if (index + 1 >= source.Length || !char.IsLowSurrogate(source[index + 1]))
                {
                    throw new ArgumentException("The source contains an unpaired UTF-16 high surrogate.", nameof(source));
                }

                index++;
            }
            else if (char.IsLowSurrogate(source[index]))
            {
                throw new ArgumentException("The source contains an unpaired UTF-16 low surrogate.", nameof(source));
            }
        }
    }

    private void ValidateBoundary(int index, string parameterName)
    {
        if (index > 0
            && index < _source.Length
            && char.IsLowSurrogate(_source[index])
            && char.IsHighSurrogate(_source[index - 1]))
        {
            throw new ArgumentException("A Markdown span cannot split a Unicode scalar value.", parameterName);
        }
    }

    private (int Line, int Column) GetLineAndColumn(int start)
    {
        var line = 1;
        var column = 1;
        var index = 0;
        while (index < start)
        {
            if (_source[index] == '\r')
            {
                index++;
                if (index < start && index < _source.Length && _source[index] == '\n')
                {
                    index++;
                }

                line++;
                column = 1;
                continue;
            }

            if (_source[index] == '\n')
            {
                index++;
                line++;
                column = 1;
                continue;
            }

            index += char.IsHighSurrogate(_source[index]) ? 2 : 1;
            column++;
        }

        return (line, column);
    }
}
