using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Stripping.Models;
using OpenForge.Cli.Core.Presentation.Stripping.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Stripping.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Stripping;

/// <summary>The one report rendering uninstall and export share.</summary>
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
