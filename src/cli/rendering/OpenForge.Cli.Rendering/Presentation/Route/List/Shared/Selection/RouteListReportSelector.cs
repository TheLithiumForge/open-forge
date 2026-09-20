using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.List.Models;
using OpenForge.Cli.Core.Presentation.Route.List.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Selection;

internal static class RouteListReportSelector
{
    internal static CliReport<RouteListData> Select(RouteListResult result, CliSelection selection)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selection);
        var facts = result.PresentationFacts;
        var rows = facts.Rows.Select(ProjectRow).ToArray();
        var findings = facts.Findings.Select(finding => Finding(facts, finding)).ToArray();
        var requestedDepth = facts.RequestedDepth;
        var trailer = requestedDepth is { Kind: RouteListPresentationDepthKind.Finite }
            && facts.Rows.Any(row => row.Kind == RouteListPresentationRowKind.Entrypoint
                && row.RelativeDepth == requestedDepth.FiniteValue
                && row.DirectChildCount is > 0)
            ? RouteListWording.DepthTrailer(facts.Rows.Count, requestedDepth)
            : null;
        return new CliReport<RouteListData>
        {
            Command = facts.Command,
            Status = facts.Status,
            Headline = Headline(facts),
            HeadlineFindingCode = HeadlineFindingCode(facts),
            Workspace = facts.WorkspacePath is { } workspace
                ? new CliWorkspaceEcho(workspace, facts.WorkspaceExplicit)
                : null,
            Findings = findings,
            Effects = [],
            Counts = Counts(facts),
            Limitations = [],
            Data = new RouteListData
            {
                Subject = Subject(facts.Selection),
                Depth = requestedDepth is { } depth ? new RouteListDataDepth { Value = depth } : null,
                Rows = rows,
                ShowPaths = selection.Detail >= CliDetail.Standard,
                ShowDetails = selection.Detail >= CliDetail.Full,
                ShowTrailer = trailer is not null,
                Trailer = trailer,
                SeparateRows = facts.Findings.Any(finding => finding.Status == CliSemanticStatus.Attention
                    || finding.Status == CliSemanticStatus.Incomplete) && rows.Length > 0,
            },
            Recovery = null,
            Next = Next(facts),
            Diagnostics = selection.Detail == CliDetail.Debug ? Diagnostics(facts) : [],
        };
    }

    private static RouteListDataRow ProjectRow(RouteListPresentationRow row)
        => new()
        {
            Id = row.Id,
            Path = row.Path,
            Description = row.Description,
            Tags = row.Tags.ToArray(),
            RelativeDepth = row.RelativeDepth,
            Result = row,
        };

    private static RouteListDataSubject? Subject(RouteListPresentationSelection selection)
        => selection.ResolvedId is { } id && selection.ResolvedPath is { } path
            ? new RouteListDataSubject { Id = id, Path = path }
            : null;

    private static CliHeadline Headline(RouteListPresentationFacts result)
    {
        var first = result.Findings.FirstOrDefault(finding => finding.Status == result.Status)
            ?? result.Findings.FirstOrDefault();
        return result.Status switch
        {
            CliSemanticStatus.Complete when result.Rows.Count == 0
                => new(RouteListWording.NoRoutes(result.Selection.ResolvedId), CliHeadlineKind.NothingToDo),
            CliSemanticStatus.Complete
                => new(RouteListWording.Listed(result.Rows.Count), CliHeadlineKind.Done),
            CliSemanticStatus.Attention
                => new(RouteListWording.ListedWithWarnings(result.Rows.Count), CliHeadlineKind.Warnings),
            CliSemanticStatus.Incomplete
                => new(RouteListWording.Incomplete(), CliHeadlineKind.Incomplete),
            CliSemanticStatus.Invalid or CliSemanticStatus.Blocked
                => new(RouteListWording.CannotList(Message(result, first)),
                    result.Status == CliSemanticStatus.Invalid ? CliHeadlineKind.CannotStart : CliHeadlineKind.Blocked),
            CliSemanticStatus.Failed
                => new(RouteListWording.Failed(first?.Cause ?? global::OpenForge.Cli.OutputText.Shared.SharedText.TitleTheResultDidNotContainAFinding()), CliHeadlineKind.Failed),
            CliSemanticStatus.Interrupted
                => new(RouteListWording.Cancelled(), CliHeadlineKind.Cancelled),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "The Route List status is not defined."),
        };
    }

    private static string? HeadlineFindingCode(RouteListPresentationFacts result)
    {
        if (result.Status is not (CliSemanticStatus.Invalid
            or CliSemanticStatus.Blocked
            or CliSemanticStatus.Failed
            or CliSemanticStatus.Interrupted))
        {
            return null;
        }

        return result.Findings.FirstOrDefault(finding => finding.Status == result.Status)?.MachineCode
            ?? result.Findings.FirstOrDefault()?.MachineCode;
    }

    private static IReadOnlyList<CliCount> Counts(RouteListPresentationFacts result)
        =>
        [
            new CliCount("routes", global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelRoutes(), result.Rows.Count),
            new CliCount("roots", global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelRoots(), result.Coverage.SelectedRootCount),
            new CliCount(
                "depth",
                global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelDepth(),
                result.RequestedDepth is { Kind: RouteListPresentationDepthKind.Finite } depth
                    ? depth.FiniteValue
                    : null),
        ];

    private static CliFinding Finding(RouteListPresentationFacts result, RouteListPresentationFinding finding)
    {
        var subject = Subject(finding);
        var message = Message(result, finding);
        var candidates = finding.CandidatePaths.Select(path => new CliCandidate(
            new CliSubject(CliSubjectKind.Source, Path: path, Id: path),
            [global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.LabelMatchesTheRequestedSourceReference()])).ToArray();
        return new CliFinding
        {
            Severity = CliReportVocabulary.Severity(finding.Status),
            Code = finding.MachineCode,
            Title = Title(finding.Code),
            Message = message,
            Subject = subject,
            Candidates = candidates,
        };
    }

    private static CliSubject Subject(RouteListPresentationFinding finding)
    {
        return finding.Code switch
        {
            RouteListPresentationFindingCode.InvalidDepth
                or RouteListPresentationFindingCode.InvalidSourceReference
                or RouteListPresentationFindingCode.UnknownSource
                or RouteListPresentationFindingCode.LoaderSubject
                => new CliSubject(CliSubjectKind.Identifier, Id: finding.Subject ?? "route list"),
            RouteListPresentationFindingCode.InvalidWorkspace
                or RouteListPresentationFindingCode.WorkspaceUnavailable
                => new CliSubject(CliSubjectKind.Workspace, Path: finding.Subject, Id: finding.Subject ?? "workspace"),
            RouteListPresentationFindingCode.OperationFailed
                or RouteListPresentationFindingCode.Interrupted
                => new CliSubject(CliSubjectKind.Identifier, Id: finding.Subject ?? "route list"),
            _ => new CliSubject(CliSubjectKind.Source, Path: finding.Subject, Id: finding.Subject ?? "route list"),
        };
    }

    private static string Title(RouteListPresentationFindingCode code) => code switch
    {
        RouteListPresentationFindingCode.InvalidDepth => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleInvalidDepth(),
        RouteListPresentationFindingCode.InvalidSourceReference => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleInvalidSourceReference(),
        RouteListPresentationFindingCode.InvalidWorkspace => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        RouteListPresentationFindingCode.WorkspaceUnavailable => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleWorkspaceIsUnavailable(),
        RouteListPresentationFindingCode.UnknownSource => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleSourceWasNotFound(),
        RouteListPresentationFindingCode.UnsupportedSource => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleSourceIsUnsupported(),
        RouteListPresentationFindingCode.LoaderSubject => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleTheLoaderIsNotARoute(),
        RouteListPresentationFindingCode.AmbiguousSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsAmbiguous(),
        RouteListPresentationFindingCode.UnsafeSource => global::OpenForge.Cli.OutputText.Shared.SharedText.TitleSourceIsUnsafe(),
        RouteListPresentationFindingCode.LoaderUnavailable => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleLoaderIsUnavailable(),
        RouteListPresentationFindingCode.LoaderMalformed => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleLoaderIsMalformed(),
        RouteListPresentationFindingCode.RouteAmbiguous => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleRouteIsAmbiguous(),
        RouteListPresentationFindingCode.MetadataMissing => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleDescriptionIsMissing(),
        RouteListPresentationFindingCode.MetadataMalformed => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleSourceMetadataIsMalformed(),
        RouteListPresentationFindingCode.AuthoredForm => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleCompatibilityEntrypoint(),
        RouteListPresentationFindingCode.IdentityCollision => global::OpenForge.Cli.OutputText.Route.Shared.RouteSharedText.TitleIdentityCollision(),
        RouteListPresentationFindingCode.ReadUnavailable => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleRouteBoundaryIsUnreadable(),
        RouteListPresentationFindingCode.PhysicalBoundary => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleRouteBoundaryIsOutsideTheWorkspace(),
        RouteListPresentationFindingCode.OperationFailed => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleRouteListFailed(),
        RouteListPresentationFindingCode.Interrupted => global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleRouteListWasCancelled(),
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Route List finding code is not defined."),
    };

    private static string Message(RouteListPresentationFacts result, RouteListPresentationFinding? finding)
        => finding is null ? global::OpenForge.Cli.OutputText.Shared.SharedText.MessageTheResultDidNotContainAFinding() : MessageCore(result, finding);

    private static string MessageCore(RouteListPresentationFacts result, RouteListPresentationFinding finding)
    {
        var subject = finding.Subject ?? global::OpenForge.Cli.OutputText.Route.List.RouteListText.LabelTheSuppliedSource();
        return finding.Code switch
        {
            RouteListPresentationFindingCode.InvalidDepth => RouteListWording.InvalidDepth(),
            RouteListPresentationFindingCode.InvalidSourceReference => RouteListWording.InvalidSourceReference(subject),
            RouteListPresentationFindingCode.InvalidWorkspace or RouteListPresentationFindingCode.WorkspaceUnavailable
                => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.WorkspaceUnavailable(subject),
            RouteListPresentationFindingCode.UnknownSource => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.UnknownSource(subject),
            RouteListPresentationFindingCode.UnsupportedSource => RouteListWording.UnsupportedSource(subject),
            RouteListPresentationFindingCode.LoaderSubject => RouteListWording.LoaderSubject(),
            RouteListPresentationFindingCode.AmbiguousSource => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.SourceAmbiguous(subject),
            RouteListPresentationFindingCode.UnsafeSource => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.SourceUnsafe(subject),
            RouteListPresentationFindingCode.LoaderUnavailable => RouteListWording.LoaderUnavailable(),
            RouteListPresentationFindingCode.LoaderMalformed => RouteListWording.LoaderMalformed(),
            RouteListPresentationFindingCode.RouteAmbiguous => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.RouteAmbiguous(FindingId(result, finding)),
            RouteListPresentationFindingCode.MetadataMissing => RouteListWording.MetadataMissing(subject),
            RouteListPresentationFindingCode.MetadataMalformed => RouteListWording.MetadataMalformed(subject, finding.Cause),
            RouteListPresentationFindingCode.AuthoredForm => RouteListWording.AuthoredForm(subject, CompatibilityName(subject)),
            RouteListPresentationFindingCode.IdentityCollision => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.IdentityCollision(FindingId(result, finding)),
            RouteListPresentationFindingCode.ReadUnavailable => RouteListWording.ReadUnavailable(subject),
            RouteListPresentationFindingCode.PhysicalBoundary => RouteListWording.PhysicalBoundary(subject),
            RouteListPresentationFindingCode.OperationFailed => OpenForge.Cli.Core.Presentation.Shared.Wording.CliFindingWording.OperationFailed(global::OpenForge.Cli.OutputText.Route.List.RouteListText.TitleRouteList(), TrimSentence(finding.Cause)),
            RouteListPresentationFindingCode.Interrupted => RouteListWording.Cancelled(),
            _ => throw new ArgumentOutOfRangeException(nameof(finding), finding.Code, "The Route List finding code is not defined."),
        };
    }

    private static string FindingId(RouteListPresentationFacts result, RouteListPresentationFinding finding)
        => result.Rows.FirstOrDefault(row => string.Equals(row.Path, finding.Subject, StringComparison.Ordinal))?.Id
            ?? result.Selection.ResolvedId
            ?? finding.Subject
            ?? "the supplied source";

    private static string CompatibilityName(string path)
    {
        var normalized = path.Replace('\\', '/');
        var slash = normalized.LastIndexOf('/');
        return normalized[(slash + 1)..];
    }

    private static string TrimSentence(string value) => value.Trim().TrimEnd('.');

    private static CliNextAction? Next(RouteListPresentationFacts result)
    {
        var first = result.Findings.FirstOrDefault();
        if (first is null)
        {
            return null;
        }

        return first.Code switch
        {
            RouteListPresentationFindingCode.InvalidSourceReference
                or RouteListPresentationFindingCode.UnknownSource
                => new CliNextAction(
                    "open-forge route list --depth=all",
                    RouteListWording.NextReason("open-forge route list --depth=all")),
            RouteListPresentationFindingCode.LoaderSubject
                => new CliNextAction(
                    "open-forge route list",
                    RouteListWording.NextReason("open-forge route list")),
            RouteListPresentationFindingCode.LoaderUnavailable
                or RouteListPresentationFindingCode.LoaderMalformed
                or RouteListPresentationFindingCode.ReadUnavailable
                => new CliNextAction("open-forge doctor", global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageInspectTheRouteBoundaryWithOpenForgeDoctor()),
            RouteListPresentationFindingCode.MetadataMissing when FindingId(result, first) is { } id
                && result.Rows.Any(row => string.Equals(row.Id, id, StringComparison.Ordinal))
                => new CliNextAction(
                    $"open-forge route update {id} --description \"...\"",
                    global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageAddTheMissingRouteDescriptionThenRerunTheListing()),
            RouteListPresentationFindingCode.MetadataMalformed
                => new CliNextAction(RouteListWording.FixByHand(), global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageRepairTheRouteMetadataThenRerunTheListing())
                { Kind = CliNextActionKind.Sentence },
            RouteListPresentationFindingCode.AuthoredForm
                => new CliNextAction(
                    global::OpenForge.Cli.OutputText.Route.List.RouteListPhrases.FormatRenameTo($"{CompatibilityName(first.Subject ?? "route")}"),
                    global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageRenameTheCompatibilityEntrypointThenRerunTheListing())
                { Kind = CliNextActionKind.Sentence },
            RouteListPresentationFindingCode.OperationFailed
                => new CliNextAction(
                    "open-forge route list --detail debug",
                    global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageRetryTheSameRouteListingWithBoundedDiagnostics()),
            RouteListPresentationFindingCode.Interrupted
                => new CliNextAction("open-forge route list", global::OpenForge.Cli.OutputText.Route.List.RouteListText.MessageRerunTheSameRouteListing()),
            _ => null,
        };
    }

    private static IReadOnlyList<string> Diagnostics(RouteListPresentationFacts result)
        =>
        [
            "route-list diagnostics",
            $"status={CliStatusDefinitions.Read(result.Status).MachineName}",
            $"workspace.lexical={result.WorkspacePath ?? "none"}",
            $"selection.kind={RouteListWording.SelectionKind(result.Selection.Kind)}",
            $"requested-depth={RouteListWording.DiagnosticsDepth(result.RequestedDepth)}",
            $"effective-depth={RouteListWording.DiagnosticsDepth(result.EffectiveDepth)}",
            $"rows={result.Rows.Count.ToString(CultureInfo.InvariantCulture)}",
            $"findings={result.Findings.Count.ToString(CultureInfo.InvariantCulture)}",
        ];
}
