using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Execution;

// Command planners choose the ordered requests. This capability records and runs them.
internal sealed class ToolCommandExecutor(ToolRunProcess run, ToolCommandJournal journal)
{
    internal async ValueTask<ToolCommandExecutionResult> ExecuteAsync(ToolCommandExecution input, CancellationToken token)
    {
        var record = input.Record with { State = ToolCommandState.Planned, Commands = [.. input.Commands.Select(Receipt)] };
        var completed = 0;
        try
        {
            token.ThrowIfCancellationRequested();
            await journal(record, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return new(record with { State = ToolCommandState.Interrupted }, completed, null); }
        catch (Exception exception) when (IsPublicationFailure(exception))
        { return new(record with { State = ToolCommandState.Failed }, completed, exception.Message); }

        for (var index = 0; index < input.Commands.Length; index++)
        {
            var current = index;
            string? publicationFailure = null;
            var started = false;
            var request = input.Commands[index] with
            {
                Started = async cancellation =>
                {
                    if (started) throw new InvalidOperationException("A process start was reported more than once.");
                    started = true;
                    record = Set(record, current, record.Commands[current] with { State = ToolCommandState.Started }, ToolCommandState.Started);
                    try { await journal(record, cancellation).ConfigureAwait(false); }
                    catch (Exception exception) when (IsPublicationFailure(exception))
                    { publicationFailure = exception.Message; throw; }
                },
            };
            ToolProcessOutcome outcome;
            try
            {
                token.ThrowIfCancellationRequested();
                outcome = await run(request, token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            { outcome = new(ToolCommandState.Interrupted, null, "The declared command was cancelled."); }
            catch (Exception exception) when (IsPublicationFailure(exception))
            { outcome = new(ToolCommandState.Failed, null, exception.Message); }
            var command = record.Commands[index] with { State = outcome.State, ExitCode = outcome.ExitCode, Cause = outcome.Cause };
            var aggregate = ToolCommandState.Failed;
            if (outcome.State == ToolCommandState.Interrupted) aggregate = ToolCommandState.Interrupted;
            else if (outcome.Succeeded) aggregate = index == input.Commands.Length - 1 ? ToolCommandState.Exited : ToolCommandState.Started;
            record = Set(record, index, command, aggregate);
            if (outcome.Succeeded) completed++;
            try { await journal(record, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception exception) when (IsPublicationFailure(exception)) { publicationFailure = exception.Message; }
            if (!outcome.Succeeded || publicationFailure is not null) return new(record, completed, publicationFailure);
        }
        if (input.Commands.Length == 0)
        {
            record = record with { State = ToolCommandState.Exited };
            try { await journal(record, CancellationToken.None).ConfigureAwait(false); }
            catch (Exception exception) when (IsPublicationFailure(exception)) { return new(record, completed, exception.Message); }
        }
        return new(record, completed, null);
    }

    private static ToolCommandReceipt Receipt(ToolProcessRequest command)
    {
        var entry = Path.GetRelativePath(command.ExecutableRoot, command.Executable).Replace(Path.DirectorySeparatorChar, '/');
        if (!ToolIdentity.IsRelativePath(entry)) throw new ArgumentException("The executable isn't inside its recorded install.", nameof(command));
        return new()
        {
            Action = command.Action,
            Host = command.Host,
            Entry = entry,
            EntrySha256 = command.EntrySha256,
            Args = [.. command.Arguments],
            Environment = new(command.Environment, StringComparer.Ordinal),
            State = ToolCommandState.Planned,
            ExitCode = null,
            Cause = null,
        };
    }

    private static ToolRunRecord Set(ToolRunRecord record, int index, ToolCommandReceipt command, ToolCommandState state)
    {
        var commands = (ToolCommandReceipt[])record.Commands.Clone();
        commands[index] = command;
        return record with { Commands = commands, State = state };
    }

    private static bool IsPublicationFailure(Exception exception) => exception is IOException or UnauthorizedAccessException or InvalidOperationException or JsonException;
}
