using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List.Models;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.List.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Extension.List;

internal static class ExtensionListPresentation
{
    private static readonly ExtensionListDataJsonContext JsonContext = new(
        new JsonSerializerOptions(ExtensionListDataJsonContext.Default.Options)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false,
        });

    internal static CliReportRendering<ExtensionListResult, ExtensionListData> Rendering { get; } = new()
    {
        Selector = ExtensionListReportSelector.Select,
        DataTextRenderer = ExtensionListDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionListData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with
        {
            ShowHeadline = selected.Report.Status != CliSemanticStatus.Complete,
            TextCounts = [],
        },
    };
}
