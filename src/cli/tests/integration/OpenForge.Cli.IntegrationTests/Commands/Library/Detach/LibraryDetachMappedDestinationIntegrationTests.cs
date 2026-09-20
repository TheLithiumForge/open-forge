using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Integration")]
public sealed class LibraryDetachMappedDestinationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task MissingSourceDoesNotBypassCurrentPermissionForExactMappedRemoval(bool granted)
    {
        using var workspace = new LibraryMutationWorkspace();
        LibraryMutationApplicationData.FrameworkFile(workspace);
        Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        workspace.RecordAt("docs", "gone.md");
        workspace.Link("docs/gone.md", "../shared/team-knowledge/gone.md");
        workspace.Write("docs/local.txt", "Unrelated consumer bytes.");
        if (granted)
        {
            workspace.Write(".agents/open-forge.json", """
                {"allowInstallPaths":["docs/gone.md"]}
                """);
        }
        var before = workspace.Snapshot();

        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(granted ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        Assert.True(result.Result.Identity.SourceIndependent);
        Assert.Equal("Unrelated consumer bytes.", File.ReadAllText(workspace.Absolute("docs/local.txt")));
        Assert.False(Directory.Exists(workspace.Absolute(LibraryMutationWorkspace.SourceRoot)));
        if (granted)
        {
            Assert.Null(new FileInfo(workspace.Absolute("docs/gone.md")).LinkTarget);
            using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            Assert.Empty(document.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.True(File.Exists(workspace.Absolute(".agents/open-forge.json")));
        }
        else
        {
            var proposed = Assert.Single(result.Result.Permissions.ProposedScopes);
            Assert.Equal("file", proposed.Kind);
            Assert.Equal("docs/gone.md", proposed.Path);
            Assert.Equal(before, workspace.Snapshot());
        }
    }
}
