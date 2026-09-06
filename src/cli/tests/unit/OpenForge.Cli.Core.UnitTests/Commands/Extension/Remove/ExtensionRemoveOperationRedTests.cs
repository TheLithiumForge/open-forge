using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveOperationRedTests
{
    [Fact(DisplayName = "Extension Remove operation returns a typed incomplete result for unavailable lifecycle facts"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public async Task OperationReturnsTypedUnavailableLifecycleResult()
    {
        var request = new ExtensionRemoveRequest(
            new CliWorkspace(
                "extension-remove-operation-workspace",
                "extension-remove-operation-workspace",
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            ExtensionRemoveMode.DryRun,
            ["toolkit"],
            prune: false,
            automatic: true,
            allowInteraction: false);
        using var input = new StringReader(string.Empty);
        using var prompts = new StringWriter();
        var operation = ExtensionRemoveOperationFactory.Create(
            new CliInteractiveSession(input, prompts, canPrompt: false));
        CliOperation<ExtensionRemoveRequest, ExtensionRemoveResult> callable = operation.ExecuteAsync;

        var result = await callable(request, TestContext.Current.CancellationToken);

        Assert.Equal("extension remove", result.Command);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code is ExtensionRemoveFindingCode.FrameworkUnavailable
                or ExtensionRemoveFindingCode.LifecycleUnavailable);
        Assert.Equal(string.Empty, prompts.ToString());
    }
}
