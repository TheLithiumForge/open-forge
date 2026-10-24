using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameEvidenceTestFacts
{
    internal const string OldPath = ".agents/guidance/old.md";
    internal const string NewPath = ".agents/guidance/new.md";
    internal const string Head = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    internal const string Blob = "bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
    internal const string Body = "# Different title\n\nAuthored rules for a specific subject.\n";

    internal static string Absolute(string path) => Path.Combine(Path.GetTempPath(), "rename-evidence-unit", path.Replace('/', Path.DirectorySeparatorChar));

    internal static RenameCurrentCandidate Candidate(string path = NewPath, string body = Body, RenameMetadataSignature? metadata = null)
    {
        var bytes = Encoding.UTF8.GetBytes(body);
        return new(path, FileStateSnapshot.File(Absolute(path), Absolute(path), bytes), RenameContentReader.Read(bytes), metadata);
    }

    internal static RenameRecordedIdentity Old(string path = OldPath, string? body = Body, RenameMetadataSignature? metadata = null)
        => new()
        {
            Path = path,
            Content = body is null ? null : RenameContentReader.Read(Encoding.UTF8.GetBytes(body)),
            Metadata = metadata,
            Head = body is null ? null : Head,
            Blob = body is null ? null : Blob,
        };

    internal static RenameEvidenceFacts Facts(ImmutableArray<RenameCurrentCandidate> candidates, ImmutableArray<RenameRecordedIdentity> old)
        => new()
        {
            Layout = WorkspaceLayout.Create(new CliWorkspace(Absolute(string.Empty), Absolute(string.Empty), CliWorkspaceSelectionMethod.ExplicitWorkspace), WorkspaceLayout.DefaultMainRoot),
            Candidates = candidates,
            OldIdentities = old,
            MissingPaths = old.Select(identity => new RenameMissingPath(identity.Path, FileExpectation.Missing(Absolute(identity.Path)))).DistinctBy(missing => missing.Path).ToImmutableArray(),
            CandidateUniverseComplete = true,
            Git = new() { Availability = RenameGitAvailability.Available, Head = Head },
        };
}
