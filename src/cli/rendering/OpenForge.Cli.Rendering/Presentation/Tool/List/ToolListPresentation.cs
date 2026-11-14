using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool.List;

namespace OpenForge.Cli.Core.Presentation.Tool.List;

internal static class ToolListPresentation
{
    internal static CliReportRendering<ToolListResult, ToolReadPayload> Rendering { get; } = new()
    {
        Selector = Select,
        DataTextRenderer = Render,
        DataJsonTypeInfo = ToolReadJsonContext.Default.ToolReadPayload,
        Shape = CliCommandShape.Data,
    };

    internal static CliHelpContent CreateHelp() => CliHelpContent.Empty;

    private static CliReport<ToolReadPayload> Select(ToolListResult result, CliSelection selection) => new()
    {
        ShowMinimalTextWarnings = true,
        Command = result.Command,
        Status = result.Status,
        Headline = new(ToolListText.Headline(), HeadlineKind(result.Status)),
        Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
        Data = result.Data,
        Next = result.Next,
        Findings = result.Findings.Select(finding => new CliFinding
        {
            Code = finding.Code,
            Title = ToolListText.Finding(),
            Message = finding.Cause,
            Severity = result.Status == CliSemanticStatus.Invalid ? CliSeverity.Error : CliSeverity.Warning,
            Subject = new(CliSubjectKind.Identifier, Id: finding.Id ?? result.Command),
            Actions = finding.NextCommand is { } command ? [new(command, finding.Cause)] : [],
        }).ToArray(),
    };

    private static CliHeadlineKind HeadlineKind(CliSemanticStatus status) => status switch
    {
        CliSemanticStatus.Complete => CliHeadlineKind.Done,
        CliSemanticStatus.Attention => CliHeadlineKind.Warnings,
        CliSemanticStatus.Incomplete => CliHeadlineKind.Incomplete,
        CliSemanticStatus.Invalid => CliHeadlineKind.CannotStart,
        CliSemanticStatus.Blocked => CliHeadlineKind.Blocked,
        CliSemanticStatus.Failed => CliHeadlineKind.Failed,
        CliSemanticStatus.Interrupted => CliHeadlineKind.Cancelled,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static CliTextDocument Render(ToolReadPayload data, CliSelection selection, CliTextStyle style)
    {
        var builder = new StringBuilder();
        foreach (var tool in data.Tools)
        {
            builder.AppendLine(CliText.Escape(ToolListText.ToolRow(tool.Id, tool.Name, tool.DeclaredRange, tool.State)));
            if (selection.Detail >= CliDetail.Standard)
            {
                builder.AppendLine(CliText.Escape(ToolListText.Origin(tool.Origin)));
                builder.AppendLine(CliText.Escape(ToolListText.Upstream(tool.Upstream)));
                builder.AppendLine(CliText.Escape(ToolListText.Licence(tool.Licence)));
                builder.AppendLine(CliText.Escape(ToolListText.Channels(tool.Channels.Select(value => value.Id))));
                builder.AppendLine(CliText.Escape(ToolListText.Profiles(tool.Profiles.Select(value => value.Range))));
            }


        }
        return new([new CliTextSpan(builder.ToString().ReplaceLineEndings("\n"))]);
    }
}
