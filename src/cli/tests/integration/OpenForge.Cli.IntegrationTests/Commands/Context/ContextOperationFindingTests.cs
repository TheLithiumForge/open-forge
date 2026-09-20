using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextOperationFindingTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Context direct operation maps failure and interruption to exact terminal results"), Trait("Feature", "context"), Trait("Evidence", "Integration"),
     InlineData(false, (int)CliSemanticStatus.Failed, (int)ContextFindingCode.OperationFailed, (int)ContextCoverageState.Failed),
     InlineData(true, (int)CliSemanticStatus.Interrupted, (int)ContextFindingCode.Interrupted, (int)ContextCoverageState.Interrupted)]
    public static async Task OperationEventsAreExactTerminalResults(
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context invalid source selection stops before startup resolution"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task InvalidSourceStopsBeforeResolution()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, ["missing-source"], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.False(result.Selection.StartupIncluded);
        Assert.Null(result.Selection.SourceCount);
        Assert.Equal(ContextCoverageState.NotStarted, result.Coverage.State);
        Assert.Empty(result.Sources);
        var unknownId = Assert.Single(result.Findings, finding => finding.Code == ContextFindingCode.InvalidSource);
        Assert.Equal(ContextInvalidSourceKind.UnknownId, unknownId.InvalidSourceKind);

        var missingPath = await ExecuteAsync(
            workspace,
            [".agents/missing-source.md"],
            Content("metadata"));
        var unresolvedPath = Assert.Single(
            missingPath.Findings,
            finding => finding.Code == ContextFindingCode.InvalidSource);
        Assert.Equal(ContextInvalidSourceKind.Other, unresolvedPath.InvalidSourceKind);
        var rendered = CliRenderingStage.Render(
            CliPresentationStage.Create(
                missingPath,
                new CliPresentation(CliFormat.Text, CliDetail.Minimal, null)),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).PrimaryContent;
        Assert.Contains(
            "Cannot read context: The exact source path does not exist.",
            rendered,
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Context ignores inactive malformed continuity but reports it when its scope is selected"), Trait("Feature", "context"), Trait("Evidence", "Integration"),
     InlineData(false),
     InlineData(true)]
    public static async Task UnavailableContinuityMetadataOnlyAffectsSelectedScope(bool invalidEncoding)
    {
        using var workspace = ContextOperationWorkspace.Create();
        if (invalidEncoding)
        {
            workspace.ReplaceBytes(".agents/state/checkpoint.md", [0xFF, 0xFE, 0xFD]);
        }
        else
        {
            workspace.ReplaceText(
                ".agents/state/checkpoint.md",
                "---\nopen-forge: [unterminated\n---\n# Checkpoint\n");
        }

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.DoesNotContain(result.Sources, source => source.Path.StartsWith(".agents/state/", StringComparison.Ordinal));

        var selected = await ExecuteAsync(workspace, ["state"], Content("metadata"));
        Assert.Equal(CliSemanticStatus.Incomplete, selected.Status);
        Assert.Equal(ContextCoverageState.Incomplete, selected.Coverage.Selection);
        var finding = Assert.Single(selected.Findings, finding =>
            finding.Code == ContextFindingCode.ClosureUnavailable
            && finding.Path == ".agents/state/checkpoint.md");
        Assert.Contains("visible child", finding.Cause, StringComparison.Ordinal);
        Assert.NotNull(selected.Next);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context ignores unavailable metadata on a definitively unrouted non-continuity source"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnroutedUnavailableMetadataDoesNotInvalidateContinuityCoverage()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.WriteText(
            ".agents/unrouted.md",
            "---\nopen-forge: [unterminated\n---\n# Unrouted\n");

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ContextCoverageState.Complete, result.Coverage.Selection);
        Assert.DoesNotContain(result.Findings, finding => finding.Path == ".agents/unrouted.md");
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/unrouted.md");
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Context ignores valid and malformed native Skill metadata in an inactive scope"),
     Trait("Feature", "context"), Trait("Evidence", "Integration"),
     InlineData(false),
     InlineData(true)]
    public static async Task RoutedNativeSkillMetadataUsesItsAuthoredForm(bool malformed)
    {
        const string skillPath = ".agents/skills/experience-design/SKILL.md";
        using var workspace = ContextOperationWorkspace.Create();
        workspace.AddRoutedSkills(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["experience-design"] = malformed
                ? "---\nname: experience-design\n---\n# Skill\n"
                : OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Skill(
                    "experience-design",
                    "Design user experiences."),
        });

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ContextCoverageState.Complete, result.Coverage.Selection);
        Assert.DoesNotContain(result.Findings, finding => finding.Path == skillPath);
        Assert.DoesNotContain(result.Sources, source => source.Path == skillPath);
        var rendered = CliRenderingStage.Render(
            CliPresentationStage.Create(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Minimal, null)),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).PrimaryContent;
        Assert.Contains("=== ", rendered, StringComparison.Ordinal);
        Assert.DoesNotContain(skillPath, rendered, StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context ignores a broken continuity route in an inactive branch"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task BrokenInactiveContinuityRouteDoesNotAffectStartup()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.CreateDirectory(".agents/projects/ambiguous");
        workspace.WriteText(
            ".agents/projects/ambiguous/_ambiguous.md",
            "---\nopen-forge:\n  description: Canonical ambiguous\n  tags: [Project]\n---\n\n# Canonical\n");
        workspace.WriteText(
            ".agents/projects/ambiguous/index.md",
            "---\nopen-forge:\n  description: Compatibility ambiguous\n  tags: [Project]\n---\n\n# Compatibility\n");
        workspace.WriteText(
            ".agents/projects/ambiguous/continuity.md",
            "---\nopen-forge:\n  description: Ambiguous continuity\n  tags: [KeepInMind, Memory]\n---\n\n# Ambiguous continuity\n");

        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ContextCoverageState.Complete, result.Coverage.Selection);
        Assert.Empty(result.Findings);
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/projects/ambiguous/continuity.md");
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context retains same-code link findings in breadth-first authored edge order"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task SameCodeLinkFindingsRetainAuthoredOrder()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.ReplaceText(
            ".agents/projects/guide.md",
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
        var rendered = CliRenderingStage.Render(
            CliPresentationStage.Create(
                result,
                new CliPresentation(CliFormat.Text, CliDetail.Standard, null)),
            OpenForge.Cli.Core.Presentation.Context.ContextPresentation.Rendering).PrimaryContent;
        Assert.Contains(
            rendered.Split('\n'),
            line => line.Contains("The link at .agents/projects/guide.md:", StringComparison.Ordinal)
                && line.Contains("points to z-missing.md", StringComparison.Ordinal));
        Assert.Contains(
            rendered.Split('\n'),
            line => line.Contains("The link at .agents/projects/guide.md:", StringComparison.Ordinal)
                && line.Contains("points to a-missing.md", StringComparison.Ordinal));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context treats an exact orphan overwrite reference as a blocked overwrite boundary"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task OrphanOverwriteReferenceIsBlocked()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.WriteText(
            ".agents/projects/orphan.overwrite.md",
            "# Orphan overwrite\n");
        var result = await ExecuteAsync(
            workspace,
            [".agents/projects/orphan.overwrite.md"],
            Content("metadata"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.OverwriteAmbiguous);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ContextFindingCode.InvalidSource);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context exact-path selection retains a safe automatic-ID collision as attention"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ExactPathIdentityCollisionIsAttention()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.CreateDirectory(".agents/projects/guide");
        workspace.WriteText(
            ".agents/projects/guide/_guide.md",
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context resolves an exact local target case mismatch as safe attention"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ExactTargetCaseMismatchIsAttention()
    {
        using var workspace = ContextOperationWorkspace.Create(
            linkedTargetPath: ".agents/projects/Linked.md");
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Context unavailable authored frontmatter forms an incomplete projection finding"),
     Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnavailableFrontmatterIsIncomplete()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.ReplaceText(
            ".agents/projects/guide.md",
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
            suppliedDetail: null,
            effectiveView: CliDetail.Standard);

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
