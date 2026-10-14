using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;

internal sealed record RenameEvidenceRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ImmutableArray<string> CandidatePaths { get; init; }
    public required ImmutableArray<string> MissingPaths { get; init; }
    public required bool CandidateUniverseComplete { get; init; }
    public required RenameEvidencePolicy Policy { get; init; }
    public ImmutableArray<RenameRecordedIdentity> RecordedIdentities { get; init; } = [];
    public ImmutableArray<RenameWeakCandidate> WeakCandidates { get; init; } = [];
    public ImmutableArray<FileExpectation> PolicyExpectations { get; init; } = [];
}

internal sealed record RenameEvidencePolicy
{
    public ImmutableArray<string> ManagedPaths { get; init; } = [];
    public ImmutableArray<string> PackageRoots { get; init; } = [];
    public ImmutableArray<string> ExcludedPaths { get; init; } = [];
    public ImmutableArray<string> PrivatePaths { get; init; } = [];
    public ImmutableArray<string> ProjectionPaths { get; init; } = [];
}
