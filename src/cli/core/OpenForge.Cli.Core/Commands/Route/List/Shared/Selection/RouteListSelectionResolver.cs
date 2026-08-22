using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Loader;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed class RouteListSelectionResolver
{
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly RouteListExplicitSelectionResolver _explicitResolver;

    internal RouteListSelectionResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
        _explicitResolver = new RouteListExplicitSelectionResolver(physicalPathResolver);
    }

    internal async ValueTask<RouteListSelectionResolution> ResolveAsync(
        RouteListRequest request,
        RouteSourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(catalogue);

        if (request.SourceReference is null)
        {
            var loader = await new LoaderDestinationResolver(_physicalPathResolver)
                .ResolveAsync(request.Workspace, catalogue, cancellationToken)
                .ConfigureAwait(false);
            return FromLoaderResolution(loader);
        }

        var parsed = RouteSourceReferenceParser.Parse(request.SourceReference);
        if (parsed.State == RouteSourceReferenceParseState.Invalid)
        {
            return RouteListSelectionResolutionFactory.Invalid(
                CreateSelection(parsed),
                new RouteListSelectionIssue(
                    RouteListFindingCode.InvalidSourceReference,
                    parsed.AttemptedId ?? parsed.AttemptedPath,
                    parsed.Cause!));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(CreateSelection(parsed), parsed.AttemptedId ?? parsed.AttemptedPath ?? ".agents/loader.md");
        }

        return _explicitResolver.Resolve(parsed, request, catalogue);
    }

    private static RouteListSelection CreateSelection(RouteSourceReferenceParseResult parsed)
    {
        return parsed.Kind switch
        {
            RouteSourceReferenceKind.SourceId => RouteListSelectionFactory.AttemptedId(parsed.AttemptedId!),
            RouteSourceReferenceKind.SourcePath => RouteListSelectionFactory.AttemptedPath(parsed.AttemptedPath!),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source reference kind is not defined."),
        };
    }

    private static RouteListSelectionResolution FromLoaderResolution(
        LoaderDestinationResolution loader)
    {
        var selection = RouteListSelectionFactory.LoaderRoots();
        return loader.State switch
        {
            LoaderDestinationResolutionState.Resolved => RouteListSelectionResolutionFactory.Resolved(
                selection,
                loader.SelectedSources),
            LoaderDestinationResolutionState.Blocked => RouteListSelectionResolutionFactory.Blocked(
                selection,
                loader.SelectedSources,
                loader.Issues),
            LoaderDestinationResolutionState.Incomplete => RouteListSelectionResolutionFactory.Incomplete(
                selection,
                loader.SelectedSources,
                loader.Issues),
            LoaderDestinationResolutionState.Interrupted => RouteListSelectionResolutionFactory.Interrupted(
                selection,
                loader.SelectedSources,
                loader.Issues),
            _ => throw new ArgumentOutOfRangeException(nameof(loader), loader.State, "The Loader resolution state is not defined."),
        };
    }

    private static RouteListSelectionResolution Interrupted(
        RouteListSelection selection,
        string subject)
    {
        return RouteListSelectionResolutionFactory.Interrupted(
            selection,
            [],
            [new RouteListSelectionIssue(
                RouteListFindingCode.Interrupted,
                subject,
                "Source selection was interrupted.")]);
    }
}
