using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;
using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionInstallPermissionIntegrationTests
{

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task ExplicitAlwaysIsRememberedAndRepeatedInstallDoesNotPrompt()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-remembered");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        try
        {
            var first = await workspace.RunAsync(
                ["extension", "install", "team", "--source", source.Path],
                "always\nyes\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(0, first.ExitCode);
            Assert.Equal("sentinel", first.RemainingInput);
            Assert.Contains(PermissionFixture.ExternalPath, first.StandardError, StringComparison.Ordinal);
            Assert.Contains("always, once, cancel:", first.StandardError, StringComparison.Ordinal);
            workspace.ReplaceText(".agents/open-forge.lock.json",
                workspace.ReadText(".agents/open-forge.lock.json").Replace(".agents/team.txt", PermissionFixture.ExternalPath, StringComparison.Ordinal));
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            var saved = File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath));
            using var permission = JsonDocument.Parse(saved);
            Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(permission.RootElement.GetProperty("allowInstallPaths").EnumerateArray()).GetString());

            var repeat = await workspace.RunAsync(
                ["extension", "install", "team", "--source", source.Path],
                "unread\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(0, repeat.ExitCode);
            Assert.Equal("unread", repeat.RemainingInput);
            Assert.DoesNotContain("[a] Allow always", repeat.StandardError, StringComparison.Ordinal);
            Assert.Equal(saved, File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task AllowOnceAppliesWithoutWritingAbsentOrMalformedSettings(bool malformed)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-once");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        const string invalid = "{ malformed authored settings";
        if (malformed)
        {
            workspace.CreateOccupant(PermissionFixture.PermissionPath, invalid);
        }
        workspace.CreateOccupant(".agents/open-forge.permissions.json", "malformed retired file");
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path],
                "once\nyes\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);
            Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(malformed, File.Exists(workspace.Combine(PermissionFixture.PermissionPath)));
            if (malformed)
            {
                Assert.Equal(invalid, workspace.ReadText(PermissionFixture.PermissionPath));
            }
            Assert.Equal("malformed retired file", workspace.ReadText(".agents/open-forge.permissions.json"));
            var repeated = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--format", "json"]);
            Assert.Equal(5, repeated.ExitCode);
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
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

            Assert.Equal(130, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
            Assert.Equal(answer is "" ? "sentinel" : null, run.RemainingInput);
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("--automatic"), InlineData("--format=json"), InlineData("--dry-run"), InlineData("redirected")]
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
            var run = await workspace.RunAsync(arguments, "always\nsentinel\n",
                standardInputRedirected: mode == "redirected", promptOutputRedirected: mode == "redirected");

            Assert.Equal(5, run.ExitCode);
            Assert.Equal("always", run.RemainingInput);
            Assert.DoesNotContain("[a] Allow always", run.StandardError, StringComparison.Ordinal);
            Assert.Equal(before, workspace.Snapshot());
            if (mode == "--format=json")
            {
                Assert.Equal(string.Empty, run.StandardError);
                using var json = JsonDocument.Parse(run.StandardOutput);
                var permissions = json.RootElement.GetProperty("data").GetProperty("permissions");
                Assert.Equal("required", permissions.GetProperty("decision").GetString());
                Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(permissions.GetProperty("missing").EnumerateArray()).GetString());
            }
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("install", false), InlineData("install", true)]
    [InlineData("update", false), InlineData("update", true)]
    [InlineData("remove", false), InlineData("remove", true)]
    public static async Task ExplicitGrantPersistsAcrossCommandsAndDryRunCannotWrite(string command, bool dryRun)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-flag");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            if (command != "install")
            {
                Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            }
            const string original = "{\"keep\":true,\"allowInstallPaths\":[]}";
            workspace.ReplaceText(PermissionFixture.PermissionPath, original);
            string[] selection = command == "remove" ? ["extension", command, "team"] : ["extension", command, "team", "--source", source.Path];
            string[] arguments = [.. selection, "--allow-path", ".apm", "--allow-path", "tools", "--automatic", "--format", "json", .. dryRun ? new[] { "--dry-run" } : Array.Empty<string>()];
            var before = workspace.Snapshot();
            var run = await workspace.RunAsync(arguments);
            Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
            if (dryRun)
            {
                Assert.Equal(before, workspace.Snapshot());
            }
            else
            {
                using var settings = JsonDocument.Parse(workspace.ReadText(PermissionFixture.PermissionPath));
                Assert.Equal([".apm", "tools"], settings.RootElement.GetProperty("allowInstallPaths").EnumerateArray().Select(entry => entry.GetString()));
                Assert.True(settings.RootElement.GetProperty("keep").GetBoolean());
            }
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task ExistingExactGrantAllowsAutomaticCopy()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-existing");
        await workspace.SeedFrameworkAsync();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        using var source = PermissionFixture.CreatePackage();
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--format", "json"]);

            Assert.Equal(0, run.ExitCode);
            workspace.ReplaceText(".agents/open-forge.lock.json",
                workspace.ReadText(".agents/open-forge.lock.json").Replace(".agents/team.txt", PermissionFixture.ExternalPath, StringComparison.Ordinal));
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(PermissionFixture.Grants, workspace.ReadText(PermissionFixture.PermissionPath));
            using var json = JsonDocument.Parse(run.StandardOutput);
            Assert.Equal("granted", json.RootElement.GetProperty("data").GetProperty("permissions").GetProperty("decision").GetString());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install cancellation after staged permission preserves unknown settings bytes")]
    [InlineData("explicit")]
    [InlineData("always")]
    public static async Task FinalDeclineDoesNotPublishPermissionGrant(string grantMode)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"permission-staged-decline-{grantMode}");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        const string originalSettings =
            "{\"schemaVersion\":1,\"keep\":{\"nested\":true},\"allowInstallPaths\":[]}";
        workspace.CreateOccupant(PermissionFixture.PermissionPath, originalSettings);
        var before = workspace.Snapshot();
        var settingsBefore = File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath));
        try
        {
            string[] arguments = grantMode == "explicit"
                ? new[] {
                    "extension", "install", "team", "--source", source.Path,
                    "--allow-path", PermissionFixture.ExternalPath,
                }
                : new[] { "extension", "install", "team", "--source", source.Path };
            var input = grantMode == "explicit"
                ? $"no{Environment.NewLine}sentinel{Environment.NewLine}"
                : $"always{Environment.NewLine}no{Environment.NewLine}sentinel{Environment.NewLine}";

            var run = await workspace.RunAsync(
                arguments,
                input,
                standardInputRedirected: false,
                promptOutputRedirected: false);

            Assert.Equal(130, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Combine(PermissionFixture.PermissionPath)));
            Assert.False(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
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
            workspace.ReplaceText(".agents/open-forge.lock.json",
                workspace.ReadText(".agents/open-forge.lock.json").Replace(".agents/team.txt", PermissionFixture.ExternalPath, StringComparison.Ordinal));
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)), "The approved external file is missing.");
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            workspace.CreateOccupant(PermissionFixture.PermissionPath, """{"allowInstallPaths":[]}""");
            var before = workspace.Snapshot();
            string[] arguments = command == "update"
                ? ["extension", "update", "team", "--source", source.Path, "--automatic", "--format", "json"]
                : ["extension", "remove", "team", "--automatic", "--format", "json"];

            var run = await workspace.RunAsync(arguments);

            using var json = JsonDocument.Parse(run.StandardOutput);
            var findings = json.RootElement.GetProperty("findings").EnumerateArray().ToArray();
            Assert.Contains($"extension-{command}.permission-required",
                findings.Select(value => value.GetProperty("code").GetString()));
            Assert.Equal(5, run.ExitCode);
            var permissions = json.RootElement.GetProperty("data").GetProperty("permissions");
            Assert.Equal("required", permissions.GetProperty("decision").GetString());
            Assert.Equal(PermissionFixture.ExternalPath, Assert.Single(permissions.GetProperty("missing").EnumerateArray()).GetString());
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

}
