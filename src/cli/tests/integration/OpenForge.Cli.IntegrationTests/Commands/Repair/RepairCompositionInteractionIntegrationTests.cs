using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.IntegrationTests.Commands.Repair.Shared.Interaction.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairCompositionInteractionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Composed Repair previews safe and guided selections before each question and applies once"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task InteractiveFlowRendersPreviewsBeforeQuestionsAndAppliesOnce()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-composed-interactive");
        var before = workspace.SnapshotState();
        var run = await RunAsync(
            ["repair"],
            workspace.Path,
            "y\n1\ny\n",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Null(run.RemainingInput);
        // The first line is a sentence stating the outcome; the legacy `Open Forge repair` header
        // and the `Status:` line are gone.
        Assert.StartsWith("Repaired ", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        // The legacy `Application: applied (1 effects)` line is gone, and with it its plural
        // grammar defect. Each repaired link is now a row under the headline.
        Assert.Contains(".agents/docs/source.md:9:8", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Preview of selected repairs", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply the 1 repair that is safe? [y/N]", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply these changes? [y/N]", run.StandardOutput, StringComparison.Ordinal);

        // The plan review renders the preview report itself at minimal detail rather than a
        // `Preview of selected repairs` header, so each preview opens with the dry-run headline.
        var firstPreview = run.StandardError.IndexOf("Would repair ", StringComparison.Ordinal);
        var safeQuestion = run.StandardError.IndexOf("Apply the 1 repair that is safe? [y/N]", StringComparison.Ordinal);
        var guidedQuestion = run.StandardError.IndexOf("choose a target for \"missing.md\".", StringComparison.Ordinal);
        var finalPreview = run.StandardError.LastIndexOf("Would repair ", StringComparison.Ordinal);
        var finalQuestion = run.StandardError.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(firstPreview >= 0, run.StandardError);
        Assert.True(firstPreview < safeQuestion, run.StandardError);
        Assert.True(safeQuestion < guidedQuestion, run.StandardError);
        Assert.True(guidedQuestion < finalPreview, run.StandardError);
        Assert.True(finalPreview < finalQuestion, run.StandardError);
        Assert.Contains("1. .agents/docs/guide.md", run.StandardError, StringComparison.Ordinal);

        Assert.Contains("skip", run.StandardError, StringComparison.Ordinal);
        var after = workspace.SnapshotState();
        Assert.Equal(before["workspace/.agents/docs/unrelated.bin"], after["workspace/.agents/docs/unrelated.bin"]);
        Assert.Contains("[Safe](guide.md)", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
        Assert.Contains("[Guided](guide.md)", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
        workspace.AssertNoRecoveryArtifacts();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Composed Repair cancellation at every interactive stage preserves the workspace"),
        InlineData("n\n", "safe"),
        InlineData("", "safe"),
        InlineData("y\n\n", "guided"),
        InlineData("y\n", "guided"),
        InlineData("y\n1\nn\n", "final"),
        InlineData("y\n1\n", "final"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task InteractiveCancellationPreservesWorkspace(string standardInput, string stage)
    {
        using var workspace = RepairIntegrationWorkspace.Create($"repair-composed-{stage}-cancel");
        var before = workspace.SnapshotState();
        var run = await RunAsync(
            ["repair"],
            workspace.Path,
            standardInput,
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Equal(CliOutputTarget.StandardError, run.PrimaryOutputTarget);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Null(run.RemainingInput);
        Assert.Contains(ExpectedPrompt(stage), run.StandardError, StringComparison.Ordinal);
        if (stage == "safe")
        {
            Assert.DoesNotContain("choose a target for", run.StandardError, StringComparison.Ordinal);
            Assert.DoesNotContain("Apply these changes? [y/N]", run.StandardError, StringComparison.Ordinal);
        }
        else if (stage == "guided")
        {
            Assert.DoesNotContain("Apply these changes? [y/N]", run.StandardError, StringComparison.Ordinal);
        }

        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Composed Repair selects an actual Library residual through the typed prompt and applies once"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task LibraryResidualSelectUsesRealPromptAndFinalConfirmation()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var authoredSource = Path.Combine(workspace.Files.Path, ".agents", "directives", "_directives.md");
        workspace.Files.Replace(".agents/directives/_directives.md",
            File.ReadAllText(authoredSource) + "\n\n## Notes\nSee [Review](review.md).\n");
        var run = await RunAsync(
            ["repair"],
            workspace.Files.Path,
            "2\n1\ny\n",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
        Assert.Null(run.RemainingInput);
        // The first line is a sentence stating the outcome; the legacy `Open Forge repair` header
        // and the `Status:` line are gone.
        Assert.StartsWith("Repaired ", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(
            "Library team-knowledge: recover ordinary replace at .agents/open-forge.lock.json?",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("1. Select", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("2. skip", run.StandardError, StringComparison.Ordinal);
        // The plan review renders the preview report itself at minimal detail rather than
        // printing a `Preview of selected repairs` header. This flow previews a result whose
        // Library recovery step has already run, so the preview opens with `Repaired ...`; the
        // line that marks it as a preview, before anything is written, is this one.
        Assert.Contains("No files were changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("Apply these changes? [y/N]", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Composed Repair Library residual skip, refusal, and EOF preserve bytes"),
        InlineData("2\n2\n", "skip"),
        InlineData("2\n\n", "refusal"),
        InlineData("2\n", "eof"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task LibraryResidualNonSelectionPreservesWorkspace(string standardInput, string outcome)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("record-replace");
        var authoredSource = Path.Combine(workspace.Files.Path, ".agents", "directives", "_directives.md");
        workspace.Files.Replace(".agents/directives/_directives.md",
            File.ReadAllText(authoredSource) + "\n\n## Notes\nSee [Review](review.md).\n");
        var before = workspace.Files.Snapshot();
        var run = await RunAsync(
            ["repair"],
            workspace.Files.Path,
            standardInput,
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Contains(
            "Library team-knowledge: recover ordinary replace at .agents/open-forge.lock.json?",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Contains("2. skip", run.StandardError, StringComparison.Ordinal);
        Assert.Null(run.RemainingInput);
        Assert.Equal(before, workspace.Files.Snapshot());
        Assert.Equal(workspace.PriorText, File.ReadAllText(workspace.TargetPath).TrimEnd('\n'));

        if (outcome == "skip")
        {
            Assert.Equal(2, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Attention, run.Status);
            Assert.Equal(CliOutputTarget.StandardOutput, run.PrimaryOutputTarget);
            Assert.DoesNotContain("Apply these changes? [y/N]", run.StandardError, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(130, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
            Assert.Equal(CliOutputTarget.StandardError, run.PrimaryOutputTarget);
            Assert.Equal(string.Empty, run.StandardOutput);
        }
    }

    private static string ExpectedPrompt(string stage)
        => stage switch
        {
            "safe" => "Apply the 1 repair that is safe? [y/N]",
            "guided" => "choose a target for \"missing.md\".",
            "final" => "Apply these changes? [y/N]",
            _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "The Repair prompt stage is not defined."),
        };

    private static async Task<RepairCompositionRun> RunAsync(
        string[] arguments,
        string currentDirectory,
        string standardInput,
        bool standardInputRedirected,
        bool promptOutputRedirected)
    {
        using var input = new StringReader(standardInput);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        using var lockStore = WorkspaceLockTestStore.Create("repair-composition-lock-store");
        _ = lockStore.Track(new CliWorkspace(
            lexicalRoot: currentDirectory,
            physicalRoot: currentDirectory,
            selectedBy: CliWorkspaceSelectionMethod.CurrentDirectory));
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "repair-composition"),
            new CliCompositionInputs
            {
                StandardInput = input,
                PromptOutput = standardError,
                StandardInputRedirected = standardInputRedirected,
                PromptOutputRedirected = promptOutputRedirected,
                LockStoreRoot = lockStore.StoreRoot,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new RepairCompositionRun
        {
            ExitCode = completion.ExitCode,
            Status = completion.Status,
            PrimaryOutputTarget = completion.PrimaryOutputTarget,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            RemainingInput = await input.ReadLineAsync(TestContext.Current.CancellationToken),
        };
    }

}
