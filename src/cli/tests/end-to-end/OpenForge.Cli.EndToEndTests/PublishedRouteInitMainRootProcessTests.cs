using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInitMainRootProcessTests
{
    private const string MainRoot = "handbook/context";
    private const string Loader = MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private const string Category = "notes";
    private const string Child = "releases";
    private const string RouteId = Category + "/" + Child;
    private const string ParentPath = MainRoot + "/" + Category + "/_" + Category + PublishedTokens.Layout.MarkdownExtension;
    private const string TargetPath = MainRoot + "/" + RouteId + "/_" + Child + PublishedTokens.Layout.MarkdownExtension;
    private const string NativeId = PublishedTokens.Layout.SkillsDirectory + "/" + Child;
    private const string NativeParent = PublishedTokens.Layout.NativeSkillsPath + "/_" + PublishedTokens.Layout.SkillsDirectory + PublishedTokens.Layout.MarkdownExtension;
    private const string NativePath = PublishedTokens.Layout.NativeSkillsPath + "/" + Child + "/_" + Child + PublishedTokens.Layout.MarkdownExtension;

    [Theory(DisplayName = "Published Route Init creates literal custom-root and native Skills chains and converges without changing settings")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter)]
    [InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootAndNativeChainsConverge(string frontmatter)
    {
        using var workspace = TemporaryWorkspace.Create(nameof(CustomRootAndNativeChainsConverge));
        using var locks = PublishedWorkspaceLockStore.Create(nameof(CustomRootAndNativeChainsConverge));
        _ = locks.Track(workspace.Path);
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},
             "{{PublishedTokens.Settings.MainRootProperty}}":"{{MainRoot}}",
             "{{PublishedTokens.Settings.FrontmatterProperty}}":"{{frontmatter}}"}
            """);
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var settings = File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath));
        var executable = PublishedExecutableTarget.Discover();
        try
        {
            foreach (var operand in new[] { PublishedTokens.Layout.LiteralReferencePrefix + TargetPath, NativeId })
            {
                string[] arguments = [PublishedTokens.Route.Command, PublishedTokens.RouteInit.Command, operand,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
                AssertComplete(await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path,
                    workspace.SnapshotHashes, [.. arguments, PublishedTokens.RouteInit.DryRun], locks.EnvironmentVariables));
                AssertComplete(await PublishedProcessTestSupport.RunAsync(executable, workspace.Path, arguments, locks.EnvironmentVariables));
                var bytes = new[] { Loader, ParentPath, TargetPath, NativeParent, NativePath }
                    .Where(path => File.Exists(workspace.Combine(path)))
                    .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
                var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path,
                    workspace.SnapshotHashes, arguments, locks.EnvironmentVariables);
                AssertComplete(repeated);
                using var report = JsonDocument.Parse(repeated.StandardOutput);
                Assert.Empty(report.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
                foreach (var (path, expected) in bytes) Assert.Equal(expected, File.ReadAllBytes(workspace.Combine(path)));
            }
            foreach (var path in new[] { ParentPath, TargetPath, NativeParent, NativePath })
                Assert.True(File.Exists(workspace.Combine(path)), path);
            Assert.Equal(settings, File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
            CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
            {
                [Loader] = File.ReadAllText(workspace.Combine(Loader)),
                [TargetPath] = File.ReadAllText(workspace.Combine(TargetPath)),
                [NativePath] = File.ReadAllText(workspace.Combine(NativePath)),
            }, testName: nameof(CustomRootAndNativeChainsConverge) + "-" + frontmatter);
            Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
            Assert.False(Directory.Exists(workspace.Combine(PublishedTokens.Sources.AgentsRoot + "/" + Category)));
            Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
        }
        finally
        {
            foreach (var path in new[] { TargetPath, ParentPath, NativePath, NativeParent })
                if (File.Exists(workspace.Combine(path))) File.Delete(workspace.Combine(path));
            foreach (var path in new[] { MainRoot + "/" + RouteId, MainRoot + "/" + Category,
                         PublishedTokens.Layout.NativeSkillsPath + "/" + Child, PublishedTokens.Layout.NativeSkillsPath })
                if (Directory.Exists(workspace.Combine(path))) Directory.Delete(workspace.Combine(path));
        }
    }

    private static void AssertComplete(ProcessRunResult result)
    {
        Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
        Assert.Empty(result.StandardError);
        using var report = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(PublishedTokens.Statuses.Complete, report.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
    }
}
