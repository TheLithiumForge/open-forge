using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Result;

internal static class RouteInspectResultPolicy
{
    internal static RouteInspectConditionCode ReadConditionCode(
        RouteInspectResolutionIssueCode code,
        RouteInspectSelection selection)
    {
        return code switch
        {
            RouteInspectResolutionIssueCode.InvalidReference => RouteInspectConditionCode.InvalidSourceReference,
            RouteInspectResolutionIssueCode.LoaderSubject => RouteInspectConditionCode.LoaderSubject,
            RouteInspectResolutionIssueCode.UnknownSource => RouteInspectConditionCode.UnknownSource,
            RouteInspectResolutionIssueCode.MissingSource => selection.ReferenceKind == RouteInspectReferenceKind.SourcePath
                ? RouteInspectConditionCode.MissingSourceFile
                : RouteInspectConditionCode.MissingSource,
            RouteInspectResolutionIssueCode.UnsupportedSource => RouteInspectConditionCode.UnsupportedSource,
            RouteInspectResolutionIssueCode.AmbiguousSource => RouteInspectConditionCode.AmbiguousSource,
            RouteInspectResolutionIssueCode.UnsafeSource => RouteInspectConditionCode.UnsafeSource,
            RouteInspectResolutionIssueCode.AmbiguousRoute => RouteInspectConditionCode.AmbiguousRoute,
            RouteInspectResolutionIssueCode.OrphanOverwrite => RouteInspectConditionCode.OrphanOverwrite,
            RouteInspectResolutionIssueCode.AmbiguousOverwrite => RouteInspectConditionCode.AmbiguousOverwrite,
            RouteInspectResolutionIssueCode.ReadUnavailable => RouteInspectConditionCode.UnreadableSource,
            RouteInspectResolutionIssueCode.IncompleteRoute => RouteInspectConditionCode.IncompleteRoute,
            RouteInspectResolutionIssueCode.OperationFailure => RouteInspectConditionCode.OperationFailed,
            RouteInspectResolutionIssueCode.Interrupted => RouteInspectConditionCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect resolution issue code is not defined."),
        };
    }

    internal static CliSemanticStatus ReadConditionStatus(RouteInspectConditionCode code)
    {
        return code switch
        {
            RouteInspectConditionCode.InvalidWorkspace
                or RouteInspectConditionCode.MissingSource
                or RouteInspectConditionCode.MultipleSources
                or RouteInspectConditionCode.InvalidSourceReference
                or RouteInspectConditionCode.LoaderSubject
                or RouteInspectConditionCode.UnknownSource
                or RouteInspectConditionCode.MissingSourceFile
                or RouteInspectConditionCode.UnsupportedSource => CliSemanticStatus.Invalid,
            RouteInspectConditionCode.AmbiguousSource
                or RouteInspectConditionCode.WorkspaceUnavailable
                or RouteInspectConditionCode.UnsafeWorkspace
                or RouteInspectConditionCode.UnsafeSource
                or RouteInspectConditionCode.AmbiguousRoute
                or RouteInspectConditionCode.OrphanOverwrite
                or RouteInspectConditionCode.AmbiguousOverwrite => CliSemanticStatus.Blocked,
            RouteInspectConditionCode.UnreadableSource
                or RouteInspectConditionCode.IncompleteRoute
                or RouteInspectConditionCode.UnavailableFact => CliSemanticStatus.Incomplete,
            RouteInspectConditionCode.OperationFailed => CliSemanticStatus.Failed,
            RouteInspectConditionCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(code),
                code,
                "The route-inspect condition code is not defined."),
        };
    }

    internal static CliSemanticStatus ReadStatus(
        RouteInspectResolution resolution,
        RouteInspectProfile? profile,
        IReadOnlyList<RouteInspectCondition> conditions,
        IReadOnlyList<RouteInspectObservation> observations)
    {
        return resolution.State switch
        {
            RouteInspectResolutionState.Invalid => CliSemanticStatus.Invalid,
            RouteInspectResolutionState.Blocked => CliSemanticStatus.Blocked,
            RouteInspectResolutionState.Failed => CliSemanticStatus.Failed,
            RouteInspectResolutionState.Interrupted => CliSemanticStatus.Interrupted,
            RouteInspectResolutionState.Resolved or RouteInspectResolutionState.Incomplete =>
                ReadOrdinaryStatus(resolution, profile, conditions, observations),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The route-inspect resolution state is not defined."),
        };
    }

    internal static CliNextAction? ReadNextAction(
        CliSemanticStatus status,
        RouteInspectResolution resolution,
        IReadOnlyList<RouteInspectCondition> conditions)
    {
        return status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => resolution.Selection.SelectionMethod == RouteInspectSelectionMethod.Interactive
                ? new CliNextAction(
                    ReadExactCommand(resolution),
                    "Rerun with the exact path for non-interactive use.")
                : null,
            CliSemanticStatus.Invalid => new CliNextAction(
                "open-forge route inspect --help",
                "Correct the named source or input, then rerun route inspect."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                "open-forge doctor",
                "Review the unavailable route fact, then rerun route inspect."),
            CliSemanticStatus.Blocked => ReadBlockedAction(resolution, conditions),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge route inspect",
                "Address the reported failure, then retry route inspect."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge route inspect",
                "Rerun the same route-inspect request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The route-inspect status is not defined."),
        };
    }

    private static CliSemanticStatus ReadOrdinaryStatus(
        RouteInspectResolution resolution,
        RouteInspectProfile? profile,
        IReadOnlyList<RouteInspectCondition> conditions,
        IReadOnlyList<RouteInspectObservation> observations)
    {
        if (conditions.Any(condition => condition.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (resolution.State == RouteInspectResolutionState.Incomplete
            || profile?.Completeness == RouteInspectCompleteness.Incomplete
            || conditions.Any(condition => condition.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return observations.Any(observation => observation.Code == RouteInspectObservationCode.AutomaticIdNotUnique)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static CliNextAction ReadBlockedAction(
        RouteInspectResolution resolution,
        IReadOnlyList<RouteInspectCondition> conditions)
    {
        var collision = conditions.FirstOrDefault(
            condition => condition.Code == RouteInspectConditionCode.AmbiguousSource);
        if (collision is not null && collision.Paths.Count != 0)
        {
            return new CliNextAction(
                $"open-forge route inspect \"{collision.Paths[0]}\"",
                "Rerun with one listed exact path to resolve the source collision.");
        }

        if (conditions.Any(condition => condition.Code == RouteInspectConditionCode.AmbiguousRoute))
        {
            return new CliNextAction(
                ReadExactCommand(resolution),
                "Rerun with the exact source path after resolving the ambiguous route.");
        }

        return new CliNextAction(
            "open-forge doctor",
            "Review the blocked source boundary, then rerun route inspect.");
    }

    private static string ReadExactCommand(RouteInspectResolution resolution)
    {
        var reference = resolution.Identity?.CanonicalWorkspaceRelativePath
            ?? resolution.Selection.RequestedReference
            ?? "<exact-path>";
        return $"open-forge route inspect \"{reference}\"";
    }
}
