using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Remove;

internal static class RouteRemovePresentation
{
    private static readonly RouteRemoveDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteRemoveResult, Models.RouteRemoveData> Rendering { get; } = new()
    {
        Selector = RouteRemoveReportSelector.Select,
        DataTextRenderer = RouteRemoveDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteRemoveData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard
                || selected.Report.Status is CliSemanticStatus.Blocked
                    or CliSemanticStatus.Failed
                    or CliSemanticStatus.Interrupted,
            TextEffects = [],
            TextCounts = [],
            Report = selected.Report with
            {
                Next = selected.Selection.Detail == CliDetail.Minimal
                    && selected.Report.Next is { } next
                    && next.Command == "open-forge cleanup"
                    ? new CliNextAction($"{next.Command}  ({next.Reason})", next.Reason)
                    : selected.Report.Next,
            },
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteRemoveHelpSections.Create();
}
