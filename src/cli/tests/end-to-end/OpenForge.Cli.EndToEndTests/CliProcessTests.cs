using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class CliProcessTests
{
    [Fact(DisplayName = "Published version is exact and workspace independent")]
    [Trait("Feature", "cli-process"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedVersionIsExactAndWorkspaceIndependent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-version-working");
        using var other = TemporaryWorkspace.Create("e2e-version-other");
        var missingWorkspace = working.Combine("missing-workspace");
        var workingBefore = working.SnapshotHashes();
        var otherBefore = other.SnapshotHashes();
        var request = new ProcessRunRequest(
            target.ExecutablePath,
            ["--workspace", missingWorkspace, "--json", "--version"],
            other.Path,
            timeout: TimeSpan.FromSeconds(30));

        var result = await ProcessRunner.RunAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(target.ExpectedVersion + Environment.NewLine, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(workingBefore, working.SnapshotHashes());
        Assert.Equal(otherBefore, other.SnapshotHashes());
    }

    [Fact(DisplayName = "Published root and route-family help expose available List and Inspect commands")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRootAndRouteFamilyHelpExposeAvailableCommands()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-help");

        var root = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, []);
        var help = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, ["--help"]);
        var group = await RunWithoutWritesAsync(target, working.Path, working.SnapshotHashes, ["route"]);
        var leaf = await RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "list", "--help"]);
        var inspectLeaf = await RunWithoutWritesAsync(
            target,
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
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
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
        var target = PublishedExecutableTarget.Discover();
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
            target,
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
                var result = await RunAsync(target, working.Path, arguments);
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
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
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

    [Theory(DisplayName = "Published route-list enforces equals-only depth syntax before the terminator")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    [InlineData("--depth=1", null, false)]
    [InlineData("--depth", null, true)]
    [InlineData("--depth", "1", true)]
    [InlineData("--depth:1", null, true)]
    public async Task PublishedRouteListEnforcesEqualsOnlyDepthSyntax(
        string option,
        string? separateValue,
        bool rejected)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();
        var arguments = new List<string>
        {
            "route", "list", "root", "--workspace", working.Path,
        };
        arguments.Add(option);
        if (separateValue is not null)
        {
            arguments.Add(separateValue);
        }

        arguments.Add("--json");
        var result = await RunAsync(target, working.Path, arguments);

        Assert.Equal(rejected ? 4 : 0, result.ExitCode);
        if (rejected)
        {
            Assert.Equal(string.Empty, result.StandardOutput);
            Assert.NotEmpty(result.StandardError);
        }
        else
        {
            Assert.Equal(string.Empty, result.StandardError);
            using var document = JsonDocument.Parse(result.StandardOutput);
            Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(1, document.RootElement.GetProperty("result").GetProperty("requestedDepth").GetInt32());
        }

        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published attached-empty depth preserves one typed JSON invalid result")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedAttachedEmptyDepthPreservesJsonInvalidResult()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            ["route", "list", "--workspace", working.Path, "--depth=", "--json"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.invalid-depth", finding.GetProperty("code").GetString());
        Assert.Equal("--depth", finding.GetProperty("subject").GetString());
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Theory(DisplayName = "Published route-list accepts omitted and boundary depth values as typed requests")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    [InlineData(null, "Finite", 1)]
    [InlineData("0", "Finite", 0)]
    [InlineData("2147483647", "Finite", 2147483647)]
    [InlineData("all", "All", 0)]
    public async Task PublishedRouteListAcceptsTypedDepthBoundaries(
        string? spelling,
        string expectedKind,
        int expectedValue)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();
        var arguments = new List<string>
        {
            "route", "list", "root", "--workspace", working.Path,
        };
        if (spelling is not null)
        {
            arguments.Add($"--depth={spelling}");
        }

        arguments.Add("--json");
        var result = await RunAsync(target, working.Path, arguments);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        var requestedDepth = document.RootElement.GetProperty("result").GetProperty("requestedDepth");
        if (expectedKind == "All")
        {
            Assert.Equal("all", requestedDepth.GetString());
        }
        else
        {
            Assert.Equal(expectedValue, requestedDepth.GetInt32());
        }

        Assert.Equal(before, working.SnapshotHashes());
    }

    [Theory(DisplayName = "Published route-list returns one typed invalid result for invalid depth values")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    [InlineData("-1", "-1")]
    [InlineData("2147483648", "2147483648")]
    [InlineData("unknown", "unknown")]
    public async Task PublishedRouteListRejectsInvalidDepthValuesWithoutWorkspaceWrites(
        string spelling,
        string expectedSubject)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            ["route", "list", "--workspace", working.Path, $"--depth={spelling}", "--json"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.invalid-depth", finding.GetProperty("code").GetString());
        Assert.Equal(expectedSubject, finding.GetProperty("subject").GetString());
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route-list rejects repeated depth occurrences as one parser error")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListRejectsRepeatedDepthOccurrencesAsParserError()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            [
                "route", "list", "root", "--workspace", working.Path,
                "--depth=0", "--depth=1", "--json",
            ]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Theory(DisplayName = "Published terminal modes reject Route List domain and local input before effects")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task PublishedTerminalModesRejectDomainAndLocalInput(string terminalOption)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var missingWorkspace = Path.Combine(working.Path, "terminal-workspace-must-not-be-created");
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            [
                "route", "list", "root", "--depth=0", terminalOption,
                "--workspace", missingWorkspace,
            ]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        var diagnostic = Assert.Single(
            result.StandardError.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Theory(DisplayName = "Published terminal modes accept well-formed global no-op options")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task PublishedTerminalModesAcceptWellFormedGlobalNoOpOptions(string terminalOption)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var missingWorkspace = Path.Combine(working.Path, "terminal-global-workspace");
        var before = working.SnapshotHashes();

        var result = await RunAsync(
            target,
            working.Path,
            [
                "route", "list", terminalOption, "--workspace", missingWorkspace,
                "--json", "--verbose", "--view=compact",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(before, working.SnapshotHashes());
    }

    [Fact(DisplayName = "Published route list preserves root path depth view overwrite and detached semantics")]
    [Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRouteListPreservesPublicSelectionDepthAndViews()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();

        var defaults = await RunAsync(target, working.Path, ["route", "list"]);
        var exactPath = await RunAsync(
            target,
            working.Path,
            ["route", "list", ".agents/root/_root.md", "--depth=0", "--view=compact"]);
        var allCompactJson = await RunAsync(
            target,
            working.Path,
            ["route", "list", "root", "--depth=all", "--view=compact", "--json"]);
        var allExpandedJson = await RunAsync(
            target,
            working.Path,
            ["route", "list", "root", "--depth=all", "--view=expanded", "--json"]);
        var overwrite = await RunAsync(
            target,
            working.Path,
            ["route", "list", ".agents/root/adjusted.overwrite.md", "--depth=0", "--json"]);
        var detached = await RunAsync(
            target,
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
        var target = PublishedExecutableTarget.Discover();
        using var complete = PublishedRouteWorkspace.CreateComplete();
        using var attention = PublishedRouteWorkspace.CreateAttention();
        using var incomplete = PublishedRouteWorkspace.CreateIncomplete();
        var completeBefore = complete.SnapshotHashes();
        var attentionBefore = attention.SnapshotHashes();
        var incompleteBefore = incomplete.SnapshotHashes();

        var plainJson = await RunAsync(
            target,
            complete.Path,
            ["route", "list", "root", "--depth=0", "--json"]);
        var verboseJson = await RunAsync(
            target,
            complete.Path,
            ["route", "list", "root", "--depth=0", "--json", "--verbose"]);
        var invalidDepth = await RunAsync(
            target,
            complete.Path,
            ["route", "list", "--depth=-1", "--json"]);
        var attentionHuman = await RunAsync(
            target,
            attention.Path,
            ["route", "list", "root", "--depth=all", "--view=expanded"]);
        var incompleteHuman = await RunAsync(
            target,
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
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-invalid");

        var unknown = await RunAsync(target, working.Path, ["--unknown"]);
        var conflict = await RunAsync(target, working.Path, ["--help", "--version"]);

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
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateCancellation(childCount: 500);
        var before = working.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        var startedProcessId = 0;
        var request = new ProcessRunRequest(
            target.ExecutablePath,
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

}
