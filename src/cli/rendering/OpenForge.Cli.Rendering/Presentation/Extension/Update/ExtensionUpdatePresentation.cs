using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Help;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Extension.Update;

internal static class ExtensionUpdatePresentation
{
    private static readonly ExtensionUpdateDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<ExtensionUpdateResult, ExtensionUpdateData> Rendering { get; } = new()
    {
        Selector = ExtensionUpdateReportSelector.Select,
        DataTextRenderer = ExtensionUpdateDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ExtensionUpdateData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = static selected => selected with
        {
            ShowWorkspace = selected.Selection.Detail >= CliDetail.Standard,
            TextEffects = [],
            TextCounts = [],
            Report = selected.Report with
            {
                Next = selected.Selection.Detail == CliDetail.Minimal
                    && selected.Report.Next is { } next
                    && next.Command.Contains("--prune --dry-run", StringComparison.Ordinal)
                        ? new CliNextAction($"{next.Command}  ({next.Reason})", next.Reason)
                        : selected.Report.Next,
            },
        },
    };

    internal static CliHelpContent CreateHelp()
        => ExtensionUpdateHelpSections.Create();
}
