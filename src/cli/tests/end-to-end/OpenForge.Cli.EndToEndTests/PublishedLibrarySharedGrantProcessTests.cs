using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibrarySharedGrantProcessTests
{
    [Theory, Trait("Feature", "workspace-settings"), Trait("Evidence", "EndToEnd")]
    [InlineData("attach", false), InlineData("attach", true)]
    [InlineData("sync", false), InlineData("sync", true)]
    [InlineData("detach", false), InlineData("detach", true)]
    public static async Task ExplicitRepeatableGrantPersistsAndDryRunWritesNothing(string command, bool dryRun)
    {
        using var workspace = new PublishedLibraryWorkspace();
        var executable = PublishedExecutableTarget.Discover();
        workspace.ConsumerRoute();
        workspace.Source("README.md");
        workspace.GrantDocs();
        try
        {
            if (command != "attach")
            {
                using var attached = PublishedLibraryWorkspace.Result(await workspace.RunAsync(executable,
                    "library", "attach", PublishedLibraryWorkspace.Id, PublishedLibraryWorkspace.SourceRoot, "--to", "docs", "--automatic", "--format=json"), "completed");
            }
            const string original = "{\"keep\":true,\"allowInstallPaths\":[]}";
            File.WriteAllText(workspace.Combine(".agents/open-forge.json"), original);
            string[] selection = command == "attach"
                ? ["library", command, PublishedLibraryWorkspace.Id, PublishedLibraryWorkspace.SourceRoot, "--to", "docs"]
                : ["library", command, PublishedLibraryWorkspace.Id];
            string[] arguments = [.. selection, "--allow-path", "docs", "--allow-path", "tools", "--format=json", .. dryRun ? new[] { "--dry-run" } : new[] { "--automatic" }];
            if (dryRun)
            {
                using var preview = PublishedLibraryWorkspace.Result(await workspace.ReadOnlyAsync(executable, arguments), "completed");
                var permissions = preview.RootElement.GetProperty("data").GetProperty("permissions");
                if (command == "attach")
                {
                    Assert.False(permissions.GetProperty("saved").GetBoolean());
                }
                else
                {
                    Assert.Equal("replace", permissions.GetProperty("action").GetString());
                    Assert.Equal("planned", permissions.GetProperty("outcome").GetString());
                }
                Assert.Equal(original, File.ReadAllText(workspace.Combine(".agents/open-forge.json")));
            }
            else
            {
                using var applied = PublishedLibraryWorkspace.Result(await workspace.RunAsync(executable, arguments), "completed");
                using var settings = JsonDocument.Parse(File.ReadAllText(workspace.Combine(".agents/open-forge.json")));
                Assert.Equal(["docs", "tools"], settings.RootElement.GetProperty("allowInstallPaths").EnumerateArray().Select(entry => entry.GetString()));
                Assert.True(settings.RootElement.GetProperty("keep").GetBoolean());
                Assert.Equal("granted", applied.RootElement.GetProperty("data").GetProperty("permissions").GetProperty("decision").GetString());
            }
        }
        finally
        {
            if (new FileInfo(workspace.Combine("docs/README.md")).LinkTarget == "../shared/team-knowledge/README.md")
            {
                File.Delete(workspace.Combine("docs/README.md"));
            }
            var parent = workspace.Combine("docs");
            if (Directory.Exists(parent) && !Directory.EnumerateFileSystemEntries(parent).Any())
            {
                Directory.Delete(parent);
            }
        }
    }
}
