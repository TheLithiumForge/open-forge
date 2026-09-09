using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;

internal sealed class LibraryInventoryCollector
{
    private readonly LibrarySourceRootObservation _source;
    private readonly string _root;
    private readonly List<EligibleSourceFile> _entries = [];
    private readonly List<LibraryInventoryExclusion> _excluded = [];
    private readonly List<LibraryInventoryUnavailablePath> _unavailable = [];

    internal LibraryInventoryCollector(LibrarySourceRootObservation source)
    {
        _source = source;
        _root = source.PhysicalSourceRoot ?? throw new ArgumentException("Inventory requires an available physical source root.", nameof(source));
    }

    internal LibraryInventoryRead Read(CancellationToken cancellationToken)
    {
        ObserveDirectory(_root, cancellationToken);
        _entries.Sort((left, right) => string.CompareOrdinal(left.SourcePath.Value, right.SourcePath.Value));
        _excluded.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
        _unavailable.Sort((left, right) => string.CompareOrdinal(left.Path, right.Path));
        var inventory = _unavailable.Count == 0
            ? LibraryInventory.Complete(_source.Request.SourceRoot, _root, _entries)
            : LibraryInventory.Classified(_source.Request.SourceRoot, _root, LibrarySourceRootState.Available,
                LibraryInventoryState.Incomplete, "One or more eligible Library source paths were unavailable.", _entries);
        return new()
        {
            Source = _source,
            Inventory = inventory,
            ExcludedPaths = [.. _excluded],
            UnavailablePaths = [.. _unavailable],
        };
    }

    private void ObserveDirectory(string directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var path = Relative(directory);
        if (LibraryEligiblePathPolicy.IsGitMetadata(_source.Request.SourceRoot, path))
        {
            Exclude(path, LibraryInventoryExclusionKind.ManagerControl);
            return;
        }
        if (!AdmitDirectory(directory))
        {
            return;
        }
        string[] children;
        try
        {
            children = Directory.GetFileSystemEntries(directory);
            Array.Sort(children, StringComparer.Ordinal);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            Unavailable(path, "The Library source directory could not be enumerated.");
            return;
        }
        if (!AdmitDirectory(directory))
        {
            return;
        }
        foreach (var child in children)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ObserveChild(child, cancellationToken);
        }
    }

    private bool AdmitDirectory(string directory)
    {
        var observation = LinkTargetReader.Read(directory);
        var path = Relative(directory);
        if (!AdmitComponent(path, observation))
        {
            return false;
        }
        if (observation.Attributes is { } attributes && (attributes & FileAttributes.Directory) != 0)
        {
            return true;
        }
        Unavailable(path, "The observed Library directory became a non-directory.");
        return false;
    }

    private void ObserveChild(string child, CancellationToken cancellationToken)
    {
        var path = Relative(child);
        var component = LinkTargetReader.Read(child);
        if (!AdmitComponent(path, component))
        {
            return;
        }
        var attributes = component.Attributes ?? throw new InvalidOperationException("An ordinary source component requires attributes.");
        if ((attributes & FileAttributes.Directory) != 0)
        {
            ObserveDirectory(child, cancellationToken);
            return;
        }
        if ((attributes & FileAttributes.Device) != 0)
        {
            Exclude(path, LibraryInventoryExclusionKind.Special);
            return;
        }
        if (!PortableWorkspacePath.TryNormalize(path, out var normalized) || normalized != path)
        {
            Unavailable(path, "The Library source filename is not a canonical portable path.");
            return;
        }
        var sourcePath = SourceRelativeEligiblePath.Create(path);
        if (LibraryEligiblePathPolicy.TryClassifyExclusion(_source.Request.SourceRoot, sourcePath, out var kind))
        {
            Exclude(path, kind);
            return;
        }
        try
        {
            using var handle = File.OpenHandle(child, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            if (handle.IsInvalid || handle.IsClosed)
            {
                Unavailable(path, "The eligible Library source file could not be opened for observation.");
                return;
            }
        }
        catch (UnauthorizedAccessException)
        {
            Unavailable(path, "The eligible Library source file is inaccessible.");
            return;
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            Unavailable(path, "The eligible Library source file disappeared during observation.");
            return;
        }
        catch (IOException)
        {
            Exclude(path, LibraryInventoryExclusionKind.Special);
            return;
        }
        var confirmed = LinkTargetReader.Read(child);
        if (confirmed.State != PathComponentState.Ordinary || confirmed.Attributes != component.Attributes)
        {
            Unavailable(path, "The eligible Library source file changed during observation.");
            return;
        }
        _entries.Add(EligibleSourceFile.Create(sourcePath, child));
    }

    private bool AdmitComponent(string path, PathComponent component)
    {
        switch (component.State)
        {
            case PathComponentState.Ordinary:
                return true;
            case PathComponentState.Link:
                Exclude(path, LibraryInventoryExclusionKind.Link);
                return false;
            case PathComponentState.Unsupported:
                Exclude(path, LibraryInventoryExclusionKind.ReparsePoint);
                return false;
            case PathComponentState.Inaccessible:
            case PathComponentState.InputOutputFailure:
            case PathComponentState.Missing:
                Unavailable(path, component.Failure?.DirectCause ?? "The Library source component could not be observed.");
                return false;
            default:
                throw new ArgumentOutOfRangeException(nameof(component), component.State, "The path component state is not defined.");
        }
    }

    private string Relative(string path) => Path.GetRelativePath(_root, path).Replace(Path.DirectorySeparatorChar, '/');
    private void Exclude(string path, LibraryInventoryExclusionKind kind) => _excluded.Add(new() { Path = path, Kind = kind });
    private void Unavailable(string path, string cause) => _unavailable.Add(new() { Path = path, Cause = cause });
}
