using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Uninstall.Shared.Application;
using OpenForge.Cli.Core.Commands.Uninstall.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Uninstall;

internal sealed class UninstallOperation(StripOperationRunner runner)
{
    internal async ValueTask<StripResult> ExecuteAsync(StripRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await runner.RunAsync(request, new UninstallRemovalSource(new PhysicalPathResolver()), token).ConfigureAwait(false);
        if (request.DryRun || result.Status != CliSemanticStatus.Complete || result.NoOp
            || result.Recovery is not (StripRecoveryState.Removed or StripRecoveryState.NotRequired))
        {
            return result;
        }

        return UninstallRuntimeFolder.TryRemove(request.Workspace)
            ? result
            : result with
            {
                Findings = [.. result.Findings, new StripFinding
                {
                    Code = StripFindingCode.RuntimeRetained,
                    Severity = StripFindingSeverity.Info,
                    Message = $"'{WorkspaceRuntimePaths.DirectoryName}' stays because it holds earlier recovery data.",
                    Path = WorkspaceRuntimePaths.DirectoryName,
                }],
                Next = new CliNextAction(CommandLines.Cleanup, "Review the earlier recovery data."),
            };
    }
}
