using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update.Shared.Planning;

public sealed class ExtensionUpdatePlanOwnershipIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Update Plan retains accepted topology input graphs and immutable ownership"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData("target-dictionary")]
    [InlineData("target-bytes")]
    [InlineData("generated-dictionary")]
    [InlineData("generated-bytes")]
    [InlineData("regions")]
    [InlineData("entry-dictionary")]
    [InlineData("entry-list")]
    [InlineData("protected-set")]
    public async Task CallerMutationDoesNotChangeAcceptedFacts(string layer)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-plan-ownership");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-plan-ownership-source");
        source.AddPackage("base", [], (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage("toolkit", ["base"], (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        var install = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, install.ExitCode);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionUpdatePlanner(
            sourceReader: new ExtensionSourceReader(resolver),
            validator: new FileExpectationValidator(resolver),
            physicalPathResolver: resolver,
            selectionPrompt: ExtensionInteractionTestFactory.UnavailableSelection,
            selectionQuestion: ExtensionUpdateWording.Selection());
        var build = await planner.BuildAsync(new ExtensionUpdateRequest(
            workspace: workspace.Workspace,
            mode: ExtensionUpdateMode.DryRun,
            requestedIds: ["toolkit"],
            all: false,
            sourcePath: source.Path,
            force: false,
            prune: false,
            automatic: true,
            allowInteraction: false), CancellationToken.None);
        var baseline = Assert.IsType<ExtensionUpdatePlan>(build.Plan);
        var targets = baseline.Topology.IntendedTargetBytes.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
        var generated = baseline.Topology.GeneratedTargetBytes.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
        var regions = baseline.Topology.Regions.ToList();
        var entryLists = baseline.Topology.GeneratedEntries.ToDictionary(pair => pair.Key, pair => pair.Value.ToList(), StringComparer.Ordinal);
        var entries = entryLists.ToDictionary(pair => pair.Key, pair => (IReadOnlyList<GeneratedNavigationEntry>)pair.Value, StringComparer.Ordinal);
        var protectedPaths = new HashSet<string>(baseline.Topology.ProtectedPaths, StringComparer.Ordinal);
        var result = baseline.Result;
        var accepted = ExtensionUpdatePlan.Create(new ExtensionUpdatePlanInput
        {
            Request = baseline.Request,
            SourceRead = baseline.SourceRead,
            SourceSignature = baseline.SourceSignature,
            Selection = baseline.Selection,
            Packages = baseline.Packages,
            FrameworkPayload = baseline.FrameworkPayload,
            Ownership = baseline.Ownership,
            IntendedOwnership = baseline.IntendedOwnership,
            Topology = new ExtensionUpdateTopology
            {
                IntendedTargetBytes = targets,
                GeneratedTargetBytes = generated,
                Regions = regions,
                GeneratedEntries = entries,
                ProtectedPaths = protectedPaths,
            },
            Facts = new ExtensionUpdateResultFacts
            {
                Selection = result.Selection,
                Source = result.Source,
                Packages = result.Packages,
                Comparisons = result.Comparisons,
                GeneratedNavigation = result.GeneratedNavigation,
                Effects = result.Effects,
                Permissions = result.Permissions,
                Lifecycle = result.Lifecycle,
                Recovery = result.Recovery,
                Verification = result.Verification,
            },
            Findings = result.Findings,
            Effects = baseline.Effects,
            DirectoryCreations = baseline.DirectoryCreations,
            OwnershipChange = baseline.OwnershipChange,
            OwnershipRecoveryTarget = baseline.OwnershipRecoveryTarget,
        });
        var acceptedResult = accepted.Result;
        AssertAcceptedFacts(baseline, accepted);
        const string target = ".agents/toolkit/_toolkit.md";
        const string loader = ".agents/loader.md";
        switch (layer)
        {
            case "target-dictionary":
                targets[target] = "changed"u8.ToArray();
                Assert.Equal("changed"u8.ToArray(), targets[target]);
                break;
            case "target-bytes":
                targets[target][0] = 0;
                Assert.Equal((byte)0, targets[target][0]);
                break;
            case "generated-dictionary":
                generated[loader] = "changed"u8.ToArray();
                Assert.Equal("changed"u8.ToArray(), generated[loader]);
                break;
            case "generated-bytes":
                generated[loader][0] = 0;
                Assert.Equal((byte)0, generated[loader][0]);
                break;
            case "regions":
                regions.Clear();
                Assert.Empty(regions);
                break;
            case "entry-dictionary":
                entries[loader] = [];
                Assert.Empty(entries[loader]);
                break;
            case "entry-list":
                entryLists[loader].Clear();
                Assert.Empty(entryLists[loader]);
                break;
            case "protected-set":
                protectedPaths.Clear();
                Assert.Empty(protectedPaths);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(layer), layer, "The accepted input layer is not defined.");
        }

        AssertAcceptedFacts(baseline, accepted);
        Assert.Same(acceptedResult, accepted.Result);
    }

    private static void AssertAcceptedFacts(ExtensionUpdatePlan expected, ExtensionUpdatePlan actual)
    {
        Assert.Equal(Encoding.UTF8.GetBytes(Document("Toolkit v2")), actual.Topology.IntendedTargetBytes[".agents/toolkit/_toolkit.md"]);
        Assert.Equal(expected.Topology.IntendedTargetBytes.Keys, actual.Topology.IntendedTargetBytes.Keys);
        Assert.Equal(expected.Topology.GeneratedTargetBytes.Keys, actual.Topology.GeneratedTargetBytes.Keys);
        foreach (var pair in expected.Topology.GeneratedTargetBytes)
        {
            Assert.Equal(pair.Value, actual.Topology.GeneratedTargetBytes[pair.Key]);
        }
        Assert.Equal(expected.Topology.Regions, actual.Topology.Regions);
        Assert.Equal(expected.Topology.GeneratedEntries.Keys, actual.Topology.GeneratedEntries.Keys);
        foreach (var pair in expected.Topology.GeneratedEntries)
        {
            Assert.Equal(pair.Value, actual.Topology.GeneratedEntries[pair.Key]);
        }
        Assert.NotEmpty(expected.Topology.ProtectedPaths);
        Assert.True(expected.Topology.ProtectedPaths.SetEquals(actual.Topology.ProtectedPaths));
        Assert.Equal(expected.Ownership, actual.Ownership);
        Assert.Equal(expected.IntendedOwnership, actual.IntendedOwnership);
        Assert.Equal(["base", "toolkit"], actual.Result.Packages.Select(value => value.Id));
        Assert.Equal(expected.AllFileChanges.Count, actual.AllFileChanges.Count);
        foreach (var (expectedValue, actualValue) in expected.AllFileChanges.Zip(actual.AllFileChanges))
        {
            Assert.Same(expectedValue, actualValue);
        }
        Assert.Equal(expected.Effects.Count, actual.Effects.Count);
        foreach (var (expectedValue, actualValue) in expected.Effects.Zip(actual.Effects))
        {
            Assert.Same(expectedValue, actualValue);
        }
        Assert.Same(expected.Result.Lifecycle, actual.Result.Lifecycle);
        Assert.Same(expected.Result.Recovery, actual.Result.Recovery);
        Assert.Same(expected.Result.Verification, actual.Result.Verification);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(heading, ["Extension"], $"# {heading}\n");
}
