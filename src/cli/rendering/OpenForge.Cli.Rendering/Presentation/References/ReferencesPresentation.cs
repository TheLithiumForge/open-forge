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
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
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
            // Minimal text can use the outgoing row for a row-state fact without an action.
            // Standard and higher detail keep every finding in the same order as JSON.
            TextFindings = selected.TextFindings.Where(finding => selected.Selection.Detail >= CliDetail.Standard
                    || finding.Actions.Count > 0
                    || !ReferencesWireVocabulary.IsRowState(finding.Code))
                .Select(finding => finding.Code == "references.invalid-direction" ? finding with { Message = string.Empty } : finding).ToArray(),
            TextCounts = [],
            ShowNextReason = selected.Report.Next?.Kind != CliNextActionKind.Sentence,
        },
    };

    internal static CliHelpContent CreateHelp()
        => ReferencesHelpSections.Create();
}
