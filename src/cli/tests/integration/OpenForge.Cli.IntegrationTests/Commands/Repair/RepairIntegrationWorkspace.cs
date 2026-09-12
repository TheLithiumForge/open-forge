using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

internal sealed class RepairIntegrationWorkspace : IDisposable
{
    internal const string SourcePath = ".agents/docs/source.md";
    internal const string SafeTargetPath = ".agents/docs/guide.md";
    internal const string GuidedTargetPath = ".agents/docs/replacement.md";
    internal const string UnrelatedPath = ".agents/docs/unrelated.bin";
    internal const string GuidedExpectedDestination = "missing.md";
    internal const string SafeIntendedDestination = "guide.md";
    internal const string GuidedIntendedDestination = "replacement.md";

    private const string SourceBodyPrefix = "\n# Source\n\n";
    private const string SafeLink = "[Safe](./guide.md)";
    private const string GuidedLink = "[Guided](missing.md)";
    private const string UnrelatedLine = "Unrelated bytes stay exactly.\n";
    private static readonly byte[] UnrelatedBytes = [0, 255, 1, 128, 13, 10, 0];
    private readonly TemporaryWorkspace _temporary;
    private bool _disposed;

    private RepairIntegrationWorkspace(TemporaryWorkspace temporary)
    {
        _temporary = temporary;
        Workspace = new CliWorkspace(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
        if (LockPath is { } lockPath && PathState(lockPath) != "absent")
        {
            throw new InvalidOperationException(
                "The Repair integration lock path must be absent before execution.");
        }

        if (PathState(RecoveryDirectory) != "absent")
        {
            throw new InvalidOperationException(
                "The Repair integration recovery path must be absent before execution.");
        }
    }

    internal CliWorkspace Workspace { get; }

    internal string Path => _temporary.Path;

    internal string Combine(string relativePath)
        => _temporary.Combine(relativePath);

    internal string RecoveryDirectory
    {
        get
        {
            var storeRoot = RecoveryBundlePathIdentity.ResolveStoreRoot(
                Environment.SpecialFolderOption.None);
            return storeRoot is null
                ? _temporary.Combine(".repair-recovery-unavailable")
                : RecoveryBundlePathIdentity.WorkspaceDirectory(
                    storeRoot,
                    Workspace.PhysicalRoot);
        }
    }

    internal string? LockPath
    {
        get
        {
            var storeRoot = WorkspaceLockStoreRoot.ResolveForCurrentUser(
                Environment.SpecialFolderOption.None);
            return storeRoot is null
                ? null
                : WorkspaceLockPathIdentity.LockPath(storeRoot, Workspace);
        }
    }

    internal static RepairIntegrationWorkspace Create(
        string purpose,
        bool includeSafeExact = true,
        bool includeGuided = true)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        try
        {
            temporary.WriteText(
                "AGENTS.md",
                "# Workspace\n\nRead `.agents/loader.md`.\n");
            temporary.WriteText(
                ".agents/loader.md",
                GeneratedLoaderDocumentBuilder.Build(
                    "- [Docs](docs/index.md) - #Docs"));
            temporary.WriteText(
                ".agents/docs/index.md",
                OpenForgeDocumentSeed.Metadata(
                    description: "Docs",
                    tags: ["Docs"],
                    body: "\n# Docs\n\nRepair integration fixture.\n"));

            var source = BuildSource(includeSafeExact, includeGuided);
            temporary.WriteText(
                SourcePath,
                OpenForgeDocumentSeed.Metadata(
                    description: "Source",
                    tags: ["Docs"],
                    body: source));
            temporary.WriteText(
                SafeTargetPath,
                OpenForgeDocumentSeed.Metadata(
                    description: "Guide",
                    tags: ["Docs"],
                    body: "\n# Guide\n\nSafe exact target.\n"));
            temporary.WriteText(
                GuidedTargetPath,
                OpenForgeDocumentSeed.Metadata(
                    description: "Guided",
                    tags: ["Docs"],
                    body: "\n# Guided\n\nGuided target.\n"));
            temporary.WriteBytes(UnrelatedPath, UnrelatedBytes);
            return new RepairIntegrationWorkspace(temporary);
        }
        catch
        {
            temporary.Dispose();
            throw;
        }
    }

    internal RepairRequest Request(
        RepairMode mode = RepairMode.Apply,
        bool automatic = false,
        IEnumerable<RepairRelinkRequest>? relinks = null,
        bool allowInteraction = false)
        => new(
            Workspace,
            mode,
            automatic,
            relinks ?? [],
            allowInteraction);

    internal RepairRelinkRequest GuidedRelink(
        string expectedDestination = GuidedExpectedDestination,
        string selectedTargetPath = GuidedTargetPath)
    {
        var location = GuidedLocation();
        return new(
            SourcePath,
            location.Line,
            location.Column,
            expectedDestination,
            selectedTargetPath,
            selectedTargetFragment: null);
    }

    internal string ReadText(string relativePath)
        => File.ReadAllText(_temporary.Combine(relativePath));

    internal byte[] ReadBytes(string relativePath)
        => File.ReadAllBytes(_temporary.Combine(relativePath));

    internal IReadOnlyDictionary<string, string> SnapshotState()
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var (path, hash) in _temporary.SnapshotHashes())
        {
            state[$"workspace/{path}"] = hash;
        }

        state["workspace-lock"] = PathState(LockPath);
        state["recovery"] = PathState(RecoveryDirectory);
        return state;
    }

    internal void AssertNoWriteInfrastructure()
    {
        Assert.Equal("absent", PathState(LockPath));
        Assert.Equal("absent", PathState(RecoveryDirectory));
        Assert.False(File.Exists(_temporary.Combine(".agents/open-forge.lock")));
    }

    internal void AssertPersistentExternalLock()
    {
        var lockPath = LockPath;
        Assert.NotNull(lockPath);
        Assert.True(File.Exists(lockPath));
        Assert.Equal(0, new FileInfo(lockPath!).Length);
        Assert.False(File.Exists(_temporary.Combine(".agents/open-forge.lock")));
    }

    internal void AssertNoRecoveryArtifacts()
    {
        var state = PathState(RecoveryDirectory);
        Assert.True(state is "absent" or "directory:", state);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            DeleteExactIfPresent(LockPath);
            DeleteRecoveryArtifacts();
        }
        finally
        {
            _temporary.Dispose();
            _disposed = true;
        }
    }

    private static string BuildSource(bool includeSafeExact, bool includeGuided)
    {
        var links = new List<string>();
        if (includeSafeExact)
        {
            links.Add(SafeLink);
        }

        if (includeGuided)
        {
            links.Add(GuidedLink);
        }

        return SourceBodyPrefix
            + string.Join('\n', links)
            + (links.Count == 0 ? string.Empty : "\n")
            + UnrelatedLine;
    }

    private SourceLocation GuidedLocation()
    {
        var source = ReadText(SourcePath);
        var start = source.IndexOf(GuidedExpectedDestination, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("The Repair fixture does not contain the guided destination.");
        }

        var line = 1 + source[..start].Count(character => character == '\n');
        var lineStart = source.LastIndexOf('\n', start == 0 ? 0 : start - 1);
        lineStart = lineStart < 0 ? 0 : lineStart + 1;
        var column = start - lineStart + 1;
        var byteOffset = Encoding.UTF8.GetByteCount(source[..start]);
        var byteLength = Encoding.UTF8.GetByteCount(GuidedExpectedDestination);
        return new SourceLocation(line, column, byteOffset, byteLength);
    }

    private static string PathState(string? path)
    {
        if (path is null)
        {
            return "unavailable";
        }

        if (File.Exists(path))
        {
            return $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";
        }

        if (!Directory.Exists(path))
        {
            return "absent";
        }

        var entries = Directory
            .EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .Select(entry =>
            {
                var relative = System.IO.Path.GetRelativePath(path, entry).Replace('\\', '/');
                return File.Exists(entry)
                    ? $"file:{relative}:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(entry)))}"
                    : $"directory:{relative}";
            });
        return $"directory:{string.Join('|', entries)}";
    }

    private void DeleteRecoveryArtifacts()
    {
        var directory = RecoveryDirectory;
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (Directory.Exists(entry))
            {
                Directory.Delete(entry, recursive: true);
            }
            else
            {
                File.Delete(entry);
            }
        }

        Directory.Delete(directory);
    }

    private static void DeleteExactIfPresent(string? path)
    {
        if (path is null || !File.Exists(path))
        {
            return;
        }

        File.Delete(path);
        var parent = System.IO.Path.GetDirectoryName(path);
        if (parent is null)
        {
            return;
        }

        while (Directory.Exists(parent)
            && !Directory.EnumerateFileSystemEntries(parent).Any())
        {
            var next = System.IO.Path.GetDirectoryName(parent);
            Directory.Delete(parent);
            parent = next;
        }
    }
}
