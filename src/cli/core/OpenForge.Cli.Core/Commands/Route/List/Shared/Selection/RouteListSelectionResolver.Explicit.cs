using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal sealed partial class RouteListSelectionResolver
{
    private RouteListSelectionResolution ResolveId(RouteListSelectionStage stage)
    {
        var candidates = stage.Catalogue.FindAllCandidatesById(stage.AttemptedReference);
        if (candidates.Count == 0)
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnknownSource,
                stage.AttemptedReference,
                "The source ID does not identify a current source.");
        }

        if (candidates.Count > 1)
        {
            return RouteListSelectionResolutionFactory.Blocked(
                stage.Attempted,
                [],
                [new RouteListSelectionIssue(
                    RouteListFindingCode.AmbiguousSource,
                    stage.AttemptedReference,
                    "The source ID identifies more than one current source.",
                    candidates.Select(candidate => candidate.CanonicalPath))]);
        }

        var candidate = candidates[0];
        var physical = ResolveCandidate(stage.Request, candidate.CanonicalPath);
        if (stage.CancellationToken.IsCancellationRequested)
        {
            return Interrupted(stage.Attempted, stage.AttemptedReference);
        }

        if (physical.State == PhysicalPathState.Missing)
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnknownSource,
                stage.AttemptedReference,
                "The catalogue source path does not exist.");
        }

        if (!MatchesCandidatePhysicalIdentity(candidate, physical))
        {
            return PhysicalBoundary(
                stage.Attempted,
                stage.AttemptedReference,
                "The catalogue source path crosses an unproved physical boundary or has changed identity.");
        }

        var logicalSource = stage.Catalogue.FindByPath(candidate.CanonicalPath);
        if (logicalSource is null
            || !string.Equals(logicalSource.Identity.CanonicalBasePath, candidate.CanonicalPath, StringComparison.Ordinal))
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnsupportedSource,
                stage.AttemptedReference,
                "The source ID does not identify a supported logical source.");
        }

        var physicalIssue = ValidatePhysicalIdentity(
            stage.Attempted,
            stage.AttemptedReference,
            candidate,
            logicalSource.Base,
            physical);
        return physicalIssue ?? ResolveSource(
            stage.Attempted,
            logicalSource,
            stage.ProjectionSet,
            stage.RouteFacts,
            RouteListSelectionKind.SourceId);
    }

    private RouteListSelectionResolution ResolvePath(RouteListSelectionStage stage)
    {
        var physical = ResolveCandidate(stage.Request, stage.AttemptedReference);
        if (stage.CancellationToken.IsCancellationRequested)
        {
            return Interrupted(stage.Attempted, stage.AttemptedReference);
        }

        if (physical.State == PhysicalPathState.Missing)
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnknownSource,
                stage.AttemptedReference,
                "The exact source path does not exist.");
        }

        if (physical.State != PhysicalPathState.Contained)
        {
            return PhysicalBoundary(
                stage.Attempted,
                stage.AttemptedReference,
                "The exact source path crosses an unproved physical boundary.");
        }

        var candidate = stage.Catalogue.FindCandidateByPath(stage.AttemptedReference);
        if (candidate is null)
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnsupportedSource,
                stage.AttemptedReference,
                "The exact contained path is not a recognized logical source.");
        }

        if (!MatchesCandidatePhysicalIdentity(candidate, physical))
        {
            return PhysicalBoundary(
                stage.Attempted,
                stage.AttemptedReference,
                "The exact source path does not match its catalogue physical identity.");
        }

        var logicalSource = stage.Catalogue.FindByPath(stage.AttemptedReference);
        if (logicalSource is null)
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnsupportedSource,
                stage.AttemptedReference,
                "The exact contained path is not a recognized logical source.");
        }

        var selectedLayer = string.Equals(
            stage.AttemptedReference,
            logicalSource.Identity.CanonicalBasePath,
            StringComparison.Ordinal)
            ? logicalSource.Base
            : logicalSource.Overwrite;
        if (selectedLayer is null
            || !string.Equals(selectedLayer.CanonicalPath, stage.AttemptedReference, StringComparison.Ordinal))
        {
            return Invalid(
                stage.Attempted,
                RouteListFindingCode.UnsupportedSource,
                stage.AttemptedReference,
                "The exact contained path is not a base or adjacent overwrite layer.");
        }

        var physicalIssue = ValidatePhysicalIdentity(
            stage.Attempted,
            stage.AttemptedReference,
            candidate,
            selectedLayer,
            physical);
        if (physicalIssue is not null)
        {
            return physicalIssue;
        }

        if (selectedLayer.Kind == SourceLayerKind.Overwrite)
        {
            var basePhysical = ResolveCandidate(stage.Request, logicalSource.Base.CanonicalPath);
            if (stage.CancellationToken.IsCancellationRequested)
            {
                return Interrupted(stage.Attempted, stage.AttemptedReference);
            }

            if (basePhysical.State == PhysicalPathState.Missing)
            {
                return Invalid(
                    stage.Attempted,
                    RouteListFindingCode.UnsupportedSource,
                    stage.AttemptedReference,
                    "The overwrite companion has no available base source.");
            }

            var baseCandidate = stage.Catalogue.FindCandidateByPath(logicalSource.Base.CanonicalPath);
            if (baseCandidate is null
                || ValidatePhysicalIdentity(
                    stage.Attempted,
                    stage.AttemptedReference,
                    baseCandidate,
                    logicalSource.Base,
                    basePhysical) is not null)
            {
                return PhysicalBoundary(
                    stage.Attempted,
                    stage.AttemptedReference,
                    "The overwrite base source crosses an unproved physical boundary or has changed identity.");
            }
        }

        return ResolveSource(
            stage.Attempted,
            logicalSource,
            stage.ProjectionSet,
            stage.RouteFacts,
            RouteListSelectionKind.SourcePath);
    }
}
