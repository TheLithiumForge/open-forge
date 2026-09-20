using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairInteractionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair safe approval leaves bytes unchanged until final confirmation"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task SafeApprovalDoesNotApplyBeforeFinalConfirmation()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-safe-before-final", includeGuided: false);
        var before = workspace.SnapshotState();
        var calls = new List<string>();
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            Interaction = ApproveSafeThenCancelFinal(calls),
        };

        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(["Safe", "Final"], calls);
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoWriteInfrastructure();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair orders safe approval, guided selection, final review, and one apply"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task CombinedFlowAppliesOnceAfterFinalApproval()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-combined-approval");
        var calls = new List<string>();
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            Interaction = ApproveAll(calls, workspace),
        };

        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(["Safe", "Reference", "Final"], calls);
        Assert.Equal(2, result.Counts.SelectedFindings);
        Assert.Equal(1, result.Application.AppliedEffects);
        Assert.Contains("guide.md", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
        Assert.Contains("replacement.md", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
        workspace.AssertNoRecoveryArtifacts();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair unavailable final confirmation returns invalid input with no write"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task UnavailableConfirmationIsInvalid()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-confirmation-unavailable", includeGuided: false);
        var before = workspace.SnapshotState();
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            Interaction = UnavailableConfirmation(),
        };

        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.ConfirmationRequired);
        Assert.Contains(result.Findings, finding => finding.Cause == RepairDefinitions.ConfirmationRequiredMessage);
        var next = Assert.IsType<CliNextAction>(result.Next);
        Assert.Equal("open-forge repair --automatic", next.Command);
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Repair guided cancellation leaves the workspace unchanged"),
        Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task GuidedCancellationHasNoEffects()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-guided-cancel");
        var before = workspace.SnapshotState();
        var components = RepairOperationFactory.CreateDefaultComponents() with
        {
            Interaction = CancelAtGuided(),
        };

        var result = await new RepairOperation(components).ExecuteAsync(
            workspace.Request(allowInteraction: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotState());
    }

    private static RepairInteraction ApproveSafeThenCancelFinal(ICollection<string> calls)
        => new(
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairReferencePromptAnswer>.Answered(RepairReferencePromptAnswer.Skip())),
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairLibraryPromptAnswer>.Answered(new RepairLibraryPromptAnswer(false))),
            (preview, question, policy, cancellationToken) =>
            {
                calls.Add(question.Kind.ToString());
                return ValueTask.FromResult(
                    question.Kind == RepairConfirmationKind.Safe
                        ? CliPromptReply<bool>.Answered(true)
                        : CliPromptReply<bool>.Cancelled());
            });

    private static RepairInteraction ApproveAll(
        ICollection<string> calls,
        RepairIntegrationWorkspace workspace)
        => new(
            (question, policy, cancellationToken) =>
            {
                calls.Add("Reference");
                return ValueTask.FromResult(
                    CliPromptReply<RepairReferencePromptAnswer>.Answered(
                        RepairReferencePromptAnswer.Selected(workspace.GuidedRelink())));
            },
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairLibraryPromptAnswer>.Answered(new RepairLibraryPromptAnswer(false))),
            (preview, question, policy, cancellationToken) =>
            {
                calls.Add(question.Kind.ToString());
                return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
            });

    private static RepairInteraction UnavailableConfirmation()
        => new(
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairReferencePromptAnswer>.Answered(RepairReferencePromptAnswer.Skip())),
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairLibraryPromptAnswer>.Answered(new RepairLibraryPromptAnswer(false))),
            static (_, question, _, _) => ValueTask.FromResult(
                question.Kind == RepairConfirmationKind.Safe
                    ? CliPromptReply<bool>.Answered(true)
                    : CliPromptReply<bool>.Unavailable()));

    private static RepairInteraction CancelAtGuided()
        => new(
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairReferencePromptAnswer>.Cancelled()),
            static (_, _, _) => ValueTask.FromResult(
                CliPromptReply<RepairLibraryPromptAnswer>.Answered(new RepairLibraryPromptAnswer(false))),
            static (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Answered(true)));
}
