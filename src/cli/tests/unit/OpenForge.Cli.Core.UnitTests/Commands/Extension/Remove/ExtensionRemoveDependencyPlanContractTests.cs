using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveDependencyPlanContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan snapshots known edges and preserves dependent-first order"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanSnapshotsEdgesAndRemovalOrder()
    {
        var toolkitDependencies = new List<string> { "library" };
        var plan = new ExtensionRemoveDependencyPlan(
        [
            Package("toolkit", selected: true, toolkitDependencies),
            Package("library", selected: true, ["core"]),
            Package("core", selected: false, []),
        ],
        ["toolkit", "library"],
        [],
        ["core"]);

        toolkitDependencies[0] = "changed-after-construction";

        Assert.Equal(["core", "library", "toolkit"], plan.Packages.Select(package => package.Id));
        Assert.Equal(["library"], plan.Packages.Single(package => package.Id == "toolkit").Dependencies);
        Assert.Equal(["toolkit", "library"], plan.RemovalOrder);
        Assert.Equal(["core"], plan.RetainedOrphanDependencyIds);
        Assert.Empty(plan.RetainedDependentBlockers);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan sorts and preserves retained-dependent blockers"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanPreservesRetainedDependentBlockers()
    {
        var blocker = new ExtensionRemoveRetainedDependentBlocker(
            "library",
            ["zeta", "alpha"]);
        var plan = new ExtensionRemoveDependencyPlan(
        [
            Package("toolkit", selected: true, ["library"]),
            Package("library", selected: true, []),
            Package("alpha", selected: false, ["library"]),
            Package("zeta", selected: false, ["library"]),
        ],
        ["toolkit", "library"],
        [blocker],
        []);

        var retained = Assert.Single(plan.RetainedDependentBlockers);
        Assert.Equal("library", retained.DependencyId);
        Assert.Equal(["alpha", "zeta"], retained.RetainedDependentIds);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan rejects backward, self, and cyclic selected order"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRejectsInvalidSelectedOrder()
    {
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
        [
            Package("toolkit", selected: true, ["library"]),
            Package("library", selected: true, []),
        ],
        ["library", "toolkit"],
        [],
        []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            [Package("toolkit", selected: true, ["toolkit"])],
            ["toolkit"],
            [],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
        [
            Package("alpha", selected: true, ["zeta"]),
            Package("zeta", selected: true, ["alpha"]),
        ],
        ["alpha", "zeta"],
        [],
        []));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan rejects unknown edges and malformed selected order"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRejectsUnknownEdgesAndMalformedOrder()
    {
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            [Package("toolkit", selected: true, ["unknown"])],
            ["toolkit"],
            [],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            [Package("toolkit", selected: true, [])],
            ["toolkit", "extra"],
            [],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            [Package("toolkit", selected: true, [])],
            ["toolkit", "toolkit"],
            [],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
        [
            Package("toolkit", selected: true, []),
            Package("toolkit", selected: false, []),
        ],
        ["toolkit"],
        [],
        []));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan rejects invalid blocker facts"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRejectsInvalidBlockers()
    {
        var packages = new[]
        {
            Package("toolkit", selected: true, ["library"]),
            Package("library", selected: true, []),
            Package("retained", selected: false, []),
        };

        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit", "library"],
            [new ExtensionRemoveRetainedDependentBlocker("library", ["missing"])],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit", "library"],
            [new ExtensionRemoveRetainedDependentBlocker("library", ["toolkit"])],
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit", "library"],
            [new ExtensionRemoveRetainedDependentBlocker("library", ["retained"])],
            []));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension Remove dependency plan rejects invalid orphan facts and duplicate model IDs"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    public void PlanRejectsInvalidOrphansAndModelIds()
    {
        var packages = new[]
        {
            Package("toolkit", selected: true, ["library"]),
            Package("library", selected: false, []),
            Package("unrelated", selected: false, []),
        };

        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit"],
            [],
            ["toolkit"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit"],
            [],
            ["missing"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit"],
            [],
            ["unrelated"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveDependencyPlan(
            packages,
            ["toolkit"],
            [],
            ["library", "library"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemovePackageFact(
            "toolkit",
            selectedForRemoval: true,
            ["library", "library"]));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRetainedDependentBlocker(
            "library",
            []));
        Assert.Throws<ArgumentException>(() => new ExtensionRemoveRetainedDependentBlocker(
            "library",
            ["alpha", "alpha"]));
    }

    private static ExtensionRemovePackageFact Package(
        string id,
        bool selected,
        IEnumerable<string> dependencies)
        => new(id, selected, dependencies);
}
