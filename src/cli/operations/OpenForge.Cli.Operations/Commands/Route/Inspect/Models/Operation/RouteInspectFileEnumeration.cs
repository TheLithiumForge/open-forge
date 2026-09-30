using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;

internal delegate ValueTask<RouteInspectFileEnumeration> RouteInspectFileEnumerator(
    string workspaceRoot,
    CancellationToken cancellationToken);

internal sealed record RouteInspectFileEnumeration
{
    private RouteInspectFileEnumeration(
        RouteInspectMatchingFilesScope? scope,
        ImmutableArray<string> paths,
        RouteInspectMatchingFilesReason? failure,
        int skippedIgnoreLines = 0)
    {
        Scope = scope;
        Paths = paths;
        Failure = failure;
        SkippedIgnoreLines = skippedIgnoreLines;
    }

    internal RouteInspectMatchingFilesScope? Scope { get; }
    internal ImmutableArray<string> Paths { get; }
    internal RouteInspectMatchingFilesReason? Failure { get; }
    internal int SkippedIgnoreLines { get; }

    internal static RouteInspectFileEnumeration Success(RouteInspectMatchingFilesScope scope, IEnumerable<string> paths, int skippedIgnoreLines = 0)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (!Enum.IsDefined(scope))
        {
            throw new ArgumentOutOfRangeException(nameof(scope));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(skippedIgnoreLines);
        return new(scope, paths.ToImmutableArray(), null, skippedIgnoreLines);
    }

    internal static RouteInspectFileEnumeration Unavailable(
        RouteInspectMatchingFilesScope? scope,
        RouteInspectMatchingFilesReason failure)
    {
        if ((scope.HasValue && !Enum.IsDefined(scope.Value)) || !Enum.IsDefined(failure))
        {
            throw new ArgumentOutOfRangeException(nameof(failure));
        }

        return new(scope, [], failure);
    }
}
