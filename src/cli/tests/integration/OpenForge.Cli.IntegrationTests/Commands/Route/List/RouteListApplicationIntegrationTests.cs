using System.Text.Json;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListApplicationIntegrationTests
{
    [Fact(DisplayName = "Composed Route List JSON retains exact ordered topology and overwrite provenance across views"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task JsonViewsRetainOrderedTopologyAndExactOverwriteSelection()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root\n- [Workspace defined](workspace-defined/_workspace-defined.md) - #Workspace");
        WriteRoute(workspace, ".agents/workspace-defined/_workspace-defined.md", "Workspace defined", "Workspace");
        WriteRoute(workspace, ".agents/detached/_detached.md", "Detached route", "Detached");
        WriteRoute(workspace, ".agents/detached/leaf.md", "Detached leaf", "Leaf");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/adjusted.md", "Adjusted route", "Adjusted");
        workspace.Write(".agents/root/adjusted.overwrite.md", "Workspace adjustment\n");
        WriteRoute(workspace, ".agents/root/child.md", "Child route", "Child");
        workspace.Write(".agents/root/native/SKILL.md", RouteListFilesystemIntegrationWorkspace.SkillMetadata("native", "Native route"));
        WriteRoute(workspace, ".agents/root/nested/_nested.md", "Nested route", "Nested");
        WriteRoute(workspace, ".agents/root/nested/deep.md", "Deep route", "Deep");
        var before = workspace.SnapshotHashes();
        var defaults = await CliHostCapture.RunAsync(["route", "list"], workspace.Path);
        Assert.Equal(0, defaults.ExitCode);
        Assert.Equal(string.Empty, defaults.Error);
        Assert.Contains("Result: complete", defaults.Output, StringComparison.Ordinal);
        Assert.Contains("Requested depth: 1", defaults.Output, StringComparison.Ordinal);
        Assert.Contains("ID: workspace-defined", defaults.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("ID: detached", defaults.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
        var detached = await CliHostCapture.RunAsync(["route", "list", "detached", "--depth=1", "--json"], workspace.Path);
        Assert.Equal(0, detached.ExitCode);
        Assert.Equal(string.Empty, detached.Error);
        using var detachedDocument = JsonDocument.Parse(detached.Output);
        var detachedRows = detachedDocument.RootElement.GetProperty("result").GetProperty("rows");
        Assert.Equal(["detached", "detached/leaf"], detachedRows.EnumerateArray().Select(row => row.GetProperty("id").GetString()));
        Assert.Equal(JsonValueKind.Null, detachedRows[0].GetProperty("absoluteDepth").ValueKind);
        Assert.Equal("detached-root", detachedRows[0].GetProperty("provenance").GetProperty("selection").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
        var compact = await CliHostCapture.RunAsync(["route", "list", "root", "--depth=all", "--view=compact", "--json"], workspace.Path);
        var expanded = await CliHostCapture.RunAsync(["route", "list", "root", "--depth=all", "--view=expanded", "--json"], workspace.Path);
        var overwrite = await CliHostCapture.RunAsync(["route", "list", ".agents/root/adjusted.overwrite.md", "--depth=0", "--json"], workspace.Path);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(0, expanded.ExitCode);
        Assert.Equal(string.Empty, expanded.Error);
        Assert.Equal(compact.Output, expanded.Output);
        using var document = JsonDocument.Parse(compact.Output);
        var result = document.RootElement.GetProperty("result");
        Assert.Equal("all", result.GetProperty("requestedDepth").GetString());
        Assert.Equal("all", result.GetProperty("effectiveDepth").GetString());
        var rows = result.GetProperty("rows");
        Assert.Equal(["root", "root/adjusted", "root/child", "root/native", "root/nested", "root/nested/deep"], rows.EnumerateArray().Select(row => row.GetProperty("id").GetString()));
        Assert.Equal("routed-native", rows[3].GetProperty("provenance").GetProperty("source").GetString());
        Assert.Equal(0, overwrite.ExitCode);
        Assert.Equal(string.Empty, overwrite.Error);
        using var overwriteDocument = JsonDocument.Parse(overwrite.Output);
        var selected = Assert.Single(overwriteDocument.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
        Assert.Equal(".agents/root/adjusted.md", selected.GetProperty("path").GetString());
        Assert.True(selected.GetProperty("provenance").GetProperty("hasOverwrite").GetBoolean());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route List findings and next guidance precede rows with exact semantic streams"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task FindingsAndNextPrecedeRowsWithExactSemanticStreams()
    {
        using var attention = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(attention, "- [Root](root/_root.md) - #Root");
        WriteRoute(attention, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(attention, ".agents/root/compat/index.md", "Compatibility route", "Compatibility");
        using var incomplete = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(incomplete, "- [Root](root/_root.md) - #Root");
        WriteRoute(incomplete, ".agents/root/_root.md", "Root route", "Root");
        incomplete.Write(".agents/root/malformed.md", "---\nopen-forge: [\n---\n\n# Malformed\n");
        var attentionBefore = attention.SnapshotHashes();
        var incompleteBefore = incomplete.SnapshotHashes();
        var attentionResult = await CliHostCapture.RunAsync(["route", "list", "root", "--depth=all", "--view=expanded"], attention.Path);
        var incompleteResult = await CliHostCapture.RunAsync(["route", "list", "root", "--depth=all", "--view=compact"], incomplete.Path);

        Assert.Equal(2, attentionResult.ExitCode);
        Assert.Equal(string.Empty, attentionResult.Error);
        Assert.Contains("Result: attention", attentionResult.Output, StringComparison.Ordinal);
        Assert.Contains("route-list.authored-form", attentionResult.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", attentionResult.Output, StringComparison.Ordinal);
        Assert.Equal(3, incompleteResult.ExitCode);
        Assert.Equal(string.Empty, incompleteResult.Error);
        Assert.StartsWith("result=incomplete  coverage=incomplete", incompleteResult.Output, StringComparison.Ordinal);
        var rowIndex = incompleteResult.Output.IndexOf("root  .agents/root/_root.md", StringComparison.Ordinal);
        var findingIndex = incompleteResult.Output.IndexOf("finding code=", StringComparison.Ordinal);
        var nextIndex = incompleteResult.Output.IndexOf("next command=", StringComparison.Ordinal);
        Assert.InRange(findingIndex, 0, rowIndex - 1);
        Assert.InRange(nextIndex, 0, rowIndex - 1);
        Assert.Equal(attentionBefore, attention.SnapshotHashes());
        Assert.Equal(incompleteBefore, incomplete.SnapshotHashes());
    }

    [Fact(DisplayName = "CLI root route family and Route List help expose the composed family and list leaf"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteFamilyHelpExposesListLeaf()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();

        var root = await CliHostCapture.RunAsync([], workspace.Path);
        var group = await CliHostCapture.RunAsync(["route"], workspace.Path);
        var leaf = await CliHostCapture.RunAsync(["route", "list", "--help"], workspace.Path);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, root.Error);
        Assert.Equal(string.Empty, group.Error);
        Assert.Equal(string.Empty, leaf.Error);
        Assert.Contains("route list", root.Output, StringComparison.Ordinal);
        var groupCommands = group.Output
            .Split(Environment.NewLine, StringSplitOptions.None)
            .SkipWhile(line => !line.Equals("Commands:", StringComparison.Ordinal))
            .Skip(1)
            .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => line.Length > 2
                && line[0] == ' '
                && line[1] == ' '
                && char.IsAsciiLetterLower(line[2]))
            .Select(line => line.TrimStart().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0]);
        Assert.Equal(["list", "inspect", "init", "create", "update", "move", "remove"], groupCommands);
        Assert.Contains("update <source-reference>", group.Output, StringComparison.Ordinal);
        Assert.Contains("remove <source-reference>", group.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Operations:", group.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("available —", group.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route list [source-reference]", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("route inspect — inspect", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("The default depth is 1", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route list .agents/memory/_memory.md", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=2 --json", leaf.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "CLI route-list executes one real workspace operation in human and JSON streams"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExecutesRealWorkspaceInHumanAndJsonStreams()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/child.md", "Child route", "Child");
        var before = workspace.SnapshotHashes();

        var human = await CliHostCapture.RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=1", "--view=compact"],
            workspace.Path);
        var json = await CliHostCapture.RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=0", "--json"],
            workspace.Path);
        var verboseJson = await CliHostCapture.RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=0", "--json", "--verbose"],
            workspace.Path);

        Assert.Equal(0, human.ExitCode);
        Assert.Equal(string.Empty, human.Error);
        Assert.Contains("result=complete", human.Output, StringComparison.Ordinal);
        Assert.Contains("root  .agents/root/_root.md", human.Output, StringComparison.Ordinal);
        Assert.Contains("root/child", human.Output, StringComparison.Ordinal);

        Assert.Equal(0, json.ExitCode);
        Assert.Equal(string.Empty, json.Error);
        using var document = JsonDocument.Parse(json.Output);
        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(0, document.RootElement.GetProperty("result").GetProperty("requestedDepth").GetInt32());
        Assert.Single(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());

        Assert.Equal(0, verboseJson.ExitCode);
        Assert.Equal(json.Output, verboseJson.Output);
        Assert.Contains("rows=1", verboseJson.Error, StringComparison.Ordinal);
        Assert.InRange(verboseJson.Error.Length, 1, 4096);
        Assert.Contains("selection.kind=source-id", verboseJson.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("Root route", verboseJson.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("root/child", verboseJson.Error, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Pinned parser normalizes native scalar forms at the composed Route List boundary"), Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public async Task PinnedParserNormalizesNativeScalarForms()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();

        var workspaceForms = new (string Name, string[] Arguments)[]
        {
            ("workspace-spaced", ["--workspace", workspace.Path]),
            ("workspace-equals", [$"--workspace={workspace.Path}"]),
            ("workspace-colon", [$"--workspace:{workspace.Path}"]),
        };
        var viewForms = new (string Name, string[] Arguments)[]
        {
            ("view-spaced", ["--view", "expanded"]),
            ("view-equals", ["--view=expanded"]),
            ("view-colon", ["--view:expanded"]),
        };

        var baseline = await CliHostCapture.RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=0", "--view=expanded"],
            workspace.Path);
        Assert.Equal(0, baseline.ExitCode);
        Assert.Equal(string.Empty, baseline.Error);
        Assert.Contains("ID: root", baseline.Output, StringComparison.Ordinal);

        foreach (var (Name, Arguments) in workspaceForms)
        {
            foreach (var viewForm in viewForms)
            {
                var arguments = new List<string> { "route", "list", "root" };
                arguments.AddRange(Arguments);
                arguments.Add("--depth=0");
                arguments.AddRange(viewForm.Arguments);
                var result = await CliHostCapture.RunAsync([.. arguments], workspace.Path);
                var form = $"{Name}, {viewForm.Name}";

                Assert.True(
                    result.ExitCode == baseline.ExitCode,
                    $"{form}: expected exit {baseline.ExitCode}, actual {result.ExitCode}.");
                Assert.True(result.Output == baseline.Output, $"{form}: standard output differed.");
                Assert.True(result.Error == baseline.Error, $"{form}: standard error differed.");
            }
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "CLI route-list enforces equals-only depth syntax before option termination"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration"),
     InlineData("--depth=1", null, false),
     InlineData("--depth", null, true),
     InlineData("--depth", "1", true),
     InlineData("--depth:1", null, true)]
    public static async Task DepthDelimiterPolicyRemainsExplicitBeforeTerminator(
        string option,
        string? separateValue,
        bool rejected)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();
        string[] arguments = separateValue is null
            ? ["route", "list", "root", "--workspace", workspace.Path, option, "--json"]
            : ["route", "list", "root", "--workspace", workspace.Path, option, separateValue, "--json"];

        var result = await CliHostCapture.RunAsync(arguments, workspace.Path);

        Assert.Equal(rejected ? 4 : 0, result.ExitCode);
        if (rejected)
        {
            Assert.Equal(string.Empty, result.Output);
            Assert.NotEmpty(result.Error);
        }
        else
        {
            Assert.Equal(string.Empty, result.Error);
            using var document = JsonDocument.Parse(result.Output);
            Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
            Assert.Equal(1, document.RootElement.GetProperty("result").GetProperty("requestedDepth").GetInt32());
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Attached-empty Route List depth preserves following --json as one typed invalid result"), Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    public async Task AttachedEmptyScalarPreservesFollowingGlobal()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["route", "list", "--workspace", workspace.Path, "--depth=", "--json"],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.invalid-depth", finding.GetProperty("code").GetString());
        Assert.Equal("--depth", finding.GetProperty("subject").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "CLI route-list accepts omitted and boundary depth values as typed requests"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration"),
     InlineData(null, "Finite", 1),
     InlineData("0", "Finite", 0),
     InlineData("2147483647", "Finite", 2147483647),
     InlineData("all", "All", 0)]
    public static async Task AcceptsTypedDepthBoundaries(
        string? spelling,
        string expectedKind,
        int expectedValue)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        WriteRoute(workspace, ".agents/root/child.md", "Child route", "Child");
        var before = workspace.SnapshotHashes();
        var arguments = new List<string>
        {
            "route", "list", "root", "--workspace", workspace.Path,
        };
        if (spelling is not null)
        {
            arguments.Add($"--depth={spelling}");
        }

        arguments.Add("--json");
        var result = await CliHostCapture.RunAsync([.. arguments], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
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

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "CLI route-list returns one typed invalid result for invalid depth values"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration"),
     InlineData("-1", "-1"),
     InlineData("2147483648", "2147483648"),
     InlineData("unknown", "unknown")]
    public static async Task RejectsInvalidDepthValuesWithoutWorkspaceWrites(
        string spelling,
        string expectedSubject)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["route", "list", "--workspace", workspace.Path, $"--depth={spelling}", "--json"],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.invalid-depth", finding.GetProperty("code").GetString());
        Assert.Equal(expectedSubject, finding.GetProperty("subject").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "CLI route-list rejects repeated depth occurrences as one parser error"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task RejectsRepeatedDepthOccurrencesAsParserError()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "route", "list", "root", "--workspace", workspace.Path,
                "--depth=0", "--depth=1", "--json",
            ],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.NotEmpty(result.Error);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "CLI route-list treats an option-like source after the terminator as domain input"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task TerminatorPreservesOptionLikeSourceAsDomainInput()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            ["route", "list", "--workspace", workspace.Path, "--json", "--", "--depth="],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        var finding = Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.Equal("route-list.unknown-source", finding.GetProperty("code").GetString());
        Assert.Equal("--depth=", finding.GetProperty("subject").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Terminal modes reject Route List domain and local input before effects"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "Integration"),
     InlineData("--help"),
     InlineData("--version")]
    public static async Task TerminalModesRejectDomainAndLocalInput(string terminalOption)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        var missingWorkspace = Path.Combine(workspace.Path, "terminal-workspace-must-not-be-created");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "route", "list", "root", "--depth=0", terminalOption,
                "--workspace", missingWorkspace,
            ],
            workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        var diagnostic = Assert.Single(
            result.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Terminal modes accept well-formed global no-op options"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "Integration"),
     InlineData("--help"),
     InlineData("--version")]
    public static async Task TerminalModesAcceptWellFormedGlobalNoOpOptions(string terminalOption)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        var missingWorkspace = Path.Combine(workspace.Path, "terminal-global-workspace");
        var before = workspace.SnapshotHashes();

        var result = await CliHostCapture.RunAsync(
            [
                "route", "list", terminalOption, "--workspace", missingWorkspace,
                "--json", "--verbose", "--view=compact",
            ],
            workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.Output);
        Assert.Equal(string.Empty, result.Error);
        Assert.False(Directory.Exists(missingWorkspace));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "CLI route-list maps an unavailable workspace to one typed null-workspace JSON result"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnavailableWorkspaceUsesTypedJsonResult()
    {
        var missing = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"open-forge-route-list-missing-{Guid.NewGuid():N}");
        var result = await CliHostCapture.RunAsync(
            ["route", "list", "--workspace", missing, "--json"],
            missing);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        Assert.Equal(
            "route-list.invalid-workspace",
            document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
    }

    [Fact(DisplayName = "CLI route-list presents one interrupted result after real mid-read cancellation"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task MidReadCancellationPresentsInterruptedResult()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        workspace.Write(".agents/root/z-slow.md", new byte[32 * 1024 * 1024]);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        using var output = new StringWriter();
        using var error = new StringWriter();

        var pending = CliHost.RunAsync(
            ["route", "list", "root", "--depth=all", "--json"],
            workspace.Path,
            new CliOutputWriters(output, error),
            cancellation.Token);
        Assert.False(pending.IsCompleted, "The real large-file read must establish a cancellable operation boundary.");
        cancellation.Cancel();
        var exitCode = await pending;

        Assert.Equal(130, exitCode);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("interrupted", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "route-list.interrupted",
            document.RootElement.GetProperty("result").GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void WriteLoader(
        RouteListFilesystemIntegrationWorkspace workspace,
        string entries)
    {
        workspace.Write(".agents/loader.md", GeneratedLoaderDocumentBuilder.Build(entries));
    }

    private static void WriteRoute(
        RouteListFilesystemIntegrationWorkspace workspace,
        string path,
        string description,
        string tag)
    {
        workspace.Write(path, RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata(description, tag));
    }

}
