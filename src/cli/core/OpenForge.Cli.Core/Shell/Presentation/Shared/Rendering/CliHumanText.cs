using System.Text;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

internal static class CliHumanText
{
    internal static void AppendHeader<TResult>(StringBuilder builder, CliPresentationRequest<TResult> presentation, string heading)
        where TResult : ICliCommandResult
    {
        var result = presentation.Result;
        var selection = result.Workspace is { } workspace ? Selection(workspace.SelectedBy) : "unavailable";
        builder.AppendLine($"""
            {heading}
            Status: {Status(result.Status)}
            Workspace: {Text(result.Workspace?.LexicalRoot ?? "unavailable")}
            Selected by: {selection}
            """);
    }

    internal static void AppendNext<TResult>(StringBuilder builder, CliPresentationRequest<TResult> presentation)
        where TResult : ICliCommandResult
    {
        if (presentation.Result.Next is { } next)
        {
            builder.AppendLine($"Next: {Text(next.Command)}");
            if (presentation.Presentation.View == CliView.Expanded)
            {
                builder.AppendLine(Text(next.Reason));
            }
        }
    }

    internal static string Outcome(string label, CliSemanticStatus status)
    {
        return status switch
        {
            CliSemanticStatus.Complete => $"{label} completed.",
            CliSemanticStatus.Attention => $"{label} requires attention.",
            CliSemanticStatus.Incomplete => $"{label} could not finish.",
            CliSemanticStatus.Invalid => $"{label} could not start because the input is invalid.",
            CliSemanticStatus.Blocked => $"{label} is blocked.",
            CliSemanticStatus.Failed => $"{label} failed.",
            CliSemanticStatus.Interrupted => $"{label} was interrupted.",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status is not defined."),
        };
    }

    internal static string Status(CliSemanticStatus value)
        => value == CliSemanticStatus.Attention ? "requires attention" : CliStatusDefinitions.Read(value).MachineName;

    internal static string Selection(CliWorkspaceSelectionMethod value) => value switch
    {
        CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
        CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The workspace selection is not defined."),
    };

    internal static string Text(string value)
        => string.Concat(value.Select(character => char.IsControl(character) ? '\uFFFD' : character));
}
