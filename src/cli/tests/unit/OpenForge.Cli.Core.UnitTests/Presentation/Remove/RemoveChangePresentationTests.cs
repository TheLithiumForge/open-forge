using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Remove;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Remove;

public sealed class RemoveChangePresentationTests
{
    [Theory(DisplayName = "Root Remove presents each settings navigation and ownership edit once while preserving JSON actions")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "remove-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ControlFileLabels(bool preview)
    {
        var outcome = preview ? "planned" : "done";
        var effects = new RemoveEffect[]
        {
            new(".agents/open-forge.json", "setting", "persist", outcome),
            new(".agents/loader.md", "navigation", "update", outcome),
            new(".agents/open-forge.lock.json", "record", "release-ownership", outcome),
            new(".agents/created.json", "setting", "create", outcome),
            new("old.txt", "file", "delete", outcome),
        };
        var result = new RemoveResult(workspace: null, target: "old.txt", kind: "path", status: CliSemanticStatus.Complete,
            findings: [], effects: effects, removed: ["old.txt"], recoveryPath: null,
            recoveryDisposition: "not-required", dryRun: preview, next: null);
        var rendering = RemovePresentation.Rendering;
        var text = CliRenderingStage.Render(new CliPresentationRequest<RemoveResult>(
            result, new(CliFormat.Text, CliDetail.Standard, null)), rendering).PrimaryContent;
        foreach (var effect in effects.Take(4))
        {
            Assert.Equal(1, text.Split(effect.Path, StringSplitOptions.None).Length - 1);
        }
        CommandOutputSnapshot.MatchSnapshot(text, preview ? "preview" : "applied");
        using var json = JsonDocument.Parse(CliRenderingStage.Render(new CliPresentationRequest<RemoveResult>(
            result, new(CliFormat.Json, CliDetail.Standard, null)), rendering).PrimaryContent);
        Assert.Equal(["replaced", "replaced", "released", "created", "deleted"],
            json.RootElement.GetProperty("effects").EnumerateArray().Select(effect => effect.GetProperty("action").GetString()));
    }
}
