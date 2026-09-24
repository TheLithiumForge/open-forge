using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Route.Remove.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove.Interaction;

public sealed class RouteRemoveInteractionApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove previews the retained plan before applying an interactive confirmation"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ConfirmationPreviewsPlanThenApplies()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-confirm");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["y"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        AssertPreviewPrecedesConfirmation(
            run.TerminalOutput,
            $"Would remove {RouteRemoveIntegrationWorkspace.LeafPath}",
            "Delete the 2 files listed above? [y/N]");
        Assert.Contains(RouteRemoveIntegrationWorkspace.LeafPath, run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains(RouteRemoveIntegrationWorkspace.LeafOverwritePath, run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains($"Removed {RouteRemoveIntegrationWorkspace.LeafPath}", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
        Assert.NotEqual(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove leaves the workspace unchanged when confirmation is declined"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task DeclinedConfirmationCancelsWithoutWrites()
    {
        await AssertConfirmationCancellationAsync(["n"]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove treats confirmation EOF as cancellation without writes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ConfirmationEndOfInputCancelsWithoutWrites()
    {
        await AssertConfirmationCancellationAsync([]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove cancels an empty ambiguous source selection without writes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task EmptySourceSelectionCancelsWithoutWrites()
    {
        await AssertSourceSelectionCancellationAsync([string.Empty]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove treats ambiguous source-selection EOF as cancellation without writes"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SourceSelectionEndOfInputCancelsWithoutWrites()
    {
        await AssertSourceSelectionCancellationAsync([]);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove plans a selected physical source when remaining guards pass"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourcePlansWhenRemainingGuardsPass()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-choice");
        workspace.SeedIdentityCollision();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--dry-run"]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.Contains($"Would remove {RouteRemoveIntegrationWorkspace.LeafPath}", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies a selected physical leaf and retains its colliding category source"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourceAppliesAndRetainsCollidingCategory()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-apply-choice");
        workspace.SeedIdentityCollision();
        workspace.WriteText(RouteRemoveIntegrationWorkspace.LeafPath, "# Exact selected source\n");
        var selectedPath = workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath);
        var selectedOverwritePath = workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath);
        var retainedPath = workspace.Combine(".agents/guidance/old guide/_old guide.md");
        var retainedBefore = File.ReadAllBytes(retainedPath);
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1", "y"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(2, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        AssertPreviewPrecedesConfirmation(
            run.TerminalOutput,
            $"Would remove {RouteRemoveIntegrationWorkspace.LeafPath}",
            "Delete the 2 files listed above? [y/N]");
        Assert.Contains("Delete the 2 files listed above? [y/N]", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains($"Removed {RouteRemoveIntegrationWorkspace.LeafPath}", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(selectedPath));
        Assert.False(File.Exists(selectedOverwritePath));
        Assert.True(File.Exists(retainedPath));
        Assert.Equal(retainedBefore, File.ReadAllBytes(retainedPath));
        Assert.NotEqual(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies a selected physical category and retains its colliding sibling leaf"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalCategoryAppliesAndRetainsCollidingSiblingLeaf()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-category-choice");
        workspace.SeedCategoryIdentityCollision();
        var retainedPath = workspace.Combine(RouteRemoveIntegrationWorkspace.CategorySiblingLeafPath);
        var retainedBefore = File.ReadAllBytes(retainedPath);
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["2", "y"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.CategoryId]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(2, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/topics matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        AssertPreviewPrecedesConfirmation(
            run.TerminalOutput,
            "Would remove the route guidance/topics",
            "Delete the");
        Assert.Contains("Removed the route guidance/topics", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryChildPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryNotesPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryResourcePath)));
        Assert.True(File.Exists(retainedPath));
        Assert.Equal(retainedBefore, File.ReadAllBytes(retainedPath));
        Assert.NotEqual(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove preserves ownership safety after a selected physical source"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task SelectedPhysicalSourceRemainsBlockedByOwnershipSafety()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-blocked");
        workspace.SeedIdentityCollision();
        workspace.WriteText(RouteRemoveIntegrationWorkspace.OwnershipPath, "{ invalid ownership json");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["1"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(5, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Delete the", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains("Cannot remove", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove does not prompt for a JSON confirmation"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task JsonApplyRequiresAutomaticWithoutPrompt()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-json");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["y"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--format", "json"]);

        Assert.Equal(4, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains("route-remove.confirmation-required", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies automatically for JSON without prompting"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task AutomaticJsonApplyDoesNotPrompt()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-automatic-json");

        var run = await RunAsync(
            workspace,
            ["y"],
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId,
                "--automatic", "--format", "json"]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove reports unavailable confirmation for redirected human input"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task RedirectedApplyRequiresConfirmationWithoutPrompt()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-redirected");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            ["y"],
            canPrompt: false,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(4, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Contains("Cannot remove", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies automatically for redirected human input without prompting"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task RedirectedAutomaticApplyDoesNotPrompt()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-redirected-automatic");

        var run = await RunAsync(
            workspace,
            ["y"],
            canPrompt: false,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"]);

        Assert.Equal(0, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Completion.Status);
        Assert.Equal(0, run.Terminal.LineReadCalls);
        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
    }

    private static async Task AssertConfirmationCancellationAsync(IEnumerable<string?> lines)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-cancel");
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            lines,
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(130, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains($"Would remove {RouteRemoveIntegrationWorkspace.LeafPath}", run.TerminalOutput, StringComparison.Ordinal);
        AssertPreviewPrecedesConfirmation(
            run.TerminalOutput,
            $"Would remove {RouteRemoveIntegrationWorkspace.LeafPath}",
            "Delete the 2 files listed above? [y/N]");
        Assert.Contains("Route remove was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task AssertSourceSelectionCancellationAsync(IEnumerable<string?> lines)
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-interaction-source-cancel");
        workspace.SeedIdentityCollision();
        var before = workspace.SnapshotHashes();

        var run = await RunAsync(
            workspace,
            lines,
            canPrompt: true,
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId]);

        Assert.Equal(130, run.Completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Completion.Status);
        Assert.Equal(1, run.Terminal.LineReadCalls);
        Assert.Contains(
            "guidance/old guide matches 2 sources. Which one?",
            run.TerminalOutput,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Delete the", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains("Route remove was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static void AssertPreviewPrecedesConfirmation(
        string terminalOutput,
        string previewText,
        string confirmationQuestion)
    {
        var previewIndex = terminalOutput.IndexOf(
            previewText,
            StringComparison.Ordinal);
        var confirmationIndex = terminalOutput.IndexOf(
            confirmationQuestion,
            StringComparison.Ordinal);
        Assert.True(previewIndex >= 0, "The retained plan preview was not rendered.");
        Assert.True(
            confirmationIndex > previewIndex,
            "The deletion confirmation must follow the retained plan preview.");
    }

    private static async Task<RouteRemoveInteractionRun> RunAsync(
        RouteRemoveIntegrationWorkspace workspace,
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
        return new RouteRemoveInteractionRun(
            completion,
            standardOutput.ToString(),
            standardError.ToString(),
            terminal.Output.ToString(),
            terminal);
    }

}
