using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;

internal sealed record LibrarySyncPlan
{
    internal LibraryMutationEffects Effects => new()
    {
        Directories = Directories,
        Links = Links,
        GeneratedRegions = GeneratedRegions,
        RecordChange = RecordChange,
    };

    public required LibraryPermissionStage? Permissions { get; init; }

    public required LibrarySyncPlanningInput Input { get; init; }

    public required LibraryPlanState State { get; init; }

    public required ImmutableArray<PlannedDirectoryCreation> Directories { get; init; }

    public required ImmutableArray<RelativeFileLinkEffect> Links { get; init; }

    public required ImmutableArray<PlannedFileChange> GeneratedRegions { get; init; }

    public required PlannedFileChange? RecordChange { get; init; }

    public required LibrariesRecord? IntendedRecord { get; init; }

    public required ImmutableArray<LibrarySyncFinding> Findings { get; init; }
}
