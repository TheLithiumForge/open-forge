using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Help;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Route.Move;

internal static class RouteMovePresentation
{
    private static readonly RouteMoveDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<RouteMoveResult, Models.RouteMoveData> Rendering { get; } = new()
    {
        Selector = RouteMoveReportSelector.Select,
        DataTextRenderer = RouteMoveDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.RouteMoveData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => RouteMoveHelpSections.Create();
}
