using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Update.Models;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Update;

internal static class RouteUpdatePresentation
{
    private static readonly RouteUpdateDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteUpdateResult, RouteUpdateData> Rendering { get; } = new()
    {
        Selector = RouteUpdateReportSelector.Select,
        DataTextRenderer = RouteUpdateDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteUpdateData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteUpdateHelpSections.Create();
}
