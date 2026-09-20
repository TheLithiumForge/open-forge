using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryRecoveryPreparationRequest
{
    public required WorkspaceLockLease Lease { get; init; }
    public required string Command { get; init; }
    public required RecoveryBundleOperation Operation { get; init; }
    public required LibraryPermissionStage? Permissions { get; init; }
    public required ImmutableArray<RelativeFileLinkEffect> Links { get; init; }
    public required ImmutableArray<PlannedFileChange> GeneratedRegions { get; init; }
    public WorkspaceOwnershipRead? Ownership { get; init; }
    public PlannedFileChange? OwnershipChange { get; init; }
    public required ImmutableArray<LibraryMappingObservation> Mappings { get; init; }
}
