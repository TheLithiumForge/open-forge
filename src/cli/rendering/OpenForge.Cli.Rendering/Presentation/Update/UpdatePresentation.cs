using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Presentation.Update.Shared.Help;
using OpenForge.Cli.Core.Presentation.Update.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Update.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Update;

internal static class UpdatePresentation
{
    private static readonly UpdateDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<UpdateResult, UpdateData> Rendering { get; } = new()
    {
        Selector = UpdateReportSelector.Select,
        DataTextRenderer = UpdateDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.UpdateData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Report.Workspace?.Explicit == true
                || selected.Selection.Detail >= CliDetail.Standard
                || selected.Report.Status is CliSemanticStatus.Blocked
                    or CliSemanticStatus.Failed
                    or CliSemanticStatus.Interrupted,
            TextEffects = [],
            TextCounts = [],
            TextFindings = selected.Report.Data.SuppressInterruptedFinding
                ? selected.TextFindings
                    .Where(finding => finding.Code != "update.interrupted")
                    .ToArray()
                : selected.TextFindings,
        },
    };

    internal static CliHelpContent CreateHelp()
        => UpdateHelpSections.Create();
}
