using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;
using OpenForge.Cli.IntegrationTests.Commands.Update;

namespace OpenForge.Cli.IntegrationTests.Framework.Ownership;

public sealed class FrameworkOwnershipWriterIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install records root managed blocks as regions and preserves authored host content")]
    public async Task InstallRecordsManagedRegions()
    {
        using var workspace = InstallOperationWorkspace.Create("ownership-install-root-regions");
        const string authored = "# Personal instructions\n\nKeep this authored section.\n";
        workspace.WriteText("AGENTS.md", authored);
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Unavailable(),
            workspace.LockStoreRoot);

        var result = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.StartsWith(authored, await workspace.ReadTextAsync("AGENTS.md", TestContext.Current.CancellationToken));
        AssertRootRegions(await workspace.ReadTextAsync(InstallOperationWorkspace.OwnershipPath, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install re-establishment retains whole-file receipts when their bytes need no write")]
    public async Task ReestablishmentRetainsUnchangedWholeFiles()
    {
        using var workspace = InstallOperationWorkspace.Create("ownership-install-retained-paths");
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Unavailable(),
            workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete,
            (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var before = await workspace.ReadTextAsync(InstallOperationWorkspace.OwnershipPath, TestContext.Current.CancellationToken);
        using var previous = JsonDocument.Parse(before);
        var paths = previous.RootElement.GetProperty("framework").GetProperty("paths")
            .EnumerateArray().Select(path => path.GetString()).ToArray();
        Assert.NotEmpty(paths);
        File.Delete(workspace.Combine(".agents/open-forge.lifecycle.json"));

        var result = await operation.ExecuteAsync(workspace.Request(force: true), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var after = await workspace.ReadTextAsync(InstallOperationWorkspace.OwnershipPath, TestContext.Current.CancellationToken);
        using var current = JsonDocument.Parse(after);
        Assert.Equal(paths, current.RootElement.GetProperty("framework").GetProperty("paths")
            .EnumerateArray().Select(path => path.GetString()).ToArray());
        AssertRootRegions(after);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update preserves root region ownership without claiming authored host files")]
    public async Task UpdatePreservesManagedRegions()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("ownership-update-root-regions");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        AssertRootRegions(workspace.ReadText(InstallOperationWorkspace.OwnershipPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Init preserves root region ownership without claiming authored host files")]
    public async Task RouteInitPreservesManagedRegions()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "ownership-route-init-root-regions", TestContext.Current.CancellationToken);

        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        AssertRootRegions(workspace.ReadText(RouteInitFrameworkIntegrationWorkspace.OwnershipPath));
    }

    private static void AssertRootRegions(string json)
    {
        using var document = JsonDocument.Parse(json);
        var framework = document.RootElement.GetProperty("framework");
        foreach (var host in new[] { "AGENTS.md", "CLAUDE.md" })
        {
            Assert.DoesNotContain(framework.GetProperty("paths").EnumerateArray(), path => path.GetString() == host);
            Assert.Contains(framework.GetProperty("regions").EnumerateArray(), region =>
                region.GetProperty("path").GetString() == host
                && region.GetProperty("region").GetString() == "open-forge");
        }
    }
}
