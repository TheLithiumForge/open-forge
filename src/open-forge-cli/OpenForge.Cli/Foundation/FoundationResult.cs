namespace OpenForge.Cli.Foundation;

internal abstract record FoundationResult;

internal sealed record FoundationSucceeded(FoundationReport Report) : FoundationResult;

internal sealed record FoundationRejected(FoundationFailure Failure) : FoundationResult;

internal sealed record FoundationFailure(FoundationFailureKind Kind, string Message);

internal enum FoundationFailureKind
{
    InvalidInput,
    Filesystem,
}

internal sealed record FoundationReport(FoundationSnapshot Snapshot, string Json);

internal sealed record FoundationSnapshot(int SchemaVersion, int MarkdownHeadingCount, string YamlName, int FileByteCount, string FileSha256, bool ExclusiveLockObserved);
