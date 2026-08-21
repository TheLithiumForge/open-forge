namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal enum RouteListSourceReferenceKind
{
    LoaderRoots,
    SourceId,
    SourcePath,
}

internal enum RouteListSourceReferenceParseState
{
    Valid,
    Invalid,
}

internal sealed class RouteListSourceReferenceParseResult
{
    private RouteListSourceReferenceParseResult(
        RouteListSourceReferenceParseState state,
        RouteListSourceReferenceKind kind,
        string? attemptedId,
        string? attemptedPath,
        string? cause)
    {
        State = state;
        Kind = kind;
        AttemptedId = attemptedId;
        AttemptedPath = attemptedPath;
        Cause = cause;
    }

    internal RouteListSourceReferenceParseState State { get; }

    internal RouteListSourceReferenceKind Kind { get; }

    internal string? AttemptedId { get; }

    internal string? AttemptedPath { get; }

    internal string? Cause { get; }

    internal static RouteListSourceReferenceParseResult LoaderRoots()
    {
        return new RouteListSourceReferenceParseResult(
            RouteListSourceReferenceParseState.Valid,
            RouteListSourceReferenceKind.LoaderRoots,
            null,
            null,
            null);
    }

    internal static RouteListSourceReferenceParseResult ValidId(string attemptedId)
    {
        return new RouteListSourceReferenceParseResult(
            RouteListSourceReferenceParseState.Valid,
            RouteListSourceReferenceKind.SourceId,
            attemptedId,
            null,
            null);
    }

    internal static RouteListSourceReferenceParseResult ValidPath(string attemptedPath)
    {
        return new RouteListSourceReferenceParseResult(
            RouteListSourceReferenceParseState.Valid,
            RouteListSourceReferenceKind.SourcePath,
            null,
            attemptedPath,
            null);
    }

    internal static RouteListSourceReferenceParseResult InvalidId(string attemptedId, string cause)
    {
        return new RouteListSourceReferenceParseResult(
            RouteListSourceReferenceParseState.Invalid,
            RouteListSourceReferenceKind.SourceId,
            attemptedId,
            null,
            cause);
    }

    internal static RouteListSourceReferenceParseResult InvalidPath(string attemptedPath, string cause)
    {
        return new RouteListSourceReferenceParseResult(
            RouteListSourceReferenceParseState.Invalid,
            RouteListSourceReferenceKind.SourcePath,
            null,
            attemptedPath,
            cause);
    }
}

internal static class RouteListSourceReferenceParser
{
    private const string AgentsPrefix = ".agents/";
    private const string DotAgentsPrefix = "./.agents/";

    internal static RouteListSourceReferenceParseResult Parse(string? value)
    {
        if (value is null)
        {
            return RouteListSourceReferenceParseResult.LoaderRoots();
        }

        if (value.StartsWith(AgentsPrefix, StringComparison.Ordinal)
            || value.StartsWith(DotAgentsPrefix, StringComparison.Ordinal))
        {
            var attemptedPath = value.StartsWith(DotAgentsPrefix, StringComparison.Ordinal)
                ? AgentsPrefix + value[DotAgentsPrefix.Length..]
                : value;
            return ParsePath(attemptedPath);
        }

        return ParseId(value);
    }

    internal static bool IsValidId(string? value)
    {
        return value is not null && IsValidSegments(value, allowAgentsPrefix: false);
    }

    internal static bool IsValidCanonicalPath(string? value)
    {
        return value is not null
            && value.StartsWith(AgentsPrefix, StringComparison.Ordinal)
            && IsValidSegments(value[AgentsPrefix.Length..], allowAgentsPrefix: false);
    }

    private static RouteListSourceReferenceParseResult ParseId(string value)
    {
        if (!IsValidId(value))
        {
            return RouteListSourceReferenceParseResult.InvalidId(
                value,
                "A source ID must contain non-empty path segments without traversal or control characters.");
        }

        return RouteListSourceReferenceParseResult.ValidId(value);
    }

    private static RouteListSourceReferenceParseResult ParsePath(string attemptedPath)
    {
        if (!IsValidCanonicalPath(attemptedPath))
        {
            return RouteListSourceReferenceParseResult.InvalidPath(
                attemptedPath,
                "An exact source path must contain non-empty .agents segments without traversal or control characters.");
        }

        return RouteListSourceReferenceParseResult.ValidPath(attemptedPath);
    }

    private static bool IsValidSegments(string value, bool allowAgentsPrefix)
    {
        if (value.Length == 0)
        {
            return false;
        }

        if (allowAgentsPrefix && value.StartsWith(AgentsPrefix, StringComparison.Ordinal))
        {
            value = value[AgentsPrefix.Length..];
        }

        if (value.Length == 0 || value.IndexOf('\\') >= 0)
        {
            return false;
        }

        foreach (var segment in value.Split('/', StringSplitOptions.None))
        {
            if (segment.Length == 0 || segment is "." or ".." || ContainsControlCharacter(segment))
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsControlCharacter(string value)
    {
        foreach (var character in value)
        {
            if (char.IsControl(character))
            {
                return true;
            }
        }

        return false;
    }
}
