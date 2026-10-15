using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

internal sealed class PathComponentReadSession
{
    private readonly Func<string, PathComponent> _reader;
    private readonly Dictionary<string, PathComponent> _directories = new(PhysicalIdentityTracker.PathComparer);

    internal PathComponentReadSession(Func<string, PathComponent>? reader = null)
    {
        _reader = reader ?? LinkTargetReader.Read;
    }

    private int _observationCount;
    internal int ObservationCount => _observationCount;

    internal PathComponent Read(string path, bool isAncestor)
    {
        if (!isAncestor) return Observe(path);
        lock (_directories)
        {
            if (_directories.TryGetValue(path, out var cached)) return cached;
            return Observe(path);
        }
    }

    private PathComponent Observe(string path)
    {
        var component = _reader(path);
        Interlocked.Increment(ref _observationCount);
        // Directory mappings are stable within one read pass. File leaves and links stay fresh.
        if (component.State == PathComponentState.Ordinary
            && component.Attributes is { } attributes && (attributes & FileAttributes.Directory) != 0)
        {
            lock (_directories) _directories.TryAdd(path, component);
        }
        return component;
    }
}
