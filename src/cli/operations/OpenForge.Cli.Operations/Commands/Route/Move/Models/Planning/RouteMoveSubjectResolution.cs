using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Navigation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveResolvedLayer
{
    public required SourceLayer Layer { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed record RouteMoveResolvedSubject
{
    public required RouteMoveRequest Request { get; init; }

    public required RouteMoveSource Source { get; init; }

    public required RouteMoveSubjectKind Kind { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceLogicalSource SelectedSource { get; init; }

    public SourceRouteFacts? RouteFacts { get; init; }

    public RouteNavigationExposure NavigationExposure { get; init; } = new()
    {
        IsCancelled = false,
    };

    public ImmutableArray<RouteMoveResolvedLayer> Layers { get; init; } = [];
}

internal sealed record RouteMoveSubjectSelection
{
    internal RouteMoveSubjectSelection(
        RouteMoveResolvedSubject? subject,
        RouteMoveResultFormation? boundary)
    {
        if ((subject is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move subject selection requires exactly one subject or boundary.");
        }

        Subject = subject;
        Boundary = boundary;
    }

    internal RouteMoveResolvedSubject? Subject { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveSubjectCatalogueSelection
{
    internal RouteMoveSubjectCatalogueSelection(
        SourceCatalogue? catalogue,
        RouteMoveResultFormation? boundary)
    {
        if ((catalogue is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move subject discovery requires exactly one catalogue or boundary.");
        }

        Catalogue = catalogue;
        Boundary = boundary;
    }

    internal SourceCatalogue? Catalogue { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveSubjectDiscovery
{
    public required RouteMoveRequest Request { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceLogicalSource SelectedSource { get; init; }

    public required RouteMoveSource Source { get; init; }

    public required RouteMoveSubjectKind Kind { get; init; }
}

internal sealed record RouteMoveSubjectDiscoveryResult
{
    internal RouteMoveSubjectDiscoveryResult(
        RouteMoveSubjectDiscovery? discovery,
        RouteMoveResultFormation? boundary)
    {
        if ((discovery is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move subject discovery requires exactly one source or boundary.");
        }

        Discovery = discovery;
        Boundary = boundary;
    }

    internal RouteMoveSubjectDiscovery? Discovery { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveSubjectResolution
{
    internal RouteMoveSubjectResolution(
        RouteMoveResolvedSubject? subject,
        RouteMoveResultFormation? boundary)
    {
        if ((subject is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move subject resolution requires exactly one subject or boundary.");
        }

        Subject = subject;
        Boundary = boundary;
    }

    internal RouteMoveResolvedSubject? Subject { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}
