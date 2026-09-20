using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Status.Models;
using OpenForge.Cli.Core.Presentation.Status.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Status.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Status;

internal static class StatusPresentation
{
    private static readonly StatusDataJsonContext JsonContext = CreateJsonContext();

    internal static CliReportRendering<StatusResult, StatusData> Rendering { get; } = new()
    {
        Selector = StatusReportSelector.Select,
        DataTextRenderer = StatusDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.StatusData,
        Shape = CliCommandShape.Summary,
        SelectText = static selected => selected with
        {
            TextCounts = selected.Selection.Detail >= CliDetail.Standard ? selected.Report.Counts : [],
        },
    };

    private static StatusDataJsonContext CreateJsonContext()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
        };
        options.Converters.Add(new StatusDataExtensionJsonConverter());
        options.Converters.Add(new StatusDataLibraryJsonConverter());
        return new StatusDataJsonContext(options);
    }
}
