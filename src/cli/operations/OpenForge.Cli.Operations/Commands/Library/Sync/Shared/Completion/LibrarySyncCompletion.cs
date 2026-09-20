using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Library.Sync.Shared.Completion;

internal static class LibrarySyncCompletion
{
    // Completion consumes immutable execution evidence. Verified receipts remain
    // monotonic residual truth; retained and unknown cleanup are distinct facts.
    internal static LibrarySyncResult Complete(LibrarySyncCompletionInput input, bool preview = false)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        var plan = input.Plan;
        var observations = input.Observations;
        var planState = plan?.State ?? LibraryPlanState.NotStarted;
        var findings = (plan?.Findings ?? []).ToList();
        var permissionStage = plan?.Permissions;
        var projectionMode = preview ? LibraryMode.DryRun : input.Request.Mode;
        if (permissionStage is not null && input.Execution.Permission is { } permissionApplication)
        {
            permissionStage = permissionStage with { Result = permissionApplication.Result, Failure = permissionApplication.Failure };
        }
        if (permissionStage?.Failure is { } permissionFailure)
        {
            AddPermissionFinding(findings, permissionFailure, input.Request.LibraryId.Value, input.Execution);
        }
        AddExecutionFindings(findings, input.Execution, input.Request.LibraryId.Value);
        var application = projectionMode == LibraryMode.DryRun || planState != LibraryPlanState.Complete
            || permissionStage?.Failure is not null && input.Execution.Permission?.Receipt is null
            ? LibraryMutationCompletionProjection.NotStarted()
            : LibraryMutationCompletionProjection.Application(input.Request.Workspace, input.Execution, EffectCount(plan));
        var sourceRoot = observations?.Source.Source.Request.SourceRoot;
        var protectedSources = sourceRoot is null ? [] : new[] { sourceRoot };
        var status = LibraryMutationCompletionProjection.Status(
            planState,
            findings.Select(finding => finding.Status),
            input.Execution,
            application,
            projectionMode == LibraryMode.DryRun,
            protectedSources);
        return new LibrarySyncResult
        {
            Status = status,
            Workspace = input.Request.Workspace,
            Next = ReadNext(findings),
            Result = new LibrarySyncPayload
            {
                Permissions = permissionStage is null ? LibraryPermissionView.NotEvaluated() : LibraryPermissionPresentation.Project(permissionStage),
                Identity = LibraryMutationCompletionProjection.Identity(
                    input.Request.LibraryId.Value,
                    sourceRoot?.Value,
                    observations?.Record.Record?.Libraries.FirstOrDefault(library => library.Id == input.Request.LibraryId)?.DestinationRoot.Value,
                    projectionMode,
                    sourceIndependent: false),
                Record = LibraryMutationCompletionProjection.Record(
                    observations?.Record,
                    input.Request.LibraryId.Value,
                    plan?.IntendedRecord),
                Source = LibraryMutationCompletionProjection.Source(
                    observations?.Source,
                    observations?.Record.Record?.Libraries.FirstOrDefault(library => library.Id == input.Request.LibraryId)?.DestinationRoot),
                Projection = LibraryMutationCompletionProjection.Projection(
                    observations,
                    input.Request.LibraryId.Value,
                    planState,
                    sourceIndependent: false),
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

    private static void AddPermissionFinding(
        List<LibrarySyncFinding> findings,
        LibraryPermissionFailure failure,
        string libraryId,
        LibraryExecutionEvidence execution)
    {
        var (status, cause) = LibraryPermissionFailureProjection.Read(failure);
        var code = failure switch
        {
            LibraryPermissionFailure.Required => LibrarySyncFindingCode.PermissionRequired,
            LibraryPermissionFailure.Declined => LibrarySyncFindingCode.PermissionDeclined,
            LibraryPermissionFailure.Invalid => LibrarySyncFindingCode.PermissionInvalid,
            LibraryPermissionFailure.Unavailable => LibrarySyncFindingCode.PermissionUnavailable,
            LibraryPermissionFailure.Changed => LibrarySyncFindingCode.PermissionChanged,
            LibraryPermissionFailure.WriteFailed => LibrarySyncFindingCode.PermissionWriteFailed,
            LibraryPermissionFailure.Interrupted => LibrarySyncFindingCode.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The Library permission failure is not defined."),
        };
        findings.Add(new LibrarySyncFinding
        {
            Code = code,
            Status = status,
            LibraryId = libraryId,
            Path = WorkspaceSettingsDefinitions.RelativePath,
            Cause = failure == LibraryPermissionFailure.Declined
                || (failure == LibraryPermissionFailure.Interrupted
                    && execution.Permission is null
                    && execution.Cancellation is null)
                ? "Library sync was cancelled. Nothing was changed."
                : cause,
        });
    }

    private static int EffectCount(LibrarySyncPlan? plan)
        => plan is null ? 0 : plan.Directories.Length + plan.Links.Length + plan.GeneratedRegions.Length
            + (plan.OwnershipChange is null ? 0 : 1);

    private static void AddExecutionFindings(
        List<LibrarySyncFinding> findings,
        LibraryExecutionEvidence execution,
        string libraryId)
    {
        if (LibraryMutationCompletionProjection.PreparationStatus(execution.RecoveryPreparationOutcome?.State) is { } preparationStatus)
        {
            findings.Add(new LibrarySyncFinding
            {
                Code = preparationStatus == CliSemanticStatus.Interrupted ? LibrarySyncFindingCode.Interrupted : LibrarySyncFindingCode.RecoveryUnavailable,
                Status = preparationStatus,
                LibraryId = libraryId,
                Path = execution.RecoveryPreparationOutcome?.ResidualPath,
                Cause = execution.RecoveryPreparationOutcome?.Cause ?? "Library recovery preparation was interrupted.",
            });
        }

        if (execution.UnexpectedFailure is { } failure)
        {
            findings.Add(new LibrarySyncFinding
            {
                Code = LibrarySyncFindingCode.OperationFailed,
                Status = CliSemanticStatus.Failed,
                LibraryId = libraryId,
                Path = null,
                Cause = failure.Cause,
            });
        }

        if (execution.Cancellation is not null)
        {
            findings.Add(new LibrarySyncFinding
            {
                Code = LibrarySyncFindingCode.Interrupted,
                Status = CliSemanticStatus.Interrupted,
                LibraryId = libraryId,
                Path = null,
                Cause = execution.Cancellation.Stage == LibraryExecutionStage.Preflight
                    ? "Library sync was cancelled. Nothing was changed."
                    : "Library sync was interrupted.",
            });
        }

        if (execution.RecoveryCleanup?.Disposition == RecoveryBundleDisposition.Retained)
        {
            findings.Add(new LibrarySyncFinding
            {
                Code = LibrarySyncFindingCode.RecoveryRetained,
                Status = CliSemanticStatus.Attention,
                LibraryId = libraryId,
                Path = execution.RecoveryCleanup.ResidualPath,
                Cause = execution.RecoveryCleanup.Cause ?? "The recovery bundle remains available.",
            });
        }
    }

    private static CliNextAction? ReadNext(IReadOnlyCollection<LibrarySyncFinding> findings)
        => findings.Any(finding => finding.Code == LibrarySyncFindingCode.ConfirmationRequired)
            ? new CliNextAction(
                "open-forge library sync --automatic",
                "Rerun the same Library Sync request with explicit automatic mode.")
            : null;
}
