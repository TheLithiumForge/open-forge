using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionPermissionLifecycleIntegrationTests
{
    [Trait("Boundary", "OS")]
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
            workspace.ReplaceText(PermissionFixture.PermissionPath, """{"allowInstallPaths":[]}""");
            if (effect == "copy")
            {
                source.ReplaceText($"content/{PermissionFixture.ExternalPath}", "updated exact content\n");
            }
            string[] arguments = command == "remove"
                ? ["extension", command, "team"]
                : ["extension", command, "team", "--source", source.Path];
            if (command == "update")
            {
                arguments = [.. arguments, "--detail", "standard"];
            }

            var run = await workspace.RunAsync(arguments, "  ALways  \nyes\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.True(run.ExitCode == 0, run.StandardOutput + run.StandardError);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Contains(PermissionFixture.ExternalPath, run.StandardError, StringComparison.Ordinal);
            if (command != "remove")
            {
                Assert.Contains(source.Path, run.StandardError, StringComparison.Ordinal);
            }
            if (command == "update")
            {
                if (command == "install")
                {
                    Assert.Contains(
                        "The team Extension is already installed and matches the package. Nothing to do.",
                        run.StandardOutput,
                        StringComparison.Ordinal);
                }
                else
                {
                    Assert.Contains("Grant: ", run.StandardOutput, StringComparison.Ordinal);
                    Assert.Contains("decision approved", run.StandardOutput, StringComparison.Ordinal);
                    Assert.Contains("action replace", run.StandardOutput, StringComparison.Ordinal);
                    Assert.Contains("outcome verified", run.StandardOutput, StringComparison.Ordinal);
                }
            }
            using var permission = JsonDocument.Parse(workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Contains(permission.RootElement.GetProperty("allowInstallPaths").EnumerateArray(), value => value.GetString() == PermissionFixture.ExternalPath);
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

    [Trait("Boundary", "OS")]
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
            workspace.ReplaceText(PermissionFixture.PermissionPath, """{"allowInstallPaths":[]}""");
            var before = workspace.Snapshot();
            var externalBefore = File.ReadAllBytes(workspace.Combine(PermissionFixture.ExternalPath));
            string[] arguments = command == "remove"
                ? ["extension", command, "team"]
                : ["extension", command, "team", "--source", source.Path];

            var run = await workspace.RunAsync(arguments, "no\nsentinel\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(130, run.ExitCode);
            Assert.Null(run.RemainingInput);
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(externalBefore, File.ReadAllBytes(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
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
            const string revoked = """{"allowInstallPaths":[]}""";
            workspace.ReplaceText(PermissionFixture.PermissionPath, revoked);
            var lifecycle = workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath);
            using var input = new ChangingInput(() => File.WriteAllText(workspace.Combine(PermissionFixture.ExternalPath), "changed during approval\n"));

            var run = await workspace.RunAsync(["extension", command, "team", "--source", source.Path], input,
                standardInputRedirected: false, promptOutputRedirected: false, CancellationToken.None, readRemainingInput: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(revoked, workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Equal(lifecycle, workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath));
            Assert.Equal("changed during approval\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
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
            var blocked = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--format", "json"]);
            Assert.Equal(5, blocked.ExitCode);
            Assert.Equal("unmanaged content\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));

            var applied = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--force", "--automatic", "--format", "json"]);

            Assert.Equal(0, applied.ExitCode);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(PermissionFixture.Grants, workspace.ReadText(PermissionFixture.PermissionPath));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    private sealed class ChangingInput(Action change) : StringReader("always\nyes\n")
    {
        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            change();
            return base.ReadLineAsync(cancellationToken);
        }
    }
}
