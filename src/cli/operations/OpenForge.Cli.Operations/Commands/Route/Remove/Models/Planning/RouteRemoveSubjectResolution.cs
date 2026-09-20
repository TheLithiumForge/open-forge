using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Navigation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;

internal sealed record RouteRemoveResolvedLayer
{
    public required SourceLayer Layer { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed record RouteRemoveResolvedSubject
{
    public required RouteRemoveRequest Request { get; init; }

    public required RouteRemoveSource Source { get; init; }

    public required RouteRemoveSubjectKind Kind { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceLogicalSource SelectedSource { get; init; }

    public SourceRouteFacts? RouteFacts { get; init; }

    public RouteNavigationExposure NavigationExposure { get; init; } = new()
    {
        IsCancelled = false,
    };

    public ImmutableArray<RouteRemoveResolvedLayer> Layers { get; init; } = [];
}

internal sealed record RouteRemoveSubjectSelection
{
    internal RouteRemoveSubjectSelection(
        RouteRemoveResolvedSubject? subject,
        RouteRemoveResultFormation? boundary)
    {
        if ((subject is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove subject selection requires exactly one subject or boundary.");
        }

        Subject = subject;
        Boundary = boundary;
    }

    internal RouteRemoveResolvedSubject? Subject { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal sealed record RouteRemoveSubjectCatalogueSelection
{
    internal RouteRemoveSubjectCatalogueSelection(
        SourceCatalogue? catalogue,
        RouteRemoveResultFormation? boundary)
    {
        if ((catalogue is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove subject discovery requires exactly one catalogue or boundary.");
        }

        Catalogue = catalogue;
        Boundary = boundary;
    }

    internal SourceCatalogue? Catalogue { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal sealed record RouteRemoveSubjectDiscovery
{
    public required RouteRemoveRequest Request { get; init; }

    public required SourceCatalogue Catalogue { get; init; }

    public required SourceLogicalSource SelectedSource { get; init; }

    public required RouteRemoveSource Source { get; init; }

    public required RouteRemoveSubjectKind Kind { get; init; }
}

internal sealed record RouteRemoveSubjectDiscoveryResult
{
    internal RouteRemoveSubjectDiscoveryResult(
        RouteRemoveSubjectDiscovery? discovery,
        RouteRemoveResultFormation? boundary)
    {
        if ((discovery is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove subject discovery requires exactly one source or boundary.");
        }

        Discovery = discovery;
        Boundary = boundary;
    }

    internal RouteRemoveSubjectDiscovery? Discovery { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}

internal sealed record RouteRemoveSubjectResolution
{
    internal RouteRemoveSubjectResolution(
        RouteRemoveResolvedSubject? subject,
        RouteRemoveResultFormation? boundary)
    {
        if ((subject is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Remove subject resolution requires exactly one subject or boundary.");
        }

        Subject = subject;
        Boundary = boundary;
    }

    internal RouteRemoveResolvedSubject? Subject { get; }

    internal RouteRemoveResultFormation? Boundary { get; }
}
