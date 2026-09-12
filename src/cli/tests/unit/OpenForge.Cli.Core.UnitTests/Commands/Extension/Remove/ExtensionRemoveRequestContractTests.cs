using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveRequestContractTests
{
    [Fact(DisplayName = "Extension Remove request snapshots IDs and preserves mode and policy flags"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void RequestSnapshotsIdsAndPolicies()
    {
        var workspace = Workspace();
        var requestedIds = new List<string> { "toolkit", "base" };
        var request = new ExtensionRemoveRequest(
            workspace,
            ExtensionRemoveMode.DryRun,
            requestedIds,
            prune: true,
            automatic: true,
            allowInteraction: false);

        requestedIds[0] = "changed-after-construction";
        requestedIds.Add("later");

        Assert.Same(workspace, request.Workspace);
        Assert.Equal(ExtensionRemoveMode.DryRun, request.Mode);
        Assert.True(request.IsDryRun);
        Assert.Equal(["toolkit", "base"], request.RequestedIds);
        Assert.NotSame(requestedIds, request.RequestedIds);
        Assert.True(request.Prune);
        Assert.True(request.Automatic);
        Assert.False(request.AllowInteraction);
    }

    [Fact(DisplayName = "Extension Remove request permits an empty ID snapshot for the human wizard"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void RequestPermitsArgumentlessPromptSelection()
    {
        var request = new ExtensionRemoveRequest(
            Workspace(),
            ExtensionRemoveMode.Apply,
            [],
            prune: false,
            automatic: false,
            allowInteraction: true);

        Assert.Empty(request.RequestedIds);
        Assert.True(request.AllowInteraction);
        Assert.False(request.IsDryRun);
    }

    [Fact(DisplayName = "Extension Remove request rejects null ingress, null IDs, and undefined modes"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void RequestRejectsInvalidIngress()
    {
        Assert.Throws<ArgumentNullException>(() => new ExtensionRemoveRequest(
            null!,
            ExtensionRemoveMode.Apply,
            ["toolkit"],
            prune: false,
            automatic: false,
            allowInteraction: false));
        Assert.Throws<ArgumentNullException>(() => new ExtensionRemoveRequest(
            Workspace(),
            ExtensionRemoveMode.Apply,
            null!,
            prune: false,
            automatic: false,
            allowInteraction: false));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRequest(
            Workspace(),
            ExtensionRemoveMode.Apply,
            ["toolkit", null!],
            prune: false,
            automatic: false,
            allowInteraction: false));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveRequest(
            Workspace(),
            (ExtensionRemoveMode)int.MaxValue,
            ["toolkit"],
            prune: false,
            automatic: false,
            allowInteraction: false));
    }

    [Fact(DisplayName = "Extension Remove request preserves duplicate IDs for binding-level validation"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void RequestPreservesDuplicatesForTypedValidation()
    {
        var request = new ExtensionRemoveRequest(
            Workspace(),
            ExtensionRemoveMode.Apply,
            ["toolkit", "toolkit"],
            prune: false,
            automatic: false,
            allowInteraction: true);

        Assert.Equal(["toolkit", "toolkit"], request.RequestedIds);
    }

    private static CliWorkspace Workspace()
        => new(
            "extension-remove-request-workspace",
            "extension-remove-request-workspace",
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
