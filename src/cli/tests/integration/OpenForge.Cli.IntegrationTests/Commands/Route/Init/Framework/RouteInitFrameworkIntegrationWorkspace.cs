using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;

internal sealed class RouteInitFrameworkIntegrationWorkspace : IDisposable
{
    private const string OwnershipMarkerName = ".open-forge-test-workspace-owner";
    internal const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    internal const string LoaderPath = ".agents/loader.md";
    internal const string FrameworkRoot = "memory";
    internal const string FrameworkRoute = "memory/crystallized/documents";

    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private bool _disposed;

    private RouteInitFrameworkIntegrationWorkspace(
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

    internal static RouteInitFrameworkIntegrationWorkspace CreateEmpty(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            return new RouteInitFrameworkIntegrationWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal static async Task<RouteInitFrameworkIntegrationWorkspace> CreateTrustedAsync(
        string purpose,
        CancellationToken cancellationToken)
    {
        var workspace = CreateEmpty(purpose);
        try
        {
            using var input = new StringReader(string.Empty);
            using var output = new StringWriter();
            var result = await InstallOperationFactory.Create(
                    new CliInteractiveSession(input, output, canPrompt: false),
                    workspace.LockStoreRoot)
                .ExecuteAsync(
                    workspace.InstallRequest(),
                    cancellationToken);
            if (result.Status != CliSemanticStatus.Complete || result.Findings.Count != 0)
            {
                throw new InvalidOperationException(
                    "The Framework integration fixture could not establish a trusted Install: "
                    + string.Join(
                        " | ",
                        result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
            }

            if (output.ToString().Length != 0)
            {
                throw new InvalidOperationException(
                    "The automatic trusted Install fixture unexpectedly produced prompt output.");
            }

            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    internal InstallRequest InstallRequest()
        => new(
            Workspace,
            InstallMode.Apply,
            force: false,
            automatic: true,
            allowsInteractiveConfirmation: false);

    internal RouteInitRequest Request(
        string target = FrameworkRoute,
        RouteInitMode mode = RouteInitMode.Apply)
        => new(
            Workspace,
            target,
            RouteInitScaffold.Framework,
            mode,
            RouteInitMetadataInput.None);

    internal string Combine(string relativePath)
        => _temporary.Combine(relativePath);

    internal bool Exists(string relativePath)
        => File.Exists(Combine(relativePath)) || Directory.Exists(Combine(relativePath));

    internal string ReadText(string relativePath)
        => File.ReadAllText(Combine(relativePath), StrictUtf8NoBom);

    internal void WriteText(string relativePath, string contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = Combine(relativePath);
        var parent = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The Framework fixture path has no parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllText(path, contents, StrictUtf8NoBom);
    }

    internal void WriteBytes(string relativePath, byte[] contents)
    {
        ArgumentNullException.ThrowIfNull(contents);
        var path = Combine(relativePath);
        var parent = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The Framework fixture path has no parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(path, contents);
    }

    internal void Delete(string relativePath)
    {
        var path = Combine(relativePath);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
            Directory.Delete(path, recursive: true);
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotHashes()
        => _temporary.SnapshotHashes();

    internal FileStream HoldExternalLock()
        => _lockStore.OpenExclusive(Workspace);

    internal string CreateRecoveryConflictCandidate()
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
            Environment.SpecialFolderOption.Create)
            ?? throw new InvalidOperationException(
                "The Framework integration recovery store is unavailable.");
        var path = RecoveryBundlePathIdentity.FinalPath(
            storeRoot,
            Workspace.PhysicalRoot,
            Guid.NewGuid());
        Directory.CreateDirectory(
            Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException(
                "The Framework recovery candidate has no parent directory."));
        File.WriteAllBytes(path, "untrusted recovery candidate"u8.ToArray());
        return path;
    }

    internal async ValueTask<int> ReadRecoveryCandidateCountAsync(
        CancellationToken cancellationToken)
    {
        var result = await RecoveryBundleCatalogue.ReadAsync(Workspace, cancellationToken);
        return result.State switch
        {
            RecoveryBundleCatalogueState.Available => result.Candidates.Length,
            RecoveryBundleCatalogueState.Unavailable => throw new InvalidOperationException(
                "The Framework integration recovery catalogue was unavailable."),
            RecoveryBundleCatalogueState.Cancelled => throw new InvalidOperationException(
                "The Framework integration recovery catalogue was cancelled."),
            _ => throw new ArgumentOutOfRangeException(
                null,
                result.State,
                "The recovery catalogue state is not defined."),
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DeleteRecoveryArtifacts();
        _lockStore.Dispose();
        DeleteFixtureEntries();
        _temporary.Dispose();
        _disposed = true;
    }

    private void DeleteFixtureEntries()
    {
        var marker = Path.Combine(_temporary.Path, OwnershipMarkerName);
        foreach (var path in Directory.EnumerateFileSystemEntries(_temporary.Path))
        {
            if (string.Equals(path, marker, StringComparison.Ordinal))
            {
                continue;
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0)
            {
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    Directory.Delete(path);
                }
                else
                {
                    File.Delete(path);
                }
            }
            else if ((attributes & FileAttributes.Directory) != 0)
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                File.Delete(path);
            }
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
                    "The Framework integration recovery directory contains an unrecognized artifact.");
            }

            File.Delete(path);
        }

        Directory.Delete(directory);
    }
}
