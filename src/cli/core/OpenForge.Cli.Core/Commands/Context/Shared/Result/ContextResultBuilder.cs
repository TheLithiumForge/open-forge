using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Result;

internal sealed class ContextResultBuilder
{
    internal ContextResult Build(
        ContextRequest request,
        ContextClosureResolution closure,
        ContextLinkExpansionFormation links,
        ContextProjectionFormation projection)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(closure);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(projection);
        var findings = closure.Findings
            .Concat(links.Findings)
            .Concat(projection.Findings)
            .OrderBy(finding => finding.Code)
            .ToArray();
        var status = ReadStatus(findings);
        var selectionCoverage = ReadSelectionCoverage(status, closure);
        var linkCoverage = ReadLinkCoverage(request, links);
        var projectionCoverage = ReadProjectionCoverage(status, closure, projection);
        var overall = ReadOverallCoverage(status, selectionCoverage, linkCoverage, projectionCoverage);
        return new ContextResult(
            workspace: request.Workspace,
            selection: new ContextSelection(
                requestedSources: closure.RequestedSources,
                startupIncluded: closure.StartupIncluded,
                additionsOnly: request.AdditionsOnly,
                linkExpansion: request.LinkExpansion,
                sourceCount: status == CliSemanticStatus.Invalid || closure.SelectionBlocked
                    ? null
                    : projection.Sources.Count),
            presentation: new ContextPresentation(
                suppliedView: request.SuppliedView,
                effectiveView: request.EffectiveView,
                content: request.Content),
            coverage: new ContextCoverage(
                state: overall,
                selection: selectionCoverage,
                links: linkCoverage,
                projection: projectionCoverage),
            paths: projection.Paths,
            links: links.Links,
            sources: projection.Sources,
            findings: findings,
            status: status,
            next: ReadNext(status, findings));
    }

    internal ContextResult Event(ContextRequest request, bool interrupted)
    {
        ArgumentNullException.ThrowIfNull(request);
        var status = interrupted ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed;
        var coverage = interrupted ? ContextCoverageState.Interrupted : ContextCoverageState.Failed;
        var code = interrupted ? ContextFindingCode.Interrupted : ContextFindingCode.OperationFailed;
        var linkCoverage = EventLinkCoverage(request, interrupted);
        return new ContextResult(
            workspace: request.Workspace,
            selection: new ContextSelection(
                requestedSources: ContextPreOperationResultBuilder.EchoSources(request.SourceReferences),
                startupIncluded: false,
                additionsOnly: request.AdditionsOnly,
                linkExpansion: request.LinkExpansion,
                sourceCount: null),
            presentation: new ContextPresentation(
                suppliedView: request.SuppliedView,
                effectiveView: request.EffectiveView,
                content: request.Content),
            coverage: new ContextCoverage(
                state: coverage,
                selection: coverage,
                links: linkCoverage,
                projection: coverage),
            paths: [],
            links: [],
            sources: [],
            findings: [new ContextFinding(
                code: code,
                subject: null,
                cause: interrupted
                    ? "Context resolution was interrupted before completion."
                    : "Context resolution failed before a normal result could be formed.",
                reference: null,
                source: null,
                layer: null,
                path: null,
                part: null,
                location: null,
                destinationLocation: null,
                candidates: [])],
            status: status,
            next: interrupted ? ContextDefinitions.InterruptedNextAction : ContextDefinitions.FailedNextAction);
    }

    private static ContextOptionalCoverageState EventLinkCoverage(
        ContextRequest request,
        bool interrupted)
    {
        if (request.LinkExpansion.Mode == ContextLinkExpansionMode.None)
        {
            return ContextOptionalCoverageState.NotRequested;
        }

        return interrupted
            ? ContextOptionalCoverageState.Interrupted
            : ContextOptionalCoverageState.Failed;
    }

    private static CliSemanticStatus ReadStatus(IReadOnlyList<ContextFinding> findings)
    {
        if (findings.Any(finding => finding.Status == CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (findings.Any(finding => finding.Status == CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return findings.Any(finding => finding.Status == CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static ContextCoverageState ReadSelectionCoverage(
        CliSemanticStatus status,
        ContextClosureResolution closure)
    {
        if (status == CliSemanticStatus.Invalid)
        {
            return ContextCoverageState.NotStarted;
        }

        if (closure.SelectionBlocked)
        {
            return ContextCoverageState.Blocked;
        }

        return closure.SelectionComplete
            ? ContextCoverageState.Complete
            : ContextCoverageState.Incomplete;
    }

    private static ContextCoverageState ReadProjectionCoverage(
        CliSemanticStatus status,
        ContextClosureResolution closure,
        ContextProjectionFormation projection)
    {
        if (status == CliSemanticStatus.Invalid)
        {
            return ContextCoverageState.NotStarted;
        }

        if (closure.SelectionBlocked)
        {
            return ContextCoverageState.Blocked;
        }

        return projection.ProjectionComplete
            ? ContextCoverageState.Complete
            : ContextCoverageState.Incomplete;
    }

    private static ContextOptionalCoverageState ReadLinkCoverage(
        ContextRequest request,
        ContextLinkExpansionFormation links)
    {
        if (request.LinkExpansion.Mode == ContextLinkExpansionMode.None)
        {
            return ContextOptionalCoverageState.NotRequested;
        }

        if (links.Blocked)
        {
            return ContextOptionalCoverageState.Blocked;
        }

        return links.Complete
            ? ContextOptionalCoverageState.Complete
            : ContextOptionalCoverageState.Incomplete;
    }

    private static ContextCoverageState ReadOverallCoverage(
        CliSemanticStatus status,
        ContextCoverageState selection,
        ContextOptionalCoverageState links,
        ContextCoverageState projection)
        => status switch
        {
            CliSemanticStatus.Invalid => ContextCoverageState.NotStarted,
            CliSemanticStatus.Blocked => ContextCoverageState.Blocked,
            _ when selection == ContextCoverageState.Incomplete
                || links == ContextOptionalCoverageState.Incomplete
                || projection == ContextCoverageState.Incomplete
                => ContextCoverageState.Incomplete,
            _ => ContextCoverageState.Complete,
        };

    private static CliNextAction? ReadNext(
        CliSemanticStatus status,
        IReadOnlyList<ContextFinding> findings)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Invalid => ContextDefinitions.InvalidNextAction,
            CliSemanticStatus.Blocked when findings.FirstOrDefault(finding => finding.Status == CliSemanticStatus.Blocked)?.Code
                == ContextFindingCode.SourceAmbiguous => ContextDefinitions.SourceAmbiguousNextAction,
            CliSemanticStatus.Blocked => ContextDefinitions.BlockedNextAction,
            CliSemanticStatus.Incomplete => ContextDefinitions.IncompleteNextAction,
            CliSemanticStatus.Failed => ContextDefinitions.FailedNextAction,
            CliSemanticStatus.Interrupted => ContextDefinitions.InterruptedNextAction,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Context semantic status is not defined."),
        };
}
