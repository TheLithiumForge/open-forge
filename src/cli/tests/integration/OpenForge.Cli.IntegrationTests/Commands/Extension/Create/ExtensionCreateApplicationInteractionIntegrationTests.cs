using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Create.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateApplicationInteractionIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Create previews before confirmation and writes apply results to stdout only"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ComposedTextPromptsAndConfirmationApplyOnlyAfterPreview()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-composed-apply");
        var scripted = ScriptedCliTerminal.Lines(["toolkit", catalogue.Path, "yes"]);

        var run = await RunAsync(
            ["extension", "create"],
            catalogue.Path,
            scripted);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.Contains("Created the toolkit Extension scaffold at", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Mode: apply", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Create these files? [y/N]", run.StandardOutput, StringComparison.Ordinal);
        Assert.True(File.Exists(catalogue.Combine("toolkit", "extension.json")));
        Assert.True(Directory.Exists(catalogue.Combine("toolkit", "content")));

        var prompts = scripted.Output.ToString();
        Assert.Contains("Extension ID (lowercase, digits and hyphens):", prompts, StringComparison.Ordinal);
        Assert.Contains("Package folder:", prompts, StringComparison.Ordinal);
        Assert.Contains("Would create the toolkit Extension scaffold at", prompts, StringComparison.Ordinal);
        Assert.Contains("Create these files? [y/N]", prompts, StringComparison.Ordinal);
        Assert.True(
            prompts.IndexOf("Would create the toolkit Extension scaffold at", StringComparison.Ordinal)
                < prompts.IndexOf("Create these files? [y/N]", StringComparison.Ordinal),
            "The retained preview must precede the final confirmation.");
        Assert.Equal(1, Count(prompts, "Create these files? [y/N]"));
        Assert.Equal(3, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
        Assert.Equal("sentinel", run.RemainingInput);
        DeleteDestination(catalogue.Combine("toolkit"));
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Create decline and EOF cancel without writes")]
    [InlineData("decline")]
    [InlineData("eof")]
    [Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ComposedConfirmationCancellationIsInterrupted(string answer)
    {
        using var catalogue = TemporaryWorkspace.Create($"extension-create-composed-cancel-{answer}");
        var before = catalogue.SnapshotHashes();
        var scripted = answer == "eof"
            ? ScriptedCliTerminal.Lines(["toolkit", catalogue.Path, null])
            : ScriptedCliTerminal.Lines(["toolkit", catalogue.Path, "no"]);

        var run = await RunAsync(
            ["extension", "create"],
            catalogue.Path,
            scripted);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains(
            "Extension create was cancelled. Nothing was changed.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, catalogue.SnapshotHashes());
        Assert.False(Directory.Exists(catalogue.Combine("toolkit")));

        var prompts = scripted.Output.ToString();
        Assert.Contains("Would create the toolkit Extension scaffold at", prompts, StringComparison.Ordinal);
        Assert.Contains("Create these files? [y/N]", prompts, StringComparison.Ordinal);
        Assert.True(
            prompts.IndexOf("Would create the toolkit Extension scaffold at", StringComparison.Ordinal)
                < prompts.IndexOf("Create these files? [y/N]", StringComparison.Ordinal),
            "The retained preview must precede the cancellation question.");
        Assert.Equal(1, Count(prompts, "Create these files? [y/N]"));
        Assert.Equal(3, scripted.LineReadCalls);
    }

    [Trait("Boundary", "Host")]
    [Fact(
        DisplayName = "Composed Extension Create reports unavailable confirmation without terminal I/O"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ComposedUnavailableConfirmationIsInvalidAndWriteFree()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-composed-unavailable");
        var before = catalogue.SnapshotHashes();
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);

        var run = await RunAsync(
            ["extension", "create", "toolkit", "--path", catalogue.Path],
            catalogue.Path,
            scripted);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Contains(
            "Extension create needs confirmation, and this session cannot ask.",
            run.StandardError,
            StringComparison.Ordinal);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Equal(before, catalogue.SnapshotHashes());
        Assert.Equal(string.Empty, scripted.Output.ToString());
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal("sentinel", run.RemainingInput);
    }

    [Trait("Boundary", "Host")]
    [Theory(
        DisplayName = "Composed Extension Create keeps automatic, JSON, and redirected modes prompt-free")]
    [InlineData("automatic")]
    [InlineData("json")]
    [InlineData("redirected")]
    [Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task ExplicitModesDoNotPromptAndAutomaticAloneApplies(string scenario)
    {
        using var catalogue = TemporaryWorkspace.Create($"extension-create-composed-mode-{scenario}");
        var before = catalogue.SnapshotHashes();
        var canPrompt = scenario != "redirected";
        var scripted = ScriptedCliTerminal.Lines(["poison"], canPrompt);
        string[] arguments = scenario switch
        {
            "automatic" =>
            ["extension", "create", "toolkit", "--path", catalogue.Path, "--automatic"],
            "json" =>
            ["extension", "create", "toolkit", "--path", catalogue.Path, "--format", "json"],
            "redirected" =>
            ["extension", "create", "toolkit", "--path", catalogue.Path],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The mode is not defined."),
        };

        var run = await RunAsync(
            arguments,
            catalogue.Path,
            scripted,
            standardInputRedirected: scenario == "redirected",
            promptOutputRedirected: scenario == "redirected");

        if (scenario == "automatic")
        {
            Assert.Equal(0, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Complete, run.Status);
            Assert.Contains("Created the toolkit Extension scaffold at", run.StandardOutput, StringComparison.Ordinal);
            Assert.DoesNotContain("Mode: apply", run.StandardOutput, StringComparison.Ordinal);
            Assert.NotEqual(before, catalogue.SnapshotHashes());
            DeleteDestination(catalogue.Combine("toolkit"));
        }
        else
        {
            Assert.Equal(4, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Invalid, run.Status);
            Assert.Equal(before, catalogue.SnapshotHashes());
            if (scenario == "json")
            {
                Assert.Contains(
                    "open-forge extension create --automatic",
                    run.StandardOutput,
                    StringComparison.Ordinal);
                Assert.Equal(string.Empty, run.StandardError);
            }
            else
            {
                Assert.Contains(
                    "Extension create needs confirmation, and this session cannot ask.",
                    run.StandardError,
                    StringComparison.Ordinal);
                Assert.Equal(string.Empty, run.StandardOutput);
                Assert.Equal("sentinel", run.RemainingInput);
            }
        }

        Assert.Equal(string.Empty, scripted.Output.ToString());
        Assert.Equal(0, scripted.WriteCalls);
        Assert.Equal(0, scripted.LineReadCalls);
        Assert.Equal(0, scripted.KeyReadCalls);
    }

    private static async Task<ExtensionCreateApplicationInteractionRun> RunAsync(
        string[] arguments,
        string currentDirectory,
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
                Terminal = scripted.Terminal,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new ExtensionCreateApplicationInteractionRun
        {
            ExitCode = completion.ExitCode,
            Status = completion.Status,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            RemainingInput = await input.ReadLineAsync(TestContext.Current.CancellationToken),
        };
    }

    private static int Count(string value, string needle)
        => value.Split(needle, StringSplitOptions.None).Length - 1;

    private static void DeleteDestination(string destination)
    {
        if (Directory.Exists(destination))
            Directory.Delete(destination, recursive: true);
        else if (File.Exists(destination))
            File.Delete(destination);
    }
}
