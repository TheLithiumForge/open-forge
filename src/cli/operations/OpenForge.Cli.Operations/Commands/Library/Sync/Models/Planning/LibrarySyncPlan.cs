using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;

internal sealed record LibrarySyncPlan
{
    internal LibraryMutationEffects Effects => new()
    {
        Directories = Directories,
        Links = Links,
        GeneratedRegions = GeneratedRegions,
        OwnershipChange = OwnershipChange,
    };

    public required LibraryPermissionStage? Permissions { get; init; }

    public required LibrarySyncPlanningInput Input { get; init; }

    public required LibraryPlanState State { get; init; }

    public required ImmutableArray<PlannedDirectoryCreation> Directories { get; init; }

    public required ImmutableArray<RelativeFileLinkEffect> Links { get; init; }

    public required ImmutableArray<PlannedFileChange> GeneratedRegions { get; init; }

    internal PlannedFileChange? OwnershipChange { get; init; }


    public required LibraryRegistrationSet? IntendedRecord { get; init; }

    public required ImmutableArray<LibrarySyncFinding> Findings { get; init; }
}
