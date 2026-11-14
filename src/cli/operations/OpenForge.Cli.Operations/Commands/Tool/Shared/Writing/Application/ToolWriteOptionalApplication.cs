using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteOptionalApplication(
    IToolChannelService channels, IToolPathHost pathHost, ToolWriteRecordPublisher publisher)
{
    internal async ValueTask DescribeAsync(ToolWritePlan plan, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        if (plan.Input.Facts.Path.Readiness == ToolPathReadiness.Ready) return;
        var manual = await pathHost.AddAsync(new()
        {
            Facts = plan.Input.Facts.Path,
            Choice = ToolPathChoice.Manual,
            ChosenShellFiles = [],
            Previous = state.Records.Path,
        }, cancellationToken).ConfigureAwait(false);
        state.PathInstructions.AddRange(manual.ManualLines);
        state.Backups.AddRange(manual.PendingBackups);
    }

    internal async ValueTask ApplyAsync(
        ToolWritePlan plan, ToolWriteExecution state, ToolWriteInteraction interaction, CancellationToken cancellationToken)
    {
        if (plan.Tools.Count == 0 || state.Stopped) return;
        var request = plan.Input.Request;
        if (request.Kind == ToolWriteKind.Remove)
        {
            foreach (var tool in plan.Tools.Where(tool => !tool.NoOp))
            {
                if (!state.Records.Machine.TryGetValue(tool.Entry.Identity.Id, out var machine) || !machine.PlacedByOpenForge) continue;
                var paths = new[] { machine.ToolPath, machine.CommandPath }.OfType<string>()
                    .Concat(machine.Launchers.Select(launcher => launcher.LauncherPath)).Distinct(StringComparer.Ordinal).ToArray();
                var approvedPaths = await ConfirmAsync(ToolWriteConsentKind.SharedFileDeletion, tool.Entry.Identity.Id, paths).ConfigureAwait(false) ? paths : [];
                var approvedRuntimes = new List<ToolNodeRuntimeRecord>();
                foreach (var runtime in state.Records.Runtimes.Values.Where(runtime => runtime.RuntimePath == machine.RuntimePath))
                    if (await ConfirmAsync(ToolWriteConsentKind.SharedFileDeletion, runtime.Version, [runtime.RuntimePath]).ConfigureAwait(false)) approvedRuntimes.Add(runtime);
                if (approvedPaths.Length == 0 && approvedRuntimes.Count == 0) continue;
                var removed = await channels.RemoveAsync(new()
                {
                    Locations = new(request.Workspace.LexicalRoot, plan.Input.Facts.HomePath),
                    Records = state.Records,
                    Expected = machine,
                    ApprovedToolPaths = approvedPaths,
                    ApprovedRuntimes = approvedRuntimes,
                }, cancellationToken).ConfigureAwait(false);
                state.Outcomes.Add(new(tool.Entry.Identity.Id, ToolWriteStages.SharedFiles,
                    removed.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed, removed.RemovedPaths, removed.KeptPaths));
                if (removed.Failure is not null)
                {
                    state.Findings.Add(new(ToolWriteFindingCode.VerificationFailed, CliSemanticStatus.Failed, tool.Entry.Identity.Id, Cause: removed.Failure));
                    state.Stopped = true;
                    return;
                }
                if (!await publisher.PublishAuxiliaryAsync(state, plan, null, removed.RemovedPaths, cancellationToken).ConfigureAwait(false)) return;
            }
            if (state.Records.Machine.Count == 0 && state.Records.Path is { } path
                && await ConfirmAsync(ToolWriteConsentKind.PathChange, null).ConfigureAwait(false))
            {
                var removed = await pathHost.RemoveAsync(new(path), cancellationToken).ConfigureAwait(false);
                state.Backups.AddRange(removed.PendingBackups);
                state.Outcomes.Add(new(ToolId: plan.Tools[^1].Entry.Identity.Id, Stage: ToolWriteStages.Path,
                    State: removed.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
                    Completed: removed.RemovedTargets, Unfinished: removed.KeptTargets));
                var remaining = path with
                {
                    Changes = [.. path.Changes.Where(change => !removed.RemovedTargets.Contains(change.Target, StringComparer.Ordinal))],
                };
                if (!await publisher.PublishAuxiliaryAsync(state, plan, remaining, [], cancellationToken).ConfigureAwait(false)) return;
                if (removed.Failure is not null || removed.KeptTargets.Count != 0)
                {
                    state.Findings.Add(new(ToolWriteFindingCode.VerificationFailed, CliSemanticStatus.Incomplete, Cause: removed.Failure));
                    state.Stopped = true;
                    return;
                }
            }
            return;
        }

        var facts = await pathHost.ObserveAsync(plan.Input.Facts.HomePath, cancellationToken).ConfigureAwait(false);
        ToolWritePathAnswer answer = new(false, []);
        if (facts.Readiness != ToolPathReadiness.Ready && request.CanPrompt && interaction.CanPrompt && !request.Yes && request.Format == CliFormat.Text)
        {
            var reply = await interaction.ChoosePath(new(facts.BinPath, [.. facts.DetectedShellFiles.Select(file =>
                new ToolWriteShellFileView(file.AbsolutePath, file.Shell == ToolShellKind.Fish))],
                plan.Input.Facts.Target.OperatingSystem != Core.Framework.Tools.Models.Channels.ToolOperatingSystem.Windows),
                new(true), cancellationToken).ConfigureAwait(false);
            if (reply.State == CliPromptState.Cancelled) throw new OperationCanceledException(cancellationToken);
            if (reply.State == CliPromptState.Answered) answer = reply.Value;
        }
        var result = await pathHost.AddAsync(new()
        {
            Facts = facts,
            Choice = answer.AddForMe ? ToolPathChoice.AddForMe : ToolPathChoice.Manual,
            ChosenShellFiles = [.. answer.ShellFiles.Select(file => new ToolShellFile(file.Path, file.Fish ? ToolShellKind.Fish : ToolShellKind.Posix))],
            Previous = state.Records.Path,
        }, cancellationToken).ConfigureAwait(false);
        state.PathInstructions.AddRange(result.ManualLines);
        state.Backups.AddRange(result.PendingBackups);
        if (answer.AddForMe && result.State is { } applied
            && !await publisher.PublishAuxiliaryAsync(state, plan, applied, [], cancellationToken).ConfigureAwait(false)) return;
        if (result.Failure is not null)
        {
            state.Findings.Add(new(ToolWriteFindingCode.VerificationFailed, CliSemanticStatus.Incomplete, Cause: result.Failure));
            state.Stopped = true;
        }

        async ValueTask<bool> ConfirmAsync(ToolWriteConsentKind kind, string? id, IReadOnlyList<string>? paths = null)
        {
            var decision = await new ToolWriteConsentOperation(interaction.ConfirmOptional).AskAsync(new()
            {
                Consent = new(request.Yes, request.CanPrompt && interaction.CanPrompt),
                Format = request.Format,
                Preview = false,
                Question = new() { Kind = kind, ToolId = id, Effects = [], Paths = paths ?? [] },
            }, cancellationToken).ConfigureAwait(false);
            if (decision == ToolWriteConsentDecision.Cancelled) throw new OperationCanceledException(cancellationToken);
            return decision == ToolWriteConsentDecision.Accepted;
        }
    }
}
