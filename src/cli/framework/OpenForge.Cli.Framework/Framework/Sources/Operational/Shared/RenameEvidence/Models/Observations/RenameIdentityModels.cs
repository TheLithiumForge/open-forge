using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Content;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

internal sealed record RenameMetadataSignature(string Description, ImmutableArray<string> Tags, ImmutableArray<string> ApplyTo);

internal sealed record RenameRecordedIdentity
{
    public required string Path { get; init; }
    public required RenameMetadataSignature? Metadata { get; init; }
    public string? Title { get; init; }
    public RenameContent? Content { get; init; }
    public string? Head { get; init; }
    public string? Blob { get; init; }
    public FileExpectation? RecordExpectation { get; init; }
}

internal sealed record RenameCurrentCandidate(string Path, FileStateSnapshot Snapshot, RenameContent Content, RenameMetadataSignature? Metadata);

internal sealed record RenameMissingPath(string Path, FileExpectation Expectation);

internal sealed record RenameWeakCandidate(string OldPath, LocalReferenceCandidateObservation Candidate);
