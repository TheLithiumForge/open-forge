using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Integration")]
public sealed class LibraryAttachPermissionLifecycleIntegrationTests
{
    private const string PermissionPath = ".agents/open-forge.permissions.json";

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task ChangedPermissionOrInventoryCannotInheritReviewedApproval(bool changeInventory)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("docs/a.md");
        LibraryMutationApplicationData.Lifecycle(workspace);
        var answered = false;
        using var input = new ChangingInput(() =>
        {
            answered = true;
            if (changeInventory)
            {
                workspace.Source("docs/new.md");
            }
            else
            {
                workspace.Write(PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
            }
        });
        using var output = new StringWriter();
        var permission = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt: true));

        var result = await new LibraryAttachOperation(permission).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply) with { AllowPrompt = true }, TestContext.Current.CancellationToken);

        Assert.True(answered, "The permission question must be reached before the changed-fact assertion.");
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Null(new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget);
        Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
        if (changeInventory)
        {
            Assert.False(File.Exists(workspace.Absolute(PermissionPath)));
        }
        else
        {
            Assert.Equal("""{"schemaVersion":1,"extensions":[],"libraries":[]}""", File.ReadAllText(workspace.Absolute(PermissionPath)));
        }
    }

    [Fact]
    public async Task LinkFailureRetainsVerifiedGrantAndLeavesRecordUnpublished()
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This ordinary link failure uses Unix directory permissions.");
            return;
        }
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("docs/a.md");
        workspace.Directory("docs");
        LibraryMutationApplicationData.Lifecycle(workspace);
        var parent = workspace.Absolute("docs");
        var mode = File.GetUnixFileMode(parent);
        using var input = new ChangingInput(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(parent, UnixFileMode.UserRead | UnixFileMode.UserExecute);
            }
        });
        using var output = new StringWriter();
        var permission = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt: true));
        string? recovery = null;
        try
        {
            var result = await new LibraryAttachOperation(permission).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply) with { AllowPrompt = true }, TestContext.Current.CancellationToken);
            recovery = result.Result.Application.Recovery.Path;

            Assert.Equal(CliSemanticStatus.Failed, result.Status);
            Assert.Equal("approved", result.Result.Permissions.Decision);
            Assert.Equal("verified", result.Result.Permissions.Outcome);
            Assert.Null(new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget);
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            using var json = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(PermissionPath)));
            Assert.Equal("docs", Assert.Single(Assert.Single(json.RootElement.GetProperty("libraries").EnumerateArray())
                .GetProperty("directories").EnumerateArray()).GetString());
            Assert.NotNull(recovery);
            Assert.True(File.Exists(recovery));
            Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/docs/a.md")));
        }
        finally
        {
            File.SetUnixFileMode(parent, mode);
            File.Delete(workspace.Absolute(PermissionPath));
            if (recovery is not null && File.Exists(recovery))
            {
                File.Delete(recovery);
            }
        }
    }

    private sealed class ChangingInput(Action change) : StringReader("yes\n")
    {
        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            change();
            return base.ReadLineAsync(cancellationToken);
        }
    }
}
