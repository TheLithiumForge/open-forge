using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Planning;

internal static class RemovePathSafety
{
    private const string AgentsDirectory = ".agents";
    private const string RecoveryDirectory = ".agents/recovery";

    internal static bool IsProtectedPath(string path)
    {
        var portablePath = PortableWorkspacePath.CreatePortableKey(path);
        return string.Equals(portablePath, AgentsDirectory, StringComparison.Ordinal)
            || ContainsGitMetadataSegment(portablePath)
            || IsSameOrDescendant(portablePath, WorkspaceSettingsDefinitions.RelativePath)
            || IsSameOrDescendant(portablePath, WorkspaceOwnershipDefinitions.RelativePath)
            || IsSameOrDescendant(portablePath, RecoveryDirectory);
    }

    internal static bool ContainsGitMetadataSegment(string path)
        => path.Split('/').Any(segment => string.Equals(segment, ".git", StringComparison.OrdinalIgnoreCase));

    internal static bool IsLibrarySourcePath(string path, WorkspaceOwnershipDocument ownership)
        => ownership.Libraries.Any(library => IsSameOrDescendant(path, library.SourceRoot)
            || IsSameOrDescendant(library.SourceRoot, path));

    internal static string? FindKnownAlias(
        string path,
        WorkspaceSettingsDocument settings,
        WorkspaceOwnershipDocument ownership,
        LibraryRegistrationSet? registrations)
    {
        var candidates = new List<string>();
        candidates.AddRange(settings.RemovedFiles);
        candidates.AddRange(settings.RemovedDirectories);
        candidates.AddRange(settings.RemovedCategories.Select(category => $".agents/{category}"));
        if (ownership.Framework is { } framework)
        {
            candidates.AddRange(framework.Paths);
            candidates.AddRange(framework.Regions.Select(region => region.Path));
        }
        foreach (var extension in ownership.Extensions)
        {
            candidates.AddRange(extension.Paths);
            candidates.AddRange(extension.Regions.Select(region => region.Path));
        }
        foreach (var library in ownership.Libraries)
        {
            candidates.Add(library.SourceRoot);
        }
        if (registrations is not null)
        {
            candidates.AddRange(registrations.Libraries
                .SelectMany(library => LibraryPathIdentity.Mappings(library))
                .Select(mapping => mapping.DestinationPath.Value));
        }

        foreach (var candidate in candidates.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            var canonical = ReadCaseAliasedPath(path, candidate);
            if (canonical is not null)
            {
                return canonical;
            }
        }
        return null;
    }

    internal static string? FindFilesystemAlias(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        string path,
        CancellationToken cancellationToken)
    {
        var requestedSegments = path.Split('/');
        var actualSegments = new List<string>(requestedSegments.Length);
        var physicalParent = workspace.PhysicalRoot;
        for (var index = 0; index < requestedSegments.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(physicalParent);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return null;
            }
            var actualName = entries
                .Select(Path.GetFileName)
                .FirstOrDefault(name => string.Equals(name, requestedSegments[index], StringComparison.OrdinalIgnoreCase));
            if (actualName is null)
            {
                return null;
            }
            if (!string.Equals(actualName, requestedSegments[index], StringComparison.Ordinal))
            {
                var alias = actualSegments.Append(actualName).Concat(requestedSegments.Skip(index + 1));
                return string.Join('/', alias);
            }
            actualSegments.Add(actualName);
            physicalParent = Path.Combine(physicalParent, actualName);
            if (index < requestedSegments.Length - 1)
            {
                var logicalParent = Path.Combine(
                    workspace.LexicalRoot,
                    string.Join('/', actualSegments).Replace('/', Path.DirectorySeparatorChar));
                if (NoFollowLeafObserver.Observe(resolver, workspace, logicalParent, cancellationToken).State
                    != NoFollowLeafState.Directory)
                {
                    return null;
                }
            }
        }
        return null;
    }

    internal static bool IsSameOrDescendant(string path, string root)
    {
        var comparer = PathComparer();
        if (comparer.Equals(path, root))
        {
            return true;
        }

        var prefix = root + "/";
        return path.Length >= prefix.Length
            && comparer.Equals(path[..prefix.Length], prefix);
    }

    internal static StringComparer PathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static string? ReadCaseAliasedPath(string requested, string established)
    {
        var requestedSegments = requested.Split('/');
        var establishedSegments = established.Split('/');
        var count = Math.Min(requestedSegments.Length, establishedSegments.Length);
        for (var index = 0; index < count; index++)
        {
            if (string.Equals(requestedSegments[index], establishedSegments[index], StringComparison.OrdinalIgnoreCase)
                && !string.Equals(requestedSegments[index], establishedSegments[index], StringComparison.Ordinal))
            {
                return string.Join('/', establishedSegments.Take(index + 1).Concat(requestedSegments.Skip(index + 1)));
            }
            if (!string.Equals(requestedSegments[index], establishedSegments[index], StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
        }
        return null;
    }
}
