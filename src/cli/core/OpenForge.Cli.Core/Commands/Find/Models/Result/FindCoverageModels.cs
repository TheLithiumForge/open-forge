namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal enum FindCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum FindProjectionCoverageState
{
    NotRequested,
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record FindSourceLocation
{
    internal FindSourceLocation(int line, int column, long byteOffset, long byteLength)
    {
        if (line < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(line), line, "A Find source location line must be positive.");
        }

        if (column < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "A Find source location column must be positive.");
        }

        if (byteOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset), byteOffset, "A Find byte offset cannot be negative.");
        }

        if (byteLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(byteLength), byteLength, "A Find byte length cannot be negative.");
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

internal sealed record FindCoverage
{
    internal FindCoverage(
        FindCoverageState state,
        FindCoverageState matching,
        FindProjectionCoverageState projection)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined.");
        }

        if (!Enum.IsDefined(matching))
        {
            throw new ArgumentOutOfRangeException(nameof(matching), matching, "The Find matching coverage state is not defined.");
        }

        if (!Enum.IsDefined(projection))
        {
            throw new ArgumentOutOfRangeException(nameof(projection), projection, "The Find projection coverage state is not defined.");
        }

        State = state;
        Matching = matching;
        Projection = projection;
    }

    internal FindCoverageState State { get; }

    internal FindCoverageState Matching { get; }

    internal FindProjectionCoverageState Projection { get; }
}
