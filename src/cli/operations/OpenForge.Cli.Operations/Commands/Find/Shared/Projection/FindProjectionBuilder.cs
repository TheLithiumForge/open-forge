using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Projection;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Projection;

internal sealed class FindProjectionBuilder
{
    private readonly FindProjectionContentBuilder _contentBuilder = new();
    private readonly FindProjectionSourceResolver _sourceResolver = new();

    internal FindProjectionFacts Build(FindProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var requestedParts = input.Request.Presentation.Content.Effective;
        if (requestedParts.Count == 0)
        {
            return new FindProjectionFacts([], [], FindProjectionCoverageState.NotRequested);
        }

        var projections = new List<FindProjection>();
        var findings = new List<FindFinding>();
        var hasUncertainProjection = false;
        var orderedMatches = input.Matches
            .OrderBy(match => match.Id, StringComparer.Ordinal)
            .ThenBy(match => match.Path, StringComparer.Ordinal)
            .ToArray();

        for (var matchIndex = 0; matchIndex < orderedMatches.Length; matchIndex++)
        {
            var match = orderedMatches[matchIndex];
            var projectionStart = projections.Count;
            var source = _sourceResolver.Resolve(new FindProjectionSourceInput(
                match,
                input.Inspections,
                input.RouteFacts));

            if (requestedParts.Any(part => part.Kind == FindContentPartKind.Metadata))
            {
                var metadata = _contentBuilder.BuildMetadata(
                    new FindMetadataProjectionInput(source, matchIndex + 1));
                projections.Add(metadata.Projection);
                findings.AddRange(metadata.Findings);
                hasUncertainProjection |= metadata.Uncertain;
            }

            foreach (var layer in source.Layers)
            {
                foreach (var part in requestedParts.Where(part => part.Kind != FindContentPartKind.Metadata))
                {
                    var physical = _contentBuilder.BuildPhysical(
                        new FindPhysicalProjectionInput(source.SourceIdentity, layer, part));
                    projections.Add(physical.Projection);
                    findings.AddRange(physical.Findings);
                    hasUncertainProjection |= physical.Uncertain;
                }
            }

            findings.AddRange(FindProjectionFindingPolicy.BuildMissingSections(
                new FindMissingSectionFindingInput(
                    source.SourceIdentity,
                    requestedParts,
                    projections.Skip(projectionStart))));
        }

        var orderedFindings = FindProjectionFindingPolicy.Order(findings);
        var coverage = hasUncertainProjection
            ? FindProjectionCoverageState.Incomplete
            : FindProjectionCoverageState.Complete;
        return new FindProjectionFacts(projections, orderedFindings, coverage);
    }
}
