using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Framework.Sources.Locations;

internal sealed class Utf8SourceMap
{
    private readonly string _source;
    private readonly Lazy<CoordinateIndex> _index;

    internal Utf8SourceMap(string source, Action? indexBuilt = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidateUtf16(source);
        _source = source;
        _index = new(() =>
        {
            var index = new CoordinateIndex(source);
            indexBuilt?.Invoke();
            return index;
        });
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

        return _index.Value.Map(start, end);
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

    private sealed class CoordinateIndex
    {
        private readonly string _source;
        private readonly int[] _byteOffsets;
        private readonly int[] _scalarOffsets;
        private readonly int[] _lineStarts;

        internal CoordinateIndex(string source)
        {
            _source = source;
            _byteOffsets = new int[source.Length + 1];
            _scalarOffsets = new int[source.Length + 1];
            var lineStarts = new List<int> { 0 };
            for (var index = 0; index < source.Length;)
            {
                var rune = Rune.GetRuneAt(source, index);
                var end = index + rune.Utf16SequenceLength;
                _byteOffsets[end] = checked(_byteOffsets[index] + rune.Utf8SequenceLength);
                _scalarOffsets[end] = _scalarOffsets[index] + 1;
                if (rune.Value == '\r') lineStarts.Add(end);
                else if (rune.Value == '\n')
                {
                    if (index > 0 && source[index - 1] == '\r') lineStarts[^1] = end;
                    else lineStarts.Add(end);
                }
                index = end;
            }
            _lineStarts = lineStarts.ToArray();
        }

        internal SourceLocation Map(int start, int end)
        {
            var lookup = start;
            if (start > 0 && start < _source.Length && _source[start] == '\n' && _source[start - 1] == '\r') lookup++;
            var line = Array.BinarySearch(_lineStarts, lookup);
            if (line < 0) line = ~line - 1;
            var lineStart = Math.Min(start, _lineStarts[line]);
            return new SourceLocation(line + 1, _scalarOffsets[start] - _scalarOffsets[lineStart] + 1,
                _byteOffsets[start], _byteOffsets[end] - _byteOffsets[start]);
        }
    }
}
