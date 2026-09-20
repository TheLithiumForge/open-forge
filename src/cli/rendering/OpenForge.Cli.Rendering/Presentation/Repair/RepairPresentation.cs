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
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard,
            TextEffects = [],
            TextCounts = selected.Selection.Detail >= CliDetail.Standard
                ? selected.Report.Counts
                    .Select(count => count with
                    {
                        Label = RepairWording.CountLabel(count.Name, count.Value),
                    })
                    .ToArray()
                : [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => RepairHelpSections.Create();
}
