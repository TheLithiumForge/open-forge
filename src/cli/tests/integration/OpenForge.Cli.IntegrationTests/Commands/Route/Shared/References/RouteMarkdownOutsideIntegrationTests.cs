using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Commands.Route.Shared.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.References;

[Trait("Boundary", "OS"), Trait("Feature", "route-markdown-catalogue"), Trait("Evidence", "Integration")]
public sealed class RouteMarkdownOutsideIntegrationTests
{
    private const string Parent = "docs";
    private const string Route = Parent + "/team";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string Body = "# Authored note\n";

    [Theory(DisplayName = "Registered content remains readable through ignored ancestors without admitting case aliases or unrelated siblings")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task InventoryScopeUsesDeclaredCasing(string mainRoot)
    {
        using var temporary = TemporaryWorkspace.Create("outside-markdown-scope");
        var layout = Layout(temporary, mainRoot);
        var note = Route + "/nested/" + Note;
        temporary.CreateFile(note, Body);
        var gitNote = Route + "/" + WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + Note;
        temporary.CreateFile(gitNote, Body);
        temporary.CreateFile(Parent + "/other/" + Note, Body);
        temporary.CreateFile(WorkspaceLayoutNames.GitMetadataDirectoryName + "/" + Note, Body);
        temporary.CreateFile(WorkspaceRuntimePaths.DirectoryName + "/" + Note, Body);
        var before = temporary.SnapshotHashes();
        var reader = new RouteMarkdownCatalogueReader(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult<ImmutableArray<string>?>([Parent, Route.ToUpperInvariant()]));
        var result = await reader.ReadAsync(new(layout, [Parent], [], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Complete, result.Coverage);
        Assert.Equal(note, Assert.Single(result.SelectedPaths));
        var alias = await reader.ReadAsync(new(layout, [Route.ToUpperInvariant()], [], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Empty(alias.SelectedPaths);
        Assert.True(alias.IgnoredPathCount > 0);
        var excluded = await reader.ReadAsync(new(layout, [Route], [Route + "/nested"], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Empty(excluded.SelectedPaths);
        var git = await reader.ReadAsync(new(layout, [gitNote], [], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Empty(git.SelectedPaths);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "A Markdown file operand reads only that file and ordinary traversal refuses links and runtime content")]
    public async Task FileScopeAndPhysicalSafety()
    {
        using var temporary = TemporaryWorkspace.Create("outside-markdown-file");
        using var foreign = TemporaryWorkspace.Create("outside-markdown-foreign");
        var layout = Layout(temporary, WorkspaceLayout.DefaultMainRoot);
        var note = Route + "/" + Note;
        temporary.CreateFile(note, Body);
        temporary.CreateFile(Route + "/sibling" + SourceFormClassifier.MarkdownExtension, Body);
        foreign.CreateFile(Note, Body);
        temporary.CreateDirectorySymbolicLink(Route + "/escape", foreign.Path);
        var reader = new RouteMarkdownCatalogueReader(new PhysicalPathResolver());
        var result = await reader.ReadAsync(new(layout, [note], [], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Equal(note, Assert.Single(result.SelectedPaths));
        result = await reader.ReadAsync(new(layout, [Route], [], new([SourceFormClassifier.MarkdownExtension])), TestContext.Current.CancellationToken);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Blocked, result.Coverage);
        Assert.Contains(result.Findings, finding => finding.Path == Route + "/escape" && finding.Code == RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe);
        Assert.DoesNotContain(Route + "/escape/" + Note, result.SelectedPaths);
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        result = await reader.ReadAsync(new(layout, [Route], [], new([SourceFormClassifier.MarkdownExtension])), cancelled.Token);
        Assert.Equal(RouteMarkdownCatalogueCoverage.Interrupted, result.Coverage);
        Assert.False(Directory.Exists(temporary.Combine(WorkspaceRuntimePaths.DirectoryName)));
    }

    private static WorkspaceLayout Layout(TemporaryWorkspace temporary, string mainRoot)
        => WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot).WithOutsideRoutes([Route]);
}
