using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Application;

internal static class LibraryDetachApplication
{
    internal static async ValueTask<LibraryExecutionEvidence> ApplyAsync(
        LibraryDetachApplicationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        var settingsDirectory = Path.GetFullPath(Path.Combine(
            input.Plan.Input.Request.Workspace.LexicalRoot,
            WorkspaceSettingsDefinitions.DirectoryName));
        var settingsParentDirectories = input.Plan.Directories
            .Where(directory => PhysicalIdentityTracker.PathComparer.Equals(directory.LogicalPath, settingsDirectory))
            .ToImmutableArray();
        var directories = input.Plan.Directories
            .Where(directory => !PhysicalIdentityTracker.PathComparer.Equals(directory.LogicalPath, settingsDirectory))
            .ToImmutableArray();
        return await LibraryMutationApplicationRunner.ApplyAsync(
            new LibraryMutationApplicationRequest
            {
                Permissions = input.Plan.Permissions,
                Lease = input.Lease,
                SettingsParentDirectories = settingsParentDirectories,
                Directories = directories,
                Links = input.Plan.Links,
                GeneratedRegions = input.Plan.GeneratedRegions,
                OwnershipChange = input.Plan.OwnershipChange,
                RecoveryPreparation = input.RecoveryPreparation,
                ProtectedSourceRoots = [.. (input.Plan.Input.Record.Record?.Libraries ?? []).Select(library => library.SourceRoot)],
            },
            cancellationToken).ConfigureAwait(false);
    }

}
