using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Route.Update.Interaction.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Interaction;

public sealed class RouteUpdateInteractionApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update applies one selected physical source through root composition"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task InteractiveChoiceUpdatesSelectedSourceOnce()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-interaction-choice");
        workspace.SeedAmbiguousTarget();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "update", RouteUpdateIntegrationWorkspace.TargetId,
                "--description", "Selected overview"]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "memory/project-alpha/overview matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.Contains(
            "description: \"Before overview\" -> \"Selected overview\"",
            run.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Selected overview", workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath), StringComparison.Ordinal);
        Assert.DoesNotContain(
            "Selected overview",
            workspace.ReadText(".agents/memory/project-alpha/overview/_overview.md"),
            StringComparison.Ordinal);
        Assert.NotEqual(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update cancels an empty source selection without writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task EmptySelectionCancelsWithoutWrites()
    {
        await AssertSelectionCancellationAsync([string.Empty]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update treats source-selection EOF as cancellation without writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task EndOfInputCancelsWithoutWrites()
    {
        await AssertSelectionCancellationAsync([]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update does not prompt for a JSON collision"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task JsonCollisionDoesNotPromptOrConsumeInput()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-interaction-json");
        workspace.SeedAmbiguousTarget();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "update", RouteUpdateIntegrationWorkspace.TargetId,
                "--description", "Should not apply", "--format", "json"]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains("route-update.route-ambiguous", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update does not prompt for a redirected human collision"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task RedirectedCollisionDoesNotPromptOrConsumeInput()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-interaction-redirected");
        workspace.SeedAmbiguousTarget();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: false,
            ["route", "update", RouteUpdateIntegrationWorkspace.TargetId,
                "--description", "Should not apply"]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains(
            $"Cannot update {RouteUpdateIntegrationWorkspace.TargetId}: {RouteUpdateIntegrationWorkspace.TargetId} could match more than one route.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task AssertSelectionCancellationAsync(IEnumerable<string?> lines)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("route-update-interaction-cancel");
        workspace.SeedAmbiguousTarget();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            lines,
            canPrompt: true,
            ["route", "update", RouteUpdateIntegrationWorkspace.TargetId,
                "--description", "Should not apply"]);

        Assert.Equal(130, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains("Route update was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<RouteUpdateInteractionRun> RunAsync(
        RouteUpdateIntegrationWorkspace workspace,
        IEnumerable<string?> lines,
        bool canPrompt,
        IReadOnlyList<string> commandArguments)
    {
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
                LockStoreRoot = workspace.LockStoreRoot,
                Terminal = terminal.Terminal,
            });

        var arguments = commandArguments
            .Concat(["--workspace", workspace.Workspace.PhysicalRoot, "--detail=minimal"])
            .ToArray();
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(workspace.Workspace.PhysicalRoot),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new RouteUpdateInteractionRun(
            completion,
            standardOutput.ToString(),
            standardError.ToString(),
            terminal.Output.ToString(),
            terminal);
    }

}
