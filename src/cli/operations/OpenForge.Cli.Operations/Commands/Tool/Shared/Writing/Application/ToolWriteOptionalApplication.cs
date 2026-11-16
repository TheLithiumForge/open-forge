using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Consent;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteOptionalApplication(
    IToolChannelService channels, IToolPathHost pathHost, ToolWriteRecordPublisher publisher)
{
    internal async ValueTask DescribeAsync(ToolWritePlan plan, ToolWriteExecution state, CancellationToken cancellationToken)
    {
        if (plan.Input.Request.Kind == ToolWriteKind.Remove || plan.Input.Facts.Path.Readiness == ToolPathReadiness.Ready || !UsesBin(plan)) return;
        var manual = await pathHost.AddAsync(new()
        {
            Facts = plan.Input.Facts.Path,
            Choice = ToolPathChoice.Manual,
            ChosenShellFiles = [],
            Previous = state.Records.Path,
        }, cancellationToken).ConfigureAwait(false);
        AddInstructions(state, manual.ManualLines);
        state.Backups.AddRange(manual.PendingBackups);
        ToolWriteCancellation.Stop(state, cancellationToken);
    }

    // Only plans that place launchers or native executables in the Open Forge bin need it on PATH.
    private static bool UsesBin(ToolWritePlan plan)
        => plan.Tools.Any(tool => tool.Channel?.Execution is { } execution && (execution.Launchers.Count > 0
            || PhysicalIdentityTracker.PathComparer.Equals(execution.NativeInstallDirectory, execution.BinPath)
            || PhysicalContainment.Contains(execution.BinPath, execution.CommandPath)));

    // Only launchers in Open Forge's bin count. Package-manager installs keep their own global bin.
    private static bool HasBinLauncher(ToolWritePlan plan, ToolMachineState machine)
        => PhysicalContainment.Contains(plan.Input.Facts.Path.BinPath, machine.CommandPath)
            || machine.Launchers.Any(launcher => PhysicalContainment.Contains(plan.Input.Facts.Path.BinPath, launcher.LauncherPath));

    private static void AddInstructions(ToolWriteExecution state, IEnumerable<string> lines)
    {
        foreach (var line in lines)
            if (!state.PathInstructions.Contains(line, StringComparer.Ordinal)) state.PathInstructions.Add(line);
    }

    // PATH history names the entries Open Forge added. Without history, the self-identifying marks name the shell blocks.
    private static IEnumerable<string> PathRemovalLines(ToolWritePlan plan, ToolPathState? path)
    {
        var bin = path?.BinPath ?? plan.Input.Facts.Path.BinPath;
        if (path is { Changes.Count: > 0 })
        {
            foreach (var change in path.Changes)
                yield return change.Kind == ToolPathChangeKind.ShellBlock
                    ? ToolPathPolicy.RemoveBlockLine(change.Target, change.StartMarker ?? ToolPathPolicy.StartMarker, change.EndMarker ?? ToolPathPolicy.EndMarker)
                    : ToolPathPolicy.RemoveUserEntryLine(bin);
            yield break;
        }
        var marked = plan.Input.Facts.Path.DetectedShellFiles.Where(file => ToolPathPolicy.HasMarkedBlock(file.AbsolutePath)).ToArray();
        foreach (var file in marked) yield return ToolPathPolicy.RemoveBlockLine(file.AbsolutePath, ToolPathPolicy.StartMarker, ToolPathPolicy.EndMarker);
        if (marked.Length == 0 && plan.Input.Facts.Path.Readiness == ToolPathReadiness.Ready) yield return ToolPathPolicy.RemoveEntryLine(bin);
    }

    internal async ValueTask ApplyAsync(
        ToolWritePlan plan, ToolWriteExecution state, ToolWriteInteraction interaction, CancellationToken cancellationToken)
    {
        if (plan.Tools.Count == 0 || state.Stopped) return;
        var request = plan.Input.Request;
        if (request.Kind == ToolWriteKind.Remove)
        {
            var removing = plan.Tools.Select(tool => tool.Entry.Identity.Id).ToHashSet(StringComparer.Ordinal);
            var removedBinTool = state.Records.Machine.Values.Any(value => removing.Contains(value.ToolId) && HasBinLauncher(plan, value));
            foreach (var tool in plan.Tools.Where(tool => !tool.NoOp))
            {
                if (!state.Records.Machine.TryGetValue(tool.Entry.Identity.Id, out var machine) || !machine.PlacedByOpenForge) continue;
                var paths = new[] { machine.ToolPath, machine.CommandPath }.OfType<string>()
                    .Concat(machine.Launchers.Select(launcher => launcher.LauncherPath)).Distinct(StringComparer.Ordinal).ToArray();
                var approvedPaths = await ConfirmAsync(ToolWriteConsentKind.SharedFileDeletion, tool.Entry.Identity.Id, paths).ConfigureAwait(false) ? paths : [];
                if (approvedPaths.Length == 0) state.ManualDeletions.AddRange(paths);
                var approvedRuntimes = new List<ToolNodeRuntimeRecord>();
                foreach (var runtime in state.Records.Runtimes.Values.Where(runtime => runtime.RuntimePath == machine.RuntimePath))
                {
                    if (await ConfirmAsync(ToolWriteConsentKind.SharedFileDeletion, runtime.Version, [runtime.RuntimePath]).ConfigureAwait(false)) approvedRuntimes.Add(runtime);
                    else state.ManualDeletions.Add(runtime.RuntimePath);
                }
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
                if (ToolWriteCancellation.Stop(state, cancellationToken)) return;
                var deletedTool = removed.RemovedPaths.Any(path => PhysicalIdentityTracker.PathComparer.Equals(path, machine.CommandPath)
                    || machine.ToolPath is { } toolPath && PhysicalIdentityTracker.PathComparer.Equals(path, toolPath)
                        && PhysicalContainment.Contains(toolPath, machine.CommandPath));
                var publication = new ToolWriteAuxiliaryPublication
                {
                    RemovedToolIds = deletedTool ? [machine.ToolId] : [],
                    RemovedRuntimeVersions = [.. approvedRuntimes.Where(runtime =>
                        removed.RemovedPaths.Contains(runtime.RuntimePath, PhysicalIdentityTracker.PathComparer)).Select(runtime => runtime.Version)],
                };
                if ((publication.RemovedToolIds.Count != 0 || publication.RemovedRuntimeVersions.Count != 0)
                    && !await publisher.PublishAuxiliaryAsync(state, plan, publication, cancellationToken).ConfigureAwait(false)) return;
                if (removed.Failure is not null)
                {
                    state.Findings.Add(new(ToolWriteFindingCode.VerificationFailed, CliSemanticStatus.Failed, tool.Entry.Identity.Id, Cause: removed.Failure));
                    state.Stopped = true;
                    return;
                }
            }
            // Remove never edits PATH. After the last tool with a launcher in bin, print the exact entry or block to remove.
            if (removedBinTool && !state.Records.Machine.Values.Any(value => !removing.Contains(value.ToolId) && HasBinLauncher(plan, value)))
                AddInstructions(state, PathRemovalLines(plan, state.Records.Path));
            return;
        }

        if (!UsesBin(plan)) return;
        var facts = await pathHost.ObserveAsync(plan.Input.Facts.HomePath, cancellationToken).ConfigureAwait(false);
        if (ToolWriteCancellation.Stop(state, cancellationToken)) return;
        if (facts.Readiness == ToolPathReadiness.Ready) return;
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
        AddInstructions(state, result.ManualLines);
        state.Backups.AddRange(result.PendingBackups);
        if (answer.AddForMe)
            state.Outcomes.Add(new(plan.Tools[^1].Entry.Identity.Id, ToolWriteStages.Path,
                result.Failure is null ? ToolWriteStages.Completed : ToolWriteStages.Failed,
                [.. (result.State?.Changes ?? []).Select(change => change.Target)],
                result.Failure is null ? [] : [facts.BinPath]));
        if (ToolWriteCancellation.Stop(state, cancellationToken)) return;
        if (answer.AddForMe && result.State is { } applied
            && !await publisher.PublishAuxiliaryAsync(state, plan, new() { Path = applied }, cancellationToken).ConfigureAwait(false)) return;
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
