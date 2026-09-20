using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Inspect;

internal static class RouteInspectPresentation
{
    private static readonly RouteInspectDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteInspectResult, Models.RouteInspectData> Rendering { get; } = new()
    {
        Selector = RouteInspectReportSelector.Select,
        DataTextRenderer = RouteInspectDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteInspectData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with { TextCounts = [] },
    };

    internal static CliHelpContent CreateHelp()
        => RouteInspectHelpSections.CreateInspect();
}
