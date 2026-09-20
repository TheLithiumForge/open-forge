using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Presentation.Cleanup.Models;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Help;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Cleanup.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Cleanup;

internal static class CleanupPresentation
{
    private static readonly CleanupDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<CleanupResult, CleanupData> Rendering { get; } = new()
    {
        Selector = CleanupReportSelector.Select,
        DataTextRenderer = CleanupDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.CleanupData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard,
            TextFindings = selected.Selection.Detail == CliDetail.Minimal
                && selected.Report.HeadlineFindingCode is not null
                    ? selected.TextFindings
                        .Where(finding => finding.Code != selected.Report.HeadlineFindingCode)
                        .ToArray()
                    : selected.TextFindings,
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => CleanupHelpSections.Create();
}
