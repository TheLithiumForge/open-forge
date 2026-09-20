using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Route.Move.Interaction.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move.Interaction;

public sealed class RouteMoveInteractionApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move plans a selected physical source when remaining guards pass"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourcePlansWhenRemainingGuardsPass()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-choice");
        workspace.SeedMinimalIdentityCollisionScenario();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination, "--dry-run"]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "Would move guidance/old guide to .agents/guidance/new guide.md",
            run.StandardOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move applies a selected physical source once and retains its colliding source"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourceAppliesWhenRemainingGuardsPass()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-apply-choice");
        workspace.SeedMinimalIdentityCollisionScenario();
        workspace.OwnLeafDestination(crossRoute: false);
        var selectedPath = workspace.Absolute(RouteMoveIntegrationWorkspace.LeafPath);
        var destinationPath = workspace.Absolute(RouteMoveIntegrationWorkspace.LeafDestination);
        var retainedPath = workspace.Absolute(".agents/guidance/old guide/_old guide.md");
        var selectedBefore = File.ReadAllBytes(selectedPath);
        var retainedBefore = File.ReadAllBytes(retainedPath);
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "Moved guidance/old guide to .agents/guidance/new guide.md",
            run.StandardOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(
            File.Exists(selectedPath),
            $"Terminal: {run.TerminalOutput}\nOutput: {run.StandardOutput}\nError: {run.StandardError}");
        Assert.Equal(selectedBefore, File.ReadAllBytes(destinationPath));
        Assert.True(File.Exists(retainedPath));
        Assert.Equal(retainedBefore, File.ReadAllBytes(retainedPath));
        Assert.NotEqual(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move retains downstream destination safety after a physical source choice"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourceRemainsBlockedByDestinationSafety()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-reference-blocked");
        workspace.SeedScenario("identity-collision");
        workspace.SeedScenario("occupied-destination");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(".agents/guidance/new guide.md already exists.", run.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("route-move.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Route Move blocks a selected source before effects when an authored missing fragment targets a colliding ID"),
     InlineData(true), InlineData(false),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task MissingFragmentCollisionBlocksBeforeEffects(bool dryRun)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-missing-fragment");
        workspace.SeedMinimalIdentityCollisionScenario();
        workspace.WriteText(
            "README.md",
            "[Old guide](.agents/guidance/old%20guide.md#missing-fragment)\n");
        var before = workspace.SnapshotHashes();

        var command = new List<string>
        {
            "route", "move", RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.LeafDestination,
        };
        if (dryRun)
        {
            command.Add("--dry-run");
        }

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            command);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.Contains("cannot be rewritten safely", run.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("route-move.", run.StandardError, StringComparison.Ordinal);
        Assert.Contains(
            "The target path is exact, but its automatic source ID is shared by more than one logical source.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move cancels an empty source selection without writes"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task EmptySelectionCancelsWithoutWrites()
    {
        await AssertSelectionCancellationAsync([string.Empty]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move treats source-selection EOF as cancellation without writes"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task EndOfInputCancelsWithoutWrites()
    {
        await AssertSelectionCancellationAsync([]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move does not prompt for a JSON collision"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task JsonCollisionDoesNotPromptOrConsumeInput()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-json");
        workspace.SeedScenario("identity-collision");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination, "--format", "json"]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains("route-move.identity-collision", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Move does not prompt for a redirected collision"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task RedirectedCollisionDoesNotPromptOrConsumeInput()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-redirected");
        workspace.SeedScenario("identity-collision");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: false,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains(
            "The ID guidance/old guide matches more than one file. Use the exact path.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.DoesNotContain("route-move.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task AssertSelectionCancellationAsync(IEnumerable<string?> lines)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("route-move-interaction-cancel");
        workspace.SeedScenario("identity-collision");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            lines,
            canPrompt: true,
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId,
                RouteMoveIntegrationWorkspace.LeafDestination]);

        Assert.Equal(130, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains("Route move was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<RouteMoveInteractionRun> RunAsync(
        RouteMoveIntegrationWorkspace workspace,
        IEnumerable<string?> lines,
        bool canPrompt,
        IReadOnlyList<string> commandArguments)
    {
        var workspacePath = workspace.Workspace.PhysicalRoot;
        var lockStoreRoot = workspace.LockStoreRoot;
        var terminal = ScriptedCliTerminal.Lines(lines, canPrompt);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = !canPrompt,
                PromptOutputRedirected = !canPrompt,
                LockStoreRoot = lockStoreRoot,
                Terminal = terminal.Terminal,
            });

        var arguments = commandArguments
            .Concat(["--workspace", workspacePath, "--detail=minimal"])
            .ToArray();
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(workspacePath),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new RouteMoveInteractionRun(
            completion,
            standardOutput.ToString(),
            standardError.ToString(),
            terminal.Output.ToString(),
            terminal);
    }

}
