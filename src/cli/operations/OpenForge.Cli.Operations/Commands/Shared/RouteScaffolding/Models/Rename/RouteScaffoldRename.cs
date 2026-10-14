using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;

internal enum RouteScaffoldRenameBasis
{
    CommittedContent,
    GitRemoval,
}

internal sealed record RouteScaffoldRenameProof
{
    public required string OldCanonicalPath { get; init; }
    public required string CurrentCanonicalPath { get; init; }
    public required string CurrentContentHash { get; init; }
    public required RouteScaffoldRenameBasis Basis { get; init; }
    public int? Similarity { get; init; }
}

internal sealed record RouteScaffoldRenameCandidate
{
    public required SourceLogicalSource Destination { get; init; }
    public required FileStateSnapshot DestinationBefore { get; init; }
    public required ImmutableArray<byte> ScaffoldBytes { get; init; }
    public SourceLogicalSource? OldSource { get; init; }
    public FileStateSnapshot? OldSnapshot { get; init; }
}

internal sealed record RouteScaffoldOldNavigation(string Path, string CanonicalFileName);

internal sealed record RouteScaffoldEntrypointIntent
{
    public required SourceLogicalSource Source { get; init; }
    public required FileStateSnapshot Before { get; init; }
    public required ImmutableArray<byte> IntendedBytes { get; init; }
    public FileStateSnapshot? DeleteBefore { get; init; }
    public RouteScaffoldOldNavigation? OldNavigation { get; init; }
    internal bool IsRename => DeleteBefore is not null;
}

internal sealed record RouteScaffoldEntrypointPlan(
    ImmutableArray<PlannedFileChange> Changes,
    ImmutableArray<RecoveryBundleTarget> RecoveryTargets);
