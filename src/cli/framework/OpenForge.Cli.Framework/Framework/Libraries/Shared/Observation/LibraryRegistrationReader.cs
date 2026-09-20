using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;

internal static class LibraryRegistrationReader
{
    internal static async ValueTask<LibraryRegistrationRead> ReadAsync(
        PhysicalPathResolver resolver, CliWorkspace workspace, CancellationToken cancellationToken)
        => Read(await WorkspaceOwnershipReader.ReadAsync(resolver, workspace, cancellationToken).ConfigureAwait(false));

    internal static LibraryRegistrationRead Read(WorkspaceOwnershipRead ownership)
    {
        if (ownership.State != WorkspaceOwnershipReadState.Complete)
        {
            return Observation(ownership, ownership.Cause ?? "The ownership lock is absent; no Library registrations are recorded.");
        }
        try
        {
            var record = ReadRegistrations(ownership.Document);
            return new LibraryRegistrationRead
            {
                State = LibraryRegistrationReadState.Complete,
                Record = record,
                Snapshot = ownership.Snapshot,
                Cause = null,
            };
        }
        catch (ArgumentException exception)
        {
            return Observation(ownership, $"The recorded Library ownership could not be interpreted: {exception.Message}");
        }
    }

    internal static LibraryRegistrationSet ReadRegistrations(WorkspaceOwnershipDocument document)
        => LibraryRegistrationSet.Create(document.Libraries
            .OrderBy(library => library.Id, StringComparer.Ordinal)
            .Select(library => LibraryRegistration.Create(LibraryId.Create(library.Id),
                WorkspaceRelativeDirectory.Create(library.SourceRoot),
                LibraryDestinationRoot.Create(library.DestinationRoot),
                library.Paths.Order(StringComparer.Ordinal).Select(SourceRelativeEligiblePath.Create).ToArray()))
            .ToArray());

    private static LibraryRegistrationRead Observation(WorkspaceOwnershipRead ownership, string cause)
        => new()
        {
            State = ownership.State switch
            {
                WorkspaceOwnershipReadState.Absent => LibraryRegistrationReadState.Missing,
                WorkspaceOwnershipReadState.Unavailable => LibraryRegistrationReadState.Unavailable,
                _ => LibraryRegistrationReadState.Malformed,
            },
            Record = null,
            Snapshot = ownership.Snapshot,
            Cause = cause,
            OwnershipObservation = cause,
        };

}
