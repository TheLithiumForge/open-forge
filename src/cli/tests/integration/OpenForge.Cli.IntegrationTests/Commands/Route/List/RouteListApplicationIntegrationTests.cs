using System.Text.Json;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListApplicationIntegrationTests
{
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
            .TakeWhile(line => !line.Equals("Notes:", StringComparison.Ordinal))
            .Where(line => line.StartsWith("  ", StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(line))
            .Select(line => line.TrimStart().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[0]);
        Assert.Equal(["list", "inspect", "init", "create", "update"], groupCommands);
        Assert.Contains("update <source-reference>", group.Output, StringComparison.Ordinal);
        Assert.Contains(
            "Planned but unavailable operations: move and remove.",
            group.Output,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Operations:", group.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("available —", group.Output, StringComparison.Ordinal);
        Assert.Contains("open-forge route list [source-reference]", leaf.Output, StringComparison.Ordinal);
        Assert.Contains("route inspect — available", leaf.Output, StringComparison.Ordinal);
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

        foreach (var workspaceForm in workspaceForms)
        {
            foreach (var viewForm in viewForms)
            {
                var arguments = new List<string> { "route", "list", "root" };
                arguments.AddRange(workspaceForm.Arguments);
                arguments.Add("--depth=0");
                arguments.AddRange(viewForm.Arguments);
                var result = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);
                var form = $"{workspaceForm.Name}, {viewForm.Name}";

                Assert.True(
                    result.ExitCode == baseline.ExitCode,
                    $"{form}: expected exit {baseline.ExitCode}, actual {result.ExitCode}.");
                Assert.True(result.Output == baseline.Output, $"{form}: standard output differed.");
                Assert.True(result.Error == baseline.Error, $"{form}: standard error differed.");
            }
        }

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "CLI route-list enforces equals-only depth syntax before option termination")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    [InlineData("--depth=1", null, false)]
    [InlineData("--depth", null, true)]
    [InlineData("--depth", "1", true)]
    [InlineData("--depth:1", null, true)]
    public async Task DepthDelimiterPolicyRemainsExplicitBeforeTerminator(
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

    [Theory(DisplayName = "CLI route-list accepts omitted and boundary depth values as typed requests")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    [InlineData(null, "Finite", 1)]
    [InlineData("0", "Finite", 0)]
    [InlineData("2147483647", "Finite", 2147483647)]
    [InlineData("all", "All", 0)]
    public async Task AcceptsTypedDepthBoundaries(
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
        var result = await CliHostCapture.RunAsync(arguments.ToArray(), workspace.Path);

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

    [Theory(DisplayName = "CLI route-list returns one typed invalid result for invalid depth values")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    [InlineData("-1", "-1")]
    [InlineData("2147483648", "2147483648")]
    [InlineData("unknown", "unknown")]
    public async Task RejectsInvalidDepthValuesWithoutWorkspaceWrites(
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

    [Theory(DisplayName = "Terminal modes reject Route List domain and local input before effects")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task TerminalModesRejectDomainAndLocalInput(string terminalOption)
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

    [Theory(DisplayName = "Terminal modes accept well-formed global no-op options")]
    [Trait("Feature", "cli-parser"), Trait("Evidence", "Integration")]
    [InlineData("--help")]
    [InlineData("--version")]
    public async Task TerminalModesAcceptWellFormedGlobalNoOpOptions(string terminalOption)
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

    [Fact(DisplayName = "CLI route-list presents one interrupted result after real mid-read cancellation")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
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
