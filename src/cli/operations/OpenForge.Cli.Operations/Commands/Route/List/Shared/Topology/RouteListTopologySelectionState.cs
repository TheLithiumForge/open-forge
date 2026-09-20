using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionState
{
    private readonly Dictionary<string, string> _selectedPathsById = new(StringComparer.Ordinal);
    private readonly HashSet<string> _visitedPaths = new(StringComparer.Ordinal);
    private IReadOnlySet<string> _selectedRootPaths = new HashSet<string>(StringComparer.Ordinal);

    internal RouteListTopologySelectionState(CancellationToken cancellationToken)
    {
        CancellationToken = cancellationToken;
    }

    internal CancellationToken CancellationToken { get; }

    internal List<RouteListRow> Rows { get; } = [];

    internal List<RouteListFinding> Findings { get; } = [];

    internal int? FirstUnresolvedDepth { get; private set; }

    internal bool IsSelectedRoot(string path)
    {
        return _selectedRootPaths.Contains(path);
    }

    internal void SetSelectedRootPaths(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        _selectedRootPaths = paths.ToHashSet(StringComparer.Ordinal);
    }

    internal bool TryVisit(string path)
    {
        return _visitedPaths.Add(path);
    }

    internal bool TryReadSelectedPath(string id, out string? firstPath)
    {
        return _selectedPathsById.TryGetValue(id, out firstPath);
    }

    internal void RegisterSelectedPath(string id, string path)
    {
        _selectedPathsById[id] = path;
    }

    internal void AddRow(RouteListRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        Rows.Add(row);
    }

    internal void AddFinding(RouteListFinding finding, int relativeDepth)
    {
        ArgumentNullException.ThrowIfNull(finding);
        Findings.Add(finding);
        if (finding.Status != CliSemanticStatus.Attention)
        {
            FirstUnresolvedDepth = FirstUnresolvedDepth is null
                ? relativeDepth
                : Math.Min(FirstUnresolvedDepth.Value, relativeDepth);
        }
    }

    internal void MarkUnresolved(int relativeDepth)
    {
        FirstUnresolvedDepth = FirstUnresolvedDepth is null
            ? relativeDepth
            : Math.Min(FirstUnresolvedDepth.Value, relativeDepth);
    }
}
