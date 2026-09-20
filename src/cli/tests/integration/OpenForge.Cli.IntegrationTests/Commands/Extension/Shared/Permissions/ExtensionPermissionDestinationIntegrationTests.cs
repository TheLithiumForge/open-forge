using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Integration")]
public sealed class ExtensionPermissionDestinationIntegrationTests
{
    [Trait("Boundary", "OS")]
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
            var lifecycle = workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath);
            const string revoked = """{"allowInstallPaths":[]}""";
            workspace.ReplaceText(PermissionFixture.PermissionPath, revoked);
            void RedirectParent()
            {
                if (Directory.Exists(workspace.Combine(".apm/.git")))
                {
                    return;
                }
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
            Assert.Equal(duringApproval ? 2 : 0, input.ReadCount);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(".apm/.git/team.md")));
            Assert.Equal(lifecycle, workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath));
            Assert.Equal(revoked, workspace.ReadText(PermissionFixture.PermissionPath));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public static async Task InstallationCannotOccupyThePermissionFileWithADirectory()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-control-parent");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage($"{PermissionFixture.PermissionPath}/note.txt");
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic", "--format", "json"]);

        Assert.Equal(5, run.ExitCode);
        Assert.False(Directory.Exists(workspace.Combine(PermissionFixture.PermissionPath)));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public static async Task InspectKeepsExternalMarkdownAndAmbiguousEntriesOpaque(bool withSource, bool ambiguousEntries)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("permission-inspect-opaque");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var bytes = ambiguousEntries
            ? "# External\r\n\r\n## Entries\r\n\r\n## Entries\r\n"
            : "# External\r\nPlain Markdown.\r\n";
        source.ReplaceText($"content/{PermissionFixture.ExternalPath}", bytes);
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        try
        {
            Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "team", "--source", source.Path, "--automatic"])).ExitCode);
            var before = workspace.Snapshot();
            string[] arguments = withSource
                ? ["extension", "inspect", "team", "--source", source.Path, "--format", "json", "--detail=full"]
                : ["extension", "inspect", "team", "--format", "json", "--detail=full"];

            var run = await workspace.RunAsync(arguments);

            using var document = JsonDocument.Parse(run.StandardOutput);
            var result = document.RootElement.GetProperty("data");
            var path = Assert.Single(result.GetProperty("files").EnumerateArray());
            // With an intended source these opaque bytes compare equal. Without
            // one, ownership alone supplies no content identity to compare.
            Assert.Equal(withSource ? "unchanged" : "unknown", path.GetProperty("relation").GetString());
            Assert.NotNull(path.GetProperty("installedSha256").GetString());
            if (withSource)
            {
                Assert.NotNull(path.GetProperty("packageSha256").GetString());
            }
            Assert.Empty(result.GetProperty("registeredIn").EnumerateArray());
            Assert.DoesNotContain(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() is "extension-inspect.fingerprint-fallback" or "extension-inspect.generated-boundary-invalid");
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal(bytes, File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    private sealed class ChangingInput(Action change) : StringReader("always\nyes\n")
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
