namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal enum SourceLoaderDestinationParseState
{
    Valid,
    Malformed,
    Unsafe,
}

internal sealed class SourceLoaderDestinationParseResult
{
    private SourceLoaderDestinationParseResult(
        SourceLoaderDestinationParseState state,
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
        if (state == SourceLoaderDestinationParseState.Valid)
        {
            ArgumentException.ThrowIfNullOrEmpty(decodedDestination);
            ArgumentException.ThrowIfNullOrWhiteSpace(canonicalPath);
            if (cause is not null)
            {
                throw new ArgumentException("A valid Loader destination cannot carry a cause.", nameof(cause));
            }
        }
        else
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
            if (state == SourceLoaderDestinationParseState.Malformed
                && decodedDestination is not null
                || state == SourceLoaderDestinationParseState.Unsafe
                    && string.IsNullOrEmpty(decodedDestination))
            {
                throw new ArgumentException("The Loader destination parse fields do not match the parse state.");
            }

            if (canonicalPath is not null)
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

    internal SourceLoaderDestinationParseState State { get; }

    internal string AttemptedDestination { get; }

    internal string? DecodedDestination { get; }

    internal string? CanonicalPath { get; }

    internal string? Cause { get; }

    internal static SourceLoaderDestinationParseResult Valid(
        string attemptedDestination,
        string decodedDestination,
        string canonicalPath)
    {
        return new SourceLoaderDestinationParseResult(
            state: SourceLoaderDestinationParseState.Valid,
            attemptedDestination: attemptedDestination,
            decodedDestination: decodedDestination,
            canonicalPath: canonicalPath,
            cause: null);
    }

    internal static SourceLoaderDestinationParseResult Malformed(
        string attemptedDestination,
        string cause)
    {
        return new SourceLoaderDestinationParseResult(
            state: SourceLoaderDestinationParseState.Malformed,
            attemptedDestination: attemptedDestination,
            decodedDestination: null,
            canonicalPath: null,
            cause: cause);
    }

    internal static SourceLoaderDestinationParseResult Unsafe(
        string attemptedDestination,
        string decodedDestination,
        string cause)
    {
        return new SourceLoaderDestinationParseResult(
            state: SourceLoaderDestinationParseState.Unsafe,
            attemptedDestination: attemptedDestination,
            decodedDestination: decodedDestination,
            canonicalPath: null,
            cause: cause);
    }
}
