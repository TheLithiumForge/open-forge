using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteInitHelpProcessTests
{
    [Fact(DisplayName = "Published Route Init is discoverable from root through leaf help with all shared exits"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task HelpExposesTheCompletePublicCommandBoundary()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var missingWorkspace = workspace.Combine("missing");
        var root = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["--help"],
            workspace.ProcessEnvironment);
        var group = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "--help"],
            workspace.ProcessEnvironment);
        var leaf = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", "--help", "--workspace", missingWorkspace],
            workspace.ProcessEnvironment);

        Assert.All([root, group, leaf], result =>
        {
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
        });
        Assert.Contains("route init", root.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("init     available", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("The route group performs no operation.", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "Planned but unavailable operation: remove.",
            group.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route init <route-target> [--framework] [--description <text>] [--responsibility <text>] [--tag=<tag>]... [--dry-run] [global flags]",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Omit --dry-run to apply the complete preflighted plan.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--template", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("complete: exit 0 and human stdout.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("attention: exit 2 and human stdout.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("incomplete: exit 3 and human stdout.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("invalid: exit 4 and human stderr.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("blocked: exit 5 and human stderr.", leaf.StandardOutput, StringComparison.Ordinal);

        // Failed and interrupted require an actual fault or caller cancellation. Public help
        // accounts for their shared process mapping without adding a product fault seam or a
        // platform-specific signal harness merely to manufacture either status.
        Assert.Contains("failed: exit 1 and human stderr.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("interrupted: exit 130 and human stderr.", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }
}

public sealed class PublishedRouteInitGenericProcessTests
{
    private const string TargetId = "docs";
    private const string TargetPath = PublishedRouteInitWorkspace.GenericTargetPath;

    [Fact(DisplayName = "Published generic Route Init previews applies and converges without prompting"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task GenericDryRunApplyAndRepeatNoOpFormOneRealJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateGeneric();
        string[] previewArguments =
        [
            "route", "init", TargetId,
            "--description", "Project documents",
            "--tag=Documentation",
            "--dry-run",
            "--json",
            "--workspace", workspace.Path,
        ];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            previewArguments,
            workspace.ProcessEnvironment);
        var verbosePreview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. previewArguments, "--verbose"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Equal(preview.ExitCode, verbosePreview.ExitCode);
        Assert.Equal(preview.StandardOutput, verbosePreview.StandardOutput);
        Assert.InRange(verbosePreview.StandardError.Length, 1, 4096);
        Assert.DoesNotContain("Apply this", preview.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply this", verbosePreview.StandardError, StringComparison.Ordinal);
        using (var document = JsonDocument.Parse(preview.StandardOutput))
        {
            var root = document.RootElement;
            Assert.Equal(
                ["schemaVersion", "command", "status", "workspace", "result", "next"],
                root.EnumerateObject().Select(property => property.Name));
            Assert.Equal("route init", root.GetProperty("command").GetString());
            Assert.Equal("complete", root.GetProperty("status").GetString());
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
            var result = root.GetProperty("result");
            Assert.Equal(
                [
                    "mode", "scaffold", "target", "plan", "framework", "entrypoints",
                    "effects", "unchangedPaths", "lifecycle", "recovery", "verification", "findings",
                ],
                result.EnumerateObject().Select(property => property.Name));
            Assert.Equal("dry-run", result.GetProperty("mode").GetString());
            Assert.Equal("generic", result.GetProperty("scaffold").GetString());
            Assert.Equal(TargetPath, result.GetProperty("target").GetProperty("path").GetString());
            Assert.Equal("complete", result.GetProperty("plan").GetProperty("completeness").GetString());
            Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
            Assert.Equal(JsonValueKind.Null, result.GetProperty("framework").ValueKind);
            Assert.Equal(
                [".agents", ".agents/docs", TargetPath],
                result.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("path").GetString()));
            var metadata = result.GetProperty("entrypoints")[0].GetProperty("metadata");
            Assert.Equal("Project documents", metadata.GetProperty("description").GetString());
            Assert.Equal(["Documentation"], metadata.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
            Assert.Empty(result.GetProperty("findings").EnumerateArray());
        }

        workspace.AssertNoLockInfrastructure();
        var application = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            [
                "route", "init", TargetId,
                "--description", "Project documents",
                "--tag=Documentation",
                "--json",
                "--workspace", workspace.Path,
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(0, application.ExitCode);
        Assert.Equal(string.Empty, application.StandardError);
        Assert.DoesNotContain("Apply this", application.StandardOutput, StringComparison.Ordinal);
        using (var document = JsonDocument.Parse(application.StandardOutput))
        {
            var root = document.RootElement;
            Assert.Equal("complete", root.GetProperty("status").GetString());
            var result = root.GetProperty("result");
            Assert.Equal("apply", result.GetProperty("mode").GetString());
            Assert.All(
                result.GetProperty("effects").EnumerateArray(),
                effect => Assert.Equal("verified", effect.GetProperty("outcome").GetString()));
            Assert.Equal("verified", result.GetProperty("verification").GetString());
            Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
            var metadata = result.GetProperty("entrypoints")[0].GetProperty("metadata");
            Assert.Equal(["Documentation"], metadata.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
        }

        Assert.True(File.Exists(workspace.Combine(TargetPath)));
        var scaffold = await workspace.ReadTextAsync(TargetPath, TestContext.Current.CancellationToken);
        Assert.Contains("open-forge:", scaffold, StringComparison.Ordinal);
        Assert.Contains("# docs", scaffold, StringComparison.Ordinal);
        Assert.DoesNotContain("rune:", scaffold, StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();

        var noOp = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", TargetId, "--view=compact", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("The route is initialized.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply this", noOp.StandardOutput, StringComparison.Ordinal);
    }
}

public sealed class PublishedRouteInitFrameworkProcessTests
{
    private const string RequestedTarget = "memory/Mobile App/crystallized/documents";
    private const string ScopePath = PublishedRouteInitWorkspace.FrameworkScopePath;
    private const string ManagedPath = PublishedRouteInitWorkspace.FrameworkManagedPath;
    private const string FinalPath = PublishedRouteInitWorkspace.FrameworkFinalPath;

    [Fact(DisplayName = "Published Framework Route Init reuses a fresh Install and converges one scoped sparse chain"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task FrameworkDryRunApplyAndRepeatNoOpPreserveOwnership()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteInitWorkspace.CreateFramework();
        var install = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["install", "--automatic", "--json", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);

        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
        workspace.AssertPersistentExternalLock();
        string[] routeArguments =
        [
            "route", "init", RequestedTarget,
            "--framework",
            "--json",
            "--workspace", workspace.Path,
        ];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. routeArguments, "--dry-run"],
            workspace.ProcessEnvironment);

        Assert.Equal(2, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using (var document = JsonDocument.Parse(preview.StandardOutput))
        {
            var root = document.RootElement;
            Assert.Equal("attention", root.GetProperty("status").GetString());
            var result = root.GetProperty("result");
            Assert.Equal("dry-run", result.GetProperty("mode").GetString());
            Assert.Equal("framework", result.GetProperty("scaffold").GetString());
            Assert.Equal("memory/mobile-app/crystallized/documents", result.GetProperty("target").GetProperty("id").GetString());
            Assert.Equal(
                ["installed-root", "scope", "managed", "managed"],
                result.GetProperty("framework").GetProperty("segments").EnumerateArray()
                    .Select(segment => segment.GetProperty("role").GetString()));
            Assert.Equal(
                "route-init.needs-authoring",
                result.GetProperty("findings")[0].GetProperty("code").GetString());
            Assert.Equal("publish", result.GetProperty("lifecycle").GetProperty("action").GetString());
            Assert.Equal("planned", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
        }

        var application = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            routeArguments,
            workspace.ProcessEnvironment);

        Assert.Equal(2, application.ExitCode);
        Assert.Equal(string.Empty, application.StandardError);
        Assert.DoesNotContain("Apply this", application.StandardOutput, StringComparison.Ordinal);
        using (var document = JsonDocument.Parse(application.StandardOutput))
        {
            var result = document.RootElement.GetProperty("result");
            Assert.Equal("verified", result.GetProperty("verification").GetString());
            Assert.Equal("verified", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
            Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
            var entrypoints = result.GetProperty("entrypoints").EnumerateArray().ToArray();
            var scope = Assert.Single(entrypoints, entrypoint => entrypoint.GetProperty("path").GetString() == ScopePath);
            Assert.Equal("user", scope.GetProperty("ownership").GetString());
            Assert.Equal(JsonValueKind.Null, scope.GetProperty("sourceAssetPath").ValueKind);
            Assert.Contains(
                "NeedsAuthoring",
                scope.GetProperty("metadata").GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
            Assert.All(
                entrypoints.Where(entrypoint => entrypoint.GetProperty("ownership").GetString() == "framework"),
                entrypoint => Assert.Equal(JsonValueKind.String, entrypoint.GetProperty("sourceAssetPath").ValueKind));
        }

        Assert.True(File.Exists(workspace.Combine(ScopePath)));
        Assert.True(File.Exists(workspace.Combine(ManagedPath)));
        Assert.True(File.Exists(workspace.Combine(FinalPath)));
        using (var lifecycle = JsonDocument.Parse(await workspace.ReadTextAsync(
                   ".agents/open-forge.lifecycle.json",
                   TestContext.Current.CancellationToken)))
        {
            var root = lifecycle.RootElement;
            Assert.Equal(
                ["schemaVersion", "fingerprintPolicy", "workspacePath", "framework", "extensions"],
                root.EnumerateObject().Select(property => property.Name));
            var extensions = root.GetProperty("extensions");
            Assert.Equal("complete", extensions.GetProperty("coverage").GetString());
            Assert.Empty(extensions.GetProperty("packages").EnumerateArray());
            Assert.Empty(extensions.GetProperty("paths").EnumerateArray());
            var lifecycleTargets = root.GetProperty("framework").GetProperty("targets").EnumerateArray().ToArray();
            Assert.DoesNotContain(lifecycleTargets, lifecycleTarget =>
                lifecycleTarget.GetProperty("path").GetString() == ScopePath);
            Assert.Contains(lifecycleTargets, lifecycleTarget =>
                lifecycleTarget.GetProperty("path").GetString() == ManagedPath
                && lifecycleTarget.GetProperty("sourceAssetPath").GetString() == ".agents/memory/crystallized/_crystallized.md");
            Assert.Contains(lifecycleTargets, lifecycleTarget =>
                lifecycleTarget.GetProperty("path").GetString() == FinalPath
                && lifecycleTarget.GetProperty("sourceAssetPath").GetString() == ".agents/memory/crystallized/documents/_documents.md");
        }

        var noOp = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            routeArguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var noOpRoot = noOpDocument.RootElement;
        Assert.Equal("complete", noOpRoot.GetProperty("status").GetString());
        var noOpResult = noOpRoot.GetProperty("result");
        Assert.Empty(noOpResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", noOpResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("already-current", noOpResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Empty(noOpResult.GetProperty("findings").EnumerateArray());
    }
}

public sealed class PublishedRouteInitStatusProcessTests
{
    [Fact(DisplayName = "Published Route Init reaches every deterministic semantic stream and exit without writes"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task DeterministicPublicStatusesUseTheirSharedProcessDisposition()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var attention = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", "draft", "--dry-run", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);
        var invalid = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", "loader", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);
        var blocked = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", "memory/crystallized/documents", "--framework", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);

        Assert.Equal(2, attention.ExitCode);
        Assert.Contains("Status: requires attention", attention.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, attention.StandardError);
        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.StandardOutput);
        Assert.Contains("Status: blocked", blocked.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "establish a trusted current Framework installation",
            blocked.StandardError,
            StringComparison.OrdinalIgnoreCase);
        workspace.AssertNoLockInfrastructure();

        using var incompleteWorkspace = PublishedRouteInitWorkspace.CreateMetadataIncomplete();
        var incomplete = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            incompleteWorkspace.Path,
            incompleteWorkspace.SnapshotState,
            ["route", "init", "root", "--dry-run"],
            incompleteWorkspace.ProcessEnvironment);

        Assert.Equal(3, incomplete.ExitCode);
        Assert.Contains("Status: incomplete", incomplete.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "inspect the unavailable route, metadata, projection, lifecycle, or recovery facts",
            incomplete.StandardOutput,
            StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, incomplete.StandardError);
    }

    [Fact(DisplayName = "Published Route Init rejects automatic mode without prompting or writing"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task AutomaticIsNotACommandMode()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "init", "docs", "--automatic", "--workspace", workspace.Path],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Unrecognized command or argument '--automatic'.", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply this", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }
}

public sealed class PublishedRouteInitNeighborProcessTests
{
    [Fact(DisplayName = "Published Route List and Inspect remain stable beside Route Init composition"),
     Trait("Feature", "route-init"), Trait("Evidence", "EndToEnd")]
    public async Task NeighboringRouteCommandsRemainReadOnlyAndReachable()
    {
        var target = PublishedExecutableTarget.Discover();
        using var listWorkspace = PublishedRouteWorkspace.CreateComplete();
        var list = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            listWorkspace.Path,
            listWorkspace.SnapshotHashes,
            ["route", "list", "root", "--depth=0", "--json"]);
        using var inspectWorkspace = PublishedRouteInspectWorkspace.CreateComplete();
        var inspect = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            inspectWorkspace.Path,
            inspectWorkspace.SnapshotHashes,
            ["route", "inspect", "root", "--json"]);

        Assert.Equal(0, list.ExitCode);
        Assert.Equal(string.Empty, list.StandardError);
        using (var document = JsonDocument.Parse(list.StandardOutput))
        {
            Assert.Equal("route list", document.RootElement.GetProperty("command").GetString());
            Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        }

        Assert.Equal(0, inspect.ExitCode);
        Assert.Equal(string.Empty, inspect.StandardError);
        using var inspectDocument = JsonDocument.Parse(inspect.StandardOutput);
        Assert.Equal("route inspect", inspectDocument.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", inspectDocument.RootElement.GetProperty("status").GetString());
    }
}
