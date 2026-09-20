using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Application;

internal static class LibrarySyncApplication
{
    internal static async ValueTask<LibraryExecutionEvidence> ApplyAsync(
        LibrarySyncApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        return await LibraryMutationApplicationRunner.ApplyAsync(
            new LibraryMutationApplicationRequest
            {
                Permissions = input.Plan.Permissions,
                Lease = input.Lease,
                Directories = input.Plan.Directories,
                Links = input.Plan.Links,
                GeneratedRegions = input.Plan.GeneratedRegions,
                OwnershipChange = input.Plan.OwnershipChange,
                RecoveryPreparation = input.RecoveryPreparation,
                ProtectedSourceRoots = [.. (input.Plan.Input.Record.Record?.Libraries ?? []).Select(library => library.SourceRoot)],
            },
            cancellationToken).ConfigureAwait(false);
    }
}
