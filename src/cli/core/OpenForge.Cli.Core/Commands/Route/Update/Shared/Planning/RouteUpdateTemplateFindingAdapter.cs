using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal static class RouteUpdateTemplateFindingAdapter
{
    internal static RouteUpdateFindingCode Read(
        RouteTemplateResolutionState state,
        RouteTemplateResolutionIssue? issue)
    {
        if (issue is { } definedIssue && !Enum.IsDefined(definedIssue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(issue),
                issue,
                "The Template resolution issue is not defined.");
        }

        if (issue is RouteTemplateResolutionIssue.ResolutionInterrupted
            or RouteTemplateResolutionIssue.ReadInterrupted)
        {
            return RouteUpdateFindingCode.Interrupted;
        }

        return state switch
        {
            RouteTemplateResolutionState.Invalid => RouteUpdateFindingCode.InvalidTemplate,
            RouteTemplateResolutionState.Blocked => RouteUpdateFindingCode.TemplateUnsafe,
            RouteTemplateResolutionState.Incomplete => ReadIncomplete(issue),
            RouteTemplateResolutionState.Resolved => throw new InvalidOperationException(
                "A resolved Template cannot form a Route Update stop finding."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Template resolution state is not defined."),
        };
    }

    private static RouteUpdateFindingCode ReadIncomplete(
        RouteTemplateResolutionIssue? issue)
        => issue switch
        {
            RouteTemplateResolutionIssue.InvalidReference =>
                RouteUpdateFindingCode.InvalidTemplate,
            RouteTemplateResolutionIssue.InvalidSourceKind
                or RouteTemplateResolutionIssue.OverwriteUnsafe
                or RouteTemplateResolutionIssue.MetadataUnsafe
                or RouteTemplateResolutionIssue.MissingClassification
                or RouteTemplateResolutionIssue.BodyBoundaryUnsafe
                or RouteTemplateResolutionIssue.SourceUnsafe =>
                RouteUpdateFindingCode.TemplateUnsafe,
            RouteTemplateResolutionIssue.SourceUnavailable =>
                RouteUpdateFindingCode.TemplateUnavailable,
            RouteTemplateResolutionIssue.ResolutionInterrupted
                or RouteTemplateResolutionIssue.ReadInterrupted =>
                RouteUpdateFindingCode.Interrupted,
            null => throw new InvalidOperationException(
                "An incomplete Template resolution requires one issue."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(issue),
                issue,
                "The Template resolution issue is not defined."),
        };
}
