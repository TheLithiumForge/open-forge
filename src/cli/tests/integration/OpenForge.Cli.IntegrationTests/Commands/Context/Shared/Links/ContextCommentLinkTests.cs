using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Comments;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Context.Shared.Links;

[Trait("Feature", "comment-links"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ContextCommentLinkTests
{
    [Fact(DisplayName = "Context requested expansion validates root and file-relative HTML comment fragments")]
    public async Task HtmlCommentExpansion()
    {
        using var workspace = new RoutedOutputWorkspace();
        var root = WorkspaceLayout.DefaultMainRoot;
        var guide = $"{root}/docs/guide.md";
        workspace.Replace(guide, $"# Guide\n\n<!-- {SourceCommentLinkReader.OpenForgeMarker} {root}/docs/reference.md#details reason -->\n<!-- {SourceCommentLinkReader.RuneMarker} ./reference.md#absent reason -->\n");
        var before = workspace.Snapshot();
        var result = await ContextOperationFactory.Create().ExecuteAsync(new(new CliWorkspace(workspace.Path, workspace.Path,
                CliWorkspaceSelectionMethod.ExplicitWorkspace), ["docs/guide"], false, new ContextContentSelection([], []),
            ContextLinkExpansion.Bounded(1), null, CliDetail.Minimal), TestContext.Current.CancellationToken);

        Assert.Contains(result.Links, link => link.Source.Path == guide && link.Target.Resolution == ContextLinkResolution.Complete);
        Assert.Contains(result.Findings, finding => finding.Code == ContextFindingCode.FragmentMissing);
        Assert.Equal(before, workspace.Snapshot());
    }
}
