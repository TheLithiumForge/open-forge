using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteInitWorkspace : IDisposable
{
    internal const string GenericTargetPath = ".agents/docs/_docs.md";
    internal const string FrameworkScopePath = ".agents/memory/mobile-app/_mobile-app.md";
    internal const string FrameworkManagedPath = ".agents/memory/mobile-app/working/_working.md";
    internal const string FrameworkFinalPath = ".agents/memory/mobile-app/working/_working.md";
    private const string MetadataMissingChildPath = ".agents/root/child.md";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly PublishedInstallWorkspace _workspace;
    private readonly string[] _routeFiles;
    private readonly string[] _routeDirectories;
    private bool _disposed;

    private PublishedRouteInitWorkspace(
        PublishedInstallWorkspace workspace,
        string[] routeFiles,
        string[] routeDirectories)
    {
        _workspace = workspace;
        _routeFiles = routeFiles;
        _routeDirectories = routeDirectories;
    }

    internal string Path => _workspace.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment => _workspace.ProcessEnvironment;

    internal static PublishedRouteInitWorkspace CreateGeneric()
        => new(
            PublishedInstallWorkspace.Create(),
            [GenericTargetPath],
            [".agents/docs"]);

    internal static PublishedRouteInitWorkspace CreateFramework()
        => new(
            PublishedInstallWorkspace.Create(),
            [FrameworkScopePath, FrameworkManagedPath, FrameworkFinalPath],
            [
                ".agents/memory/mobile-app/working",
                ".agents/memory/mobile-app/working",
                ".agents/memory/mobile-app",
            ]);

    internal static PublishedRouteInitWorkspace CreateMetadataIncomplete()
    {
        var workspace = new PublishedRouteInitWorkspace(
            PublishedInstallWorkspace.Create(),
            [MetadataMissingChildPath],
            [".agents/root"]);
        try
        {
            Directory.CreateDirectory(workspace.Combine(".agents/root"));
            File.WriteAllText(
                workspace.Combine(MetadataMissingChildPath),
                "# Child without authored metadata\n",
                StrictUtf8NoBom);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal string Combine(params string[] segments) => _workspace.Combine(segments);

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotState();

    internal Task<string> ReadTextAsync(
        string relativePath,
        CancellationToken cancellationToken)
        => _workspace.ReadTextAsync(relativePath, cancellationToken);

    internal void AssertPersistentExternalLock() => _workspace.AssertPersistentExternalLock();

    internal void AssertNoLockInfrastructure() => _workspace.AssertNoLockInfrastructure();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            foreach (var routeFile in _routeFiles)
            {
                DeleteOrdinaryFileIfPresent(Combine(routeFile));
            }

            foreach (var routeDirectory in _routeDirectories)
            {
                DeleteEmptyOrdinaryDirectoryIfPresent(Combine(routeDirectory));
            }
        }
        finally
        {
            _workspace.Dispose();
            _disposed = true;
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
            throw new InvalidOperationException("The published Route Init cleanup target is not an ordinary file.");
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
            throw new InvalidOperationException("The published Route Init cleanup target is not an ordinary directory.");
        }

        if (Directory.EnumerateFileSystemEntries(path).Any())
        {
            throw new InvalidOperationException("The published Route Init cleanup directory is not empty.");
        }

        Directory.Delete(path);
    }
}
