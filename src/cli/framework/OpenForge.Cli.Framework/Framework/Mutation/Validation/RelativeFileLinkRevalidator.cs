using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal static class RelativeFileLinkRevalidator
{
    internal static ValueTask<RelativeFileLinkValidationResult> ValidateAsync(
        PhysicalPathResolver physicalPathResolver,
        CliWorkspace workspace,
        RelativeFileLinkEffect effect,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(effect);
        return ValueTask.FromResult(Validate(
            physicalPathResolver,
            workspace,
            effect,
            cancellationToken));
    }

    internal static ValueTask<RelativeFileLinkValidationResult> ValidateAsync(
        PhysicalPathResolver physicalPathResolver,
        WorkspaceLockLease lease,
        RelativeFileLinkEffect effect,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lease);
        ArgumentNullException.ThrowIfNull(effect);
        if (!lease.IsHeldFor(lease.Request.Workspace))
        {
            throw new ArgumentException(
                "Relative file-link revalidation requires a live workspace lease.",
                nameof(lease));
        }

        return ValueTask.FromResult(Validate(
            physicalPathResolver,
            lease.Request.Workspace,
            effect,
            cancellationToken));
    }

    private static RelativeFileLinkValidationResult Validate(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        RelativeFileLinkEffect effect,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkValidationResult.Cancelled(effect);
        }

        var logicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            effect.DestinationPath.Value.Replace('/', Path.DirectorySeparatorChar)));
        var expected = effect.Expected.State switch
        {
            Framework.Filesystem.PhysicalPaths.Models.NoFollowLeafState.Missing =>
                Framework.Filesystem.PhysicalPaths.Models.NoFollowLeafObservation.Missing(logicalPath),
            Framework.Filesystem.PhysicalPaths.Models.NoFollowLeafState.RelativeFileLink
                when effect.Expected.Link is { } link =>
                Framework.Filesystem.PhysicalPaths.Models.NoFollowLeafObservation.CreateRelativeFileLink(logicalPath, link),
            _ => throw new InvalidOperationException("A link effect has an unsupported expected state."),
        };

        try
        {
            if (!TryResolvePhysicalLeaf(
                    resolver,
                    workspace,
                    logicalPath,
                    cancellationToken,
                    out _,
                    out var parentFailure,
                    out var parentCause))
            {
                if (parentFailure is not null)
                {
                    return RelativeFileLinkValidationResult.Failed(
                        effect,
                        parentFailure);
                }

                return RelativeFileLinkValidationResult.Blocked(
                    effect,
                    NoFollowLeafObservation.Classified(
                        logicalPath,
                        NoFollowLeafState.Unknown,
                        new FilesystemFailure(
                            FilesystemFailureKind.Unsupported,
                            parentCause)),
                    parentCause);
            }

            var actual = NoFollowLeafObserver.Observe(
                resolver,
                workspace,
                logicalPath,
                cancellationToken);
            return RelativeFileLinkValidator.Validate(effect, expected, actual);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RelativeFileLinkValidationResult.Cancelled(effect);
        }
    }

    internal static bool TryResolvePhysicalLeaf(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        string logicalPath,
        CancellationToken cancellationToken,
        out string physicalPath,
        out FilesystemFailure? failure,
        out string cause)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(logicalPath);
        physicalPath = string.Empty;
        failure = null;
        cause = string.Empty;
        cancellationToken.ThrowIfCancellationRequested();
        if (!PhysicalContainment.Contains(workspace.LexicalRoot, logicalPath))
        {
            cause = "The link destination is outside the selected workspace.";
            return false;
        }

        var strictParentCause = ValidateOrdinaryParents(
            workspace,
            logicalPath,
            cancellationToken);
        if (strictParentCause is not null)
        {
            cause = strictParentCause;
            return false;
        }

        var logicalParent = Path.GetDirectoryName(logicalPath);
        if (logicalParent is null)
        {
            cause = "The link destination has no parent directory.";
            return false;
        }

        var resolution = resolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalParent);
        cancellationToken.ThrowIfCancellationRequested();
        if (resolution.State != PhysicalPathState.Contained)
        {
            failure = resolution.Failure;
            cause = resolution.Failure?.DirectCause
                ?? "The link destination parent is not safely contained in the selected workspace.";
            return false;
        }

        var physicalParent = resolution.GetContainedPhysicalPath();
        var component = LinkTargetReader.Read(physicalParent);
        cancellationToken.ThrowIfCancellationRequested();
        if (component.State != PathComponentState.Ordinary
            || component.Attributes is not { } attributes
            || (attributes & FileAttributes.Directory) == 0
            || (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
        {
            failure = component.Failure;
            cause = component.Failure?.DirectCause
                ?? "The resolved link destination parent is not a real ordinary directory.";
            return false;
        }

        var candidate = Path.Combine(
            physicalParent,
            Path.GetFileName(logicalPath));
        if (!PhysicalContainment.Contains(workspace.PhysicalRoot, candidate))
        {
            cause = "The resolved link destination leaf is outside the selected workspace.";
            return false;
        }

        physicalPath = candidate;
        return true;
    }

    private static string? ValidateOrdinaryParents(
        CliWorkspace workspace,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        var relativeParent = Path.GetRelativePath(
            workspace.LexicalRoot,
            Path.GetDirectoryName(logicalPath)
                ?? throw new InvalidOperationException(
                    "A link destination requires a parent directory."));
        var current = workspace.LexicalRoot;
        if (relativeParent == ".")
        {
            return null;
        }

        foreach (var segment in relativeParent.Split(
                     [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
                     StringSplitOptions.RemoveEmptyEntries))
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = Path.Combine(current, segment);
            var component = LinkTargetReader.Read(current);
            if (component.State != PathComponentState.Ordinary
                || component.Attributes is not { } attributes
                || (attributes & FileAttributes.Directory) == 0
                || (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return "Every Library link destination parent must be a real ordinary directory.";
            }
        }

        return null;
    }
}
