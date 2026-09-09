using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using static OpenForge.Cli.EndToEndTests.Shared.PublishedProcess.PublishedProcessTestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedShellBoundaryProcessTests
{
    [Fact(DisplayName = "Published route-list accepts an option-like source after the terminator"), Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
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

    [Theory(DisplayName = "Published route-list enforces equals-only depth syntax before the terminator"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd"),
     InlineData("--depth=1", null, false),
     InlineData("--depth", null, true),
     InlineData("--depth", "1", true),
     InlineData("--depth:1", null, true)]
    public static async Task PublishedRouteListEnforcesEqualsOnlyDepthSyntax(
        string option,
        string? separateValue,
        bool rejected)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteWorkspace.CreateComplete();
        var before = working.SnapshotHashes();
        var arguments = new List<string>
        {
            "route",
            "list",
            "root",
            "--workspace",
            working.Path,
            option
        };
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

    [Fact(DisplayName = "Published attached-empty depth preserves one typed JSON invalid result"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
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

    [Fact(DisplayName = "Published route-list rejects repeated depth occurrences as one parser error"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
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

    [Theory(DisplayName = "Published terminal modes reject Route List domain and local input before effects"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd"),
     InlineData("--help"),
     InlineData("--version")]
    public static async Task PublishedTerminalModesRejectDomainAndLocalInput(string terminalOption)
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

    [Theory(DisplayName = "Published terminal modes accept well-formed global no-op options"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd"),
     InlineData("--help"),
     InlineData("--version")]
    public static async Task PublishedTerminalModesAcceptWellFormedGlobalNoOpOptions(string terminalOption)
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

    [Fact(DisplayName = "Published Route Inspect version bypasses a missing workspace in terminal mode"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    public async Task InspectVersionBypassesWorkspaceAndOperation()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-route-inspect-version");
        var missingWorkspace = working.Combine("missing-workspace");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            [
                "route",
                "inspect",
                "--workspace",
                missingWorkspace,
                "--json",
                "--view=compact",
                "--verbose",
                "--version",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(target.ExpectedVersion + Environment.NewLine, result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published Route Inspect preserves an option-like operand after the process terminator"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    public async Task OptionLikeOperandAfterTerminatorRemainsDomainInput()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedRouteInspectWorkspace.CreateComplete();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["route", "inspect", "--workspace", working.Path, "--json", "--", "--view"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            "--view",
            document.RootElement.GetProperty("result").GetProperty("selection")
                .GetProperty("requestedReference").GetString());
    }

    [Theory(DisplayName = "Published Route Inspect terminal modes reject source input before workspace and operation"),
     InlineData("--help", "root"),
     InlineData("--version", "root"),
     InlineData("--help", "option-like"),
     InlineData("--version", "option-like"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    public static async Task TerminalModesRejectSourceInputBeforeWorkspaceAndOperation(
        string terminalMode,
        string sourceKind)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-route-inspect-terminal-conflict");
        var missingWorkspace = working.Combine("missing-workspace");
        var arguments = new List<string>
        {
            "route",
            "inspect",
            "--workspace",
            missingWorkspace,
            "--json",
            "--view=compact",
            "--verbose",
            terminalMode,
        };
        switch (sourceKind)
        {
            case "root":
                arguments.Add("root");
                break;
            case "option-like":
                arguments.Add("--");
                arguments.Add("--view");
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(sourceKind),
                    sourceKind,
                    "The terminal source case is not defined.");
        }

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            arguments);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        var diagnostic = Assert.Single(
            result.StandardError.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4096);
        Assert.False(Directory.Exists(missingWorkspace));
    }

    [Fact(DisplayName = "Published Route Init rejects automatic mode without prompting or writing"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
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

    [Fact(DisplayName = "Published Route Move rejects a third positional operand at the shell boundary without writes"),
     Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    public async Task ThirdPositionalOperandIsShellInvalidWithoutDomainEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = await PublishedRouteMoveWorkspace.CreateAsync(target);
        var result = await RunMoveWithoutWritesAsync(
            target,
            workspace,
            [
                "route", "move", PublishedRouteMoveWorkspace.SourceId,
                PublishedRouteMoveWorkspace.DestinationPath, "extra",
            ]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.False(string.IsNullOrWhiteSpace(result.StandardError));
        Assert.DoesNotContain("Status:", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("route-move.", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Theory(DisplayName = "Published parser accepts each native global scalar delimiter"),
     InlineData("spaced"), InlineData("equals"), InlineData("colon"), Trait("Feature", "cli-parser"), Trait("Evidence", "EndToEnd")]
    public static async Task NativeGlobalScalarDelimitersReachTheCommand(string form)
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedRouteWorkspace.CreateComplete();
        var arguments = new List<string> { "route", "list", "root", "--depth=0", "--json" };
        AddScalar(arguments, "--workspace", workspace.Path, form);
        AddScalar(arguments, "--view", "compact", form);
        var result = await RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotHashes, arguments);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("root", Assert.Single(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray()).GetProperty("id").GetString());
    }

    private static void AddScalar(
        List<string> arguments,
        string option,
        string value,
        string form)
    {
        switch (form)
        {
            case "spaced":
                arguments.Add(option);
                arguments.Add(value);
                break;
            case "equals":
                arguments.Add($"{option}={value}");
                break;
            case "colon":
                arguments.Add($"{option}:{value}");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(form), form, "The native scalar form is not defined.");
        }
    }

    private static Task<ProcessRunResult> RunMoveWithoutWritesAsync(
        PublishedExecutableTarget target,
        PublishedRouteMoveWorkspace workspace,
        string[] arguments)
        => PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.Path, workspace.SnapshotState, arguments, workspace.ProcessEnvironment);
}
