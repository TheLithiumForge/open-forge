using OpenForge.Cli.Core.Shell.Definitions;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Models;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Exclude.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Exclude;

internal static class RouteExcludePresentation
{
    private static readonly RouteExcludeDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteExcludeResult, RouteExcludeData> Rendering { get; } = new()
    {
        Selector = RouteExcludeReportSelector.Select,
        DataTextRenderer = RouteExcludeDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteExcludeData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with { TextCounts = [] },
    };
}
