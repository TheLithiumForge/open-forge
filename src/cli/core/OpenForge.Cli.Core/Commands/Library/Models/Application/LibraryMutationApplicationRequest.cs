using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

internal sealed record LibraryMutationApplicationRequest
{
    public required WorkspaceLockLease Lease { get; init; }
    public required ImmutableArray<PlannedDirectoryCreation> Directories { get; init; }
    public required ImmutableArray<RelativeFileLinkEffect> Links { get; init; }
    public required ImmutableArray<PlannedFileChange> GeneratedRegions { get; init; }
    public required PlannedFileChange? RecordChange { get; init; }
    public required RecoveryBundlePreparation? RecoveryPreparation { get; init; }
    public required ImmutableArray<WorkspaceRelativeDirectory> ProtectedSourceRoots { get; init; }
}
