using System.Text.Json;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class CliProcessTests
{
    [Fact(DisplayName = "Published version is exact and workspace independent")]
    [Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedVersionIsExactAndWorkspaceIndependent()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-version-working");
        using var other = TemporaryWorkspace.Create("e2e-version-other");
        var missingWorkspace = working.Combine("missing-workspace");
        var request = new ProcessRunRequest(
            environment.ExecutablePath,
            ["--workspace", missingWorkspace, "--json", "--version"],
            other.Path,
            timeout: TimeSpan.FromSeconds(30));

        var result = await ProcessRunner.RunAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(environment.ExpectedVersion + Environment.NewLine, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published root and route-family help expose available List and Inspect commands")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootAndRouteFamilyHelpExposeAvailableCommands()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-help");

        var root = await RunWithoutWritesAsync(environment, working.Path, working.SnapshotHashes, []);
        var help = await RunWithoutWritesAsync(environment, working.Path, working.SnapshotHashes, ["--help"]);
        var group = await RunWithoutWritesAsync(environment, working.Path, working.SnapshotHashes, ["route"]);
        var leaf = await RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "list", "--help"]);
        var inspectLeaf = await RunWithoutWritesAsync(
            environment,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "--help"]);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(0, inspectLeaf.ExitCode);
        Assert.Equal(root.StandardOutput, help.StandardOutput);
        Assert.Contains("Open Forge CLI (`open-forge`)", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Discovery:", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route list", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route inspect", root.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("list     available", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("inspect  available", group.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route list [source-reference] [--depth=<non-negative-integer|all>]",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory --depth=all", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "open-forge route inspect <source-reference>",
            inspectLeaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Equal(string.Empty, help.StandardError);
        Assert.Equal(string.Empty, group.StandardError);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Equal(string.Empty, inspectLeaf.StandardError);
    }

    [Fact(DisplayName = "Published route list emits one structured result without mutating the workspace")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListEmitsStructuredReadOnlyResult()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            environment,
            working.Path,
            ["route", "list", "root", "--depth=0", "--json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("route list", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var row = Assert.Single(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
        Assert.Equal("root", row.GetProperty("id").GetString());
        Assert.Equal("Root", row.GetProperty("tags")[0].GetString());
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route-list preserves native global delimiters"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListPreservesGlobalDelimiterParity()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var workspaceForms = new (string Name, string[] Arguments)[]
        {
            ("workspace-spaced", ["--workspace", working.Path]),
            ("workspace-equals", [$"--workspace={working.Path}"]),
            ("workspace-colon", [$"--workspace:{working.Path}"]),
        };
        var viewForms = new (string Name, string[] Arguments)[]
        {
            ("view-spaced", ["--view", "compact"]),
            ("view-equals", ["--view=compact"]),
            ("view-colon", ["--view:compact"]),
        };

        var baseline = await RunAsync(
            environment,
            working.Path,
            [
                "route", "list", "root", "--workspace", working.Path,
                "--json", "--depth=0", "--view=compact",
            ]);
        Assert.Equal(0, baseline.ExitCode);
        Assert.Equal(string.Empty, baseline.StandardError);
        using (var baselineDocument = JsonDocument.Parse(baseline.StandardOutput))
        {
            Assert.Equal("complete", baselineDocument.RootElement.GetProperty("status").GetString());
            var row = Assert.Single(
                baselineDocument.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
            Assert.Equal("root", row.GetProperty("id").GetString());
        }

        foreach (var workspaceForm in workspaceForms)
        {
            foreach (var viewForm in viewForms)
            {
                var arguments = new List<string> { "route", "list", "root" };
                arguments.AddRange(workspaceForm.Arguments);
                arguments.Add("--json");
                arguments.Add("--depth=0");
                arguments.AddRange(viewForm.Arguments);
                var result = await RunAsync(environment, working.Path, arguments);
                var form = $"{workspaceForm.Name}, {viewForm.Name}";

                Assert.True(
                    result.ExitCode == baseline.ExitCode,
                    $"{form}: expected exit {baseline.ExitCode}, actual {result.ExitCode}.");
                Assert.True(result.StandardOutput == baseline.StandardOutput, $"{form}: standard output differed.");
                Assert.True(result.StandardError == baseline.StandardError, $"{form}: standard error differed.");
            }
        }

        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route-list accepts an option-like source after the terminator"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListAcceptsOptionLikeSourceAfterTerminator()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            environment,
            working.Path,
            ["route", "list", "--workspace", working.Path, "--json", "--", "--depth"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var invalidDocument = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("route list", invalidDocument.RootElement.GetProperty("command").GetString());
        Assert.Equal("invalid", invalidDocument.RootElement.GetProperty("status").GetString());
        var findings = invalidDocument.RootElement.GetProperty("result").GetProperty("findings");
        var finding = Assert.Single(findings.EnumerateArray());
        Assert.Equal("route-list.unknown-source", finding.GetProperty("code").GetString());
        Assert.Equal("--depth", finding.GetProperty("subject").GetString());
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route list preserves root path depth view overwrite and detached semantics")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListPreservesPublicSelectionDepthAndViews()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var defaults = await RunAsync(environment, working.Path, ["route", "list"]);
        var exactPath = await RunAsync(
            environment,
            working.Path,
            ["route", "list", ".agents/root/_root.md", "--depth=0", "--view=compact"]);
        var allCompactJson = await RunAsync(
            environment,
            working.Path,
            ["route", "list", "root", "--depth=all", "--view=compact", "--json"]);
        var allExpandedJson = await RunAsync(
            environment,
            working.Path,
            ["route", "list", "root", "--depth=all", "--view=expanded", "--json"]);
        var overwrite = await RunAsync(
            environment,
            working.Path,
            ["route", "list", ".agents/root/adjusted.overwrite.md", "--depth=0", "--json"]);
        var detached = await RunAsync(
            environment,
            working.Path,
            ["route", "list", "detached", "--depth=all", "--json"]);

        Assert.Equal(0, defaults.ExitCode);
        Assert.Equal(string.Empty, defaults.StandardError);
        Assert.Contains("Result: complete", defaults.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Requested depth: 1", defaults.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("ID: workspace-defined", defaults.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("ID: detached", defaults.StandardOutput, StringComparison.Ordinal);

        Assert.Equal(0, exactPath.ExitCode);
        Assert.Equal(string.Empty, exactPath.StandardError);
        Assert.Contains("result=complete  coverage=complete", exactPath.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("requestedDepth=0 effectiveDepth=0", exactPath.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("root  .agents/root/_root.md", exactPath.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("root/child", exactPath.StandardOutput, StringComparison.Ordinal);

        Assert.Equal(0, allCompactJson.ExitCode);
        Assert.Equal(string.Empty, allCompactJson.StandardError);
        Assert.Equal(allCompactJson.StandardOutput, allExpandedJson.StandardOutput);
        using (var document = JsonDocument.Parse(allCompactJson.StandardOutput))
        {
            var result = document.RootElement.GetProperty("result");
            Assert.Equal("all", result.GetProperty("requestedDepth").GetString());
            Assert.Equal("all", result.GetProperty("effectiveDepth").GetString());
            Assert.Equal(
                ["root", "root/adjusted", "root/child", "root/native", "root/nested", "root/nested/deep"],
                result.GetProperty("rows").EnumerateArray().Select(row => row.GetProperty("id").GetString()));
            Assert.Equal("routed-native", result.GetProperty("rows")[3].GetProperty("provenance").GetProperty("source").GetString());
        }

        Assert.Equal(0, overwrite.ExitCode);
        using (var document = JsonDocument.Parse(overwrite.StandardOutput))
        {
            var row = Assert.Single(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
            Assert.Equal(".agents/root/adjusted.md", row.GetProperty("path").GetString());
            Assert.True(row.GetProperty("provenance").GetProperty("hasOverwrite").GetBoolean());
        }

        Assert.Equal(0, detached.ExitCode);
        using (var document = JsonDocument.Parse(detached.StandardOutput))
        {
            var rows = document.RootElement.GetProperty("result").GetProperty("rows");
            Assert.Equal(["detached", "detached/leaf"], rows.EnumerateArray().Select(row => row.GetProperty("id").GetString()));
            Assert.Equal(JsonValueKind.Null, rows[0].GetProperty("absoluteDepth").ValueKind);
            Assert.Equal("detached-root", rows[0].GetProperty("provenance").GetProperty("selection").GetString());
        }

        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route list keeps diagnostics failures streams exits and no-write behavior typed")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListKeepsDiagnosticsAndFailureStreamsTyped()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var complete = PublishedRouteWorkspace.CreateComplete();
        using var attention = PublishedRouteWorkspace.CreateAttention();
        using var incomplete = PublishedRouteWorkspace.CreateIncomplete();
        var completeBefore = complete.SnapshotHashes();
        var attentionBefore = attention.SnapshotHashes();
        var incompleteBefore = incomplete.SnapshotHashes();

        var plainJson = await RunAsync(
            environment,
            complete.Path,
            ["route", "list", "root", "--depth=0", "--json"]);
        var verboseJson = await RunAsync(
            environment,
            complete.Path,
            ["route", "list", "root", "--depth=0", "--json", "--verbose"]);
        var invalidDepth = await RunAsync(
            environment,
            complete.Path,
            ["route", "list", "--depth=-1", "--json"]);
        var emptyDepth = await RunAsync(
            environment,
            complete.Path,
            ["route", "list", "--depth=", "--json"]);
        var attentionHuman = await RunAsync(
            environment,
            attention.Path,
            ["route", "list", "root", "--depth=all", "--view=expanded"]);
        var incompleteHuman = await RunAsync(
            environment,
            incomplete.Path,
            ["route", "list", "root", "--depth=all", "--view=compact"]);

        Assert.Equal(0, verboseJson.ExitCode);
        Assert.Equal(plainJson.StandardOutput, verboseJson.StandardOutput);
        Assert.InRange(verboseJson.StandardError.Length, 1, 4096);
        Assert.Contains("selection.kind=source-id", verboseJson.StandardError, StringComparison.Ordinal);
        Assert.Contains("rows=1", verboseJson.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Root route", verboseJson.StandardError, StringComparison.Ordinal);

        Assert.Equal(4, invalidDepth.ExitCode);
        Assert.Equal(string.Empty, invalidDepth.StandardError);
        using (var document = JsonDocument.Parse(invalidDepth.StandardOutput))
        {
            Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "route-list.invalid-depth",
                document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
            Assert.Equal(
                "open-forge route list --help",
                document.RootElement.GetProperty("next").GetProperty("command").GetString());
        }

        Assert.Equal(4, emptyDepth.ExitCode);
        Assert.Equal(string.Empty, emptyDepth.StandardError);
        using (var document = JsonDocument.Parse(emptyDepth.StandardOutput))
        {
            Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(
                "route-list.invalid-depth",
                document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
            Assert.Equal(
                "--depth",
                document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("subject").GetString());
        }

        Assert.Equal(2, attentionHuman.ExitCode);
        Assert.Equal(string.Empty, attentionHuman.StandardError);
        Assert.Contains("Result: attention", attentionHuman.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("route-list.authored-form", attentionHuman.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", attentionHuman.StandardOutput, StringComparison.Ordinal);

        Assert.Equal(3, incompleteHuman.ExitCode);
        Assert.Equal(string.Empty, incompleteHuman.StandardError);
        Assert.StartsWith("result=incomplete  coverage=incomplete", incompleteHuman.StandardOutput, StringComparison.Ordinal);
        Assert.True(
            incompleteHuman.StandardOutput.IndexOf("finding code=", StringComparison.Ordinal)
            < incompleteHuman.StandardOutput.IndexOf("root  .agents/root/_root.md", StringComparison.Ordinal));
        Assert.True(
            incompleteHuman.StandardOutput.IndexOf("next command=", StringComparison.Ordinal)
            < incompleteHuman.StandardOutput.IndexOf("root  .agents/root/_root.md", StringComparison.Ordinal));

        Assert.Equal(completeBefore, complete.SnapshotHashes());
        Assert.Equal(attentionBefore, attention.SnapshotHashes());
        Assert.Equal(incompleteBefore, incomplete.SnapshotHashes());
    }

    [Fact(DisplayName = "Published parser failure uses fixed invalid exit and stderr")]
    [Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedParserFailureUsesFixedInvalidExitAndStandardError()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = TemporaryWorkspace.Create("e2e-invalid");

        var unknown = await RunAsync(environment, working.Path, ["--unknown"]);
        var conflict = await RunAsync(environment, working.Path, ["--help", "--version"]);

        Assert.Equal(4, unknown.ExitCode);
        Assert.Equal(string.Empty, unknown.StandardOutput);
        Assert.Contains("unknown", unknown.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(4, conflict.ExitCode);
        Assert.Equal(string.Empty, conflict.StandardOutput);
        Assert.Contains("mutually exclusive", conflict.StandardError, StringComparison.OrdinalIgnoreCase);
    }

    [Fact(DisplayName = "Published process cancellation kills and drains the owned child")]
    [Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedProcessCancellationKillsAndDrainsOwnedChild()
    {
        var environment = PublishedExecutableEnvironment.ReadRequired();
        using var working = PublishedRouteWorkspace.CreateCancellation(childCount: 500);
        var before = working.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        var startedProcessId = 0;
        var request = new ProcessRunRequest(
            environment.ExecutablePath,
            ["route", "list", "root", "--depth=all", "--json"],
            working.Path,
            timeout: TimeSpan.FromSeconds(30),
            processStarted: processId =>
            {
                startedProcessId = processId;
                cancellation.Cancel();
            });

        var exception = await Assert.ThrowsAsync<ProcessRunCanceledException>(() =>
            ProcessRunner.RunAsync(request, cancellation.Token));
        Assert.True(startedProcessId > 0);
        Assert.Equal(startedProcessId, exception.ProcessId);
        Assert.True(exception.KillRequested);
        Assert.Equal(before, working.SnapshotHashes());
    }

    private static Task<ProcessRunResult> RunAsync(
        PublishedExecutableEnvironment environment,
        string workingDirectory,
        IReadOnlyList<string> arguments)
    {
        return ProcessRunner.RunAsync(
            new ProcessRunRequest(
                environment.ExecutablePath,
                arguments,
                workingDirectory,
                timeout: TimeSpan.FromSeconds(30)),
            TestContext.Current.CancellationToken);
    }

    private static async Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedExecutableEnvironment environment,
        string workingDirectory,
        Func<IReadOnlyDictionary<string, string>> snapshot,
        IReadOnlyList<string> arguments)
    {
        var before = snapshot();
        var result = await RunAsync(environment, workingDirectory, arguments);
        Assert.Equal(before, snapshot());
        return result;
    }
}
