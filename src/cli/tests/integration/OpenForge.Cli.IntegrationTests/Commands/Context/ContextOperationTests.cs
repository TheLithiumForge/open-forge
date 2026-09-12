using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextOperationTests
{
    [Fact(DisplayName = "Context resolves the startup-required closure in exact loading and continuity order"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ResolvesStartupClosureInExactOrder()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(workspace, [], Content("metadata"));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Selection.StartupIncluded);
        Assert.False(result.Selection.AdditionsOnly);
        Assert.Equal(4, result.Selection.SourceCount);
        Assert.Equal(
        [
            "AGENTS.md",
            ".agents/loader.md",
            ".agents/docs/_docs.md",
            ".agents/docs/topic.md",
        ], result.Sources.Select(source => source.Path));
        Assert.Empty(result.Findings);
        Assert.Null(result.Next);
    }

    [Theory(DisplayName = "Context reads ordinary continuity only through loaded or selected parents"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task ContinuityUsesParentLoadingBoundary(bool loadParentAtStartup)
    {
        using var workspace = ContextOperationWorkspace.Create();
        if (loadParentAtStartup)
        {
            workspace.ReplaceText(".agents/loader.md", workspace.ReadText(".agents/loader.md")
                .Replace("#Memory", "#KeepInMind #Memory", StringComparison.Ordinal));
        }

        workspace.WriteText(".agents/state/checkpoint.overwrite.md", "# Continuity overwrite\n");
        var result = await ExecuteAsync(workspace, loadParentAtStartup ? [] : ["state"], Content("metadata"), additionsOnly: !loadParentAtStartup);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var checkpoint = Assert.Single(result.Sources, source => source.Id == "state/checkpoint");
        var reason = Assert.Single(checkpoint.InclusionReasons);
        Assert.Equal(ContextInclusionReasonKind.KeepInMind, reason.Kind);
        Assert.Equal(".agents/state/_state.md", reason.Source?.Path);
        Assert.Equal([ContextSourceLayerKind.Base, ContextSourceLayerKind.Overwrite], checkpoint.Layers.Select(layer => layer.Kind));
        Assert.DoesNotContain(result.Sources, source => source.Path.StartsWith(".agents/projects/", StringComparison.Ordinal));
        Assert.Equal([".agents/state/_state.md", ".agents/state/checkpoint.md"],
            result.Sources.Where(source => source.Path.StartsWith(".agents/state/", StringComparison.Ordinal)).Select(source => source.Path));
    }

    [Theory(DisplayName = "Context preserves both loading reasons and emits each exposed file once"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task DualLoadingTagsPreserveReasonsWithoutDuplicateContent(bool selectScope)
    {
        using var workspace = ContextOperationWorkspace.Create();
        const string scope = ".agents/state/_state.md";
        const string record = ".agents/state/checkpoint.md";
        workspace.ReplaceText(scope, workspace.ReadText(scope).Replace("#KeepInMind", "#LoadNow #KeepInMind", StringComparison.Ordinal));
        workspace.ReplaceText(record, workspace.ReadText(record).Replace("KeepInMind, Memory", "LoadNow, KeepInMind, Memory", StringComparison.Ordinal));
        if (!selectScope)
        {
            workspace.ReplaceText(".agents/loader.md", workspace.ReadText(".agents/loader.md")
                .Replace("#Memory", "#LoadNow #Memory", StringComparison.Ordinal));
        }

        var result = await ExecuteAsync(workspace, selectScope ? ["state"] : [], Content("metadata"));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var checkpoint = Assert.Single(result.Sources, source => source.Path == record);
        Assert.Equal([ContextInclusionReasonKind.LoadNow, ContextInclusionReasonKind.KeepInMind], checkpoint.InclusionReasons.Select(reason => reason.Kind));
        Assert.All(checkpoint.InclusionReasons, reason => Assert.Equal(scope, reason.Source?.Path));
    }

    [Fact(DisplayName = "Context additions-only subtracts startup while retaining explicit closure order and overwrite layering"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task AdditionsOnlySubtractsStartupAndRetainsLayers()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(result.Selection.StartupIncluded);
        Assert.True(result.Selection.AdditionsOnly);
        Assert.Equal(2, result.Selection.SourceCount);
        Assert.Equal(
            [".agents/projects/_projects.md", ".agents/projects/guide.md"],
            result.Sources.Select(source => source.Path));
        var guide = result.Sources[1];
        Assert.Equal(
            [ContextSourceLayerKind.Base, ContextSourceLayerKind.Overwrite],
            guide.Layers.Select(layer => layer.Kind));
        Assert.Equal(
            [".agents/projects/guide.md", ".agents/projects/guide.overwrite.md"],
            guide.Layers.Select(layer => layer.Path));
        Assert.Equal(
            [ContextInclusionReasonKind.SelectedSource],
            guide.InclusionReasons.Select(reason => reason.Kind));
    }

    [Fact(DisplayName = "Context traverses visible LoadNow from every entrypoint in a selected ancestor chain"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task SelectedAncestorChainExposesLoadNowDescendants()
    {
        using var workspace = ContextOperationWorkspace.Create(includeSelectedAncestorLoadNow: true);
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(
            [
                ".agents/projects/_projects.md",
                ".agents/projects/guide.md",
                ".agents/projects/ancestor-load.md",
            ],
            result.Sources.Select(source => source.Path));
        var reason = Assert.Single(result.Sources[2].InclusionReasons);
        Assert.Equal(ContextInclusionReasonKind.LoadNow, reason.Kind);
        Assert.Equal(".agents/projects/_projects.md", reason.Source?.Path);
        Assert.Equal("projects/guide", reason.Reference);
    }

    [Fact(DisplayName = "Context projects exact authored text and structural headings and sections in canonical layer order"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ProjectsExactAuthoredContentInCanonicalOrder()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("section:Rules", "body", "headings", "frontmatter"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Contains(result.Findings, finding =>
            finding.Code == ContextFindingCode.SectionMissing
            && finding.Source?.Path == ".agents/projects/_projects.md");
        var guide = Assert.Single(result.Sources, source => source.Id == "projects/guide");
        var baseLayer = guide.Layers[0];
        Assert.Equal(
            [ContextProjectionPart.Frontmatter, ContextProjectionPart.Headings, ContextProjectionPart.Body, ContextProjectionPart.Section],
            baseLayer.Projections.Select(projection => projection.Part));
        Assert.Equal(ContextOperationWorkspace.GuideFrontmatter.TrimEnd('\n'), baseLayer.Projections[0].Text);
        Assert.Equal(
            [("Guide", 1, ContextHeadingForm.Atx), ("Rules", 2, ContextHeadingForm.Atx)],
            baseLayer.Projections[1].Headings.Select(heading => (heading.Text, heading.Level, heading.Form)));
        Assert.Equal(ContextOperationWorkspace.GuideBody, baseLayer.Projections[2].Text);
        Assert.Equal("## Rules\n\nBase rule.\n\n[Linked](linked.md#details) and [External](https://example.com).\n", baseLayer.Projections[3].Text);
        Assert.All(baseLayer.Projections, projection => Assert.Equal(ContextProjectionState.Available, projection.State));
    }

    [Fact(DisplayName = "Context preserves Setext heading form in structural projection"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task SetextHeadingsRetainExactForm()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.ReplaceText(
            ".agents/projects/guide.md",
            $"{ContextOperationWorkspace.GuideFrontmatter}\nGuide\n=====\n\nRules\n-----\n\nSetext body.\n");

        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("headings"),
            additionsOnly: true);

        var layer = Assert.Single(result.Sources, source => source.Id == "projects/guide").Layers[0];
        Assert.Equal(
            [("Guide", 1, ContextHeadingForm.Setext), ("Rules", 2, ContextHeadingForm.Setext)],
            Assert.Single(layer.Projections).Headings.Select(heading => (heading.Text, heading.Level, heading.Form)));
    }

    [Fact(DisplayName = "Context distinguishes duplicate ambiguous sections from matching base and overwrite sections"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task SectionProjectionRetainsLayerMeaningAndDuplicateAmbiguity()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var complete = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("section:Rules"),
            additionsOnly: true);

        var guide = Assert.Single(complete.Sources, source => source.Id == "projects/guide");
        Assert.Equal(2, guide.Layers.Count);
        Assert.All(guide.Layers, layer =>
        {
            var projection = Assert.Single(layer.Projections);
            Assert.Equal(ContextProjectionPart.Section, projection.Part);
            Assert.Equal(ContextProjectionState.Available, projection.State);
            Assert.Contains("## Rules", projection.Text, StringComparison.Ordinal);
        });

        workspace.ReplaceText(
            ".agents/projects/guide.md",
            $"{ContextOperationWorkspace.GuideFrontmatter}\n# Guide\n\n## Rules\n\nFirst.\n\n## Rules\n\nSecond.\n");
        var ambiguous = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("section:Rules"),
            additionsOnly: true);

        Assert.Equal(CliSemanticStatus.Incomplete, ambiguous.Status);
        Assert.Contains(ambiguous.Findings, finding =>
            finding.Code == ContextFindingCode.SectionAmbiguous
            && finding.Path == ".agents/projects/guide.md");
        var ambiguousBase = Assert.Single(ambiguous.Sources, source => source.Id == "projects/guide").Layers[0];
        Assert.Equal(ContextProjectionState.Ambiguous, Assert.Single(ambiguousBase.Projections).State);
    }

    [Theory(DisplayName = "Context classifies section absence across each complete logical source"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(true, false, 1, 1, false)]
    [InlineData(false, true, 1, 1, false)]
    [InlineData(true, true, 2, 0, false)]
    [InlineData(false, false, 0, 2, true)]
    public async Task SectionAbsenceUsesLogicalSourceCoverage(
        bool baseContainsSection,
        bool overwriteContainsSection,
        int expectedAvailable,
        int expectedMissing,
        bool expectedFinding)
    {
        using var workspace = ContextOperationWorkspace.Create();
        const string projects = ".agents/projects/_projects.md";
        workspace.ReplaceText(
            projects,
            workspace.ReadText(projects).Replace(
                "\n## Entries",
                "\n## Rules\n\nParent rule.\n\n## Entries",
                StringComparison.Ordinal));
        var baseBody = baseContainsSection
            ? "\n## Rules\n\nBase rule.\n"
            : "\nBase body.\n";
        workspace.ReplaceText(
            ".agents/projects/guide.md",
            $"{ContextOperationWorkspace.GuideFrontmatter}\n# Guide\n{baseBody}");
        var overwriteBody = overwriteContainsSection
            ? "\n## Rules\n\nOverwrite rule.\n"
            : "\nOverwrite body.\n";
        workspace.ReplaceText(
            ".agents/projects/guide.overwrite.md",
            $"---\nopen-forge:\n  description: Guide overwrite\n  tags: [Guide]\n---\n\n# Guide overwrite\n{overwriteBody}");

        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("section:Rules"),
            additionsOnly: true);

        var expectedStatus = expectedFinding ? CliSemanticStatus.Attention : CliSemanticStatus.Complete;
        Assert.True(
            result.Status == expectedStatus,
            string.Join(" | ", result.Findings.Select(finding => $"{finding.Code}: {finding.Path}: {finding.Cause}")));
        var guide = Assert.Single(result.Sources, source => source.Id == "projects/guide");
        var projections = guide.Layers.SelectMany(layer => layer.Projections).ToArray();
        Assert.Equal(expectedAvailable, projections.Count(projection => projection.State == ContextProjectionState.Available));
        Assert.Equal(expectedMissing, projections.Count(projection => projection.State == ContextProjectionState.Missing));
        Assert.Equal(
            expectedFinding,
            result.Findings.Any(finding =>
                finding.Code == ContextFindingCode.SectionMissing
                && finding.Source?.Path == ".agents/projects/guide.md"));
    }

    [Fact(DisplayName = "Context follows links breadth-first with cycles, external observations, and additions set subtraction"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task FollowsLinksBreadthFirstWithoutFetchingOrRepeatingSources()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.All);

        Assert.True(
            result.Status == CliSemanticStatus.Complete,
            string.Join(" | ", result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(
            [".agents/projects/_projects.md", ".agents/projects/guide.md", ".agents/projects/linked.md"],
            result.Sources.Select(source => source.Path));
        Assert.Equal([1, 1, 2], result.Links.Select(link => link.Depth));
        Assert.Equal(
            [ContextLinkDisposition.Selected, ContextLinkDisposition.ExternalUnchecked, ContextLinkDisposition.Cycle],
            result.Links.Select(link => link.Disposition));
        Assert.Equal(ContextLinkNetwork.NetworkNotAttempted, result.Links[1].Target.Network);
        Assert.Empty(result.Findings);
    }

    [Fact(DisplayName = "Context reports a missing fragment without selecting the destination"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MissingFragmentIsIncompleteAndUnresolved()
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.ReplaceText(
            ".agents/projects/guide.md",
            $"{ContextOperationWorkspace.GuideFrontmatter}\n# Guide\n\n[Missing fragment](linked.md#absent).\n");

        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.All);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/projects/linked.md");
        var link = Assert.Single(result.Links);
        Assert.Equal(ContextLinkResolution.FragmentMissing, link.Target.Resolution);
        Assert.Equal(ContextLinkDisposition.Unresolved, link.Disposition);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.FragmentMissing);
    }

    [Theory(DisplayName = "Context applies bounded breadth-first traversal at depth two and higher"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(2, 4)]
    [InlineData(3, 5)]
    public async Task BoundedTraversalHonorsDepthTwoAndHigher(int depth, int expectedSourceCount)
    {
        using var workspace = ContextOperationWorkspace.Create();
        workspace.ReplaceText(
            ".agents/projects/linked.md",
            "---\nopen-forge:\n  description: Linked\n  tags: [Guide]\n---\n\n# Linked\n\n## Details\n\n[Deep](deep.md).\n");
        workspace.WriteText(
            ".agents/projects/deep.md",
            "---\nopen-forge:\n  description: Deep\n  tags: [Guide]\n---\n\n# Deep\n\n[Deeper](deeper.md).\n");
        workspace.WriteText(
            ".agents/projects/deeper.md",
            "---\nopen-forge:\n  description: Deeper\n  tags: [Guide]\n---\n\n# Deeper\n");

        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.Bounded(depth));

        Assert.True(
            result.Status == CliSemanticStatus.Complete,
            string.Join(" | ", result.Findings.Select(finding => $"{finding.Code}: {finding.Path}: {finding.Cause}")));
        Assert.Equal(expectedSourceCount, result.Sources.Count);
        Assert.Contains(result.Sources, source => source.Path == ".agents/projects/deep.md");
        Assert.Equal(depth == 3, result.Sources.Any(source => source.Path == ".agents/projects/deeper.md"));
        Assert.All(result.Links, link => Assert.InRange(link.Depth, 1, depth));
    }

    [Fact(DisplayName = "Context additions subtracts the startup closure after equal-depth link expansion"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task AdditionsSubtractExpandedStartupSources()
    {
        using var workspace = ContextOperationWorkspace.Create(startupLinksToSelectedTarget: true);
        var result = await ExecuteAsync(
            workspace,
            ["projects/guide"],
            Content("metadata"),
            additionsOnly: true,
            linkExpansion: ContextLinkExpansion.Bounded(1));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(
            [".agents/projects/_projects.md", ".agents/projects/guide.md"],
            result.Sources.Select(source => source.Path));
        Assert.DoesNotContain(result.Sources, source => source.Path == ".agents/projects/linked.md");
        Assert.Equal(ContextOptionalCoverageState.Complete, result.Coverage.Links);
    }

    private static ValueTask<ContextResult> ExecuteAsync(
        ContextOperationWorkspace workspace,
        IEnumerable<string> sources,
        ContextContentSelection content,
        bool additionsOnly = false,
        ContextLinkExpansion? linkExpansion = null)
        => ContextOperationFactory.Create().ExecuteAsync(
            new ContextRequest(
                workspace: workspace.Workspace,
                sourceReferences: sources,
                additionsOnly: additionsOnly,
                content: content,
                linkExpansion: linkExpansion ?? ContextLinkExpansion.None,
                suppliedView: null,
                effectiveView: CliView.Expanded),
            CancellationToken.None);

    private static ContextContentSelection Content(params string[] values)
    {
        var parts = values.Select(value => value switch
        {
            "metadata" => new ContextContentPart(
                kind: ContextContentPartKind.Metadata,
                name: null,
                canonicalValue: value),
            "frontmatter" => new ContextContentPart(
                kind: ContextContentPartKind.Frontmatter,
                name: null,
                canonicalValue: value),
            "headings" => new ContextContentPart(
                kind: ContextContentPartKind.Headings,
                name: null,
                canonicalValue: value),
            "body" => new ContextContentPart(
                kind: ContextContentPartKind.Body,
                name: null,
                canonicalValue: value),
            _ when value.StartsWith("section:", StringComparison.Ordinal) => new ContextContentPart(
                kind: ContextContentPartKind.Section,
                name: value["section:".Length..],
                canonicalValue: value),
            _ => throw new ArgumentOutOfRangeException(nameof(values), value, "The Context test content part is not defined."),
        }).ToArray();
        var effective = parts
            .OrderBy(part => part.Kind)
            .ToArray();
        return new ContextContentSelection(supplied: parts, effective: effective);
    }
}
