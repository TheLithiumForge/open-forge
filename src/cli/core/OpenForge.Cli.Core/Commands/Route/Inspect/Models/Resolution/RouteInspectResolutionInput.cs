using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectResolutionInput
{
    internal RouteInspectResolutionInput(
        RouteInspectRequest request,
        RouteSourceReferenceParseResult parsed,
        RouteInspectSelection unresolvedSelection,
        RouteSourceCatalogue catalogue,
        IReadOnlySet<string> unsafePaths,
        PhysicalPathResolution? exactPathPhysical)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(parsed);
        ArgumentNullException.ThrowIfNull(unresolvedSelection);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(unsafePaths);
        if ((parsed.Kind == RouteSourceReferenceKind.SourcePath) != (exactPathPhysical is not null))
        {
            throw new ArgumentException(
                "An exact physical path is required only for a parsed source-path reference.",
                nameof(exactPathPhysical));
        }

        Request = request;
        Parsed = parsed;
        UnresolvedSelection = unresolvedSelection;
        Catalogue = catalogue;
        UnsafePaths = unsafePaths;
        ExactPathPhysical = exactPathPhysical;
    }

    internal RouteInspectRequest Request { get; }

    internal RouteSourceReferenceParseResult Parsed { get; }

    internal RouteInspectSelection UnresolvedSelection { get; }

    internal RouteSourceCatalogue Catalogue { get; }

    internal IReadOnlySet<string> UnsafePaths { get; }

    internal PhysicalPathResolution? ExactPathPhysical { get; }
}
