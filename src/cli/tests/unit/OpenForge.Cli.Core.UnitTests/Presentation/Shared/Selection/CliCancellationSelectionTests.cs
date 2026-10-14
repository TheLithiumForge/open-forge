using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Selection;

public sealed class CliCancellationSelectionTests
{
    [Theory(DisplayName = "Pre-effect cancellation removes only human interruption diagnostics"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(nameof(CliDetail.Minimal))]
    [InlineData(nameof(CliDetail.Standard))]
    [InlineData(nameof(CliDetail.Full))]
    [InlineData(nameof(CliDetail.Debug))]
    public void PureCancellationPreservesReport(string detailName)
    {
        var detail = Enum.Parse<CliDetail>(detailName);
        var selected = Selected(detail, []);
        var text = CliCancellationSelection.BeforeEffects(selected, "example.interrupted");
        Assert.Same(selected.Report, text.Report);
        Assert.Empty(text.TextFindings);
        Assert.Equal(0, Assert.Single(text.TextCounts, count => count.Name == "errors").Value);
        Assert.Empty(text.TextLimitations);
        Assert.Empty(text.TextDiagnostics);
        Assert.Single(text.Report.Findings);
        Assert.Equal(detail == CliDetail.Debug ? 1 : 0, text.Report.Diagnostics.Count);
        Assert.Equal(1, Assert.Single(text.Report.Counts, count => count.Name == "errors").Value);
    }

    [Theory(DisplayName = "Cancellation preserves real and uncertain partial effects"), Trait("Boundary", "Output"), Trait("Feature", "cli-presentation"), Trait("Evidence", "Unit")]
    [InlineData(nameof(CliEffectOutcome.Done))]
    [InlineData(nameof(CliEffectOutcome.Unknown))]
    [InlineData(nameof(CliEffectOutcome.Failed))]
    public void PartialEffectsRemainVisible(string outcomeName)
    {
        var outcome = Enum.Parse<CliEffectOutcome>(outcomeName);
        var selected = Selected(CliDetail.Full, [new CliEffect { Path = "source.md", Kind = CliEffectKind.File, Action = CliEffectAction.Deleted, Outcome = outcome }]);
        Assert.Same(selected, CliCancellationSelection.BeforeEffects(selected, "example.interrupted"));
    }

    private static CliSelectedReport<object> Selected(CliDetail detail, IReadOnlyList<CliEffect> effects)
        => CliReportTrimmer.Trim(new CliReport<object>
        {
            Command = "example",
            Status = CliSemanticStatus.Interrupted,
            Headline = new CliHeadline("Example was cancelled. Nothing was changed.", CliHeadlineKind.Cancelled),
            Findings = [new CliFinding { Code = "example.interrupted", Severity = CliSeverity.Error, Title = "Cancelled", Message = "Cancelled", Subject = new CliSubject(CliSubjectKind.Identifier, Id: "example") }],
            Limitations = [new CliLimitation("scan", "The scan was interrupted.")],
            Effects = effects,
            Diagnostics = ["The operation was interrupted."],
            Data = new object(),
        }, new CliSelection(detail), CliCommandShape.ChangeReport);
}
