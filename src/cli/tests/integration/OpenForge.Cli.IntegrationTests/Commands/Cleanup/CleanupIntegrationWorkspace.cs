using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Serialization;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Cleanup;

internal sealed class CleanupIntegrationWorkspace : IDisposable
{
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;
    private readonly List<string> _recoveryPaths = [];
    private bool _disposed;

    private CleanupIntegrationWorkspace(
        TemporaryWorkspace temporary,
        WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        LockPath = _lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal string Path => _temporary.Path;

    internal string RecoveryDirectoryPath
    {
        get
        {
            var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
                Environment.SpecialFolderOption.None)
                ?? throw new InvalidOperationException(
                    "LocalApplicationData must remain observable for Cleanup integration evidence.");
            return RecoveryBundlePathIdentity.WorkspaceDirectory(
                storeRoot,
                Workspace.PhysicalRoot);
        }
    }

    internal WorkspaceLockStoreRoot LockStoreRoot => _lockStore.StoreRoot;

    internal string LockPath { get; }

    internal bool LockInfrastructureExists => _lockStore.InfrastructureExists;

    internal static CleanupIntegrationWorkspace Create(
        string purpose,
        bool withEntry = true)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        try
        {
            var workspace = new CleanupIntegrationWorkspace(temporary, lockStore);
            if (withEntry)
            {
                workspace.WriteText("AGENTS.md", "# Cleanup workspace\n");
            }

            return workspace;
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal void WriteText(string relativePath, string contents)
        => _temporary.WriteText(relativePath, contents);

    internal void WriteBytes(string relativePath, byte[] contents)
        => _temporary.WriteBytes(relativePath, contents);

    internal string Combine(string relativePath)
        => _temporary.Combine(relativePath);

    internal string CreateDirectory(string relativePath)
        => _temporary.CreateDirectory(relativePath);

    internal IReadOnlyDictionary<string, string> SnapshotWorkspace()
        => _temporary.SnapshotHashes();

    internal IReadOnlyDictionary<string, string> SnapshotRecovery()
    {
        var directory = RecoveryDirectoryPath;
        FileAttributes directoryAttributes;
        try
        {
            directoryAttributes = File.GetAttributes(directory);
        }
        catch (FileNotFoundException)
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }
        catch (DirectoryNotFoundException)
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        if ((directoryAttributes & FileAttributes.ReparsePoint) != 0
            || (directoryAttributes & FileAttributes.Directory) == 0)
        {
            return new SortedDictionary<string, string>(StringComparer.Ordinal);
        }

        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var path in Directory.EnumerateFileSystemEntries(
                     directory,
                     "*",
                     SearchOption.TopDirectoryOnly)
                     .Order(StringComparer.Ordinal))
        {
            CaptureRecoveryEntry(directory, path, snapshot);
        }

        return snapshot;
    }

    internal byte[] SnapshotLockBytes()
        => File.Exists(LockPath) ? File.ReadAllBytes(LockPath) : [];

    internal FileStream HoldLock()
        => _lockStore.OpenExclusive(Workspace);

    internal async Task<CleanupIntegrationRun> RunAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "cleanup-integration"),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = LockStoreRoot,
            });
        var completion = await application.RunAsync(
            [.. arguments],
            new CliProcessEnvironment(Path),
            new CliOutputWriters(standardOutput, standardError),
            cancellationToken);
        return new CleanupIntegrationRun(
            completion.Status,
            completion.ExitCode,
            completion.PrimaryOutputTarget,
            standardOutput.ToString(),
            standardError.ToString());
    }

    internal async Task<string> AddVerifiedFinalAsync(
        RecoveryBundleProducer producer,
        RecoveryBundleOperation operation,
        string command,
        Guid? operationId = null)
    {
        var id = operationId ?? Guid.NewGuid();
        var relativeTarget = $"cleanup-target-{id:N}.bin";
        var targetPath = _temporary.CreateFile(relativeTarget, "cleanup prior bytes\n");
        var priorBytes = await File.ReadAllBytesAsync(
            targetPath,
            TestContext.Current.CancellationToken);
        var prior = FileStateSnapshot.File(targetPath, targetPath, priorBytes);
        var input = RecoveryBundleInput.Create(
            Workspace,
            command,
            RecoveryBundleAttribution.Create(producer, operation, Workspace),
            id,
            [RecoveryBundleTarget.Create(
                PlannedFileChange.Delete(prior.Expectation),
                prior)]);
        var prepared = await RecoveryBundleStoreIntegrationTests.Store().PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        if (prepared.State != RecoveryBundlePreparationState.Prepared
            || prepared.Preparation is not { } preparation)
        {
            throw new InvalidOperationException(
                $"The Cleanup fixture could not prepare a verified final: {prepared.State}.");
        }

        _recoveryPaths.Add(preparation.BundlePath);
        return preparation.BundlePath;
    }

    internal string AddDraft(Guid? operationId = null)
    {
        var path = CandidatePath(
            operationId ?? Guid.NewGuid(),
            RecoveryBundleCandidateKind.Draft);
        WriteRecoveryFile(path, [0x01, 0x02, 0x03]);
        return path;
    }

    internal string AddMalformedFinal(Guid? operationId = null)
    {
        var path = CandidatePath(
            operationId ?? Guid.NewGuid(),
            RecoveryBundleCandidateKind.Final);
        WriteRecoveryFile(path, "not a recovery zip"u8.ToArray());
        return path;
    }

    internal async Task<string> AddUnsupportedFinalAsync(Guid? operationId = null)
    {
        var id = operationId ?? Guid.NewGuid();
        var path = CandidatePath(id, RecoveryBundleCandidateKind.Final);
        Directory.CreateDirectory(RecoveryDirectoryPath);
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
            OperationId = id.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = WorkspaceIdentity.NormalizePhysicalPath(Workspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(Workspace.PhysicalRoot),
            Attribution = RecoveryBundleAttributionCodec.Serialize(
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    Workspace)),
            Entries = [],
        };
        await stream.WriteAsync(
            RecoveryBundleManifestCodec.Serialize(document),
            TestContext.Current.CancellationToken);
        _recoveryPaths.Add(path);
        return path;
    }

    internal string AddUnavailableFinal(Guid? operationId = null)
        => AddUnavailableCandidate(operationId ?? Guid.NewGuid(), RecoveryBundleCandidateKind.Final);

    internal string AddUnsafeDraft(Guid? operationId = null)
        => AddUnavailableCandidate(operationId ?? Guid.NewGuid(), RecoveryBundleCandidateKind.Draft);

    internal string AddUnknown(string fileName = "keep-me.zip")
    {
        var path = System.IO.Path.Combine(RecoveryDirectoryPath, fileName);
        WriteRecoveryFile(path, "unknown recovery material"u8.ToArray());
        return path;
    }

    internal string AddUnknownSupportFile(
        string directoryName,
        string nestedDirectoryName,
        string fileName,
        byte[] bytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryName);
        ArgumentException.ThrowIfNullOrWhiteSpace(nestedDirectoryName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(bytes);
        var supportDirectory = System.IO.Path.Combine(
            RecoveryDirectoryPath,
            directoryName);
        var nestedDirectory = System.IO.Path.Combine(
            supportDirectory,
            nestedDirectoryName);
        Directory.CreateDirectory(nestedDirectory);
        _recoveryPaths.Add(supportDirectory);
        _recoveryPaths.Add(nestedDirectory);
        var path = System.IO.Path.Combine(nestedDirectory, fileName);
        WriteRecoveryFile(path, bytes);
        return path;
    }

    internal string AddMissingAttributionFinal(Guid? operationId = null)
        => AddSchemaV1Final(
            operationId ?? Guid.NewGuid(),
            "missing attribution payload\n"u8.ToArray(),
            rewriteManifest: RemoveAttribution);

    internal string AddInvalidAttributionFinal(Guid? operationId = null)
        => AddSchemaV1Final(
            operationId ?? Guid.NewGuid(),
            "invalid attribution payload\n"u8.ToArray(),
            rewriteManifest: InvalidateAttribution);

    internal string AddPayloadLengthMismatchFinal(Guid? operationId = null)
    {
        var payload = "payload length mismatch\n"u8.ToArray();
        return AddSchemaV1Final(
            operationId ?? Guid.NewGuid(),
            payload,
            declaredLength: payload.Length + 1);
    }

    internal string AddPayloadHashMismatchFinal(Guid? operationId = null)
    {
        var payload = "payload hash mismatch\n"u8.ToArray();
        var declaredHash = Convert.ToHexStringLower(
            SHA256.HashData("different payload bytes\n"u8.ToArray()));
        return AddSchemaV1Final(
            operationId ?? Guid.NewGuid(),
            payload,
            declaredHash: declaredHash);
    }

    internal string BlockRecoveryDirectory()
    {
        var directory = RecoveryDirectoryPath;
        var parent = System.IO.Path.GetDirectoryName(directory)
            ?? throw new InvalidOperationException(
                "A Cleanup recovery bucket requires an observable parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(directory, "recovery bucket collision"u8.ToArray());
        _recoveryPaths.Add(directory);
        return directory;
    }

    internal string AddAlias(string fileName, string targetPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);
        Directory.CreateDirectory(RecoveryDirectoryPath);
        var path = System.IO.Path.Combine(RecoveryDirectoryPath, fileName);
        File.CreateSymbolicLink(path, targetPath);
        _recoveryPaths.Add(path);
        return path;
    }

    internal string AddMismatchedFinal(Guid? operationId = null)
    {
        var id = operationId ?? Guid.NewGuid();
        var path = CandidatePath(id, RecoveryBundleCandidateKind.Final);
        var foreignWorkspacePath = System.IO.Path.Combine(
            System.IO.Path.GetDirectoryName(Path)
                ?? throw new InvalidOperationException(
                    "A mismatched Cleanup fixture requires a containing directory."),
            $"cleanup-foreign-{id:N}");
        var foreignWorkspace = new CliWorkspace(
            lexicalRoot: foreignWorkspacePath,
            physicalRoot: foreignWorkspacePath,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var priorBytes = "mismatched prior bytes\n"u8.ToArray();
        var manifest = new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion,
            Command = "index",
            OperationId = id.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = WorkspaceIdentity.NormalizePhysicalPath(foreignWorkspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(foreignWorkspace.PhysicalRoot),
            Attribution = RecoveryBundleAttributionCodec.Serialize(
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    foreignWorkspace)),
            Entries =
            [
                new RecoveryBundleManifestEntryV1
                {
                    Ordinal = 0,
                    Target = "mismatched-target.bin",
                    Kind = "delete",
                    PriorLength = priorBytes.Length,
                    PriorSha256 = Convert.ToHexStringLower(SHA256.HashData(priorBytes)),
                    Payload = RecoveryBundleFormatV1.PayloadName(0),
                    IntendedAbsent = true,
                    IntendedLength = null,
                    IntendedSha256 = null,
                },
            ],
        };
        Directory.CreateDirectory(RecoveryDirectoryPath);
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: false);
        var manifestEntry = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
        using (var manifestStream = manifestEntry.Open())
        {
            var manifestBytes = RecoveryBundleManifestCodec.Serialize(manifest);
            manifestStream.Write(manifestBytes);
        }

        var payloadEntry = archive.CreateEntry(RecoveryBundleFormatV1.PayloadName(0));
        using var payloadStream = payloadEntry.Open();
        payloadStream.Write(priorBytes);
        _recoveryPaths.Add(path);
        return path;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var path in _recoveryPaths
                     .OrderByDescending(path => path.Length)
                     .Distinct(StringComparer.Ordinal))
        {
            DeleteExact(path);
        }

        DeleteEmptyDirectory(RecoveryDirectoryPath);
        _lockStore.Dispose();
        _temporary.Dispose();
        _disposed = true;
    }

    private string CandidatePath(
        Guid operationId,
        RecoveryBundleCandidateKind kind)
        => System.IO.Path.Combine(
            RecoveryDirectoryPath,
            kind == RecoveryBundleCandidateKind.Final
                ? RecoveryBundleFormatV1.FinalFileName(operationId)
                : RecoveryBundleFormatV1.DraftFileName(operationId));

    private string AddUnavailableCandidate(
        Guid operationId,
        RecoveryBundleCandidateKind kind)
    {
        var path = CandidatePath(operationId, kind);
        Directory.CreateDirectory(path);
        _recoveryPaths.Add(path);
        return path;
    }

    private string AddSchemaV1Final(
        Guid operationId,
        byte[] payload,
        Func<byte[], byte[]>? rewriteManifest = null,
        long? declaredLength = null,
        string? declaredHash = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var path = CandidatePath(operationId, RecoveryBundleCandidateKind.Final);
        var payloadHash = Convert.ToHexStringLower(SHA256.HashData(payload));
        var document = new RecoveryBundleManifestV1
        {
            SchemaVersion = RecoveryBundleFormatV1.SchemaVersion,
            Command = "index",
            OperationId = operationId.ToString(RecoveryBundleFormatV1.OperationIdFormat),
            WorkspacePath = WorkspaceIdentity.NormalizePhysicalPath(Workspace.PhysicalRoot),
            WorkspaceKey = WorkspaceIdentity.Key(Workspace.PhysicalRoot),
            Attribution = RecoveryBundleAttributionCodec.Serialize(
                RecoveryBundleAttribution.Create(
                    RecoveryBundleProducer.Index,
                    RecoveryBundleOperation.Index,
                    Workspace)),
            Entries =
            [
                new RecoveryBundleManifestEntryV1
                {
                    Ordinal = 0,
                    Target = $"cleanup-variant-target-{operationId:N}.bin",
                    Kind = "delete",
                    PriorLength = declaredLength ?? payload.Length,
                    PriorSha256 = declaredHash ?? payloadHash,
                    Payload = RecoveryBundleFormatV1.PayloadName(0),
                    IntendedAbsent = true,
                    IntendedLength = null,
                    IntendedSha256 = null,
                },
            ],
        };
        var manifestBytes = RecoveryBundleManifestCodec.Serialize(document);
        if (rewriteManifest is not null)
        {
            manifestBytes = rewriteManifest(manifestBytes);
        }

        WriteRecoveryArchive(path, manifestBytes, payload);
        return path;
    }

    private void WriteRecoveryArchive(
        string path,
        byte[] manifestBytes,
        byte[] payload)
    {
        Directory.CreateDirectory(RecoveryDirectoryPath);
        using var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var archive = new ZipArchive(file, ZipArchiveMode.Create, leaveOpen: false);
        var manifestEntry = archive.CreateEntry(RecoveryBundleFormatV1.ManifestEntryName);
        using (var manifestStream = manifestEntry.Open())
        {
            manifestStream.Write(manifestBytes);
        }

        var payloadEntry = archive.CreateEntry(RecoveryBundleFormatV1.PayloadName(0));
        using var payloadStream = payloadEntry.Open();
        payloadStream.Write(payload);
        _recoveryPaths.Add(path);
    }

    private static byte[] RemoveAttribution(byte[] manifestBytes)
    {
        var document = ParseManifestObject(manifestBytes);
        document.Remove("attribution");
        return StrictUtf8NoBom.GetBytes(document.ToJsonString());
    }

    private static byte[] InvalidateAttribution(byte[] manifestBytes)
    {
        var document = ParseManifestObject(manifestBytes);
        var attribution = document["attribution"]?.AsObject()
            ?? throw new InvalidOperationException(
                "The invalid-attribution fixture requires a serialized attribution object.");
        attribution["producer"] = "not-a-producer";
        return StrictUtf8NoBom.GetBytes(document.ToJsonString());
    }

    private static JsonObject ParseManifestObject(byte[] manifestBytes)
        => JsonNode.Parse(manifestBytes)?.AsObject()
            ?? throw new InvalidOperationException(
                "The Cleanup fixture manifest must serialize as a JSON object.");

    private static void CaptureRecoveryEntry(
        string root,
        string path,
        SortedDictionary<string, string> snapshot)
    {
        var attributes = File.GetAttributes(path);
        var relative = System.IO.Path.GetRelativePath(root, path).Replace('\\', '/');
        if ((attributes & FileAttributes.ReparsePoint) != 0)
        {
            snapshot[relative] = "link";
            return;
        }

        if ((attributes & FileAttributes.Directory) != 0)
        {
            snapshot[relative] = "directory";
            foreach (var child in Directory.EnumerateFileSystemEntries(
                         path,
                         "*",
                         SearchOption.TopDirectoryOnly)
                         .Order(StringComparer.Ordinal))
            {
                CaptureRecoveryEntry(root, child, snapshot);
            }

            return;
        }

        snapshot[relative] = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    }

    private void WriteRecoveryFile(string path, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var parent = System.IO.Path.GetDirectoryName(path)
            ?? throw new InvalidOperationException(
                "A Cleanup recovery fixture path requires a parent directory.");
        Directory.CreateDirectory(parent);
        File.WriteAllBytes(path, bytes);
        _recoveryPaths.Add(path);
    }

    private static void DeleteExact(string path)
    {
        try
        {
            var attributes = File.GetAttributes(path);
            if ((attributes & FileAttributes.ReparsePoint) != 0
                || (attributes & FileAttributes.Directory) == 0)
            {
                File.Delete(path);
            }
            else
            {
                Directory.Delete(path, recursive: false);
            }
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
    }

    private static void DeleteEmptyDirectory(string path)
    {
        if (Directory.Exists(path)
            && !Directory.EnumerateFileSystemEntries(path).Any())
        {
            Directory.Delete(path, recursive: false);
        }
    }
}

internal sealed record CleanupIntegrationRun(
    CliSemanticStatus Status,
    int ExitCode,
    CliOutputTarget PrimaryOutputTarget,
    string StandardOutput,
    string StandardError)
{
    internal JsonDocument ParseJson()
        => JsonDocument.Parse(StandardOutput);
}
