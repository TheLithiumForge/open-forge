using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Status.Models.Result;

internal sealed record StatusLibrary
{
    public required CliSemanticStatus State { get; init; }
    public required LibraryStatusView Observation { get; init; }
    public required ImmutableArray<StatusLibraryRegistration> Records { get; init; }
    public required StatusLibraryCounts Counts { get; init; }
    public required ImmutableArray<StatusFinding> Findings { get; init; }
}

internal sealed record StatusLibraryRegistration
{
    public required LibraryId Id { get; init; }
    public required WorkspaceRelativeDirectory SourceRoot { get; init; }
    public required LibrarySourceRootState SourceRootState { get; init; }
    public required OperationalSourceAvailability SourceAvailability { get; init; }
    public required StatusIntegerValue Registered { get; init; }
    public required StatusLibraryCounts Counts { get; init; }
    public required ImmutableArray<StatusLibraryLink> Links { get; init; }
}

internal sealed record StatusLibraryLink(LibraryMappingObservation Observation, string SourceId);

internal sealed record StatusLibraryCounts
{
    public required StatusIntegerValue Registered { get; init; }
    public required StatusIntegerValue Current { get; init; }
    public required StatusIntegerValue Missing { get; init; }
    public required StatusIntegerValue Changed { get; init; }
    public required StatusIntegerValue Blocked { get; init; }
    public required StatusIntegerValue Unavailable { get; init; }
}
