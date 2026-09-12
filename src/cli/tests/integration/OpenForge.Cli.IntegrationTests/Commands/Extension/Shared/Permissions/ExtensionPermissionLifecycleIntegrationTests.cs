using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionPermissionLifecycleIntegrationTests
{
    [Theory]
    [InlineData("install", "preserve content")]
    [InlineData("update", "copy")]
    [InlineData("update", "preserve content")]
    [InlineData("remove", "delete")]
    public static async Task ReapprovalUsesTheReviewedLifecycleAndReportsSavedPermission(string command, string effect)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-lifecycle-approval");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            var installed = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"]);
            Assert.Equal(0, installed.ExitCode);
            workspace.ReplaceText(PermissionFixture.PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
            if (effect == "copy")
            {
                source.ReplaceText($"content/{PermissionFixture.ExternalPath}", "updated exact content\n");
            }
            string[] arguments = command == "remove"
                ? ["extension", command, "team"]
                : ["extension", command, "team", "--source", source.Path];

            var run = await workspace.RunAsync(arguments, "  YEs  \nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Contains($"({effect})", run.StandardError, StringComparison.Ordinal);
            if (command != "remove")
            {
                Assert.Contains(source.Path, run.StandardError, StringComparison.Ordinal);
            }
            Assert.Contains("Permissions: approved", run.StandardOutput, StringComparison.Ordinal);
            Assert.Contains("Permissions: approved; record replace; verified", run.StandardOutput, StringComparison.Ordinal);
            using var permission = JsonDocument.Parse(workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Equal("team", Assert.Single(permission.RootElement.GetProperty("extensions").EnumerateArray()).GetProperty("id").GetString());
            if (command == "remove")
            {
                Assert.False(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)));
            }
            else
            {
                Assert.Equal(effect == "copy" ? "updated exact content\n" : "content bytes\n",
                    File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            }
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData("update")]
    [InlineData("remove")]
    public static async Task DecliningLifecycleReapprovalLeavesAllReviewedStateUnchanged(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-lifecycle-decline");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            workspace.ReplaceText(PermissionFixture.PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
            var before = workspace.Snapshot();
            var externalBefore = File.ReadAllBytes(workspace.Combine(PermissionFixture.ExternalPath));
            string[] arguments = command == "remove"
                ? ["extension", command, "team"]
                : ["extension", command, "team", "--source", source.Path];

            var run = await workspace.RunAsync(arguments, "no\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(externalBefore, File.ReadAllBytes(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData("install")]
    [InlineData("update")]
    public static async Task ChangedUnchangedTargetDoesNotInheritPermissionApproval(string command)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-no-op-changed");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            const string revoked = """{"schemaVersion":1,"extensions":[],"libraries":[]}""";
            workspace.ReplaceText(PermissionFixture.PermissionPath, revoked);
            var lifecycle = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
            using var input = new ChangingInput(() => File.WriteAllText(workspace.Combine(PermissionFixture.ExternalPath), "changed during approval\n"));

            var run = await workspace.RunAsync(["extension", command, "team", "--source", source.Path], input,
                standardInputRedirected: false, promptOutputRedirected: false, CancellationToken.None, readRemainingInput: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(revoked, workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Equal(lifecycle, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
            Assert.Equal("changed during approval\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Fact]
    public static async Task InitialExternalOccupantRequiresForceSeparatelyFromPermission()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-external-force");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        Directory.CreateDirectory(workspace.Combine(".apm/agents"));
        workspace.CreateOccupant(PermissionFixture.ExternalPath, "unmanaged content\n");
        try
        {
            var blocked = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--json"]);
            Assert.Equal(5, blocked.ExitCode);
            Assert.Equal("unmanaged content\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));

            var applied = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--force", "--automatic", "--json"]);

            Assert.Equal(0, applied.ExitCode);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(PermissionFixture.Grants, workspace.ReadText(PermissionFixture.PermissionPath));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
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
