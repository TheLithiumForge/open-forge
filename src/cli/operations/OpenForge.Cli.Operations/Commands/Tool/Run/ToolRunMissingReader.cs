using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run;

// Registration point: workspace Status and Doctor adapt the same missing-tool fact as Run.
internal sealed class ToolRunMissingReader(ToolRunOperation operation)
{
    internal async ValueTask<ToolRunFailure[]> ReadAsync(CliWorkspace workspace, CancellationToken token)
    {
        var observed = await ToolWorkspaceReader.ReadAsync(workspace, token).ConfigureAwait(false);
        return await ReadAsync(workspace, new(observed.Settings, observed.Lock), token).ConfigureAwait(false);
    }

    internal async ValueTask<ToolRunFailure[]> ReadAsync(CliWorkspace workspace, ToolRunWorkspaceRecords records, CancellationToken token)
    {
        var missing = new List<ToolRunFailure>();
        foreach (var id in records.Settings.Document.Tools.Keys.Order(StringComparer.Ordinal))
        {
            var resolution = await operation.ResolveSelectedAsync(new()
            {
                Id = id,
                WorkspacePath = workspace.LexicalRoot,
                CurrentDirectory = workspace.LexicalRoot,
                HostProjectDirectory = null,
                Arguments = [],
                Interactive = false,
            }, workspace, records, token).ConfigureAwait(false);
            if (resolution is ToolRunRefused { Failure.Code: ToolRunFailureCode.DeclaredToolMissing } refused) missing.Add(refused.Failure);
        }
        token.ThrowIfCancellationRequested();
        return [.. missing];
    }
}
