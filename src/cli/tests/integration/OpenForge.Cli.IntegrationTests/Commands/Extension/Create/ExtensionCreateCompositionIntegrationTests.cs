using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateCompositionIntegrationTests
{
    [Fact(DisplayName = "Root-composed Extension Create wizard reads supplied stdin and writes prompts only to stderr"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task RootCompositionOwnsPromptCapableWizardSession()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-composed-wizard");
        var before = catalogue.SnapshotHashes();
        var run = await RunAsync(
            ["extension", "create", "--dry-run"],
            catalogue.Path,
            $"development-toolkit{Environment.NewLine}{catalogue.Path}{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(
            $"Stable ID (lowercase ASCII letters or digits separated by single hyphens):{Environment.NewLine}"
            + $"Catalogue path (existing ordinary directory; aliases are allowed):{Environment.NewLine}",
            run.StandardError);
        Assert.Contains("ID: development-toolkit", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Mode: dry-run", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Status: complete", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Stable ID", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Theory(DisplayName = "Root-composed Extension Create prompts only for eligible human omissions"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("explicit", false, false, 0, CliSemanticStatus.Complete, "development-toolkit")]
    [InlineData("redirected", true, true, 4, CliSemanticStatus.Invalid, "development-toolkit")]
    [InlineData("automatic", false, false, 4, CliSemanticStatus.Invalid, "development-toolkit")]
    [InlineData("json", false, false, 4, CliSemanticStatus.Invalid, "development-toolkit")]
    public async Task NonPromptFlowsDoNotPromptOrConsumeInput(
        string scenario,
        bool standardInputRedirected,
        bool promptOutputRedirected,
        int expectedExitCode,
        object expectedStatusValue,
        string expectedRemainingInput)
    {
        using var catalogue = TemporaryWorkspace.Create($"extension-create-composed-{scenario}");
        var before = catalogue.SnapshotHashes();
        var run = await RunAsync(
            Arguments(scenario, catalogue.Path),
            catalogue.Path,
            $"development-toolkit{Environment.NewLine}{catalogue.Path}{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected,
            promptOutputRedirected);

        Assert.Equal(expectedExitCode, run.ExitCode);
        Assert.Equal(Assert.IsType<CliSemanticStatus>(expectedStatusValue), run.Status);
        Assert.DoesNotContain("Stable ID (", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Stable ID (", run.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Catalogue path (", run.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Catalogue path (", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(expectedRemainingInput, run.RemainingInput);
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Theory(DisplayName = "Root Extension group and Create leaf help derive from the composed command tree"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    [InlineData("root")]
    [InlineData("group")]
    [InlineData("leaf")]
    public async Task ComposedHelpIsTruthful(string scope)
    {
        using var workspace = TemporaryWorkspace.Create($"extension-create-help-{scope}");
        var run = await RunAsync(
            HelpArguments(scope),
            workspace.Path,
            string.Empty,
            standardInputRedirected: true,
            promptOutputRedirected: true);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        switch (scope)
        {
            case "root":
                Assert.Contains("extension create  Create one local Extension package scaffold.", run.StandardOutput, StringComparison.Ordinal);
                break;
            case "group":
                Assert.Contains("create   Create one local catalogue scaffold without installing it.", run.StandardOutput, StringComparison.Ordinal);
                Assert.DoesNotContain("create   Planned", run.StandardOutput, StringComparison.Ordinal);
                break;
            case "leaf":
                Assert.Contains("open-forge extension create [<stable-id>]", run.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("--dependency <stable-id>]...", run.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("Required input and interaction", run.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("Workspace and recovery boundary", run.StandardOutput, StringComparison.Ordinal);
                Assert.Contains("Results and streams", run.StandardOutput, StringComparison.Ordinal);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scope), scope, "The help scope is not defined.");
        }

        Assert.Empty(workspace.SnapshotHashes());
    }

    private static string[] Arguments(string scenario, string cataloguePath)
        => scenario switch
        {
            "explicit" => ["extension", "create", "development-toolkit", "--path", cataloguePath, "--dry-run"],
            "redirected" => ["extension", "create", "--dry-run"],
            "automatic" => ["extension", "create", "--automatic", "--dry-run"],
            "json" => ["extension", "create", "--json", "--dry-run"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The non-prompt scenario is not defined."),
        };

    private static string[] HelpArguments(string scope)
        => scope switch
        {
            "root" => [],
            "group" => ["extension"],
            "leaf" => ["extension", "create", "--help"],
            _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, "The help scope is not defined."),
        };

    private static async Task<ExtensionCreateCompositionRun> RunAsync(
        string[] arguments,
        string currentDirectory,
        string standardInput,
        bool standardInputRedirected,
        bool promptOutputRedirected)
    {
        using var input = new StringReader(standardInput);
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
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(currentDirectory),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);
        return new ExtensionCreateCompositionRun
        {
            ExitCode = completion.ExitCode,
            Status = completion.Status,
            StandardOutput = standardOutput.ToString(),
            StandardError = standardError.ToString(),
            RemainingInput = await input.ReadLineAsync(TestContext.Current.CancellationToken),
        };
    }
}

internal sealed record ExtensionCreateCompositionRun
{
    public required int ExitCode { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }

    public required string? RemainingInput { get; init; }
}
