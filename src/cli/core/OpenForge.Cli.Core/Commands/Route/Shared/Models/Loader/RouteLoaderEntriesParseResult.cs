using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

internal enum RouteLoaderEntriesParseState
{
    Valid,
    Malformed,
}

internal sealed class RouteLoaderEntriesParseResult
{
    private RouteLoaderEntriesParseResult(
        RouteLoaderEntriesParseState state,
        IReadOnlyList<RouteLoaderDestinationParseResult> destinations,
        string? cause,
        string? attemptedDestination)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Loader Entries parse state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(destinations);
        if (destinations.Any(destination => destination is null))
        {
            throw new ArgumentException("Loader Entries destinations cannot contain null.", nameof(destinations));
        }

        if (state == RouteLoaderEntriesParseState.Valid
            && (cause is not null || attemptedDestination is not null)
            || state == RouteLoaderEntriesParseState.Malformed && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException("The Loader Entries parse fields do not match the parse state.");
        }

        State = state;
        Destinations = destinations;
        Cause = cause;
        AttemptedDestination = attemptedDestination;
    }

    internal RouteLoaderEntriesParseState State { get; }

    internal IReadOnlyList<RouteLoaderDestinationParseResult> Destinations { get; }

    internal string? Cause { get; }

    internal string? AttemptedDestination { get; }

    internal static RouteLoaderEntriesParseResult Valid(
        IEnumerable<RouteLoaderDestinationParseResult> destinations)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        return new RouteLoaderEntriesParseResult(
            RouteLoaderEntriesParseState.Valid,
            new ReadOnlyCollection<RouteLoaderDestinationParseResult>(destinations.ToArray()),
            null,
            null);
    }

    internal static RouteLoaderEntriesParseResult Malformed(
        string cause,
        string? attemptedDestination = null,
        IEnumerable<RouteLoaderDestinationParseResult>? destinations = null)
    {
        return new RouteLoaderEntriesParseResult(
            RouteLoaderEntriesParseState.Malformed,
            new ReadOnlyCollection<RouteLoaderDestinationParseResult>(destinations?.ToArray() ?? []),
            cause,
            attemptedDestination);
    }
}
