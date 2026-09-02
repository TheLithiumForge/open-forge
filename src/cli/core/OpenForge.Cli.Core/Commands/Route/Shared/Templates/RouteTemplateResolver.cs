using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
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

namespace OpenForge.Cli.Core.Commands.Route.Shared.Templates;

internal sealed class RouteTemplateResolver
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal async ValueTask<RouteTemplateResolution> ResolveAsync(
        RouteTemplateResolutionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Reference);

        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                RouteTemplateResolutionState.Incomplete,
                RouteTemplateResolutionIssue.ResolutionInterrupted,
                "Route Template resolution was interrupted.",
                request.Reference);
        }

        var physicalPathResolver = new PhysicalPathResolver();
        var resolver = new SourceReferenceResolver((workspace, path) =>
            physicalPathResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path)));
        var resolution = resolver.Resolve(request.Reference, request.Catalogue);
        if (resolution.State != SourceReferenceResolutionState.Resolved
            || resolution.Source is not { } source)
        {
            return FromUnresolved(resolution);
        }

        if (source.Base.Form != SourceDocumentForm.Markdown)
        {
            return Stop(
                RouteTemplateResolutionState.Invalid,
                RouteTemplateResolutionIssue.InvalidSourceKind,
                "The selected Template must be one ordinary routed Markdown source.",
                request.Reference);
        }

        if (source.Overwrite is not null)
        {
            return Stop(
                RouteTemplateResolutionState.Blocked,
                RouteTemplateResolutionIssue.OverwriteUnsafe,
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
            return FromRead(request.Reference, read);
        }

        var document = new MarkdownDocumentParser().Parse(text);
        var metadata = new FrameworkDocumentMetadataParser().Parse(document);
        if (metadata.State == FrameworkDocumentMetadataState.Malformed)
        {
            return Stop(
                RouteTemplateResolutionState.Blocked,
                RouteTemplateResolutionIssue.MetadataUnsafe,
                "The selected Template metadata is malformed.",
                source.Identity.CanonicalBasePath);
        }

        if (metadata.State != FrameworkDocumentMetadataState.Complete
            || metadata.Metadata is not { } values
            || !values.Tags.Contains("Template", StringComparer.Ordinal))
        {
            return Stop(
                RouteTemplateResolutionState.Invalid,
                RouteTemplateResolutionIssue.MissingClassification,
                "The selected source is not classified with the exact Template tag.",
                source.Identity.CanonicalBasePath);
        }

        if (document.BodySpan is not { } bodySpan)
        {
            return Stop(
                RouteTemplateResolutionState.Blocked,
                RouteTemplateResolutionIssue.BodyBoundaryUnsafe,
                "The selected Template body boundary is unavailable.",
                source.Identity.CanonicalBasePath);
        }

        var bodyBytes = ImmutableArray.CreateRange(
            StrictUtf8.GetBytes(text[bodySpan.Start..bodySpan.End]));
        return new RouteTemplateResolution
        {
            State = RouteTemplateResolutionState.Resolved,
            Template = new RouteTemplateSelection
            {
                Requested = request.Reference,
                Id = source.Identity.AutomaticId,
                Path = source.Identity.CanonicalBasePath,
                BodyByteLength = bodyBytes.Length,
            },
            Source = source,
            BodyBytes = bodyBytes,
            Issue = null,
            Cause = null,
            Target = null,
        };
    }

    private static RouteTemplateResolution FromUnresolved(
        SourceReferenceResolution resolution)
        => resolution.State switch
        {
            SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unsupported => Stop(
                    RouteTemplateResolutionState.Invalid,
                    RouteTemplateResolutionIssue.InvalidReference,
                    resolution.Cause ?? "The Template reference is invalid.",
                    resolution.Value),
            SourceReferenceResolutionState.Unknown => Stop(
                    RouteTemplateResolutionState.Invalid,
                    RouteTemplateResolutionIssue.InvalidReference,
                    resolution.Cause ?? "The Template reference is unknown.",
                    resolution.Value),
            SourceReferenceResolutionState.Ambiguous
                or SourceReferenceResolutionState.Unsafe => Stop(
                    RouteTemplateResolutionState.Blocked,
                    RouteTemplateResolutionIssue.SourceUnsafe,
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

    private static RouteTemplateResolution FromRead(
        string reference,
        SourceDocumentReadResult read)
        => read.Verification.State switch
        {
            SourceLayerVerificationState.Unsafe
                or SourceLayerVerificationState.Changed => Stop(
                    RouteTemplateResolutionState.Blocked,
                    RouteTemplateResolutionIssue.SourceUnsafe,
                    "The selected Template physical identity is unsafe or changed.",
                    reference),
            SourceLayerVerificationState.Cancelled => Stop(
                    RouteTemplateResolutionState.Incomplete,
                    RouteTemplateResolutionIssue.ReadInterrupted,
                    "Route Template reading was interrupted.",
                    reference),
            SourceLayerVerificationState.Verified
                or SourceLayerVerificationState.Missing
                or SourceLayerVerificationState.Unavailable => Stop(
                    RouteTemplateResolutionState.Incomplete,
                    RouteTemplateResolutionIssue.SourceUnavailable,
                    read.Read?.Failure?.DirectCause
                        ?? "The selected Template content is unavailable.",
                    reference),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.Verification.State,
                "The source-layer verification state is not defined."),
        };

    private static RouteTemplateResolution Stop(
        RouteTemplateResolutionState state,
        RouteTemplateResolutionIssue issue,
        string cause,
        string target)
        => new()
        {
            State = state,
            Template = null,
            Source = null,
            BodyBytes = [],
            Issue = issue,
            Cause = cause,
            Target = target,
        };
}
