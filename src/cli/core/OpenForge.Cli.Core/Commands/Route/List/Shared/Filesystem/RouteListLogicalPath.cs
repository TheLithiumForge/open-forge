using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal static class RouteListLogicalPath
{
    internal const string AgentsRoot = ".agents";

    internal static bool IsCanonical(string? path) =>
        path == AgentsRoot || RouteLogicalPath.IsCanonical(path);

    internal static bool IsCanonicalSegment(string? segment) =>
        RouteLogicalPath.IsCanonicalSegment(segment);

    internal static string Combine(string parent, string name)
    {
        if (parent == AgentsRoot)
        {
            if (!IsCanonicalSegment(name))
            {
                throw new ArgumentException("The logical child name is not canonical.", nameof(name));
            }

            return $"{parent}/{name}";
        }

        return RouteLogicalPath.Combine(parent, name);
    }

    internal static string ToLexicalPath(string workspaceRoot, string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        if (logicalPath == AgentsRoot)
        {
            return Path.Combine(workspaceRoot, AgentsRoot);
        }

        if (!RouteLogicalPath.IsCanonical(logicalPath))
        {
            throw new ArgumentException("The logical path is not canonical.", nameof(logicalPath));
        }

        return RouteLogicalPath.ToLexicalPath(workspaceRoot, logicalPath);
    }

    internal static string ReadFileName(string logicalPath)
    {
        return RouteLogicalPath.ReadFileName(logicalPath);
    }

    internal static string ReadParent(string logicalPath)
    {
        return RouteLogicalPath.ReadParent(logicalPath);
    }
}
