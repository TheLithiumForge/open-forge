using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Create.Models;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Create.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Create;

internal static class RouteCreatePresentation
{
    private static readonly RouteCreateDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteCreateResult, RouteCreateData> Rendering { get; } = new()
    {
        Selector = RouteCreateReportSelector.Select,
        DataTextRenderer = RouteCreateDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteCreateData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
            ShowNext = selected.Report.Data.TextNextLines.Count == 0,
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteCreateHelpSections.Create();
}
