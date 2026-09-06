using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveSelectionContractTests
{
    [Fact(DisplayName = "Extension Remove selection snapshots IDs and preserves explicit selection identity"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void SelectionSnapshotsExplicitIds()
    {
        var ids = new List<string> { "zeta", "alpha" };
        var selection = new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ids);

        ids[0] = "changed-after-construction";
        ids.Add("later");

        Assert.Equal(ExtensionRemoveSelectionKind.ExplicitIds, selection.SelectedBy);
        Assert.Equal(["zeta", "alpha"], selection.Ids);
        Assert.NotSame(ids, selection.Ids);
    }

    [Fact(DisplayName = "Extension Remove selection preserves interactive wizard identity"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void SelectionPreservesInteractiveIdentity()
    {
        var selection = new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.InteractiveIds,
            ["toolkit"]);

        Assert.Equal(ExtensionRemoveSelectionKind.InteractiveIds, selection.SelectedBy);
        Assert.Equal(["toolkit"], selection.Ids);
    }

    [Fact(DisplayName = "Extension Remove selection rejects undefined kinds, null IDs, duplicates, blanks, and empty input"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void SelectionRejectsInvalidKindsAndIds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionRemoveSelection(
            (ExtensionRemoveSelectionKind)int.MaxValue,
            ["toolkit"]));
        Assert.Throws<ArgumentNullException>(() => new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            null!));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            []));
        Assert.Throws<ArgumentNullException>(() => new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["toolkit", null!]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["toolkit", "toolkit"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveSelection(
            ExtensionRemoveSelectionKind.ExplicitIds,
            ["   "]));
    }
}
