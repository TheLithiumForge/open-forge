using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

internal static class SourceLinkLexicalPathResolver
{
    internal static SourceLinkLexicalPathResult Resolve(
        string lexicalRoot,
        string sourceCanonicalPath,
        string decodedPath)
    {
        var sourceLexicalPath = Path.Combine(
            lexicalRoot,
            sourceCanonicalPath.Replace('/', Path.DirectorySeparatorChar));
        var sourceDirectory = Path.GetDirectoryName(sourceLexicalPath);
        if (sourceDirectory is null)
        {
            return SourceLinkLexicalPathResult.SourceDirectoryMissing;
        }

        string lexicalTarget;
        try
        {
            lexicalTarget = Path.GetFullPath(
                string.IsNullOrEmpty(decodedPath)
                    ? sourceLexicalPath
                    : Path.Combine(sourceDirectory, decodedPath));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return SourceLinkLexicalPathResult.Malformed;
        }

        if (!PhysicalContainment.Contains(lexicalRoot, lexicalTarget))
        {
            return SourceLinkLexicalPathResult.OutsideWorkspace;
        }

        var canonicalPath = ReadCanonicalPath(lexicalRoot: lexicalRoot, target: lexicalTarget);
        return SourceLinkLexicalPathResult.Complete(lexicalTarget: lexicalTarget, canonicalPath: canonicalPath);
    }

    private static string ReadCanonicalPath(string lexicalRoot, string target)
    {
        var relative = Path.GetRelativePath(lexicalRoot, target);
        return relative == "."
            ? "."
            : relative.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
    }
}
