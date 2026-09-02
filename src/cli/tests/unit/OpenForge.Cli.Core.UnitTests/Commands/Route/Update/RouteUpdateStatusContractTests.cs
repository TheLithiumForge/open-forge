using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateStatusContractTests
{
    [Fact(DisplayName = "Route Update result builder maps every accepted status"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void ResultBuilderMapsEveryAcceptedStatus()
    {
        Assert.Equal(CliSemanticStatus.Complete, RouteUpdateTestData.Result().Status);
        AssertStatus(RouteUpdateFindingCode.InvalidInput, CliSemanticStatus.Invalid);
        AssertStatus(RouteUpdateFindingCode.WorkspaceUnsafe, CliSemanticStatus.Blocked);
        AssertStatus(RouteUpdateFindingCode.WorkspaceUnavailable, CliSemanticStatus.Incomplete);
        AssertStatus(RouteUpdateFindingCode.TemplateBodyProtected, CliSemanticStatus.Attention);
        AssertStatus(RouteUpdateFindingCode.WriteFailed, CliSemanticStatus.Failed);
        AssertStatus(RouteUpdateFindingCode.Interrupted, CliSemanticStatus.Interrupted);

        var precedence = RouteUpdateTestData.Result(
            RouteUpdateTestData.VerifiedNoOpFormation(
                findings:
                [
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.TemplateBodyProtected),
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.WorkspaceUnavailable),
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.WorkspaceUnsafe),
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.InvalidInput),
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.Interrupted),
                    RouteUpdateTestData.Finding(RouteUpdateFindingCode.WriteFailed),
                ]));

        Assert.Equal(CliSemanticStatus.Failed, precedence.Status);
    }

    [Fact(DisplayName = "Route Update next guidance follows the accepted precedence"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void NextGuidanceFollowsAcceptedPrecedence()
    {
        AssertNext(
            RouteUpdateFindingCode.InvalidPatch,
            "open-forge route update --help",
            "Correct the named Route Update input, then rerun the request.");
        AssertNext(
            RouteUpdateFindingCode.WorkspaceLockUnavailable,
            "open-forge route update",
            "Wait for the blocking condition or inspect the changed target, then rerun Route Update from a fresh plan.");
        AssertNext(
            RouteUpdateFindingCode.RouteAmbiguous,
            "open-forge doctor",
            "Inspect the blocked workspace, route, identity, metadata, generated-region, Template, or recovery boundary before rerunning Route Update.");
        AssertNext(
            RouteUpdateFindingCode.TemplateUnavailable,
            "open-forge doctor",
            "Inspect the unavailable route, Template, projection, or recovery facts before relying on this Route Update result.");
        AssertNext(
            RouteUpdateFindingCode.TemplateBodyProtected,
            "open-forge route update",
            "review the authored body; the Template body was not applied.");
        AssertNext(
            RouteUpdateFindingCode.WriteFailed,
            "open-forge route update --verbose",
            "Report the failure and retry the same Route Update request with bounded diagnostics.");
        AssertNext(
            RouteUpdateFindingCode.Interrupted,
            "open-forge route update",
            "Rerun the same Route Update request.");
        Assert.Null(RouteUpdateTestData.Result().Next);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RouteUpdateDefinitions.ReadNextAction(
                (CliSemanticStatus)int.MaxValue,
                []));

        var retained = RouteUpdateTestData.VerifiedNoOpFormation(
            findings:
            [
                RouteUpdateTestData.Finding(RouteUpdateFindingCode.TemplateBodyProtected),
                RouteUpdateTestData.Finding(RouteUpdateFindingCode.RecoveryArtifactRetained),
            ]) with
        {
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.Retained,
                ResidualPath = "/tmp/open-forge-recovery.zip",
            },
        };
        var cleanup = RouteUpdateTestData.Result(retained).Next;

        Assert.Equal("open-forge cleanup", cleanup?.Command);
        Assert.Equal(
            "Review and remove the reported recovery artifact after confirming the verified Route Update result.",
            cleanup?.Reason);
    }

    [Theory(DisplayName = "Route Update status guidance outranks protected body review outside attention")]
    [InlineData(
        (int)CliSemanticStatus.Blocked,
        (int)RouteUpdateFindingCode.RouteAmbiguous,
        "open-forge doctor",
        "Inspect the blocked workspace, route, identity, metadata, generated-region, Template, or recovery boundary before rerunning Route Update.")]
    [InlineData(
        (int)CliSemanticStatus.Incomplete,
        (int)RouteUpdateFindingCode.TemplateUnavailable,
        "open-forge doctor",
        "Inspect the unavailable route, Template, projection, or recovery facts before relying on this Route Update result.")]
    [InlineData(
        (int)CliSemanticStatus.Failed,
        (int)RouteUpdateFindingCode.WriteFailed,
        "open-forge route update --verbose",
        "Report the failure and retry the same Route Update request with bounded diagnostics.")]
    [InlineData(
        (int)CliSemanticStatus.Interrupted,
        (int)RouteUpdateFindingCode.Interrupted,
        "open-forge route update",
        "Rerun the same Route Update request.")]
    [Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void NonAttentionStatusGuidanceOutranksProtectedBodyReview(
        int statusValue,
        int findingValue,
        string expectedCommand,
        string expectedReason)
    {
        var expectedStatus = (CliSemanticStatus)statusValue;
        var finding = (RouteUpdateFindingCode)findingValue;
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            findings:
            [
                RouteUpdateTestData.Finding(RouteUpdateFindingCode.TemplateBodyProtected),
                RouteUpdateTestData.Finding(finding),
            ]);

        var result = RouteUpdateTestData.Result(formation);

        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedCommand, result.Next?.Command);
        Assert.Equal(expectedReason, result.Next?.Reason);
    }

    private static void AssertStatus(
        RouteUpdateFindingCode code,
        CliSemanticStatus expected)
    {
        var result = RouteUpdateTestData.Result(
            RouteUpdateTestData.VerifiedNoOpFormation(
                findings: [RouteUpdateTestData.Finding(code)]));

        Assert.Equal(expected, result.Status);
    }

    private static void AssertNext(
        RouteUpdateFindingCode code,
        string command,
        string reason)
    {
        var result = RouteUpdateTestData.Result(
            RouteUpdateTestData.VerifiedNoOpFormation(
                findings: [RouteUpdateTestData.Finding(code)]));

        Assert.Equal(command, result.Next?.Command);
        Assert.Equal(reason, result.Next?.Reason);
    }
}
