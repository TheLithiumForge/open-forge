using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Application;

internal sealed class FindSourceResolver(
    SourceReadSessionRead sourceSessionRead,
    FindUniverseResolver universeResolver,
    SourcePhysicalPathResolver physicalPathResolver,
    FindRouteFactsReader routeFactsReader)
{
    private readonly SourceReadSessionRead _sourceSessionRead = sourceSessionRead;
    private readonly FindUniverseResolver _universeResolver = universeResolver;
    private readonly SourcePhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FindRouteFactsReader _routeFactsReader = routeFactsReader;

    internal ValueTask<SourceReadSession> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => _sourceSessionRead(workspace, cancellationToken);

    internal void ProbePairedOverwritePaths(
        FindRequest request,
        SourceReadSession sourceSession,
        CancellationToken cancellationToken)
    {
        var probedPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in request.UniverseFilter.Include.Concat(request.UniverseFilter.Exclude))
        {
            var parsed = SourceReferenceParser.Parse(value);
            if (parsed.State != SourceReferenceParseState.Valid
                || parsed.Kind != SourceReferenceKind.SourcePath
                || parsed.AttemptedPath is not { } path
                || !probedPaths.Add(path))
            {
                continue;
            }

            var source = sourceSession.Catalogue.FindByPath(path);
            if (source?.Overwrite is not { CanonicalPath: var overwritePath }
                || !string.Equals(overwritePath, path, StringComparison.Ordinal))
            {
                continue;
            }

            cancellationToken.ThrowIfCancellationRequested();
            _physicalPathResolver(request.Workspace, path);
        }
    }

    internal FindUniverseResolution ResolveUniverse(FindUniverseInput input)
        => _universeResolver.Resolve(input);

    internal ValueTask<SourceRouteFacts> ReadRouteFactsAsync(
        FindRouteFactsInput input,
        CancellationToken cancellationToken)
        => _routeFactsReader(
            new SourceRouteFactsRequest(
                input.SourceSession.Catalogue,
                input.Selection),
            input.SourceSession.DocumentReader,
            cancellationToken);
}
