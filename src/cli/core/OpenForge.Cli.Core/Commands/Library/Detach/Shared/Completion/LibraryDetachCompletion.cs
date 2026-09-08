using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;

internal static class LibraryDetachCompletion
{
    // Completion consumes immutable observed evidence, never the derived
    // Outcome.Application. Verified receipts remain monotonic residual truth;
    // positive retained cleanup and unknown cleanup are distinct facts.
    internal static LibraryDetachResult Complete(LibraryDetachCompletionInput input)
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
                Identity = LibraryMutationCompletionProjection.Identity(
                    input.Request.LibraryId.Value,
                    sourceRoot?.Value,
                    input.Request.Mode,
                    sourceIndependent: true),
                Record = LibraryMutationCompletionProjection.Record(
                    observations?.Record,
                    input.Request.LibraryId.Value,
                    plan?.IntendedRecord),
                Projection = LibraryMutationCompletionProjection.Projection(
                    observations?.Record,
                    source: null,
                    observations?.Mappings,
                    observations?.Ownership,
                    input.Request.LibraryId.Value,
                    planState,
                    sourceIndependent: true),
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

    private static WorkspaceRelativeDirectory? ReadSourceRoot(
        LibraryDetachPlanningInput? observations,
        string libraryId)
        => observations?.Record.Record?.Libraries
            .FirstOrDefault(library => string.Equals(library.Id.Value, libraryId, StringComparison.Ordinal))
            ?.SourceRoot;

    private static int EffectCount(LibraryDetachPlan? plan)
        => plan is null ? 0 : plan.Directories.Length + plan.Links.Length + plan.GeneratedRegions.Length + (plan.RecordChange is null ? 0 : 1);

    private static void AddExecutionFindings(
        List<LibraryDetachFinding> findings,
        LibraryExecutionEvidence execution,
        string libraryId)
    {
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
