using System.Diagnostics.CodeAnalysis;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceLogicalPath
{
    internal const string WorkspaceEntryPath = "AGENTS.md";
    internal const string AgentsRoot = ".agents";
    internal const string LoaderPath = AgentsRoot + "/loader.md";

    internal static bool IsCanonical([NotNullWhen(true)] string? path)
    {
        return IsCanonicalSource(path);
    }

    internal static bool IsCanonicalRoot([NotNullWhen(true)] string? path)
    {
        if (path == AgentsRoot)
        {
            return true;
        }

        return IsCanonicalSource(path);
    }

    internal static bool IsCanonicalSource([NotNullWhen(true)] string? path)
    {
        if (path is null || path == AgentsRoot)
        {
            return false;
        }

        return path.StartsWith($"{AgentsRoot}/", StringComparison.Ordinal)
            && path[(AgentsRoot.Length + 1)..]
                .Split('/', StringSplitOptions.None)
                .All(IsCanonicalSegment);
    }

    internal static bool IsCanonicalSegment([NotNullWhen(true)] string? segment)
    {
        if (string.IsNullOrEmpty(segment)
            || segment is "." or ".."
            || segment.IndexOfAny(['/', '\\']) >= 0)
        {
            return false;
        }

        return segment.All(character => !char.IsControl(character));
    }

    internal static string ReadParent(string logicalPath)
    {
        if (!IsCanonicalSource(logicalPath))
        {
            throw new ArgumentException("The logical path does not have a canonical parent.", nameof(logicalPath));
        }

        return logicalPath[..logicalPath.LastIndexOf('/')];
    }

    internal static string ReadFileName(string logicalPath)
    {
        if (!IsCanonicalSource(logicalPath))
        {
            throw new ArgumentException("The logical path does not identify a canonical source.", nameof(logicalPath));
        }

        return logicalPath[(logicalPath.LastIndexOf('/') + 1)..];
    }

    internal static string Combine(string parent, string name)
    {
        if (!IsCanonicalRoot(parent))
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
        if (!IsCanonicalRoot(logicalPath))
        {
            throw new ArgumentException("The logical path is not canonical.", nameof(logicalPath));
        }

        return Path.Combine(
            workspaceRoot,
            logicalPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
