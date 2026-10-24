using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Extension.Update;
using OpenForge.Cli.Core.Presentation.Library.Attach;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

public sealed partial class WorkspaceLockFailurePresentationTests
{
    private void VerifyOtherPackages(WorkspaceLockFailureInfo failure, CliSemanticStatus status, string cause)
    {
        var cancelled = failure.Reason == WorkspaceLockFailureReason.Cancelled;
        Verify(ExtensionUpdateResult.Empty(Workspace, ExtensionUpdateMode.Apply, false, false, true,
            new ExtensionUpdateFinding(cancelled ? ExtensionUpdateFindingCode.Interrupted : ExtensionUpdateFindingCode.WorkspaceLockUnavailable, cause)
            { LockFailure = failure }), ExtensionUpdatePresentation.Rendering, failure,
            ExtensionUpdateDefinitions.ReadMachineName(cancelled ? ExtensionUpdateFindingCode.Interrupted : ExtensionUpdateFindingCode.WorkspaceLockUnavailable), status);
        Verify(ExtensionRemoveResult.Empty(Workspace, ExtensionRemoveMode.Apply, true,
            new ExtensionRemoveFinding(cancelled ? ExtensionRemoveFindingCode.Interrupted : ExtensionRemoveFindingCode.WorkspaceLockUnavailable, cause)
            { LockFailure = failure }), ExtensionRemovePresentation.Rendering, failure,
            ExtensionRemoveDefinitions.ReadMachineName(cancelled ? ExtensionRemoveFindingCode.Interrupted : ExtensionRemoveFindingCode.WorkspaceLockUnavailable), status);

        Verify(new LibraryAttachResult
        {
            Status = status,
            Workspace = Workspace,
            Next = null,
            Result = new LibraryAttachPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationPresentationData.Identity(false),
                Record = LibraryMutationPresentationData.Record(),
                Source = LibraryMutationPresentationData.Source(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [new LibraryAttachFinding
                {
                    Code = cancelled ? LibraryAttachFindingCode.Interrupted : LibraryAttachFindingCode.LockUnavailable,
                    Status = status, Cause = cause, Path = null, LibraryId = LibraryMutationPlanningData.Id, LockFailure = failure,
                }],
            },
        }, LibraryAttachPresentation.Rendering, failure, LibraryAttachDefinitions.ReadFindingCode(cancelled ? LibraryAttachFindingCode.Interrupted : LibraryAttachFindingCode.LockUnavailable), status);

        Verify(new LibraryDetachResult
        {
            Status = status,
            Workspace = Workspace,
            Next = null,
            Result = new LibraryDetachPayload
            {
                Permissions = LibraryPermissionView.NotEvaluated(),
                Identity = LibraryMutationPresentationData.Identity(false),
                Record = LibraryMutationPresentationData.Record(),
                Projection = LibraryMutationPresentationData.Projection(),
                Plan = LibraryMutationPresentationData.Plan(),
                Application = LibraryMutationPresentationData.Application(),
                Findings = [new LibraryDetachFinding
                {
                    Code = cancelled ? LibraryDetachFindingCode.Interrupted : LibraryDetachFindingCode.LockUnavailable,
                    Status = status, Cause = cause, Path = null, LibraryId = LibraryMutationPlanningData.Id, OccupantKind = null, LockFailure = failure,
                }],
            },
        }, LibraryDetachPresentation.Rendering, failure, LibraryDetachDefinitions.ReadFindingCode(cancelled ? LibraryDetachFindingCode.Interrupted : LibraryDetachFindingCode.LockUnavailable), status);
    }
}
