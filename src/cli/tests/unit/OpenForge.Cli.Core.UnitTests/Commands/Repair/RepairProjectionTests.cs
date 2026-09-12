using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class RepairProjectionTests
{
    [Fact(DisplayName = "Repair JSON projection preserves mode relink selection plan no-op lifecycle and counts coordinates"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void JsonProjectionCarriesAllRepairCoordinates()
    {
        var result = NoOpResult();
        var json = RepairJsonRenderer.Render(Presentation(result, CliOutputFormat.Json));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("repair", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal("/tmp/open-forge-repair-workspace", root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());

        var projected = root.GetProperty("result");
        Assert.Equal("dry-run", projected.GetProperty("mode").GetString());
        Assert.True(projected.GetProperty("automatic").GetBoolean());
        Assert.Equal("automatic-and-explicit", projected.GetProperty("selectionMode").GetString());
        var relink = Assert.Single(projected.GetProperty("relinks").EnumerateArray());
        Assert.Equal(1, relink.GetProperty("line").GetInt32());
        Assert.Equal(1, relink.GetProperty("column").GetInt32());
        Assert.Equal("old", relink.GetProperty("expectedDestination").GetString());
        Assert.Equal(".agents/docs/new.md", relink.GetProperty("target").GetProperty("path").GetString());
        Assert.Equal("automatic-and-explicit", projected.GetProperty("selection").GetProperty("mode").GetString());

        var plan = projected.GetProperty("plan");
        Assert.False(plan.GetProperty("blocked").GetBoolean());
        Assert.True(plan.GetProperty("noOp").GetBoolean());
        Assert.Empty(plan.GetProperty("effects").EnumerateArray());
        Assert.Single(plan.GetProperty("noOps").EnumerateArray());
        Assert.Equal("no-op", Assert.Single(plan.GetProperty("steps").EnumerateArray()).GetProperty("outcome").GetString());

        var counts = projected.GetProperty("counts");
        Assert.Equal(1, counts.GetProperty("selectedFindings").GetInt32());
        Assert.Equal(1, counts.GetProperty("noOps").GetInt32());
        Assert.Equal("not-requested", projected.GetProperty("preflight").GetProperty("state").GetString());
        Assert.Equal("not-required", projected.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Null(root.GetProperty("next").GetString());
    }

    [Fact(DisplayName = "Repair JSON envelope and result properties have one deterministic schema order"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void JsonSchemaPropertyOrderIsStable()
    {
        using var document = JsonDocument.Parse(
            RepairJsonRenderer.Render(Presentation(NoOpResult(), CliOutputFormat.Json)));

        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            document.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(
            [
                "libraryExecution",
                "mode",
                "automatic",
                "selectionMode",
                "relinks",
                "diagnosis",
                "selection",
                "plan",
                "affectedPaths",
                "counts",
                "preflight",
                "application",
                "verification",
                "recovery",
                "postDiagnosis",
                "findings",
            ],
            document.RootElement
                .GetProperty("result")
                .EnumerateObject()
                .Select(property => property.Name));
    }

    [Fact(DisplayName = "Repair human and JSON projections expose the same semantic status plan and no-op facts"), Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void HumanAndJsonProjectionParityIsExplicit()
    {
        var result = NoOpResult();
        var human = RepairPresentation.RenderHuman(Presentation(result, CliOutputFormat.Human));
        using var document = JsonDocument.Parse(
            RepairJsonRenderer.Render(Presentation(result, CliOutputFormat.Json)));
        var projected = document.RootElement.GetProperty("result");

        Assert.Contains("Preview of selected repairs", human, StringComparison.Ordinal);
        Assert.Contains("Mode: dry-run", human, StringComparison.Ordinal);
        Assert.Contains("Plan: 1 steps / 0 effects / 1 no-ops", human, StringComparison.Ordinal);
        Assert.Contains("no-op: .agents/docs/guide.md:1:1", human, StringComparison.Ordinal);
        Assert.Contains("No files changed (--dry-run).", human, StringComparison.Ordinal);
        Assert.Contains("Status: complete", human, StringComparison.Ordinal);

        Assert.Equal("complete", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("dry-run", projected.GetProperty("mode").GetString());
        Assert.Equal(
            projected.GetProperty("plan").GetProperty("noOp").GetBoolean(),
            human.Contains("/ 1 no-ops", StringComparison.Ordinal));
        Assert.Equal(
            projected.GetProperty("counts").GetProperty("noOps").GetInt32(),
            projected.GetProperty("plan").GetProperty("noOps").GetArrayLength());
    }

    [Theory(DisplayName = "Both Repair human views preserve selected no-ops and every failure cause"),
        InlineData(false), InlineData(true),
        Trait("Feature", "repair"), Trait("Evidence", "Unit")]
    public void HumanViewsKeepSelectedChangesAndFailures(bool expanded)
    {
        var view = expanded ? CliView.Expanded : CliView.Compact;
        var result = NoOpResult();
        var presentation = new CliPresentationRequest<RepairResult>(result,
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal));
        var before = RepairJsonRenderer.Render(presentation);
        var text = RepairPresentation.RenderHuman(presentation);
        Assert.Contains("no-op: .agents/docs/guide.md:1:1", text, StringComparison.Ordinal);
        Assert.Contains("Repaired: 0; new findings: 0", text, StringComparison.Ordinal);
        Assert.Equal(before, RepairJsonRenderer.Render(presentation));

        var failure = RepairTestData.Result(findings:
            [new RepairFinding(RepairFindingCode.InvalidInput, "Select a current link.")]);
        text = RepairPresentation.RenderHuman(new CliPresentationRequest<RepairResult>(failure,
            new CliPresentation(CliOutputFormat.Human, view, CliVerbosity.Normal)));
        Assert.Contains("Select a current link.", text, StringComparison.Ordinal);
        Assert.Contains("INVALID:", text, StringComparison.Ordinal);
    }

    private static RepairResult NoOpResult()
    {
        var relink = RepairTestData.Relink(expectedDestination: "old");
        var selected = RepairTestData.Selected();
        var selection = RepairTestData.Selection(
            selected,
            RepairSelectionMode.AutomaticAndExplicit);
        var request = RepairTestData.Request(
            automatic: true,
            relinks: [relink]);
        var step = RepairTestData.Step(
            selected,
            noOp: RepairTestData.NoOp(),
            outcome: RepairStepOutcome.NoOp);
        var plan = new RepairPlan(request, selection, [step], [], []);
        var facts = RepairTestData.CompleteFacts(
            selection: selection,
            plan: plan,
            affectedPaths: [RepairTestData.SourcePath],
            counts: new RepairCounts(
                selectedFindings: 1,
                unselectedFindings: 0,
                repaired: 0,
                remaining: 0,
                newFindings: 0,
                manual: 0,
                guided: 0,
                blocked: 0,
                selectedEffects: 0,
                appliedEffects: 0,
                verifiedEffects: 0,
                noOps: 1,
                conflicts: 0));
        return RepairTestData.Result(
            facts: facts,
            automatic: true,
            selectionMode: RepairSelectionMode.AutomaticAndExplicit,
            relinks: [relink]);
    }

    private static CliPresentationRequest<RepairResult> Presentation(
        RepairResult result,
        CliOutputFormat format)
        => new(
            result,
            new CliPresentation(format, CliView.Expanded, CliVerbosity.Normal));
}
