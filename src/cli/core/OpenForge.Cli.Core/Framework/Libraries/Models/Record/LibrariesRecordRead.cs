using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Record;

internal enum LibrariesRecordReadState
{
    Missing,
    Complete,
    Malformed,
    Unavailable,
    Blocked,
}

internal sealed record LibrariesRecordRead
{
    public required LibrariesRecordReadState State { get; init; }
    public required LibrariesRecord? Record { get; init; }
    public required FileStateSnapshot? Snapshot { get; init; }
    public required string? Cause { get; init; }
}

internal enum LibrariesRecordDecodeIssue
{
    None,
    Malformed,
    AmbiguousOwnership,
}

internal sealed record LibrariesRecordDecode
{
    public required LibrariesRecordReadState State { get; init; }
    public required LibrariesRecord? Record { get; init; }
    public required LibrariesRecordDecodeIssue Issue { get; init; }
    public required string? Cause { get; init; }
}
