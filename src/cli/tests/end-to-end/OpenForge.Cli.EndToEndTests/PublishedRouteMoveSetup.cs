using OpenForge.Cli.EndToEndTests.Shared.Route;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal static class PublishedRouteMoveSetup
{
    internal static void DeleteArtifacts(TemporaryWorkspace temporary)
    {
        string[] routePaths =
        [
            PublishedRouteWorkspaceSeed.SourcePath,
            PublishedRouteWorkspaceSeed.SourceOverwritePath,
            PublishedRouteMoveWorkspace.DestinationPath,
            PublishedRouteMoveWorkspace.DestinationOverwritePath,
            PublishedRouteWorkspaceSeed.CategoryPath,
            ".agents/guidance/topics/child.md",
            ".agents/guidance/topics/notes.md",
            ".agents/guidance/topics/image.bin",
            ".agents/archive/_archive.md",
            "invalid.md",
            "README.md",
            "definitions.md",
            ".agents/open-forge.lifecycle.json",
            ".agents/open-forge.lock.json",
            "AGENTS.md",
            "CLAUDE.md",
        ];
        foreach (var path in PublishedInstallWorkspace.EmbeddedPayloadPaths
                     .Concat(routePaths)
                     .Distinct(StringComparer.Ordinal))
        {
            DeleteOrdinaryFileIfPresent(temporary.Combine(path));
        }

        foreach (var directory in PublishedInstallWorkspace.EmbeddedPayloadPaths
                     .Select(path => Path.GetDirectoryName(temporary.Combine(path)))
                     .Append(temporary.Combine(".agents/guidance/topics"))
                     .Append(temporary.Combine(".agents/archive"))
                     .Append(temporary.Combine(".agents"))
                     .OfType<string>()
                     .Distinct(StringComparer.Ordinal)
                     .OrderByDescending(path => path.Length))
        {
            DeleteEmptyOrdinaryDirectoryIfPresent(directory);
        }
    }

    private static void DeleteOrdinaryFileIfPresent(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException(
                "The published Route Move cleanup target is not an ordinary file.");
        }

        File.Delete(path);
    }

    private static void DeleteEmptyOrdinaryDirectoryIfPresent(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException(
                "The published Route Move cleanup target is not an ordinary directory.");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
