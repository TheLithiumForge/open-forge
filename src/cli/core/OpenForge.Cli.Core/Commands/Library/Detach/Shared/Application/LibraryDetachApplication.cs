using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Application;

internal static class LibraryDetachApplication
{
    internal static async ValueTask<LibraryExecutionEvidence> ApplyAsync(
        LibraryDetachApplicationInput input,
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
                RecordChange = input.Plan.RecordChange,
                RecoveryPreparation = input.RecoveryPreparation,
                ProtectedSourceRoots = [.. (input.Plan.Input.Record.Record?.Libraries ?? []).Select(library => library.SourceRoot)],
            },
            cancellationToken).ConfigureAwait(false);
    }

}
