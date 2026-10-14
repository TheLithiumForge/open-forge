using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Presentation.Context.Models;
using OpenForge.Cli.Core.Presentation.Context.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Context.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Context;

internal static class ContextPresentation
{
    private static readonly ContextDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<ContextResult, ContextData> Rendering { get; } = new()
    {
        Selector = ContextReportSelector.Select,
        DataTextRenderer = ContextDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ContextData,
        Shape = CliCommandShape.Data,
        SelectText = ContextReportSelector.SelectText,
    };
}
