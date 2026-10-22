using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Presentation.Help.Models;
using OpenForge.Cli.Core.Presentation.Help.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Help.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Help;

internal static class HelpPresentation
{
    private static readonly HelpDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<HelpResult, HelpData> Rendering { get; } = new()
    {
        Selector = HelpReportSelector.Select,
        DataTextRenderer = HelpDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.HelpData,
        Shape = CliCommandShape.Data,
        SelectText = HelpReportSelector.SelectText,
    };
}
