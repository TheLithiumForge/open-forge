using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.OutputText.Tool.Status;

namespace OpenForge.Cli.Core.Presentation.Tool.Status;

internal static class ToolStatusPresentation
{
    internal static CliReportRendering<ToolStatusResult, ToolReadPayload> Rendering { get; } = new()
    {
        Selector = Select,
        DataTextRenderer = Render,
        DataJsonTypeInfo = ToolReadJsonContext.Default.ToolReadPayload,
        Shape = CliCommandShape.Data,
    };

    internal static CliHelpContent CreateHelp() => CliHelpContent.Empty;

    private static CliReport<ToolReadPayload> Select(ToolStatusResult result, CliSelection selection) => new()
    {
        ShowMinimalTextWarnings = true,
        Command = result.Command,
        Status = result.Status,
        Headline = new(ToolStatusText.Headline(), HeadlineKind(result.Status)),
        Workspace = result.WorkspacePath is { } path ? new(path, result.WorkspaceExplicit) : null,
        Data = result.Data,
        Next = result.Next,
        Findings = result.Findings.Select(finding => new CliFinding
        {
            Code = finding.Code,
            Title = ToolStatusText.Finding(),
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
            builder.AppendLine(CliText.Escape(ToolStatusText.ToolRow(tool.Id, tool.Name, tool.DeclaredRange, tool.State)));
            builder.AppendLine(CliText.Escape(ToolStatusText.Version(tool.LockedVersion, tool.DetectedVersion)));
            builder.AppendLine(CliText.Escape(ToolStatusText.Machine(tool.MachineState, tool.NativeRecordState)));
            builder.AppendLine(CliText.Escape(ToolStatusText.Bin(tool.BinPath, tool.BinReadiness)));
            builder.AppendLine(CliText.Escape(ToolStatusText.ManagerBin(tool.ManagerGlobalBinPath, tool.ManagerGlobalBinReadiness)));
            builder.AppendLine(CliText.Escape(ToolStatusText.ToolPath(tool.ToolPath)));
            builder.AppendLine(CliText.Escape(ToolStatusText.RuntimePath(tool.RuntimePath)));
            foreach (var native in tool.NativeSettings)
                builder.AppendLine(CliText.Escape(ToolStatusText.NativeSetting(native.ChangeId, native.NativeHomePath, native.Previous, native.Applied, native.ManualChangeArguments)));
            foreach (var change in tool.Changes)
            {
                builder.AppendLine(CliText.Escape(ToolStatusText.StoredChange(change.Id, change.Disposition, change.Current, change.Desired, change.Conflict)));
                if (change.NativeEnvironmentOverrides is null) builder.AppendLine(CliText.Escape(ToolStatusText.OverrideUnavailable()));
                else if (change.NativeEnvironmentOverrides.Count == 0) builder.AppendLine(CliText.Escape(ToolStatusText.OverrideNone()));
                else
                {
                    foreach (var environment in change.NativeEnvironmentOverrides)
                        builder.AppendLine(CliText.Escape(ToolStatusText.Override(environment.Key, environment.Value)));
                }
            }
            foreach (var removal in tool.Removals)
                builder.AppendLine(CliText.Escape(ToolStatusText.RetiredAddition(removal.Id, removal.Disposition, removal.Conflict)));

        }
        return new([new CliTextSpan(builder.ToString().ReplaceLineEndings("\n"))]);
    }
}
