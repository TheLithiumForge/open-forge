using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteApplication(
    ToolWriteChannelApplication channel, ToolWriteNativeApplication native,
    ToolWriteConfigApplication config, ToolWriteOptionalApplication optional)
{
    internal ValueTask DescribeAsync(ToolWritePlan plan, ToolWriteExecution state, CancellationToken cancellationToken)
        => optional.DescribeAsync(plan, state, cancellationToken);

    internal ValueTask CompleteAsync(ToolWriteExecution state, CancellationToken cancellationToken)
        => native.CompleteBackupsAsync(state, cancellationToken);

    internal async ValueTask ApplyAsync(
        ToolWritePlan plan, ToolWriteExecution state, ToolWriteInteraction interaction, CancellationToken cancellationToken)
    {
        foreach (var tool in plan.Tools)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (state.Stopped) break;
            if (tool.NoOp) continue;
            if (plan.Input.Request.Kind == ToolWriteKind.Remove)
                await RemoveAsync(plan, tool, state, interaction, cancellationToken).ConfigureAwait(false);
            else
                await InstallAsync(plan, tool, state, cancellationToken).ConfigureAwait(false);
        }
        if (!state.Stopped) await optional.ApplyAsync(plan, state, interaction, cancellationToken).ConfigureAwait(false);
        if (!state.Stopped) await CompleteAsync(state, cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask InstallAsync(
        ToolWritePlan plan, ToolWriteToolPlan tool, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        var id = tool.Entry.Identity.Id;
        var detection = await channel.ApplyAsync(plan, tool, state, cancellationToken).ConfigureAwait(false);
        if (detection?.Invocation is null) return;
        var actual = await native.RefreshAsync(tool, detection.Invocation, cancellationToken).ConfigureAwait(false);
        if (actual.Native?.Problems.Count > 0)
        {
            Fail(state, id, ToolWriteFindingCode.NativePlanBlocked, string.Join("\n", actual.Native.Problems));
            return;
        }
        if (!await native.ApplyAsync(actual, state, cancellationToken).ConfigureAwait(false)) return;
        if (await config.ApplyAsync(plan, actual, state, cancellationToken).ConfigureAwait(false)
            && !state.Outcomes.Any(outcome => outcome.ToolId == id && outcome.Stage == ToolWriteStages.Config))
            state.Outcomes.Add(new(id, ToolWriteStages.Config, ToolWriteStages.Completed, [], []));
    }

    private async ValueTask RemoveAsync(
        ToolWritePlan plan, ToolWriteToolPlan tool, ToolWriteExecution state,
        ToolWriteInteraction interaction, CancellationToken cancellationToken)
    {
        var request = plan.Input.Request;
        var nativeChosen = tool.Native is not null;
        if (tool.DeferredNativeRemoval is { } deferred)
        {
            var decision = await new ToolWriteConsentOperation(interaction.ConfirmOptional).AskAsync(new()
            {
                Consent = new(request.Yes, request.CanPrompt && interaction.CanPrompt),
                Format = request.Format,
                Preview = false,
                Question = new()
                {
                    Kind = ToolWriteConsentKind.AccountUninstall,
                    ToolId = tool.Entry.Identity.Id,
                    Effects = [.. deferred.Setup.Effects.Select(ToolWriteProjection.Effect)],
                },
            }, cancellationToken).ConfigureAwait(false);
            if (decision == ToolWriteConsentDecision.Cancelled) throw new OperationCanceledException(cancellationToken);
            nativeChosen = decision == ToolWriteConsentDecision.Accepted;
        }
        if (nativeChosen)
        {
            var actual = tool;
            if (tool.DeferredNativeRemoval is { } chosen)
            {
                actual = await native.RefreshAsync(tool, chosen.Invocation, cancellationToken).ConfigureAwait(false);
                if (actual.Native is { } prepared) state.NativeRemovals[tool.Entry.Identity.Id] = prepared;
                if (actual.Native?.Problems.Count > 0)
                {
                    state.Findings.Add(new(ToolWriteFindingCode.NativePlanBlocked, CliSemanticStatus.Blocked,
                        tool.Entry.Identity.Id, Cause: string.Join("\n", actual.Native.Problems)));
                    state.Stopped = true;
                    return;
                }
            }
            if (!await native.ApplyAsync(actual, state, cancellationToken).ConfigureAwait(false)) return;
        }
        else if (tool.Native is not null)
        {
            state.Outcomes.Add(new(tool.Entry.Identity.Id, ToolWriteStages.Native, ToolWriteStages.Skipped, [], []));
        }
        if (await config.RemoveAsync(plan, tool, state, cancellationToken).ConfigureAwait(false)
            && !state.Outcomes.Any(outcome => outcome.ToolId == tool.Entry.Identity.Id && outcome.Stage == ToolWriteStages.Config))
            state.Outcomes.Add(new(tool.Entry.Identity.Id, ToolWriteStages.Config, ToolWriteStages.Completed, [], []));
    }

    private static void Fail(ToolWriteExecution state, string id, ToolWriteFindingCode code, string? cause)
    {
        state.Findings.Add(new(code, CliSemanticStatus.Failed, id, Cause: cause));
        state.Stopped = true;
    }
}
