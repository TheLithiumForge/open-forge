using OpenForge.Cli.Core.Shell.Definitions;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Include.Models;
using OpenForge.Cli.Core.Presentation.Route.Include.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Include.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Include;

internal static class RouteIncludePresentation
{
    private static readonly RouteIncludeDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteIncludeResult, RouteIncludeData> Rendering { get; } = new()
    {
        Selector = RouteIncludeReportSelector.Select,
        DataTextRenderer = RouteIncludeDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteIncludeData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with { TextCounts = [] },
    };
}
