using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePathPlanContractTests
{
    [Fact(DisplayName = "Extension Remove path plans preserve shared, final-owner, changed, and missing classifications"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PathPlansPreserveClassificationsAndActions()
    {
        var shared = PathPlan(
            ".agents/shared.md",
            ExtensionRemovePathClassification.Shared,
            ["zeta", "alpha"],
            ["other"],
            ExtensionRemovePathAction.RetainShared);
        var unchanged = PathPlan(
            ".agents/unchanged.md",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete);
        var changedKeep = PathPlan(
            ".agents/changed-keep.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.KeepAsUnmanaged);
        var changedDelete = PathPlan(
            ".agents/changed-delete.md",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete);
        var missing = PathPlan(
            ".agents/missing.md",
            ExtensionRemovePathClassification.Missing,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.ReleaseOwnership);

        Assert.Equal(["alpha", "zeta"], shared.SelectedOwnerIds);
        Assert.Equal(["other"], shared.RemainingOwnerIds);
        Assert.Equal(ExtensionRemovePathAction.RetainShared, shared.Action);
        Assert.Equal(ExtensionRemovePathAction.Delete, unchanged.Action);
        Assert.Equal(ExtensionRemovePathAction.KeepAsUnmanaged, changedKeep.Action);
        Assert.Equal(ExtensionRemovePathAction.Delete, changedDelete.Action);
        Assert.Equal(ExtensionRemovePathAction.ReleaseOwnership, missing.Action);
    }

    [Fact(DisplayName = "Extension Remove path plans reject inconsistent classifications and actions"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PathPlansRejectInconsistentClassificationsAndActions()
    {
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.Shared,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.RetainShared));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.Shared,
            ["toolkit"],
            ["other"],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.KeepAsUnmanaged));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.Missing,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            ["other"],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathPlan(
            "path",
            (ExtensionRemovePathClassification)int.MaxValue,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentOutOfRangeException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.ChangedFinalOwner,
            ["toolkit"],
            [],
            (ExtensionRemovePathAction)int.MaxValue));
    }

    [Fact(DisplayName = "Extension Remove path plans reject empty, duplicate, and overlapping owners"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PathPlansRejectInvalidOwners()
    {
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            [],
            [],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            ["toolkit", "toolkit"],
            [],
            ExtensionRemovePathAction.Delete));
        Assert.Throws<ArgumentException>(() => PathPlan(
            "path",
            ExtensionRemovePathClassification.Shared,
            ["toolkit"],
            ["toolkit"],
            ExtensionRemovePathAction.RetainShared));
        Assert.Throws<ArgumentException>(() => PathPlan(
            " ",
            ExtensionRemovePathClassification.UnchangedFinalOwner,
            ["toolkit"],
            [],
            ExtensionRemovePathAction.Delete));
    }

    private static ExtensionRemovePathPlan PathPlan(
        string path,
        ExtensionRemovePathClassification classification,
        IEnumerable<string> selectedOwnerIds,
        IEnumerable<string> remainingOwnerIds,
        ExtensionRemovePathAction action)
        => new(path, classification, selectedOwnerIds, remainingOwnerIds, action);
}
