using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;
using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionInstallPermissionIntegrationTests
{

    [Fact]
    public async Task ExplicitYesIsRememberedAndRepeatedInstallDoesNotPrompt()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-remembered");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        try
        {
            var first = await workspace.RunAsync(
                ["extension", "install", "team", "--source", source.Path],
                "yes\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(0, first.ExitCode);
            Assert.Equal("sentinel", first.RemainingInput);
            Assert.Contains(PermissionFixture.ExternalPath, first.StandardError, StringComparison.Ordinal);
            Assert.Contains("[y/N]", first.StandardError, StringComparison.Ordinal);
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            var saved = File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath));
            using var permission = JsonDocument.Parse(saved);
            var grant = Assert.Single(permission.RootElement.GetProperty("extensions").EnumerateArray());
            Assert.Equal("team", grant.GetProperty("id").GetString());
            Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(grant.GetProperty("paths").EnumerateArray()).GetString());

            var repeat = await workspace.RunAsync(
                ["extension", "install", "team", "--source", source.Path],
                "unread\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(0, repeat.ExitCode);
            Assert.Equal("unread", repeat.RemainingInput);
            Assert.DoesNotContain("[y/N]", repeat.StandardError, StringComparison.Ordinal);
            Assert.Equal(saved, File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData(null), InlineData(""), InlineData("no"), InlineData("anything")]
    public static async Task DeclineLeavesPermissionContentAndOwnershipUnchanged(string? answer)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-declined");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var before = workspace.Snapshot();
        try
        {
            var run = await workspace.RunAsync(
                ["extension", "install", "team", "--source", source.Path],
                (answer is null ? string.Empty : $"{answer}\nsentinel\n"), standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Blocked, run.Status);
            Assert.Equal(answer is null ? null : "sentinel", run.RemainingInput);
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData("--automatic"), InlineData("--json"), InlineData("--dry-run"), InlineData("redirected")]
    public static async Task UnattendedOrPreviewCannotGrantPermission(string mode)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-no-prompt");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var before = workspace.Snapshot();
        string[] arguments = mode == "redirected"
            ? ["extension", "install", "team", "--source", source.Path]
            : ["extension", "install", "team", "--source", source.Path, mode];
        try
        {
            var run = await workspace.RunAsync(arguments, "yes\nsentinel\n",
                standardInputRedirected: mode == "redirected", promptOutputRedirected: mode == "redirected");

            Assert.Equal(5, run.ExitCode);
            Assert.Equal("yes", run.RemainingInput);
            Assert.DoesNotContain("[y/N]", run.StandardError, StringComparison.Ordinal);
            Assert.Equal(before, workspace.Snapshot());
            if (mode == "--json")
            {
                Assert.Equal(string.Empty, run.StandardError);
                using var json = JsonDocument.Parse(run.StandardOutput);
                var permissions = json.RootElement.GetProperty("result").GetProperty("permissions");
                Assert.Equal("required", permissions.GetProperty("decision").GetString());
                Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(permissions.GetProperty("missing").EnumerateArray()).GetProperty("path").GetString());
            }
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Fact]
    public async Task ExistingExactGrantAllowsAutomaticCopy()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-existing");
        await workspace.SeedFrameworkAsync();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        using var source = PermissionFixture.CreatePackage();
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--json"]);

            Assert.Equal(0, run.ExitCode);
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(PermissionFixture.Grants, workspace.ReadText(PermissionFixture.PermissionPath));
            using var json = JsonDocument.Parse(run.StandardOutput);
            Assert.Equal("granted", json.RootElement.GetProperty("result").GetProperty("permissions").GetProperty("decision").GetString());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData("update"), InlineData("remove")]
    public static async Task RevocationBlocksSelectedLifecycleWithoutReadingPermissionAsOwnership(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-revoked");
        await workspace.SeedFrameworkAsync();
        using var seed = ExtensionInstallCatalogue.Create("permission-seed");
        seed.AddPackage("team", [], (".agents/team.txt", "content bytes\n"));
        var installed = await workspace.RunAsync(["extension", "install", "team", "--source", seed.Path, "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        using var source = ExtensionInstallCatalogue.Create("permission-selected");
        source.AddPackage("team", [], (PermissionFixture.ExternalPath, "content bytes\n"));
        try
        {
            Directory.CreateDirectory(workspace.Combine(".apm/agents"));
            File.Move(workspace.Combine(".agents/team.txt"), workspace.Combine(PermissionFixture.ExternalPath));
            workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.LifecyclePath,
                workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath).Replace(".agents/team.txt", PermissionFixture.ExternalPath, StringComparison.Ordinal));
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            workspace.CreateOccupant(PermissionFixture.PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
            var before = workspace.Snapshot();
            string[] arguments = command == "update"
                ? ["extension", "update", "team", "--source", source.Path, "--automatic", "--json"]
                : ["extension", "remove", "team", "--automatic", "--json"];

            var run = await workspace.RunAsync(arguments);

            using var json = JsonDocument.Parse(run.StandardOutput);
            Assert.Contains($"extension-{command}.permission-required",
                json.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray()
                    .Select(value => value.GetProperty("code").GetString()));
            Assert.Equal(5, run.ExitCode);
            var permissions = json.RootElement.GetProperty("result").GetProperty("permissions");
            Assert.Equal("required", permissions.GetProperty("decision").GetString());
            Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(permissions.GetProperty("missing").EnumerateArray()).GetProperty("path").GetString());
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

}
