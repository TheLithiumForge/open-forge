using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Presentation.Index.Models;
using OpenForge.Cli.Core.Presentation.Index.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Index.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Index;

internal static class IndexPresentation
{
    private static readonly IndexDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<IndexResult, IndexData> Rendering { get; } = new()
    {
        Selector = IndexReportSelector.Select,
        DataTextRenderer = IndexDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.IndexData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with { TextEffects = [], TextCounts = [] },
    };
}
