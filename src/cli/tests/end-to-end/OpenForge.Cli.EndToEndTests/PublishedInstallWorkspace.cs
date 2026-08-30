using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedInstallWorkspace : IDisposable
{
    private const string AgentsPath = "AGENTS.md";
    private const string ClaudePath = "CLAUDE.md";
    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static IReadOnlyList<string> EmbeddedPayloadPaths { get; } =
    [
        ".agents/directives/_directives.md",
        ".agents/guidance/_guidance.md",
        ".agents/guidance/adaptive-collaboration.md",
        ".agents/loader.md",
        ".agents/maps/_maps.md",
        ".agents/memory/_memory.md",
        ".agents/memory/archived/_archived.md",
        ".agents/memory/crystallized/_crystallized.md",
        ".agents/memory/crystallized/decisions/_decisions.md",
        ".agents/memory/crystallized/documents/_documents.md",
        ".agents/memory/emerging/_emerging.md",
        ".agents/memory/emerging/analysis/_analysis.md",
        ".agents/memory/emerging/ideas/_ideas.md",
        ".agents/memory/emerging/observations/_observations.md",
        ".agents/memory/working/_working.md",
        ".agents/memory/working/checkpoints/_checkpoints.md",
        ".agents/memory/working/handoffs/_handoffs.md",
        ".agents/patterns/_patterns.md",
        ".agents/skills/_skills.md",
        ".agents/templates/_templates.md",
        ".agents/workflows/_workflows.md",
    ];

    private readonly TemporaryWorkspace _workspace;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private bool _disposed;

    private PublishedInstallWorkspace(
        TemporaryWorkspace workspace,
        PublishedWorkspaceLockStore lockStore)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        _ = _lockStore.Track(_workspace.Path);
    }

    internal string Path => _workspace.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment =>
        _lockStore.EnvironmentVariables;

    internal string Combine(params string[] segments) => _workspace.Combine(segments);

    internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

    internal static PublishedInstallWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("e2e-install-workspace");
        var lockStore = PublishedWorkspaceLockStore.Create("e2e-install-lock-store");
        try
        {
            workspace.CreateFile("workspace-note.md", "preserve workspace sibling\n");
            return new PublishedInstallWorkspace(workspace, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal IReadOnlyList<string> InstalledPayloadPaths()
        => Directory
            .EnumerateFiles(Combine(".agents"), "*", SearchOption.AllDirectories)
            .Select(path => System.IO.Path.GetRelativePath(Path, path).Replace('\\', '/'))
            .Where(path => path is not LifecyclePath)
            .Order(StringComparer.Ordinal)
            .ToArray();

    internal Task<string> ReadTextAsync(
        string relativePath,
        CancellationToken cancellationToken)
        => File.ReadAllTextAsync(Combine(relativePath), cancellationToken);

    internal void ReplaceInstalledText(string relativePath, string contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(contents);
        var path = Combine(relativePath);
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The published Install target is not an ordinary file.");
        }

        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    internal void AssertPersistentExternalLock()
        => _lockStore.AssertPersistentZeroByteLock(Path);

    internal void AssertNoLockInfrastructure()
        => _lockStore.AssertNoInfrastructure();

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DeleteInstallArtifacts();
        _lockStore.Dispose();
        _workspace.Dispose();
        _disposed = true;
    }

    private void DeleteInstallArtifacts()
    {
        foreach (var path in EmbeddedPayloadPaths
                     .Append(LifecyclePath)
                     .Append(AgentsPath)
                     .Append(ClaudePath))
        {
            DeleteOrdinaryFileIfPresent(Combine(path));
        }

        foreach (var directory in EmbeddedPayloadPaths
                     .Select(path => System.IO.Path.GetDirectoryName(Combine(path)))
                     .Append(Combine(".agents"))
                     .Where(path => path is not null)
                     .Select(path => path!)
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
            throw new InvalidOperationException("The published Install cleanup target is not an ordinary file.");
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
            throw new InvalidOperationException("The published Install cleanup target is not an ordinary directory.");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}

internal sealed class RelocatedPublishedInstallLayout : IDisposable
{
    private readonly TemporaryWorkspace _layout;

    private RelocatedPublishedInstallLayout(
        TemporaryWorkspace layout,
        string executablePath)
    {
        _layout = layout;
        ExecutablePath = executablePath;
    }

    internal string ExecutablePath { get; }

    internal static RelocatedPublishedInstallLayout Create(PublishedExecutableTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var sourceDirectory = System.IO.Path.GetDirectoryName(target.ExecutablePath)
            ?? throw new InvalidOperationException("The published executable has no containing directory.");
        var layout = TemporaryWorkspace.Create("e2e-install-relocated-layout");
        try
        {
            CopyDirectories(layout, sourceDirectory);
            CopyFiles(layout, sourceDirectory);
            var executableRelativePath = System.IO.Path.GetRelativePath(
                sourceDirectory,
                target.ExecutablePath);
            return new RelocatedPublishedInstallLayout(
                layout,
                layout.Combine(executableRelativePath));
        }
        catch
        {
            layout.Dispose();
            throw;
        }
    }

    internal Task<ProcessRunResult> RunAsync(
        string workingDirectory,
        IReadOnlyList<string> arguments,
        IReadOnlyDictionary<string, string> environmentVariables)
        => ProcessRunner.RunAsync(
            new ProcessRunRequest(
                ExecutablePath,
                arguments,
                workingDirectory,
                timeout: TimeSpan.FromSeconds(30))
            {
                EnvironmentVariables = environmentVariables,
            },
            TestContext.Current.CancellationToken);

    public void Dispose() => _layout.Dispose();

    private static void CopyDirectories(
        TemporaryWorkspace destination,
        string sourceDirectory)
    {
        foreach (var directory in Directory.EnumerateDirectories(
                     sourceDirectory,
                     "*",
                     SearchOption.AllDirectories))
        {
            EnsureOrdinaryDirectory(directory);
            destination.CreateDirectory(System.IO.Path.GetRelativePath(
                sourceDirectory,
                directory));
        }
    }

    private static void CopyFiles(
        TemporaryWorkspace destination,
        string sourceDirectory)
    {
        foreach (var file in Directory.EnumerateFiles(
                     sourceDirectory,
                     "*",
                     SearchOption.AllDirectories))
        {
            EnsureOrdinaryFile(file);
            var copied = destination.CreateFile(
                System.IO.Path.GetRelativePath(sourceDirectory, file),
                File.ReadAllBytes(file));
            if (!OperatingSystem.IsWindows())
            {
                File.SetUnixFileMode(copied, File.GetUnixFileMode(file));
            }
        }
    }

    private static void EnsureOrdinaryDirectory(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException("The published layout contains a non-ordinary directory.");
        }
    }

    private static void EnsureOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The published layout contains a non-ordinary file.");
        }
    }
}
