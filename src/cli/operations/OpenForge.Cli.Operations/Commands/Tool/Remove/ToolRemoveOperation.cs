using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Remove;

/// <summary>
/// Removes Open Forge's own changes for one tool without a plan question, because they are small and exactly
/// reversible. Then it offers the tool's own uninstall, which runs only on an explicit yes.
/// </summary>
internal sealed class ToolRemoveOperation(ToolObserver observer, ToolSteps runner, ToolFileApplication files, ToolInteraction interaction)
{
    internal async ValueTask<ToolWriteResult> ExecuteAsync(ToolWriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entry = observer.Catalogue.Find(request.Id ?? string.Empty) ?? throw new InvalidOperationException("Binding admits only catalogue IDs.");
        try
        {
            var plan = await ToolEditPlanner.PlanAsync(entry, request.Workspace, removal: true, cancellationToken).ConfigureAwait(false);
            if (plan.Conflict is { } conflict) return ToolResults.Stop(request, conflict);
            var version = await observer.DetectAsync(entry, request.Workspace, cancellationToken).ConfigureAwait(false);
            var uninstall = version is null ? [] : ToolSteps.Plan(entry.Uninstall, ToolWire.NotRun).ToList();
            var tool = new ToolWriteTool
            {
                Id = entry.Id,
                Name = entry.Name,
                InstalledVersion = version,
                UninstallDescription = entry.Uninstall.Description,
                Steps = [.. uninstall.Select(item => item.Step)],
                Changes = plan.Changes,
            };
            if (request.Preview || plan.Edits.Count == 0 && version is null) return ToolResults.Create(request, CliSemanticStatus.Complete, [tool]);

            var applied = await files.ApplyAsync(request.Workspace, request.Kind, plan.Edits, cancellationToken).ConfigureAwait(false);
            tool = tool with { Changes = ToolResults.WithOutcomes(plan.Changes, applied.Applied, applied.Failure is not null) };
            if (applied.Failure is { } code)
            {
                var status = code == ToolFindingCode.Interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed;
                return ToolResults.Create(request, status, [tool], [new(code, status, entry.Id, applied.Cause)]);
            }
            if (uninstall.Count == 0 || !await ConfirmUninstallAsync(request, tool, cancellationToken).ConfigureAwait(false))
                return ToolResults.Create(request, CliSemanticStatus.Complete, [tool]);

            var (ran, failure) = await runner.RunAsync(
                [.. uninstall.Select(item => item with { Step = item.Step with { Outcome = ToolWire.Planned } })],
                entry.Id, request.Workspace, cancellationToken).ConfigureAwait(false);
            tool = tool with { Steps = ran };
            return failure is null
                ? ToolResults.Create(request, CliSemanticStatus.Complete, [tool])
                : ToolResults.Create(request, CliSemanticStatus.Failed, [tool], [failure]);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ToolResults.Stop(request, new(ToolFindingCode.Interrupted, CliSemanticStatus.Interrupted, entry.Id));
        }
    }

    // The default is no. --yes takes the default, and a session that can't ask leaves the commands for the person to run.
    private async ValueTask<bool> ConfirmUninstallAsync(ToolWriteRequest request, ToolWriteTool tool, CancellationToken cancellationToken)
    {
        if (request.Yes || request.Format == CliFormat.Json || !interaction.CanPrompt) return false;
        var reply = await interaction.ConfirmUninstall(new(tool), new(true), cancellationToken).ConfigureAwait(false);
        return reply.State == CliPromptState.Answered && reply.Value;
    }
}
