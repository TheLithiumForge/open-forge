using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Presentation.References.Models;
using OpenForge.Cli.Core.Commands.References;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Boundary", "Host"), Trait("Feature", ReferencesDefinitions.CommandIdentity), Trait("Evidence", "Integration")]
public sealed class ReferencesOutsideSourcesIntegrationTests
{
    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ReferencesOutsideWorkspace.CustomRoot)]
    public async Task IncomingRegistrationAddsInventoryWithoutCountingEntriesOrOpaqueLinks(string mainRoot)
    {
        using var workspace = new ReferencesOutsideWorkspace(mainRoot);
        var before = await Read(workspace, ReferencesOutsideWorkspace.Target, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.In);
        Assert.Equal(new[] { ReferencesOutsideWorkspace.Leaf, ReferencesOutsideWorkspace.Leaf }, IncomingPaths(before));
        Assert.DoesNotContain(before.GetProperty(PublishedTokens.ReferencesData.Scanned).EnumerateArray(), row => row.GetProperty(PublishedTokens.Json.Path).GetString() == ReferencesOutsideWorkspace.Unlisted);
        workspace.Register();
        var bytes = File.ReadAllBytes(workspace.Files.Combine(ReferencesOutsideWorkspace.Leaf));
        var hashes = workspace.Files.SnapshotHashes(includeRuntime: true);
        var after = await Read(workspace, ReferencesOutsideWorkspace.Target, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.In);
        Assert.Equal(new[] { ReferencesOutsideWorkspace.Leaf, ReferencesOutsideWorkspace.Leaf, ReferencesOutsideWorkspace.Unlisted }, IncomingPaths(after));
        Assert.All(after.GetProperty(PublishedTokens.ReferencesData.Incoming).EnumerateArray(), row => Assert.Equal(
            SourceWorkspaceRelativePath.LiteralReferencePrefix + row.GetProperty(PublishedTokens.Json.Path).GetString(), row.GetProperty(PublishedTokens.Json.Id).GetString()));
        Assert.DoesNotContain(ReferencesOutsideWorkspace.Opaque, IncomingPaths(after));
        Assert.DoesNotContain(ReferencesOutsideWorkspace.Anchor, IncomingPaths(after));
        Assert.DoesNotContain(workspace.Layout.LoaderPath, IncomingPaths(after));
        Assert.Equal(bytes, File.ReadAllBytes(workspace.Files.Combine(ReferencesOutsideWorkspace.Leaf)));
        Assert.Equal(hashes, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(ReferencesOutsideWorkspace.CustomRoot)]
    public async Task ExactOutsideOutgoingKeepsAuthoredOccurrencesAndDirectionWhileFiltersBoundIncoming(string mainRoot)
    {
        using var workspace = new ReferencesOutsideWorkspace(mainRoot);
        var before = await Read(workspace, ReferencesOutsideWorkspace.Leaf, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out);
        AssertOutgoing(before);
        workspace.Register();
        var hashes = workspace.Files.SnapshotHashes(includeRuntime: true);
        var after = await Read(workspace, ReferencesOutsideWorkspace.Leaf, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out);
        AssertOutgoing(after);
        Assert.Equal(before.GetRawText(), after.GetRawText());
        Assert.False(after.TryGetProperty(PublishedTokens.ReferencesData.Incoming, out _));
        var filtered = await Read(workspace, ReferencesOutsideWorkspace.Target, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.In, ReferencesDefinitions.Include.Name,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + ReferencesOutsideWorkspace.Anchor,
            ReferencesDefinitions.Exclude.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + ReferencesOutsideWorkspace.Unlisted);
        Assert.Equal(new[] { ReferencesOutsideWorkspace.Leaf, ReferencesOutsideWorkspace.Leaf }, IncomingPaths(filtered));
        var opaque = await Read(workspace, ReferencesOutsideWorkspace.Opaque, ReferencesDefinitions.Direction.Name, ReferencesDefinitions.Out);
        Assert.Empty(opaque.GetProperty(PublishedTokens.ReferencesData.Outgoing).EnumerateArray());
        Assert.Equal(hashes, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    private static void AssertOutgoing(JsonElement data)
    {
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + ReferencesOutsideWorkspace.Leaf,
            data.GetProperty(PublishedTokens.ReferencesData.Source).GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(new[] { ReferencesOutsideWorkspace.Destination, ReferencesOutsideWorkspace.Destination + "#details" },
            data.GetProperty(PublishedTokens.ReferencesData.Outgoing).EnumerateArray().Select(row => row.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(ReferencesDataOutgoing.Destination))).GetString()));
        Assert.All(data.GetProperty(PublishedTokens.ReferencesData.Outgoing).EnumerateArray(), row => Assert.Equal(ReferencesOutsideWorkspace.Target, row.GetProperty(PublishedTokens.ReferencesData.ResolvedPath).GetString()));
    }

    private static string?[] IncomingPaths(JsonElement data)
        => data.GetProperty(PublishedTokens.ReferencesData.Incoming).EnumerateArray().Select(row => row.GetProperty(PublishedTokens.Json.Path).GetString()).ToArray();

    private static async Task<JsonElement> Read(ReferencesOutsideWorkspace workspace, string source, params string[] arguments)
    {
        var preserved = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await CliHostCapture.RunAsync([ReferencesDefinitions.CommandIdentity, SourceWorkspaceRelativePath.LiteralReferencePrefix + source,
            .. arguments, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat, CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], workspace.Files.Path);
        Assert.True(result.ExitCode == 0, result.Output + result.Error);
        Assert.Empty(result.Error);
        Assert.Equal(preserved, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Layout.Workspace)));
        using var json = JsonDocument.Parse(result.Output);
        return json.RootElement.GetProperty(PublishedTokens.Json.Data).Clone();
    }
}
