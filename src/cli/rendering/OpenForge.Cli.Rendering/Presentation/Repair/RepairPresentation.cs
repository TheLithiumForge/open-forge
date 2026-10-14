using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Presentation.Repair.Models;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Help;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Repair;

internal static class RepairPresentation
{
    private static readonly RepairDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<RepairResult, RepairData> Rendering { get; } = new()
    {
        Selector = RepairReportSelector.Select,
        DataTextRenderer = RepairDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RepairData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = SelectText,
    };

    private static CliSelectedReport<RepairData> SelectText(CliSelectedReport<RepairData> selected)
    {
        IReadOnlyList<OpenForge.Cli.Core.Presentation.Shared.Models.CliCount> counts = [];
        var onlyChoices = selected.Report.Status == CliSemanticStatus.Attention
            && selected.Report.Data.Remaining.Count > 0
            && selected.Report.Data.Remaining.All(item => item.Kind == "guided") && selected.Report.Effects.Count == 0;
        var cannotAsk = selected.Report.Findings.Any(finding => finding.Code == "repair.selection-required");
        if (selected.Selection.Detail >= CliDetail.Standard && !onlyChoices && !cannotAsk)
        {
            if (selected.Report.Data.TextLibraryCounts is { } library)
            {
                counts =
                [
                    new("linksRepaired", RepairWording.LibraryLinksRestored(library.Restored, library.Preview), library.Restored),
                    new("problemsRemaining", RepairWording.LibraryLinksRemaining(library.Remaining, library.Preview), library.Remaining),
                ];
            }
            else
            {
                counts = selected.Report.Counts.Select(count => count with
                {
                    Label = RepairWording.CountLabel(count.Name, count.Value),
                }).ToArray();
            }
        }
        return selected with
        {
            TextHeadline = selected.Report.Data.TextHeadline is { } headline
                ? new(headline, selected.Report.Headline.Kind)
                : selected.Report.Headline,
            TextEffects = [],
            TextFindings = selected.TextFindings.Where(finding => finding.Code is not ("repair.rename-proven" or "repair.selection-required"))
                .Select(finding => finding.TextChoices.Count == 0 ? finding : finding with { Actions = [] }).ToArray(),
            TextCounts = counts,
            ShowNextReason = !onlyChoices && !cannotAsk,
        };
    }

    internal static CliHelpContent CreateHelp()
        => RepairHelpSections.Create();
}
