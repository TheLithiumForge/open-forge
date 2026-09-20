namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal enum LibraryApplicationState
{
    NotStarted,
    NoOp,
    Applied,
    Failed,
    Interrupted,
}

internal enum LibraryVerificationState
{
    NotStarted,
    Verified,
    Failed,
    Unavailable,
}

internal enum LibraryRecoveryState
{
    NotRequested,
    Prepared,
    Removed,
    Retained,
    Unknown,
}

internal enum LibraryResidualKind
{
    Directory,
    Link,
    GeneratedRegion,
    Record,
    Recovery,
}

internal enum LibraryResidualState
{
    Retained,
    Unknown,
}

internal enum LibraryRecordPublicationState
{
    NotStarted,
    Verified,
    Failed,
    Unknown,
}
