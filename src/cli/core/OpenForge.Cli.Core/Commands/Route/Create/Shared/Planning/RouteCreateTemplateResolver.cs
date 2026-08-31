using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreateTemplateResolver
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal ValueTask<RouteCreateTemplateResolution> ResolveAsync(
        RouteCreateRequest request,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        if (request.TemplateReference is not { } reference)
        {
            return new ValueTask<RouteCreateTemplateResolution>(NotRequested());
        }

        return ResolveRequestedAsync(request, catalogue, reference, cancellationToken);
    }

    private static async ValueTask<RouteCreateTemplateResolution> ResolveRequestedAsync(
        RouteCreateRequest request,
        SourceCatalogue catalogue,
        string reference,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                RouteCreateTemplateResolutionState.Incomplete,
                RouteCreateFindingCode.Interrupted,
                "Route Create Template resolution was interrupted.",
                reference);
        }

        var physicalPathResolver = new PhysicalPathResolver();
        var resolver = new SourceReferenceResolver((workspace, path) =>
            physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path)));
        var resolution = resolver.Resolve(reference, catalogue);
        if (resolution.State != SourceReferenceResolutionState.Resolved
            || resolution.Source is not { } source)
        {
            return FromUnresolved(resolution);
        }

        if (source.Base.Form != SourceDocumentForm.Markdown)
        {
            return Stop(
                RouteCreateTemplateResolutionState.Invalid,
                RouteCreateFindingCode.InvalidTemplate,
                "The selected Template must be one ordinary routed Markdown source.",
                reference);
        }

        if (source.Overwrite is not null)
        {
            return Stop(
                RouteCreateTemplateResolutionState.Blocked,
                RouteCreateFindingCode.TemplateUnsafe,
                "A Template with an overwrite companion cannot be instantiated as one file.",
                source.Identity.CanonicalBasePath);
        }

        var read = await new SourceDocumentReader(request.Workspace)
            .ReadAsync(source.Base, cancellationToken)
            .ConfigureAwait(false);
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Read?.State != FileReadState.Complete
            || read.Read.Value is not { } text)
        {
            return FromRead(reference, read);
        }

        var document = new MarkdownDocumentParser().Parse(text);
        var metadata = new FrameworkDocumentMetadataParser().Parse(document);
        if (metadata.State == FrameworkDocumentMetadataState.Malformed)
        {
            return Stop(
                RouteCreateTemplateResolutionState.Blocked,
                RouteCreateFindingCode.MetadataUnsafe,
                "The selected Template metadata is malformed.",
                source.Identity.CanonicalBasePath);
        }

        if (metadata.State != FrameworkDocumentMetadataState.Complete
            || metadata.Metadata is not { } values
            || !values.Tags.Contains("Template", StringComparer.Ordinal))
        {
            return Stop(
                RouteCreateTemplateResolutionState.Invalid,
                RouteCreateFindingCode.InvalidTemplate,
                "The selected source is not classified with the exact Template tag.",
                source.Identity.CanonicalBasePath);
        }

        if (document.BodySpan is not { } bodySpan)
        {
            return Stop(
                RouteCreateTemplateResolutionState.Blocked,
                RouteCreateFindingCode.TemplateUnsafe,
                "The selected Template body boundary is unavailable.",
                source.Identity.CanonicalBasePath);
        }

        var bodyBytes = ImmutableArray.CreateRange(
            StrictUtf8.GetBytes(text[bodySpan.Start..bodySpan.End]));
        return new RouteCreateTemplateResolution
        {
            State = RouteCreateTemplateResolutionState.Resolved,
            Template = new RouteCreateTemplate
            {
                Requested = reference,
                Id = source.Identity.AutomaticId,
                Path = source.Identity.CanonicalBasePath,
                Classification = RouteCreateTemplateClassification.Template,
                BodyByteLength = bodyBytes.Length,
            },
            Source = source,
            BodyBytes = bodyBytes,
            Finding = null,
        };
    }

    private static RouteCreateTemplateResolution FromUnresolved(
        SourceReferenceResolution resolution)
        => resolution.State switch
        {
            SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unsupported => Stop(
                    RouteCreateTemplateResolutionState.Invalid,
                    RouteCreateFindingCode.InvalidTemplate,
                    resolution.Cause ?? "The Template reference is invalid.",
                    resolution.Value),
            SourceReferenceResolutionState.Unknown => Stop(
                RouteCreateTemplateResolutionState.Incomplete,
                RouteCreateFindingCode.TemplateUnavailable,
                resolution.Cause ?? "The Template source is unavailable.",
                resolution.Value),
            SourceReferenceResolutionState.Ambiguous
                or SourceReferenceResolutionState.Unsafe => Stop(
                    RouteCreateTemplateResolutionState.Blocked,
                    RouteCreateFindingCode.TemplateUnsafe,
                    resolution.Cause ?? "The Template source is unsafe or ambiguous.",
                    resolution.Value),
            SourceReferenceResolutionState.Resolved => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "A resolved Template reference requires its source."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The source-reference resolution state is not defined."),
        };

    private static RouteCreateTemplateResolution FromRead(
        string reference,
        SourceDocumentReadResult read)
        => read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => Stop(
                    RouteCreateTemplateResolutionState.Blocked,
                    RouteCreateFindingCode.TemplateUnsafe,
                    "The selected Template physical identity is unsafe or changed.",
                    reference),
            SourceLayerVerificationState.Cancelled => Stop(
                    RouteCreateTemplateResolutionState.Incomplete,
                    RouteCreateFindingCode.Interrupted,
                    "Route Create Template reading was interrupted.",
                    reference),
            SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable => Stop(
                    RouteCreateTemplateResolutionState.Incomplete,
                    RouteCreateFindingCode.TemplateUnavailable,
                    read.Read?.Failure?.DirectCause
                        ?? "The selected Template content is unavailable.",
                    reference),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };

    private static RouteCreateTemplateResolution NotRequested()
        => new()
        {
            State = RouteCreateTemplateResolutionState.NotRequested,
            Template = null,
            Source = null,
            BodyBytes = [],
            Finding = null,
        };

    private static RouteCreateTemplateResolution Stop(
        RouteCreateTemplateResolutionState state,
        RouteCreateFindingCode code,
        string cause,
        string target)
        => new()
        {
            State = state,
            Template = null,
            Source = null,
            BodyBytes = [],
            Finding = new RouteCreateFinding(code, cause, target),
        };
}
