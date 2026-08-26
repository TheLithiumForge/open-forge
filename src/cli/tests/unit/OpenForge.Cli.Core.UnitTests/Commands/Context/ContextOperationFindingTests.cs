using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Context;

public sealed class ContextOperationFindingTests
{
    [Theory(DisplayName = "Context direct operation maps failure and interruption to exact terminal results"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData(false, (int)CliSemanticStatus.Failed, (int)ContextFindingCode.OperationFailed, (int)ContextCoverageState.Failed)]
    [InlineData(true, (int)CliSemanticStatus.Interrupted, (int)ContextFindingCode.Interrupted, (int)ContextCoverageState.Interrupted)]
    public async Task OperationEventsAreExactTerminalResults(
        bool interrupted,
        int expectedStatusValue,
        int expectedFindingValue,
        int expectedCoverageValue)
    {
        using var workspace = ContextOperationWorkspace.Create();
        var request = Request(workspace, []);
        using var cancellation = new CancellationTokenSource();
        ContextOperation operation;
        if (interrupted)
        {
            cancellation.Cancel();
            operation = ContextOperationFactory.Create();
        }
        else
        {
            operation = new ContextOperation((_, _) => throw new IOException("Injected Context graph failure."));
        }

        var result = await operation.ExecuteAsync(request, cancellation.Token);

        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        Assert.Equal((ContextCoverageState)expectedCoverageValue, result.Coverage.State);
        Assert.Equal((ContextFindingCode)expectedFindingValue, Assert.Single(result.Findings).Code);
        Assert.NotNull(result.Next);
        Assert.Empty(result.Sources);
        Assert.Empty(result.Links);
    }

    [Fact(DisplayName = "Context invalid source selection stops before startup resolution")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task InvalidSourceStopsBeforeResolution()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, ["missing-source"], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.False(result.Selection.StartupIncluded);
        Assert.Null(result.Selection.SourceCount);
        Assert.Equal(ContextCoverageState.NotStarted, result.Coverage.State);
        Assert.Empty(result.Sources);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.InvalidSource);
    }

    [Theory(DisplayName = "Context cannot report complete when global continuity membership metadata is malformed or unreadable"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnavailableGlobalContinuityMetadataIsIncomplete(bool invalidEncoding)
    {
        using var workspace = ContextOperationWorkspace.Create();
        var checkpoint = Path.Combine(workspace.Root, ".agents/state/checkpoint.md");
        if (invalidEncoding)
        {
            File.WriteAllBytes(checkpoint, [0xFF, 0xFE, 0xFD]);
        }
        else
        {
            File.WriteAllText(checkpoint, "---\nopen-forge: [unterminated\n---\n# Checkpoint\n");
        }

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ContextCoverageState.Incomplete, result.Coverage.State);
        Assert.Equal(ContextCoverageState.Incomplete, result.Coverage.Selection);
        var finding = Assert.Single(result.Findings, finding =>
            finding.Code == ContextFindingCode.ClosureUnavailable
            && finding.Path == ".agents/state/checkpoint.md");
        Assert.Contains("continuity membership", finding.Cause, StringComparison.Ordinal);
        Assert.NotNull(result.Next);
    }

    [Fact(DisplayName = "Context ignores unavailable metadata on a definitively unrouted non-continuity source"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task UnroutedUnavailableMetadataDoesNotInvalidateContinuityCoverage()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var unrelated = Path.Combine(workspace.Root, ".agents/unrouted.md");
        File.WriteAllText(unrelated, "---\nopen-forge: [unterminated\n---\n# Unrouted\n");

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ContextCoverageState.Complete, result.Coverage.Selection);
        Assert.DoesNotContain(result.Findings, finding => finding.Path == ".agents/unrouted.md");
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/unrouted.md");
    }

    [Fact(DisplayName = "Context reports an authored global continuity source whose route is broken"), Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task BrokenGlobalContinuityRouteIsIncomplete()
    {
        using var workspace = ContextOperationWorkspace.Create();
        Directory.CreateDirectory(Path.Combine(workspace.Root, ".agents/projects/ambiguous"));
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/ambiguous/_ambiguous.md"),
            "---\nopen-forge:\n  description: Canonical ambiguous\n  tags: [Project]\n---\n\n# Canonical\n");
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/ambiguous/index.md"),
            "---\nopen-forge:\n  description: Compatibility ambiguous\n  tags: [Project]\n---\n\n# Compatibility\n");
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/ambiguous/continuity.md"),
            "---\nopen-forge:\n  description: Ambiguous continuity\n  tags: [KeepInMind, Memory]\n---\n\n# Ambiguous continuity\n");

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ContextCoverageState.Incomplete, result.Coverage.Selection);
        var finding = Assert.Single(result.Findings, finding =>
            finding.Code == ContextFindingCode.ClosureUnavailable
            && finding.Path == ".agents/projects/ambiguous/continuity.md");
        Assert.Contains("route", finding.Cause, StringComparison.Ordinal);
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/projects/ambiguous/continuity.md");
    }

    [Fact(DisplayName = "Context retains same-code link findings in breadth-first authored edge order")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task SameCodeLinkFindingsRetainAuthoredOrder()
    {
        using var workspace = ContextOperationWorkspace.Create();
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/guide.md"),
            $"{ContextOperationWorkspace.GuideFrontmatter}\n# Guide\n\n[Z](z-missing.md) [A](a-missing.md).\n");
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.All);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(
            ["z-missing.md", "a-missing.md"],
            result.Findings
                .Where(finding => finding.Code == ContextFindingCode.TargetMissing)
                .Select(finding => finding.Subject));
    }

    [Fact(DisplayName = "Context treats an exact orphan overwrite reference as a blocked overwrite boundary")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task OrphanOverwriteReferenceIsBlocked()
    {
        using var workspace = ContextOperationWorkspace.Create();
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/orphan.overwrite.md"),
            "# Orphan overwrite\n");
        var result = await ExecuteAsync(
            workspace,
            [".agents/projects/orphan.overwrite.md"],
            Content("metadata"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.OverwriteAmbiguous);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ContextFindingCode.InvalidSource);
    }

    [Fact(DisplayName = "Context exact-path selection retains a safe automatic-ID collision as attention")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task ExactPathIdentityCollisionIsAttention()
    {
        using var workspace = ContextOperationWorkspace.Create();
        Directory.CreateDirectory(Path.Combine(workspace.Root, ".agents/projects/guide"));
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/guide/_guide.md"),
            "# Colliding guide entrypoint\n");
        var result = await ExecuteAsync(
            workspace,
            [".agents/projects/guide.md"],
            Content("metadata"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var finding = Assert.Single(result.Findings, finding => finding.Code == ContextFindingCode.IdentityCollision);
        Assert.Equal(
            [".agents/projects/guide.md", ".agents/projects/guide/_guide.md"],
            finding.Candidates.Select(candidate => candidate.Path));
    }

    [Fact(DisplayName = "Context resolves an exact local target case mismatch as safe attention")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task ExactTargetCaseMismatchIsAttention()
    {
        using var workspace = ContextOperationWorkspace.Create();
        File.Move(
            Path.Combine(workspace.Root, ".agents/projects/linked.md"),
            Path.Combine(workspace.Root, ".agents/projects/Linked.md"));
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.Bounded(1));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var link = Assert.Single(result.Links, link => link.RawDestination == "linked.md#details");
        Assert.Equal(ContextLinkResolution.CaseMismatch, link.Target.Resolution);
        Assert.Equal(".agents/projects/Linked.md", link.Target.Path);
        Assert.Equal(ContextLinkDisposition.Selected, link.Disposition);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.TargetCaseMismatch);
    }

    [Fact(DisplayName = "Context unavailable authored frontmatter forms an incomplete projection finding")]
    [Trait("Feature", "context"), Trait("Evidence", "Unit")]
    public async Task UnavailableFrontmatterIsIncomplete()
    {
        using var workspace = ContextOperationWorkspace.Create();
        File.WriteAllText(
            Path.Combine(workspace.Root, ".agents/projects/guide.md"),
            "---\nopen-forge:\n  description: Unterminated\n# Guide\n");
        var result = await ExecuteAsync(
            workspace,
            [".agents/projects/guide.md"],
            Content("frontmatter"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.ProjectionUnavailable);
        var guide = Assert.Single(result.Sources, source => source.Path == ".agents/projects/guide.md");
        Assert.Equal(ContextProjectionState.Unavailable, Assert.Single(guide.Layers[0].Projections).State);
    }

    private static ValueTask<ContextResult> ExecuteAsync(
        ContextOperationWorkspace workspace,
        IEnumerable<string> sources,
        ContextContentSelection content,
        bool additionsOnly = false,
        ContextLinkExpansion? linkExpansion = null)
        => ContextOperationFactory.Create().ExecuteAsync(
            Request(workspace, sources, content, additionsOnly, linkExpansion),
            CancellationToken.None);

    private static ContextRequest Request(
        ContextOperationWorkspace workspace,
        IEnumerable<string> sources,
        ContextContentSelection? content = null,
        bool additionsOnly = false,
        ContextLinkExpansion? linkExpansion = null)
        => new(
            workspace: workspace.Workspace,
            sourceReferences: sources,
            additionsOnly: additionsOnly,
            content: content ?? Content("metadata"),
            linkExpansion: linkExpansion ?? ContextLinkExpansion.None,
            suppliedView: null,
            effectiveView: CliView.Expanded);

    private static ContextContentSelection Content(string value)
    {
        var part = value switch
        {
            "metadata" => new ContextContentPart(
                kind: ContextContentPartKind.Metadata,
                name: null,
                canonicalValue: value),
            "frontmatter" => new ContextContentPart(
                kind: ContextContentPartKind.Frontmatter,
                name: null,
                canonicalValue: value),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The Context test content part is not defined."),
        };
        return new ContextContentSelection(supplied: [part], effective: [part]);
    }
}
