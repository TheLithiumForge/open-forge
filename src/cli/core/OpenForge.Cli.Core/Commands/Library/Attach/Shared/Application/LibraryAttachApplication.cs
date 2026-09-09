using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Application;

internal static class LibraryAttachApplication
{
    internal static async ValueTask<LibraryAttachApplicationOutcome> ApplyAsync(
        LibraryAttachApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var run = await LibraryMutationApplicationRunner.ApplyAsync(
            new LibraryMutationApplicationRequest
            {
                Permissions = input.Plan.Permissions,
                Lease = input.Lease,
                Directories = input.Plan.Directories,
                Links = input.Plan.Links,
                GeneratedRegions = input.Plan.GeneratedRegions,
                RecordChange = input.Plan.RecordChange,
                RecoveryPreparation = input.RecoveryPreparation,
                ProtectedSourceRoots = [.. (input.Plan.Input.Record.Record?.Libraries ?? []).Select(library => library.SourceRoot)
                    .Append(input.Plan.Input.Request.SourceRoot).Distinct()],
            },
            cancellationToken).ConfigureAwait(false);
        return new LibraryAttachApplicationOutcome
        {
            Application = run.Application,
            Execution = run.Execution,
        };
    }
}
