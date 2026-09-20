using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallInteractionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install selection prompt retries exact IDs and then applies after final confirmation"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
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
            "--detail", "standard",
        ],
            $"unknown{Environment.NewLine}2{Environment.NewLine}yes{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Contains("base", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("toolkit", run.StandardError, StringComparison.Ordinal);
        Assert.True(Count(run.StandardError, "base") >= 2, "The complete finite inventory must be shown again after an invalid answer.");
        Assert.True(Count(run.StandardError, "toolkit") >= 2, "The complete finite inventory must be shown again after an invalid answer.");
        Assert.Contains("Dependency order: base, toolkit", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("base", run.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("toolkit", run.StandardOutput, StringComparison.Ordinal);
        Assert.Equal("remaining", run.RemainingInput);
        Assert.True(File.Exists(workspace.Combine(".agents/base/_base.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
    }

    [Trait("Boundary", "OS")]
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
            Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() == "extension-install.selection-required");
        }
        else
        {
            Assert.Equal(string.Empty, run.StandardOutput);
            Assert.Contains("Cannot install:", run.StandardError, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install prompt end of input is interrupted and writes nothing"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
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

        Assert.Equal(130, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, run.Status);
        Assert.Contains("Extension install was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
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
        Assert.Contains("Extension install was cancelled. Nothing was changed.", run.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install force prompt is exact authority and explicit force keeps only final confirmation"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
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

        Assert.Equal(130, declined.ExitCode);
        Assert.Equal(CliSemanticStatus.Interrupted, declined.Status);
        Assert.Contains(".agents/toolkit.md", declined.StandardError, StringComparison.Ordinal);
        Assert.Equal("remaining", declined.RemainingInput);
        Assert.Equal(beforeDecline, workspace.Snapshot());

        var applied = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--force",
            "--detail", "full",
        ],
            $"yes{Environment.NewLine}remaining{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.DoesNotContain("Replace the", applied.StandardError, StringComparison.Ordinal);
        Assert.Contains("Apply these changes? [y/N]", applied.StandardError, StringComparison.Ordinal);
        Assert.Equal("remaining", applied.RemainingInput);
        Assert.Contains("# Toolkit", workspace.ReadText(".agents/toolkit.md"), StringComparison.Ordinal);
        Assert.Contains("Recovery:", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("removed", applied.StandardOutput, StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install asks for permission before replacement and final plan approval")]
    public async Task PermissionPrecedesReplacementAndFinalApproval()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-permission-force-order");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateDirectory(".apm/agents");
        workspace.CreateOccupant(PermissionFixture.ExternalPath, "existing occupant\n");
        try
        {
            var run = await workspace.RunAsync(
            [
                "extension", "install", "team",
                "--source", source.Path,
            ],
                $"always{Environment.NewLine}yes{Environment.NewLine}yes{Environment.NewLine}sentinel{Environment.NewLine}",
                standardInputRedirected: false,
                promptOutputRedirected: false);

            Assert.Equal(0, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Complete, run.Status);
            Assert.Equal("sentinel", run.RemainingInput);
            Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            using var settings = JsonDocument.Parse(workspace.ReadText(PermissionFixture.PermissionPath));
            Assert.Contains(
                settings.RootElement.GetProperty("allowInstallPaths").EnumerateArray(),
                path => path.GetString() == PermissionFixture.ExternalPath);

            var output = run.StandardError;
            var permission = output.IndexOf("always, once, cancel:", StringComparison.Ordinal);
            var replacement = output.IndexOf(
                "Replace the 1 existing file listed above? [y/N]",
                StringComparison.Ordinal);
            var apply = output.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
            Assert.True(permission >= 0, "The permission prompt was not rendered.");
            Assert.True(replacement > permission, "Replacement approval must follow permission approval.");
            Assert.True(apply > replacement, "Final plan approval must follow replacement approval.");
            Assert.Equal(1, Count(output, "always, once, cancel:"));
            Assert.Equal(1, Count(output, "Replace the 1 existing file listed above? [y/N]"));
            Assert.Equal(1, Count(output, "Apply these changes? [y/N]"));
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install keeps trusted installed dependencies disabled and untouched")]
    public async Task InstalledDependencyIsNotASecondMutationTarget()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-installed-dependency");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-installed-dependency-source");
        source.AddPackage("base", [], (".agents/base.md", Document("Base")));
        source.AddPackage("toolkit", ["base"], (".agents/toolkit.md", Document("Toolkit")));

        var installed = await workspace.RunAsync(
        [
            "extension", "install", "base",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);
        Assert.Equal(0, installed.ExitCode);
        var baseBytes = File.ReadAllBytes(workspace.Combine(".agents/base.md"));
        var beforeOwnership = workspace.ReadExtensionOwnership();
        var beforeBaseOwnership = Assert.Single(
            beforeOwnership.EnumerateArray(),
            extension => extension.GetProperty("id").GetString() == "base").GetRawText();

        var run = await workspace.RunAsync(
        [
            "extension", "install",
            "--source", source.Path,
        ],
            $"2{Environment.NewLine}yes{Environment.NewLine}sentinel{Environment.NewLine}",
            standardInputRedirected: false,
            promptOutputRedirected: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal("sentinel", run.RemainingInput);
        Assert.Equal(baseBytes, File.ReadAllBytes(workspace.Combine(".agents/base.md")));
        var afterBaseOwnership = Assert.Single(
            workspace.ReadExtensionOwnership().EnumerateArray(),
            extension => extension.GetProperty("id").GetString() == "base").GetRawText();
        Assert.Equal(beforeBaseOwnership, afterBaseOwnership);
        Assert.Contains("base", run.StandardError, StringComparison.Ordinal);
        Assert.Contains("installed", run.StandardError, StringComparison.Ordinal);
        Assert.True(File.Exists(workspace.Combine(".agents/toolkit.md")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install dry-run reports force prerequisite without opening a force prompt")]
    public async Task DryRunDoesNotPromptForInitialForce()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-dry-run-force-prompt");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        workspace.CreateDirectory(".apm/agents");
        workspace.CreateOccupant(PermissionFixture.ExternalPath, "existing occupant\n");
        workspace.CreateOccupant(PermissionFixture.PermissionPath, PermissionFixture.Grants);
        var before = workspace.Snapshot();
        try
        {
            var run = await workspace.RunAsync(
            [
                "extension", "install", "team",
                "--source", source.Path,
                "--dry-run",
            ],
                $"remaining{Environment.NewLine}",
                standardInputRedirected: false,
                promptOutputRedirected: false);

            Assert.Equal(5, run.ExitCode);
            Assert.Equal(CliSemanticStatus.Blocked, run.Status);
            Assert.Equal("remaining", run.RemainingInput);
            Assert.DoesNotContain("Replace the", run.StandardError, StringComparison.Ordinal);
            Assert.Equal(before, workspace.Snapshot());
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    [Trait("Boundary", "OS")]
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
            "--automatic", "--dry-run", "--format", "json",
            "--workspace", workspace.Path,
        ]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(
            "open-forge extension install toolkit --force --dry-run",
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
            arguments.Add("--format=json");
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
