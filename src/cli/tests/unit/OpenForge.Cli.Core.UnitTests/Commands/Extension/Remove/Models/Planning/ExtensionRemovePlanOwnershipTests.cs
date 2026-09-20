using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove.Models.Planning;

public sealed class ExtensionRemovePlanOwnershipTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Remove Plan isolates each mutable caller topology layer"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    [InlineData("target-dictionary")]
    [InlineData("target-bytes")]
    [InlineData("entry-dictionary")]
    [InlineData("entry-list")]
    [InlineData("protected-set")]
    public void TopologyPreservesAcceptedValues(string layer)
    {
        var fixture = new Fixture();
        var plan = ExtensionRemovePlan.Create(fixture.Input);

        switch (layer)
        {
            case "target-dictionary":
                fixture.Targets[".agents/loader.md"] = "changed"u8.ToArray();
                Assert.Equal("changed"u8.ToArray(), fixture.Targets[".agents/loader.md"]);
                break;
            case "target-bytes":
                fixture.Bytes[0] = (byte)'x';
                Assert.Equal((byte)'x', fixture.Bytes[0]);
                break;
            case "entry-dictionary":
                fixture.EntryMap[".agents/loader.md"] = [];
                Assert.Empty(fixture.EntryMap[".agents/loader.md"]);
                break;
            case "entry-list":
                fixture.Entries.Clear();
                Assert.Empty(fixture.Entries);
                break;
            case "protected-set":
                fixture.ProtectedPaths.Clear();
                Assert.Empty(fixture.ProtectedPaths);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(layer), layer, "The caller topology layer is not defined.");
        }

        Assert.Equal("navigation"u8.ToArray(), plan.Topology.IntendedTargetBytes[".agents/loader.md"]);
        var entry = Assert.Single(plan.Topology.GeneratedEntries[".agents/loader.md"]);
        Assert.Equal(".agents/retained.md", entry.CanonicalPath);
        Assert.Equal("Retained source", entry.Description);
        Assert.Equal("retained.md", entry.Destination);
        Assert.Equal(["base"], entry.Tags);
        Assert.Equal("- [Retained](retained.md)", entry.Line);
        Assert.Equal(".agents/retained.md", Assert.Single(plan.Topology.ProtectedPaths));
    }

    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Remove Plan preserves no-op and blocked classifications after caller decision replacement"), Trait("Feature", "extension-remove"), Trait("Evidence", "Unit")]
    [InlineData(false)]
    [InlineData(true)]
    public void PlanningPreservesAcceptedClassifications(bool inspectBlocked)
    {
        var fixture = new Fixture();
        var plan = ExtensionRemovePlan.Create(fixture.Input);

        fixture.Decisions[0] = fixture.Decisions[0] with { Disposition = ExtensionRemovePlanningDisposition.Blocked };

        Assert.False(fixture.Input.Planning.IsNoOp);
        Assert.True(fixture.Input.Planning.IsBlocked);
        Assert.Equal(".agents/toolkit.md", Assert.Single(plan.Planning.Decisions).Path.Path);
        if (inspectBlocked)
        {
            Assert.False(plan.Planning.IsBlocked);
        }
        else
        {
            Assert.True(plan.Planning.IsNoOp);
        }
    }

    private sealed class Fixture
    {
        internal Fixture()
        {
            var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-remove-ownership"));
            var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
            var source = new SourceLogicalSource(
                new SourceLogicalIdentity("retained", ".agents/retained.md"),
                new SourceLayer(".agents/retained.md", Path.Combine(root, ".agents", "retained.md"), SourceDocumentForm.Markdown, SourceLayerKind.Base));
            Targets.Add(".agents/loader.md", Bytes);
            Entries.Add(new GeneratedNavigationEntry(source, "Retained source", "retained.md", ["base"], "- [Retained](retained.md)"));
            EntryMap.Add(".agents/loader.md", Entries);
            Decisions.Add(new ExtensionRemovePlanningDecision
            {
                Path = new ExtensionRemovePathPlan(
                    ".agents/toolkit.md", ExtensionRemovePathClassification.Shared,
                    ["toolkit"], ["other"], ExtensionRemovePathAction.RetainShared),
                Disposition = ExtensionRemovePlanningDisposition.Retain,
            });
            Input = new ExtensionRemovePlanInput
            {
                Request = new ExtensionRemoveRequest(workspace, ExtensionRemoveMode.Apply, ["toolkit"], true, false),
                Selection = new ExtensionRemoveSelection(ExtensionRemoveSelectionKind.ExplicitIds, ["toolkit"]),
                Dependencies = new ExtensionRemoveDependencyPlan(
                    [new ExtensionRemovePackageFact("toolkit", true, []), new ExtensionRemovePackageFact("other", false, [])],
                    ["toolkit"], [], []),
                Planning = new ExtensionRemovePlanningPlan
                {
                    Decisions = Decisions,
                },
                Topology = new ExtensionRemoveTopology
                {
                    IntendedTargetBytes = Targets,
                    GeneratedEntries = EntryMap,
                    ProtectedPaths = ProtectedPaths,
                },
                Effects = [],
            };
        }

        internal byte[] Bytes { get; } = "navigation"u8.ToArray();

        internal Dictionary<string, byte[]> Targets { get; } = new(StringComparer.Ordinal);

        internal List<GeneratedNavigationEntry> Entries { get; } = [];

        internal Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>> EntryMap { get; } = new(StringComparer.Ordinal);

        internal HashSet<string> ProtectedPaths { get; } = new(StringComparer.Ordinal) { ".agents/retained.md" };

        internal List<ExtensionRemovePlanningDecision> Decisions { get; } = [];

        internal ExtensionRemovePlanInput Input { get; }
    }
}
