using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;

internal delegate ValueTask<RenameGitObservation> RenameGitReader(RenameGitRequest request, CancellationToken cancellationToken);

internal sealed record RenameGitRequest(CliWorkspace Workspace, RenameEvidencePolicy Policy);

internal enum RenameGitAvailability
{
    Available,
    NotRequested,
    NoRepository,
    Unborn,
    Unavailable,
    TimedOut,
}

internal sealed record RenameGitBlob(string Path, string Blob, ImmutableArray<byte> Bytes);
internal sealed record RenameGitTrackedPath(string Path, string Blob);

internal sealed record RenameGitObservation
{
    public required RenameGitAvailability Availability { get; init; }
    public string? Head { get; init; }
    public string? RepositoryPrefix { get; init; }
    public ImmutableArray<RenameGitBlob> Blobs { get; init; } = [];
    public ImmutableArray<RenameGitTrackedPath> TrackedPaths { get; init; } = [];

    internal static RenameGitObservation Unavailable(RenameGitAvailability availability)
        => new() { Availability = availability };
}
