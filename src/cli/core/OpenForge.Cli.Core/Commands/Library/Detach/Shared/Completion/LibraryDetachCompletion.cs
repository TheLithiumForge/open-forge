using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;

internal static class LibraryDetachCompletion
{
    // Completion consumes immutable execution evidence. Verified receipts remain
    // monotonic residual truth; retained and unknown cleanup are distinct facts.
    internal static LibraryDetachResult Complete(LibraryDetachCompletionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        var plan = input.Plan;
        var observations = input.Observations;
        var planState = plan?.State ?? LibraryPlanState.NotStarted;
        var findings = (plan?.Findings ?? []).ToList();
        var permissionStage = plan?.Permissions;
        if (permissionStage is not null && input.Execution.Permission is { } permissionApplication)
        {
            permissionStage = permissionStage with { Result = permissionApplication.Result, Failure = permissionApplication.Failure };
        }
        if (permissionStage?.Failure is { } permissionFailure)
        {
            AddPermissionFinding(findings, permissionFailure, input.Request.LibraryId.Value);
        }
        AddExecutionFindings(findings, input.Execution, input.Request.LibraryId.Value);
        var application = input.Request.Mode == LibraryMode.DryRun || planState != LibraryPlanState.Complete
            || permissionStage?.Failure is not null && input.Execution.Permission?.Receipt is null
            ? LibraryMutationCompletionProjection.NotStarted()
            : LibraryMutationCompletionProjection.Application(input.Request.Workspace, input.Execution, EffectCount(plan));
        var sourceRoot = ReadSourceRoot(observations, input.Request.LibraryId.Value);
        var protectedSources = sourceRoot is null ? [] : new[] { sourceRoot };
        var status = LibraryMutationCompletionProjection.Status(
            planState,
            findings.Select(finding => finding.Status),
            input.Execution,
            application,
            input.Request.Mode == LibraryMode.DryRun,
            protectedSources);
        return new LibraryDetachResult
        {
            Status = status,
            Workspace = input.Request.Workspace,
            Next = null,
            Result = new LibraryDetachPayload
            {
                Permissions = permissionStage is null ? LibraryPermissionView.NotEvaluated() : LibraryPermissionPresentation.Project(permissionStage),
                Identity = LibraryMutationCompletionProjection.Identity(
                    input.Request.LibraryId.Value,
                    sourceRoot?.Value,
                    observations?.Record.Record?.Libraries.FirstOrDefault(library => library.Id == input.Request.LibraryId)?.DestinationRoot.Value,
                    input.Request.Mode,
                    sourceIndependent: true),
                Record = LibraryMutationCompletionProjection.Record(
                    observations?.Record,
                    input.Request.LibraryId.Value,
                    plan?.IntendedRecord),
                Projection = LibraryMutationCompletionProjection.Projection(
                    observations,
                    input.Request.LibraryId.Value,
                    planState,
                    sourceIndependent: true),
                Plan = LibraryMutationCompletionProjection.Plan(new()
                {
                    Workspace = input.Request.Workspace,
                    State = planState,
                    Effects = plan?.Effects,
                }),
                Application = application,
                Findings = [.. findings],
            },
        };
    }

    private static WorkspaceRelativeDirectory? ReadSourceRoot(
        LibraryDetachPlanningInput? observations,
        string libraryId)
        => observations?.Record.Record?.Libraries
            .FirstOrDefault(library => string.Equals(library.Id.Value, libraryId, StringComparison.Ordinal))
            ?.SourceRoot;

    private static void AddPermissionFinding(List<LibraryDetachFinding> findings, LibraryPermissionFailure failure, string libraryId)
    {
        var (status, cause) = LibraryPermissionFailureProjection.Read(failure);
        var code = failure switch
        {
            LibraryPermissionFailure.Required => LibraryDetachFindingCode.PermissionRequired,
            LibraryPermissionFailure.Declined => LibraryDetachFindingCode.PermissionDeclined,
            LibraryPermissionFailure.Invalid => LibraryDetachFindingCode.PermissionInvalid,
            LibraryPermissionFailure.Unavailable => LibraryDetachFindingCode.PermissionUnavailable,
            LibraryPermissionFailure.Changed => LibraryDetachFindingCode.PermissionChanged,
            LibraryPermissionFailure.WriteFailed => LibraryDetachFindingCode.PermissionWriteFailed,
            LibraryPermissionFailure.Interrupted => LibraryDetachFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The Library permission failure is not defined."),
        };
        findings.Add(new LibraryDetachFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = WorkspacePermissionDefinitions.RelativePath,
            Cause = cause,
        });
    }

    private static int EffectCount(LibraryDetachPlan? plan)
        => plan is null ? 0 : plan.Directories.Length + plan.Links.Length + plan.GeneratedRegions.Length + (plan.RecordChange is null ? 0 : 1);

    private static void AddExecutionFindings(
        List<LibraryDetachFinding> findings,
        LibraryExecutionEvidence execution,
        string libraryId)
    {
        if (LibraryMutationCompletionProjection.PreparationStatus(execution.RecoveryPreparationOutcome?.State) is { } preparationStatus)
        {
            findings.Add(new LibraryDetachFinding
            {
                Code = preparationStatus == CliSemanticStatus.Interrupted ? LibraryDetachFindingCode.Interrupted : LibraryDetachFindingCode.RecoveryUnavailable,
                Status = preparationStatus,
                LibraryId = libraryId,
                Path = execution.RecoveryPreparationOutcome?.ResidualPath,
                Cause = execution.RecoveryPreparationOutcome?.Cause ?? "Library recovery preparation was interrupted.",
            });
        }

        if (execution.UnexpectedFailure is { } failure)
        {
            findings.Add(new LibraryDetachFinding
            {
                Code = LibraryDetachFindingCode.OperationFailed,
                Status = CliSemanticStatus.Failed,
                LibraryId = libraryId,
                Path = null,
                Cause = failure.Cause,
            });
        }

        if (execution.Cancellation is not null)
        {
            findings.Add(new LibraryDetachFinding
            {
                Code = LibraryDetachFindingCode.Interrupted,
                Status = CliSemanticStatus.Interrupted,
                LibraryId = libraryId,
                Path = null,
                Cause = "Library detach was interrupted.",
            });
        }

        if (execution.RecoveryCleanup?.Disposition == RecoveryBundleDisposition.Retained)
        {
            findings.Add(new LibraryDetachFinding
            {
                Code = LibraryDetachFindingCode.RecoveryRetained,
                Status = CliSemanticStatus.Attention,
                LibraryId = libraryId,
                Path = execution.RecoveryCleanup.ResidualPath,
                Cause = execution.RecoveryCleanup.Cause ?? "The recovery bundle remains available.",
            });
        }
    }
}
