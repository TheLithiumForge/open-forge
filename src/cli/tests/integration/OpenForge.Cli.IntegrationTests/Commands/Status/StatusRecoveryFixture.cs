using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Serialization.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed class StatusRecoveryFixture : IDisposable
{
    private readonly StatusIntegrationWorkspace _workspace;
    private readonly string _workspaceDirectory;
    private readonly List<string> _ownedPaths = [];
    private bool _disposed;

    private StatusRecoveryFixture(StatusIntegrationWorkspace workspace)
    {
        _workspace = workspace;
        _workspaceDirectory = workspace.RecoveryDirectory();
    }

    internal string WorkspaceDirectory => _workspaceDirectory;

    internal static StatusRecoveryFixture Create(StatusIntegrationWorkspace workspace)
        => new(workspace);

    internal async Task<string> AddVerifiedFinalAsync()
    {
        var operationId = Guid.NewGuid();
        var relativePath = $"status-recovery-target-{operationId:N}.bin";
        var priorBytes = "prior recovery bytes\n"u8.ToArray();
        var targetPath = _workspace.Combine(relativePath);
        _workspace.WriteBytes(relativePath, priorBytes);
        var prior = FileStateSnapshot.File(targetPath, targetPath, priorBytes);
        var input = RecoveryBundleInput.Create(
            _workspace.Workspace,
            command: "status integration",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Index,
                RecoveryBundleOperation.Index,
                _workspace.Workspace),
            operationId,
            targets:
            [
                RecoveryBundleTarget.Create(
                    PlannedFileChange.Delete(prior.Expectation),
                    prior),
            ]);
        var preparation = await RecoveryBundleStore.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        if (preparation.State != RecoveryBundlePreparationState.Prepared
            || preparation.Preparation is not { } prepared)
        {
            throw new InvalidOperationException(
                $"The Status recovery fixture could not prepare a verified final: {preparation.State}.");
        }

        _ownedPaths.Add(prepared.BundlePath);
        return prepared.BundlePath;
    }

    internal string AddDraft()
    {
        var path = StatusRecoveryCatalogue.CandidatePath(
            _workspaceDirectory,
            Guid.NewGuid(),
            RecoveryBundleCandidateKind.Draft);
        WriteExternalFile(path, [0x01, 0x02, 0x03]);
        return path;
    }

    internal string AddMalformedFinal()
    {
        var path = StatusRecoveryCatalogue.CandidatePath(
            _workspaceDirectory,
            Guid.NewGuid(),
            RecoveryBundleCandidateKind.Final);
        WriteExternalFile(path, "not a zip"u8.ToArray());
        return path;
    }

    internal async Task<string> AddUnsupportedFinalAsync()
    {
        var operationId = Guid.NewGuid();
        var path = StatusRecoveryCatalogue.CandidatePath(
            _workspaceDirectory,
            operationId,
            RecoveryBundleCandidateKind.Final);
        Directory.CreateDirectory(_workspaceDirectory);
        await using var file = new FileStream(
            path,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: true);
        var manifest = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
        await using var stream = manifest.Open();
        var document = new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion + 1,
            Command = "future",
            OperationId = operationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = WorkspaceIdentity.NormalizePhysicalPath(_workspace.Path),
            WorkspaceKey = WorkspaceIdentity.Key(_workspace.Path),
            Attribution = RecoveryBundleAttributionCodec.Serialize(
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    _workspace.Workspace)),
            Entries = [],
        };
        await stream.WriteAsync(
            RecoveryBundleManifestCodec.Serialize(document),
            TestContext.Current.CancellationToken);
        _ownedPaths.Add(path);
        return path;
    }

    internal string AddUnavailableFinal()
    {
        var path = StatusRecoveryCatalogue.CandidatePath(
            _workspaceDirectory,
            Guid.NewGuid(),
            RecoveryBundleCandidateKind.Final);
        Directory.CreateDirectory(path);
        _ownedPaths.Add(path);
        return path;
    }

    private void WriteExternalFile(string path, byte[] bytes)
    {
        var parent = Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException("The recovery fixture path requires a parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(path, bytes);
        if (path.StartsWith(_workspaceDirectory, StringComparison.Ordinal))
        {
            _ownedPaths.Add(path);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _ownedPaths
                     .OrderByDescending(path => path.Length)
                     .Distinct(StringComparer.Ordinal))
        {
            StatusRecoveryCatalogue.DeleteExact(path);
        }

        StatusRecoveryCatalogue.DeleteEmptyDirectory(_workspaceDirectory);

        _disposed = true;
    }

}
