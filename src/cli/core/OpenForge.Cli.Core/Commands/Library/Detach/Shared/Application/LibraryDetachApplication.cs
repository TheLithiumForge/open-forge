using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Application;

internal static class LibraryDetachApplication
{
    internal static async ValueTask<LibraryDetachApplicationOutcome> ApplyAsync(
        LibraryDetachApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var sourceRoot = ReadSourceRoot(input);
        var run = await LibraryMutationApplicationRunner.ApplyAsync(
            new LibraryMutationApplicationRequest
            {
                Lease = input.Lease,
                Directories = input.Plan.Directories,
                Links = input.Plan.Links,
                GeneratedRegions = input.Plan.GeneratedRegions,
                RecordChange = input.Plan.RecordChange,
                RecoveryPreparation = input.RecoveryPreparation,
                ProtectedSourceRoots = sourceRoot is null ? [] : [sourceRoot],
            },
            cancellationToken).ConfigureAwait(false);
        return new LibraryDetachApplicationOutcome
        {
            Application = run.Application,
            Execution = run.Execution,
        };
    }

    private static WorkspaceRelativeDirectory? ReadSourceRoot(LibraryDetachApplicationInput input)
        => input.Plan.Input.Record.Record?.Libraries
            .SingleOrDefault(library => library.Id == input.Plan.Input.Request.LibraryId)
            ?.SourceRoot;
}
