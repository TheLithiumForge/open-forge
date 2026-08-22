using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal static class RouteSourceReferenceParser
{
    private const string AgentsPrefix = ".agents/";
    private const string DotAgentsPrefix = "./.agents/";

    internal static RouteSourceReferenceParseResult Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

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
        return value is not null
            && !value.StartsWith(AgentsPrefix, StringComparison.Ordinal)
            && IsValidSegments(value);
    }

    internal static bool IsValidCanonicalPath(string? value)
    {
        return RouteLogicalPath.IsCanonical(value);
    }

    private static RouteSourceReferenceParseResult ParseId(string value)
    {
        return IsValidId(value)
            ? RouteSourceReferenceParseResult.ValidId(value)
            : RouteSourceReferenceParseResult.InvalidId(
                value,
                "A source ID must contain non-empty path segments without traversal or control characters.");
    }

    private static RouteSourceReferenceParseResult ParsePath(string attemptedPath)
    {
        return IsValidCanonicalPath(attemptedPath)
            ? RouteSourceReferenceParseResult.ValidPath(attemptedPath)
            : RouteSourceReferenceParseResult.InvalidPath(
                attemptedPath,
                "An exact source path must contain non-empty .agents segments without traversal or control characters.");
    }

    private static bool IsValidSegments(string value)
    {
        if (value.Length == 0 || value.IndexOf('\\') >= 0)
        {
            return false;
        }

        foreach (var segment in value.Split('/', StringSplitOptions.None))
        {
            if (!RouteLogicalPath.IsCanonicalSegment(segment))
            {
                return false;
            }
        }

        return true;
    }
}
