using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteCreateProcessTests
{
    [Fact(DisplayName = "Published Route group and Create leaf help bypass workspace and lock effects"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task GroupAndLeafHelpAreTruthfulNoWriteTerminalModes()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        var missingWorkspace = workspace.Combine("missing-workspace");
        var group = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route"],
            workspace.ProcessEnvironment);
        var leaf = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["route", "create", "--help", "--workspace", missingWorkspace],
            workspace.ProcessEnvironment);

        Assert.All([group, leaf], result =>
        {
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
        });
        Assert.Contains("Commands:", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("list <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("init <route-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("create <file-target>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("update <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("list     available", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("The route group performs no operation.", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "Planned but unavailable operations: move and remove.",
            group.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route create <file-target> --description <text> --tag=<tag>...",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Metadata", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Template", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Write policy", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missingWorkspace));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Create JSON dry-run is exact write-free and verbose-stable"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunPreservesOrderedEnvelopeEffectsAndVerboseParity()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        string[] arguments =
        [
            "route", "create", PublishedRouteCreateWorkspace.TargetId,
            "--description", PublishedRouteCreateWorkspace.Description,
            "--tag=Docs",
            "--tag=Overview",
            "--responsibility", PublishedRouteCreateWorkspace.Responsibility,
            "--dry-run",
            "--json",
        ];
        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);
        var verbose = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [.. arguments, "--verbose"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Equal(preview.ExitCode, verbose.ExitCode);
        Assert.Equal(preview.StandardOutput, verbose.StandardOutput);
        var diagnostic = Assert.Single(
            verbose.StandardError.Split(
                Environment.NewLine,
                StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4095);
        Assert.Contains("status=complete; mode=dry-run", diagnostic, StringComparison.Ordinal);
        Assert.Contains($"target={PublishedRouteCreateWorkspace.TargetId}", diagnostic, StringComparison.Ordinal);
        Assert.Contains("effects=2", diagnostic, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(preview.StandardOutput);
        var root = document.RootElement;
        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route create", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(
            "current-directory",
            root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        AssertPropertyOrder(
            result,
            "mode",
            "target",
            "parent",
            "metadata",
            "template",
            "plan",
            "effects",
            "unchangedPaths",
            "recovery",
            "verification",
            "findings");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.TargetId,
            result.GetProperty("target").GetProperty("id").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.TargetPath,
            result.GetProperty("target").GetProperty("path").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.ParentId,
            result.GetProperty("parent").GetProperty("id").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.ParentPath,
            result.GetProperty("parent").GetProperty("path").GetString());
        Assert.Equal("canonical", result.GetProperty("parent").GetProperty("form").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.Description,
            result.GetProperty("metadata").GetProperty("description").GetString());
        Assert.Equal(
            PublishedRouteCreateWorkspace.Responsibility,
            result.GetProperty("metadata").GetProperty("responsibility").GetString());
        Assert.Equal(
            ["Docs", "Overview"],
            result.GetProperty("metadata").GetProperty("tags").EnumerateArray()
                .Select(tag => tag.GetString()));
        Assert.Equal(JsonValueKind.Null, result.GetProperty("template").ValueKind);
        Assert.Equal("complete", result.GetProperty("plan").GetProperty("completeness").GetString());
        Assert.Equal("safe", result.GetProperty("plan").GetProperty("safety").GetString());
        var effects = result.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(2, effects.Length);
        var targetEffect = AssertEffect(
            effects[0],
            PublishedRouteCreateWorkspace.TargetPath,
            "routed-file",
            "create");
        var targetChange = targetEffect.GetProperty("change");
        Assert.Equal(JsonValueKind.Null, targetChange.GetProperty("before").ValueKind);
        Assert.Equal(
            PublishedRouteCreateWorkspace.ExpectedTargetHash,
            targetChange.GetProperty("expected").GetString());
        var parentEffect = AssertEffect(
            effects[1],
            PublishedRouteCreateWorkspace.ParentPath,
            "generated-region",
            "replace");
        var parentChange = parentEffect.GetProperty("change");
        Assert.Equal(
            workspace.InitialParentHash,
            parentChange.GetProperty("before").GetString());
        Assert.Equal(
            workspace.ExpectedParentHash,
            parentChange.GetProperty("expected").GetString());
        Assert.Equal(
            [".agents/loader.md"],
            result.GetProperty("unchangedPaths").EnumerateArray()
                .Select(path => path.GetString()));
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("not-requested", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Create applies exact bytes then repeats as a verified no-op"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyCreatesExactTargetUpdatesOnlyParentInteriorAndConverges()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        workspace.OwnApplicationCreatedTarget();
        var before = workspace.SnapshotState();
        var parentBefore = await workspace.ReadParentAsync(
            TestContext.Current.CancellationToken);
        string[] arguments =
        [
            "route", "create", PublishedRouteCreateWorkspace.TargetId,
            "--description", PublishedRouteCreateWorkspace.Description,
            "--tag=Docs",
            "--tag=Overview",
            "--responsibility", PublishedRouteCreateWorkspace.Responsibility,
        ];

        var application = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, application.ExitCode);
        Assert.Equal(string.Empty, application.StandardError);
        Assert.Contains("The routed file was created.", application.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", application.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            $"{PublishedRouteCreateWorkspace.TargetPath}: create routed-file / verified",
            application.StandardOutput,
            StringComparison.Ordinal);
        workspace.AssertOrdinaryTargetFile();
        Assert.Equal(
            PublishedRouteCreateWorkspace.ExpectedTargetDocument,
            await workspace.ReadTargetAsync(TestContext.Current.CancellationToken));
        Assert.Equal(
            Encoding.UTF8.GetBytes(PublishedRouteCreateWorkspace.ExpectedTargetDocument),
            await workspace.ReadTargetBytesAsync(TestContext.Current.CancellationToken));
        var parentAfter = await workspace.ReadParentAsync(
            TestContext.Current.CancellationToken);
        Assert.Equal(workspace.InitialParentDocument, parentBefore);
        Assert.Equal(workspace.ExpectedParentDocument, parentAfter);
        Assert.Equal(
            parentBefore.Replace(
                PublishedRouteCreateWorkspace.InitialEntry,
                PublishedRouteCreateWorkspace.ExpectedEntry,
                StringComparison.Ordinal),
            parentAfter);
        Assert.Equal(
            Encoding.UTF8.GetBytes(workspace.ExpectedParentDocument),
            await workspace.ReadParentBytesAsync(TestContext.Current.CancellationToken));
        var after = workspace.SnapshotState();
        Assert.Equal(before.Count + 1, after.Count);
        Assert.Equal(before[".agents/loader.md"], after[".agents/loader.md"]);
        Assert.NotEqual(before[PublishedRouteCreateWorkspace.ParentPath], after[PublishedRouteCreateWorkspace.ParentPath]);
        Assert.Contains(PublishedRouteCreateWorkspace.TargetPath, after.Keys);
        workspace.AssertPersistentExternalLock();

        var noOp = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains(
            "The routed file already matches the requested content.",
            noOp.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Status: complete", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();
    }

    [Fact(DisplayName = "Published Route Create invalid input uses invalid stderr exit without writes"), Trait("Feature", "route-create"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidMetadataUsesSharedStatusStreamAndExitWithoutWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteCreateWorkspace.Create();
        var invalid = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            [
                "route", "create", PublishedRouteCreateWorkspace.TargetId,
                "--tag=Docs",
            ],
            workspace.ProcessEnvironment);

        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("The routed file was not created.", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "requires one nonblank --description value",
            invalid.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route create --help", invalid.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    private static JsonElement AssertEffect(
        JsonElement effect,
        string path,
        string kind,
        string action)
    {
        Assert.Equal(path, effect.GetProperty("path").GetString());
        Assert.Equal(kind, effect.GetProperty("kind").GetString());
        Assert.Equal(action, effect.GetProperty("action").GetString());
        AssertPropertyOrder(effect.GetProperty("change"), "before", "expected");
        Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        Assert.Equal("none", effect.GetProperty("residual").GetString());
        return effect;
    }

    private static void AssertPropertyOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
