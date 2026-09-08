using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.Models.Application;

// Structural scope evidence contains no source bytes, hashes, or read receipts.
// The collector retains every attempted mutation target, including failed or
// uncertain attempts, alongside the source roots protected by the preflight.
internal sealed record LibrarySourceEffectScopeFacts
{
    public required ImmutableArray<WorkspaceRelativeDirectory> ProtectedSourceRoots { get; init; }
    public required ImmutableArray<CanonicalRelativePath> AttemptedMutationTargets { get; init; }
    public required bool IsComplete { get; init; }

    internal void Validate()
    {
        if (ProtectedSourceRoots.IsDefault || ProtectedSourceRoots.Any(root => root is null))
        {
            throw new ArgumentException("Source scope facts require an initialized collection of protected roots.", nameof(ProtectedSourceRoots));
        }

        if (AttemptedMutationTargets.IsDefault || AttemptedMutationTargets.Any(path => path is null))
        {
            throw new ArgumentException("Source scope facts require an initialized collection of attempted targets.", nameof(AttemptedMutationTargets));
        }
    }
}
