using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;

internal static class WorkspacePermissionChangePlanner
{
    internal static PlannedFileChange? PlanLibrary(LibraryPermissionChangeRequest request)
    {
        if (request.Approval.ApprovedScopes.IsEmpty && request.Approval.PreviousSourceRoot is null)
        {
            return null;
        }
        var document = ReadDocument(request.Observation, out var snapshot);
        var intended = WorkspacePermissionCodec.Write(WorkspacePermissionEvaluator.GrantLibrary(document, request.Approval));
        return CreateChange(snapshot, intended);
    }

    internal static PlannedFileChange? Plan(WorkspacePermissionChangeRequest request)
    {
        if (request.Approval.Decision != WorkspacePermissionDecision.Approved || request.Approval.Missing.IsEmpty)
        {
            return null;
        }
        var document = ReadDocument(request.Observation, out var snapshot);
        var intended = WorkspacePermissionCodec.Write(WorkspacePermissionEvaluator.Grant(document, request.Approval.Missing));
        return CreateChange(snapshot, intended);
    }

    internal static (WorkspacePermissionAction Action, RecoveryBundleTarget? RecoveryTarget) ReadActionAndRecovery(
        WorkspacePermissionRead? observation,
        PlannedFileChange? change)
    {
        var action = change?.Kind switch
        {
            null => WorkspacePermissionAction.None,
            PlannedFileChangeKind.Create => WorkspacePermissionAction.Create,
            PlannedFileChangeKind.Replace => WorkspacePermissionAction.Replace,
            _ => throw new InvalidOperationException("Permission approval can only create or replace its consumer document."),
        };
        RecoveryBundleTarget? recovery = null;
        if (change is not null && observation?.Snapshot is { } before)
        {
            recovery = change.Kind == PlannedFileChangeKind.Create
                ? RecoveryBundleTarget.CreateReversible(change, before)
                : RecoveryBundleTarget.Create(change, before);
        }
        return (action, recovery);
    }

    private static WorkspacePermissionDocument ReadDocument(WorkspacePermissionRead observation, out FileStateSnapshot snapshot)
    {
        if (observation.State is not (WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete)
            || observation.Snapshot is not { } observedSnapshot)
        {
            throw new InvalidOperationException("Permission approval requires a complete safe observation.");
        }
        var document = WorkspacePermissionDocument.Empty;
        if (observation.State == WorkspacePermissionReadState.Complete)
        {
            document = observation.Document
                ?? throw new InvalidOperationException("A complete permission observation requires its decoded document.");
        }
        snapshot = observedSnapshot;
        return document;
    }

    private static PlannedFileChange? CreateChange(FileStateSnapshot snapshot, byte[] intended)
    {
        if (snapshot.Kind == FileExpectationKind.Missing)
        {
            return PlannedFileChange.Create(snapshot.Expectation, intended);
        }
        if (snapshot.Bytes.AsSpan().SequenceEqual(intended))
        {
            return null;
        }
        return PlannedFileChange.Replace(snapshot.Expectation, intended);
    }
}
