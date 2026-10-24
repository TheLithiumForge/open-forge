using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameMetadataEvidenceIntegrationTests
{
    internal const string Old = ".agents/guidance/billing/old.md";
    internal const string New = ".agents/guidance/invoices/new.md";
    internal const string Parent = ".agents/guidance/_guidance.md";
    internal const string Note = "---\nopen-forge:\n  description: Specific authored rounding policy\n  tags: [Guidance, Billing]\n  applyTo: ['tests/**', 'src/**']\n---\n\n# A distinct title\n\nAn edited authored body.\n";

    [Theory(DisplayName = "A valid stale Entries signature proves an edited move with no repository no executable or unborn HEAD")]
    [InlineData("no-repository"), InlineData("no-executable"), InlineData("unborn")]
    public async Task StaleEntries(string gitState)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        if (gitState == "unborn") await repository.InitializeAsync(unborn: true);
        repository.Write(New, Note);
        repository.Write(Parent, "# Guidance\n\n## Entries\n\n- [Specific authored rounding policy](billing/old.md) - #Guidance #Billing - applies to `src/**`, `tests/**`\n");
        var records = ReadEntries(repository);
        Assert.Single(records);
        var executable = gitState == "no-executable" ? Path.Combine(repository.Root, "missing-git") : "git";
        var reader = new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader(executable, repository.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync);
        var before = repository.Snapshot();
        var result = await reader.ReadAsync(repository.Request(Old) with { RecordedIdentities = records }, TestContext.Current.CancellationToken);
        var proof = Assert.IsType<RenameEvidenceObservation>(result.Find(Old)?.Proven);
        Assert.Equal(New, proof.NewPath);
        Assert.Equal(RenameEvidenceBasis.RecordedMetadata, proof.Basis);
        Assert.Equal(["src/**", "tests/**"], proof.Metadata?.ApplyTo);
        Assert.Equal(records[0].RecordExpectation, proof.RecordExpectation);
        Assert.NotEqual(RenameGitAvailability.Available, result.Facts.Git.Availability);
        Assert.Equal(before, repository.Snapshot());
    }

    [Fact(DisplayName = "Duplicate metadata signatures stay guided rather than selecting by enumeration order")]
    public async Task DuplicateMetadata()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        repository.Write(New, Note);
        repository.Write(".agents/guidance/invoices/second.md", Note);
        repository.Write(Parent, "# Guidance\n\n## Entries\n\n- [Specific authored rounding policy](billing/old.md) - #Guidance #Billing - applies to `src/**`, `tests/**`\n");
        var result = await new RenameEvidenceReader(new PhysicalPathResolver())
            .ReadAsync(repository.Request(Old) with { RecordedIdentities = ReadEntries(repository) }, TestContext.Current.CancellationToken);
        Assert.Equal(RenameResolution.WeakOnly, result.Find(Old)?.Resolution);
        Assert.Equal(2, result.Find(Old)?.GuidedCandidatePaths.Length);
        Assert.Empty(result.Find(Old)?.StrongEvidence ?? []);
    }

    [Fact(DisplayName = "Index removes stale rows without inventing non-Git identity history")]
    public async Task StaleRecordRemoved()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        repository.Write(New, Note);
        repository.Write(Parent, "# Guidance\n\n## Entries\n\n- [Specific authored rounding policy](invoices/new.md) - #Guidance #Billing - applies to `src/**`, `tests/**`\n");
        var result = await new RenameEvidenceReader(new PhysicalPathResolver())
            .ReadAsync(repository.Request(Old) with { RecordedIdentities = ReadEntries(repository) }, TestContext.Current.CancellationToken);
        Assert.Equal(RenameResolution.NoCandidate, result.Find(Old)?.Resolution);
    }

    internal static System.Collections.Immutable.ImmutableArray<RenameRecordedIdentity> ReadEntries(RenameEvidenceGitWorkspace repository)
    {
        var bytes = File.ReadAllBytes(repository.PathFor(Parent));
        var content = RenameContentReader.Read(bytes);
        return RenameRecordedEntryReader.Read(WorkspaceLayout.Create(repository.Workspace, WorkspaceLayout.DefaultMainRoot), Parent,
            FileStateSnapshot.File(repository.PathFor(Parent), repository.PathFor(Parent), bytes), content.Document);
    }
}
