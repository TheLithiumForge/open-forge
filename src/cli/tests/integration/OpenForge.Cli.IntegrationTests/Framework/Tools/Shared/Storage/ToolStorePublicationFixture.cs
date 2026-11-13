using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Storage;

internal sealed class ToolStorePublicationFixture : IDisposable
{
    internal const string UnitPath = "fixture/1.0.0/test-target/pin";
    internal const string Entry = "bin/fixture.bin";
    internal const string Other = "usage.txt";
    internal const string Bin = "bin";
    private const string StoreDirectory = "store";
    internal static readonly byte[] Bytes = [0, 127, 255, 13, 10];
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-store-publication");
    internal ToolStoreRegistry Store { get; }
    internal ToolStorePublisher Publisher => new(Store);
    internal string First { get; }
    internal string Second { get; }

    internal ToolStorePublicationFixture()
    {
        Store = new(ToolStoreLocator.FromRoot(Temporary.CreateDirectory(StoreDirectory)));
        var paths = new PhysicalPathResolver();
        First = paths.ResolveRoot(Temporary.CreateDirectory("first-workspace")).GetContainedPhysicalPath();
        Second = paths.ResolveRoot(Temporary.CreateDirectory("second-workspace")).GetContainedPhysicalPath();
    }

    internal async ValueTask<ToolPreparedStoreUnit> PrepareAsync(CancellationToken token, string kind = ToolStoreRegistryDefinitions.Install,
        string path = UnitPath, string[]? dependencies = null)
    {
        await Store.InitializeAsync(token);
        var area = Store.Area(kind);
        var staging = ToolStoreRegistryDefinitions.StagingDirectory + "/" + Guid.NewGuid().ToString("N");
        area.EnsureDirectory(staging + "/" + Bin, token);
        await File.WriteAllBytesAsync(area.At(staging + "/" + Entry), Bytes, token);
        await File.WriteAllBytesAsync(area.At(staging + "/" + Other), Bytes, token);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Bytes));
        return new()
        {
            Kind = kind,
            Path = path,
            RecipeSha256 = hash,
            StagingPath = staging,
            Files = [new ToolFileReceipt(null, Entry, hash, false), new ToolFileReceipt(null, Other, hash, false)],
            Directories = [Bin],
            Dependencies = dependencies ?? [],
        };
    }

    internal static ToolLocalRegistration Registration(string physicalWorkspace) => new() { PhysicalWorkspace = physicalWorkspace, CodeRoot = "." };
    internal static string Key(ToolPreparedStoreUnit prepared) => ToolStoreIdentity.UnitKey(prepared.Kind, prepared.Path);

    internal SortedDictionary<string, string> SnapshotHashes()
    {
        var result = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var pending = new Queue<DirectoryInfo>();
        pending.Enqueue(new(Temporary.Path));
        var lockPath = Store.State.At(ToolStoreRegistryDefinitions.Lock);
        while (pending.TryDequeue(out var directory))
            foreach (var entry in directory.EnumerateFileSystemInfos())
            {
                if (entry.LinkTarget is not null || (entry.Attributes & FileAttributes.ReparsePoint) != 0)
                    throw new IOException("The fixture snapshot contains a link.");
                var path = Path.GetRelativePath(Temporary.Path, entry.FullName).Replace(Path.DirectorySeparatorChar, '/');
                if (entry is DirectoryInfo child) { result.Add(path + "/", string.Empty); pending.Enqueue(child); }
                else if (!string.Equals(entry.FullName, lockPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    result.Add(path, Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(entry.FullName))));
            }
        return result;
    }

    public void Dispose()
    {
        var target = Path.GetFullPath(Temporary.Combine(StoreDirectory));
        var relative = Path.GetRelativePath(Temporary.Path, target);
        if (Path.IsPathRooted(relative) || relative.StartsWith("..", StringComparison.Ordinal)) throw new InvalidOperationException("The fixture cleanup target escaped its root.");
        if (Directory.Exists(target)) DeleteProduced(new DirectoryInfo(target));
        Temporary.Dispose();
    }

    private static void DeleteProduced(DirectoryInfo directory)
    {
        if (directory.LinkTarget is null && (directory.Attributes & FileAttributes.ReparsePoint) == 0)
            foreach (var entry in directory.EnumerateFileSystemInfos())
            {
                if (entry is DirectoryInfo child) DeleteProduced(child);
                else entry.Delete();
            }
        directory.Delete(recursive: false);
    }
}
