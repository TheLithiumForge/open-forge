using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Topology;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionFindings
{
    private readonly RouteListTopologyInput _input;
    private readonly RouteListTopologySelectionState _state;

    internal RouteListTopologySelectionFindings(
        RouteListTopologyInput input,
        RouteListTopologySelectionState state)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(state);
        _input = input;
        _state = state;
    }

    internal void AddSelectionIssues(IEnumerable<RouteListSelectionIssue> issues)
    {
        foreach (var issue in issues)
        {
            _state.AddFinding(RouteListTopologyFindingPolicy.FromSelectionIssue(issue), relativeDepth: 0);
        }
    }

    internal void InspectIdentity(RouteTopologyNode node, int relativeDepth)
    {
        var source = node.Source;
        var collision = _input.Inventory.Catalogue.IdentityCollisions.SingleOrDefault(candidate =>
            candidate.Paths.Contains(source.CanonicalPath, StringComparer.Ordinal));
        if (collision is not null
            && !_state.TryReadSelectedPath(source.Id, out _))
        {
            _state.AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.IdentityCollision,
                    CliSemanticStatus.Attention,
                    source.CanonicalPath,
                    $"The source ID also identifies {string.Join(", ", collision.Paths.Where(path => !string.Equals(path, source.CanonicalPath, StringComparison.Ordinal)))}."),
                relativeDepth);
        }

        if (_state.TryReadSelectedPath(source.Id, out var firstPath)
            && !string.Equals(firstPath, source.CanonicalPath, StringComparison.Ordinal))
        {
            _state.AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.IdentityCollision,
                    CliSemanticStatus.Blocked,
                    source.CanonicalPath,
                    $"The selected route ID also identifies {firstPath}."),
                relativeDepth);
            return;
        }

        _state.RegisterSelectedPath(source.Id, source.CanonicalPath);
    }

    internal void EnsureMetadataFinding(RouteTopologyNode node, int relativeDepth)
    {
        var source = node.Source;
        if (_state.Findings.Any(finding =>
                string.Equals(finding.Subject, source.CanonicalPath, StringComparison.Ordinal)
                && finding.Code is (
                    RouteListFindingCode.MetadataMissing
                    or RouteListFindingCode.MetadataMalformed
                    or RouteListFindingCode.ReadUnavailable)))
        {
            _state.MarkUnresolved(relativeDepth);
            return;
        }

        var (code, cause) = node.Source.Metadata.State switch
        {
            RouteSourceMetadataState.Missing => (
                RouteListFindingCode.MetadataMissing,
                "Required source metadata is missing."),
            RouteSourceMetadataState.Malformed => (
                RouteListFindingCode.MetadataMalformed,
                "The source frontmatter or metadata shape is malformed."),
            RouteSourceMetadataState.ReadUnavailable => (
                RouteListFindingCode.ReadUnavailable,
                "The source metadata could not be read."),
            _ => (
                RouteListFindingCode.MetadataMalformed,
                "The source does not contain complete route metadata."),
        };
        _state.AddFinding(
            new RouteListFinding(
                code,
                CliSemanticStatus.Incomplete,
                source.CanonicalPath,
                cause),
            relativeDepth);
    }

    internal void AddLoaderBoundaryIssues()
    {
        var loaderSelection = _input.LoaderRootSelection
            ?? throw new InvalidOperationException("The Loader-root boundary is unavailable for a resolved explicit selection.");
        if (loaderSelection.Issues.Count == 0)
        {
            throw new InvalidOperationException("An incomplete Loader-root boundary requires a typed issue.");
        }

        AddSelectionIssues(loaderSelection.Issues);
    }

    internal void AddRouteAmbiguous(RouteTopologyNode node, int relativeDepth, string cause)
    {
        _state.AddFinding(
            new RouteListFinding(
                RouteListFindingCode.RouteAmbiguous,
                CliSemanticStatus.Blocked,
                node.Source.CanonicalPath,
                cause,
                node.ParentState == RouteTopologyParentState.Ambiguous
                    ? node.ParentPaths
                    : null),
            relativeDepth);
    }

    internal void AddInterruption(string subject, int relativeDepth)
    {
        _state.AddFinding(
            new RouteListFinding(
                RouteListFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                subject,
                "Route topology selection was interrupted."),
            relativeDepth);
    }

    internal string ReadSelectionSubject()
    {
        return _input.Selection.Selection.AttemptedId
            ?? _input.Selection.Selection.AttemptedPath
            ?? ".agents/loader.md";
    }
}
