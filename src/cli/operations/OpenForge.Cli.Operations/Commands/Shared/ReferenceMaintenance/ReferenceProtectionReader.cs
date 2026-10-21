using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;

internal sealed class ReferenceProtectionReader(PhysicalPathResolver paths)
{
    private static readonly RoutedMetadataCompletionPolicy ProtectionPolicy = new();
    internal async ValueTask<ReferenceProtectionRead> ReadAsync(CliWorkspace workspace, CancellationToken token)
    {
        var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        if (ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete)
            || settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)) return new(null, []);
        return new(new()
        {
            Settings = settings.Document,
            Ownership = ownership.Document,
        }, [ownership.Snapshot?.Expectation ?? FileExpectation.Missing(ownership.LogicalPath),
            settings.Snapshot?.Expectation ?? FileExpectation.Missing(settings.LogicalPath)]);
    }

    internal static bool Allows(string path, RoutedMetadataPolicyContext policy)
        // Library records do not own ordinary authored bytes; physical projections retain their no-follow boundaries.
        => ProtectionPolicy.Protection(path, policy)
            is RoutedMetadataEligibility.Eligible or RoutedMetadataEligibility.LibraryProjection;

    internal static ReferenceProtection Read(string path, RoutedMetadataPolicyContext policy)
        => new(ProtectionPolicy.Protection(path, policy),
            policy.Ownership.ManagedPaths().FirstOrDefault(claim => claim.Path == path));
}
