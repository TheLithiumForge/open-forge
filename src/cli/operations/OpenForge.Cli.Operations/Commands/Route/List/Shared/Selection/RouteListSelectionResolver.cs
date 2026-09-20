using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private sealed record RouteListSelectionStage(
        RouteListSelection Attempted,
        string AttemptedReference,
        RouteListRequest Request,
        SourceCatalogue Catalogue,
        RouteSourceProjectionSet ProjectionSet,
        SourceRouteFacts RouteFacts,
        CancellationToken CancellationToken);

    private const string LoaderPath = SourceLogicalPath.LoaderPath;

    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteListSelectionResolver(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _physicalPathResolver = physicalPathResolver;
    }

    internal ValueTask<RouteListSelectionResolution> ResolveAsync(
        RouteListRequest request,
        SourceCatalogue catalogue,
        RouteSourceProjectionSet projectionSet,
        SourceRouteFacts routeFacts,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(projectionSet);
        ArgumentNullException.ThrowIfNull(routeFacts);
        ValidateBoundary(request, catalogue, projectionSet, routeFacts);

        if (request.SourceReference is null)
        {
            return new ValueTask<RouteListSelectionResolution>(
                ResolveLoaderRoots(catalogue, projectionSet, routeFacts, cancellationToken));
        }

        var parsed = SourceReferenceParser.Parse(request.SourceReference);
        var attempted = CreateSelection(parsed);
        var attemptedReference = ReadAttemptedSubject(attempted);
        if (parsed.State == SourceReferenceParseState.Invalid)
        {
            return new ValueTask<RouteListSelectionResolution>(
                RouteListSelectionResolutionFactory.Invalid(
                    attempted,
                    new RouteListSelectionIssue(
                        RouteListFindingCode.InvalidSourceReference,
                        attemptedReference,
                        parsed.Cause ?? throw new InvalidOperationException("An invalid source reference requires a cause."))));
        }

        if (WasCancelled(catalogue, projectionSet, routeFacts, cancellationToken))
        {
            return new ValueTask<RouteListSelectionResolution>(
                Interrupted(attempted, attemptedReference));
        }

        var stage = new RouteListSelectionStage(
            attempted,
            attemptedReference,
            request,
            catalogue,
            projectionSet,
            routeFacts,
            cancellationToken);
        var resolution = parsed.Kind switch
        {
            SourceReferenceKind.SourceId => ResolveId(stage),
            SourceReferenceKind.SourcePath => ResolvePath(stage),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source reference kind is not defined."),
        };
        return new ValueTask<RouteListSelectionResolution>(resolution);
    }

    private static RouteListSelection CreateSelection(SourceReferenceParseResult parsed)
    {
        return parsed.Kind switch
        {
            SourceReferenceKind.SourceId => RouteListSelectionFactory.AttemptedId(
                parsed.AttemptedId ?? throw new InvalidOperationException("An ID reference requires an attempted ID.")),
            SourceReferenceKind.SourcePath => RouteListSelectionFactory.AttemptedPath(
                parsed.AttemptedPath ?? throw new InvalidOperationException("A path reference requires an attempted path.")),
            _ => throw new ArgumentOutOfRangeException(nameof(parsed), parsed.Kind, "The source reference kind is not defined."),
        };
    }

    private static string ReadAttemptedSubject(RouteListSelection selection)
    {
        return selection.Kind switch
        {
            RouteListSelectionKind.SourceId => selection.AttemptedId
                ?? throw new InvalidOperationException("An ID selection requires an attempted ID."),
            RouteListSelectionKind.SourcePath => selection.AttemptedPath
                ?? throw new InvalidOperationException("A path selection requires an attempted path."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selection),
                selection.Kind,
                "An explicit route-list selection kind is required."),
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
