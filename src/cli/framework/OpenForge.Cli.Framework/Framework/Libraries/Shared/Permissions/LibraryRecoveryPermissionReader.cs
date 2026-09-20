using OpenForge.Cli.Core.Framework.Ownership;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Permissions;

internal static class LibraryRecoveryPermissionReader
{
    internal static async ValueTask<LibraryRecoveryPermissionRead> ObserveAsync(
        CliWorkspace workspace,
        LibraryResidualEvidence evidence,
        CancellationToken cancellationToken)
    {
        var selected = evidence.CurrentRecord.Record?.Libraries.SingleOrDefault(library => library.Id == evidence.LibraryId)
            ?? evidence.VerifiedPriorRecord?.Record.Libraries.SingleOrDefault(library => library.Id == evidence.LibraryId)
            ?? throw new InvalidOperationException("Library recovery requires current or verified prior record membership.");
        var current = await LibraryRegistrationReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        var target = evidence.Entry.Input.Context.Entry.TargetPath;
        var paths = target == WorkspaceOwnershipDefinitions.RelativePath
            ? LibraryPathIdentity.Mappings(selected).Select(mapping => mapping.DestinationPath.Value)
            : [target];
        var required = paths.Where(path => !path.StartsWith(WorkspaceSettingsDefinitions.ImplicitPathPrefix, StringComparison.Ordinal)).ToImmutableArray();
        var observation = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        var evaluation = WorkspaceAllowList.Evaluate(observation.Document.AllowInstallPaths, required);
        var targetKey = PortableWorkspacePath.CreatePortableKey(target);
        var sourceRoots = (current.Record?.Libraries ?? []).Select(library => library.SourceRoot).Append(selected.SourceRoot);
        var outsideSources = sourceRoots.All(root =>
        {
            var sourceKey = PortableWorkspacePath.CreatePortableKey(root.Value);
            return targetKey != sourceKey && !targetKey.StartsWith($"{sourceKey}/", StringComparison.Ordinal);
        });
        var recordSafe = current.State is LibraryRegistrationReadState.Complete or LibraryRegistrationReadState.Missing;
        return new(observation, evaluation, evaluation.Missing.IsEmpty && recordSafe && outsideSources);
    }

    internal static ValueTask<LibraryRecoveryPermissionRead> ReadAsync(
        WorkspaceLockLease lease,
        LibraryResidualEvidence evidence,
        CancellationToken cancellationToken)
    {
        if (!lease.IsHeldFor(evidence.Residual.Workspace))
        {
            throw new ArgumentException("Library recovery permission admission requires a live same-workspace lease.", nameof(lease));
        }
        return ObserveAsync(lease.Request.Workspace, evidence, cancellationToken);
    }
}
