using System.Text.Json;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusPresentationTests
{
    [Fact(DisplayName = "Status human views preserve required facts and contracted next actions"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HumanViewsPreserveRequiredFactsAndContractedNextActions()
    {
        var result = StatusResultSeeds.Representative(
            CliSemanticStatus.Attention,
            StatusDefinitions.AttentionNextAction);

        var expanded = StatusHumanRenderer.Render(Presentation(result, CliView.Expanded));
        var compact = StatusHumanRenderer.Render(Presentation(result, CliView.Compact));

        foreach (var output in new[] { expanded, compact })
        {
            Assert.Contains("Open Forge is installed.", output, StringComparison.Ordinal);
            Assert.Contains("Workspace:", output, StringComparison.Ordinal);
            Assert.Contains("Selected by:", output, StringComparison.Ordinal);
            Assert.Contains("Status: requires attention", output, StringComparison.Ordinal);
            Assert.Contains("Open Forge is installed.", output, StringComparison.Ordinal);
            Assert.Contains("Startup:", output, StringComparison.Ordinal);
            Assert.Contains("Startup share: 25%", output, StringComparison.Ordinal);
            Assert.Contains("Continuity (may load again)", output, StringComparison.Ordinal);
            Assert.Contains("Root categories:", output, StringComparison.Ordinal);
            Assert.Contains("Extensions:", output, StringComparison.Ordinal);
            Assert.Contains("Managed files:", output, StringComparison.Ordinal);
            Assert.Contains("Verified recovery records:", output, StringComparison.Ordinal);
            Assert.Contains("Incomplete drafts:", output, StringComparison.Ordinal);
            Assert.Contains("Next: open-forge doctor", output, StringComparison.Ordinal);
            Assert.Single(
                output.Split(Environment.NewLine),
                line => line.TrimStart().StartsWith("Next:", StringComparison.Ordinal));
            Assert.DoesNotContain("open-forge repair", output, StringComparison.Ordinal);
        }

        Assert.Contains("Total available context", expanded, StringComparison.Ordinal);
        Assert.Contains("Largest continuity sources", expanded, StringComparison.Ordinal);
        Assert.DoesNotContain("Largest continuity sources", compact, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Status human results preserve every semantic status and shared primary stream"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HumanPrimaryStreamsFollowEverySemanticStatus()
    {
        var expected = new (CliSemanticStatus Status, string Label, CliOutputTarget Target)[]
        {
            (CliSemanticStatus.Complete, "complete", CliOutputTarget.StandardOutput),
            (CliSemanticStatus.Attention, "requires attention", CliOutputTarget.StandardOutput),
            (CliSemanticStatus.Incomplete, "incomplete", CliOutputTarget.StandardOutput),
            (CliSemanticStatus.Invalid, "invalid", CliOutputTarget.StandardError),
            (CliSemanticStatus.Blocked, "blocked", CliOutputTarget.StandardError),
            (CliSemanticStatus.Failed, "failed", CliOutputTarget.StandardError),
            (CliSemanticStatus.Interrupted, "interrupted", CliOutputTarget.StandardError),
        };

        foreach (var (status, label, target) in expected)
        {
            var result = StatusResultSeeds.Representative(status, Next(status));
            var output = StatusHumanRenderer.Render(Presentation(result, CliView.Compact));

            Assert.Contains($"Status: {label}", output, StringComparison.Ordinal);
            Assert.Equal(target, CliStatusDefinitions.Read(result.Status).Disposition.HumanOutputTarget);
        }
    }

    [Fact(DisplayName = "Status human event results retain honest unavailable Library facts"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HumanEventResultsRetainHonestUnavailableLibraryFacts()
    {
        var result = StatusResultBuilder.Event(
            null,
            StatusFindingCode.Interrupted,
            null,
            "Status observation was interrupted.");

        var output = StatusHumanRenderer.Render(Presentation(result, CliView.Compact));

        Assert.Contains("Libraries", output, StringComparison.Ordinal);
        Assert.Contains("State: incomplete", output, StringComparison.Ordinal);
        Assert.Contains("Record: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("registered: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("current: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("missing: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("changed: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("blocked: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("unavailable: unavailable", output, StringComparison.Ordinal);
        Assert.Contains("Records: unavailable", output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Status human and JSON renderers project the same frozen result"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HumanAndJsonRenderersProjectTheSameFrozenResult()
    {
        var result = StatusResultSeeds.Representative(
            CliSemanticStatus.Attention,
            StatusDefinitions.AttentionNextAction);

        var human = StatusHumanRenderer.Render(Presentation(result, CliView.Expanded));
        var json = StatusJsonRenderer.Render(new CliPresentationRequest<StatusResult>(
            result,
            new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));

        Assert.NotEmpty(human);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal("status", parsed.RootElement.GetProperty("command").GetString());
        Assert.Equal("attention", parsed.RootElement.GetProperty("status").GetString());
        Assert.Contains("Status: requires attention", human, StringComparison.Ordinal);
        Assert.NotNull(result.Workspace);
        Assert.Equal(result.Workspace.LexicalRoot, parsed.RootElement.GetProperty("workspace").GetProperty("path").GetString());
    }

    private static CliPresentationRequest<StatusResult> Presentation(StatusResult result, CliView view)
        => new(
            result,
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal));

    private static CliNextAction? Next(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => StatusDefinitions.AttentionNextAction,
            CliSemanticStatus.Incomplete => StatusDefinitions.IncompleteNextAction,
            CliSemanticStatus.Invalid => StatusDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked => StatusDefinitions.BlockedNextAction,
            CliSemanticStatus.Failed => StatusDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => StatusDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Status result is not defined."),
        };
}
