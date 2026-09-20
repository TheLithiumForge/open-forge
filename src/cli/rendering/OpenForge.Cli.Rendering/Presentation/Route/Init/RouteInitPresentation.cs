using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Init.Models;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Init.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Init;

internal static class RouteInitPresentation
{
    private static readonly RouteInitDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteInitResult, RouteInitData> Rendering { get; } = new()
    {
        Selector = RouteInitReportSelector.Select,
        DataTextRenderer = RouteInitDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteInitData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteInitHelpSections.Create();
}
