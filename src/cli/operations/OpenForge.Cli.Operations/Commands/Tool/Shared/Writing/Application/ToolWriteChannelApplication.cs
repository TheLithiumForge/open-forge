using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteChannelApplication(
    IToolChannelService channels, IToolVersionService versions, ToolWriteRecordPublisher publisher)
{
    internal async ValueTask<ToolDetectionResult?> ApplyAsync(
        ToolWritePlan plan, ToolWriteToolPlan tool, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        var id = tool.Entry.Identity.Id;
        var channel = tool.Channel ?? throw new InvalidOperationException("An admitted install needs its channel plan.");
        var installed = await channels.InstallAsync(new(channel), cancellationToken).ConfigureAwait(false);
        state.Backups.AddRange(installed.PendingBackups);
        state.Outcomes.Add(new(id, ToolWriteStages.Channel, installed.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
            installed.CompletedEffects, installed.Failure is null ? [] : [ToolWriteStages.Channel]));
        if (installed.Failure is not null || installed.State is not { } machine)
            return Fail(ToolWriteFindingCode.ChannelBlocked, installed.Failure);
        var detection = await channels.DetectAsync(new(tool.Entry, plan.Input.Request.Workspace.LexicalRoot)
        {
            RecordedState = machine,
        }, cancellationToken).ConfigureAwait(false);
        if (detection.Failure is not null || detection.Invocation is null || detection.Version is not { } version
            || !versions.Satisfies(version, new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = channel.Version }))
            return Fail(ToolWriteFindingCode.VerificationFailed, detection.Failure);
        var previous = state.Records.Portable?.Lock.GetValueOrDefault(id);
        if (!await publisher.PublishAsync(state, new()
        {
            Plan = plan,
            Tool = tool,
            Machine = machine,
            Runtimes = installed.Runtimes,
            Lock = new()
            {
                Version = channel.Version,
                Channel = channel.ChannelId,
                ProfileDigest = previous?.ProfileDigest,
                Changes = previous?.Changes ?? [],
            },
        }, cancellationToken).ConfigureAwait(false)) return null;
        return detection;

        ToolDetectionResult? Fail(ToolWriteFindingCode code, string? cause)
        {
            state.Findings.Add(new(code, CliSemanticStatus.Failed, id, Cause: cause));
            state.Stopped = true;
            return null;
        }
    }
}
