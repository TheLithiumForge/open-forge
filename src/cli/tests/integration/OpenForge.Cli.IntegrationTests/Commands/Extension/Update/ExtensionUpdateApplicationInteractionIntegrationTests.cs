using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Update.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateApplicationInteractionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Update freezes the chosen package and confirms after the retained preview"),
     Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ComposedSelectionIsAppliedOnceAcrossRevalidationAndVerification()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-composed-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-composed-selection-source");
        await SeedChangedSelectionAsync(workspace, source);
        var sourceBefore = source.Snapshot();

        var scripted = ScriptedCliTerminal.Lines(["2", "yes"]);
        var run = await RunAsync(
            workspace,
            ["extension", "update", "--source", source.Path],
            scripted);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Equal(Document("Toolkit v2"), workspace.ReadText(".agents/toolkit.md"));
        Assert.Equal(Document("Base v1"), workspace.ReadText(".agents/base.md"));
        Assert.Equal(sourceBefore, source.Snapshot());

        var prompts = run.TerminalOutput;
        Assert.Equal(2, scripted.LineReadCalls);
        Assert.Equal(1, Count(prompts, "Which Extensions do you want to update?"));
        Assert.Equal(1, Count(prompts, "Apply these changes? [y/N]"));
        Assert.Contains("Would update the toolkit Extension to 1.0.0.", prompts, StringComparison.Ordinal);
        Assert.Contains("Updated the toolkit Extension to 1.0.0.", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Apply these changes? [y/N]", run.StandardOutput, StringComparison.Ordinal);
        Assert.True(
            prompts.IndexOf("Would update the toolkit Extension to 1.0.0.", StringComparison.Ordinal)
                < prompts.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal),
            "The retained preview must precede the final confirmation.");
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Update selection blank input and EOF cancel before planning")]
    [InlineData("blank")]
    [InlineData("eof")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ComposedSelectionCancellationIsInterrupted(string inputKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-composed-selection-cancel-{inputKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-composed-selection-cancel-source-{inputKind}");
        source.AddPackage("alpha", [], (".agents/alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta.md", Document("Beta")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        var scripted = inputKind == "eof"
            ? ScriptedCliTerminal.Lines([])
            : ScriptedCliTerminal.Lines([string.Empty]);

        var run = await RunAsync(
            workspace,
            ["extension", "update", "--source", source.Path],
            scripted);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains(
            "Extension update was cancelled. Nothing was changed.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(1, scripted.LineReadCalls);
        Assert.DoesNotContain("Apply these changes? [y/N]", run.TerminalOutput, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Update confirmation decline and EOF cancel after preview")]
    [InlineData("decline")]
    [InlineData("eof")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ComposedConfirmationCancellationIsInterrupted(string answer)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-composed-confirm-cancel-{answer}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-composed-confirm-cancel-source-{answer}");
        await SeedChangedSelectionAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        string?[] lines = answer == "eof"
            ? new string?[] { "2", null }
            : ["2", "no"];
        var scripted = ScriptedCliTerminal.Lines(lines);

        var run = await RunAsync(
            workspace,
            ["extension", "update", "--source", source.Path],
            scripted);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains(
            "Extension update was cancelled. Nothing was changed.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(2, scripted.LineReadCalls);
        Assert.Contains("Would update the toolkit Extension to 1.0.0.", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Equal(1, Count(run.TerminalOutput, "Apply these changes? [y/N]"));
        Assert.True(
            run.TerminalOutput.IndexOf("Would update the toolkit Extension to 1.0.0.", StringComparison.Ordinal)
                < run.TerminalOutput.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal),
            "The retained preview must precede the cancellation question.");
    }

    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Update prune uses one deletion-count confirmation"),
     Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task PruneUsesOneSpecializedFinalConfirmation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-composed-prune");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-composed-prune-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        source.RemovePayload("toolkit", ".agents/toolkit.md");
        var sourceBefore = source.Snapshot();
        var scripted = ScriptedCliTerminal.Lines(["yes"]);

        var run = await RunAsync(
            workspace,
            ["extension", "update", "toolkit", "--source", source.Path, "--prune"],
            scripted);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(sourceBefore, source.Snapshot());
        Assert.Equal(1, scripted.LineReadCalls);
        Assert.Equal(1, Count(run.TerminalOutput, "Delete the 1 file listed above? [y/N]"));
        Assert.DoesNotContain("Apply these changes? [y/N]", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains("Would update the toolkit Extension to 1.0.0.", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Contains("Updated the toolkit Extension to 1.0.0.", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Delete the 1 file listed above? [y/N]", run.StandardOutput, StringComparison.Ordinal);
        Assert.True(
            run.TerminalOutput.IndexOf("Would update the toolkit Extension to 1.0.0.", StringComparison.Ordinal)
                < run.TerminalOutput.IndexOf("Delete the 1 file listed above? [y/N]", StringComparison.Ordinal),
            "The retained prune preview must precede the final confirmation.");
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Update keeps automatic, JSON, and redirected modes prompt-free")]
    [InlineData("automatic")]
    [InlineData("json")]
    [InlineData("redirected")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ExplicitModesDoNotPromptAndAutomaticAloneApplies(string scenario)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-composed-mode-{scenario}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-composed-mode-source-{scenario}");
        await SeedChangedSelectionAsync(workspace, source);
        var before = workspace.Snapshot();
        var scripted = ScriptedCliTerminal.Lines(["poison"], canPrompt: scenario != "redirected");
        string[] arguments = scenario switch
        {
            "automatic" =>
            ["extension", "update", "toolkit", "--source", source.Path, "--automatic"],
            "json" =>
            ["extension", "update", "toolkit", "--source", source.Path, "--format", "json"],
            "redirected" =>
            ["extension", "update", "toolkit", "--source", source.Path],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The mode is not defined."),
        };

        var run = await RunAsync(
            workspace,
            arguments,
            scripted,
            standardInputRedirected: scenario == "redirected",
            promptOutputRedirected: scenario == "redirected");

        if (scenario == "automatic")
        {
            Assert.Equal(0, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Complete, run.Status);
            Assert.Contains("Updated the toolkit Extension to 1.0.0.", run.StandardOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("Status:", run.StandardOutput, StringComparison.Ordinal);
            Assert.NotEqual(before, workspace.Snapshot());
        }
        else
        {
            Assert.Equal(4, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Invalid, run.Status);
            Assert.Equal(before, workspace.Snapshot());
            if (scenario == "json")
            {
                Assert.Contains(
                    "extension-update.confirmation-required",
                    run.StandardOutput,
                    StringComparison.Ordinal);
                Assert.Equal(string.Empty, run.StandardError);
            }
            else
            {
                Assert.Contains(
                    "Extension update needs confirmation, and this session cannot ask.",
                    run.StandardError,
                    StringComparison.Ordinal);
                Assert.Equal(string.Empty, run.StandardOutput);
                Assert.Equal("sentinel", run.RemainingInput);
            }
        }

        Assert.Equal(string.Empty, run.TerminalOutput);
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
    }

    private static async Task SeedChangedSelectionAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        source.AddPackage("base", [], (".agents/base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit.md", Document("Toolkit v2"));
    }

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static async Task<ExtensionUpdateApplicationInteractionRun> RunAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        string[] arguments,
        ScriptedCliTerminal scripted,
        bool standardInputRedirected = false,
        bool promptOutputRedirected = false)
    {
        using var input = new StringReader("sentinel\n");
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = input,
                PromptOutput = standardError,
                StandardInputRedirected = standardInputRedirected,
                PromptOutputRedirected = promptOutputRedirected,
                LockStoreRoot = workspace.LockStoreRoot,
                Terminal = scripted.Terminal,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(workspace.Path),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new ExtensionUpdateApplicationInteractionRun
        {
            ExitCode = completion.ExitCode,
            Status = completion.Status,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            TerminalOutput = scripted.Output.ToString(),
            RemainingInput = await input.ReadLineAsync(TestContext.Current.CancellationToken),
        };
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");

    private static int Count(string value, string needle)
        => value.Split(needle, StringSplitOptions.None).Length - 1;
}
