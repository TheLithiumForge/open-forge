namespace OpenForge.Cli.Core.Framework.Distribution.Models.Content;

using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

internal enum FrameworkManagedBlockState
{
    Absent,
    Present,
    Blocked,
}

internal sealed record FrameworkManagedBlockRecognition
{
    public required FrameworkManagedBlockState State { get; init; }

    public required int? Start { get; init; }

    public required int? EndExclusive { get; init; }

    public required byte[]? ExistingBlockBytes { get; init; }

    public required string? Cause { get; init; }

    internal static FrameworkManagedBlockRecognition Absent()
        => new()
        {
            State = FrameworkManagedBlockState.Absent,
            Start = null,
            EndExclusive = null,
            ExistingBlockBytes = null,
            Cause = null,
        };

    internal static FrameworkManagedBlockRecognition Present(
        int start,
        int endExclusive,
        byte[] existingBlockBytes)
        => new()
        {
            State = FrameworkManagedBlockState.Present,
            Start = start,
            EndExclusive = endExclusive,
            ExistingBlockBytes = existingBlockBytes,
            Cause = null,
        };

    internal static FrameworkManagedBlockRecognition Blocked(string cause)
        => new()
        {
            State = FrameworkManagedBlockState.Blocked,
            Start = null,
            EndExclusive = null,
            ExistingBlockBytes = null,
            Cause = cause,
        };
}
