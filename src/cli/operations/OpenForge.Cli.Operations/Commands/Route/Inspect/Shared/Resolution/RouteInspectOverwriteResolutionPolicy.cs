using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectOverwriteResolutionPolicy
{
    internal static RouteOverwriteFact? FindAmbiguousCandidate(
        RouteSourceProjectionSet projectionSet,
        string candidatePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidatePath);
        return projectionSet.OverwriteFacts.FirstOrDefault(fact =>
            fact.State == RouteOverwriteState.Ambiguous
            && fact.CandidateBasePaths.Contains(candidatePath, StringComparer.Ordinal));
    }

    internal static RouteInspectResolution Blocked(
        RouteInspectSelection selection,
        RouteOverwriteFact overwrite)
    {
        var issueCode = overwrite.State == RouteOverwriteState.Orphan
            ? RouteInspectResolutionIssueCode.OrphanOverwrite
            : RouteInspectResolutionIssueCode.AmbiguousOverwrite;
        var message = overwrite.State == RouteOverwriteState.Orphan
            ? "The overwrite companion has no available base source."
            : "The overwrite companion has more than one possible base source.";
        return RouteInspectResolution.Create(
            RouteInspectResolutionState.Blocked,
            selection,
            null,
            null,
            [RouteInspectResolutionSupport.CreateIssue(
                issueCode,
                overwrite.CanonicalPath,
                message,
                overwrite.CandidateBasePaths)]);
    }
}
