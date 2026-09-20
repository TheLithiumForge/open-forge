using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Interaction;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateSourceSelector(
    SourceCatalogueReader catalogueReader,
    SourceRouteFactsResolver routeFactsResolver,
    PhysicalPathResolver physicalPathResolver,
    CliPrompt<RouteUpdateSourceSelectionQuestion, string>? sourceSelectionPrompt = null)
{
    private readonly SourceCatalogueReader _catalogueReader = catalogueReader;
    private readonly SourceRouteFactsResolver _routeFactsResolver = routeFactsResolver;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly CliPrompt<RouteUpdateSourceSelectionQuestion, string>? _sourceSelectionPrompt = sourceSelectionPrompt;

    internal async ValueTask<RouteUpdateSourceSelectionBuild> SelectAsync(
        RouteUpdateRequest operation,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                operation,
                RouteUpdateTarget.Unresolved(operation.SourceReference),
                RouteUpdateFindingCode.Interrupted,
                "Route Update target observation was interrupted.",
                isIncomplete: true);
        }

        var catalogue = await _catalogueReader.ReadAsync(
                new SourceCatalogueRequest(
                    operation.Workspace,
                    [SourceLogicalPath.AgentsRoot]),
                cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return Stop(
                operation,
                RouteUpdateTarget.Unresolved(operation.SourceReference),
                RouteUpdateFindingCode.Interrupted,
                "Route Update source discovery was interrupted.",
                isIncomplete: true);
        }

        if (catalogue.Issues.FirstOrDefault(issue =>
                issue.Stage == SourceCatalogueIssueStage.Root) is { } rootIssue)
        {
            return FromRootIssue(operation, rootIssue);
        }

        var resolution = Resolve(operation.ResolutionReference, catalogue);
        if (resolution.State == SourceReferenceResolutionState.Ambiguous
            && operation.FrozenSourceReference is null)
        {
            var selection = await SelectAmbiguousSourceAsync(operation, resolution, cancellationToken)
                .ConfigureAwait(false);
            if (selection.State == CliPromptState.Cancelled)
            {
                return Stop(
                    operation,
                    RouteUpdateTarget.Unresolved(operation.SourceReference),
                    RouteUpdateFindingCode.Interrupted,
                    "Route update was cancelled. Nothing was changed.",
                    isIncomplete: false);
            }

            if (selection.State == CliPromptState.Unavailable)
            {
                return FromUnresolved(operation, resolution);
            }

            operation = operation.FreezeSource(selection.Value);
            resolution = Resolve(operation.ResolutionReference, catalogue);
        }

        if (resolution.State != SourceReferenceResolutionState.Resolved
            || resolution.Source is not { } source)
        {
            return FromUnresolved(operation, resolution);
        }

        var target = Target(operation.SourceReference, resolution, source);
        if (HasPhysicalAlias(catalogue, source))
        {
            return Stop(
                operation,
                target,
                RouteUpdateFindingCode.IdentityCollision,
                "The selected source shares one physical identity with another logical path.",
                isIncomplete: false);
        }

        if (!TryReadForm(source.Base.Form, out var form))
        {
            return Stop(
                operation,
                target,
                RouteUpdateFindingCode.InvalidTarget,
                "Route Update accepts ordinary Markdown and recognized entrypoint bases only.",
                isIncomplete: false);
        }

        target = target with { Form = form };
        var reader = new SourceDocumentReader(operation.Workspace);
        var routeFacts = await _routeFactsResolver.ResolveAsync(
                new SourceRouteFactsRequest(catalogue, catalogue.SelectAll()),
                reader,
                cancellationToken)
            .ConfigureAwait(false);
        if (routeFacts.IsCancelled)
        {
            return Stop(
                operation,
                target,
                RouteUpdateFindingCode.Interrupted,
                "Route Update route observation was interrupted.",
                isIncomplete: true);
        }

        var route = routeFacts.RouteFacts.Single(fact => string.Equals(
            fact.Identity.CanonicalBasePath,
            source.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        if (route.State != SourceRouteState.Routed)
        {
            return FromRouteState(operation, target, route.State);
        }

        return RouteUpdateSourceSelectionBuild.Complete(
            new RouteUpdateSourceSelection
            {
                Request = operation,
                Target = target,
                Catalogue = catalogue,
                Source = source,
            });
    }

    private SourceReferenceResolution Resolve(string reference, SourceCatalogue catalogue)
        => new SourceReferenceResolver((workspace, path) =>
            _physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path)))
            .Resolve(reference, catalogue);

    private async ValueTask<CliPromptReply<string>> SelectAmbiguousSourceAsync(
        RouteUpdateRequest request,
        SourceReferenceResolution resolution,
        CancellationToken cancellationToken)
    {
        if (!request.AllowInteractiveSourceSelection || _sourceSelectionPrompt is null)
        {
            return CliPromptReply<string>.Unavailable();
        }

        var requestedId = request.SourceReference;
        var paths = resolution.Candidates
            .Select(candidate => candidate.Identity.CanonicalBasePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        CliPromptReply<string> reply;
        try
        {
            reply = await _sourceSelectionPrompt(
                    new RouteUpdateSourceSelectionQuestion(requestedId, paths),
                    new CliPromptPolicy(true),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CliPromptReply<string>.Cancelled();
        }
        if (reply.State == CliPromptState.Answered
            && paths.Contains(reply.Value, StringComparer.Ordinal))
        {
            return reply;
        }

        return reply.State == CliPromptState.Cancelled
            ? CliPromptReply<string>.Cancelled()
            : CliPromptReply<string>.Unavailable();
    }

    private static RouteUpdateSourceSelectionBuild FromRootIssue(
        RouteUpdateRequest request,
        SourceCatalogueIssue issue)
    {
        var code = issue.Code switch
        {
            SourceCatalogueIssueCode.RootUnsafe => RouteUpdateFindingCode.WorkspaceUnsafe,
            SourceCatalogueIssueCode.RootMissing
                or SourceCatalogueIssueCode.RootUnavailable => RouteUpdateFindingCode.WorkspaceUnavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(issue),
                issue.Code,
                "A non-root issue cannot form a Route Update root boundary."),
        };
        var fallback = code == RouteUpdateFindingCode.WorkspaceUnsafe
            ? "The workspace .agents root is unsafe."
            : "The workspace .agents root is unavailable.";
        return Stop(
            request,
            RouteUpdateTarget.Unresolved(request.SourceReference),
            code,
            issue.Failure?.DirectCause ?? fallback,
            isIncomplete: code == RouteUpdateFindingCode.WorkspaceUnavailable);
    }

    private static RouteUpdateSourceSelectionBuild FromUnresolved(
        RouteUpdateRequest request,
        SourceReferenceResolution resolution)
    {
        var code = resolution.State switch
        {
            SourceReferenceResolutionState.Ambiguous => RouteUpdateFindingCode.RouteAmbiguous,
            SourceReferenceResolutionState.Unsafe => RouteUpdateFindingCode.TargetUnsafe,
            SourceReferenceResolutionState.Unknown
                or SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unsupported => RouteUpdateFindingCode.InvalidTarget,
            SourceReferenceResolutionState.Resolved => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "A resolved reference requires one source."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The source-reference state is not defined."),
        };
        return Stop(
            request,
            RouteUpdateTarget.Unresolved(request.SourceReference),
            code,
            resolution.Cause ?? "The selected source is unavailable.",
            isIncomplete: false);
    }

    private static RouteUpdateSourceSelectionBuild FromRouteState(
        RouteUpdateRequest request,
        RouteUpdateTarget target,
        SourceRouteState state)
        => state switch
        {
            SourceRouteState.Unrouted => Stop(
                request,
                target,
                RouteUpdateFindingCode.InvalidTarget,
                "The selected source is detached from the Loader-exposed route topology.",
                isIncomplete: false),
            SourceRouteState.Ambiguous => Stop(
                request,
                target,
                RouteUpdateFindingCode.RouteAmbiguous,
                "The selected source has ambiguous routed parent meaning.",
                isIncomplete: false),
            SourceRouteState.Unavailable => Stop(
                request,
                target,
                RouteUpdateFindingCode.InspectionIncomplete,
                "The selected source route facts are unavailable.",
                isIncomplete: true),
            SourceRouteState.Routed => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "A routed source does not form a Route Update stop boundary."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The source route state is not defined."),
        };

    private static RouteUpdateTarget Target(
        string requested,
        SourceReferenceResolution resolution,
        SourceLogicalSource source)
        => new()
        {
            Requested = requested,
            SelectedBy = ReadSelection(requested, resolution, source),
            Id = source.Identity.AutomaticId,
            Path = source.Identity.CanonicalBasePath,
            Form = null,
            OverwritePaths = source.Overwrite is { } overwrite
                ? [overwrite.CanonicalPath]
                : [],
        };

    private static RouteUpdateTargetSelection ReadSelection(
        string requested,
        SourceReferenceResolution resolution,
        SourceLogicalSource source)
    {
        if (resolution.Form == SourceReferenceKind.SourceId)
        {
            return RouteUpdateTargetSelection.SourceId;
        }

        var normalized = SourceReferenceParser.Parse(requested).AttemptedPath;
        return string.Equals(normalized, source.Overwrite?.CanonicalPath, StringComparison.Ordinal)
            ? RouteUpdateTargetSelection.OverwritePath
            : RouteUpdateTargetSelection.BasePath;
    }

    private static bool TryReadForm(SourceDocumentForm source, out RouteUpdateTargetForm form)
    {
        form = source switch
        {
            SourceDocumentForm.Markdown => RouteUpdateTargetForm.OrdinaryMarkdown,
            SourceDocumentForm.CanonicalEntrypoint => RouteUpdateTargetForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint =>
                RouteUpdateTargetForm.CompatibilityEntrypoint,
            SourceDocumentForm.Loader
                or SourceDocumentForm.Skill
                or SourceDocumentForm.OverwriteCompanion => default,
            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source,
                "The source document form is not defined."),
        };
        return source is SourceDocumentForm.Markdown
            or SourceDocumentForm.CanonicalEntrypoint
            or SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }

    private static bool HasPhysicalAlias(SourceCatalogue catalogue, SourceLogicalSource source)
    {
        var selectedPaths = new HashSet<string>(StringComparer.Ordinal)
        {
            source.Base.CanonicalPath,
        };
        if (source.Overwrite is { } overwrite)
        {
            selectedPaths.Add(overwrite.CanonicalPath);
        }

        return catalogue.Issues.Any(issue =>
            issue.Code == SourceCatalogueIssueCode.PhysicalAlias
            && (selectedPaths.Contains(issue.AttemptedCanonicalPath)
                || issue.RelatedPaths.Any(selectedPaths.Contains)));
    }

    private static RouteUpdateSourceSelectionBuild Stop(
        RouteUpdateRequest request,
        RouteUpdateTarget target,
        RouteUpdateFindingCode code,
        string cause,
        bool isIncomplete)
        => RouteUpdateSourceSelectionBuild.Stop(
            RouteUpdateObservationBoundaryProjector.Stop(
                new RouteUpdateObservationBoundaryInput
                {
                    Request = request,
                    Target = target,
                    Code = code,
                    Cause = cause,
                    IsIncomplete = isIncomplete,
                }));

}
