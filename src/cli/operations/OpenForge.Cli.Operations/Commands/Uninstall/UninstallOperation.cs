using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Uninstall.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Uninstall;

// Uninstall is strip with a removal source. The runtime folder is the plan's own last step.
internal sealed class UninstallOperation(StripOperationRunner runner)
{
    internal ValueTask<StripResult> ExecuteAsync(StripRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runner.RunAsync(request, new UninstallRemovalSource(new PhysicalPathResolver()), token);
    }
}
