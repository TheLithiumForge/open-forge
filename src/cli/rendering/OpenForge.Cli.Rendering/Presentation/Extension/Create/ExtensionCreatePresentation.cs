using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Create.Models;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Create.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.Create;

internal static class ExtensionCreatePresentation
{
    private static readonly ExtensionCreateDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<ExtensionCreateResult, ExtensionCreateData> Rendering { get; } = new()
    {
        Selector = ExtensionCreateReportSelector.Select,
        DataTextRenderer = ExtensionCreateDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionCreateData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };
}
