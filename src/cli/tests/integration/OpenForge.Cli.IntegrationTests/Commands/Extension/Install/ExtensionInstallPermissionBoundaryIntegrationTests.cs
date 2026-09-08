using System.IO.Compression;
using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionInstallPermissionBoundaryIntegrationTests
{
    [Theory]
    [InlineData(".git/config")]
    [InlineData(".agents/open-forge.permissions.json")]
    [InlineData(".agents/open-forge.libraries.json")]
    public static async Task PermissionAndForceCannotAuthorizeProtectedDestinations(string target)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-protected");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage(target);
        workspace.CreateOccupant(PermissionFixture.PermissionPath,
            """{"schemaVersion":1,"extensions":[{"id":"team","paths":[".git/config"]}],"libraries":[]}""");
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--force", "--automatic", "--json"]);

        Assert.Equal(5, run.ExitCode);
        using var json = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains("extension-install.target-unsafe", json.RootElement.GetProperty("result").GetProperty("findings")
            .EnumerateArray().Select(value => value.GetProperty("code").GetString()));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact]
    public async Task MalformedConsumerPermissionIsPreservedWithoutAQuestion()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-invalid-consumer");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, "{ malformed consumer permission\n");
        var before = workspace.Snapshot();
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path],
                "yes\n", standardInputRedirected: false, promptOutputRedirected: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal("yes", run.RemainingInput);
            Assert.DoesNotContain("[y/N]", run.StandardError, StringComparison.Ordinal);
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Fact]
    public async Task CancellationDuringApprovalDoesNotGrantOrCopy()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-cancel-approval");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        using var cancellation = new CancellationTokenSource();
        using var input = new CancellingTextReader(cancellation);
        var before = workspace.Snapshot();
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path], input,
                standardInputRedirected: false, promptOutputRedirected: false, cancellation.Token, readRemainingInput: false);

            Assert.Equal(130, run.ExitCode);
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task ChangedReviewedFactsDoNotInheritApproval(bool changeSource)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-stale-approval");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var lifecycleBefore = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
        var answered = false;
        using var input = new ChangingInput(() =>
        {
            answered = true;
            if (changeSource)
            {
                source.ReplaceText($"content/{PermissionFixture.ExternalPath}", "changed after review\n");
            }
            else
            {
                workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
            }
        });
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path], input,
                standardInputRedirected: false, promptOutputRedirected: false, CancellationToken.None, readRemainingInput: false);

            Assert.True(answered, "The exact permission question was not reached.");
            Assert.Equal(5, run.ExitCode);
            Assert.False(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(lifecycleBefore, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
            if (changeSource)
            {
                Assert.False(File.Exists(workspace.Combine(PermissionFixture.PermissionPath)));
            }
            else
            {
                Assert.Equal(PermissionFixture.Grants, workspace.ReadText(PermissionFixture.PermissionPath));
            }
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task FailedCopyRetainsApprovedPermissionAndExactRecovery(bool existingPermission)
    {
        if (OperatingSystem.IsWindows())
        {
            Assert.Skip("This copy failure uses Unix directory permissions.");
            return;
        }
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-partial-effect");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        const string original = "{\"schemaVersion\":1,\"extensions\":[{\"id\":\"keep\",\"paths\":[\"keep.txt\"]}],\"libraries\":[]}\r\n";
        if (existingPermission)
        {
            workspace.CreateOccupant(PermissionFixture.PermissionPath, original);
        }
        var parent = Directory.CreateDirectory(workspace.Combine(".apm/agents")).FullName;
        var originalMode = File.GetUnixFileMode(parent);
        var lifecycleBefore = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
        using var input = new ChangingInput(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(parent, UnixFileMode.UserRead | UnixFileMode.UserExecute);
            }
        });
        try
        {
            var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path], input,
                standardInputRedirected: false, promptOutputRedirected: false, CancellationToken.None, readRemainingInput: false);

            Assert.Equal(1, run.ExitCode);
            Assert.False(File.Exists(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.True(File.Exists(workspace.Combine(PermissionFixture.PermissionPath)), "Explicit approval must remain after copy failure.");
            using var permission = JsonDocument.Parse(workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Contains(permission.RootElement.GetProperty("extensions").EnumerateArray(), value => value.GetProperty("id").GetString() == "team");
            Assert.Equal(lifecycleBefore, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
            const string prefix = "recovery-candidate:";
            var candidate = Assert.Single(workspace.Snapshot().Keys, value => value.StartsWith(prefix, StringComparison.Ordinal));
            using var archive = ZipFile.OpenRead(candidate[prefix.Length..]);
            using var manifestStream = Assert.IsType<ZipArchiveEntry>(archive.GetEntry("manifest.json")).Open();
            using var manifest = JsonDocument.Parse(manifestStream);
            var entry = Assert.Single(manifest.RootElement.GetProperty("entries").EnumerateArray(),
                value => value.GetProperty("logicalPath").GetString() == PermissionFixture.PermissionPath);
            Assert.Equal(existingPermission ? "ordinary-file" : "missing", entry.GetProperty("prior").GetProperty("kind").GetString());
            if (existingPermission)
            {
                var payload = Assert.IsType<string>(entry.GetProperty("priorPayload").GetString());
                using var priorStream = Assert.IsType<ZipArchiveEntry>(archive.GetEntry(payload)).Open();
                using var priorReader = new StreamReader(priorStream);
                Assert.Equal(original, await priorReader.ReadToEndAsync(TestContext.Current.CancellationToken));
            }
            else
            {
                Assert.Equal(JsonValueKind.Null, entry.GetProperty("priorPayload").ValueKind);
            }
        }
        finally
        {
            File.SetUnixFileMode(parent, originalMode);
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
