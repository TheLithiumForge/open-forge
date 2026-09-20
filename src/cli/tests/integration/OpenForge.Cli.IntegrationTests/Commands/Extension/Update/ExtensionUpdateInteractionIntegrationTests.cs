using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateInteractionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Update freezes the installed selection through revalidation and reviews the final plan once"),
     Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task InteractiveSelectionIsFrozenAcrossRevalidation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-interactive-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-interactive-selection-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit.md", Document("Toolkit v2"));

        var scripted = ScriptedCliTerminal.Lines(["2", "yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var selectorCalls = 0;
        var interaction = ExtensionInteractionTestFactory.ForUpdate(prompts) with
        {
            SelectPackages = async (question, policy, cancellationToken) =>
            {
                selectorCalls++;
                return await prompts.MultiSelectAsync(question, policy, cancellationToken);
            },
        };

        var result = await ExtensionUpdateOperationFactory.Create(
                interaction,
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionUpdateRequest(
                    workspace.Workspace,
                    ExtensionUpdateMode.Apply,
                    [],
                    all: false,
                    sourcePath: source.Path,
                    force: false,
                    prune: false,
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.Equal(1, selectorCalls);
        Assert.Equal(Document("Toolkit v2"), workspace.ReadText(".agents/toolkit.md"));
        Assert.Equal(Document("Base v1"), workspace.ReadText(".agents/base.md"));
        var output = scripted.Output.ToString();
        Assert.Contains("Which Extensions do you want to update?", output, StringComparison.Ordinal);
        Assert.Contains("Apply these changes? [y/N]", output, StringComparison.Ordinal);
        var preview = output.IndexOf("Would update the toolkit Extension to 1.0.0.", StringComparison.Ordinal);
        var previewPath = output.IndexOf(".agents/toolkit.md", StringComparison.Ordinal);
        var apply = output.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(preview >= 0 && preview < apply, "The complete preview must precede confirmation.");
        Assert.True(previewPath >= 0 && previewPath < apply, "The planned path must precede confirmation.");
        Assert.Equal(1, Count(output, "Apply these changes? [y/N]"));
    }

    [Trait("Boundary", "OS")]
    [Theory(
        DisplayName = "Extension Update selection cancellation and EOF leave the workspace unchanged")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SelectionCancellationIsInterrupted(bool endOfInput)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-selection-cancel-{(endOfInput ? "eof" : "blank")}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-selection-cancel-source-{(endOfInput ? "eof" : "blank")}");
        source.AddPackage("alpha", [], (".agents/alpha.md", Document("Alpha")));
        source.AddPackage("beta", [], (".agents/beta.md", Document("Beta")));
        await InstallAllAsync(workspace, source);
        var before = workspace.Snapshot();
        var scripted = endOfInput
            ? ScriptedCliTerminal.Lines([])
            : ScriptedCliTerminal.Lines([string.Empty]);
        var prompts = new CliPrompts(scripted.Terminal);

        var result = await ExtensionUpdateOperationFactory.Create(
                ExtensionInteractionTestFactory.ForUpdate(prompts),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionUpdateRequest(
                    workspace.Workspace,
                    ExtensionUpdateMode.Apply,
                    [],
                    all: false,
                    sourcePath: source.Path,
                    force: false,
                    prune: false,
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionUpdateFindingCode.Interrupted
                && finding.Cause == "Extension update was cancelled. Nothing was changed.");
        Assert.Equal(before, workspace.Snapshot());
        Assert.DoesNotContain("Apply these changes? [y/N]", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Update prune specializes the single final confirmation with the deletion count"),
     Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task PruneUsesOneSpecializedFinalConfirmation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-prune-interaction");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-prune-interaction-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        source.RemovePayload("toolkit", ".agents/toolkit.md");

        var scripted = ScriptedCliTerminal.Lines(["yes"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var result = await ExtensionUpdateOperationFactory.Create(
                ExtensionInteractionTestFactory.ForUpdate(prompts),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionUpdateRequest(
                    workspace.Workspace,
                    ExtensionUpdateMode.Apply,
                    ["toolkit"],
                    all: false,
                    sourcePath: source.Path,
                    force: false,
                    prune: true,
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        var output = scripted.Output.ToString();
        Assert.Contains("Delete the 1 file listed above? [y/N]", output, StringComparison.Ordinal);
        Assert.Equal(1, Count(output, "Delete the 1 file listed above? [y/N]"));
        Assert.DoesNotContain("Apply these changes? [y/N]", output, StringComparison.Ordinal);
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
