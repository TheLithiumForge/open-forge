using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;
using static OpenForge.Cli.Core.Commands.Find.Shared.Rendering.FindHumanValues;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindExpandedRenderer
{
    internal static string Render(FindResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);
        var noun = result.Matches.Count == 1 ? "source" : "sources";
        var lines = new List<string>
        {
            $"Found {result.Matches.Count} matching {noun}.",
            $"Status: {CliHumanText.Status(result.Status)}",
            $"Workspace: {FindTextEscaping.Escape(Workspace(result.Workspace))}",
            $"Selected by: {SelectedBy(result.Workspace)}",
            $"Coverage: {Coverage(result.Coverage.State)}",
        };
        if (result.Coverage.Matching != result.Coverage.State)
        {
            lines.Add($"Matching coverage: {Coverage(result.Coverage.Matching)}");
        }
        if (result.Presentation.Content.IsRequested)
        {
            lines.Add($"Projection coverage: {ProjectionCoverage(result.Coverage.Projection)}");
        }
        FindFindingHumanRenderer.Add(lines, result);
        lines.Add(string.Empty);
        AddMatches(lines, result.Matches);
        if (result.Matches.Count == 0 && result.Coverage.State != FindCoverageState.Complete)
        {
            lines.Add("No matches established; the search is not complete.");
        }
        lines.Add(string.Empty);
        lines.Add("Search details:");
        lines.Add("Filters:");
        AddPredicates(lines, result.Query.Predicates);
        lines.Add($"Require: {Requirement(result.Query.Requirement)}");
        lines.Add("Within:");
        AddWithin(lines, result.Query.Within);
        lines.Add("Source universe:");
        lines.Add($"  Mode:       {UniverseMode(result.Universe.Mode)}");
        AddSelectors(lines, "Include", result.Universe.Include);
        AddSelectors(lines, "Exclude", result.Universe.Exclude);
        lines.Add($"  Inspected: {Count(result.Universe.InspectedCount)} of {Count(result.Universe.CandidateCount)} candidates");
        if (result.Universe.MatchedCount != result.Matches.Count)
        {
            lines.Add($"  Matched count: {Count(result.Universe.MatchedCount)}");
        }
        if (FindNextHumanRenderer.Line(result) is { } next)
        {
            lines.Add(next);
            lines.Add(FindTextEscaping.Escape(result.Next?.Reason ?? string.Empty));
        }
        if (result.Presentation.Content.IsRequested)
        {
            FindContentHumanRenderer.AddProjectionBlocks(lines, result.Matches);
        }
        return string.Join(Environment.NewLine, lines);
    }

    private static void AddPredicates(
        ICollection<string> lines,
        IReadOnlyList<FindPredicate> predicates)
    {
        if (predicates.Count == 0)
        {
            lines.Add("  none");
            return;
        }

        foreach (var predicate in predicates)
        {
            lines.Add($"  {PredicateKind(predicate.Kind),-8} {FindTextEscaping.Escape(predicate.SuppliedValue)}");
        }
    }

    private static void AddWithin(
        ICollection<string> lines,
        FindRegionSelection within)
    {
        lines.Add($"  Supplied: {Regions(within.Supplied)}");
        lines.Add($"  Tag:     {Regions(within.Tag)}");
        lines.Add($"  Heading: {Regions(within.Heading)}");
    }

    private static void AddSelectors(
        ICollection<string> lines,
        string label,
        IReadOnlyList<FindSelector> selectors)
    {
        if (selectors.Count == 0)
        {
            return;
        }

        foreach (var selector in selectors)
        {
            lines.Add($"  {label}:    {FindTextEscaping.Escape(selector.Value)}");
            lines.Add($"    Form: {SelectorForm(selector.Form)}");
            lines.Add($"    Resolution: {SelectorResolution(selector.Resolution)}");
            if (selector.Identity is not null)
            {
                lines.Add($"    Identity: {Identity(selector.Identity)}");
            }
            if (selector.SourceKind is not null)
            {
                lines.Add($"    Source kind: {SourceKind(selector.SourceKind)}");
            }
            if (selector.Expansion is not null)
            {
                lines.Add($"    Expansion: {SelectorExpansion(selector.Expansion)}");
            }
            if (selector.Candidates.Count != 0)
            {
                lines.Add($"    Candidates: {Candidates(selector.Candidates)}");
            }
        }
    }

    private static void AddMatches(
        ICollection<string> lines,
        IReadOnlyList<FindMatch> matches)
    {
        foreach (var match in matches)
        {
            lines.Add(FindTextEscaping.Escape(match.Id));
            lines.Add($"  Path: {FindTextEscaping.Escape(match.Path)}");
            if (match.Description is { } description)
            {
                lines.Add($"  Description: {FindTextEscaping.Escape(description)}");
            }
            lines.Add("  Matched:");
            if (match.Evidence.Count == 0)
            {
                lines.Add("    none");
                continue;
            }

            foreach (var evidence in match.Evidence)
            {
                lines.Add($"    {EvidenceLine(evidence)}");
            }
        }
    }

    private static string EvidenceLine(FindEvidence evidence)
    {
        var region = evidence.Kind == FindPredicateKind.Heading
            ? "heading"
            : Region(evidence.Region);
        var line = $"{FindTextEscaping.Escape(evidence.Authored)} — {region}, {Layer(evidence.Layer)}";
        return evidence.Kind == FindPredicateKind.Heading
            ? $"{line}, line {evidence.Location.Line}"
            : line;
    }

}
