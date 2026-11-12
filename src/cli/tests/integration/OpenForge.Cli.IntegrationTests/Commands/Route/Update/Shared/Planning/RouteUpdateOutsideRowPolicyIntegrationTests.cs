using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Shared.Planning;

[Trait("Feature", "route-update-outside"), Trait("Evidence", "IntegrationSafety"), Trait("Boundary", "OS")]
public sealed class RouteUpdateOutsideRowPolicyIntegrationTests
{
    private const string Foreign = "foreign/readonly.md";
    private const string TargetDestination = "note.md";
    private const string ForeignDestination = "../../../" + Foreign;
    private const string Rows = $"- [First independent label]({TargetDestination}) - #{SourceLoadingClosureResolver.LoadNowTag} #Fixture - applies to `src/**`\n"
        + $"- [Foreign independent label]({ForeignDestination}) - #Fixture - applies to `docs/**`\n"
        + $"- [Second independent label]({SourceWorkspaceRelativePath.LiteralReferencePrefix}{TargetDestination}) - #{SourceLoadingClosureResolver.KeepInMindTag} #Fixture - applies to `tests/**`";
    private const string MalformedRow = "- [Keep this label](note.md) - Independent row policy - #Fixture";

    [Theory(DisplayName = "Outside Update rebuilds repeated target rows from metadata, keeps leaving rows and leaves foreign inputs unchanged")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, null, true), InlineData(WorkspaceLayout.DefaultMainRoot, null, false)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, null, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, null, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName, true), InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName, false)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName, true), InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.UnderscoreIndexFileName, false)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreIndexFileName, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreIndexFileName, false)]
    public async Task RepeatedTargetRowsFollowMetadata(string root, string? compatibility, bool listed)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root, listed);
        if (compatibility is not null)
        {
            File.Delete(workspace.Files.Combine(workspace.Anchor));
            workspace.Files.WriteText(workspace.DirectoryPath + "/" + compatibility, RouteUpdateOutsideWorkspace.Entrypoint());
        }
        workspace.Files.WriteText(Foreign, "# Independently authored readable input\n\nNo complete routed metadata.\n");
        workspace.Files.ReplaceText(workspace.Parent, RouteUpdateOutsideWorkspace.Entrypoint(Rows).Replace("\r\n", "\n", StringComparison.Ordinal));
        var leaving = RowBytes(workspace, ForeignDestination);
        var parsed = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent)));
        Assert.Equal(SourceGeneratedEntriesState.Complete, parsed.State);
        Assert.Equal([TargetDestination, ForeignDestination, SourceWorkspaceRelativePath.LiteralReferencePrefix + TargetDestination],
            parsed.Entries.Select(entry => entry.Destination));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var preview = await workspace.Execute(workspace.Request(mode: RouteUpdateMode.DryRun));
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        var plan = await workspace.Plan();
        var retained = plan.Observation.Catalogue.IncludedRows.Where(row => row.ParentPath == workspace.Parent).ToArray();
        Assert.Equal(parsed.Entries.Select(entry => entry.Destination), retained.Select(row => row.Entry.Destination));
        Assert.Equal(parsed.Entries.Select(entry => entry.Description), retained.Select(row => row.Entry.Description));
        Assert.Equal(parsed.Entries.Select(entry => entry.ApplyTo.Single().Text), retained.Select(row => row.Entry.ApplyTo.Single().Text));
        Assert.DoesNotContain(plan.Navigation.ReadonlySnapshots, snapshot => snapshot.Expectation.LogicalPath == workspace.Files.Combine(Foreign));
        var applied = await workspace.Execute(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, applied.Verification);
        Assert.Equal(new[] { workspace.Parent, workspace.Target }.Order(StringComparer.Ordinal),
            applied.Effects.Select(effect => effect.Path).Order(StringComparer.Ordinal));
        var entries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent))).Entries;
        Assert.Equal([TargetDestination, ForeignDestination], entries.Select(entry => entry.Destination));
        Assert.Equal(RouteUpdateOutsideWorkspace.After, entries[0].Description);
        Assert.Equal(leaving, RowBytes(workspace, ForeignDestination));
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["updated-parent.content"] = workspace.Text(workspace.Parent),
        });
        Assert.Equal(RouteUpdateOutsideWorkspace.ChangedDocument(), workspace.Bytes(workspace.Target));
        workspace.AssertPreserved(before);
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        Assert.Empty((await workspace.Execute(workspace.Request())).Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    private static byte[] RowBytes(RouteUpdateOutsideWorkspace workspace, string destination)
    {
        var document = new MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent));
        var row = Assert.Single(SourceGeneratedEntriesParser.Parse(document).Entries, entry => entry.Destination == destination);
        return Encoding.UTF8.GetBytes(document.Source[row.Span.Start..row.Span.End]);
    }

    [Theory(DisplayName = "Outside Update refuses malformed parent row syntax without discarding independent authored bytes")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task MalformedRowsCannotBeSilentlyReplaced(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        workspace.Files.ReplaceText(workspace.Parent, RouteUpdateOutsideWorkspace.Entrypoint(MalformedRow).Replace("\r\n", "\n", StringComparison.Ordinal));
        var parsed = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent)));
        Assert.Equal(SourceGeneratedEntriesState.Unavailable, parsed.State);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var parent = Encoding.UTF8.GetBytes(workspace.Text(workspace.Parent));
        var result = await workspace.Execute(workspace.Request());
        Assert.Equal(parent, workspace.Bytes(workspace.Parent));
        Assert.Equal(Encoding.UTF8.GetBytes(RouteUpdateOutsideWorkspace.AuthoredDocument), workspace.Bytes(workspace.Target));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.GeneratedRegionUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }
}
