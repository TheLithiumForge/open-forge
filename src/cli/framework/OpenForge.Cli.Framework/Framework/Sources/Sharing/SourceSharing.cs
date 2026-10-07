using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Sources.Models.Sharing;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Sharing;

/// <summary>
/// Neutral sharing facts for generated navigation, defined by the lifecycle-provenance contract.
/// Local discovery remains complete. Only a consumer requesting shareable navigation uses this view.
/// </summary>
internal sealed class SourceSharing
{
    private readonly ImmutableArray<SourceSharingRoute> _routes;

    internal SourceSharing(ImmutableArray<SourceSharingRoute> routes)
    {
        foreach (var route in routes)
        {
            if (route is null
                || !IsCanonicalPath(route.Directory)
                || !route.Directory.StartsWith($"{SourceLogicalPath.AgentsRoot}/", StringComparison.Ordinal)
                || !IsCanonicalPath(route.Entrypoint)
                || SourceLogicalPath.ReadParent(route.Entrypoint) != route.Directory
                || !SourceFormClassifier.TryClassify(route.Entrypoint, out var form)
                || !SourceFormClassifier.IsEntrypoint(form))
                throw new ArgumentException("A Gitignored route requires a workspace-relative directory and its direct recognized entrypoint.", nameof(routes));
        }
        var directories = routes.Select(route => PortableWorkspacePath.CreatePortableKey(route.Directory)).ToArray();
        if (directories.Distinct(StringComparer.Ordinal).Count() != directories.Length
            || directories.Any(directory => directories.Any(other => other != directory && directory.StartsWith($"{other}/", StringComparison.Ordinal))))
            throw new ArgumentException("Gitignored route directories must be distinct and cannot overlap.", nameof(routes));
        _routes = routes;
    }

    internal bool Includes(string path)
        => !_routes.Any(route => PortableWorkspacePath.CreatePortableKey(path).StartsWith($"{PortableWorkspacePath.CreatePortableKey(route.Directory)}/", StringComparison.Ordinal)
            && PortableWorkspacePath.CreatePortableKey(path) != PortableWorkspacePath.CreatePortableKey(route.Entrypoint));

    internal string? FindUnavailableEntrypoint(SourceCatalogue catalogue)
    {
        foreach (var route in _routes)
        {
            var candidates = catalogue.Candidates.Where(candidate => candidate.Form is { } form
                && SourceFormClassifier.IsEntrypoint(form)
                && SourceLogicalPath.ReadParent(candidate.CanonicalPath) == route.Directory).ToArray();
            if (candidates.Length != 1 || candidates[0].CanonicalPath != route.Entrypoint
                || catalogue.FindByPath(route.Entrypoint) is null) return route.Entrypoint;
        }
        return null;
    }

    internal SourceCatalogue Project(SourceCatalogue catalogue)
        => new(catalogue.Workspace,
            catalogue.Candidates.Where(candidate => Includes(candidate.CanonicalPath)),
            catalogue.Sources.Where(source => Includes(source.Identity.CanonicalBasePath))
                .Select(source => source.Overwrite is { } overwrite && !Includes(overwrite.CanonicalPath)
                    ? new SourceLogicalSource(source.Identity, source.Base) : source),
            catalogue.Issues.Where(issue => Includes(issue.AttemptedCanonicalPath)
                || issue.RelatedPaths.Any(Includes)), catalogue.IsCancelled);

    private static bool IsCanonicalPath(string path)
        => PortableWorkspacePath.TryNormalize(path, out var normalized) && normalized == path;
}
