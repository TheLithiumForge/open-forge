
namespace OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;

internal enum LibraryRecordViewState
{
    NotStarted,
    Missing,
    Complete,
    Invalid,
    Unavailable,
    Blocked,
    Failed,
    Interrupted,
}

internal enum LibrarySourceRootViewState
{
    NotStarted,
    Available,
    Missing,
    Unavailable,
    Invalid,
    Blocked,
}

internal enum LibraryLinkViewState
{
    NotStarted,
    Current,
    Missing,
    Changed,
    Unavailable,
    Blocked,
}

internal enum LibraryCoverage
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum LibraryComparisonRelation
{
    NotStarted,
    Current,
    Added,
    Retired,
    Missing,
    Changed,
    Unavailable,
    Blocked,
}

internal enum LibraryMutationRecordState
{
    NotStarted,
    Missing,
    Complete,
    Invalid,
    Unavailable,
    Blocked,
}

internal enum LibraryMutationInventoryState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
}

internal enum LibraryCollisionKind
{
    Occupant,
    Parent,
    Ownership,
    Duplicate,
    Unsafe,
    Unavailable,
}

internal enum LibraryExclusionKind
{
    Loader,
    Entrypoint,
    Overwrite,
    ManagerControl,
    Link,
    ReparsePoint,
    Special,
}

internal enum LibraryOwnershipKind
{
    Unowned,
    Extension,
    Framework,
    Library,
    Manager,
    Unavailable,
    Blocked,
}
