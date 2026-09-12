using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Libraries.Operational.Shared.Reading;

internal sealed class LibraryObservationWorkspace : IDisposable
{
    internal const string RecordPath = ".agents/open-forge.libraries.json";
    internal const string SingleRecord = """
        {"schemaVersion":1,"libraries":[{"id":"team","sourceRoot":"shared/team","destinationRoot":".","paths":[".agents/a.md"]}]}
        """;
    private readonly List<string> _externalRecoveryFiles = [];
    private string? _externalRecoveryDirectory;

    internal LibraryObservationWorkspace()
    {
        Files = TemporaryWorkspace.Create("library-producer-read");
        var root = Files.CreateDirectory("workspace");
        Workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Write(".agents/loader.md", "# Local loader\n");
        Write("AGENTS.md", "# Local entry\n");
        Write("sibling.txt", "consumer sibling\n");
    }

    internal TemporaryWorkspace Files { get; }
    internal CliWorkspace Workspace { get; }
    internal string PathFor(string path) => Files.Combine($"workspace/{path}");
    internal string Write(string path, string text) => Files.CreateFile($"workspace/{path}", text);

    internal void Source(string id)
        => Write($"shared/{id}/.agents/{id}.md", $"# Source {id}\n");

    internal string CreateRecoveryArchive(Guid operationId)
    {
        var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify)
            ?? throw new InvalidOperationException("The fixture requires the current-user recovery store.");
        var directory = RecoveryBundlePathIdentity.WorkspaceDirectory(storeRoot, Workspace.PhysicalRoot);
        if (_externalRecoveryDirectory is null)
        {
            Assert.False(Directory.Exists(directory));
            Directory.CreateDirectory(directory);
            _externalRecoveryDirectory = directory;
        }
        var path = RecoveryBundlePathIdentity.FinalPath(storeRoot, Workspace.PhysicalRoot, operationId);
        using (File.Open(path, FileMode.CreateNew, FileAccess.Write))
        {
            _externalRecoveryFiles.Add(path);
        }
        return path;
    }

    internal void TrustedLifecycle()
    {
        Write(".agents/framework-owned.md", "# Framework-owned file\n");
        Write(".agents/open-forge.lifecycle.json", $$$"""
            {"schemaVersion":1,"fingerprintPolicy":"open-forge-markdown-v1","workspacePath":"{{{JsonEncodedText.Encode(Workspace.PhysicalRoot)}}}",
             "framework":{"coverage":"complete","source":{"id":"open-forge","version":"1.0.0",
               "inventoryFingerprint":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"},
               "targets":[{"path":".agents/framework-owned.md","sourceAssetPath":".agents/framework-owned.md","region":null,
                 "baselineFingerprint":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","fingerprintKind":"exact-bytes"}],"generatedRegions":[]},
             "extensions":{"coverage":"complete","packages":[],"paths":[]}}
            """);
    }

    internal void ThreeLibraries(string? missingSource = null)
    {
        Write(RecordPath, """
            {"schemaVersion":1,"libraries":[
              {"id":"alpha","sourceRoot":"shared/alpha","destinationRoot":".","paths":[".agents/alpha.md"]},
              {"id":"beta","sourceRoot":"shared/beta","destinationRoot":".","paths":[".agents/beta.md"]},
              {"id":"gamma","sourceRoot":"shared/gamma","destinationRoot":".","paths":[".agents/gamma.md"]}
            ]}
            """);
        foreach (var id in new[] { "alpha", "beta", "gamma" })
        {
            if (id != missingSource)
            {
                Source(id);
            }
        }
        Source("unregistered");
    }

    internal LibraryDoctorView DoctorView(bool currentRecord)
    {
        var path = PathFor(RecordPath);
        return new LibraryDoctorView
        {
            State = OperationalViewState.Complete,
            Ownership = null,
            LinkCapability = null,
            Record = new LibrariesRecordRead
            {
                State = currentRecord ? LibrariesRecordReadState.Complete : LibrariesRecordReadState.Missing,
                Record = currentRecord ? TeamRecord() : null,
                Snapshot = currentRecord
                    ? FileStateSnapshot.File(path, path, Encoding.UTF8.GetBytes(SingleRecord))
                    : FileStateSnapshot.Missing(path),
                Cause = null,
            },
            Inventories = [],
            Mappings = [],
        };
    }

    internal static LibrariesRecord TeamRecord()
        => LibrariesRecord.Create([
            LibraryRecord.Create(LibraryId.Create("team"), WorkspaceRelativeDirectory.Create("shared/team"), LibraryDestinationRoot.Create("."),
                [SourceRelativeEligiblePath.Create(".agents/a.md")]),
        ]);

    internal SortedDictionary<string, string> Snapshot()
    {
        var entries = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in Files.SnapshotHashes())
        {
            entries.Add(pair.Key, pair.Value);
        }
        Capture(new DirectoryInfo(Files.Path), entries);
        var lockRoot = WorkspaceLockStoreRoot.ResolveForCurrentUser(Environment.SpecialFolderOption.DoNotVerify);
        var recoveryRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.DoNotVerify);
        Assert.NotNull(lockRoot);
        Assert.NotNull(recoveryRoot);
        Assert.False(File.Exists(WorkspaceLockPathIdentity.LockPath(lockRoot, Workspace)));
        if (_externalRecoveryDirectory is null)
        {
            Assert.False(Directory.Exists(RecoveryBundlePathIdentity.WorkspaceDirectory(recoveryRoot, Workspace.PhysicalRoot)));
        }
        else
        {
            Assert.True(Directory.Exists(_externalRecoveryDirectory));
            Assert.Equal(_externalRecoveryFiles.Order(StringComparer.Ordinal),
                Directory.EnumerateFileSystemEntries(_externalRecoveryDirectory).Order(StringComparer.Ordinal));
            foreach (var path in _externalRecoveryFiles)
            {
                Assert.Null(new FileInfo(path).LinkTarget);
                entries.Add($"recovery/{Path.GetFileName(path)}", Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))));
            }
        }
        return entries;
    }

    private void Capture(DirectoryInfo directory, SortedDictionary<string, string> entries)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            var path = Path.GetRelativePath(Files.Path, entry.FullName).Replace('\\', '/');
            if (entry.LinkTarget is { } target)
            {
                entries.Add(path, $"link:{target}");
            }
            else if (entry is DirectoryInfo child)
            {
                entries.Add($"{path}/", "directory");
                Capture(child, entries);
            }
        }
    }

    public void Dispose()
    {
        try
        {
            foreach (var path in _externalRecoveryFiles)
            {
                File.Delete(path);
            }
            if (_externalRecoveryDirectory is { } directory && Directory.Exists(directory)
                && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
        finally
        {
            Files.Dispose();
        }
    }
}
