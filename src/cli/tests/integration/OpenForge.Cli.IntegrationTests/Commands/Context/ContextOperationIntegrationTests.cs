using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextOperationIntegrationTests
{
    [Theory(DisplayName = "Context real-filesystem operation maps injected failure and pre-cancellation without writes"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    [InlineData(false, (int)CliSemanticStatus.Failed, (int)ContextFindingCode.OperationFailed)]
    [InlineData(true, (int)CliSemanticStatus.Interrupted, (int)ContextFindingCode.Interrupted)]
    public async Task TerminalEventsRemainTypedAndReadOnly(
        bool interrupted,
        int expectedStatusValue,
        int expectedFindingValue)
    {
        using var workspace = TemporaryWorkspace.Create("context-terminal-event");
        workspace.WriteText("AGENTS.md", "# Workspace\n");
        var before = workspace.SnapshotHashes();
        var selected = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.CurrentDirectory);
        var part = new ContextContentPart(
            kind: ContextContentPartKind.Metadata,
            name: null,
            canonicalValue: "metadata");
        var request = new ContextRequest(
            workspace: selected,
            sourceReferences: [],
            additionsOnly: false,
            content: new ContextContentSelection(supplied: [part], effective: [part]),
            linkExpansion: ContextLinkExpansion.None,
            suppliedView: null,
            effectiveView: CliView.Expanded);
        using var cancellation = new CancellationTokenSource();
        ContextOperation operation;
        if (interrupted)
        {
            cancellation.Cancel();
            operation = ContextOperationFactory.Create();
        }
        else
        {
            operation = new ContextOperation((_, _) => throw new IOException("Injected Context integration failure."));
        }

        var result = await operation.ExecuteAsync(request, cancellation.Token);

        Assert.Equal((CliSemanticStatus)expectedStatusValue, result.Status);
        Assert.Equal((ContextFindingCode)expectedFindingValue, Assert.Single(result.Findings).Code);
        var presentation = new CliPresentation(
            CliOutputFormat.Json,
            CliView.Expanded,
            CliVerbosity.Normal);
        var json = ContextJsonRenderer.Render(new CliPresentationRequest<ContextResult>(result, presentation));
        using var document = JsonDocument.Parse(json);
        Assert.Equal(
            interrupted ? "interrupted" : "failed",
            document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            interrupted ? "interrupted" : "failed",
            document.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("state").GetString());
        Assert.Equal(
            CliOutputTarget.StandardError,
            CliStatusDefinitions.Read(result.Status).Disposition.HumanOutputTarget);
        Assert.Equal(interrupted ? 130 : 1, CliStatusDefinitions.Read(result.Status).Disposition.ExitCode);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
