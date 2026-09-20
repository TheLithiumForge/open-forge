using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

internal sealed class InstallOperationWorkspace : IDisposable
{
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static readonly IReadOnlyList<string> EmbeddedPayloadPaths =
    [
        ".agents/directives/_directives.md",
        ".agents/guidance/_guidance.md",
        ".agents/loader.md",
        ".agents/maps/_maps.md",
        ".agents/memory/_memory.md",
        ".agents/memory/archived/_archived.md",
        ".agents/memory/crystallized/_crystallized.md",
        ".agents/memory/emerging/_emerging.md",
        ".agents/memory/working/_working.md",
        ".agents/patterns/_patterns.md",
        ".agents/skills/_skills.md",
        ".agents/templates/_templates.md",
    ];

    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string AgentsPath = "AGENTS.md";
    private const string ClaudePath = "CLAUDE.md";

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private bool _disposed;

    private InstallOperationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = _lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal string PhysicalPath => _temporary.Path;

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal string Combine(string relativePath)
        => _temporary.Combine(relativePath);

    internal static InstallOperationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            return new InstallOperationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal InstallRequest Request(
        InstallMode mode = InstallMode.Apply,
        bool force = false,
        bool automatic = true,
        bool allowsInteractiveConfirmation = false)
        => new(
            Workspace,
            mode,
            force,
            automatic,
            allowsInteractiveConfirmation);

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal void WriteText(string relativePath, string contents)
        => _temporary.WriteText(relativePath, contents);

    internal void CreateDirectory(string relativePath)
        => _temporary.CreateDirectory(relativePath);

    internal void ReplaceInstalledText(string relativePath, string contents)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentNullException.ThrowIfNull(contents);

        var path = _temporary.Combine(relativePath);
        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The Install test target is not an ordinary file.");
        }

        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    internal Task<string> ReadTextAsync(
        string relativePath,
        CancellationToken cancellationToken)
        => File.ReadAllTextAsync(_temporary.Combine(relativePath), cancellationToken);

    internal bool Exists(string relativePath)
        => File.Exists(_temporary.Combine(relativePath))
            || Directory.Exists(_temporary.Combine(relativePath));

    internal bool AgentsDirectoryExists()
        => Directory.Exists(_temporary.Combine(".agents"));

    internal FileStream HoldExternalLock()
        => _lockStore.OpenExclusive(Workspace);

    internal bool RecoveryDirectoryExists()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.None);
        return storeRoot is not null
            && Directory.Exists(RecoveryBundlePathIdentity.WorkspaceDirectory(
                storeRoot,
                Workspace.PhysicalRoot));
    }

    internal async ValueTask<int> ReadRecoveryCandidateCountAsync(
        CancellationToken cancellationToken)
    {
        var result = await RecoveryBundleCatalogue.ReadAsync(Workspace, cancellationToken);
        return result.State switch
        {
            RecoveryBundleCatalogueState.Available => result.Candidates.Length,
            RecoveryBundleCatalogueState.Unavailable => throw new InvalidOperationException(
                "The Install integration recovery catalogue was unavailable."),
            RecoveryBundleCatalogueState.Cancelled => throw new InvalidOperationException(
                "The Install integration recovery catalogue was cancelled."),
            _ => throw new ArgumentOutOfRangeException(
                paramName: null,
                actualValue: result.State,
                message: "The recovery catalogue state is not defined."),
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DeleteRecoveryArtifacts();
        DeleteInstallArtifacts();
        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private void DeleteInstallArtifacts()
    {
        foreach (var path in EmbeddedPayloadPaths
                     .Append(LifecyclePath)
                     .Append(OwnershipPath)
                     .Append(AgentsPath)
                     .Append(ClaudePath))
        {
            DeleteOrdinaryFileIfPresent(_temporary.Combine(path));
        }

        foreach (var directory in EmbeddedPayloadPaths
                     .Select(path => Path.GetDirectoryName(_temporary.Combine(path)))
                     .Append(_temporary.Combine(".agents"))
                     .OfType<string>()
                     .Distinct(StringComparer.Ordinal)
                     .OrderByDescending(path => path.Length))
        {
            DeleteEmptyOrdinaryDirectoryIfPresent(directory);
        }
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
            var fileName = Path.GetFileName(path);
            if (!RecoveryBundleFormatV1.TryParseCandidateFileName(
                    fileName,
                    out _,
                    out _))
            {
                throw new InvalidOperationException(
                    "The Install recovery directory contains an unrecognized artifact.");
            }

            DeleteOrdinaryFileIfPresent(path);
        }

        Directory.Delete(directory);
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
            throw new InvalidOperationException("The Install test cleanup target is not an ordinary file.");
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
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != FileAttributes.Directory)
        {
            throw new InvalidOperationException("The Install test cleanup target is not an ordinary directory.");
        }

        if (!Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path);
        }
    }
}
