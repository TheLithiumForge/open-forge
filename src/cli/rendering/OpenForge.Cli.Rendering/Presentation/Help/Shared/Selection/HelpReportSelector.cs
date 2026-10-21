using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Presentation.Help.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Help.Shared.Selection;

internal static class HelpReportSelector
{
    internal static CliReport<HelpData> Select(HelpResult result, CliSelection selection)
    {
        var complete = result.Status == CliSemanticStatus.Complete;
        var cause = result.Cause ?? string.Empty;
        return new CliReport<HelpData>
        {
            Command = result.Command,
            Status = result.Status,
            Headline = Headline(result),
            HeadlineFindingCode = result.FindingCode,
            Findings = complete ? [] :
            [
                new CliFinding
                {
                    Severity = CliSeverity.Error,
                    Code = result.FindingCode ?? throw new InvalidOperationException("A Help finding requires a code."),
                    Title = Title(result.Status),
                    Message = cause,
                    Subject = new CliSubject(CliSubjectKind.Identifier, Id: result.Topic ?? result.Command),
                },
            ],
            Data = new HelpData
            {
                Topic = result.Topic,
                Markdown = result.Authoring is { } authoring
                    ? string.Concat(global::OpenForge.Cli.OutputText.Help.HelpText.ConceptsMarkdown(),
                        global::OpenForge.Cli.OutputText.Help.HelpText.AuthoringHeading(),
                        global::OpenForge.Cli.OutputText.Help.HelpText.AuthoringNote(), authoring)
                    : null,
                Topics = result.Topics,
                Complete = complete,
            },
            Next = result.Next,
        };
    }

    private static CliHeadline Headline(HelpResult result)
        => result.Status switch
        {
            CliSemanticStatus.Complete => new(global::OpenForge.Cli.OutputText.Help.HelpText.Completed(), CliHeadlineKind.Done),
            CliSemanticStatus.Invalid => new(global::OpenForge.Cli.OutputText.Help.HelpText.Invalid(result.Cause ?? string.Empty), CliHeadlineKind.CannotStart),
            CliSemanticStatus.Failed => new(global::OpenForge.Cli.OutputText.Help.HelpText.Invalid(result.Cause ?? string.Empty), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted => new(global::OpenForge.Cli.OutputText.Help.HelpText.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Help status is not defined."),
        };

    private static string Title(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Invalid => global::OpenForge.Cli.OutputText.Help.HelpText.InvalidTitle(),
            CliSemanticStatus.Failed => global::OpenForge.Cli.OutputText.Help.HelpText.FailedTitle(),
            CliSemanticStatus.Interrupted => global::OpenForge.Cli.OutputText.Help.HelpText.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Help finding status is not defined."),
        };

    internal static CliSelectedReport<HelpData> SelectText(CliSelectedReport<HelpData> selected)
        => selected with
        {
            ShowHeadline = !selected.Report.Data.Complete,
        };
}
