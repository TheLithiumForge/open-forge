using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusLibrary
{
    public required CliSemanticStatus State { get; init; }
    public required LibraryStatusView Observation { get; init; }
    public string? RecordCause { get; init; }
    public string? OwnershipObservation { get; init; }
    public required StatusLibraryRecordState RecordState { get; init; }
    public required ImmutableArray<StatusLibraryRegistration> Records { get; init; }
    public required StatusLibraryCounts Counts { get; init; }
    public required ImmutableArray<StatusFinding> Findings { get; init; }
}

internal sealed record StatusLibraryRegistration
{
    public required LibraryId Id { get; init; }
    public required WorkspaceRelativeDirectory SourceRoot { get; init; }
    public required LibraryDestinationRoot DestinationRoot { get; init; }
    public required StatusLibrarySourceRootState SourceRootState { get; init; }
    public required StatusSourceAvailability SourceAvailability { get; init; }
    public required StatusIntegerValue Registered { get; init; }
    public required StatusLibraryCounts Counts { get; init; }
    public required ImmutableArray<StatusLibraryLink> Links { get; init; }

    internal string IdValue => Id.Value;

    internal string SourceRootValue => SourceRoot.Value;

    internal string DestinationRootValue => DestinationRoot.Value;

    internal string? SourceRootCause { get; init; }
}

internal sealed record StatusLibraryLink(LibraryMappingObservation Observation, string? SourceId)
{
    // Presentation consumes typed state while the original observation retains
    // the operational details needed by status aggregation.
    public StatusTargetState State { get; init; } = StatusTargetState.Unavailable;

    public string SourcePath => Observation.Mapping.SourcePath.Value;

    public string DestinationPath => Observation.Mapping.DestinationPath.Value;

    public string ExpectedRelativeLink => Observation.Mapping.ExpectedRelativeLink.Value;

    public string? ObservedRelativeLink => Observation.Leaf.RelativeFileLink?.RawRelativeTarget;

    public string? Cause => Observation.Cause;
}

internal sealed record StatusLibraryCounts
{
    public required StatusIntegerValue Registered { get; init; }
    public required StatusIntegerValue Current { get; init; }
    public required StatusIntegerValue Missing { get; init; }
    public required StatusIntegerValue Changed { get; init; }
    public required StatusIntegerValue Blocked { get; init; }
    public required StatusIntegerValue Unavailable { get; init; }
}
