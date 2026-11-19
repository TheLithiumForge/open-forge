using OpenForge.Cli.Core.Commands.Stripping;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;

namespace OpenForge.Cli.Core.Commands.Uninstall;

// Uninstall applies the shared plan in place. The runtime folder is the plan's own last step.
internal sealed class UninstallOperation(StripOperationRunner runner)
{
    internal ValueTask<StripResult> ExecuteAsync(StripRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        return runner.RunAsync(request, token);
    }
}
