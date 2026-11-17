using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Tool.Shared.Writing;

internal static class ToolWritePresentation
{
    private static readonly ToolWriteJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<ToolWriteResult, ToolWriteData> Rendering { get; } = new()
    {
        Selector = ToolWriteReportSelector.Select,
        DataTextRenderer = ToolWriteDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ToolWriteData,
        Shape = CliCommandShape.ChangeReport,
    };
}
