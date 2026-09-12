using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Result;

internal static class ContextPreOperationResultBuilder
{
    internal static ContextResult Invalid(
        ContextBindingInput input,
        CliWorkspace? workspace,
        ContextContentSelection content,
        ContextLinkExpansion linkExpansion,
        IEnumerable<ContextFinding> findings)
        => Create(
            input: input,
            workspace: workspace,
            content: content,
            linkExpansion: linkExpansion,
            findings: findings,
            status: CliSemanticStatus.Invalid,
            coverage: ContextCoverageState.NotStarted,
            next: ContextDefinitions.InvalidNextAction);

    internal static ContextResult WorkspaceBlocked(
        ContextBindingInput input,
        ContextContentSelection content,
        ContextLinkExpansion linkExpansion,
        ContextFinding finding)
        => Create(
            input: input,
            workspace: null,
            content: content,
            linkExpansion: linkExpansion,
            findings: [finding],
            status: CliSemanticStatus.Blocked,
            coverage: ContextCoverageState.Blocked,
            next: ContextDefinitions.BlockedNextAction);

    internal static IReadOnlyList<ContextRequestedSource> EchoSources(
        IEnumerable<string> sourceReferences)
    {
        ArgumentNullException.ThrowIfNull(sourceReferences);
        return sourceReferences.Select(value =>
        {
            var parsed = SourceReferenceParser.Parse(value);
            return new ContextRequestedSource(
                supplied: value,
                form: parsed.Kind,
                resolution: parsed.State == SourceReferenceParseState.Valid
                    ? SourceReferenceResolutionState.Unknown
                    : SourceReferenceResolutionState.Invalid,
                source: null,
                routeState: null,
                candidates: []);
        }).ToArray();
    }

    private static ContextResult Create(
        ContextBindingInput input,
        CliWorkspace? workspace,
        ContextContentSelection content,
        ContextLinkExpansion linkExpansion,
        IEnumerable<ContextFinding> findings,
        CliSemanticStatus status,
        ContextCoverageState coverage,
        CliNextAction next)
        => new(
            workspace: workspace,
            selection: new ContextSelection(
                requestedSources: EchoSources(input.Sources),
                startupIncluded: false,
                additionsOnly: input.AdditionsOnly,
                linkExpansion: linkExpansion,
                sourceCount: null),
            presentation: new ContextPresentation(
                suppliedView: input.SuppliedView,
                effectiveView: input.EffectiveView,
                content: content),
            coverage: new ContextCoverage(
                state: coverage,
                selection: coverage,
                links: linkExpansion.Mode == ContextLinkExpansionMode.None
                    ? ContextOptionalCoverageState.NotRequested
                    : OptionalCoverage(coverage),
                projection: coverage),
            paths: [],
            links: [],
            sources: [],
            findings: findings,
            status: status,
            next: next);

    private static ContextOptionalCoverageState OptionalCoverage(ContextCoverageState coverage)
        => coverage switch
        {
            ContextCoverageState.NotStarted => ContextOptionalCoverageState.NotStarted,
            ContextCoverageState.Complete => ContextOptionalCoverageState.Complete,
            ContextCoverageState.Incomplete => ContextOptionalCoverageState.Incomplete,
            ContextCoverageState.Blocked => ContextOptionalCoverageState.Blocked,
            ContextCoverageState.Failed => ContextOptionalCoverageState.Failed,
            ContextCoverageState.Interrupted => ContextOptionalCoverageState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Context coverage is not defined."),
        };
}
