using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Navigation;

internal sealed record RouteNavigationExposure
{
    public ImmutableArray<string> ExposedPaths { get; init; } = [];

    public ImmutableArray<string> UnavailableParents { get; init; } = [];

    public required bool IsCancelled { get; init; }
}
