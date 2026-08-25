using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;

internal sealed class RouteListTopologySelectionFindings
{
    private readonly RouteListTopologyInput _input;
    private readonly SourceRouteFacts _routeFacts;
    private readonly RouteListTopologySelectionState _state;

    internal RouteListTopologySelectionFindings(
        RouteListTopologyInput input,
        SourceRouteFacts routeFacts,
        RouteListTopologySelectionState state)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(routeFacts);
        ArgumentNullException.ThrowIfNull(state);
        _input = input;
        _routeFacts = routeFacts;
        _state = state;
    }

    internal void AddSelectionIssues(IEnumerable<RouteListSelectionIssue> issues)
    {
        foreach (var issue in issues)
        {
            _state.AddFinding(RouteListTopologyFindingPolicy.FromSelectionIssue(issue), relativeDepth: 0);
        }
    }

    internal void InspectIdentity(
        SourceRouteNode node,
        SourceRouteFact routeFact,
        int relativeDepth)
    {
        var source = RouteListTopologyProjectionPolicy.ReadSource(_input, node);
        var collisionPaths = _input.Inventory.SourceCatalogue.Sources
            .Where(candidate => string.Equals(
                candidate.Identity.AutomaticId,
                routeFact.Identity.AutomaticId,
                StringComparison.Ordinal))
            .Select(candidate => candidate.Identity.CanonicalBasePath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (routeFact.IsIdentityUnique != (collisionPaths.Length == 1))
        {
            throw new InvalidOperationException("The neutral route identity fact does not match the selected source facts.");
        }

        if (!routeFact.IsIdentityUnique
            && !_state.TryReadSelectedPath(source.Id, out _))
        {
            _state.AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.IdentityCollision,
                    CliSemanticStatus.Attention,
                    source.CanonicalPath,
                    $"The source ID also identifies {string.Join(", ", collisionPaths.Where(path => !string.Equals(path, source.CanonicalPath, StringComparison.Ordinal)))}."),
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

    internal void EnsureMetadataFinding(SourceRouteNode node, int relativeDepth)
    {
        var source = RouteListTopologyProjectionPolicy.ReadSource(_input, node);
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

        var (code, cause) = source.Metadata.State switch
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

    internal void AddRouteAmbiguous(SourceRouteNode node, int relativeDepth, string cause)
    {
        var source = RouteListTopologyProjectionPolicy.FindSource(_input, node);
        _state.AddFinding(
            new RouteListFinding(
                RouteListFindingCode.RouteAmbiguous,
                CliSemanticStatus.Blocked,
                source?.CanonicalPath ?? node.Identity.CanonicalBasePath,
                cause,
                node.ParentState == SourceRouteParentState.Ambiguous
                    ? node.ParentPaths
                    : null),
            relativeDepth);
    }

    internal void AddUnavailable(SourceRouteFact routeFact, int relativeDepth)
    {
        if (routeFact.State != SourceRouteState.Unavailable)
        {
            throw new ArgumentException("Only an unavailable source route fact can form an unavailable boundary.", nameof(routeFact));
        }

        if (_input.LoaderRootSelection is { State: not RouteListSelectionResolutionState.Resolved })
        {
            AddLoaderBoundaryIssues();
            return;
        }

        var issue = _routeFacts.Issues.FirstOrDefault(candidate =>
            string.Equals(candidate.CanonicalPath, routeFact.Identity.CanonicalBasePath, StringComparison.Ordinal)
            || candidate.RelatedPaths.Contains(routeFact.Identity.CanonicalBasePath, StringComparer.Ordinal));
        if (issue is null)
        {
            _state.AddFinding(
                new RouteListFinding(
                    RouteListFindingCode.LoaderUnavailable,
                    CliSemanticStatus.Incomplete,
                    routeFact.Identity.CanonicalBasePath,
                    "Required route facts are unavailable."),
                relativeDepth);
            return;
        }

        _state.AddFinding(RouteListTopologyFindingPolicy.FromRouteIssue(issue), relativeDepth);
    }

    internal void AddUnsupportedSource(RouteSource source)
    {
        _state.AddFinding(
            new RouteListFinding(
                RouteListFindingCode.UnsupportedSource,
                CliSemanticStatus.Invalid,
                source.CanonicalPath,
                "The selected source is not a routed sibling of an entrypoint."),
            relativeDepth: 0);
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
