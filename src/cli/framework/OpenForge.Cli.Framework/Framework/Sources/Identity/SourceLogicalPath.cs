using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceLogicalPath
{
    internal const string WorkspaceEntryPath = WorkspaceLayoutNames.AgentHostFileName;
    internal const string AgentsRoot = WorkspaceLayout.DefaultMainRoot;
    internal const string LoaderPath = AgentsRoot + "/" + WorkspaceLayoutNames.LoaderFileName;

    /// <summary>
    /// The guide for people at the top of `.agents/`. It is never a routed source, so agents do not
    /// read it at startup and maintenance commands neither index it nor ask it for frontmatter.
    /// </summary>
    internal const string HumanGuidePath = AgentsRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;

    internal static bool IsCanonical([NotNullWhen(true)] string? path)
    {
        return IsCanonicalSource(path);
    }

    internal static bool IsCanonicalRoot([NotNullWhen(true)] string? path)
        => IsCanonicalSource(path);

    internal static bool IsCanonicalSource([NotNullWhen(true)] string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path[0] == '/' || (path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':')) return false;
        var segments = path.AsSpan();
        foreach (var range in segments.Split('/'))
            if (!IsCanonicalSegment(segments[range])) return false;
        return true;
    }

    // Source observations retain lexical names even when portable writes would reject them.
    internal static bool IsPrimarySource(WorkspaceLayout layout, [NotNullWhen(true)] string? path)
        => IsPrimarySource(new SourceContentRoots(layout), path);

    internal static bool IsPrimarySource(SourceContentRoots roots, [NotNullWhen(true)] string? path)
    {
        if (!IsCanonicalSource(path)) return false;
        if (path == WorkspaceLayout.NativeSkillsPath) return true;
        if (!roots.IsDefault && (path == roots.MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName
            || path.StartsWith(roots.MainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName + "/", StringComparison.Ordinal))) return false;
        foreach (var root in roots.SourceRoots)
            if (path.Length > root.Length && path[root.Length] == '/' && path.StartsWith(root, StringComparison.Ordinal)) return true;
        return false;
    }

    internal static bool IsPrimaryMarkdownSource(SourceContentRoots roots, [NotNullWhen(true)] string? path)
        => IsPrimarySource(roots, path) && path.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.Ordinal);

    internal static bool IsPrimaryNonMarkdownSource(SourceContentRoots roots, string? path)
        => IsPrimarySource(roots, path) && !IsPrimaryMarkdownSource(roots, path);

    internal static bool IsPrimaryOverwriteSource(SourceContentRoots roots, [NotNullWhen(true)] string? path)
        => IsPrimarySource(roots, path) && SourceOverwritePath.HasSuffix(path);

    internal static string ReadSourceCoordinate(WorkspaceLayout layout, string path)
        => ReadSourceCoordinate(new SourceContentRoots(layout), path);

    internal static string ReadSourceCoordinate(SourceContentRoots roots, string path)
        => path[((path == WorkspaceLayout.NativeSkillsPath || path.StartsWith(WorkspaceLayout.NativeSkillsPath + "/", StringComparison.Ordinal))
            ? WorkspaceLayout.DefaultMainRoot.Length + 1 : roots.MainRoot.Length + 1)..];

    internal static bool IsCanonicalSegment([NotNullWhen(true)] string? segment)
    {
        return segment is not null && IsCanonicalSegment(segment.AsSpan());
    }

    private static bool IsCanonicalSegment(ReadOnlySpan<char> segment)
    {
        if (segment.IsEmpty || segment.SequenceEqual(".") || segment.SequenceEqual("..")) return false;
        foreach (var character in segment)
            if (character is '/' or '\\' || char.IsControl(character)) return false;
        return true;
    }

    internal static string ReadParent(string logicalPath)
    {
        if (!IsCanonicalSource(logicalPath))
        {
            throw new ArgumentException("The logical path does not have a canonical parent.", nameof(logicalPath));
        }

        var slash = logicalPath.LastIndexOf('/');
        return slash < 0 ? "." : logicalPath[..slash];
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
        if (!SourceWorkspaceRelativePath.IsCanonical(logicalPath))
        {
            throw new ArgumentException("The logical path is not canonical.", nameof(logicalPath));
        }

        return Path.Combine(
            workspaceRoot,
            logicalPath.Replace('/', Path.DirectorySeparatorChar));
    }
}
