using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "install-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallLinkScopeIntegrationTests
{
    [Fact(DisplayName = "Install without the archive does not seed its exclusion and Update keeps authored settings unchanged")]
    public async Task OmittedArchiveAndUpdatePreserveChoice()
    {
        using var workspace = InstallOperationWorkspace.Create("install-without-archive");
        workspace.WriteText(".agents/open-forge.json", "{\"removedDirectories\":[\".agents/memory/archived\"],\"future\":true}");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        Assert.False(workspace.Exists(".agents/memory/archived/_archived.md"));
        var before = await File.ReadAllBytesAsync(workspace.Combine(".agents/open-forge.json"), TestContext.Current.CancellationToken);
        using var settings = JsonDocument.Parse(before);
        Assert.False(settings.RootElement.TryGetProperty("ignoreLinksFrom", out _));
        var update = await CliHostCapture.RunAsync(["update", "--automatic"], workspace.Workspace.LexicalRoot);
        Assert.True(update.ExitCode == 0, update.Output + update.Error);
        Assert.Equal(before, await File.ReadAllBytesAsync(workspace.Combine(".agents/open-forge.json"), TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Install seeds archive link scope only when the authored key is absent")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SeedsAbsentChoice(bool emptyChoice)
    {
        using var workspace = InstallOperationWorkspace.Create("install-link-scope");
        var before = emptyChoice ? "{\"ignoreLinksFrom\":[],\"future\":{\"keep\":7}}" : "{\"future\":{\"keep\":7}}";
        workspace.WriteText(".agents/open-forge.json", before);
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var snapshot = workspace.SnapshotHashes();
        var preview = await operation.ExecuteAsync(workspace.Request(mode: InstallMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(snapshot, workspace.SnapshotHashes());
        var result = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        using var json = JsonDocument.Parse(await File.ReadAllBytesAsync(workspace.Combine(".agents/open-forge.json"), TestContext.Current.CancellationToken));
        Assert.Equal(emptyChoice ? [] : new[] { ".agents/memory/archived" }, json.RootElement.GetProperty("ignoreLinksFrom").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(7, json.RootElement.GetProperty("future").GetProperty("keep").GetInt32());
        Assert.True(workspace.Exists(".agents/memory/archived/_archived.md"));
    }
}
