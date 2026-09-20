using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Remove.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationInteractionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Remove applies the dependent closure once after preview and confirmation"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ComposedDependentSelectionIsFrozenAndAppliedOnce()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-composed-dependent-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-composed-dependent-selection-source");
        source.AddPackage("base", [], (".agents/base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        var sourceBefore = source.Snapshot();
        var scripted = ScriptedCliTerminal.Lines(["1", "yes"]);

        var run = await RunAsync(
            workspace,
            ["extension", "remove"],
            scripted);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.False(File.Exists(workspace.Combine(".agents/base.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Empty(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal(sourceBefore, source.Snapshot());

        var prompts = run.TerminalOutput;
        Assert.Equal(2, scripted.LineReadCalls);
        Assert.Equal(1, Count(prompts, "Which Extensions do you want to remove?"));
        Assert.Contains("needed by", prompts, StringComparison.Ordinal);
        Assert.Equal(1, Count(prompts, "Delete the 2 files listed above? [y/N]"));
        Assert.Contains("Would remove the base Extension.", prompts, StringComparison.Ordinal);
        Assert.Contains("Removed 2 Extensions: base, toolkit.", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Would remove", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Delete the 2 files listed above? [y/N]", run.StandardOutput, StringComparison.Ordinal);
        Assert.True(
            prompts.IndexOf("Would remove the base Extension.", StringComparison.Ordinal)
                < prompts.IndexOf("Delete the 2 files listed above? [y/N]", StringComparison.Ordinal),
            "The retained dependent-closure preview must precede confirmation.");
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Remove selection blank input and EOF cancel before planning")]
    [InlineData("blank")]
    [InlineData("eof")]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ComposedSelectionCancellationIsInterrupted(string inputKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-composed-selection-cancel-{inputKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-composed-selection-cancel-source-{inputKind}");
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
            ["extension", "remove"],
            scripted);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains(
            "Extension remove was cancelled. Nothing was changed.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(1, scripted.LineReadCalls);
        Assert.DoesNotContain("Delete the ", run.TerminalOutput, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Remove confirmation decline and EOF cancel after dependent preview")]
    [InlineData("decline")]
    [InlineData("eof")]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ComposedConfirmationCancellationIsInterrupted(string answer)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-composed-confirm-cancel-{answer}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-composed-confirm-cancel-source-{answer}");
        source.AddPackage("base", [], (".agents/base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        string?[] lines = answer == "eof"
            ? new string?[] { "1", null }
            : ["1", "no"];
        var scripted = ScriptedCliTerminal.Lines(lines);

        var run = await RunAsync(
            workspace,
            ["extension", "remove"],
            scripted);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains(
            "Extension remove was cancelled. Nothing was changed.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(2, scripted.LineReadCalls);
        Assert.Contains("Would remove the base Extension.", run.TerminalOutput, StringComparison.Ordinal);
        Assert.Equal(1, Count(run.TerminalOutput, "Delete the 2 files listed above? [y/N]"));
        Assert.True(
            run.TerminalOutput.IndexOf("Would remove the base Extension.", StringComparison.Ordinal)
                < run.TerminalOutput.IndexOf("Delete the 2 files listed above? [y/N]", StringComparison.Ordinal),
            "The retained dependent-closure preview must precede cancellation.");
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Remove keeps automatic, JSON, and redirected modes prompt-free")]
    [InlineData("automatic")]
    [InlineData("json")]
    [InlineData("redirected")]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task ExplicitModesDoNotPromptAndAutomaticAloneApplies(string scenario)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-composed-mode-{scenario}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-composed-mode-source-{scenario}");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        var before = workspace.Snapshot();
        var scripted = ScriptedCliTerminal.Lines(["poison"], canPrompt: scenario != "redirected");
        string[] arguments = scenario switch
        {
            "automatic" => ["extension", "remove", "toolkit", "--automatic"],
            "json" => ["extension", "remove", "toolkit", "--format", "json"],
            "redirected" => ["extension", "remove", "toolkit"],
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
            Assert.Contains("Removed the toolkit Extension.", run.StandardOutput, StringComparison.Ordinal);
            Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
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
                    "extension-remove.confirmation-required",
                    run.StandardOutput,
                    StringComparison.Ordinal);
                Assert.Equal(string.Empty, run.StandardError);
            }
            else
            {
                Assert.Contains(
                    "Extension remove needs confirmation, and this session cannot ask.",
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

    private static async Task<ExtensionRemoveApplicationInteractionRun> RunAsync(
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
        return new ExtensionRemoveApplicationInteractionRun
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
