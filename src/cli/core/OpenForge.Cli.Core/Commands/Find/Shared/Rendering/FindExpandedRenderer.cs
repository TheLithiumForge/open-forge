using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindExpandedRenderer
{
    internal static string Render(FindResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);

        var lines = new List<string>
        {
            $"Workspace: {Workspace(result.Workspace)}",
            $"Selected by: {SelectedBy(result.Workspace)}",
            string.Empty,
            "Filters:",
        };
        AddPredicates(lines, result.Query.Predicates);
        lines.Add($"Require: {Requirement(result.Query.Requirement)}");
        lines.Add("Within:");
        AddWithin(lines, result.Query.Within);
        lines.Add("Source universe:");
        lines.Add($"  Mode:       {UniverseMode(result.Universe.Mode)}");
        AddSelectors(lines, "Include", result.Universe.Include);
        AddSelectors(lines, "Exclude", result.Universe.Exclude);
        lines.Add($"  Candidates: {Count(result.Universe.CandidateCount)}");
        lines.Add($"  Inspected:  {Count(result.Universe.InspectedCount)}");
        lines.Add($"  Matched:    {Count(result.Universe.MatchedCount)}");
        lines.Add($"Coverage: {Coverage(result.Coverage.State)}");
        lines.Add($"Matching coverage: {Coverage(result.Coverage.Matching)}");
        lines.Add($"Projection coverage: {ProjectionCoverage(result.Coverage.Projection)}");
        AddFindings(lines, result);
        lines.Add($"Matches: {result.Matches.Count}");
        AddMatches(lines, result.Matches);
        if (ReadNextLine(result) is { } next)
        {
            lines.Add(next);
        }

        if (result.Presentation.Content.IsRequested)
        {
            AddProjectionBlocks(lines, result.Matches);
        }

        return string.Join(Environment.NewLine, lines);
    }

    internal static string? ReadNextLine(FindResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Next is null)
        {
            if (result.Status is CliSemanticStatus.Complete or CliSemanticStatus.Attention)
            {
                return null;
            }

            throw new InvalidOperationException("A non-terminal Find status requires a typed next action.");
        }

        return result.Status switch
        {
            CliSemanticStatus.Incomplete when IsCommand(result, "open-forge doctor")
                => "Next: open-forge doctor",
            CliSemanticStatus.Invalid when IsCommand(result, "open-forge find --help")
                => "Next: correct the named Find input.",
            CliSemanticStatus.Blocked when IsCommand(result, "open-forge find")
                => "Next: rerun with one listed exact path for each ambiguous selector.",
            CliSemanticStatus.Blocked when IsCommand(result, "open-forge doctor")
                => "Next: open-forge doctor",
            CliSemanticStatus.Failed when IsCommand(result, "open-forge find --verbose")
                => "Next: report the failure and retry with bounded diagnostics.",
            CliSemanticStatus.Interrupted when IsCommand(result, "open-forge find")
                => "Next: rerun the same request.",
            CliSemanticStatus.Complete or CliSemanticStatus.Attention
                => throw new InvalidOperationException("A complete Find result cannot carry a next action."),
            _ => throw new InvalidOperationException("The typed Find next action is not defined."),
        };
    }

    internal static void AddProjectionBlocks(
        ICollection<string> lines,
        IReadOnlyList<FindMatch> matches)
    {
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(matches);
        foreach (var match in matches)
        {
            if (match.Projections.Count == 0)
            {
                continue;
            }

            foreach (var projection in match.Projections)
            {
                AddProjection(lines, projection);
            }
        }
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
            lines.Add($"  {label}:    omitted");
            return;
        }

        foreach (var selector in selectors)
        {
            lines.Add($"  {label}:    {FindTextEscaping.Escape(selector.Value)}");
            lines.Add($"    Form: {SelectorForm(selector.Form)}");
            lines.Add($"    Resolution: {SelectorResolution(selector.Resolution)}");
            lines.Add($"    Identity: {Identity(selector.Identity)}");
            lines.Add($"    Source kind: {SourceKind(selector.SourceKind)}");
            lines.Add($"    Expansion: {SelectorExpansion(selector.Expansion)}");
            lines.Add($"    Candidates: {Candidates(selector.Candidates)}");
        }
    }

    private static void AddFindings(ICollection<string> lines, FindResult result)
    {
        lines.Add($"Findings: {result.Findings.Count}");
        for (var index = 0; index < result.Findings.Count; index++)
        {
            var finding = result.Findings[index];
            lines.Add($"Finding {index + 1}:");
            lines.Add($"  Code: {FindDefinitions.ReadFindingCode(finding.Code)}");
            lines.Add($"  Status: {Status(finding.Status)}");
            lines.Add($"  Subject: {Optional(finding.Subject)}");
            lines.Add($"  Cause: {FindTextEscaping.Escape(finding.Cause)}");
            lines.Add($"  Selector role: {SelectorRole(finding.SelectorRole)}");
            lines.Add($"  Selector occurrence: {Number(finding.SelectorOccurrence)}");
            lines.Add($"  Source: {Identity(finding.Source)}");
            lines.Add($"  Layer: {Layer(finding.Layer)}");
            lines.Add($"  Path: {Optional(finding.Path)}");
            lines.Add($"  Region: {Region(finding.Region)}");
            lines.Add($"  Location: {Location(finding.Location)}");
            lines.Add($"  Candidates: {Candidates(finding.Candidates)}");
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
            lines.Add($"  Description: {Optional(match.Description)}");
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

    private static void AddProjection(
        ICollection<string> lines,
        FindProjection projection)
    {
        lines.Add($"Projection: {ProjectionPart(projection)}");
        lines.Add($"  Layer: {Layer(projection.Layer)}");
        lines.Add($"  Path: {Optional(projection.Path)}");
        lines.Add($"  State: {ProjectionState(projection.State)}");
        if (projection.State != FindProjectionState.Available)
        {
            return;
        }

        if (projection.Part == FindContentPartKind.Metadata)
        {
            AddMetadata(lines, projection.Metadata);
        }
        else if (projection.Part == FindContentPartKind.Headings)
        {
            AddHeadings(lines, projection.Headings);
        }
        else
        {
            AddText(lines, projection);
        }
    }

    private static void AddMetadata(
        ICollection<string> lines,
        FindMetadata? metadata)
    {
        if (metadata is null)
        {
            throw new InvalidOperationException("An available Find metadata projection requires metadata facts.");
        }

        lines.Add("  Metadata:");
        lines.Add($"    Position: {metadata.Position.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        lines.Add($"    ID: {FindTextEscaping.Escape(metadata.Id)}");
        lines.Add($"    Base path: {FindTextEscaping.Escape(metadata.Path)}");
        lines.Add($"    Route state: {RouteState(metadata.RouteState)}");
        lines.Add($"    Route: {Optional(metadata.Route)}");
        lines.Add("    Layers:");
        foreach (var layer in metadata.Layers)
        {
            lines.Add(
                $"      {Layer(layer.Kind)} frontmatter: {FindTextEscaping.Escape(layer.Path)}");
        }
    }

    private static void AddHeadings(
        ICollection<string> lines,
        IReadOnlyList<FindProjectedHeading> headings)
    {
        lines.Add("  Headings:");
        if (headings.Count == 0)
        {
            lines.Add("    none");
            return;
        }

        foreach (var heading in headings)
        {
            lines.Add(
                $"    - {FindTextEscaping.Escape(heading.Text)} "
                + $"(level {heading.Level}, {HeadingForm(heading.Form)}, "
                + $"{Canonical(heading.Canonical)}, {Location(heading.Location)})");
        }
    }

    private static void AddText(
        ICollection<string> lines,
        FindProjection projection)
    {
        if (projection.Text is null || projection.Location is null)
        {
            throw new InvalidOperationException("An available Find text projection requires text and location facts.");
        }

        lines.Add("  Text:");
        lines.Add(string.Concat(
            "<<<",
            Environment.NewLine,
            projection.Text,
            Environment.NewLine,
            ">>>"));
        lines.Add($"  Location: {Location(projection.Location)}");
    }

    private static string Workspace(CliWorkspace? workspace)
        => workspace is null
            ? "none"
            : workspace.LexicalRoot;

    private static string SelectedBy(CliWorkspace? workspace)
        => workspace?.SelectedBy switch
        {
            null => "none",
            CliWorkspaceSelectionMethod.CurrentDirectory => "current directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "--workspace",
            _ => throw new ArgumentOutOfRangeException(
                nameof(workspace),
                workspace.SelectedBy,
                "The Find workspace selection method is not defined."),
        };

    private static string Requirement(FindRequirement requirement)
        => requirement switch
        {
            FindRequirement.All => FindDefinitions.All,
            FindRequirement.Any => FindDefinitions.Any,
            _ => throw new ArgumentOutOfRangeException(
                nameof(requirement),
                requirement,
                "The Find requirement is not defined."),
        };

    private static string Regions(IEnumerable<FindRegion> regions)
    {
        var values = regions
            .Select(region => FindTextEscaping.Escape(region.CanonicalValue))
            .ToArray();
        return values.Length == 0 ? "omitted" : string.Join(", ", values);
    }

    private static string Count(int? count)
        => count?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "not established";

    private static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    private static string Coverage(FindCoverageState state)
        => state switch
        {
            FindCoverageState.NotStarted => "not-started",
            FindCoverageState.Complete => "complete",
            FindCoverageState.Incomplete => "incomplete",
            FindCoverageState.Blocked => "blocked",
            FindCoverageState.Failed => "failed",
            FindCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined."),
        };

    private static string ProjectionCoverage(FindProjectionCoverageState state)
        => state switch
        {
            FindProjectionCoverageState.NotRequested => "not-requested",
            FindProjectionCoverageState.NotStarted => "not-started",
            FindProjectionCoverageState.Complete => "complete",
            FindProjectionCoverageState.Incomplete => "incomplete",
            FindProjectionCoverageState.Blocked => "blocked",
            FindProjectionCoverageState.Failed => "failed",
            FindProjectionCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection coverage state is not defined."),
        };

    private static string UniverseMode(FindUniverseMode mode)
        => mode switch
        {
            FindUniverseMode.Default => "default",
            FindUniverseMode.Filtered => "filtered",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Find universe mode is not defined."),
        };

    private static string PredicateKind(FindPredicateKind kind)
        => kind switch
        {
            FindPredicateKind.Tag => "Tag:",
            FindPredicateKind.Heading => "Heading:",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find predicate kind is not defined."),
        };

    private static string SelectorForm(SourceReferenceKind? form)
        => form switch
        {
            null => "none",
            SourceReferenceKind.SourceId => "id",
            SourceReferenceKind.SourcePath => "path",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The Find selector form is not defined."),
        };

    private static string SelectorResolution(FindSelectorResolution resolution)
        => resolution switch
        {
            FindSelectorResolution.Resolved => "resolved",
            FindSelectorResolution.Invalid => "invalid",
            FindSelectorResolution.Unknown => "unknown",
            FindSelectorResolution.Unsupported => "unsupported",
            FindSelectorResolution.Ambiguous => "ambiguous",
            FindSelectorResolution.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution,
                "The Find selector resolution is not defined."),
        };

    private static string SourceKind(FindSourceKind? kind)
        => kind switch
        {
            null => "none",
            FindSourceKind.Loader => "loader",
            FindSourceKind.Entrypoint => "entrypoint",
            FindSourceKind.Skill => "skill",
            FindSourceKind.Ordinary => "ordinary",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find source kind is not defined."),
        };

    private static string SelectorExpansion(FindSelectorExpansion? expansion)
        => expansion switch
        {
            null => "none",
            FindSelectorExpansion.Folder => "folder",
            FindSelectorExpansion.Source => "source",
            _ => throw new ArgumentOutOfRangeException(
                nameof(expansion),
                expansion,
                "The Find selector expansion is not defined."),
        };

    private static string Identity(FindSourceIdentity? identity)
        => identity is null
            ? "none"
            : $"{FindTextEscaping.Escape(identity.Id)} -> {FindTextEscaping.Escape(identity.Path)}";

    private static string Candidates(IEnumerable<FindSourceIdentity> candidates)
    {
        var values = candidates
            .Select(candidate => Identity(candidate))
            .ToArray();
        return values.Length == 0 ? "none" : $"[{string.Join(", ", values)}]";
    }

    private static string SelectorRole(FindSelectorRole? role)
        => role switch
        {
            null => "none",
            FindSelectorRole.Include => "include",
            FindSelectorRole.Exclude => "exclude",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The Find selector role is not defined."),
        };

    private static string Number(int? value)
        => value?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "none";

    private static string Layer(SourceLayerKind? layer)
        => layer switch
        {
            null => "none",
            SourceLayerKind.Base => "base",
            SourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    private static string Region(FindRegion? region)
        => region is null ? "none" : FindTextEscaping.Escape(region.CanonicalValue);

    private static string Optional(string? value)
        => value is null ? "none" : FindTextEscaping.Escape(value);

    private static string Location(FindSourceLocation? location)
        => location is null
            ? "none"
            : $"line {location.Line}, column {location.Column}, byte {location.ByteOffset}, length {location.ByteLength}";

    private static string ProjectionPart(FindProjection projection)
        => projection.Part switch
        {
            FindContentPartKind.Metadata => FindDefinitions.Metadata,
            FindContentPartKind.Frontmatter => FindDefinitions.Frontmatter,
            FindContentPartKind.Headings => FindDefinitions.Headings,
            FindContentPartKind.Body => FindDefinitions.Body,
            FindContentPartKind.Section when projection.Name is not null
                => $"{FindDefinitions.SectionPrefix}{FindTextEscaping.Escape(projection.Name)}",
            FindContentPartKind.Section => throw new InvalidOperationException(
                "A Find section projection requires a typed section name."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(projection),
                projection.Part,
                "The Find projection part is not defined."),
        };

    private static string ProjectionState(FindProjectionState state)
        => state switch
        {
            FindProjectionState.Available => "available",
            FindProjectionState.Missing => "missing",
            FindProjectionState.Unavailable => "unavailable",
            FindProjectionState.Ambiguous => "ambiguous",
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The Find projection state is not defined."),
        };

    private static string RouteState(FindRouteState state)
        => state switch
        {
            FindRouteState.Routed => "routed",
            FindRouteState.Unrouted => "unrouted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find route state is not defined."),
        };

    private static string HeadingForm(MarkdownHeadingForm form)
        => form switch
        {
            MarkdownHeadingForm.Atx => "atx",
            MarkdownHeadingForm.Setext => "setext",
            _ => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The Markdown heading form is not defined."),
        };

    private static string Canonical(bool canonical)
        => canonical ? "canonical" : "noncanonical";

    private static bool IsCommand(FindResult result, string command)
        => result.Next is not null
            && string.Equals(result.Next.Command, command, StringComparison.Ordinal);
}
