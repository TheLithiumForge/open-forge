using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Sync.Shared.Rendering;

public sealed class LibrarySyncPresentationTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "complete", (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Attention, "attention", (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete", (int)CliOutputTarget.StandardOutput)]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid", (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked", (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Failed, "failed", (int)CliOutputTarget.StandardError)]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted", (int)CliOutputTarget.StandardError)]
    public void EveryStatusUsesConcreteHumanCoordinates(int statusValue, string wireStatus, int humanTargetValue)
    {
        var result = Result(statusValue);
        var renderers = new CliRendererSet<LibrarySyncResult>(LibrarySyncPresentation.RenderHuman, LibrarySyncPresentation.RenderJson);
        var human = CliRenderingStage.Render(new CliPresentationRequest<LibrarySyncResult>(result,
            new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)), renderers, diagnosticRenderer: null);
        Assert.Equal((CliOutputTarget)humanTargetValue, human.PrimaryTarget);
        Assert.Contains("team-knowledge", human.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains(wireStatus, human.PrimaryContent, StringComparison.Ordinal);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "complete")]
    [InlineData((int)CliSemanticStatus.Attention, "attention")]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete")]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid")]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked")]
    [InlineData((int)CliSemanticStatus.Failed, "failed")]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted")]
    public void EveryStatusUsesConcreteJsonCoordinates(int statusValue, string wireStatus)
    {
        var result = Result(statusValue);
        var renderers = new CliRendererSet<LibrarySyncResult>(LibrarySyncPresentation.RenderHuman, LibrarySyncPresentation.RenderJson);
        var json = CliRenderingStage.Render(new CliPresentationRequest<LibrarySyncResult>(result,
            new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)), renderers, diagnosticRenderer: null);
        Assert.Equal(CliOutputTarget.StandardOutput, json.PrimaryTarget);
        using var document = JsonDocument.Parse(json.PrimaryContent);
        Assert.Equal(["schemaVersion", "command", "status", "workspace", "result", "next"],
            [.. document.RootElement.EnumerateObject().Select(property => property.Name)]);
        Assert.Equal(1, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("library sync", document.RootElement.GetProperty("command").GetString());
        Assert.Equal(wireStatus, document.RootElement.GetProperty("status").GetString());
        var application = document.RootElement.GetProperty("result").GetProperty("application");
        Assert.Equal("not-started", application.GetProperty("state").GetString());
        Assert.Equal("not-requested", application.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, application.GetProperty("recordPublication").GetProperty("publishedLast").ValueKind);
        Assert.Empty(application.GetProperty("residuals").EnumerateArray());
    }

    private static LibrarySyncResult Result(int statusValue)
        => new()
        {
            Status = (CliSemanticStatus)statusValue,
            Workspace = LibraryMutationPlanningData.Workspace,
            Next = null,
            Result = new LibrarySyncPayload
            {
                Identity = LibraryMutationPresentationData.Identity(false),
                Record = LibraryMutationPresentationData.Record(),
                Source = LibraryMutationPresentationData.Source(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [],
            },
        };
}
