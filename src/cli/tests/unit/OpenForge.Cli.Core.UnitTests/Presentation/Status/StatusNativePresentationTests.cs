using System.Text.Json;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Status;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.UnitTests.Commands.Status;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Status;

public sealed class StatusNativePresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Native Status healthy minimal output keeps the two-line contract"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void HealthyMinimalOutputKeepsTheTwoLineContract()
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        var result = seed with
        {
            Workspace = new CliWorkspace(
                StatusObservationSeeds.Workspace().LexicalRoot,
                StatusObservationSeeds.Workspace().PhysicalRoot,
                CliWorkspaceSelectionMethod.CurrentDirectory),
            Facts = seed.Facts with
            {
                Lifecycle = new StatusLifecycle(
                    seed.Facts.Lifecycle.Framework,
                    EmptyExtensions()),
            },
        };

        var text = Text(result, CliDetail.Minimal);

        Assert.Equal(
            "Open Forge is installed and current.\n"
            + "  Startup reads 4 of 7 routed files, about 0.0k tokens.\n",
            text);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Native Status selects count and row shapes by detail"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void JsonSelectsCountAndRowShapesByDetail()
    {
        var result = StatusResultSeeds.Representative(CliSemanticStatus.Complete);

        using var standard = JsonDocument.Parse(Json(result, CliDetail.Standard));
        using var full = JsonDocument.Parse(Json(result, CliDetail.Full));
        var standardExtension = Assert.Single(standard.RootElement.GetProperty("data").GetProperty("extensions").EnumerateArray());
        var fullExtension = Assert.Single(full.RootElement.GetProperty("data").GetProperty("extensions").EnumerateArray());

        Assert.Equal(JsonValueKind.Object, standardExtension.GetProperty("files").ValueKind);
        Assert.Equal(JsonValueKind.Array, fullExtension.GetProperty("files").ValueKind);
        Assert.Contains(fullExtension.GetProperty("files").EnumerateArray(), row =>
            row.GetProperty("path").GetString() == ".agents/shared.md");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Native Status keeps a matching blocked finding as the headline cause"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void BlockedHeadlineUsesTheMatchingFinding()
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Blocked);
        var result = seed with
        {
            Findings =
            [
                new StatusFinding
                {
                    Code = StatusFindingCode.ContextInventoryIncomplete,
                    Status = CliSemanticStatus.Incomplete,
                    Subject = ".agents/maps/_maps.md",
                    Cause = "The context measurement is incomplete.",
                },
                new StatusFinding
                {
                    Code = StatusFindingCode.WorkspaceUnsafe,
                    Status = CliSemanticStatus.Blocked,
                    Subject = ".agents/loader.md",
                    Cause = "The workspace identity is ambiguous.",
                },
            ],
        };

        var selected = Select(result, CliDetail.Minimal);

        Assert.Equal(
            "Cannot check this workspace: The workspace identity is ambiguous.\n",
            selected.Report.Headline.Sentence + "\n");
        Assert.Equal("status.workspace-unsafe", selected.Report.HeadlineFindingCode);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Native Status leaves not-applicable counts without limitations"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void NotApplicableCountsHaveNoUnavailableLimitation()
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        var notApplicable = new StatusMeasurement(
            new StatusIntegerValue(StatusValueState.NotApplicable, null),
            new StatusIntegerValue(StatusValueState.NotApplicable, null),
            new StatusIntegerValue(StatusValueState.NotApplicable, null),
            new StatusIntegerValue(StatusValueState.NotApplicable, null));
        var result = seed with
        {
            Facts = seed.Facts with
            {
                Context = seed.Facts.Context with
                {
                    Startup = seed.Facts.Context.Startup with { Current = notApplicable },
                },
            },
        };

        var selected = Select(result, CliDetail.Minimal);

        Assert.Null(Assert.Single(selected.Report.Counts, count => count.Name == "startupFiles").UnavailableReason);
        Assert.DoesNotContain(selected.Report.Limitations, limitation => limitation.What == "startup files");
    }

    private static StatusExtensionLifecycle EmptyExtensions()
        => new()
        {
            State = StatusLifecycleState.Absent,
            SourceAvailability = StatusSourceAvailability.NotApplicable,
            Installed = [],
            ManagedFiles = new StatusManagedExtensionFiles
            {
                Counts = new StatusManagedTargetCounts
                {
                    Current = StatusResultSeeds.Available(0),
                    Changed = StatusResultSeeds.Available(0),
                    Missing = StatusResultSeeds.Available(0),
                    Unavailable = StatusResultSeeds.Available(0),
                    Blocked = StatusResultSeeds.Available(0),
                },
                Targets = [],
            },
        };

    private static CliSelectedReport<StatusData> Select(StatusResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = StatusPresentation.Rendering;
        return rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }

    private static string Text(StatusResult result, CliDetail detail)
    {
        var selected = Select(result, detail);
        return CliTextRenderer.Render(selected, CliTextStyle.Plain, StatusPresentation.Rendering.DataTextRenderer).Content;
    }

    private static string Json(StatusResult result, CliDetail detail)
    {
        var selected = Select(result, detail);
        return CliJsonRenderer.Render(selected, StatusPresentation.Rendering.DataJsonTypeInfo);
    }
}
