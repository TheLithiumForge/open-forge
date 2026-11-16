using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Shared.Stripping.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Stripping.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Shared.Stripping;

/// <summary>The one report rendering strip and uninstall share.</summary>
internal static class StripPresentation
{
    private static readonly StripDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<StripResult, StripData> Rendering { get; } = new()
    {
        Selector = StripReportSelector.Select,
        DataTextRenderer = StripDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.StripData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with { TextEffects = [] },
    };
}
