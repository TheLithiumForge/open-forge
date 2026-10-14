using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameCandidateAdmissionIntegrationTests
{
    [Fact(DisplayName = "Managed private excluded package overwrite and projection records never enter the candidate universe")]
    public async Task ExcludedRecords()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        foreach (var path in new[] { ".agents/guidance/managed.md", ".agents/guidance/private.md", ".agents/guidance/excluded.md",
            ".agents/skills/package/SKILL.md", ".agents/skills/package/references/note.md", ".agents/guidance/note.overwrite.md", ".agents/guidance/projection.md" })
        {
            repository.Write(path, "# Authored\n\nSame authored body.\n");
        }
        repository.Write(".agents/guidance/ordinary.md", "# Authored\n\nSame authored body.\n");
        using var privateHandle = new FileStream(repository.PathFor(".agents/guidance/private.md"), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var request = repository.Request() with
        {
            Policy = new RenameEvidencePolicy
            {
                ManagedPaths = [".agents/guidance/managed.md"],
                PrivatePaths = [".agents/guidance/private.md"],
                ExcludedPaths = [".agents/guidance/excluded.md"],
                ProjectionPaths = [".agents/guidance/projection.md"],
            },
        };
        var result = await new RenameEvidenceReader(new PhysicalPathResolver()).ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(".agents/guidance/ordinary.md", Assert.Single(result.Facts.Candidates).Path);
    }

    [Fact(DisplayName = "Directories non-Markdown invalid UTF-8 and uncontained names are excluded from ordinary evidence")]
    public async Task OrdinaryReadableContainedOnly()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        repository.Write(".agents/guidance/ordinary.md", "# Authored\n");
        repository.Write(".agents/guidance/resource.txt", "Resource");
        repository.Write(".agents/guidance/binary.md", "placeholder");
        File.WriteAllBytes(repository.PathFor(".agents/guidance/binary.md"), [0xFF]);
        Directory.CreateDirectory(repository.PathFor(".agents/guidance/directory.md"));
        var request = repository.Request() with
        {
            CandidatePaths = [".agents/guidance/ordinary.md", ".agents/guidance/resource.txt", ".agents/guidance/binary.md", ".agents/guidance/directory.md", ".agents/../outside.md"],
        };
        var result = await new RenameEvidenceReader(new PhysicalPathResolver()).ReadAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(".agents/guidance/ordinary.md", Assert.Single(result.Facts.Candidates).Path);
    }
}
