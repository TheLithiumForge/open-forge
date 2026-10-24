using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Settings.Models.Observation;

internal enum WorkspaceLayoutIssueCode
{
    InvalidSetting,
    UnsafeRoot,
    MissingLoader,
    ConflictingLoader,
    OwnershipMismatch,
    PointerMismatch,
    ObservationUnavailable,
    OutsideDirectoryMissing,
    OutsideDirectoryUnsafe,
    OutsideCatalogueUnsafe,
    OutsideObservationUnavailable,
    OutsideOverlap,
}

internal sealed record WorkspaceLayoutIssue(
    WorkspaceLayoutIssueCode Code,
    string Path,
    string Cause,
    ImmutableArray<string> RelatedPaths);
