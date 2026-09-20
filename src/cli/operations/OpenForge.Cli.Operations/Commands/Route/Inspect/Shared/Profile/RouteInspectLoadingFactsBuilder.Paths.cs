using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal sealed partial class RouteInspectLoadingFactsBuilder
{
    private static bool HasTag(RouteSource source, string tag)
    {
        return source.Metadata.Tags.Contains(tag, StringComparer.Ordinal);
    }

    private void CheckCancellation()
    {
        _cancellationToken.ThrowIfCancellationRequested();
    }
}
