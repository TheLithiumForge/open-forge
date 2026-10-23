using System.Text.Encodings.Web;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Install.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Install;

internal static class InstallPresentation
{
    private static readonly InstallDataJsonContext JsonContext = new(new JsonSerializerOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    });

    internal static CliReportRendering<InstallResult, InstallData> Rendering { get; } = new()
    {
        Selector = InstallReportSelector.Select,
        DataTextRenderer = InstallDataTextRenderer.Render,
        DataJsonTypeInfo = JsonContext.InstallData,
        Shape = CliCommandShape.ChangeReport,
        SelectText = SelectText,
    };

    internal static CliReportRendering<InstallResult, InstallData> PlanReviewRendering { get; } = Rendering with
    {
        Selector = static (result, selection) =>
        {
            var report = InstallReportSelector.Select(result, selection);
            return report with
            {
                Data = report.Data with
                {
                    ShowNoChanges = false,
                    TextReviewHeadline = report.Headline.Sentence,
                    TextReviewWorkspace = report.Workspace?.Path,
                }
            };
        },
        SelectText = static selected => SelectText(selected) with { ShowHeadline = false, ShowWorkspace = false },
    };

    private static CliSelectedReport<InstallData> SelectText(CliSelectedReport<InstallData> selected)
        => InstallInputTextSelection.Select(selected with
        {
            ShowWorkspace = selected.Report.Workspace is not null,
            TextEffects = [],
            TextCounts = [],
            TextFindings = selected.Selection.Detail == CliDetail.Minimal
                && selected.Report.Status == CliSemanticStatus.Blocked
                && selected.Report.Data.BlockedFindingIdentities.Count > 0
                    ? selected.TextFindings
                        .Where(finding => !IsRepresentedBlockedFinding(
                            finding,
                            selected.Report.Data.BlockedFindingIdentities))
                        .ToArray()
                    : selected.Report.Data.SuppressInterruptedFinding
                        ? selected.TextFindings
                            .Where(finding => finding.Code != InstallWireVocabulary.Name(InstallFindingCode.Interrupted) || finding.LockFailure is not null)
                            .ToArray()
                        : selected.TextFindings,
        });

    private static bool IsRepresentedBlockedFinding(
        CliFinding finding,
        IReadOnlyList<InstallDataTextFindingIdentity> blockedFindings)
    {
        if (finding.Subject.Path is not { } path)
        {
            return false;
        }

        return blockedFindings.Any(blockedFinding =>
            string.Equals(
                finding.Code,
                InstallWireVocabulary.Name(blockedFinding.Code),
                StringComparison.Ordinal)
            && string.Equals(path, blockedFinding.Path, StringComparison.Ordinal));
    }
}
