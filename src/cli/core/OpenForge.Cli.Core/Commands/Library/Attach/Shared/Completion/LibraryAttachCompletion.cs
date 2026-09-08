using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Attach.Shared.Completion;

internal static class LibraryAttachCompletion
{
    // Completion consumes immutable observed evidence, never the derived
    // Outcome.Application. Verified receipts remain monotonic residual truth;
    // positive retained cleanup and unknown cleanup are distinct facts.
    internal static LibraryAttachResult Complete(LibraryAttachCompletionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        var plan = input.Plan;
        var observations = input.Observations;
        var planState = plan?.State ?? LibraryPlanState.NotStarted;
        var findings = (plan?.Findings ?? []).ToList();
        AddExecutionFindings(findings, input.Execution, input.Request.LibraryId.Value);
        var application = input.Request.Mode == LibraryMode.DryRun || planState != LibraryPlanState.Complete
            ? LibraryMutationCompletionProjection.NotStarted()
            : LibraryMutationCompletionProjection.Application(input.Execution, EffectCount(plan));
        var status = LibraryMutationCompletionProjection.Status(
            planState,
            findings.Select(finding => finding.Status),
            input.Execution,
            application,
            input.Request.Mode == LibraryMode.DryRun,
            [input.Request.SourceRoot]);
        return new LibraryAttachResult
        {
            Status = status,
            Workspace = input.Request.Workspace,
            Next = null,
            Result = new LibraryAttachPayload
            {
                Identity = LibraryMutationCompletionProjection.Identity(
                    input.Request.LibraryId.Value,
                    input.Request.SourceRoot.Value,
                    input.Request.Mode,
                    sourceIndependent: false),
                Record = LibraryMutationCompletionProjection.Record(
                    observations?.Record,
                    input.Request.LibraryId.Value,
                    plan?.IntendedRecord),
                Source = LibraryMutationCompletionProjection.Source(observations?.Source),
                Projection = LibraryMutationCompletionProjection.Projection(
                    observations?.Record,
                    observations?.Source,
                    observations?.Mappings,
                    observations?.Ownership,
                    input.Request.LibraryId.Value,
                    planState,
                    sourceIndependent: false),
                Plan = LibraryMutationCompletionProjection.Plan(
                    planState,
                    plan?.Directories,
                    plan?.Links,
                    plan?.GeneratedRegions,
                    plan?.RecordChange),
                Application = application,
                Findings = [.. findings],
            },
        };
    }

    private static int EffectCount(LibraryAttachPlan? plan)
        => plan is null ? 0 : plan.Directories.Length + plan.Links.Length + plan.GeneratedRegions.Length + (plan.RecordChange is null ? 0 : 1);

    private static void AddExecutionFindings(
        List<LibraryAttachFinding> findings,
        LibraryExecutionEvidence execution,
        string libraryId)
    {
        if (execution.UnexpectedFailure is { } failure)
        {
            findings.Add(new LibraryAttachFinding
            {
                Code = LibraryAttachFindingCode.OperationFailed,
                Status = CliSemanticStatus.Failed,
                LibraryId = libraryId,
                Path = null,
                Cause = failure.Cause,
            });
        }

        if (execution.Cancellation is not null)
        {
            findings.Add(new LibraryAttachFinding
            {
                Code = LibraryAttachFindingCode.Interrupted,
                Status = CliSemanticStatus.Interrupted,
                LibraryId = libraryId,
                Path = null,
                Cause = "Library attach was interrupted.",
            });
        }

        if (execution.RecoveryCleanup?.Disposition == RecoveryBundleDisposition.Retained)
        {
            findings.Add(new LibraryAttachFinding
            {
                Code = LibraryAttachFindingCode.RecoveryRetained,
                Status = CliSemanticStatus.Attention,
                LibraryId = libraryId,
                Path = execution.RecoveryCleanup.ResidualPath,
                Cause = execution.RecoveryCleanup.Cause ?? "The recovery bundle remains available.",
            });
        }
    }
}
