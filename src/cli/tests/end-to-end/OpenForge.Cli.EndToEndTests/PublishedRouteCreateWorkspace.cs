using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

internal sealed class PublishedRouteCreateWorkspace : IDisposable
{
    internal const string ParentId = "memory/project-alpha";
    internal const string ParentPath = ".agents/memory/project-alpha/_project-alpha.md";
    internal const string TargetId = "memory/project-alpha/overview";
    internal const string TargetPath = ".agents/memory/project-alpha/overview.md";
    internal const string Description = "Project overview";
    internal const string Responsibility = "Explains the project";
    internal const string InitialEntry = "- none - No entries - #Empty";
    internal const string ExpectedEntry = "- [Project overview](overview.md) - #Docs #Overview";
    internal const string ExpectedTargetDocument = "---\n"
        + "open-forge:\n"
        + "  description: Project overview\n"
        + "  tags: [Docs, Overview]\n"
        + "  responsibility: Explains the project\n"
        + "---\n";

    private readonly TemporaryWorkspace _temporary;
    private readonly PublishedWorkspaceLockStore _lockStore;
    private string? _applicationCreatedTargetPath;
    private bool _disposed;

    private PublishedRouteCreateWorkspace(
        TemporaryWorkspace temporary,
        PublishedWorkspaceLockStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        _ = _lockStore.Track(temporary.Path);
    }

    internal string Path => _temporary.Path;

    internal IReadOnlyDictionary<string, string> ProcessEnvironment =>
        _lockStore.EnvironmentVariables;

    internal string InitialParentDocument => ParentDocument(InitialEntry);

    internal string ExpectedParentDocument => ParentDocument(ExpectedEntry);

    internal static string ExpectedTargetHash => Hash(ExpectedTargetDocument);

    internal string InitialParentHash => Hash(InitialParentDocument);

    internal string ExpectedParentHash => Hash(ExpectedParentDocument);

    internal static PublishedRouteCreateWorkspace Create()
    {
        var temporary = TemporaryWorkspace.Create("e2e-route-create");
        var lockStore = PublishedWorkspaceLockStore.Create(
            "e2e-route-create-lock-store");
        try
        {
            temporary.WriteText(
                ".agents/loader.md",
                GeneratedLoaderDocumentBuilder.Build(
                    "- [Project Alpha](memory/project-alpha/_project-alpha.md) - #Project"));
            temporary.WriteText(ParentPath, ParentDocument(InitialEntry));
            return new PublishedRouteCreateWorkspace(temporary, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            temporary.Dispose();
            throw;
        }
    }

    internal string Combine(params string[] segments) => _temporary.Combine(segments);

    internal IReadOnlyDictionary<string, string> SnapshotState() =>
        _temporary.SnapshotHashes();

    internal Task<string> ReadTargetAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(Combine(TargetPath), Encoding.UTF8, cancellationToken);

    internal Task<byte[]> ReadTargetBytesAsync(CancellationToken cancellationToken)
        => File.ReadAllBytesAsync(Combine(TargetPath), cancellationToken);

    internal Task<string> ReadParentAsync(CancellationToken cancellationToken)
        => File.ReadAllTextAsync(Combine(ParentPath), Encoding.UTF8, cancellationToken);

    internal Task<byte[]> ReadParentBytesAsync(CancellationToken cancellationToken)
        => File.ReadAllBytesAsync(Combine(ParentPath), cancellationToken);

    internal void AssertOrdinaryTargetFile()
    {
        var attributes = File.GetAttributes(Combine(TargetPath));
        Assert.Equal(
            (FileAttributes)0,
            attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
    }

    internal void OwnApplicationCreatedTarget()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_applicationCreatedTargetPath is not null)
        {
            throw new InvalidOperationException(
                "The published Route Create target is already positively owned for cleanup.");
        }

        var path = Combine(TargetPath);
        if (AttributesIfPresent(path) is not null)
        {
            throw new InvalidOperationException(
                "The published Route Create target must be absent before the production run.");
        }

        _applicationCreatedTargetPath = path;
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

        try
        {
            DeleteApplicationCreatedTarget();
        }
        finally
        {
            try
            {
                _lockStore.Dispose();
            }
            finally
            {
                _temporary.Dispose();
                _disposed = true;
            }
        }
    }

    private static string ParentDocument(string entry)
        => OpenForgeDocumentSeed.Metadata(
            description: "Project Alpha",
            tags: ["Project"],
            body: $"\n{OpenForgeDocumentSeed.GeneratedEntries(entry)}");

    private static string Hash(string contents)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(contents)));

    private void DeleteApplicationCreatedTarget()
    {
        if (_applicationCreatedTargetPath is not { } path)
        {
            return;
        }

        var attributes = AttributesIfPresent(path);
        if (attributes is null)
        {
            return;
        }

        if ((attributes.Value & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException(
                "The published Route Create cleanup target is not an ordinary file.");
        }

        File.Delete(path);
    }

    private static FileAttributes? AttributesIfPresent(string path)
    {
        try
        {
            return File.GetAttributes(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
    }
}
