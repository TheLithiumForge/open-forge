using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal sealed partial class RouteInspectSourceSelectionResolver
{
    private async ValueTask<RouteInspectResolution> ResolveIdAsync(
        RouteInspectResolutionInput input,
        CancellationToken cancellationToken)
    {
        var requestedId = input.Parsed.AttemptedId
            ?? throw new InvalidOperationException("A source-ID selection requires its attempted ID.");
        var candidates = input.Catalogue.FindAllCandidatesById(requestedId);
        var candidatePaths = candidates
            .Select(candidate => candidate.CanonicalPath)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(input);
        }

        if (candidatePaths.Length == 0)
        {
            return RouteInspectResolution.Create(
                RouteInspectResolutionState.Invalid,
                input.UnresolvedSelection,
                null,
                null,
                [RouteInspectResolutionSupport.CreateIssue(
                    RouteInspectResolutionIssueCode.UnknownSource,
                    requestedId,
                    "The source ID does not identify a current source.")]);
        }

        if (candidatePaths.Length > 1)
        {
            var collisionSelection = new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                RouteInspectSelectionMethod.Unresolved,
                requestedId,
                candidatePaths);
            return await ResolveCollisionAsync(
                    input,
                    collisionSelection,
                    cancellationToken)
                .ConfigureAwait(false);
        }

        var candidate = candidates.Single();
        if (candidate.PhysicalState != PhysicalPathState.Contained)
        {
            return Unsafe(input.UnresolvedSelection, requestedId);
        }

        var logicalSource = input.Catalogue.FindAllById(requestedId).SingleOrDefault();
        if (logicalSource is null)
        {
            return Unsafe(input.UnresolvedSelection, requestedId);
        }

        var selection = new RouteInspectSelection(
            RouteInspectReferenceKind.SourceId,
            RouteInspectSelectionMethod.AutomaticId,
            requestedId,
            []);
        return ResolveSource(input, selection, logicalSource, logicalSource.Identity.CanonicalBasePath, cancellationToken);
    }

    private async ValueTask<RouteInspectResolution> ResolveCollisionAsync(
        RouteInspectResolutionInput input,
        RouteInspectSelection collisionSelection,
        CancellationToken cancellationToken)
    {
        var requestedId = collisionSelection.RequestedReference
            ?? throw new InvalidOperationException("An ambiguous source selection requires its requested ID.");
        string? selectedPath;
        try
        {
            selectedPath = await _interactiveSourceSelector
                .TrySelectPathAsync(input.Request, collisionSelection, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteInspectResolutionSupport.Interrupted(collisionSelection, requestedId);
        }

        if (selectedPath is null)
        {
            return BlockedByCollision(collisionSelection);
        }

        var selectedCandidate = input.Catalogue.FindCandidateByPath(selectedPath);
        if (selectedCandidate is null
            || selectedCandidate.PhysicalState != PhysicalPathState.Contained)
        {
            return Unsafe(collisionSelection, selectedPath);
        }

        var selectedSource = input.Catalogue.FindByPath(selectedPath);
        if (selectedSource is null)
        {
            return Unsafe(collisionSelection, selectedPath);
        }

        var interactiveSelection = new RouteInspectSelection(
            RouteInspectReferenceKind.SourceId,
            RouteInspectSelectionMethod.Interactive,
            requestedId,
            []);
        return ResolveSource(
            input,
            interactiveSelection,
            selectedSource,
            selectedPath,
            cancellationToken);
    }

    private static RouteInspectResolution BlockedByCollision(RouteInspectSelection selection)
    {
        var requestedId = selection.RequestedReference
            ?? throw new InvalidOperationException("An ambiguous source selection requires its requested ID.");
        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Blocked,
            selection,
            null,
            null,
            [RouteInspectResolutionSupport.CreateIssue(
                RouteInspectResolutionIssueCode.AmbiguousSource,
                requestedId,
                "The source ID identifies more than one current source.",
                selection.CandidatePaths)]);
    }
}
