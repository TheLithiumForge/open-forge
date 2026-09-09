using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Permissions.Shared.Planning;

internal static class WorkspacePermissionChangePlanner
{
    internal static PlannedFileChange? PlanLibrary(LibraryPermissionChangeRequest request)
    {
        if (request.Approval.ApprovedScopes.IsEmpty && request.Approval.PreviousSourceRoot is null)
        {
            return null;
        }
        if (request.Observation.State is not (WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete)
            || request.Observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("Permission approval requires a complete safe observation.");
        }
        var document = WorkspacePermissionDocument.Empty;
        if (request.Observation.State == WorkspacePermissionReadState.Complete)
        {
            document = request.Observation.Document
                ?? throw new InvalidOperationException("A complete permission observation requires its decoded document.");
        }
        var intended = WorkspacePermissionCodec.Write(WorkspacePermissionEvaluator.GrantLibrary(document, request.Approval));
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

    internal static PlannedFileChange? Plan(WorkspacePermissionChangeRequest request)
    {
        if (request.Approval.Decision != WorkspacePermissionDecision.Approved || request.Approval.Missing.IsEmpty)
        {
            return null;
        }
        if (request.Observation.State is not (WorkspacePermissionReadState.Missing or WorkspacePermissionReadState.Complete)
            || request.Observation.Snapshot is not { } snapshot)
        {
            throw new InvalidOperationException("Permission approval requires a complete safe observation.");
        }
        var document = WorkspacePermissionDocument.Empty;
        if (request.Observation.State == WorkspacePermissionReadState.Complete)
        {
            document = request.Observation.Document
                ?? throw new InvalidOperationException("A complete permission observation requires its decoded document.");
        }
        var intended = WorkspacePermissionCodec.Write(WorkspacePermissionEvaluator.Grant(document, request.Approval.Missing));
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
