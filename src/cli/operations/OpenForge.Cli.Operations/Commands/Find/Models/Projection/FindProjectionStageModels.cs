using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Find.Models.Projection;

internal sealed record FindProjectionInput
{
    internal FindProjectionInput(
        FindRequest request,
        FindUniverse universe,
        IEnumerable<FindLayerInspectionFacts> inspections,
        IEnumerable<FindMatch> matches,
        SourceRouteFacts? routeFacts)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(universe);
        ArgumentNullException.ThrowIfNull(inspections);
        ArgumentNullException.ThrowIfNull(matches);
        var materializedInspections = inspections.ToArray();
        var materializedMatches = matches.ToArray();
        if (materializedInspections.Any(inspection => inspection is null)
            || materializedMatches.Any(match => match is null))
        {
            throw new ArgumentException("Find projection input collections cannot contain null values.");
        }

        Request = request;
        Universe = universe;
        Inspections = Array.AsReadOnly(materializedInspections);
        Matches = Array.AsReadOnly(materializedMatches);
        RouteFacts = routeFacts;
    }

    internal FindRequest Request { get; }

    internal FindUniverse Universe { get; }

    internal IReadOnlyList<FindLayerInspectionFacts> Inspections { get; }

    internal IReadOnlyList<FindMatch> Matches { get; }

    internal SourceRouteFacts? RouteFacts { get; }
}

internal sealed record FindProjectionFacts
{
    internal FindProjectionFacts(
        IEnumerable<FindProjection> projections,
        IEnumerable<FindFinding> findings,
        FindProjectionCoverageState coverage)
    {
        ArgumentNullException.ThrowIfNull(projections);
        ArgumentNullException.ThrowIfNull(findings);
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Find projection coverage state is not defined.");
        }

        var materializedProjections = projections.ToArray();
        var materializedFindings = findings.ToArray();
        if (materializedProjections.Any(projection => projection is null)
            || materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find projection facts cannot contain null values.");
        }

        Projections = Array.AsReadOnly(materializedProjections);
        Findings = Array.AsReadOnly(materializedFindings);
        Coverage = coverage;
    }

    internal IReadOnlyList<FindProjection> Projections { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal FindProjectionCoverageState Coverage { get; }
}
