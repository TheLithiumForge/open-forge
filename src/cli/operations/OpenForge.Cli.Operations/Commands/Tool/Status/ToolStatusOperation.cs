using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Status;

internal sealed class ToolStatusOperation(ToolReadObserver observer)
{
    internal async ValueTask<ToolStatusResult> ExecuteAsync(ToolStatusRequest request, CancellationToken token)
    {
        try
        {
            var records = await observer.ReadRecordsAsync(request.Workspace, token).ConfigureAwait(false);
            IReadOnlyList<string> ids = request.Id is { } id ? [id] : (records.Portable?.Settings.Keys ?? []).Order(StringComparer.Ordinal).ToArray();
            var data = await observer.ReadAsync(request.Workspace, records, ids, token).ConfigureAwait(false);
            var findings = ToolReadObserver.Findings(records, data, ToolStatusDefinitions.UnavailableCode);
            var first = findings.FirstOrDefault(finding => finding.NextCommand is not null);
            return new()
            {
                Workspace = request.Workspace,
                Status = ToolReadObserver.Status(records, data),
                Data = data,
                Findings = findings,
                Next = first?.NextCommand is { } command ? new(command, first.Cause) : null
            };
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        { return new() { Workspace = request.Workspace, Status = CliSemanticStatus.Interrupted, Data = new([]) }; }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            return new()
            {
                Workspace = request.Workspace,
                Status = CliSemanticStatus.Incomplete,
                Data = new([]),
                Findings = [new(ToolStatusDefinitions.UnavailableCode, request.Id, exception.Message, null)]
            };
        }
    }
}
