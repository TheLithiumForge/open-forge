using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionPermissionDestinationIntegrationTests
{
    [Theory]
    [InlineData("remove", false)]
    [InlineData("remove", true)]
    [InlineData("update", false)]
    [InlineData("update", true)]
    public static async Task LifecycleNeverFollowsAnExternalParentIntoProtectedContent(string command, bool duringApproval)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-parent-link");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            if (command == "update")
            {
                source.MoveFile($"content/{PermissionFixture.ExternalPath}", "retired.md");
            }
            var lifecycle = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
            const string revoked = """{"schemaVersion":1,"extensions":[],"libraries":[]}""";
            workspace.ReplaceText(PermissionFixture.PermissionPath, revoked);
            void RedirectParent()
            {
                Directory.Move(workspace.Combine(".apm/agents"), workspace.Combine(".apm/.git"));
                Directory.CreateSymbolicLink(workspace.Combine(".apm/agents"), ".git");
            }
            if (!duringApproval)
            {
                RedirectParent();
            }
            using var input = new ChangingInput(() =>
            {
                if (duringApproval)
                {
                    RedirectParent();
                }
            });
            string[] arguments = command == "remove"
                ? ["extension", "remove", "team"]
                : ["extension", "update", "team", "--source", source.Path, "--prune"];

            var run = await workspace.RunAsync(arguments, input, standardInputRedirected: false,
                promptOutputRedirected: false, CancellationToken.None, readRemainingInput: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(duringApproval ? 1 : 0, input.ReadCount);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(".apm/.git/team.md")));
            Assert.Equal(lifecycle, workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));
            Assert.Equal(revoked, workspace.ReadText(PermissionFixture.PermissionPath));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Fact]
    public static async Task InstallationCannotOccupyThePermissionFileWithADirectory()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-control-parent");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage($"{PermissionFixture.PermissionPath}/note.txt");
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--json"]);

        Assert.Equal(5, run.ExitCode);
        Assert.False(Directory.Exists(workspace.Combine(PermissionFixture.PermissionPath)));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public static async Task InspectKeepsExternalMarkdownAndMarkerBytesOpaque(bool withSource, bool markerLike)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-inspect-opaque");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var bytes = markerLike
            ? "# External\r\n<!-- open-forge:generated-index:end -->\r\n"
            : "# External\r\nPlain Markdown.\r\n";
        source.ReplaceText($"content/{PermissionFixture.ExternalPath}", bytes);
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            var before = workspace.Snapshot();
            string[] arguments = withSource
                ? ["extension", "inspect", "team", "--source", source.Path, "--json"]
                : ["extension", "inspect", "team", "--json"];

            var run = await workspace.RunAsync(arguments);

            using var document = JsonDocument.Parse(run.StandardOutput);
            var result = document.RootElement.GetProperty("result");
            var path = Assert.Single(result.GetProperty("comparison").GetProperty("paths").EnumerateArray());
            Assert.Equal("unchanged", path.GetProperty("relation").GetString());
            Assert.Equal("exact-bytes", path.GetProperty("current").GetProperty("kind").GetString());
            Assert.Equal(path.GetProperty("baseline").GetProperty("sha256").GetString(), path.GetProperty("current").GetProperty("sha256").GetString());
            if (withSource)
            {
                Assert.Equal("exact-bytes", path.GetProperty("intended").GetProperty("kind").GetString());
            }
            Assert.Empty(result.GetProperty("generated").GetProperty("regions").EnumerateArray());
            Assert.DoesNotContain(result.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() is "extension-inspect.fingerprint-fallback" or "extension-inspect.generated-boundary-invalid");
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(bytes, File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    private sealed class ChangingInput(Action change) : StringReader("yes\n")
    {
        internal int ReadCount { get; private set; }

        public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            ReadCount++;
            change();
            return base.ReadLineAsync(cancellationToken);
        }
    }
}
