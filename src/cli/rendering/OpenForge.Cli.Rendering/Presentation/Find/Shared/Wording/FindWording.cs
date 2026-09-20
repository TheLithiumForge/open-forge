using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.Presentation.Find.Shared.Wording;

internal static class FindWording
{
    internal static string Completed(long count, string query)
    {
        var noun = count == 1 ? global::OpenForge.Cli.OutputText.Find.FindText.LabelSource() : global::OpenForge.Cli.OutputText.Shared.SharedText.LabelSources();
        var verb = count == 1 ? global::OpenForge.Cli.OutputText.Find.FindText.LabelMatches() : global::OpenForge.Cli.OutputText.Find.FindText.LabelMatch();
        return string.IsNullOrEmpty(query)
            ? FormattableString.Invariant($"{count} {noun} {verb}.")
            : FormattableString.Invariant($"{count} {noun} {verb} {query}.");
    }

    internal static string NoMatches(string query, bool bareInventory)
        => bareInventory
            ? global::OpenForge.Cli.OutputText.Find.FindText.MessageNoMarkdownSourcesWereFoundUnderAgents()
            : global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatNoSourcesMatch($"{query}");

    internal static string Incomplete() => global::OpenForge.Cli.OutputText.Find.FindText.MessageTheSearchIsIncomplete();

    internal static string CannotSearch(string problem)
        => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatCannotSearch($"{problem.TrimEnd('.')}");

    internal static string Failed(string reason)
        => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatFindStoppedBecauseOfAnUnexpectedError($"{reason.TrimEnd('.')}");

    internal static string Cancelled() => global::OpenForge.Cli.OutputText.Find.FindText.MessageFindWasCancelled();

    internal static string FindingCode(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => "find.invalid-input",
            FindFindingCode.InvalidSelector => "find.invalid-selector",
            FindFindingCode.WorkspaceUnavailable => "find.workspace-unavailable",
            FindFindingCode.WorkspaceUnsafe => "find.workspace-unsafe",
            FindFindingCode.SelectorAmbiguous => "find.selector-ambiguous",
            FindFindingCode.SelectorUnsafe => "find.selector-unsafe",
            FindFindingCode.IdentityCollision => "find.identity-collision",
            FindFindingCode.CandidateUnsafe => "find.candidate-unsafe",
            FindFindingCode.LayerUnresolved => "find.layer-unresolved",
            FindFindingCode.InspectionUnavailable => "find.inspection-unavailable",
            FindFindingCode.InvalidEncoding => "find.invalid-encoding",
            FindFindingCode.FrontmatterUnavailable => "find.frontmatter-unavailable",
            FindFindingCode.SectionAmbiguous => "find.section-ambiguous",
            FindFindingCode.ProjectionMissing => "find.projection-missing",
            FindFindingCode.ProjectionUnavailable => "find.projection-unavailable",
            FindFindingCode.OperationFailed => "find.operation-failed",
            FindFindingCode.Interrupted => "find.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };

    internal static string FindingTitle(FindFindingCode code)
        => code switch
        {
            FindFindingCode.InvalidInput => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleInvalidInput(),
            FindFindingCode.InvalidSelector => global::OpenForge.Cli.OutputText.Find.FindText.TitleInvalidSelector(),
            FindFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
            FindFindingCode.WorkspaceUnsafe => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnsafe(),
            FindFindingCode.SelectorAmbiguous => global::OpenForge.Cli.OutputText.Find.FindText.TitleSelectorIsAmbiguous(),
            FindFindingCode.SelectorUnsafe => global::OpenForge.Cli.OutputText.Find.FindText.TitleSelectorIsUnsafe(),
            FindFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIdentityCollides(),
            FindFindingCode.CandidateUnsafe => global::OpenForge.Cli.OutputText.Find.FindText.TitleSourceCandidateIsUnsafe(),
            FindFindingCode.LayerUnresolved => global::OpenForge.Cli.OutputText.Find.FindText.TitleOverwriteHasNoBaseSource(),
            FindFindingCode.InspectionUnavailable => global::OpenForge.Cli.OutputText.Find.FindText.TitleSourceCouldNotBeInspected(),
            FindFindingCode.InvalidEncoding => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceEncodingIsInvalid(),
            FindFindingCode.FrontmatterUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleFrontmatterCouldNotBeRead(),
            FindFindingCode.SectionAmbiguous => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSectionIsAmbiguous(),
            FindFindingCode.ProjectionMissing => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSectionIsMissing(),
            FindFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleContentPartIsUnavailable(),
            FindFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Find.FindText.TitleFindFailed(),
            FindFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Find.FindText.TitleFindWasCancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Find finding code is not defined."),
        };

    internal static string FindingMessage(FindFinding finding)
    {
        ArgumentNullException.ThrowIfNull(finding);
        var path = finding.Path ?? finding.Source?.Path ?? finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheSource();
        var reason = finding.Cause.TrimEnd('.');
        return finding.Code switch
        {
            FindFindingCode.InvalidInput => InvalidInputProblem(finding.Cause),
            FindFindingCode.InvalidSelector => InvalidSelector(finding),
            FindFindingCode.WorkspaceUnavailable => CliFindingWording.WorkspaceUnavailable(path),
            FindFindingCode.WorkspaceUnsafe => CliFindingWording.WorkspaceUnsafe(path, reason),
            FindFindingCode.SelectorAmbiguous => CliFindingWording.SelectorAmbiguous(finding.Subject ?? global::OpenForge.Cli.OutputText.Find.FindText.LabelTheSelector()),
            FindFindingCode.SelectorUnsafe => CliFindingWording.SelectorUnsafe(finding.Subject ?? global::OpenForge.Cli.OutputText.Find.FindText.LabelTheSelector()),
            FindFindingCode.IdentityCollision => CliFindingWording.IdentityCollision(finding.Subject ?? global::OpenForge.Cli.OutputText.Shared.SharedText.LabelTheRequestedId()),
            FindFindingCode.CandidateUnsafe => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatCouldNotBeCheckedSafelyAndWasSkipped($"{path}"),
            FindFindingCode.LayerUnresolved => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatHasNoBaseFileAndWasSkipped($"{finding.Subject ?? path}"),
            FindFindingCode.InspectionUnavailable => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatCouldNotBeReadAndWasSkipped($"{path}"),
            FindFindingCode.InvalidEncoding => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatIsNotValidUtf8AndWasSkipped($"{path}"),
            FindFindingCode.FrontmatterUnavailable => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatTheFrontmatterOfCouldNotBeReadSoItsTagsWereNotMatched($"{path}"),
            FindFindingCode.SectionAmbiguous => global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatHasMoreThanOneSectionNamedNoneWasReturned($"{path}", $"{finding.Region?.Name ?? finding.Subject ?? "the requested section"}"),
            FindFindingCode.ProjectionMissing => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatHasNoSectionNamed($"{path}", $"{finding.Region?.Name ?? finding.Subject ?? "the requested section"}"),
            FindFindingCode.ProjectionUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheOfCouldNotBeProduced($"{Part(finding)}", $"{path}", $"{reason}"),
            FindFindingCode.OperationFailed => CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Find.FindText.TitleFind(), reason),
            FindFindingCode.Interrupted => Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Find finding code is not defined."),
        };
    }

    internal static string Query(FindQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var values = query.Predicates.Select(predicate =>
                $"--{PredicateOption(predicate.Kind)} {predicate.SuppliedValue}")
            .ToList();
        if (query.Requirement == FindRequirement.Any
            || query.Predicates.Count > 1)
        {
            values.Add($"--require {Requirement(query.Requirement)}");
        }

        if (query.Within.Supplied.Count > 0)
        {
            values.Add($"--within {string.Join(',', query.Within.Supplied.Select(region => region.CanonicalValue))}");
        }

        return string.Join(' ', values);
    }

    internal static string PredicateKind(FindPredicateKind kind)
        => kind switch
        {
            FindPredicateKind.Tag => "tag",
            FindPredicateKind.Heading => "heading",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Find predicate kind is not defined."),
        };

    internal static string PredicateOption(FindPredicateKind kind)
        => PredicateKind(kind);

    internal static string Requirement(FindRequirement requirement)
        => requirement switch
        {
            FindRequirement.All => "all",
            FindRequirement.Any => "any",
            _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, "The Find requirement is not defined."),
        };

    internal static string Region(FindRegion region) => region.CanonicalValue;

    internal static string Part(FindFinding finding)
        => finding.Region?.CanonicalValue
            ?? finding.Subject
            ?? global::OpenForge.Cli.OutputText.Find.FindText.LabelContent();

    internal static string NextInvalidSelector() => global::OpenForge.Cli.OutputText.Find.FindText.MessageListSourceIdsAndExactPathsThenRerunFind();

    internal static string NextIncomplete() => global::OpenForge.Cli.OutputText.Find.FindText.MessageInspectTheUnavailableSourceOrProjectionFactsBeforeRelyingOnThisFindResult();

    internal static string Layer(Enum layer)
        => layer.ToString() switch
        {
            "Base" => "base",
            "Overwrite" => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The Find layer kind is not defined."),
        };

    internal static string ProjectionState(FindProjectionState state)
        => state switch
        {
            FindProjectionState.Available => "available",
            FindProjectionState.Missing => "missing",
            FindProjectionState.Unavailable => "unavailable",
            FindProjectionState.Ambiguous => "ambiguous",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find projection state is not defined."),
        };

    internal static string ProjectionPart(FindProjection projection)
        => projection.Part switch
        {
            FindContentPartKind.Metadata => "metadata",
            FindContentPartKind.Frontmatter => "frontmatter",
            FindContentPartKind.Headings => "headings",
            FindContentPartKind.Body => "body",
            FindContentPartKind.Section => $"section:{projection.Name}",
            _ => throw new ArgumentOutOfRangeException(nameof(projection), projection.Part, "The Find projection part is not defined."),
        };

    internal static string EvidenceKind(FindPredicateKind kind) => PredicateKind(kind);

    internal static string Coverage(FindCoverageState state)
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

    internal static string ProjectionCoverage(FindProjectionCoverageState state)
        => state switch
        {
            FindProjectionCoverageState.NotRequested => "not run",
            FindProjectionCoverageState.NotStarted => "not-started",
            FindProjectionCoverageState.Complete => "complete",
            FindProjectionCoverageState.Incomplete => "incomplete",
            FindProjectionCoverageState.Blocked => "blocked",
            FindProjectionCoverageState.Failed => "failed",
            FindProjectionCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Find projection coverage state is not defined."),
        };

    private static string InvalidInputProblem(string cause)
    {
        var problem = cause.TrimEnd('.');
        if (problem.StartsWith("--require accepts only all or any", StringComparison.Ordinal))
        {
            return global::OpenForge.Cli.OutputText.Find.FindText.MessageRequireMustBeAllOrAny();
        }

        if (problem.StartsWith("Unknown Find region '", StringComparison.Ordinal)
            && problem.EndsWith("'", StringComparison.Ordinal))
        {
            var value = problem["Unknown Find region '".Length..^1];
            return global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatWithinIsNotAKnownRegion($"{value}");
        }

        return cause;
    }

    private static string InvalidSelector(FindFinding finding)
    {
        var option = finding.SelectorRole switch
        {
            FindSelectorRole.Include => "--include",
            FindSelectorRole.Exclude => "--exclude",
            _ => global::OpenForge.Cli.OutputText.Find.FindText.LabelTheSelector(),
        };
        return global::OpenForge.Cli.OutputText.Find.FindPhrases.FormatIsNotASourceIdOrAPathUnderAgents($"{option}", $"{finding.Subject ?? "<value>"}");
    }
}
