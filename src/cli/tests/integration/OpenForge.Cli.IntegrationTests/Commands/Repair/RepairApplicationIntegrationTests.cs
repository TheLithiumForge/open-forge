using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Repair automatic application changes only safe-exact links and leaves "
        + "guided candidates unselected"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task AutomaticApplicationLeavesGuidedCandidatesUnselected()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-automatic-application",
            includeSafeExact: true,
            includeGuided: true);
        var before = workspace.SnapshotState();
        var sourceBefore = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var unrelatedBefore = workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath);

        var result = await ExecuteAsync(
            workspace.Request(
                mode: RepairMode.Apply,
                automatic: true));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RepairMode.Apply, result.Mode);
        Assert.NotNull(result.Selection);
        Assert.Single(result.Selection!.Selected);
        var unselected = Assert.Single(result.Selection.Unselected);
        Assert.Equal(
            RepairCatalogueMember.MissingTargetRelink,
            unselected.Member);
        Assert.Contains(
            RepairSelectionOrigin.Automatic,
            result.Selection.Selected[0].Origins);
        Assert.NotNull(result.Plan);
        Assert.Single(result.Plan!.Steps);
        Assert.Single(result.Plan.Effects);
        Assert.Empty(result.Plan.NoOps);
        Assert.Empty(result.Plan.Conflicts);
        Assert.Equal(
            [
                RepairDependencyDomain.WorkspaceContainment,
                RepairDependencyDomain.RouteAndHeading,
                RepairDependencyDomain.LocalReference,
            ],
            result.Plan.Steps[0].Dependency.Domains);
        Assert.Equal(
            [
                RepairVerificationKind.DestinationLiteral,
                RepairVerificationKind.SameTargetIdentity,
                RepairVerificationKind.ResultingBytes,
            ],
            result.Plan.Steps[0].Verification.Kinds);
        Assert.Equal(RepairRecoveryRequirementKind.Required, result.Plan.Steps[0].Recovery.Kind);
        Assert.Equal(RepairApplicationState.Applied, result.Application.State);
        Assert.Equal(RepairVerificationState.Verified, result.Verification.Targets);
        Assert.Equal(RepairVerificationState.Verified, result.Verification.ResultingBytes);
        Assert.Equal(RepairVerificationState.Verified, result.Verification.PostConditions);
        Assert.Equal(RepairRecoveryState.Removed, result.Recovery.State);
        Assert.Equal(RepairPostDiagnosisState.Complete, result.PostDiagnosis.State);
        Assert.Equal(RepairIntegrationWorkspace.GuidedExpectedDestination, ReadGuidedDestination(workspace));
        Assert.Equal(
            RepairIntegrationWorkspace.SafeIntendedDestination,
            ReadSafeDestination(workspace));
        Assert.Equal(unrelatedBefore, workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath));
        Assert.NotEqual(before["workspace/" + RepairIntegrationWorkspace.SourcePath],
            workspace.SnapshotState()["workspace/" + RepairIntegrationWorkspace.SourcePath]);
        Assert.NotEqual(sourceBefore, workspace.ReadText(RepairIntegrationWorkspace.SourcePath));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Repair explicit relink preserves labels and unrelated bytes, removes recovery, "
        + "and converges to a verified no-op"),
     Trait("Feature", "repair"), Trait("Evidence", "Integration")]
    public async Task ExplicitRelinkPreservesBytesAndRerunsAsVerifiedNoOp()
    {
        using var workspace = RepairIntegrationWorkspace.Create(
            "repair-explicit-relink",
            includeSafeExact: false,
            includeGuided: true);
        var sourceBefore = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var unrelatedBefore = workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath);

        var first = await ExecuteAsync(
            workspace.Request(
                relinks: [workspace.GuidedRelink()]));

        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.NotNull(first.Plan);
        Assert.Single(first.Plan!.Effects);
        Assert.Empty(first.Plan.Conflicts);
        Assert.Equal(RepairApplicationState.Applied, first.Application.State);
        Assert.Equal(RepairVerificationState.Verified, first.Verification.Targets);
        Assert.Equal(RepairVerificationState.Verified, first.Verification.ResultingBytes);
        Assert.Equal(RepairVerificationState.Verified, first.Verification.PostConditions);
        Assert.Equal(RepairRecoveryState.Removed, first.Recovery.State);
        Assert.Equal(RepairPostDiagnosisState.Complete, first.PostDiagnosis.State);
        Assert.Equal(
            "[Guided](replacement.md)\nUnrelated bytes stay exactly.\n",
            ReadBody(workspace));
        Assert.Contains(
            "[Guided]",
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "[Safe]",
            workspace.ReadText(RepairIntegrationWorkspace.SourcePath),
            StringComparison.Ordinal);
        Assert.Equal(unrelatedBefore, workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath));
        workspace.AssertPersistentExternalLock();
        workspace.AssertNoRecoveryArtifacts();

        var afterFirst = workspace.SnapshotState();
        var second = await ExecuteAsync(
            workspace.Request(
                mode: RepairMode.Apply,
                automatic: true));

        Assert.Equal(CliSemanticStatus.Complete, second.Status);
        Assert.NotNull(second.Plan);
        Assert.True(second.Plan!.IsNoOp);
        Assert.Empty(second.Plan.Effects);
        Assert.Empty(second.Plan.Conflicts);
        Assert.Equal(RepairApplicationState.NotRequested, second.Application.State);
        Assert.Equal(RepairVerificationState.Verified, second.Verification.Targets);
        Assert.Equal(RepairVerificationState.Verified, second.Verification.ResultingBytes);
        Assert.Equal(RepairVerificationState.Verified, second.Verification.PostConditions);
        Assert.Equal(RepairRecoveryState.NotRequired, second.Recovery.State);
        Assert.Equal(RepairPostDiagnosisState.Complete, second.PostDiagnosis.State);
        Assert.Equal(afterFirst, workspace.SnapshotState());
        Assert.NotEqual(sourceBefore, workspace.ReadText(RepairIntegrationWorkspace.SourcePath));
    }

    private static async Task<RepairResult> ExecuteAsync(
        RepairRequest request)
        => await new RepairOperation().ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

    private static string ReadBody(RepairIntegrationWorkspace workspace)
    {
        var source = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var bodyStart = source.IndexOf("# Source", StringComparison.Ordinal);
        Assert.True(bodyStart >= 0);
        return source[(bodyStart + "# Source\n\n".Length)..];
    }

    private static string ReadSafeDestination(RepairIntegrationWorkspace workspace)
    {
        var source = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var open = source.IndexOf("[Safe](", StringComparison.Ordinal);
        Assert.True(open >= 0);
        var start = open + "[Safe](".Length;
        var end = source.IndexOf(')', start);
        Assert.True(end > start);
        return source[start..end];
    }

    private static string ReadGuidedDestination(RepairIntegrationWorkspace workspace)
    {
        var source = workspace.ReadText(RepairIntegrationWorkspace.SourcePath);
        var open = source.IndexOf("[Guided](", StringComparison.Ordinal);
        Assert.True(open >= 0);
        var start = open + "[Guided](".Length;
        var end = source.IndexOf(')', start);
        Assert.True(end > start);
        return source[start..end];
    }
}
