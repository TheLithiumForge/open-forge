using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Models;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Remove;

internal static class ExtensionRemovePresentation
{
    private static readonly ExtensionRemoveDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<ExtensionRemoveResult, ExtensionRemoveData> Rendering { get; } = new()
    {
        Selector = ExtensionRemoveReportSelector.Select,
        DataTextRenderer = ExtensionRemoveDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionRemoveData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            TextEffects = [],
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => ExtensionRemoveHelpSections.Create();
}
