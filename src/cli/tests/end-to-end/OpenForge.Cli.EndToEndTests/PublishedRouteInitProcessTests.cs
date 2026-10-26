using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInitProcessTests
{
    [Theory(DisplayName = "Published Init restores an unlisted outside anchor and nested chain without changing Loader or settings")]
    [InlineData(PublishedTokens.Sources.AgentsRoot), InlineData("knowledge/main")]
    [Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task RegisteredOutsideChainConverges(string mainRoot)
    {
        const string outside = "docs/team";
        const string child = outside + "/decisions";
        const string anchor = outside + "/_team" + PublishedTokens.Layout.MarkdownExtension;
        const string leaf = child + "/_decisions" + PublishedTokens.Layout.MarkdownExtension;
        using var workspace = TemporaryWorkspace.Create(nameof(RegisteredOutsideChainConverges));
        using var locks = PublishedWorkspaceLockStore.Create(nameof(RegisteredOutsideChainConverges));
        _ = locks.Track(workspace.Path);
        workspace.CreateDirectory(outside);
        workspace.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{mainRoot}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["{{outside}}"]}""");
        var loader = mainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
        workspace.WriteText(loader, OpenForgeDocumentSeed.GeneratedEntries(PublishedTokens.Metadata.EmptyEntry));
        workspace.WriteBytes("unrelated.bin", [0, 255, 13, 10]);
        var before = workspace.SnapshotHashes();
        var executable = PublishedExecutableTarget.Discover();
        string[] arguments = [PublishedTokens.Route.Command, PublishedTokens.RouteInit.Command,
            PublishedTokens.Layout.LiteralReferencePrefix + leaf,
            PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
        try
        {
            var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path,
                workspace.SnapshotHashes, [.. arguments, PublishedTokens.RouteInit.DryRun], locks.EnvironmentVariables);
            Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, preview.ExitCode);
            Assert.Empty(preview.StandardError);
            var applied = await PublishedProcessTestSupport.RunAsync(executable, workspace.Path, arguments, locks.EnvironmentVariables);
            Assert.True(applied.ExitCode == PublishedTokens.Statuses.CompleteExitCode, applied.StandardOutput + applied.StandardError);
            Assert.Empty(applied.StandardError);
            Assert.True(File.Exists(workspace.Combine(anchor)));
            Assert.True(File.Exists(workspace.Combine(leaf)));
            var after = workspace.SnapshotHashes();
            foreach (var entry in before) Assert.Equal(entry.Value, after[entry.Key]);
            Assert.Equal(new[] { anchor, leaf }, after.Keys.Except(before.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal));
            var repeat = await PublishedProcessTestSupport.RunWithoutWritesAsync(executable, workspace.Path,
                workspace.SnapshotHashes, arguments, locks.EnvironmentVariables);
            Assert.Equal(PublishedTokens.Statuses.CompleteExitCode, repeat.ExitCode);
            using var report = JsonDocument.Parse(repeat.StandardOutput);
            Assert.Empty(report.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        }
        finally
        {
            foreach (var path in new[] { leaf, anchor })
                if (File.Exists(workspace.Combine(path))) File.Delete(workspace.Combine(path));
            if (Directory.Exists(workspace.Combine(child))) Directory.Delete(workspace.Combine(child));
        }
    }

    [Fact(DisplayName = "Published generic Route Init emits canonical atomic metadata for a comma-separated apply-to value"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task CommaSeparatedApplyToEmitsCanonicalAtomicList()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateGeneric();
        var result = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "init", "docs",
                "--description", "Project documents",
                "--tag=Documentation",
                "--apply-to", "src/*.ts,**/*.cs",
                "--dry-run",
                "--format=json",
                "--detail", "full",
                "--workspace", workspace.Path,
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("dry-run", document.RootElement.GetProperty("data").GetProperty("mode").GetString());
        Assert.Equal(
            ["**/*.cs", "src/*.ts"],
            document.RootElement.GetProperty("data").GetProperty("metadata").GetProperty("applyTo")
                .EnumerateArray().Select(value => value.GetString()));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published generic Route Init previews applies and converges"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task GenericDryRunApplyAndRepeatNoOpFormOneRealJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateGeneric();
        string[] arguments = ["route", "init", "docs", "--description", "Project documents", "--tag=Documentation", "--apply-to", "src/*.ts", "--apply-to=**/*.cs", "--format=json", "--detail", "full", "--workspace", workspace.Path];
        var preview = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--dry-run"]);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal("dry-run", previewDocument.RootElement.GetProperty("data").GetProperty("mode").GetString());
        Assert.Equal(
            ["**/*.cs", "src/*.ts"],
            previewDocument.RootElement.GetProperty("data").GetProperty("metadata").GetProperty("applyTo")
                .EnumerateArray().Select(value => value.GetString()));
        workspace.AssertNoLockInfrastructure();

        var textPreview = await RunWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "init", "docs",
                "--description", "Project documents",
                "--tag=Documentation",
                "--apply-to", "src/*.ts",
                "--apply-to=**/*.cs",
                "--dry-run",
                "--detail", "standard",
                "--workspace", workspace.Path,
            ]);
        Assert.Equal(0, textPreview.ExitCode);
        Assert.Contains("Applies to: `**/*.cs`, `src/*.ts`", textPreview.StandardOutput, StringComparison.Ordinal);

        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("verified", appliedDocument.RootElement.GetProperty("data").GetProperty("verification").GetString());
        Assert.Equal(
            ["**/*.cs", "src/*.ts"],
            appliedDocument.RootElement.GetProperty("data").GetProperty("metadata").GetProperty("applyTo")
                .EnumerateArray().Select(value => value.GetString()));
        Assert.True(File.Exists(workspace.Combine(PublishedRouteInitWorkspace.GenericTargetPath)));
        var scaffold = await workspace.ReadTextAsync(PublishedRouteInitWorkspace.GenericTargetPath, TestContext.Current.CancellationToken);
        Assert.Contains("Project documents", scaffold, StringComparison.Ordinal);
        Assert.Contains("# docs", scaffold, StringComparison.Ordinal);
        Assert.Contains("applyTo: [\"**/*.cs\", \"src/*.ts\"]", scaffold, StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();

        var noOp = await RunWithoutWritesAsync(target, workspace, ["route", "init", "docs", "--format=json", "--workspace", workspace.Path]);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(noOpDocument.RootElement.GetProperty("effects").EnumerateArray());
        Assert.All(
            noOpDocument.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray(),
            entrypoint => Assert.Equal("unchanged", entrypoint.GetProperty("outcome").GetString()));
    }

    [Fact(DisplayName = "Published Framework Route Init reuses Install and converges one sparse scope"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task FrameworkDryRunApplyAndRepeatNoOpPreserveOwnership()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateFramework();
        var installed = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
            ["install", "--automatic", "--format=json", "--workspace", workspace.Path], workspace.ProcessEnvironment);
        Assert.Equal(0, installed.ExitCode);
        Assert.Equal(string.Empty, installed.StandardError);
        string[] arguments = ["route", "init", "memory/Mobile App/working", "--framework", "--format=json", "--detail", "full", "--workspace", workspace.Path];
        var preview = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--dry-run"]);
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        Assert.Equal("framework", previewDocument.RootElement.GetProperty("data").GetProperty("scaffold").GetString());

        var applied = await PublishedProcessTestSupport.RunAsync(target, workspace.Path, arguments, workspace.ProcessEnvironment);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("verified", appliedDocument.RootElement.GetProperty("data").GetProperty("verification").GetString());
        Assert.True(File.Exists(workspace.Combine(PublishedRouteInitWorkspace.FrameworkFinalPath)));
        workspace.AssertPersistentExternalLock();

        var noOp = await RunWithoutWritesAsync(target, workspace, arguments);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        Assert.Empty(noOpDocument.RootElement.GetProperty("effects").EnumerateArray());
        Assert.All(
            noOpDocument.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray(),
            entrypoint => Assert.Equal("unchanged", entrypoint.GetProperty("outcome").GetString()));
    }

    [Fact(DisplayName = "Published Framework Route Init refuses a workspace without trusted Install"), Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task FrameworkWithoutTrustedInstallIsBlocked()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState,
            ["route", "init", "memory/working", "--framework", "--workspace", workspace.Path, "--detail", "standard"], workspace.ProcessEnvironment);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Cannot initialize", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("The Framework scaffold needs an installed Framework", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge install --dry-run", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteInitWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
