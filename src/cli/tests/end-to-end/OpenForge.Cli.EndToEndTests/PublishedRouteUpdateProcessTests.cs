using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedRouteUpdateHelpProcessTests
{
    [Fact(DisplayName = "Published Route Update is discoverable with exact grammar policy and all shared exits")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task HelpExposesCompletePublicBoundary()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var root = await RunWithoutWritesAsync(target, workspace, ["--help"]);
        var group = await RunWithoutWritesAsync(target, workspace, ["route", "--help"]);
        var leaf = await RunWithoutWritesAsync(target, workspace, ["route", "update", "--help"]);

        Assert.All([root, group, leaf], result =>
        {
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
        });
        Assert.Contains("route update", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("update <source-reference>", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route update <source-reference>",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("--description <text>", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--responsibility <text>", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--tag=<tag>", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--template <template-reference>", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("--dry-run", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", leaf.StandardOutput, StringComparison.Ordinal);
        AssertExitMapping(leaf.StandardOutput, "complete", 0, "stdout");
        AssertExitMapping(leaf.StandardOutput, "attention", 2, "stdout");
        AssertExitMapping(leaf.StandardOutput, "incomplete", 3, "stdout");
        AssertExitMapping(leaf.StandardOutput, "invalid", 4, "stderr");
        AssertExitMapping(leaf.StandardOutput, "blocked", 5, "stderr");

        // Failed and interrupted require a real filesystem fault or caller signal.
        // Their public mappings are frozen without adding a product fault seam.
        AssertExitMapping(leaf.StandardOutput, "failed", 1, "stderr");
        AssertExitMapping(leaf.StandardOutput, "interrupted", 130, "stderr");
        workspace.AssertNoLockInfrastructure();
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteUpdateWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertExitMapping(
        string help,
        string status,
        int exitCode,
        string stream)
        => Assert.Contains(
            $"{status}: exit {exitCode} and human {stream}.",
            help,
            StringComparison.Ordinal);
}

public sealed class PublishedRouteUpdateJourneyProcessTests
{
    [Fact(DisplayName = "Published Route Update JSON dry-run is ordered nullable write-free and verbose-stable")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task JsonDryRunFreezesSchemaAndDiagnosticSeparation()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
            "--dry-run",
            "--json",
        ];
        var preview = await RunWithoutWritesAsync(target, workspace, arguments);
        var verbose = await RunWithoutWritesAsync(target, workspace, [.. arguments, "--verbose"]);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Equal(preview.ExitCode, verbose.ExitCode);
        Assert.Equal(preview.StandardOutput, verbose.StandardOutput);
        Assert.InRange(verbose.StandardError.Length, 1, 4096);
        Assert.Contains("status=complete; mode=dry-run", verbose.StandardError, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(preview.StandardOutput);
        var root = document.RootElement;
        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route update", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        AssertOrder(
            result,
            "mode", "target", "patch", "template", "plan", "effects",
            "unchangedPaths", "recovery", "verification", "findings");
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("template").ValueKind);
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
        {
            AssertOrder(effect.GetProperty("change"), "before", "expected");
            Assert.Equal(JsonValueKind.String, effect.GetProperty("change").GetProperty("before").ValueKind);
            Assert.Equal(JsonValueKind.String, effect.GetProperty("change").GetProperty("expected").ValueKind);
            Assert.Equal("planned", effect.GetProperty("outcome").GetString());
        });
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published Route Update applies exact base and navigation bytes then converges")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task ApplyThenNoOpIsOneExactJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        var targetBefore = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentBefore = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        string[] arguments =
        [
            "route", "update", PublishedRouteUpdateWorkspace.TargetId,
            "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
            "--tag=After",
            "--tag=Memory",
        ];

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        var targetAfter = await workspace.ReadTargetAsync(TestContext.Current.CancellationToken);
        var parentAfter = await workspace.ReadParentAsync(TestContext.Current.CancellationToken);
        Assert.Equal(
            targetBefore.Replace("Before overview", "After overview", StringComparison.Ordinal)
                .Replace("[Before, Memory]", "[After, Memory]", StringComparison.Ordinal),
            targetAfter);
        Assert.Equal(
            parentBefore.Replace(
                "- [Before overview](overview.md) - #Before #Memory",
                "- [After overview](overview.md) - #After #Memory",
                StringComparison.Ordinal),
            parentAfter);
        workspace.AssertPersistentExternalLock();
        var after = workspace.SnapshotState();

        var noOp = await RunWithoutWritesAsync(target, workspace, arguments);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("No files changed.", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(after, workspace.SnapshotState());
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteUpdateWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}

public sealed class PublishedRouteUpdateStatusProcessTests
{
    [Fact(DisplayName = "Published Route Update accepts only attached-empty responsibility removal")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task ResponsibilityRemovalRequiresAttachedEmptySpelling()
    {
        var target = PublishedExecutableTarget.Discover();
        foreach (var option in new[] { "--responsibility=", "--responsibility:" })
        {
            using var workspace = PublishedRouteUpdateWorkspace.Create();
            var result = await PublishedProcessTestSupport.RunAsync(
                target,
                workspace.Path,
                [
                    "route", "update", PublishedRouteUpdateWorkspace.TargetId, option,
                ],
                workspace.ProcessEnvironment);

            Assert.Equal(0, result.ExitCode);
            Assert.Equal(string.Empty, result.StandardError);
            Assert.DoesNotContain(
                "responsibility:",
                await workspace.ReadTargetAsync(TestContext.Current.CancellationToken),
                StringComparison.Ordinal);
        }

        using var bareWorkspace = PublishedRouteUpdateWorkspace.Create();
        var bare = await RunWithoutWritesAsync(
            target,
            bareWorkspace,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId, "--responsibility",
            ]);
        AssertDisposition(bare, 4, "invalid", standardError: true);
        Assert.Contains(
            "--responsibility accepts exactly one value.",
            bare.StandardError,
            StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Route Update deterministic statuses use exact streams exits and zero-write policy")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task DeterministicStatusesHaveSharedProcessDisposition()
    {
        var target = PublishedExecutableTarget.Discover();
        using var attentionWorkspace = PublishedRouteUpdateWorkspace.Create();
        var attention = await RunWithoutWritesAsync(
            target,
            attentionWorkspace,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--template", PublishedRouteUpdateWorkspace.TemplateId,
                "--dry-run",
            ]);
        using var missingTemplateWorkspace = PublishedRouteUpdateWorkspace.Create();
        missingTemplateWorkspace.RemoveTemplate();
        var missingTemplate = await RunWithoutWritesAsync(
            target,
            missingTemplateWorkspace,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--template", PublishedRouteUpdateWorkspace.TemplateId,
                "--dry-run",
            ]);
        using var incompleteWorkspace = PublishedRouteUpdateWorkspace.Create();
        incompleteWorkspace.RemoveAgentsRoot();
        var incomplete = await RunWithoutWritesAsync(
            target,
            incompleteWorkspace,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
                "--dry-run",
            ]);
        using var invalidWorkspace = PublishedRouteUpdateWorkspace.Create();
        var invalid = await RunWithoutWritesAsync(
            target,
            invalidWorkspace,
            ["route", "update", PublishedRouteUpdateWorkspace.TargetId]);
        using var blockedWorkspace = PublishedRouteUpdateWorkspace.Create();
        blockedWorkspace.SeedAmbiguousTarget();
        var blocked = await RunWithoutWritesAsync(
            target,
            blockedWorkspace,
            [
                "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
                "--dry-run",
            ]);

        AssertDisposition(attention, 2, "attention", standardError: false);
        AssertDisposition(missingTemplate, 4, "invalid", standardError: true);
        AssertDisposition(incomplete, 3, "incomplete", standardError: false);
        AssertDisposition(invalid, 4, "invalid", standardError: true);
        AssertDisposition(blocked, 5, "blocked", standardError: true);
    }

    [Fact(DisplayName = "Published Route Update reports a real post-write verification failure on stderr")]
    [Trait("Feature", "route-update"), Trait("Evidence", "EndToEnd")]
    public async Task PostWriteVerificationFailureHasFailedProcessDisposition()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteUpdateWorkspace.Create();
        workspace.SeedVerificationWindow();
        using var monitorStop = new CancellationTokenSource();
        using var monitorReady = new ManualResetEventSlim();
        var monitor = MonitorAppliedParentAsync(
            workspace,
            monitorReady,
            monitorStop.Token);
        Assert.True(
            monitorReady.Wait(
                TimeSpan.FromSeconds(5),
                TestContext.Current.CancellationToken),
            "The process monitor must be polling before the native process starts.");
        ProcessRunResult result;
        try
        {
            result = await PublishedProcessTestSupport.RunAsync(
                target,
                workspace.Path,
                [
                    "route", "update", PublishedRouteUpdateWorkspace.TargetId,
                    "--description", PublishedRouteUpdateWorkspace.ExpectedDescription,
                    "--tag=After",
                    "--tag=Memory",
                    "--view=expanded",
                ],
                workspace.ProcessEnvironment);
        }
        finally
        {
            monitorStop.Cancel();
        }

        var recoveryArtifact = Assert.Single(workspace.RemoveRetainedRecoveryArtifacts());
        Assert.True(await monitor, "The process monitor must observe the applied parent effect.");
        Assert.Equal(1, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains(
            $"Route Update did not update {PublishedRouteUpdateWorkspace.TargetId}",
            result.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("Status: failed", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Before:", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Expected:", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Recovery: retained", result.StandardError, StringComparison.Ordinal);
        Assert.Contains(recoveryArtifact, result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Verification: failed", result.StandardError, StringComparison.Ordinal);
        Assert.True(
            result.StandardError.Contains(
                "The current file state no longer matches the planned expectation.",
                StringComparison.Ordinal)
            || result.StandardError.Contains(
                "The final Route Update state did not rebuild as the exact complete safe no-op.",
                StringComparison.Ordinal),
            "The failed process must report the exact receipt or rebuilt-state verification cause.");
        Assert.Contains(
            "Next: open-forge route update --verbose",
            result.StandardError,
            StringComparison.Ordinal);
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteUpdateWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            arguments,
            workspace.ProcessEnvironment);

    private static void AssertDisposition(
        ProcessRunResult result,
        int exitCode,
        string status,
        bool standardError)
    {
        Assert.Equal(exitCode, result.ExitCode);
        var expected = standardError ? result.StandardError : result.StandardOutput;
        var empty = standardError ? result.StandardOutput : result.StandardError;
        var humanStatus = status == "attention" ? "requires attention" : status;
        Assert.Equal(string.Empty, empty);
        Assert.Contains($"Status: {humanStatus}", expected, StringComparison.Ordinal);
    }

    private static Task<bool> MonitorAppliedParentAsync(
        PublishedRouteUpdateWorkspace workspace,
        ManualResetEventSlim ready,
        CancellationToken cancellationToken)
        => Task.Factory.StartNew(
            () =>
            {
                ready.Set();
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var parent = workspace.ReadParent();
                        if (parent.Contains(
                                PublishedRouteUpdateWorkspace.ExpectedDescription,
                                StringComparison.Ordinal))
                        {
                            workspace.MutateTargetAfterApplication();
                            return true;
                        }
                    }
                    catch (IOException)
                    {
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    Thread.Yield();
                }

                return false;
            },
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
}
