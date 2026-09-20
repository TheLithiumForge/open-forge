using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Permissions;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Unit")]
public sealed class LibraryPermissionPresentationTests
{
    [Trait("Boundary", "Output")]
    [Theory]
    [InlineData((int)WorkspacePermissionOutcome.NotRequested, "not-requested")]
    [InlineData((int)WorkspacePermissionOutcome.Planned, "planned")]
    [InlineData((int)WorkspacePermissionOutcome.NotStarted, "not-started")]
    [InlineData((int)WorkspacePermissionOutcome.Verified, "verified")]
    [InlineData((int)WorkspacePermissionOutcome.VerificationFailed, "verification-failed")]
    [InlineData((int)WorkspacePermissionOutcome.CompletionUnknown, "completion-unknown")]
    public void ScopeProjectionPreservesPathAndActualWriteOutcome(int outcome, string expected)
    {
        var leaf = "docs/a.md";
        var stage = new LibraryPermissionStage
        {
            Observation = null,
            Approval = new()
            {
                Leaves = new([leaf], [leaf], WorkspacePermissionDecision.Approved),
                ProposedScopes = [new(LibraryPermissionScopeKind.Directory, "docs")],
                ApprovedScopes = [new(LibraryPermissionScopeKind.Directory, "docs")],
            },
            Result = new([leaf], [leaf], WorkspacePermissionDecision.Approved, WorkspacePermissionAction.Replace, (WorkspacePermissionOutcome)outcome),
            Change = null,
            RecoveryTarget = null,
            Failure = null,
        };

        var view = LibraryPermissionPresentation.Project(stage);

        Assert.Equal("approved", view.Decision);
        Assert.Equal("replace", view.Action);
        Assert.Equal(expected, view.Outcome);
        Assert.Equal("docs/a.md", Assert.Single(view.Required));
        Assert.Equal("directory", Assert.Single(view.ApprovedScopes).Kind);
        Assert.Equal("docs", Assert.Single(view.ApprovedScopes).Path);
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void UnevaluatedPermissionCannotClaimNoPermissionWasRequired()
    {
        var view = LibraryPermissionView.NotEvaluated();

        Assert.Equal("not-evaluated", view.Decision);
        Assert.Equal("none", view.Action);
        Assert.Equal("not-requested", view.Outcome);
        Assert.Empty(view.Required);
        Assert.Empty(view.ProposedScopes);
    }
    [Trait("Boundary", "Output")]
    [Theory]
    [InlineData((int)LibraryPermissionScopeKind.File, "file")]
    [InlineData((int)LibraryPermissionScopeKind.Directory, "directory")]
    public void ApprovedScopeKindRemainsExplicit(int kind, string expected)
    {
        var view = LibraryPermissionPresentation.Project(ScopeStage((LibraryPermissionScopeKind)kind));

        Assert.Equal(expected, Assert.Single(view.ApprovedScopes).Kind);
        Assert.Equal(expected, Assert.Single(view.ProposedScopes).Kind);
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void UndefinedScopeKindCannotBeRenderedAsAnOrdinaryGrant()
        => Assert.Throws<ArgumentOutOfRangeException>(() =>
            LibraryPermissionPresentation.Project(ScopeStage((LibraryPermissionScopeKind)int.MaxValue)));

    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData((int)RelativeFileLinkEffectKind.Create, (int)LibraryPermissionEffect.CreateLink)]
    [InlineData((int)RelativeFileLinkEffectKind.Delete, (int)LibraryPermissionEffect.RemoveLink)]
    [InlineData(null, (int)LibraryPermissionEffect.RetainLink)]
    public void PlannedEffectRetainsItsPermissionMeaning(int? kind, int expected)
        => Assert.Equal((LibraryPermissionEffect)expected, LibraryPermissionPresentation.ReadEffect((RelativeFileLinkEffectKind?)kind));

    [Trait("Boundary", "Processing")]
    [Fact]
    public void UndefinedPlannedEffectCannotBecomeRetention()
        => Assert.Throws<ArgumentOutOfRangeException>(() => LibraryPermissionPresentation.ReadEffect((RelativeFileLinkEffectKind)int.MaxValue));

    private static LibraryPermissionStage ScopeStage(LibraryPermissionScopeKind kind)
    {
        var leaf = kind == LibraryPermissionScopeKind.Directory ? "docs/a.md" : "docs";
        var scope = new LibraryPermissionScope(kind, "docs");
        return new()
        {
            Observation = null,
            Approval = new()
            {
                Leaves = new([leaf], [leaf], WorkspacePermissionDecision.Approved),
                ProposedScopes = [scope],
                ApprovedScopes = [scope],
            },
            Result = new([leaf], [leaf], WorkspacePermissionDecision.Approved, WorkspacePermissionAction.Create, WorkspacePermissionOutcome.Planned),
            Change = null,
            RecoveryTarget = null,
            Failure = null,
        };
    }
}
