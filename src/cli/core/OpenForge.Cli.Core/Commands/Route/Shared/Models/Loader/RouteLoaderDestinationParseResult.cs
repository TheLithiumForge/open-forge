namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Loader;

internal enum RouteLoaderDestinationParseState
{
    Valid,
    Malformed,
    Unsafe,
}

internal sealed class RouteLoaderDestinationParseResult
{
    private RouteLoaderDestinationParseResult(
        RouteLoaderDestinationParseState state,
        string attemptedDestination,
        string? decodedDestination,
        string? canonicalPath,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Loader destination parse state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(attemptedDestination);
        if (state == RouteLoaderDestinationParseState.Valid)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(decodedDestination);
            ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
            if (cause is not null)
            {
                throw new ArgumentException("A valid Loader destination cannot carry a cause.", nameof(cause));
            }
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
            if (state == RouteLoaderDestinationParseState.Malformed
                && decodedDestination is not null
                || state == RouteLoaderDestinationParseState.Unsafe
                    && string.IsNullOrEmpty(decodedDestination))
            {
                throw new ArgumentException("The Loader destination parse fields do not match the parse state.");
            }

            if (state == RouteLoaderDestinationParseState.Malformed
                && canonicalPath is not null
                || state == RouteLoaderDestinationParseState.Unsafe
                    && canonicalPath is not null)
            {
                throw new ArgumentException("A non-valid Loader destination cannot carry a canonical path.", nameof(canonicalPath));
            }
        }

        State = state;
        AttemptedDestination = attemptedDestination;
        DecodedDestination = decodedDestination;
        CanonicalPath = canonicalPath;
        Cause = cause;
    }

    internal RouteLoaderDestinationParseState State { get; }

    internal string AttemptedDestination { get; }

    internal string? DecodedDestination { get; }

    internal string? CanonicalPath { get; }

    internal string? Cause { get; }

    internal static RouteLoaderDestinationParseResult Valid(
        string attemptedDestination,
        string decodedDestination,
        string canonicalPath)
    {
        return new RouteLoaderDestinationParseResult(
            RouteLoaderDestinationParseState.Valid,
            attemptedDestination,
            decodedDestination,
            canonicalPath,
            null);
    }

    internal static RouteLoaderDestinationParseResult Malformed(string attemptedDestination, string cause)
    {
        return new RouteLoaderDestinationParseResult(
            RouteLoaderDestinationParseState.Malformed,
            attemptedDestination,
            null,
            null,
            cause);
    }

    internal static RouteLoaderDestinationParseResult Unsafe(
        string attemptedDestination,
        string decodedDestination,
        string cause)
    {
        return new RouteLoaderDestinationParseResult(
            RouteLoaderDestinationParseState.Unsafe,
            attemptedDestination,
            decodedDestination,
            null,
            cause);
    }
}
