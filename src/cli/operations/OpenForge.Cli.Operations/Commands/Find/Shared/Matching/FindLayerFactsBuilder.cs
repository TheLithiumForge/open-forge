using OpenForge.Cli.Core.Commands.Find.Models.Matching;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Matching;

internal sealed class FindLayerFactsBuilder
{
    internal FindSourceLayerFacts Build(FindLayerFactsInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inspections = input.Inspections
            .OrderBy(inspection => FindMatchingFindingPolicy.ReadLayerRank(inspection.Layer.Kind))
            .ThenBy(inspection => inspection.Layer.CanonicalPath, StringComparer.Ordinal)
            .ToArray();
        var layers = new List<FindLayerFacts>(input.Source.Overwrite is null ? 1 : 2);
        var findings = new List<FindFinding>();
        var coverage = FindCoverageState.Complete;

        AddLayer(input.Source.Base);
        if (input.Source.Overwrite is { } overwrite)
        {
            AddLayer(overwrite);
        }

        var description = layers
            .FirstOrDefault(layer => ReferenceEquals(layer.Layer, input.Source.Base))
            ?.Inspection
            ?.Description;
        return new FindSourceLayerFacts(
            input.Source,
            description,
            layers.ToArray(),
            findings.ToArray(),
            coverage);

        void AddLayer(SourceLayer layer)
        {
            var inspection = inspections.FirstOrDefault(value =>
                value.Layer.Kind == layer.Kind
                && string.Equals(value.Layer.CanonicalPath, layer.CanonicalPath, StringComparison.Ordinal));

            if (inspection is null)
            {
                var unavailable = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                    FindFindingCode.InspectionUnavailable,
                    input.Source,
                    layer,
                    null,
                    "The source layer could not be inspected."));
                FindMatchingFindingPolicy.Add(findings, unavailable);
                coverage = FindMatchingFindingPolicy.MergeCoverage(
                    coverage,
                    FindCoverageState.Incomplete);
            }
            else
            {
                foreach (var finding in inspection.Findings)
                {
                    findings.Add(finding);
                    coverage = FindMatchingFindingPolicy.MergeCoverage(
                        coverage,
                        FindMatchingFindingPolicy.ReadCoverage(finding.Code));
                }

                if (inspection.Document?.BodySpan is null)
                {
                    coverage = FindMatchingFindingPolicy.MergeCoverage(
                        coverage,
                        FindCoverageState.Incomplete);
                    if (inspection.Findings.Count == 0)
                    {
                        var unavailable = FindMatchingFindingPolicy.Create(new FindMatchingFindingInput(
                            FindFindingCode.InspectionUnavailable,
                            input.Source,
                            layer,
                            null,
                            "The source layer could not be inspected."));
                        FindMatchingFindingPolicy.Add(findings, unavailable);
                    }
                }
            }

            layers.Add(new FindLayerFacts(input.Source, layer, inspection));
        }
    }
}
