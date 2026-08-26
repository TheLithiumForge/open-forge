namespace OpenForge.Cli.Core.Framework.Sources.Models.Locations;

internal sealed record SourceLocation
{
    internal SourceLocation(int line, int column, long byteOffset, long byteLength)
    {
        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(line), line, "A source location line must be positive.");
        }

        if (column < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "A source location column must be positive.");
        }

        if (byteOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset), byteOffset, "A source byte offset cannot be negative.");
        }

        if (byteLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), byteLength, "A source byte length cannot be negative.");
        }

        _ = checked(byteOffset + byteLength);
        Line = line;
        Column = column;
        ByteOffset = byteOffset;
        ByteLength = byteLength;
    }

    internal int Line { get; }

    internal int Column { get; }

    internal long ByteOffset { get; }

    internal long ByteLength { get; }
}
