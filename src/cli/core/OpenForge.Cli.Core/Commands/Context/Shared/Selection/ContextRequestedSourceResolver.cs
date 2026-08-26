using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Selection;

internal sealed class ContextRequestedSourceResolver
{
    internal ContextRequestedSourceResolution Resolve(ContextRequest request, ContextGraph graph)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(graph);
        var resolver = new SourceReferenceResolver((workspace, canonicalPath) =>
            new PhysicalPathResolver().ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath)));
        var requests = request.SourceReferences.Select(reference => Resolve(reference, graph, resolver)).ToArray();
        var hasInvalid = requests.Any(value => IsInvalid(value.Resolution, graph.Catalogue));
        var findings = new List<ContextFinding>();
        var blocked = false;
        foreach (var value in requests)
        {
            if (value.Resolution.State != SourceReferenceResolutionState.Resolved)
            {
                var finding = ResolutionFinding(value, graph.Catalogue);
                findings.Add(finding);
                blocked |= finding.Status == Shell.Definitions.CliSemanticStatus.Blocked;
                continue;
            }

            if (!hasInvalid && ExactPathIdentityCollision(value, graph.Catalogue) is { } collision)
            {
                findings.Add(collision);
            }
        }

        return new ContextRequestedSourceResolution
        {
            Requests = requests,
            Findings = findings,
            HasInvalid = hasInvalid,
            Blocked = blocked,
        };
    }

    private static ContextResolvedRequest Resolve(
        string reference,
        ContextGraph graph,
        SourceReferenceResolver resolver)
    {
        var resolution = resolver.Resolve(reference, graph.Catalogue);
        var source = resolution.Source is null
            ? null
            : graph.FindByPath(resolution.Source.Identity.CanonicalBasePath);
        return new ContextResolvedRequest
        {
            Reference = reference,
            Resolution = resolution,
            Source = source,
            Result = new ContextRequestedSource(
                supplied: reference,
                form: resolution.Form,
                resolution: resolution.State,
                source: source is null ? null : Identity(source),
                routeState: source is null ? null : ToRouteState(source.RouteState),
                candidates: resolution.Candidates.Select(candidate => new ContextSourceIdentity(
                    id: candidate.Identity.AutomaticId,
                    path: candidate.Identity.CanonicalBasePath))),
        };
    }

    private static ContextFinding ResolutionFinding(
        ContextResolvedRequest request,
        SourceCatalogue catalogue)
    {
        var orphanOverwrite = IsOrphanOverwrite(request.Resolution, catalogue);
        var code = request.Resolution.State switch
        {
            SourceReferenceResolutionState.Ambiguous => ContextFindingCode.SourceAmbiguous,
            SourceReferenceResolutionState.Unsafe => ContextFindingCode.SourceUnsafe,
            _ when orphanOverwrite => ContextFindingCode.OverwriteAmbiguous,
            _ => ContextFindingCode.InvalidSource,
        };
        IEnumerable<ContextSourceIdentity> candidates = request.Resolution.Candidates.Select(candidate =>
            new ContextSourceIdentity(
                id: candidate.Identity.AutomaticId,
                path: candidate.Identity.CanonicalBasePath));
        if (orphanOverwrite
            && request.Resolution.CanonicalPath is { } path
            && catalogue.FindCandidateByPath(path)?.AutomaticId is { } id)
        {
            candidates = catalogue.FindAllById(id).Select(candidate => new ContextSourceIdentity(
                id: candidate.Identity.AutomaticId,
                path: candidate.Identity.CanonicalBasePath));
        }

        return Finding(
            code: code,
            subject: request.Reference,
            cause: orphanOverwrite
                ? "The exact overwrite path has no unique adjacent logical base source."
                : request.Resolution.Cause ?? "The source reference could not be resolved.",
            reference: request.Reference,
            source: null,
            path: request.Resolution.CanonicalPath,
            candidates: candidates);
    }

    private static ContextFinding? ExactPathIdentityCollision(
        ContextResolvedRequest request,
        SourceCatalogue catalogue)
    {
        if (request.Resolution.Form != SourceReferenceKind.SourcePath
            || request.Resolution.Source is not { } logicalSource)
        {
            return null;
        }

        var candidates = catalogue.FindAllById(logicalSource.Identity.AutomaticId);
        return candidates.Count < 2
            ? null
            : Finding(
                code: ContextFindingCode.IdentityCollision,
                subject: request.Reference,
                cause: "The exact source path is safe, but its automatic ID is shared by more than one logical source.",
                reference: request.Reference,
                source: request.Source is null ? null : Identity(request.Source),
                path: logicalSource.Identity.CanonicalBasePath,
                candidates: candidates.Select(candidate => new ContextSourceIdentity(
                    id: candidate.Identity.AutomaticId,
                    path: candidate.Identity.CanonicalBasePath)));
    }

    private static bool IsInvalid(SourceReferenceResolution resolution, SourceCatalogue catalogue)
        => !IsOrphanOverwrite(resolution, catalogue)
            && resolution.State is SourceReferenceResolutionState.Invalid
                or SourceReferenceResolutionState.Unknown
                or SourceReferenceResolutionState.Unsupported;

    private static bool IsOrphanOverwrite(
        SourceReferenceResolution resolution,
        SourceCatalogue catalogue)
        => resolution.CanonicalPath is { } path
            && catalogue.Issues.Any(issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite
                && string.Equals(issue.AttemptedCanonicalPath, path, StringComparison.Ordinal));

    private static ContextFinding Finding(
        ContextFindingCode code,
        string? subject,
        string cause,
        string? reference,
        ContextSourceIdentity? source,
        string? path,
        IEnumerable<ContextSourceIdentity> candidates)
        => new(
            code: code,
            subject: subject,
            cause: cause,
            reference: reference,
            source: source,
            layer: null,
            path: path,
            part: null,
            location: null,
            destinationLocation: null,
            candidates: candidates);

    private static ContextSourceIdentity Identity(ContextGraphSource source)
        => new(id: source.Id, path: source.CanonicalPath);

    private static ContextRouteState ToRouteState(SourceRouteState state)
        => state switch
        {
            SourceRouteState.Routed => ContextRouteState.Routed,
            SourceRouteState.Unrouted => ContextRouteState.Unrouted,
            SourceRouteState.Ambiguous => ContextRouteState.Ambiguous,
            SourceRouteState.Unavailable => ContextRouteState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The route state is not defined."),
        };
}
