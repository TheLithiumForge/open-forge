using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Documents;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Find.Models.Matching;

internal sealed record FindBodyTagOccurrence
{
    internal FindBodyTagOccurrence(string authored, SourceLocation location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authored);
        ArgumentNullException.ThrowIfNull(location);
        Authored = authored;
        Location = location;
    }

    internal string Authored { get; }

    internal SourceLocation Location { get; }
}

internal sealed record FindBodyTagInput
{
    internal FindBodyTagInput(MarkdownDocumentFacts document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Document = document;
    }

    internal MarkdownDocumentFacts Document { get; }
}

internal enum FindBodyTagAvailability
{
    Complete,
    Unavailable,
}

internal sealed record FindBodyTagFacts
{
    internal FindBodyTagFacts(
        FindBodyTagAvailability availability,
        IEnumerable<FindBodyTagOccurrence> occurrences)
    {
        if (!Enum.IsDefined(availability))
        {
            throw new ArgumentOutOfRangeException(nameof(availability), availability, "The Find body-tag availability is not defined.");
        }

        ArgumentNullException.ThrowIfNull(occurrences);
        var materializedOccurrences = occurrences.ToArray();
        if (materializedOccurrences.Any(occurrence => occurrence is null))
        {
            throw new ArgumentException("Find body-tag occurrences cannot contain null values.", nameof(occurrences));
        }

        if (availability == FindBodyTagAvailability.Unavailable && materializedOccurrences.Length != 0)
        {
            throw new ArgumentException("Unavailable Find body-tag facts cannot carry occurrences.", nameof(occurrences));
        }

        Availability = availability;
        Occurrences = Array.AsReadOnly(materializedOccurrences);
    }

    internal FindBodyTagAvailability Availability { get; }

    internal IReadOnlyList<FindBodyTagOccurrence> Occurrences { get; }
}

internal sealed record FindMatchingInput
{
    internal FindMatchingInput(
        FindRequest request,
        FindUniverse universe,
        IEnumerable<FindLayerInspectionFacts> inspections)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(universe);
        ArgumentNullException.ThrowIfNull(inspections);
        var materializedInspections = inspections.ToArray();
        if (materializedInspections.Any(inspection => inspection is null))
        {
            throw new ArgumentException("Find matching inspections cannot contain null values.", nameof(inspections));
        }

        Request = request;
        Universe = universe;
        Inspections = Array.AsReadOnly(materializedInspections);
    }

    internal FindRequest Request { get; }

    internal FindUniverse Universe { get; }

    internal IReadOnlyList<FindLayerInspectionFacts> Inspections { get; }
}

internal sealed record FindMatchingFacts
{
    internal FindMatchingFacts(
        IEnumerable<FindMatch> matches,
        IEnumerable<FindFinding> findings,
        FindCoverageState coverage)
    {
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(findings);
        if (!Enum.IsDefined(coverage))
        {
            throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Find matching coverage state is not defined.");
        }

        var materializedMatches = matches.ToArray();
        var materializedFindings = findings.ToArray();
        if (materializedMatches.Any(match => match is null)
            || materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find matching facts cannot contain null values.");
        }

        Matches = Array.AsReadOnly(materializedMatches);
        Findings = Array.AsReadOnly(materializedFindings);
        Coverage = coverage;
    }

    internal IReadOnlyList<FindMatch> Matches { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal FindCoverageState Coverage { get; }
}
