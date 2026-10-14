using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameSourceMoveResolver(PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<RenameSourceMoveTarget?> ResolveAsync(RenameSourceMoveRequest request, RenameEvidenceResult evidence, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(evidence);
        cancellationToken.ThrowIfCancellationRequested();
        var reference = request.Reference;
        if (reference.Facts.Target.Resolution != SourceLinkTargetResolution.Missing) return null;
        var sourceMoves = evidence.Resolutions.Select(resolution => resolution.Proven).OfType<RenameEvidenceObservation>()
            .Where(proof => proof.NewPath == reference.SourcePath).ToArray();
        if (sourceMoves.Length != 1) return null;
        var sourceMove = sourceMoves[0];
        var path = reference.Destination.Split('#', 2)[0];
        if (path.Length == 0 || path.Contains('?') || path.Contains(':') || path.Contains('\\') || path.StartsWith('/')) return null;
        if (!SourceDestinationDecoder.TryDecode(path, out var decoded, out _)
            || decoded.Contains('?') || decoded.Contains(':') || decoded.Contains('\\') || decoded.StartsWith('/')) return null;
        var oldTarget = SourceLinkLexicalPathResolver.Resolve(lexicalRoot: request.Workspace.LexicalRoot, sourceCanonicalPath: sourceMove.OldPath, decodedPath: decoded);
        if (oldTarget is not { State: SourceLinkLexicalPathState.Complete, CanonicalPath: { } oldPath }) return null;
        var targetResolution = evidence.Find(oldPath);
        if (targetResolution is not null && targetResolution.Proven is null) return null;
        var currentPath = targetResolution?.Proven?.NewPath ?? oldPath;
        try { _ = SourceWorkspaceRelativePath.ValidateMarkdown(currentPath, nameof(currentPath)); }
        catch (ArgumentException) { return null; }
        var logical = Path.Combine(request.Workspace.LexicalRoot, currentPath.Replace('/', Path.DirectorySeparatorChar));
        if (NoFollowLeafObserver.Observe(physicalPathResolver, request.Workspace, logical, cancellationToken).State != NoFollowLeafState.OrdinaryFile) return null;
        var physical = physicalPathResolver.ResolveCandidate(request.Workspace.LexicalRoot, request.Workspace.PhysicalRoot, logical);
        if (physical.State != PhysicalPathState.Contained) return null;
        try
        {
            var bytes = await File.ReadAllBytesAsync(physical.GetContainedPhysicalPath(), cancellationToken).ConfigureAwait(false);
            var content = RenameContentReader.Read(bytes);
            if (targetResolution?.Proven is { } targetMove && content.Hash != targetMove.NewHash) return null;
            // Consumers retain the authored fragment only after their existing fragment verifier accepts it.
            return new RenameSourceMoveTarget(SourceMove: sourceMove, TargetMove: targetResolution?.Proven,
                OldTargetPath: oldPath, NewTargetPath: currentPath, Fragment: reference.Facts.Fragment,
                TargetSnapshot: FileStateSnapshot.File(logicalPath: logical, physicalPath: physical.GetContainedPhysicalPath(), bytes: bytes));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or DecoderFallbackException or ArgumentException or InvalidOperationException)
        {
            return null;
        }
    }
}
