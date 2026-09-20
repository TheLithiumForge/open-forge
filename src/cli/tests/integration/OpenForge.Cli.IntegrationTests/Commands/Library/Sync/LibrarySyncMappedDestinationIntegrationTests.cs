using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Integration")]
public sealed class LibrarySyncMappedDestinationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact]
    public async Task MixedSyncQuestionDisclosesAllDestinations()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("new.md");
        workspace.Source("kept.md");
        workspace.RecordAt("docs", "kept.md", "old.md");
        workspace.Link("docs/kept.md", "../shared/team-knowledge/kept.md");
        workspace.Link("docs/old.md", "../shared/team-knowledge/old.md");
        using var input = new StringReader("no\n");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains("docs   (directory: everything under it)", output.ToString(), StringComparison.Ordinal);
        Assert.Equal("Library sync was cancelled. Nothing was changed.", Assert.Single(result.Result.Findings).Cause);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task RememberedDirectoryCoversNewDescendantAndRetirementWithoutTouchingContent()
    {
        using var workspace = new LibraryMutationWorkspace();
        LibraryMutationApplicationData.FrameworkFile(workspace);
        workspace.Source("future/new.md");
        workspace.RecordAt("docs", "old.md");
        workspace.Link("docs/old.md", "../shared/team-knowledge/old.md");
        workspace.Write(".agents/open-forge.json", """
            {"allowInstallPaths":["docs"]}
            """);
        var permissionBefore = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));

        try
        {
            var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Null(new FileInfo(workspace.Absolute("docs/old.md")).LinkTarget);
            Assert.Equal("../../shared/team-knowledge/future/new.md", new FileInfo(workspace.Absolute("docs/future/new.md")).LinkTarget);
            Assert.Null(new DirectoryInfo(workspace.Absolute("docs/future")).LinkTarget);
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/future/new.md")));
            Assert.Equal(permissionBefore, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
            Assert.Equal("granted", result.Result.Permissions.Decision);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
        }
        finally
        {
            if (new FileInfo(workspace.Absolute("docs/future/new.md")).LinkTarget == "../../shared/team-knowledge/future/new.md")
            {
                File.Delete(workspace.Absolute("docs/future/new.md"));
            }
            var parent = workspace.Absolute("docs/future");
            if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task UncoveredSiblingBlocksWholeSyncAndProposesOnlyItsParent()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("approved/a.md");
        workspace.Source("new/b.md");
        workspace.RecordAt("docs", "approved/a.md");
        workspace.Link("docs/approved/a.md", "../../shared/team-knowledge/approved/a.md");
        workspace.Write(".agents/open-forge.json", """
            {"allowInstallPaths":["docs/approved"]}
            """);
        var before = workspace.Snapshot();

        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal("required", result.Result.Permissions.Decision);
        Assert.Equal("docs/new/b.md", Assert.Single(result.Result.Permissions.Missing));
        Assert.Equal("docs/new", Assert.Single(result.Result.Permissions.ProposedScopes).Path);
        Assert.Equal(before, workspace.Snapshot());
    }
}
