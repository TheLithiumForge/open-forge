using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Application;

/// <summary>A step to report and, when it isn't skipped, the catalogue command it runs.</summary>
internal sealed record ToolPlannedStep(ToolWriteStep Step, ToolCommand? Command);

/// <summary>Plans catalogue commands as report steps and runs them in order, stopping at the first failure.</summary>
internal sealed class ToolSteps(IToolProcessHost processes)
{
    // A failed command's report keeps only its last lines, enough to show the error without flooding the terminal.
    internal const int OutputLines = 20;

    internal static IEnumerable<ToolPlannedStep> Plan(string kind, ToolStep step, string outcome = ToolWire.Planned)
        => step.Commands.Select((command, index) => new ToolPlannedStep(new()
        {
            Kind = kind,
            Command = command.Text,
            // The effect and notes describe the step as a whole, so they follow its last command.
            Effect = index == step.Commands.Length - 1 ? step.Effect : null,
            Notes = index == step.Commands.Length - 1 ? step.Notes : [],
            Outcome = outcome,
        }, command));

    internal static IEnumerable<ToolPlannedStep> Plan(ToolUninstall uninstall, string outcome)
        => uninstall.Commands.Select(command => new ToolPlannedStep(new()
        {
            Kind = ToolWire.Uninstall,
            Command = command.Text,
            Outcome = outcome,
        }, command));

    /// <summary>Runs every planned step and returns the reported steps with the first failure, if any.</summary>
    internal async ValueTask<(List<ToolWriteStep> Steps, ToolFinding? Failure)> RunAsync(
        IReadOnlyList<ToolPlannedStep> planned, string toolId, CliWorkspace workspace, CancellationToken cancellationToken)
    {
        var steps = new List<ToolWriteStep>();
        ToolFinding? failure = null;
        foreach (var item in planned)
        {
            if (failure is not null || item.Command is null || item.Step.Outcome != ToolWire.Planned)
            {
                steps.Add(failure is not null && item.Step.Outcome == ToolWire.Planned ? item.Step with { Outcome = ToolWire.NotStarted } : item.Step);
                continue;
            }
            var result = await processes.RunAsync(new(item.Command, workspace.LexicalRoot, ToolDefinitions.CommandTimeout), cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Succeeded)
            {
                steps.Add(item.Step with { Outcome = ToolWire.Done });
                continue;
            }
            steps.Add(item.Step with { Outcome = ToolWire.Failed, ExitCode = result.ExitCode, Output = Tail(result.OutputTail) });
            failure = new(ToolFindingCode.CommandFailed, CliSemanticStatus.Failed, toolId, item.Step.Command);
        }
        return (steps, failure);
    }

    private static string Tail(string output)
    {
        var lines = output.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        return string.Join('\n', lines.Skip(Math.Max(0, lines.Length - OutputLines)));
    }
}
