using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallOwnershipReaderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension commands select lock ownership when legacy package ownership disagrees")]
    [Trait("Feature", "extension-ownership"), Trait("Evidence", "Integration")]
    [InlineData("install")]
    [InlineData("update")]
    [InlineData("remove")]
    public async Task CommandsUseLockOwnershipAndPreserveUnselectedOwners(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-lock-readers");
        using var source = ExtensionInstallCatalogue.Create("extension-lock-readers-source");
        await workspace.SeedFrameworkAsync();
        source.AddPackage("alpha", [], (".agents/alpha.txt", "alpha bytes\n"));
        source.AddPackage("beta", [], (".agents/beta.txt", "beta bytes\n"));
        var installed = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, installed.ExitCode);
        const string leftover = "{invalid earlier state with no package ownership}";
        workspace.CreateOccupant(ExtensionInstallIntegrationWorkspace.LifecyclePath, leftover);
        string[] arguments = command == "remove"
            ? ["extension", "remove", "alpha", "--automatic", "--format", "json"]
            : command == "install"
                ? ["extension", command, "alpha", "--source", source.Path, "--automatic", "--format", "json", "--detail", "full"]
                : ["extension", command, "alpha", "--source", source.Path, "--automatic", "--format", "json"];

        var run = await workspace.RunAsync(arguments);

        Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
        Assert.Equal(leftover, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
            TestContext.Current.CancellationToken);
        Assert.Equal(["beta"], ownership.Document.OwnersOf(".agents/beta.txt"));
        Assert.Equal("beta bytes\n", workspace.ReadText(".agents/beta.txt"));
        Assert.Equal(command != "remove", File.Exists(workspace.Combine(".agents/alpha.txt")));
        Assert.Equal(command != "remove", ownership.Document.Extensions.Any(extension => extension.Id == "alpha"));
    }
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install uses current targets and skips unavailable lock publication without reading retired records")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("missing"), InlineData("malformed"), InlineData("directory")]
    public async Task UnknownLockDoesNotGateSafeNewInstallation(string condition)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-unknown-lock");
        using var source = ExtensionInstallCatalogue.Create("extension-install-unknown-lock-source");
        await workspace.SeedFrameworkAsync();
        const string leftover = "{ unrelated retired record }";
        workspace.CreateOccupant(ExtensionInstallIntegrationWorkspace.LifecyclePath, leftover);
        workspace.CreateOccupant(".agents/open-forge.libraries.json", leftover);
        workspace.ReplaceText(".agents/loader.md", workspace.ReadText(".agents/loader.md") + "\n## Workspace Notes\n\nAuthored workspace note.\n");
        var loader = workspace.ReadText(".agents/loader.md");
        var lockPath = workspace.Combine(ExtensionInstallIntegrationWorkspace.OwnershipPath);
        File.Delete(lockPath);
        if (condition == "malformed")
        {
            workspace.CreateOccupant(ExtensionInstallIntegrationWorkspace.OwnershipPath, "{");
        }
        else if (condition == "directory")
        {
            Directory.CreateDirectory(lockPath);
        }
        source.AddPackage("alpha", [], (".agents/alpha.txt", "alpha bytes\n"));

        var run = await workspace.RunAsync(["extension", "install", "alpha", "--source", source.Path, "--automatic", "--format", "json", "--detail", "full"]);

        Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
        Assert.Equal("alpha bytes\n", workspace.ReadText(".agents/alpha.txt"));
        Assert.Equal(loader, workspace.ReadText(".agents/loader.md"));
        Assert.Equal(leftover, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
        Assert.Equal(leftover, workspace.ReadText(".agents/open-forge.libraries.json"));
        using var json = System.Text.Json.JsonDocument.Parse(run.StandardOutput);
        var data = json.RootElement.GetProperty("data");
        Assert.Equal(condition == "directory" ? "not-requested" : "verified", data.GetProperty("verification").GetProperty("extensionsLifecycle").GetString());
        if (condition == "directory")
        {
            Assert.True(Directory.Exists(lockPath));
        }
        else
        {
            var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace,
                TestContext.Current.CancellationToken);
            Assert.Equal(["alpha"], ownership.Document.OwnersOf(".agents/alpha.txt"));
        }
    }
}
