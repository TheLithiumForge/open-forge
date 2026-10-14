using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RenameEvidenceGitIntegrationTests
{
    private const string OldPath = ".agents/directives/csharp/style.md";
    private const string NewPath = ".agents/guidance/moved-style.md";

    [Fact(DisplayName = "A tracked missing HEAD path and untracked successor are proved without any filesystem or Git writes")]
    public async Task TrackedMissingUntrackedSuccessor()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var original = File.ReadAllBytes(repository.PathFor(OldPath));
        repository.Move(OldPath, NewPath);
        var before = repository.Snapshot();

        var result = await Read(repository);

        var proof = Assert.IsType<RenameEvidenceObservation>(result.Find(OldPath)?.Proven);
        Assert.Equal(NewPath, proof.NewPath);
        Assert.Equal(RenameEvidenceBasis.CommittedContent, proof.Basis);
        Assert.True(proof.SameContent);
        Assert.Equal(original, proof.CandidateSnapshot.Bytes);
        Assert.NotNull(proof.Head);
        Assert.NotNull(proof.OldBlob);
        Assert.Equal(before, repository.Snapshot());
    }

    [Fact(DisplayName = "Committed identity survives Index removing the old generated Entries row")]
    public async Task AlreadyIndexedOrder()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        repository.Move(OldPath, NewPath);
        repository.Write(".agents/directives/csharp/_csharp.md", "# C sharp\n\n## Entries\n\n- [Current description](../../../guidance/moved-style.md) - #Guidance\n");
        var result = await Read(repository);
        Assert.Equal(RenameEvidenceBasis.CommittedContent, result.Find(OldPath)?.Proven?.Basis);
    }

    [Fact(DisplayName = "An edited untracked successor uses the directly verified weighted score rather than exact content wording")]
    public async Task EditedSuccessor()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        repository.Move(OldPath, NewPath);
        File.AppendAllText(repository.PathFor(NewPath), "\nAn additional authored rule.\n");
        var result = await Read(repository);
        var proof = Assert.IsType<RenameEvidenceObservation>(result.Find(OldPath)?.Proven);
        Assert.Equal(RenameEvidenceBasis.GitRename, proof.Basis);
        Assert.InRange(Assert.IsType<int>(proof.Similarity), 90, 99);
        Assert.False(proof.SameContent);
        Assert.NotEqual(proof.OldHash, proof.NewHash);
    }

    [Fact(DisplayName = "Old HEAD metadata can prove a heavily edited file after the stale Entries row is gone")]
    public async Task HeadMetadataForEditedSuccessor()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var old = RenameContentReader.Read(File.ReadAllBytes(repository.PathFor(OldPath)));
        var body = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(old.Document.BodySpan);
        repository.Move(OldPath, NewPath);
        File.WriteAllText(repository.PathFor(NewPath), old.Document.Source[..body.Start] + "# Completely rewritten\n\nA different authored rule.\n");
        var result = await Read(repository);
        Assert.Equal(RenameEvidenceBasis.RecordedMetadata, result.Find(OldPath)?.Proven?.Basis);
    }

    [Fact(DisplayName = "Two identical successors remain guided and deletion has no invented successor")]
    public async Task AmbiguousThenDeleted()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        repository.Move(OldPath, NewPath);
        var duplicate = ".agents/guidance/second-style.md";
        File.Copy(repository.PathFor(NewPath), repository.PathFor(duplicate));
        var ambiguous = await Read(repository);
        Assert.Equal(RenameResolution.SeveralStrongCandidates, ambiguous.Find(OldPath)?.Resolution);
        File.Delete(repository.PathFor(NewPath));
        File.Delete(repository.PathFor(duplicate));
        var deleted = await Read(repository);
        Assert.Equal(RenameResolution.NoCandidate, deleted.Find(OldPath)?.Resolution);
    }

    [Fact(DisplayName = "Every file in a folder move needs its own proof including the canonical entrypoint")]
    public async Task FolderMoveIsPerFile()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        repository.Write(".agents/directives/csharp/untracked.md", "# Untracked child\n\nUnrecorded authored content.\n");
        Directory.Move(repository.PathFor(".agents/directives/csharp"), repository.PathFor(".agents/directives/renamed"));
        var result = await Read(repository, ".agents/directives/csharp/untracked.md");
        foreach (var leaf in new[] { "_csharp.md", "design.md", "style.md" })
        {
            Assert.Equal($".agents/directives/renamed/{leaf}", result.Find($".agents/directives/csharp/{leaf}")?.Proven?.NewPath);
        }

        Assert.Equal(RenameResolution.NoCandidate, result.Find(".agents/directives/csharp/untracked.md")?.Resolution);
        var canonical = Assert.Single(RenameCanonicalEntrypointEvidence.Read(result));
        Assert.Equal(".agents/directives/renamed/_renamed.md", canonical.CanonicalPath);
        Assert.Equal(".agents/directives/renamed/_csharp.md", canonical.Proof.NewPath);
    }

    [Fact(DisplayName = "A workspace within a larger repository retains its own prefix and never admits outside candidates")]
    public async Task InsideLargerRepository()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(nested: true);
        const string old = ".agents/guidance/adaptive-collaboration.md";
        const string current = ".agents/guidance/manual-collaboration.md";
        repository.Move(old, current);
        var result = await Read(repository, old);
        Assert.Equal("src/extensions/collaboration/content/", result.Facts.Git.RepositoryPrefix);
        Assert.Equal(current, result.Find(old)?.Proven?.NewPath);
        Assert.All(result.Facts.Git.TrackedPaths, tracked => Assert.StartsWith(".agents/", tracked.Path));
        Assert.Single(result.Resolutions);
    }

    [Fact(DisplayName = "An unborn repository supplies an internal optional observation without blocking ordinary candidates")]
    public async Task UnbornHead()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        repository.Write(NewPath, "# Current note\n\nAuthored body.\n");
        var result = await Read(repository);
        Assert.Equal(RenameGitAvailability.Unborn, result.Facts.Git.Availability);
        Assert.Single(result.Facts.Candidates);
        Assert.Equal(RenameResolution.NoCandidate, result.Find(OldPath)?.Resolution);
    }

    [Fact(DisplayName = "The byte Git runner preserves UTF-8 BOM and CRLF blob bytes")]
    public async Task BlobBytesStayExact()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        repository.Write(NewPath, "\uFEFF# Authored\r\n\r\nBody without final newline");
        var runner = new GitProcessRunner("git", repository.Configure);
        var blob = await runner.RunAsync(repository.Root, ["hash-object", "-w", NewPath], TestContext.Current.CancellationToken);
        Assert.NotNull(blob);
        Assert.Equal(0, blob.Value.ExitCode);
        var read = await runner.RunBytesAsync(repository.Root, ["cat-file", "blob", blob.Value.Output.Trim()], TestContext.Current.CancellationToken);
        Assert.NotNull(read);
        Assert.Equal(File.ReadAllBytes(repository.PathFor(NewPath)), read.Value.Output);
    }

    [Fact(DisplayName = "Fresh evidence rejects changed bytes disappearance restored old paths and a new competing successor")]
    public async Task Revalidation()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        repository.Move(OldPath, NewPath);
        var before = await Read(repository);
        Assert.True(RenameEvidenceRevalidator.IsCurrent(before, await Read(repository), OldPath));
        var bytes = File.ReadAllBytes(repository.PathFor(NewPath));
        File.AppendAllText(repository.PathFor(NewPath), "\nAn edit.\n");
        Assert.False(RenameEvidenceRevalidator.IsCurrent(before, await Read(repository), OldPath));
        File.WriteAllBytes(repository.PathFor(NewPath), bytes);
        repository.Write(".agents/guidance/competing.md", System.Text.Encoding.UTF8.GetString(bytes));
        Assert.False(RenameEvidenceRevalidator.IsCurrent(before, await Read(repository), OldPath));
        File.Delete(repository.PathFor(".agents/guidance/competing.md"));
        File.WriteAllBytes(repository.PathFor(OldPath), bytes);
        Assert.False(RenameEvidenceRevalidator.IsCurrent(before, await Read(repository), OldPath));
        File.Delete(repository.PathFor(OldPath));
        File.Delete(repository.PathFor(NewPath));
        Assert.False(RenameEvidenceRevalidator.IsCurrent(before, await Read(repository), OldPath));
    }

    private static ValueTask<RenameEvidenceResult> Read(RenameEvidenceGitWorkspace repository, params string[] missing)
        => new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader(configureProcess: repository.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync)
            .ReadAsync(repository.Request(missing.Length == 0 ? [OldPath] : missing), TestContext.Current.CancellationToken);

    [Fact(DisplayName = "Metadata uniqueness retains old HEAD signatures even after a still-present file is edited")]
    public async Task LiveHeadMetadataRemainsAvailable()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync();
        var bytes = File.ReadAllBytes(repository.PathFor(OldPath));
        repository.Write(OldPath, "---\ndescription: A different current description\ntags: [Guidance]\n---\n\n# Current title\n\nA different body.\n");
        var result = await Read(repository);
        var record = Assert.Single(result.Facts.OldIdentities.Where(record => record.Path == OldPath));
        Assert.NotNull(record.Metadata);
        Assert.NotEqual("A different current description", record.Metadata.Description);
        Assert.Equal(bytes, record.Content?.Bytes ?? []);
    }
}
