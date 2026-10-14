using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallRetryIntegrationTests
{
    [Theory(DisplayName = "Local Extension force previews preserve source workspace selection and presentation without writes")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task LocalSourceForceRetryPreservesRequest(bool all)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-retry workspace's");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-retry source's");
        source.AddPackage("local", [], (".agents/local.md", "# Local\n"));
        workspace.CreateOccupant(".agents/local.md", "user file\n");
        var before = workspace.Snapshot();
        string[] selection = all ? ["--all"] : ["local"];
        string[] arguments = ["extension", "install", .. selection, "--source", source.Path, "--workspace", workspace.Path,
            "--automatic", "--format", "json", "--detail", "standard", "--detail-filter", "warning"];
        var blocked = await workspace.RunAsync(arguments);
        Assert.Equal(5, blocked.ExitCode);
        using var json = JsonDocument.Parse(blocked.StandardOutput);
        var command = json.RootElement.GetProperty("next").GetProperty("command").GetString();
        var expected = $"open-forge extension install {(all ? "--all" : "local")} --source {CliCommandText.Quote(source.Path)} --workspace {CliCommandText.Quote(workspace.Path)} --automatic --force --dry-run --format json --detail standard --detail-filter warning";
        Assert.Equal(expected, command);
        Assert.Equal(before, workspace.Snapshot());
        var preview = await workspace.RunAsync([.. arguments, "--force", "--dry-run"]);
        Assert.Equal(0, preview.ExitCode);
        using var previewJson = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal("local", Assert.Single(previewJson.RootElement.GetProperty("data").GetProperty("packages").EnumerateArray()).GetProperty("id").GetString());
        Assert.Equal(before, workspace.Snapshot());
        var text = await workspace.RunAsync(["extension", "install", .. selection, "--source", source.Path, "--workspace", workspace.Path, "--automatic"]);
        var textNext = Assert.Single(text.StandardError.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries), line => line.StartsWith("Next:", StringComparison.Ordinal));
        Assert.Equal($"Next: open-forge extension install {(all ? "--all" : "local")} --source {CliCommandText.Quote(source.Path)} --workspace {CliCommandText.Quote(workspace.Path)} --automatic --force --dry-run", textNext);
    }

    [Fact(DisplayName = "Local Extension permission retry preserves the request and adds the missing path only")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task PermissionRetryPreservesRequest()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-permission-retry");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var before = workspace.Snapshot();
        var result = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--workspace", workspace.Path,
            "--automatic", "--force", "--dry-run", "--format", "json"]);
        Assert.Equal(5, result.ExitCode);
        using var json = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal($"open-forge extension install team --source {CliCommandText.Quote(source.Path)} --workspace {CliCommandText.Quote(workspace.Path)} --allow-path {CliCommandText.Quote(PermissionFixture.ExternalPath)} --automatic --force --dry-run --format json",
            json.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(before, workspace.Snapshot());
    }
}
