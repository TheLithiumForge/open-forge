using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;

internal sealed class RouteInspectDirectoryEligibilityCache
{
    private static readonly StringComparer DirectoryComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;
    private readonly Dictionary<string, PhysicalPathResolution> _directories = new(DirectoryComparer);
    private readonly Dictionary<string, FileAttributes> _attributes = new(DirectoryComparer);
    private readonly PhysicalPathResolver _resolver = new();
    private readonly CancellationToken _cancellationToken;

    internal RouteInspectDirectoryEligibilityCache(PhysicalPathResolution root, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _directories.Add(Path.TrimEndingDirectorySeparator(root.LogicalPath), root);
    }

    internal void ObserveAttributes(string directory, FileAttributes attributes)
        => _attributes.TryAdd(directory, attributes);

    internal PhysicalPathResolution Read(string directory)
    {
        _cancellationToken.ThrowIfCancellationRequested();
        if (_directories.TryGetValue(directory, out var cached))
        {
            return cached;
        }

        var result = Inspect(directory);
        _directories.Add(directory, result);
        return result;
    }

    private PhysicalPathResolution Inspect(string directory)
    {
        var parent = Path.GetDirectoryName(directory)
            ?? throw new InvalidOperationException("A contained directory requires a cached ancestor.");
        var parentResolution = Read(parent);
        if (parentResolution.State != PhysicalPathState.Contained)
        {
            return parentResolution;
        }

        if (string.Equals(Path.GetFileName(directory), ".git", StringComparison.OrdinalIgnoreCase))
        {
            return PhysicalPathResolution.Classified(PhysicalPathState.Missing, directory);
        }

        try
        {
            var attributes = _attributes.TryGetValue(directory, out var observed)
                ? observed
                : File.GetAttributes(directory);
            if ((attributes & FileAttributes.ReparsePoint) != 0
                || (attributes & FileAttributes.Directory) == 0
                || RouteInspectMatchingFilesScanner.HasGitMarker(directory))
            {
                return PhysicalPathResolution.Classified(PhysicalPathState.Missing, directory);
            }
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            return PhysicalPathResolution.Classified(PhysicalPathState.Missing, directory);
        }

        return _resolver.ResolveCandidate(parent, parentResolution.GetContainedPhysicalPath(), directory);
    }
}
