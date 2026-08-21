using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Process;

namespace OpenForge.Cli.EndToEndTests.Commands.Route.List;

public sealed class RouteListProcessTests
{
    [Fact(DisplayName = "Published CLI exposes root help for the route-list command family"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task RootHelpListsRouteFamily()
    {
        var result = await RunCliAsync("--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("route", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI exposes route-group help without running a domain operation"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task RouteGroupHelpListsListOperation()
    {
        var result = await RunCliAsync("route", "--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("list", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("schemaVersion", result.Stdout, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published CLI exposes route-list help with accepted source depth and global syntax"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task RouteListHelpListsAcceptedSyntax()
    {
        var result = await RunCliAsync("route", "list", "--help");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("source-reference", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--depth", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--workspace", result.Stdout, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published CLI bare route group shows help and runs no route operation"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task BareRouteGroupIsNonOperational()
    {
        var result = await RunCliAsync("route");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("list", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("result=", result.Stdout, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published CLI version is ordinary stdout text even when JSON is composed"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task VersionWithJsonIsOrdinaryText()
    {
        var result = await RunCliAsync("--version", "--json");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliEndToEndEnvironment.ReadRequired().ExpectedVersion, result.Stdout.Trim());
        Assert.DoesNotContain("schemaVersion", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI route-list help is ordinary stdout text even when JSON is composed"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task HelpWithJsonIsOrdinaryText()
    {
        var result = await RunCliAsync("route", "list", "--help", "--json");

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("Usage", result.Stdout, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("schemaVersion", result.Stdout, StringComparison.Ordinal);
        Assert.Equal(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI renders route-list terminal composition conflicts as structured invalid results"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task TerminalCompositionConflictUsesInvalidJsonResult()
    {
        var result = await RunCliAsync("route", "list", "--help", "--depth=1", "--json");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.DoesNotContain("Usage:", result.Stdout, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.Stdout);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
    }

    [Fact(DisplayName = "Published CLI rejects invalid route-list input on stderr with invalid exit"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task InvalidInputUsesStderrAndExitFour()
    {
        var result = await RunCliAsync("route", "list", "--depth=-1");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.Contains("depth", result.Stderr, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "Published CLI route-list human and JSON projections use the same operation"),
     InlineData("--view=expanded"),
     InlineData("--view=compact"),
     InlineData("--json"),
     Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task ProjectionsUseOneOperation(string presentation)
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var result = await RunCliAsync("route", "list", "--workspace", workspace.Path, presentation);

        Assert.Equal(0, result.ExitCode);
        if (presentation == "--json")
        {
            Assert.Contains("\"schemaVersion\":1", result.Stdout, StringComparison.Ordinal);
            Assert.DoesNotContain("Open Forge route list", result.Stdout, StringComparison.Ordinal);
        }
        else if (presentation == "--view=compact")
        {
            Assert.Contains("result=", result.Stdout, StringComparison.Ordinal);
            Assert.Contains("coverage=", result.Stdout, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("route list", result.Stdout, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact(DisplayName = "Published CLI accepts one explicit source and requested depth"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task AcceptsExplicitSourceAndDepth()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var result = await RunCliAsync(
            "route",
            "list",
            "root",
            "--workspace",
            workspace.Path,
            "--depth=0",
            "--json");

        Assert.Equal(0, result.ExitCode);
        using var document = JsonDocument.Parse(result.Stdout);
        var payload = document.RootElement.GetProperty("result");
        Assert.Equal(0, payload.GetProperty("effectiveDepth").GetInt32());
        var row = Assert.Single(payload.GetProperty("rows").EnumerateArray());
        Assert.Equal("root", row.GetProperty("id").GetString());
    }

    [Fact(DisplayName = "Published CLI maps semantic human status streams and exits without mixing JSON stdout"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task MapsStreamsAndExits()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var result = await RunCliAsync("route", "list", "--workspace", workspace.Path, "unknown");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Stdout);
        Assert.NotEqual(string.Empty, result.Stderr);
    }

    [Fact(DisplayName = "Published CLI repeats route-list requests deterministically"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task RepeatsDeterministically()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var first = await RunCliAsync("route", "list", "--workspace", workspace.Path, "--json");
        var second = await RunCliAsync("route", "list", "--workspace", workspace.Path, "--json");

        Assert.Equal(first.ExitCode, second.ExitCode);
        Assert.Equal(first.Stdout, second.Stdout);
        Assert.Equal(first.Stderr, second.Stderr);
    }

    [Fact(DisplayName = "Published CLI route-list execution leaves workspace bytes unchanged"), Trait("Feature", "route-list"), Trait("Evidence", "EndToEnd")]
    public async Task DoesNotMutateWorkspace()
    {
        using var workspace = PublishedRouteWorkspace.Create();
        var before = workspace.Snapshot();

        var result = await RunCliAsync("route", "list", "--workspace", workspace.Path, "--depth=all");

        Assert.Equal(0, result.ExitCode);
        Assert.NotEqual(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.Equal(before, workspace.Snapshot());
    }

    private static Task<CliProcessResult> RunCliAsync(params string[] arguments)
    {
        var environment = CliEndToEndEnvironment.ReadRequired();
        var request = new CliProcessRequest(
            environment.ExecutablePath,
            arguments);
        return CliProcessRunner.RunAsync(request, TestContext.Current.CancellationToken);
    }
}
