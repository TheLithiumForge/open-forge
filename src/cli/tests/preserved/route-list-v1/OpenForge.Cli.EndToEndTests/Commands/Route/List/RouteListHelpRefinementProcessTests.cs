using OpenForge.Cli.EndToEndTests.Process;

namespace OpenForge.Cli.EndToEndTests.Commands.Route.List;

public sealed class RouteListHelpRefinementProcessTests
{
    [Fact(DisplayName = "Published root help orders Discovery and treats globals as terminal no-ops"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task RootHelpProvidesOrderedDiscovery()
    {
        var result = await RunCliAsync(
            "--workspace", MissingWorkspace(), "--json", "--view=compact", "--verbose", "--help");

        AssertTerminalText(result);
        var usage = result.Stdout.IndexOf("Usage:", StringComparison.Ordinal);
        var discovery = result.Stdout.IndexOf("Discovery:", StringComparison.Ordinal);
        var options = result.Stdout.IndexOf("Options:", StringComparison.Ordinal);
        Assert.True(usage >= 0);
        Assert.True(discovery > usage);
        Assert.True(options > discovery);
        Assert.Contains("open-forge route list", result.Stdout, StringComparison.Ordinal);
        AssertCanonicalGlobalOptions(result.Stdout);
    }

    [Fact(DisplayName = "Published bare route group shows available and unavailable route operations without execution"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task BareRouteGroupShowsOperationAvailability()
    {
        var result = await RunCliAsync(
            "route", "--workspace", MissingWorkspace(), "--json", "--view=compact", "--verbose");

        AssertTerminalText(result);
        AssertAvailableListOperation(result.Stdout);
        AssertUnavailableRouteOperations(result.Stdout);
    }

    [Fact(DisplayName = "Published route-group help shows actual available and unavailable operations"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task RouteGroupHelpShowsOperationAvailability()
    {
        var result = await RunCliAsync(
            "route", "--workspace", MissingWorkspace(), "--json", "--view=expanded", "--verbose", "--help");

        AssertTerminalText(result);
        AssertAvailableListOperation(result.Stdout);
        AssertUnavailableRouteOperations(result.Stdout);
        AssertCanonicalGlobalOptions(result.Stdout);
    }

    [Fact(DisplayName = "Published route-list help exposes grammar defaults complete examples and unavailable related commands"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task RouteListHelpIsCompleteAndSymbolDerived()
    {
        var result = await RunCliAsync(
            "route", "list", "--workspace", MissingWorkspace(), "--json", "--view=compact", "--verbose", "--help");

        AssertTerminalText(result);
        Assert.Contains("source-reference", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--workspace <path>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--depth=<non-negative-integer|all>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--view=<compact|expanded>", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("Default: 1", result.Stdout, StringComparison.Ordinal);
        AssertCanonicalGlobalOptions(result.Stdout);
        AssertSingleOptionDeclaration(result.Stdout, "--depth");
        AssertSingleOptionDeclaration(result.Stdout, "--workspace");
        AssertSingleOptionDeclaration(result.Stdout, "--json");
        AssertSingleOptionDeclaration(result.Stdout, "--view");
        AssertSingleOptionDeclaration(result.Stdout, "--verbose");
        AssertSingleOptionDeclaration(result.Stdout, "--help");
        AssertSingleOptionDeclaration(result.Stdout, "--version");

        Assert.Contains("Examples:", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=0", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=2", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list --depth=all", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list memory/working/cli-release --depth=all", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("open-forge route list .agents/workspace/_workspace.md --depth=all", result.Stdout, StringComparison.Ordinal);
        Assert.Contains("--json --view=compact", result.Stdout, StringComparison.Ordinal);

        Assert.Contains("Related commands:", result.Stdout, StringComparison.Ordinal);
        AssertUnavailableRelatedCommand(result.Stdout, "route inspect");
        AssertUnavailableRelatedCommand(result.Stdout, "find");
        AssertUnavailableRelatedCommand(result.Stdout, "references");
        AssertUnavailableRelatedCommand(result.Stdout, "context");
    }

    [Fact(DisplayName = "Published version remains ordinary stdout text and executes no route operation with composed globals"), Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "EndToEnd")]
    public async Task VersionRemainsTerminalTextWithGlobals()
    {
        var result = await RunCliAsync(
            "route", "list", "--workspace", MissingWorkspace(), "--json", "--view=compact", "--verbose", "--version");

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(CliEndToEndEnvironment.ReadRequired().ExpectedVersion, result.Stdout.Trim());
        Assert.Equal(string.Empty, result.Stderr);
        Assert.DoesNotContain("schemaVersion", result.Stdout, StringComparison.Ordinal);
    }

    private static void AssertTerminalText(CliProcessResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEqual(string.Empty, result.Stdout);
        Assert.Equal(string.Empty, result.Stderr);
        Assert.DoesNotContain("schemaVersion", result.Stdout, StringComparison.Ordinal);
        Assert.DoesNotContain("result=", result.Stdout, StringComparison.Ordinal);
    }

    private static void AssertCanonicalGlobalOptions(string help)
    {
        Assert.Contains("--workspace", help, StringComparison.Ordinal);
        Assert.Contains("--json", help, StringComparison.Ordinal);
        Assert.Contains("--view", help, StringComparison.Ordinal);
        Assert.Contains("--verbose", help, StringComparison.Ordinal);
        Assert.Contains("--help", help, StringComparison.Ordinal);
        Assert.Contains("--version", help, StringComparison.Ordinal);
        Assert.DoesNotContain("-h,", help, StringComparison.Ordinal);
        Assert.DoesNotContain("-v,", help, StringComparison.Ordinal);
        Assert.DoesNotContain("-?,", help, StringComparison.Ordinal);
    }

    private static void AssertAvailableListOperation(string help)
    {
        var line = Assert.Single(
            help.Split('\n', StringSplitOptions.None),
            candidate => candidate.Contains("list", StringComparison.Ordinal)
                && candidate.Contains("List deterministic authored route topology", StringComparison.Ordinal));
        Assert.DoesNotContain("unavailable", line, StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertUnavailableRouteOperations(string help)
    {
        foreach (var operation in new[] { "inspect", "init", "create", "update", "move", "remove" })
        {
            AssertUnavailableRelatedCommand(help, operation);
        }
    }

    private static void AssertUnavailableRelatedCommand(string help, string command)
    {
        var lines = help.Split('\n', StringSplitOptions.None);
        var commandLine = Assert.Single(
            lines.Select((line, index) => (line, index)),
            candidate => StartsWithCommandToken(candidate.line.TrimStart(), command));
        var commandIndentation = CountIndentation(commandLine.line);
        var block = new List<string> { commandLine.line };
        for (var index = commandLine.index + 1; index < lines.Length; index++)
        {
            if (string.IsNullOrWhiteSpace(lines[index])
                || CountIndentation(lines[index]) <= commandIndentation)
            {
                break;
            }

            block.Add(lines[index]);
        }

        Assert.Contains(
            block,
            line => line.Contains("unavailable", StringComparison.OrdinalIgnoreCase));
    }

    private static bool StartsWithCommandToken(string line, string command)
    {
        if (!line.StartsWith(command, StringComparison.Ordinal))
        {
            return false;
        }

        if (line.Length == command.Length)
        {
            return true;
        }

        var boundary = line[command.Length];
        return !char.IsLetterOrDigit(boundary) && boundary is not '-' and not '_';
    }

    private static int CountIndentation(string line)
    {
        var indentation = 0;
        while (indentation < line.Length && char.IsWhiteSpace(line[indentation]))
        {
            indentation++;
        }

        return indentation;
    }

    private static void AssertSingleOptionDeclaration(string help, string option)
    {
        Assert.Equal(
            1,
            help.Split('\n', StringSplitOptions.None)
                .Count(line => line.TrimStart().StartsWith(option, StringComparison.Ordinal)));
    }

    private static string MissingWorkspace()
    {
        return System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"open-forge-help-missing-{Guid.NewGuid():N}");
    }

    private static Task<CliProcessResult> RunCliAsync(params string[] arguments)
    {
        var environment = CliEndToEndEnvironment.ReadRequired();
        return CliProcessRunner.RunAsync(
            new CliProcessRequest(environment.ExecutablePath, arguments),
            TestContext.Current.CancellationToken);
    }
}
