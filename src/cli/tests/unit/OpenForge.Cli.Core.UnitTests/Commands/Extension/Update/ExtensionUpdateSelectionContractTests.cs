using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Update;

public sealed class ExtensionUpdateSelectionContractTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Extension Update selection snapshots explicit roots and preserves the selected kind"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void SelectionSnapshotsExplicitRoots()
    {
        var rootIds = new[] { "toolkit", "alpha" };
        var selection = new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitIds,
            rootIds);

        rootIds[0] = "changed-after-construction";

        Assert.Equal(ExtensionUpdateSelectionKind.ExplicitIds, selection.SelectedBy);
        Assert.Equal(["toolkit", "alpha"], selection.RootIds);
        Assert.NotSame(rootIds, selection.RootIds);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Extension Update all-package selections require an empty root set"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void AllPackageSelectionsRequireEmptyRoots()
    {
        var explicitAll = new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitAll,
            []);
        var interactiveAll = new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.InteractiveAll,
            []);

        Assert.Empty(explicitAll.RootIds);
        Assert.Empty(interactiveAll.RootIds);
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitAll,
            ["toolkit"]));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.InteractiveAll,
            ["toolkit"]));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Extension Update selection rejects undefined kinds null IDs duplicate IDs and blank IDs"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void SelectionRejectsInvalidKindsAndIds()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionUpdateSelection(
            (ExtensionUpdateSelectionKind)int.MaxValue,
            []));
        Assert.Throws<ArgumentNullException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitIds,
            null!));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitIds,
            ["toolkit", null!]));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitIds,
            ["toolkit", "toolkit"]));
        Assert.Throws<ArgumentException>(() => new ExtensionUpdateSelection(
            ExtensionUpdateSelectionKind.ExplicitIds,
            ["   "]));
    }
}
