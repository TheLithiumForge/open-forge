using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Extension.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.Inspect;

internal static class ExtensionInspectPresentation
{
    private static readonly ExtensionInspectDataJsonContext JsonContext = new(
        new JsonSerializerOptions(ExtensionInspectDataJsonContext.Default.Options)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
        });

    internal static CliReportRendering<ExtensionInspectResult, ExtensionInspectData> Rendering { get; } = new()
    {
        Selector = ExtensionInspectReportSelector.Select,
        DataTextRenderer = ExtensionInspectDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionInspectData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextFindings = selected.TextFindings
                .Where(finding => !ExtensionInspectWording.IsFileDifferenceCode(finding.Code))
                .ToArray(),
        },
    };
}
