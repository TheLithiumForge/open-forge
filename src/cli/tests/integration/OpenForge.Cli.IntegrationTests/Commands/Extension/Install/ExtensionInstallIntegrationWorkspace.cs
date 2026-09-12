using System.Text;
using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

internal sealed class ExtensionInstallIntegrationWorkspace : IDisposable
{
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly TemporaryWorkspace _workspace;
    private readonly WorkspaceLockTestStore _lockStore;

    private ExtensionInstallIntegrationWorkspace(
        TemporaryWorkspace workspace,
        WorkspaceLockTestStore lockStore)
    {
        _workspace = workspace;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = _lockStore.Track(Workspace);
    }

    internal string Path => _workspace.Path;

    internal CliWorkspace Workspace { get; }

    internal bool LockInfrastructureExists => _lockStore.InfrastructureExists;

    internal static ExtensionInstallIntegrationWorkspace Create(string purpose)
    {
        var workspace = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-locks");
        try
        {
            workspace.CreateFile("workspace-note.md", "preserve workspace content\n");
            return new ExtensionInstallIntegrationWorkspace(workspace, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal async Task SeedFrameworkAsync()
    {
        var run = await RunAsync(["install", "--automatic"]);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        Assert.True(Directory.Exists(Combine(".agents")));
        Assert.True(File.Exists(Combine(LifecyclePath)));
    }

    internal async Task<ExtensionInstallRun> RunAsync(
        string[] arguments,
        string standardInput = "",
        bool standardInputRedirected = true,
        bool promptOutputRedirected = true)
    {
        using var input = new StringReader(standardInput);
        return await RunAsync(
            arguments,
            input,
            standardInputRedirected,
            promptOutputRedirected,
            CancellationToken.None);
    }

    internal async Task<ExtensionInstallRun> RunAsync(
        string[] arguments,
        TextReader standardInput,
        bool standardInputRedirected,
        bool promptOutputRedirected,
        CancellationToken cancellationToken,
        bool readRemainingInput = true)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"),
            new CliCompositionInputs
            {
                StandardInput = standardInput,
                PromptOutput = standardError,
                StandardInputRedirected = standardInputRedirected,
                PromptOutputRedirected = promptOutputRedirected,
                LockStoreRoot = _lockStore.StoreRoot,
            });
        var completion = await application.RunAsync(
            arguments,
            new CliProcessEnvironment(Path),
            new CliOutputWriters(standardOutput, standardError),
            cancellationToken);
        return new ExtensionInstallRun(
            completion.ExitCode,
            completion.Status,
            standardOutput.ToString(),
            standardError.ToString(),
            readRemainingInput
                ? await standardInput.ReadLineAsync(CancellationToken.None)
                : null);
    }

    internal FileStream HoldLock() => _lockStore.OpenExclusive(Workspace);

    internal string Combine(params string[] segments) => _workspace.Combine(segments);

    internal IReadOnlyDictionary<string, string> Snapshot()
    {
        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var file in _workspace.SnapshotHashes())
        {
            snapshot.Add($"file:{file.Key}", file.Value);
        }

        foreach (var directory in Directory.EnumerateDirectories(
                     Path,
                     "*",
                     SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var attributes = File.GetAttributes(directory);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException(
                    "The Extension Install snapshot contains a non-ordinary workspace directory.");
            }

            var relative = System.IO.Path.GetRelativePath(Path, directory)
                .Replace(System.IO.Path.DirectorySeparatorChar, '/');
            snapshot.Add($"directory:{relative}", "ordinary");
        }

        AppendRecoverySnapshot(snapshot);
        return snapshot;
    }

    internal string ReadText(string relativePath) => File.ReadAllText(Combine(relativePath));

    internal JsonElement ReadFrameworkLifecycle()
    {
        using var document = JsonDocument.Parse(ReadText(LifecyclePath));
        return document.RootElement.GetProperty("framework").Clone();
    }

    internal JsonElement ReadExtensionsLifecycle()
    {
        using var document = JsonDocument.Parse(ReadText(LifecyclePath));
        return document.RootElement.GetProperty("extensions").Clone();
    }

    internal void CreateOccupant(string relativePath, string contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(contents);

        var path = Combine(relativePath);
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The Extension Install occupant requires a parent directory.");
        var attributes = File.GetAttributes(parent);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException("The Extension Install occupant parent is not an ordinary directory.");
        }

        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, StrictUtf8NoBom);
        writer.Write(contents);
    }

    internal void ReplaceText(string relativePath, string contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(contents);

        var path = Combine(relativePath);
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The Extension Install test target is not an ordinary file.");
        }

        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    public void Dispose()
    {
        DeleteRecoveryArtifacts();
        DeleteOrdinaryFile(Combine("AGENTS.md"));
        DeleteOrdinaryFile(Combine("CLAUDE.md"));
        DeleteOrdinaryTree(Combine(".agents"));
        _lockStore.Dispose();
        _workspace.Dispose();
    }

    private void DeleteRecoveryArtifacts()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return;
        }

        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            Workspace.PhysicalRoot);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var path in Directory.EnumerateFileSystemEntries(
                     directory,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            if (!RecoveryBundleFormatV1.TryParseCandidateFileName(
                    System.IO.Path.GetFileName(path),
                    out _,
                    out _))
            {
                throw new InvalidOperationException(
                    "The Extension Install recovery directory contains an unrecognized artifact.");
            }

            DeleteOrdinaryFile(path);
        }

        Directory.Delete(directory);
    }

    private void AppendRecoverySnapshot(SortedDictionary<string, string> snapshot)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        if (storeRoot is null)
        {
            return;
        }

        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(
            storeRoot,
            Workspace.PhysicalRoot);
        if (!Directory.Exists(directory))
        {
            return;
        }

        snapshot.Add($"recovery-directory:{directory}", "ordinary");
        foreach (var path in Directory.EnumerateFileSystemEntries(
                     directory,
                     "*",
                     SearchOption.TopDirectoryOnly).Order(StringComparer.Ordinal))
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException(
                    "The Extension Install recovery snapshot contains a non-ordinary candidate.");
            }

            snapshot.Add(
                $"recovery-candidate:{path}",
                FileExpectation.Hash(File.ReadAllBytes(path)));
        }
    }

    private static void DeleteOrdinaryTree(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
        {
            var attributes = File.GetAttributes(entry);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException(
                    "The Extension Install cleanup tree contains a non-ordinary entry.");
            }
        }

        Directory.Delete(path, recursive: true);
    }

    private static void DeleteOrdinaryFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException(
                "The Extension Install cleanup target is not an ordinary file.");
        }

        File.Delete(path);
    }
}

internal sealed record ExtensionInstallRun(
    int ExitCode,
    CliSemanticStatus Status,
    string StandardOutput,
    string StandardError,
    string? RemainingInput);

internal sealed class ExtensionInstallCatalogue : IDisposable
{
    private readonly TemporaryWorkspace _source;

    private ExtensionInstallCatalogue(TemporaryWorkspace source)
    {
        _source = source;
    }

    internal string Path => _source.Path;

    internal IReadOnlyDictionary<string, string> Snapshot() => _source.SnapshotHashes();

    internal static ExtensionInstallCatalogue Create(string purpose)
        => new(TemporaryWorkspace.Create(purpose));

    internal void AddPackage(
        string id,
        IReadOnlyList<string> dependencies,
        params (string Target, string Contents)[] payload)
    {
        _source.WriteText(
            $"{id}/extension.json",
            $$"""
              {
                "id": "{{id}}",
                "name": "{{id}}",
                "description": "Extension package {{id}}.",
                "version": "1.0.0",
                "dependencies": [{{string.Join(", ", dependencies.Select(value => $"\"{value}\""))}}]
              }
              """);
        foreach (var (target, contents) in payload)
        {
            _source.WriteText($"{id}/content/{target}", contents);
        }
    }

    internal string PackagePath(string id) => _source.Combine(id);

    internal byte[] ReadPayloadBytes(string id, string target)
        => File.ReadAllBytes(_source.Combine($"{id}/content/{target}"));

    internal void ReplacePayload(string id, string target, string contents)
        => _source.ReplaceText($"{id}/content/{target}", contents);

    public void Dispose() => _source.Dispose();
}
