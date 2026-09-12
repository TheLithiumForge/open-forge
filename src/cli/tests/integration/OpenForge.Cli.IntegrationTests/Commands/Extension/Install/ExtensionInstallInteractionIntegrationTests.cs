using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallInteractionIntegrationTests
{
    [Fact(DisplayName = "Extension Install selection prompt retries exact IDs and then applies without generic confirmation"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task SelectionPromptRetriesLocallyAndConsumesNoConfirmation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-selection-prompt");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-selection-prompt-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));

        var run = await workspace.RunAsync(
        [
            "extension", "install",
            "--source", source.Path,
        ],
            $"unknown{Environment.NewLine}toolkit{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Contains("base", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("toolkit", run.StandardError, StringComparison.Ordinal);
        Assert.True(Count(run.StandardError, "base") >= 2, "The complete finite inventory must be shown again after an invalid answer.");
        Assert.True(Count(run.StandardError, "toolkit") >= 2, "The complete finite inventory must be shown again after an invalid answer.");
        Assert.Contains("Packages in dependency order:", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("base", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("toolkit", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Package selection: interactive-ids", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Selected packages: toolkit", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  base: required dependency; requires none", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("  toolkit: selected; requires base", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
        Assert.True(File.Exists(workspace.Combine(".agents/base/_base.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
    }

    [Theory(DisplayName = "Extension Install automatic JSON and redirected requests never prompt or choose a multi-package source"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("automatic")]
    [InlineData("json")]
    [InlineData("redirected")]
    public async Task NonInteractiveSelectionIsInvalidAndDoesNotConsumeInput(string scenario)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create($"extension-install-noninteractive-{scenario}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create($"extension-install-noninteractive-source-{scenario}");
        source.AddPackage("alpha", [], (".agents/alpha/_alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta/_beta.md", Document("Beta")));
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(
            NonInteractiveArguments(scenario, source.Path),
            $"alpha{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: scenario == "redirected",
            promptOutputRedirected: scenario == "redirected");

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Equal("alpha", run.RemainingInput);
        Assert.Equal(before, workspace.Snapshot());
        if (scenario == "json")
        {
            Assert.Equal(string.Empty, run.StandardError);
            using var document = JsonDocument.Parse(run.StandardOutput);
            Assert.Contains(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() == "extension-install.selection-required");
        }
        else
        {
            Assert.Equal(string.Empty, run.StandardOutput);
            Assert.Contains("Status: invalid", run.StandardError, StringComparison.Ordinal);
        }
    }

    [Fact(DisplayName = "Extension Install prompt end of input is invalid and writes nothing"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task PromptEndOfInputIsInvalid()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-prompt-eof");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-prompt-eof-source");
        source.AddPackage("alpha", [], (".agents/alpha/_alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta/_beta.md", Document("Beta")));
        var before = workspace.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install",
            "--source", source.Path,
        ],
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Invalid, run.Status);
        Assert.Contains("Status: invalid", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("extension-install.interaction-ended", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Extension Install caller cancellation during selection is interrupted and writes nothing"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task PromptCancellationIsInterrupted()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-prompt-cancel");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-prompt-cancel-source");
        source.AddPackage("alpha", [], (".agents/alpha/_alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta/_beta.md", Document("Beta")));
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        using var input = new CancellingTextReader(cancellation);

        var run = await workspace.RunAsync(
        [
            "extension", "install",
            "--source", source.Path,
        ],
            input,
            standardInputRedirected: false,
            promptOutputRedirected: false,
            cancellationToken: cancellation.Token,
            readRemainingInput: false);

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains("Status: interrupted", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact(DisplayName = "Extension Install force prompt is exact authority and explicit force adds no generic confirmation"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task EligibleInitialForceIsTheOnlyApplyPrompt()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-force-prompt");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-force-prompt-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        workspace.CreateOccupant(".agents/toolkit.md", "preserve occupant\n");
        var beforeDecline = workspace.Snapshot();

        var declined = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
        ],
            $"no{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(5, declined.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, declined.Status);
        Assert.Contains(".agents/toolkit.md", declined.StandardError, StringComparison.Ordinal);
        Assert.Equal("remaining", declined.RemainingInput);
        Assert.Equal(beforeDecline, workspace.Snapshot());

        var applied = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--force",
        ],
            $"remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Equal("remaining", applied.RemainingInput);
        Assert.Contains("# Toolkit", workspace.ReadText(".agents/toolkit.md"), StringComparison.Ordinal);
        Assert.Contains("Recovery:", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("removed", applied.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Install initial-force next preserves every normalized request flag"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task InitialForceNextPreservesNormalizedRequest()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-force-next");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-force-next-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        workspace.CreateOccupant(".agents/toolkit.md", "plain initial occupant\n");
        var before = workspace.Snapshot();

        var result = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--dry-run", "--json",
            "--workspace", workspace.Path,
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(
            $"open-forge extension install toolkit --source '{source.Path}' --force --automatic --dry-run --workspace '{workspace.Path}'",
            document.RootElement.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(before, workspace.Snapshot());
    }

    private static string[] NonInteractiveArguments(string scenario, string sourcePath)
    {
        var arguments = new List<string>
        {
            "extension", "install",
            "--source", sourcePath,
        };
        if (scenario == "automatic")
        {
            arguments.Add("--automatic");
        }

        if (scenario == "json")
        {
            arguments.Add("--json");
        }

        return [.. arguments];
    }

    private static string Document(string name)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            name,
            ["Extension"],
            $"# {name}\n");

    private static int Count(string value, string needle)
        => value.Split(needle, StringSplitOptions.None).Length - 1;
}

internal sealed class CancellingTextReader(CancellationTokenSource cancellation) : TextReader
{
    private bool _cancelled;

    public override ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        if (_cancelled)
        {
            return ValueTask.FromResult<string?>(null);
        }

        _cancelled = true;
        cancellation.Cancel();
        return ValueTask.FromCanceled<string?>(cancellation.Token);
    }
}
