namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal static class RouteListLogicalPath
{
    internal const string AgentsRoot = ".agents";

    internal static bool IsCanonical(string? path)
    {
        if (path is null)
        {
            return false;
        }

        if (path == AgentsRoot)
        {
            return true;
        }

        return path.StartsWith($"{AgentsRoot}/", StringComparison.Ordinal)
            && IsCanonicalRemainder(path[(AgentsRoot.Length + 1)..]);
    }

    internal static bool IsCanonicalSegment(string? segment)
    {
        if (string.IsNullOrEmpty(segment)
            || segment is "." or ".."
            || segment.IndexOfAny(['/', '\\']) >= 0)
        {
            return false;
        }

        foreach (var character in segment)
        {
            if (char.IsControl(character))
            {
                return false;
            }
        }

        return true;
    }

    internal static string Combine(string parent, string name)
    {
        if (!IsCanonical(parent))
        {
            throw new ArgumentException("The parent logical path is not canonical.", nameof(parent));
        }

        if (!IsCanonicalSegment(name))
        {
            throw new ArgumentException("The logical child name is not canonical.", nameof(name));
        }

        return $"{parent}/{name}";
    }

    internal static string ToLexicalPath(string workspaceRoot, string logicalPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        if (!IsCanonical(logicalPath))
        {
            throw new ArgumentException("The logical path is not canonical.", nameof(logicalPath));
        }

        return Path.Combine(
            workspaceRoot,
            logicalPath.Replace('/', Path.DirectorySeparatorChar));
    }

    internal static string ReadFileName(string logicalPath)
    {
        if (!IsCanonical(logicalPath) || logicalPath == AgentsRoot)
        {
            throw new ArgumentException("The logical path does not identify a file or child directory.", nameof(logicalPath));
        }

        return logicalPath[(logicalPath.LastIndexOf('/') + 1)..];
    }

    internal static string ReadParent(string logicalPath)
    {
        if (!IsCanonical(logicalPath) || logicalPath == AgentsRoot)
        {
            throw new ArgumentException("The logical path does not have a parent below .agents.", nameof(logicalPath));
        }

        return logicalPath[..logicalPath.LastIndexOf('/')];
    }

    private static bool IsCanonicalRemainder(string remainder)
    {
        return remainder.Length > 0
            && remainder.Split('/', StringSplitOptions.None).All(IsCanonicalSegment);
    }
}
