using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.List;

internal sealed class ToolListOperation(ToolReadObserver observer)
{
    internal static ToolReadPayload Catalogue(ToolCatalogue catalogue, ToolRecordsResult? records = null) => new(catalogue.Entries.OrderBy(pair => pair.Key, StringComparer.Ordinal)
        .Select(pair => ToolReadProjection.Catalogue(pair.Value, records?.Portable?.Settings.GetValueOrDefault(pair.Key))).ToArray());

    internal async ValueTask<ToolListResult> ExecuteAsync(ToolListRequest request, CancellationToken token)
    {
        try
        {
            var records = await observer.ReadRecordsAsync(request.Workspace, token).ConfigureAwait(false);
            var data = Catalogue(observer.Catalogue, records);
            var status = records.PortableState is ToolRecordReadState.Invalid or ToolRecordReadState.Unavailable ? CliSemanticStatus.Incomplete : CliSemanticStatus.Complete;
            return new()
            {
                Workspace = request.Workspace,
                Status = status,
                Data = data,
                Findings = records.Problems.Select(problem => new ToolReadFinding(ToolListDefinitions.UnavailableCode, null, problem, null)).ToArray()
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
                Data = Catalogue(observer.Catalogue),
                Findings = [new(ToolListDefinitions.UnavailableCode, null, exception.Message, null)]
            };
        }
    }
}
