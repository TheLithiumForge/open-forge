namespace OpenForge.Cli.Core.Commands.Install.Models.Planning;

internal enum ManagedBlockState
{
    Absent,
    Present,
    Blocked,
}

internal sealed record ManagedBlockResolution
{
    public required ManagedBlockState State { get; init; }

    public required byte[]? ExistingBlockBytes { get; init; }

    public required byte[]? IntendedDocumentBytes { get; init; }

    public required string? Cause { get; init; }

    internal static ManagedBlockResolution Absent(byte[] intendedDocumentBytes)
        => new()
        {
            State = ManagedBlockState.Absent,
            ExistingBlockBytes = null,
            IntendedDocumentBytes = intendedDocumentBytes,
            Cause = null,
        };

    internal static ManagedBlockResolution Present(
        byte[] existingBlockBytes,
        byte[] intendedDocumentBytes)
        => new()
        {
            State = ManagedBlockState.Present,
            ExistingBlockBytes = existingBlockBytes,
            IntendedDocumentBytes = intendedDocumentBytes,
            Cause = null,
        };

    internal static ManagedBlockResolution Blocked(string cause)
        => new()
        {
            State = ManagedBlockState.Blocked,
            ExistingBlockBytes = null,
            IntendedDocumentBytes = null,
            Cause = cause,
        };
}
