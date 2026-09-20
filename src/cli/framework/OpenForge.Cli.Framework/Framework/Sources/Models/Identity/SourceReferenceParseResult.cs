namespace OpenForge.Cli.Core.Framework.Sources.Models.Identity;

internal enum SourceReferenceKind
{
    SourceId,
    SourcePath,
}

internal enum SourceReferenceParseState
{
    Valid,
    Invalid,
}

internal sealed class SourceReferenceParseResult
{
    private SourceReferenceParseResult(
        SourceReferenceParseState state,
        SourceReferenceKind kind,
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

        if ((kind == SourceReferenceKind.SourceId) != (attemptedId is not null)
            || (kind == SourceReferenceKind.SourcePath) != (attemptedPath is not null))
        {
            throw new ArgumentException("The attempted source identity does not match the parse kind.");
        }

        if (state == SourceReferenceParseState.Valid && cause is not null
            || state == SourceReferenceParseState.Invalid && cause is null)
        {
            throw new ArgumentException("Only invalid source-reference results carry a cause.", nameof(cause));
        }

        State = state;
        Kind = kind;
        AttemptedId = attemptedId;
        AttemptedPath = attemptedPath;
        Cause = cause;
    }

    internal SourceReferenceParseState State { get; }

    internal SourceReferenceKind Kind { get; }

    internal string? AttemptedId { get; }

    internal string? AttemptedPath { get; }

    internal string? Cause { get; }

    internal static SourceReferenceParseResult ValidId(string attemptedId)
    {
        return new SourceReferenceParseResult(
            state: SourceReferenceParseState.Valid,
            kind: SourceReferenceKind.SourceId,
            attemptedId: attemptedId,
            attemptedPath: null,
            cause: null);
    }

    internal static SourceReferenceParseResult ValidPath(string attemptedPath)
    {
        return new SourceReferenceParseResult(
            state: SourceReferenceParseState.Valid,
            kind: SourceReferenceKind.SourcePath,
            attemptedId: null,
            attemptedPath: attemptedPath,
            cause: null);
    }

    internal static SourceReferenceParseResult InvalidId(string attemptedId, string cause)
    {
        return new SourceReferenceParseResult(
            state: SourceReferenceParseState.Invalid,
            kind: SourceReferenceKind.SourceId,
            attemptedId: attemptedId,
            attemptedPath: null,
            cause: cause);
    }

    internal static SourceReferenceParseResult InvalidPath(string attemptedPath, string cause)
    {
        return new SourceReferenceParseResult(
            state: SourceReferenceParseState.Invalid,
            kind: SourceReferenceKind.SourcePath,
            attemptedId: null,
            attemptedPath: attemptedPath,
            cause: cause);
    }
}
