using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.List.Models;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.List;

internal static class RouteListPresentation
{
    private static readonly RouteListDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteListResult, Models.RouteListData> Rendering { get; } = new()
    {
        Selector = RouteListReportSelector.Select,
        DataTextRenderer = RouteListDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteListData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with
        {
            ShowHeadline = !(selected.Report.Status == CliSemanticStatus.Complete
                && selected.Report.Data.Rows.Count > 0
                && selected.Report.Findings.Count == 0),
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteListHelpSections.CreateList();
}
