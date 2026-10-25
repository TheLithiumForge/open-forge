using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Route.Shared.References;

internal sealed partial class RouteMarkdownCatalogueReader
{
    private sealed record DirectoryBoundary(string CanonicalPath, string PhysicalPath);

    private sealed class ReadState(RouteMarkdownCatalogueRequest request)
    {
        private readonly Queue<DirectoryBoundary> _directories = new();
        private readonly HashSet<string> _physicalDirectories = new(
            PhysicalIdentityTracker.PathComparer);
        private readonly HashSet<string> _selectedPaths = new(StringComparer.Ordinal);
        private readonly List<RouteMarkdownCatalogueFinding> _findings = [];
        private readonly HashSet<string> _ignoredPaths = new(PhysicalIdentityTracker.PathComparer);
        private readonly HashSet<string> _gitIgnoredPaths = new(PhysicalIdentityTracker.PathComparer);
        private bool _interrupted;

        internal RouteMarkdownCatalogueRequest Request { get; } = request;

        internal void SetGitIgnoredPaths(IEnumerable<string> paths) => _gitIgnoredPaths.UnionWith(paths);

        internal bool HasBlockedFinding => _findings.Any(finding => finding.Code is
            RouteMarkdownCatalogueFindingCode.IncludedRootUnsafe
            or RouteMarkdownCatalogueFindingCode.ExcludedPathUnsafe
            or RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe);

        internal void Enqueue(DirectoryBoundary directory) => _directories.Enqueue(directory);

        internal bool TryDequeue(out DirectoryBoundary directory)
            => _directories.TryDequeue(out directory!);

        internal bool MarkDirectory(string physicalPath) => _physicalDirectories.Add(physicalPath);

        internal bool IsExcluded(string path)
            => SourceLogicalPath.IsOutsideSource(Request.Layout, path)
                && path.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.OrdinalIgnoreCase)
                || Request.ExcludedPaths.Any(excluded => path == excluded
                || excluded != "." && path.StartsWith($"{excluded}/", StringComparison.Ordinal)
                || excluded == ".");

        internal void AddSelected(string path) => _selectedPaths.Add(path);

        private bool IsContentTraversalPath(string path)
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            return Request.Layout.SourceRoots.Any(root => string.Equals(root, path, comparison)
                || path.StartsWith(root + "/", comparison) || root.StartsWith(path + "/", comparison))
                || Request.Layout.OutsideRoutes.Any(route => path == route.DirectoryPath
                    || path.StartsWith(route.DirectoryPath + "/", StringComparison.Ordinal)
                    || route.DirectoryPath.StartsWith(path + "/", StringComparison.Ordinal));
        }

        internal string NormalizeContentRoot(string path)
            => Request.Layout.SourceRoots.FirstOrDefault(root => PhysicalIdentityTracker.PathComparer.Equals(root, path)) ?? path;

        internal bool IsGitIgnored(string path)
        {
            if (IsContentTraversalPath(path))
            {
                return false;
            }

            var candidate = path;
            while (!_gitIgnoredPaths.Contains(candidate))
            {
                var separator = candidate.LastIndexOf('/');
                if (separator < 0) return false;
                candidate = candidate[..separator];
            }

            _ignoredPaths.Add(path);
            return true;
        }

        internal void MarkInterrupted() => _interrupted = true;

        internal void AddFinding(
            RouteMarkdownCatalogueFindingCode code,
            string path,
            string cause)
            => _findings.Add(new RouteMarkdownCatalogueFinding(code, path, cause));

        internal RouteMarkdownCatalogue Form(bool interrupted)
        {
            if (interrupted || _interrupted)
            {
                _findings.Add(new RouteMarkdownCatalogueFinding(
                    RouteMarkdownCatalogueFindingCode.Interrupted,
                    path: null,
                    "Markdown catalogue inspection was interrupted."));
                return new RouteMarkdownCatalogue(
                    RouteMarkdownCatalogueCoverage.Interrupted,
                    _selectedPaths,
                    _findings,
                    _ignoredPaths.Count);
            }

            var coverage = HasBlockedFinding
                ? RouteMarkdownCatalogueCoverage.Blocked
                : _findings.Count > 0
                    ? RouteMarkdownCatalogueCoverage.Incomplete
                    : RouteMarkdownCatalogueCoverage.Complete;
            return new RouteMarkdownCatalogue(coverage, _selectedPaths, _findings, _ignoredPaths.Count);
        }
    }
}
