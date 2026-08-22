using System.Text.Json;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListApplicationIntegrationTests
{
    [Fact(DisplayName = "CLI root route family and Route List help expose the composed family and list leaf"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ComposedRouteFamilyHelpExposesListLeaf()
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();

        var root = await RunAsync([], workspace.Path);
        var group = await RunAsync(["route"], workspace.Path);
        var leaf = await RunAsync(["route", "list", "--help"], workspace.Path);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(0, group.ExitCode);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, root.Error);
        Assert.Equal(string.Empty, group.Error);
        Assert.Equal(string.Empty, leaf.Error);
        Assert.Contains("route list", root.Output, StringComparison.Ordinal);
        Assert.Contains("list", group.Output, StringComparison.Ordinal);
        Assert.Contains("inspect  available", group.Output, StringComparison.Ordinal);
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

        var human = await RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=1", "--view=compact"],
            workspace.Path);
        var json = await RunAsync(
            ["route", "list", "root", "--workspace", workspace.Path, "--depth=0", "--json"],
            workspace.Path);
        var verboseJson = await RunAsync(
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

    [Fact(DisplayName = "CLI route-list keeps real results identical across native workspace and view delimiters"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task NativeGlobalDelimiterFormsProduceIdenticalRealWorkspaceResults()
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

        var baseline = await RunAsync(
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
                var result = await RunAsync(arguments.ToArray(), workspace.Path);
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

    [Theory(DisplayName = "CLI route-list retains its explicit depth delimiter policy before option termination")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    [InlineData("--depth", "1")]
    [InlineData("--depth:1", null)]
    public async Task DepthDelimiterPolicyRemainsExplicitBeforeTerminator(
        string option,
        string? separateValue)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        WriteLoader(workspace, "- [Root](root/_root.md) - #Root");
        WriteRoute(workspace, ".agents/root/_root.md", "Root route", "Root");
        var before = workspace.SnapshotHashes();
        string[] arguments = separateValue is null
            ? ["route", "list", "root", option, "--json"]
            : ["route", "list", "root", option, separateValue, "--json"];

        var result = await RunAsync(arguments, workspace.Path);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.NotEmpty(result.Error);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "CLI route-list maps an unavailable workspace to one typed null-workspace JSON result"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task UnavailableWorkspaceUsesTypedJsonResult()
    {
        var missing = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"open-forge-route-list-missing-{Guid.NewGuid():N}");
        var result = await RunAsync(
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

    private static async Task<HostResult> RunAsync(
        string[] arguments,
        string currentDirectory)
    {
        using var output = new StringWriter();
        using var error = new StringWriter();
        var exitCode = await CliHost.RunAsync(
            arguments,
            currentDirectory,
            new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
        return new HostResult(exitCode, output.ToString(), error.ToString());
    }

    private static void WriteLoader(
        RouteListFilesystemIntegrationWorkspace workspace,
        string entries)
    {
        workspace.Write(
            ".agents/loader.md",
            $"""
            # Open Forge Loader

            ## Entries

            <!-- open-forge:generated-index:start -->
            {entries}
            <!-- open-forge:generated-index:end -->
            """);
    }

    private static void WriteRoute(
        RouteListFilesystemIntegrationWorkspace workspace,
        string path,
        string description,
        string tag)
    {
        workspace.Write(path, RouteListFilesystemIntegrationWorkspace.OpenForgeMetadata(description, tag));
    }

    private sealed record HostResult(int ExitCode, string Output, string Error);
}
