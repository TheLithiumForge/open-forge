using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal enum SourceLoaderEntriesParseState
{
    Valid,
    Malformed,
}

internal sealed class SourceLoaderEntriesParseResult
{
    private SourceLoaderEntriesParseResult(
        SourceLoaderEntriesParseState state,
        IReadOnlyList<SourceLoaderDestinationParseResult> destinations,
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

        if (state == SourceLoaderEntriesParseState.Valid
            && (cause is not null || attemptedDestination is not null)
            || state == SourceLoaderEntriesParseState.Malformed && string.IsNullOrWhiteSpace(cause))
        {
            throw new ArgumentException("The Loader Entries parse fields do not match the parse state.");
        }

        State = state;
        Destinations = destinations;
        Cause = cause;
        AttemptedDestination = attemptedDestination;
    }

    internal SourceLoaderEntriesParseState State { get; }

    internal IReadOnlyList<SourceLoaderDestinationParseResult> Destinations { get; }

    internal string? Cause { get; }

    internal string? AttemptedDestination { get; }

    internal static SourceLoaderEntriesParseResult Valid(
        IEnumerable<SourceLoaderDestinationParseResult> destinations)
    {
        ArgumentNullException.ThrowIfNull(destinations);
        return new SourceLoaderEntriesParseResult(
            SourceLoaderEntriesParseState.Valid,
            new ReadOnlyCollection<SourceLoaderDestinationParseResult>(destinations.ToArray()),
            null,
            null);
    }

    internal static SourceLoaderEntriesParseResult Malformed(
        string cause,
        string? attemptedDestination = null,
        IEnumerable<SourceLoaderDestinationParseResult>? destinations = null)
    {
        return new SourceLoaderEntriesParseResult(
            SourceLoaderEntriesParseState.Malformed,
            new ReadOnlyCollection<SourceLoaderDestinationParseResult>(destinations?.ToArray() ?? []),
            cause,
            attemptedDestination);
    }
}
