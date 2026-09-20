using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

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
        private bool _interrupted;

        internal RouteMarkdownCatalogueRequest Request { get; } = request;

        internal bool HasBlockedFinding => _findings.Any(finding => finding.Code is
            RouteMarkdownCatalogueFindingCode.IncludedRootUnsafe
            or RouteMarkdownCatalogueFindingCode.ExcludedPathUnsafe
            or RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe);

        internal void Enqueue(DirectoryBoundary directory) => _directories.Enqueue(directory);

        internal bool TryDequeue(out DirectoryBoundary directory)
            => _directories.TryDequeue(out directory!);

        internal bool MarkDirectory(string physicalPath) => _physicalDirectories.Add(physicalPath);

        internal bool IsExcluded(string path)
            => Request.ExcludedPaths.Any(excluded => path == excluded
                || excluded != "." && path.StartsWith($"{excluded}/", StringComparison.Ordinal)
                || excluded == ".");

        internal void AddSelected(string path) => _selectedPaths.Add(path);

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
                    _findings);
            }

            var coverage = HasBlockedFinding
                ? RouteMarkdownCatalogueCoverage.Blocked
                : _findings.Count > 0
                    ? RouteMarkdownCatalogueCoverage.Incomplete
                    : RouteMarkdownCatalogueCoverage.Complete;
            return new RouteMarkdownCatalogue(coverage, _selectedPaths, _findings);
        }
    }
}
