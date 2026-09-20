using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Presentation.References.Models;
using OpenForge.Cli.Core.Presentation.References.Shared.Help;
using OpenForge.Cli.Core.Presentation.References.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.References.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.References;

internal static class ReferencesPresentation
{
    private static readonly ReferencesDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false,
    });

    internal static CliReportRendering<ReferencesResult, ReferencesData> Rendering { get; } = new()
    {
        Selector = ReferencesReportSelector.Select,
        DataTextRenderer = ReferencesDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.ReferencesData,
        Shape = CliCommandShape.Data,
        SelectText = static selected => selected with
        {
            // The headline is the source path when there are rows, so the listed findings must
            // not repeat a row-state fact the `out` row already prints inline. This trims text
            // only: the report, and therefore JSON, keeps every finding at every level.
            TextFindings = selected.Selection.Detail >= CliDetail.Full
                ? selected.TextFindings
                : selected.TextFindings
                    .Where(finding => !ReferencesWireVocabulary.IsRowState(finding.Code))
                    .ToArray(),
            TextCounts = [],
        },
    };

    internal static CliHelpContent CreateHelp()
        => ReferencesHelpSections.Create();
}
