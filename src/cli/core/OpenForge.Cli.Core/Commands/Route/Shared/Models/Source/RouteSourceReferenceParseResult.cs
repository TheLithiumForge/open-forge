namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

internal enum RouteSourceReferenceKind
{
    SourceId,
    SourcePath,
}

internal enum RouteSourceReferenceParseState
{
    Valid,
    Invalid,
}

internal sealed class RouteSourceReferenceParseResult
{
    private RouteSourceReferenceParseResult(
        RouteSourceReferenceParseState state,
        RouteSourceReferenceKind kind,
        string? attemptedId,
        string? attemptedPath,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source-reference parse state is not defined.");
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The source-reference kind is not defined.");
        }

        ValidateShape(state, kind, attemptedId, attemptedPath, cause);
        State = state;
        Kind = kind;
        AttemptedId = attemptedId;
        AttemptedPath = attemptedPath;
        Cause = cause;
    }

    internal RouteSourceReferenceParseState State { get; }

    internal RouteSourceReferenceKind Kind { get; }

    internal string? AttemptedId { get; }

    internal string? AttemptedPath { get; }

    internal string? Cause { get; }

    internal static RouteSourceReferenceParseResult ValidId(string attemptedId)
    {
        return new RouteSourceReferenceParseResult(
            RouteSourceReferenceParseState.Valid,
            RouteSourceReferenceKind.SourceId,
            attemptedId,
            null,
            null);
    }

    internal static RouteSourceReferenceParseResult ValidPath(string attemptedPath)
    {
        return new RouteSourceReferenceParseResult(
            RouteSourceReferenceParseState.Valid,
            RouteSourceReferenceKind.SourcePath,
            null,
            attemptedPath,
            null);
    }

    internal static RouteSourceReferenceParseResult InvalidId(string attemptedId, string cause)
    {
        return new RouteSourceReferenceParseResult(
            RouteSourceReferenceParseState.Invalid,
            RouteSourceReferenceKind.SourceId,
            attemptedId,
            null,
            cause);
    }

    internal static RouteSourceReferenceParseResult InvalidPath(string attemptedPath, string cause)
    {
        return new RouteSourceReferenceParseResult(
            RouteSourceReferenceParseState.Invalid,
            RouteSourceReferenceKind.SourcePath,
            null,
            attemptedPath,
            cause);
    }

    private static void ValidateShape(
        RouteSourceReferenceParseState state,
        RouteSourceReferenceKind kind,
        string? attemptedId,
        string? attemptedPath,
        string? cause)
    {
        if ((kind == RouteSourceReferenceKind.SourceId) != (attemptedId is not null)
            || (kind == RouteSourceReferenceKind.SourcePath) != (attemptedPath is not null))
        {
            throw new ArgumentException("The attempted source identity does not match the parse kind.");
        }

        if (state == RouteSourceReferenceParseState.Valid && cause is not null
            || state == RouteSourceReferenceParseState.Invalid && cause is null)
        {
            throw new ArgumentException("Only invalid source-reference results carry a cause.", nameof(cause));
        }
    }
}
