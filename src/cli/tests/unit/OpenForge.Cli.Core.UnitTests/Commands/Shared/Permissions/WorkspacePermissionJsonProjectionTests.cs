using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Commands.Shared.Permissions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.Permissions;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionJsonProjectionTests
{
    [Trait("Boundary", "Output")]
    [Theory]
    [InlineData((int)WorkspacePermissionDecision.NotEvaluated, "not-evaluated")]
    [InlineData((int)WorkspacePermissionDecision.NotRequired, "not-required")]
    [InlineData((int)WorkspacePermissionDecision.Granted, "granted")]
    [InlineData((int)WorkspacePermissionDecision.Required, "required")]
    [InlineData((int)WorkspacePermissionDecision.Approved, "approved")]
    [InlineData((int)WorkspacePermissionDecision.Declined, "declined")]
    public void MapsEveryDecision(int value, string expected)
        => Assert.Equal(expected, WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionDecision)value));

    [Trait("Boundary", "Output")]
    [Theory]
    [InlineData((int)WorkspacePermissionAction.None, "none")]
    [InlineData((int)WorkspacePermissionAction.Create, "create")]
    [InlineData((int)WorkspacePermissionAction.Replace, "replace")]
    public void MapsEveryAction(int value, string expected)
        => Assert.Equal(expected, WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionAction)value));

    [Trait("Boundary", "Output")]
    [Theory]
    [InlineData((int)WorkspacePermissionOutcome.NotRequested, "not-requested")]
    [InlineData((int)WorkspacePermissionOutcome.Planned, "planned")]
    [InlineData((int)WorkspacePermissionOutcome.NotStarted, "not-started")]
    [InlineData((int)WorkspacePermissionOutcome.Verified, "verified")]
    [InlineData((int)WorkspacePermissionOutcome.VerificationFailed, "verification-failed")]
    [InlineData((int)WorkspacePermissionOutcome.CompletionUnknown, "completion-unknown")]
    public void MapsEveryOutcome(int value, string expected)
        => Assert.Equal(expected, WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionOutcome)value));

    [Trait("Boundary", "Output")]
    [Fact]
    public void RejectsUndefinedEnumValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionDecision)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePermissionJsonProjection.ReadName((WorkspacePermissionOutcome)int.MaxValue));
    }

    [Trait("Boundary", "Output")]
    [Fact]
    public void UnevaluatedResultDoesNotClaimNoPermissionIsRequired()
    {
        var result = WorkspacePermissionJsonProjection.Create(WorkspacePermissionResult.NotEvaluated);

        Assert.Equal(".agents/open-forge.json", result.Path);
        Assert.Equal("not-evaluated", result.Decision);
        Assert.Equal("none", result.Action);
        Assert.Equal("not-requested", result.Outcome);
        Assert.Empty(result.Required);
        Assert.Empty(result.Missing);
    }
}
