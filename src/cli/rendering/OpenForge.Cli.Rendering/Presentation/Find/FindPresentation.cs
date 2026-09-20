using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Presentation.Find.Models;
using OpenForge.Cli.Core.Presentation.Find.Shared.Help;
using OpenForge.Cli.Core.Presentation.Find.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Find.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Find;

internal static class FindPresentation
{
    private static readonly FindDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<FindResult, FindData> Rendering { get; } = new()
    {
        Selector = FindReportSelector.Select,
        DataTextRenderer = FindDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.FindData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with
        {
            ShowHeadline = selected.Selection.Detail >= CliDetail.Standard
                || selected.Report.Data.Matches.Count == 0
                || selected.Report.Status is CliSemanticStatus.Invalid
                    or CliSemanticStatus.Blocked
                    or CliSemanticStatus.Failed
                    or CliSemanticStatus.Interrupted,
            TextCounts = selected.Selection.Detail >= CliDetail.Full ? selected.Report.Counts : [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => FindHelpSections.Create();
}
