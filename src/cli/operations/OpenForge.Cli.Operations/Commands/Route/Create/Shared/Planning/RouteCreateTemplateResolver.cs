using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateTemplateResolver
{
    private readonly RouteTemplateResolver _resolver = new();

    internal async ValueTask<RouteCreateTemplateResolution> ResolveAsync(
        RouteCreateRequest request,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        if (request.TemplateReference is not { } reference)
        {
            return NotRequested();
        }

        var resolution = await _resolver.ResolveAsync(
            new RouteTemplateResolutionRequest
            {
                Workspace = request.Workspace,
                Reference = reference,
                Catalogue = catalogue,
            },
            cancellationToken).ConfigureAwait(false);
        return resolution.State == RouteTemplateResolutionState.Resolved
            ? Resolved(resolution)
            : Stopped(resolution);
    }

    private static RouteCreateTemplateResolution Resolved(
        RouteTemplateResolution resolution)
    {
        var template = resolution.Template
            ?? throw new InvalidOperationException(
                "Resolved Route Template facts require a Template selection.");
        var source = resolution.Source
            ?? throw new InvalidOperationException(
                "Resolved Route Template facts require a source.");
        return new RouteCreateTemplateResolution
        {
            State = RouteCreateTemplateResolutionState.Resolved,
            Template = new RouteCreateTemplate
            {
                Requested = template.Requested,
                Id = template.Id,
                Path = template.Path,
                Classification = RouteCreateTemplateClassification.Template,
                BodyByteLength = template.BodyByteLength,
            },
            Source = source,
            BodyBytes = resolution.BodyBytes,
            Finding = null,
        };
    }

    private static RouteCreateTemplateResolution Stopped(
        RouteTemplateResolution resolution)
    {
        var issue = resolution.Issue
            ?? throw new InvalidOperationException(
                "Stopped Route Template facts require an issue.");
        var cause = issue switch
        {
            RouteTemplateResolutionIssue.ResolutionInterrupted =>
                "Route Create Template resolution was interrupted.",
            RouteTemplateResolutionIssue.ReadInterrupted =>
                "Route Create Template reading was interrupted.",
            _ => resolution.Cause
                ?? throw new InvalidOperationException(
                    "Stopped Route Template facts require a cause."),
        };
        var code = issue switch
        {
            RouteTemplateResolutionIssue.InvalidReference
                or RouteTemplateResolutionIssue.InvalidSourceKind
                or RouteTemplateResolutionIssue.MissingClassification =>
                    RouteCreateFindingCode.InvalidTemplate,
            RouteTemplateResolutionIssue.MetadataUnsafe =>
                    RouteCreateFindingCode.MetadataUnsafe,
            RouteTemplateResolutionIssue.OverwriteUnsafe
                or RouteTemplateResolutionIssue.BodyBoundaryUnsafe
                or RouteTemplateResolutionIssue.SourceUnsafe =>
                    RouteCreateFindingCode.TemplateUnsafe,
            RouteTemplateResolutionIssue.SourceUnavailable =>
                    RouteCreateFindingCode.TemplateUnavailable,
            RouteTemplateResolutionIssue.ResolutionInterrupted
                or RouteTemplateResolutionIssue.ReadInterrupted =>
                    RouteCreateFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                issue,
                "The Route Template resolution issue is not defined."),
        };
        var state = resolution.State switch
        {
            RouteTemplateResolutionState.Invalid =>
                RouteCreateTemplateResolutionState.Invalid,
            RouteTemplateResolutionState.Blocked =>
                RouteCreateTemplateResolutionState.Blocked,
            RouteTemplateResolutionState.Incomplete =>
                RouteCreateTemplateResolutionState.Incomplete,
            RouteTemplateResolutionState.Resolved => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "A resolved Route Template cannot form a stopped Create result."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The Route Template resolution state is not defined."),
        };
        return new RouteCreateTemplateResolution
        {
            State = state,
            Template = null,
            Source = null,
            BodyBytes = [],
            Finding = new RouteCreateFinding(
                code,
                cause,
                resolution.Target),
        };
    }

    private static RouteCreateTemplateResolution NotRequested()
        => new()
        {
            State = RouteCreateTemplateResolutionState.NotRequested,
            Template = null,
            Source = null,
            BodyBytes = [],
            Finding = null,
        };
}
