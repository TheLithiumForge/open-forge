using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

public sealed class LibraryRepairProjectionTests
{
    [Fact(DisplayName = "Repair Library human details retain the selected path, attribution and observed link identity"),
        Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void HumanDetailsRetainSelectedLibraryIdentity()
    {
        var evidence = LibraryRepairData.Evidence();
        var plan = LibraryRepairData.Plan(evidence);
        var result = RepairTestData.Result(facts: RepairTestData.CompleteFacts(
            selection: plan.Selection, plan: plan));
        var builder = new StringBuilder();

        RepairLibraryPresentation.Append(builder, result);

        var text = builder.ToString();
        Assert.Contains(evidence.Entry.Input.Context.LogicalPath.Replace("\\", "\\\\", StringComparison.Ordinal), text, StringComparison.Ordinal);
        Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
        Assert.Contains(evidence.Residual.Candidate.Path.Replace("\\", "\\\\", StringComparison.Ordinal), text, StringComparison.Ordinal);
        Assert.Contains("../../shared/team-knowledge/.agents/directives/review.md", text, StringComparison.Ordinal);
        Assert.Contains("not followed", text, StringComparison.Ordinal);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("OrdinaryCreate", "ordinary-create"), InlineData("OrdinaryReplace", "ordinary-replace")]
    [InlineData("OrdinaryReplaceGeneratedRegion", "ordinary-replace-generated-region"), InlineData("OrdinaryDelete", "ordinary-delete")]
    [InlineData("RelativeFileLinkCreate", "relative-file-link-create"), InlineData("RelativeFileLinkDelete", "relative-file-link-delete")]
    public void ProposalRetainsExactTypedEntryAndOriginalAttribution(string kind, string wire)
    {
        var evidence = LibraryRepairData.Evidence(Enum.Parse<RecoveryEntryKind>(kind));
        var json = RepairLibraryPresentation.Proposal(new RepairLibraryRecoveryProposal(evidence));
        Assert.Equal("team-knowledge", json.LibraryId);
        Assert.Equal(wire, json.EntryKind);
        Assert.Equal("intended", json.Comparison);
        Assert.Equal(evidence.Residual.Candidate.Path, json.BundlePath);
        Assert.Equal(evidence.Entry.Input.Context.LogicalPath, json.LogicalPath);
        Assert.Equal(0, json.EntryOrdinal);
        Assert.False(json.VerifiedPriorRecord);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Lease", "lease"), InlineData("Revalidation", "revalidation"), InlineData("ForwardPreparation", "forward-preparation")]
    [InlineData("Effect", "effect"), InlineData("Verification", "verification")]
    [InlineData("ForwardCleanup", "forward-cleanup"), InlineData("PostDiagnosis", "post-diagnosis")]
    public void ExecutionProjectionRetainsIndependentCancellationWithoutInventingResidual(string stage, string wire)
    {
        var execution = LibraryRepairData.Execution() with
        {
            Cancellation = new RepairLibraryCancellation(Enum.Parse<RepairLibraryExecutionStage>(stage), 0),
        };
        var json = RepairLibraryPresentation.Execution(execution);
        Assert.Empty(json.Receipts);
        Assert.Null(json.ForwardBundlePath);
        Assert.Null(json.ForwardCleanupState);
        Assert.NotNull(json.Cancellation);
        Assert.Equal(wire, json.Cancellation.Stage);
        Assert.Equal(0, json.Cancellation.EffectOrdinal);
    }
}
