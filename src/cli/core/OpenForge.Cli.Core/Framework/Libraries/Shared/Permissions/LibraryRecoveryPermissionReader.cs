using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;
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
        var current = await LibrariesRecordReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        var target = evidence.Entry.Input.Context.Entry.TargetPath;
        var paths = target == LibraryPathIdentity.RecordRelativePath
            ? LibraryPathIdentity.Mappings(selected).Select(mapping => mapping.DestinationPath.Value)
            : [target];
        var subject = new LibraryPermissionSubject(selected.Id.Value, selected.SourceRoot.Value);
        var required = paths.Where(path => !path.StartsWith(WorkspacePermissionDefinitions.ImplicitPathPrefix, StringComparison.Ordinal))
            .Select(path => new WorkspacePermissionRequirement(subject, path)).ToImmutableArray();
        var observation = await WorkspacePermissionReader.ReadAsync(new PhysicalPathResolver(), workspace, cancellationToken).ConfigureAwait(false);
        var safe = observation.State switch
        {
            WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete => true,
            WorkspacePermissionReadState.Invalid or WorkspacePermissionReadState.Blocked or WorkspacePermissionReadState.Unavailable => false,
            _ => throw new ArgumentOutOfRangeException(nameof(evidence), observation.State, "The permission observation state is not defined."),
        };
        var evaluation = WorkspacePermissionEvaluator.Evaluate(observation.Document ?? WorkspacePermissionDocument.Empty, required);
        if (!safe && !required.IsEmpty)
        {
            return new(observation, evaluation with { Decision = WorkspacePermissionDecision.NotEvaluated }, IsAdmitted: false);
        }
        var targetKey = PortableWorkspacePath.CreatePortableKey(target);
        var sourceRoots = (current.Record?.Libraries ?? []).Select(library => library.SourceRoot).Append(selected.SourceRoot);
        var outsideSources = sourceRoots.All(root =>
        {
            var sourceKey = PortableWorkspacePath.CreatePortableKey(root.Value);
            return targetKey != sourceKey && !targetKey.StartsWith($"{sourceKey}/", StringComparison.Ordinal);
        });
        var recordSafe = current.State is LibrariesRecordReadState.Complete or LibrariesRecordReadState.Missing;
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
