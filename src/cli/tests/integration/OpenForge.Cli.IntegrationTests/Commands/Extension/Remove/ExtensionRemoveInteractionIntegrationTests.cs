using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveInteractionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove selects dependents and freezes the closure through revalidation"),
     Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    public async Task InteractiveSelectionIncludesDependentsAndIsFrozen()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-interactive-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-interactive-selection-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);

        var scripted = ScriptedCliTerminal.Lines(["1", "yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var selectorCalls = 0;
        var interaction = ExtensionInteractionTestFactory.ForRemove(prompts) with
        {
            SelectPackages = async (question, policy, cancellationToken) =>
            {
                selectorCalls++;
                return await prompts.MultiSelectAsync(question, policy, cancellationToken);
            },
        };

        var result = await ExtensionRemoveOperationFactory.Create(
                interaction,
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionRemoveRequest(
                    workspace.Workspace,
                    ExtensionRemoveMode.Apply,
                    [],
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Equal(1, selectorCalls);
        Assert.False(File.Exists(workspace.Combine(".agents/base.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Empty(workspace.ReadExtensionOwnership().EnumerateArray());
        var output = scripted.Output.ToString();
        Assert.Contains("Which Extensions do you want to remove?", output, StringComparison.Ordinal);
        Assert.Contains("needed by", output, StringComparison.Ordinal);
        Assert.Contains("Delete the 2 files listed above? [y/N]", output, StringComparison.Ordinal);
        var preview = output.IndexOf("Would remove the", StringComparison.Ordinal);
        var previewPath = output.IndexOf(".agents/base.md", StringComparison.Ordinal);
        var delete = output.IndexOf("Delete the 2 files listed above? [y/N]", StringComparison.Ordinal);
        Assert.True(preview >= 0 && preview < delete, "The complete preview must precede confirmation.");
        Assert.True(previewPath >= 0 && previewPath < delete, "The planned path must precede confirmation.");
        Assert.Equal(1, Count(output, "Delete the 2 files listed above? [y/N]"));
    }

    [Trait("Boundary", "OS")]
    [Theory(
        DisplayName = "Extension Remove selection cancellation and EOF leave the workspace unchanged")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelectionCancellationIsInterrupted(bool endOfInput)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-selection-cancel-{(endOfInput ? "eof" : "blank")}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-selection-cancel-source-{(endOfInput ? "eof" : "blank")}");
        source.AddPackage("alpha", [], (".agents/alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta.md", Document("Beta")));
        await InstallAllAsync(workspace, source);
        var before = workspace.Snapshot();
        var scripted = endOfInput
            ? ScriptedCliTerminal.Lines([])
            : ScriptedCliTerminal.Lines([string.Empty]);
        var prompts = new CliPrompts(scripted.Terminal);

        var result = await ExtensionRemoveOperationFactory.Create(
                ExtensionInteractionTestFactory.ForRemove(prompts),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionRemoveRequest(
                    workspace.Workspace,
                    ExtensionRemoveMode.Apply,
                    [],
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.Interrupted
                && finding.Cause == "Extension remove was cancelled. Nothing was changed.");
        Assert.Equal(before, workspace.Snapshot());
        Assert.DoesNotContain("Delete the ", scripted.Output.ToString(), StringComparison.Ordinal);
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

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");

    private static int Count(string value, string needle)
        => value.Split(needle, StringSplitOptions.None).Length - 1;
}
