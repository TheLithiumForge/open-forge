using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal static class StatusInstalledWorkspaceArtifacts
{
    internal static bool Contains(string relativePath)
        => InstallOperationWorkspace.EmbeddedPayloadPaths.Contains(
                relativePath,
                StringComparer.Ordinal)
            || relativePath is StatusIntegrationWorkspace.LifecyclePath
                or StatusIntegrationWorkspace.OwnershipPath
                or "AGENTS.md"
                or "CLAUDE.md";

    internal static void Delete(TemporaryWorkspace temporary)
    {
        foreach (var path in InstallOperationWorkspace.EmbeddedPayloadPaths
                     .Append(StatusIntegrationWorkspace.LifecyclePath)
                     .Append(StatusIntegrationWorkspace.OwnershipPath)
                     .Append("AGENTS.md")
                     .Append("CLAUDE.md"))
        {
            DeleteOrdinaryFileIfPresent(temporary.Combine(path));
        }

        var directories = new HashSet<string>(StringComparer.Ordinal)
        {
            temporary.Combine(".agents"),
        };
        foreach (var path in InstallOperationWorkspace.EmbeddedPayloadPaths)
        {
            var directory = System.IO.Path.GetDirectoryName(temporary.Combine(path));
            if (directory is not null)
            {
                directories.Add(directory);
            }
        }

        foreach (var directory in directories.OrderByDescending(path => path.Length))
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
            throw new InvalidOperationException("The Status install cleanup target is not an ordinary file.");
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
            throw new InvalidOperationException("The Status install cleanup target is not an ordinary directory.");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
