using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Result;

internal sealed class FindResultInspectedCountBuilder
{
    internal int? Read(FindResultInspectedCountInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.CandidateCount is null
            || input.MatchingCoverage is FindCoverageState.NotStarted or FindCoverageState.Blocked)
        {
            return null;
        }

        var inspected = input.Inspections
            .GroupBy(inspection => new MatchKey(
                inspection.Source.Identity.AutomaticId,
                inspection.Source.Identity.CanonicalBasePath))
            .Count(group => IsInspectedSource(input.Request, group));
        return Math.Min(inspected, input.CandidateCount.Value);
    }

    private static bool IsInspectedSource(
        FindRequestEcho request,
        IEnumerable<FindLayerInspectionFacts> inspections)
    {
        var facts = inspections.ToArray();
        if (facts.Length == 0)
        {
            return false;
        }

        var source = facts[0].Source;
        var layers = new[] { source.Base, source.Overwrite }
            .Where(layer => layer is not null)
            .Cast<SourceLayer>()
            .ToArray();
        return layers.All(layer => facts.Any(inspection =>
            string.Equals(inspection.Layer.CanonicalPath, layer.CanonicalPath, StringComparison.Ordinal)
            && IsMatchingComplete(request, inspection)));
    }

    private static bool IsMatchingComplete(
        FindRequestEcho request,
        FindLayerInspectionFacts inspection)
    {
        if (inspection.Document?.BodySpan is null)
        {
            return false;
        }

        if (request.Query.EffectivePredicates.Count == 0)
        {
            return true;
        }

        foreach (var predicate in request.Query.EffectivePredicates)
        {
            if (predicate.Kind == FindPredicateKind.Heading)
            {
                continue;
            }

            foreach (var region in request.Query.Within.Tag)
            {
                if (region.Kind is FindRegionKind.Frontmatter or FindRegionKind.Document
                    && inspection.Frontmatter?.Availability != FindFrontmatterAvailability.Complete)
                {
                    return false;
                }

                if (region.Kind is FindRegionKind.Body or FindRegionKind.Section or FindRegionKind.Document
                    && inspection.BodyTags?.Availability != FindBodyTagAvailability.Complete)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private readonly record struct MatchKey(string Id, string Path);
}
