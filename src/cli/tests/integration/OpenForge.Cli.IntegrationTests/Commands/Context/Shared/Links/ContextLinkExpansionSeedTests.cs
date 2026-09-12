using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Graph;
using OpenForge.Cli.Core.Commands.Context.Shared.Links;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Context.Shared.Links;

public sealed class ContextLinkExpansionSeedTests
{
    [Fact(DisplayName = "Context link expansion traverses a zero-reason seed without adding it to selected sources"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ZeroReasonSeedStillSuppliesTraversalLinks()
    {
        using var workspace = ContextOperationWorkspace.Create();
        var graph = await new ContextGraphBuilder().BuildAsync(workspace.Workspace, CancellationToken.None);
        var guide = Assert.IsType<ContextGraphSource>(graph.FindByPath(".agents/projects/guide.md"));
        var seed = new ContextSelectedGraphSource { Source = guide, InclusionReasons = [] };
        var closure = new ContextClosureResolution(
            requestedSources: [],
            startupSources: [seed],
            combinedSources: [seed],
            resultSources: [seed],
            findings: [],
            startupIncluded: true,
            selectionComplete: true,
            selectionBlocked: false);
        var request = new ContextRequest(
            workspace: workspace.Workspace,
            sourceReferences: [],
            additionsOnly: false,
            content: new ContextContentSelection([], []),
            linkExpansion: ContextLinkExpansion.Bounded(1),
            suppliedView: null,
            effectiveView: CliView.Compact);

        var expansion = await new ContextLinkExpander().ExpandAsync(request, graph, closure, CancellationToken.None);

        Assert.True(expansion.Complete);
        Assert.False(expansion.Blocked);
        Assert.Empty(expansion.Findings);
        Assert.Equal([".agents/projects/linked.md"], expansion.StartupSources.Select(row => row.Source.CanonicalPath));
        Assert.Equal([".agents/projects/linked.md"], expansion.CombinedSources.Select(row => row.Source.CanonicalPath));
        Assert.Equal([".agents/projects/linked.md"], expansion.ResultSources.Select(row => row.Source.CanonicalPath));
        var reason = Assert.Single(Assert.Single(expansion.ResultSources).InclusionReasons);
        Assert.Equal(ContextInclusionReasonKind.LinkedSource, reason.Kind);
        Assert.Equal(".agents/projects/guide.md", reason.Source?.Path);
        Assert.Equal(1, reason.Depth);
        Assert.Equal(2, expansion.Links.Count);
        Assert.Empty(seed.InclusionReasons);
        Assert.Same(guide, seed.Source);
    }
}
