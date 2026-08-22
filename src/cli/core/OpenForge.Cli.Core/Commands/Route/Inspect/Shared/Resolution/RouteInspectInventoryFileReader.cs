using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed class RouteInspectInventoryFileReader
{
    internal async ValueTask<RouteInspectInventoryFile?> ReadAsync(
        string physicalPath,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        if (!RouteSourceFormClassifier.TryClassify(logicalPath, out var form))
        {
            return null;
        }

        var read = await StrictUtf8FileReader
            .ReadAsync(physicalPath, logicalPath, cancellationToken)
            .ConfigureAwait(false);
        return new RouteInspectInventoryFile(
            logicalPath,
            physicalPath,
            form,
            read.State,
            read.Value);
    }
}
