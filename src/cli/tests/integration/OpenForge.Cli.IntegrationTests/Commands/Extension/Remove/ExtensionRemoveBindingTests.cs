using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveBindingTests
{
    [Fact(DisplayName = "Extension Remove binding reports duplicate typed IDs as an invalid result"), Trait("Feature", "extension-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task BindingRejectsDuplicateTypedIds()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var result = await InvokeBindingAsync(
            symbols,
            ["toolkit", "toolkit"],
            CliFormat.Text);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.InvalidInput);
    }

    [Fact(DisplayName = "Extension Remove binding requires IDs for a non-prompt JSON request"), Trait("Feature", "extension-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task BindingRequiresIdsOutsidePromptCapableHumanMode()
    {
        var symbols = ExtensionRemoveBinding.CreateSymbols(ExtensionBinding.CreateGroup());
        var result = await InvokeBindingAsync(symbols, [], CliFormat.Json);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.SelectionRequired);
        Assert.Equal(
            "open-forge extension list",
            Assert.IsType<CliNextAction>(result.Next).Command);
    }

    private static async ValueTask<ExtensionRemoveResult> InvokeBindingAsync(
        ExtensionRemoveSymbols symbols,
        string[] arguments,
        CliFormat format)
    {
        ExtensionRemoveResult? observed = null;
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionRemoveOperationFactory.Create(
            ExtensionInteractionTestFactory.ForRemove(prompts));
        var binding = OpenForge.Cli.Core.Shell.Composition.CliReportBinding.Close(ExtensionRemoveBinding.CreateRequestBinding(
            symbols,
            CliHelpContent.Empty,
            operation),
            OpenForge.Cli.Core.UnitTests.Commands.Shared.Presentation.CommandBindingTestRendering.Create<ExtensionRemoveResult>(selected: result => observed = result));
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();

        var parse = symbols.Command.Parse(arguments);
        var completion = await ((ICliCommandBinding)binding).InvokeAsync(
            new CliBindingParse(parse, arguments),
            Invocation(format),
            new CliOutputWriters(standardOutput, standardError),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, completion.Status);
        return Assert.IsType<ExtensionRemoveResult>(observed);
    }

    private static CliInvocation Invocation(CliFormat format)
    {
        const string path = "extension-remove-binding-workspace";
        var workspace = new CliWorkspace(
            path,
            path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliDetail.Standard, null),
            CliTerminalMode.None,
            new CliWorkspaceRequest(null, path),
            workspace);
    }
}
