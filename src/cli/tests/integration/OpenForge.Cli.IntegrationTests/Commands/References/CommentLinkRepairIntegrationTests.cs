using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "comment-links"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class CommentLinkRepairIntegrationTests
{
    private const string CodePath = "src/Order.cs";
    private const string OldFragment = "rule";
    private static string GuidePath => $"{WorkspaceLayout.DefaultMainRoot}/docs/guide.md";
    private static string NewGuidePath => $"{WorkspaceLayout.DefaultMainRoot}/docs/new-guide.md";

    [Theory(DisplayName = "A suggested comment target with spaces survives preview application and fresh diagnosis"), InlineData(false), InlineData(true)]
    public async Task TargetWithSpaces(bool encodedSelection)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        var oldTarget = $"{GuidePath}#{OldFragment}";
        var authored = $"// {SourceCommentLinkReader.OpenForgeMarker} {oldTarget} explains this behavior\nclass Order {{}}\n";
        repository.Write(CodePath, authored);
        await Commit(repository);
        var targetPath = $"{WorkspaceLayout.DefaultMainRoot}/docs/new guide %20.md";
        repository.Move(GuidePath, targetPath);
        await Commit(repository);

        var view = await Read(repository);
        var occurrence = Assert.Single(view.References, item => item.SourcePath == CodePath);
        var candidate = Assert.Single(RepairCandidateMapper.Read(occurrence, view).Items,
            item => Uri.UnescapeDataString(item.Target.CanonicalTargetPath) == targetPath);
        var selection = encodedSelection ? SourceDestinationEncoder.EncodePath(targetPath) : candidate.Target.CanonicalTargetPath;
        var location = Assert.IsType<Core.Framework.Sources.Models.Locations.SourceLocation>(occurrence.DestinationLocation);
        var relink = new RepairRelinkRequest(CodePath, location.Line, location.Column, oldTarget, selection, OldFragment);
        var operation = new RepairOperation(RepairOperationFactory.CreateDefaultComponents(historyRead:
            new GitSourceLinkHistoryReader(repository.Configure).ReadAsync));
        var before = repository.Snapshot();
        var preview = await operation.ExecuteAsync(new(repository.Workspace, RepairMode.DryRun, false, [relink], false), TestContext.Current.CancellationToken);
        Assert.False(preview.Plan?.IsBlocked ?? true);
        var intended = $"{SourceDestinationEncoder.EncodePath(targetPath)}#{OldFragment}";
        Assert.Equal(intended, Assert.Single(preview.Plan?.Selection.Selected ?? []).Resolution.IntendedDestination);
        Assert.Equal(before, repository.Snapshot());
        var applied = await operation.ExecuteAsync(new(repository.Workspace, RepairMode.Apply, false, [relink], false), TestContext.Current.CancellationToken);
        Assert.False(applied.Plan?.IsBlocked ?? true);
        Assert.Equal(RepairVerificationState.Verified, applied.Verification.ResultingBytes);
        Assert.Equal(authored.Replace(oldTarget, intended, StringComparison.Ordinal), File.ReadAllText(repository.PathFor(CodePath)));
        var fresh = Assert.Single((await Read(repository)).References, item => item.SourcePath == CodePath);
        Assert.Equal(targetPath, fresh.Facts.Target.Path);
        Assert.Equal(OldFragment, fresh.Facts.Fragment);
        Assert.Null(fresh.Facts.Finding);
    }

    [Theory(DisplayName = "Git target diff identifies a unique heading rename with a changed or empty body"), InlineData(false), InlineData(true)]
    public async Task HeadingDiff(bool empty)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        repository.Write(CodePath, $"// {SourceCommentLinkReader.RuneMarker} {GuidePath}#{OldFragment} explains this behavior\n");
        if (empty) repository.Write(GuidePath, Document("# Guide\n\n## Rule\n"));
        await Commit(repository);
        repository.Write(GuidePath, Document(empty ? "# Guide\n\n## Changed rule\n" : "# Guide\n\n## Changed rule\n\nA changed section body.\n"));
        await Commit(repository);
        var before = repository.Snapshot();
        var view = await Read(repository);
        var occurrence = Assert.Single(view.References, item => item.SourcePath == CodePath);
        var candidate = Assert.Single(view.Candidates, item => item.SourcePath == CodePath && item.Candidate.Path == GuidePath);
        Assert.Equal("changed-rule", candidate.Fragment);
        Assert.Contains(candidate.Bases, basis => basis.Kind == LocalReferenceCandidateBasisKind.GitHeadingChange);
        Assert.Contains(Assert.Single(RepairCandidateMapper.Read(occurrence, view).Items).Evidence,
            evidence => evidence.Kind == RepairCandidateEvidenceKind.GitHeadingChange);
        Assert.Equal(before, repository.Snapshot());
    }

    [Fact(DisplayName = "Repair labels a unique heading slug as weak in its target choice"), Trait("Boundary", "Output")]
    public async Task WeakSlugRepairChoice()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        repository.Write(CodePath, $"// {SourceCommentLinkReader.OpenForgeMarker} {GuidePath}#relocated-rule explains this behavior\n");
        repository.Write(NewGuidePath, Document("# New guide\n\n## Relocated rule\n\nAn unrelated section body.\n"));
        var before = repository.Snapshot();
        var result = await new RepairOperation().ExecuteAsync(new(repository.Workspace, RepairMode.DryRun, false, [], false), TestContext.Current.CancellationToken);
        var candidate = Assert.Single(Assert.Single(result.PostDiagnosis.Proposals, item => item.SourceCanonicalPath == CodePath).Candidates?.Items ?? []);
        Assert.Contains(candidate.Evidence, evidence => evidence.Kind == RepairCandidateEvidenceKind.UniqueSlug);
        CommandOutputRenderers<RepairResult>.From(RepairPresentation.Rendering).MatchDetails(result, "repair-weak-slug-choice");
        Assert.Equal(before, repository.Snapshot());
    }

    [Theory(DisplayName = "Git file and heading moves produce selectable comment repairs that preserve surrounding bytes"), InlineData(false), InlineData(true)]
    public async Task MoveAndHeading(bool heading)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        var oldTarget = $"{GuidePath}#{OldFragment}";
        var authored = $"// {SourceCommentLinkReader.OpenForgeMarker} {oldTarget} explains this behavior\nclass Order {{ void Save() {{}} }}\n";
        repository.Write(CodePath, authored);
        await Commit(repository);
        var targetPath = heading ? GuidePath : NewGuidePath;
        var fragment = heading ? "updated-rule" : OldFragment;
        if (heading)
            repository.Write(GuidePath, Document("# Guide\n\n## Updated rule\n\nKeep this section body.\n"));
        else repository.Move(GuidePath, NewGuidePath);
        await Commit(repository);

        var view = await Read(repository);
        var occurrence = Assert.Single(view.References, item => item.SourcePath == CodePath);
        var suggested = Assert.Single(view.Candidates, item => item.SourcePath == CodePath
            && item.Candidate.Path == targetPath && item.Fragment == fragment);
        Assert.Contains(suggested.Bases, basis => basis.Kind == (heading
            ? LocalReferenceCandidateBasisKind.GitHeadingChange : LocalReferenceCandidateBasisKind.GitRename));
        Assert.Contains(Assert.Single(RepairCandidateMapper.Read(occurrence, view).Items,
                item => item.Target.CanonicalTargetPath == targetPath).Evidence,
            evidence => evidence.Kind == (heading ? RepairCandidateEvidenceKind.GitHeadingChange : RepairCandidateEvidenceKind.GitRename));
        var location = Assert.IsType<OpenForge.Cli.Core.Framework.Sources.Models.Locations.SourceLocation>(occurrence.DestinationLocation);
        var relink = new RepairRelinkRequest(CodePath, location.Line, location.Column, oldTarget, targetPath, fragment);
        var operation = new RepairOperation(RepairOperationFactory.CreateDefaultComponents(historyRead:
            new GitSourceLinkHistoryReader(repository.Configure).ReadAsync));
        var before = repository.Snapshot();
        var preview = await operation.ExecuteAsync(new(repository.Workspace, RepairMode.DryRun, false, [relink], false), TestContext.Current.CancellationToken);
        Assert.False(preview.Plan?.IsBlocked ?? true);
        Assert.Single(preview.Plan?.Selection.Selected ?? []);
        Assert.Equal(before, repository.Snapshot());
        var applied = await operation.ExecuteAsync(new(repository.Workspace, RepairMode.Apply, false, [relink], false), TestContext.Current.CancellationToken);
        Assert.False(applied.Plan?.IsBlocked ?? true);
        Assert.Equal(authored.Replace(oldTarget, $"{targetPath}#{fragment}", StringComparison.Ordinal),
            File.ReadAllText(repository.PathFor(CodePath)));
    }

    [Fact(DisplayName = "An uncommitted moved code target is suggested from its committed content")]
    public async Task UncommittedCodeMove()
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        repository.Write(CodePath, "class Order { void Save() {} }\n");
        repository.Write(GuidePath, Document($"# Guide\n\n<!-- {SourceCommentLinkReader.RuneMarker} {CodePath}#Order.Save implements this rule -->\n"));
        await Commit(repository);
        const string movedCode = "src/MovedOrder.cs";
        repository.Move(CodePath, movedCode);
        var before = repository.Snapshot();
        var view = await Read(repository);
        Assert.Contains(view.Candidates, candidate => candidate.SourcePath == GuidePath && candidate.Candidate.Path == movedCode
            && candidate.Bases.Any(basis => basis.Kind == LocalReferenceCandidateBasisKind.GitRename));
        Assert.Equal(before, repository.Snapshot());
    }

    [Theory(DisplayName = "Cross-document suggestions distinguish historical section content from a unique weak slug"), InlineData(true), InlineData(false)]
    public async Task SectionSuggestions(bool history)
    {
        using var repository = new RenameEvidenceGitWorkspace();
        await repository.InitializeAsync(unborn: true);
        Seed(repository);
        repository.Write(CodePath, $"// {SourceCommentLinkReader.OpenForgeMarker} {GuidePath}#{OldFragment} explains this behavior\n");
        if (history) await Commit(repository);
        repository.Write(GuidePath, Document("# Guide\n\n## Replacement\n\nA different rule.\n"));
        var heading = history ? "Moved rule" : "Rule";
        var body = history ? "Keep this section body." : "An unrelated section body.";
        repository.Write(NewGuidePath, Document($"# New guide\n\n## {heading}\n\n{body}\n"));

        var before = repository.Snapshot();
        var view = await Read(repository);
        var candidate = Assert.Single(view.Candidates, item => item.SourcePath == CodePath && item.Candidate.Path == NewGuidePath);
        Assert.Equal(history ? "moved-rule" : OldFragment, candidate.Fragment);
        Assert.Contains(candidate.Bases, basis => basis.Kind == (history
            ? LocalReferenceCandidateBasisKind.SectionContent : LocalReferenceCandidateBasisKind.UniqueSlug));
        var occurrence = Assert.Single(view.References, item => item.SourcePath == CodePath);
        Assert.Contains(Assert.Single(RepairCandidateMapper.Read(occurrence, view).Items,
                item => item.Target.CanonicalTargetPath == NewGuidePath).Evidence,
            evidence => evidence.Kind == (history ? RepairCandidateEvidenceKind.SectionContent : RepairCandidateEvidenceKind.UniqueSlug));
        Assert.Equal(before, repository.Snapshot());
    }

    private static async ValueTask<LocalReferenceDoctorView> Read(RenameEvidenceGitWorkspace repository)
    {
        var paths = new PhysicalPathResolver();
        var contributor = new LocalReferenceOperationalContributor(new RouteSourceInspector(new SourceReadSessionReader(paths), paths),
            new SourceLinkDestinationResolver(paths, StrictUtf8FileReader.ReadAsync, new MarkdownDocumentParser().Parse),
            new LocalReferenceCandidateReader(), new GitSourceLinkHistoryReader(repository.Configure).ReadAsync);
        return await contributor.ReadDoctorAsync(repository.Workspace, TestContext.Current.CancellationToken);
    }

    private static string Document(string body) => OpenForgeDocumentSeed.Metadata(description: "Guide", tags: ["Docs"], body: body);

    private static void Seed(RenameEvidenceGitWorkspace repository)
    {
        var root = WorkspaceLayout.DefaultMainRoot;
        repository.Write(SourceLogicalPath.WorkspaceEntryPath, $"# Workspace\n\nRead `{root}/{WorkspaceLayoutNames.LoaderFileName}`.\n");
        repository.Write($"{root}/{WorkspaceLayoutNames.LoaderFileName}", GeneratedLoaderDocumentBuilder.Build("- [Docs](docs/_docs.md) - #Docs"));
        repository.Write($"{root}/docs/_docs.md", OpenForgeDocumentSeed.Metadata(description: "Docs", tags: ["Docs"],
            body: "# Docs\n\n## Entries\n\n- [Guide](guide.md) - #Docs\n"));
        repository.Write(GuidePath, Document("# Guide\n\n## Rule\n\nKeep this section body.\n"));
    }

    private static async Task Commit(RenameEvidenceGitWorkspace repository)
    {
        await repository.GitAsync("add", "--all");
        await repository.GitAsync("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test", "commit", "--quiet", "-m", "fixture");
    }
}
