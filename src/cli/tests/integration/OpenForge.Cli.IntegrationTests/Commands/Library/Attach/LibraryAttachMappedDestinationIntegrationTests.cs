using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

[Trait("Feature", "library-mapping"), Trait("Evidence", "Integration")]
public sealed class LibraryAttachMappedDestinationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(".", "README.md", "shared/team-knowledge/README.md")]
    [InlineData("docs", "docs/README.md", "../shared/team-knowledge/README.md")]
    [InlineData(".apm/agents/team", ".apm/agents/team/README.md", "../../../shared/team-knowledge/README.md")]
    public static async Task ApprovedMappingCreatesIndividualLeafAndRecordsBothRoots(
        string destinationRoot, string destination, string rawTarget)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        LibraryMutationApplicationData.FrameworkFile(workspace);
        using var input = new StringReader("always\nsentinel\n");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var request = workspace.Attach(LibraryMode.Apply) with
        {
            DestinationRoot = LibraryDestinationRoot.Create(destinationRoot),
            AllowPrompt = true,
            Automatic = false,
        };

        try
        {
            var result = await new LibraryAttachOperation(permissions, LibraryPermissionTestPrompt.AttachConfirmation()).ExecuteAsync(request, TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(rawTarget, new FileInfo(workspace.Absolute(destination)).LinkTarget);
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/README.md")));
            Assert.Equal("sentinel", input.ReadLine());
            Assert.Equal("approved", result.Result.Permissions.Decision);
            Assert.Equal("create", result.Result.Permissions.Action);
            Assert.Equal("verified", result.Result.Permissions.Outcome);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.True(result.Result.Application.RecordPublication.PublishedLast);
            using var json = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            var library = Assert.Single(json.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.Equal(destinationRoot, library.GetProperty("destinationRoot").GetString());
            Assert.Equal("shared/team-knowledge", library.GetProperty("sourceRoot").GetString());
            Assert.Equal("README.md", Assert.Single(library.GetProperty("paths").EnumerateArray()).GetString());
            if (destinationRoot != ".")
            {
                Assert.Null(new DirectoryInfo(workspace.Absolute(destinationRoot)).LinkTarget);
            }
        }
        finally
        {
            if (new FileInfo(workspace.Absolute(destination)).LinkTarget == rawTarget)
            {
                File.Delete(workspace.Absolute(destination));
            }
            File.Delete(workspace.Absolute(LibraryMutationWorkspace.RecordPath));
            File.Delete(workspace.Absolute(".agents/open-forge.json"));
            var parent = System.IO.Path.GetDirectoryName(workspace.Absolute(destination));
            while (parent is not null && parent != workspace.Path && Directory.Exists(parent)
                && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
                parent = System.IO.Path.GetDirectoryName(parent);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("shared/team-knowledge")]
    [InlineData("shared/other")]
    [InlineData(".git")]
    [InlineData(".agents/open-forge.json")]
    public static async Task ProtectedOrSourceDestinationBlocksBeforePermissionQuestion(string destinationRoot)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("a.md");
        workspace.Directory("shared/other");
        workspace.Write(LibraryMutationWorkspace.OwnershipPath, """
            {"schemaVersion":1,"libraries":[{"id":"other","sourceRoot":"shared/other","destinationRoot":"other-docs","paths":[]}]}
            """);
        using var input = new StringReader("always\n");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var request = workspace.Attach(LibraryMode.Apply) with
        {
            DestinationRoot = LibraryDestinationRoot.Create(destinationRoot),
            AllowPrompt = true,
        };

        var result = await new LibraryAttachOperation(permissions).ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal("not-evaluated", result.Result.Permissions.Decision);
        Assert.Equal("always", input.ReadLine());
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public static async Task SourceContainingManagementEffectsBlocksBeforePreviewOrPrompt(bool externalLeaf, bool dryRun)
    {
        using var workspace = new LibraryMutationWorkspace();
        if (externalLeaf)
        {
            workspace.Write(".agents/README.md", "Source bytes.");
        }
        using var input = new StringReader("always\n");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(permissions).ExecuteAsync(workspace.Attach(dryRun ? LibraryMode.DryRun : LibraryMode.Apply) with
        {
            SourceRoot = WorkspaceRelativeDirectory.Create(".agents"),
            DestinationRoot = LibraryDestinationRoot.Create("docs"),
            AllowPrompt = true,
        }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal("not-evaluated", result.Result.Permissions.Decision);
        Assert.Null(result.Result.Application.Recovery.Path);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal("always", input.ReadLine());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task DryRunCannotPromptOrRememberMissingExternalGrant()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        using var input = new StringReader("always\n");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(permissions).ExecuteAsync(
            workspace.Attach() with { AllowPrompt = true }, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal("required", result.Result.Permissions.Decision);
        Assert.Equal("always", input.ReadLine());
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }
}
