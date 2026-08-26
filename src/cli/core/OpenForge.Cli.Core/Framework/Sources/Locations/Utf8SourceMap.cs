using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Locations;

internal sealed class Utf8SourceMap
{
    private static readonly Encoding Utf8 = new UTF8Encoding(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly string _source;

    internal Utf8SourceMap(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateUtf16(source);
        _source = source;
    }

    internal SourceLocation Map(int start, int length)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start), start, "A source span start cannot be negative.");
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "A source span length cannot be negative.");
        }

        var end = checked(start + length);
        if (end > _source.Length)
        {
            throw new ArgumentException("The source span must be contained by the mapped source.", nameof(length));
        }

        ValidateBoundary(start, nameof(start));
        ValidateBoundary(end, nameof(length));

        var (line, column) = GetLineAndColumn(start);
        var byteOffset = Utf8.GetByteCount(_source.AsSpan(0, start));
        var byteLength = Utf8.GetByteCount(_source.AsSpan(start, length));
        return new SourceLocation(line, column, byteOffset, byteLength);
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
            throw new ArgumentException("A source span cannot split a Unicode scalar value.", parameterName);
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
