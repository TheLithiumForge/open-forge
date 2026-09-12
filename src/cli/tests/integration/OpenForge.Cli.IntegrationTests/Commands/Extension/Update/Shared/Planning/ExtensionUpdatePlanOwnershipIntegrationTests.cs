using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update.Shared.Planning;

public sealed class ExtensionUpdatePlanOwnershipIntegrationTests
{
    [Theory(DisplayName = "Update Plan retains accepted topology and lifecycle input graphs"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData("target-dictionary")]
    [InlineData("target-bytes")]
    [InlineData("generated-dictionary")]
    [InlineData("generated-bytes")]
    [InlineData("regions")]
    [InlineData("entry-dictionary")]
    [InlineData("entry-list")]
    [InlineData("protected-set")]
    [InlineData("framework-targets")]
    [InlineData("framework-regions")]
    [InlineData("current-packages")]
    [InlineData("current-dependencies")]
    [InlineData("current-paths")]
    [InlineData("current-owners")]
    [InlineData("intended-packages")]
    [InlineData("intended-dependencies")]
    [InlineData("intended-paths")]
    [InlineData("intended-owners")]
    public async Task CallerMutationDoesNotChangeAcceptedFacts(string layer)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-plan-ownership");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-plan-ownership-source");
        source.AddPackage("base", [], (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage("toolkit", ["base"], (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        var install = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic", "--json"]);
        Assert.Equal(0, install.ExitCode);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionUpdatePlanner(
            sourceReader: new ExtensionSourceReader(resolver),
            lifecycleStore: new LifecycleStore(resolver),
            validator: new FileExpectationValidator(resolver),
            frameworkCurrentness: new FrameworkLifecycleCurrentnessReader(resolver),
            physicalPathResolver: resolver);
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
        var framework = CopyFramework(baseline.FrameworkLifecycle);
        var current = CopyExtensions(baseline.CurrentLifecycle);
        var intended = CopyExtensions(baseline.IntendedLifecycle);
        var result = baseline.Result;
        var accepted = ExtensionUpdatePlan.Create(new ExtensionUpdatePlanInput
        {
            Request = baseline.Request,
            SourceRead = baseline.SourceRead,
            SourceSignature = baseline.SourceSignature,
            Selection = baseline.Selection,
            Packages = baseline.Packages,
            FrameworkPayload = baseline.FrameworkPayload,
            FrameworkLifecycle = framework,
            LifecycleRead = baseline.LifecycleRead,
            CurrentLifecycle = current,
            IntendedLifecycle = intended,
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
            LifecycleChange = baseline.LifecycleChange,
            LifecycleRecoveryTarget = baseline.LifecycleRecoveryTarget,
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
            case "framework-targets":
                framework.Targets[0] = framework.Targets[1];
                Assert.Equal(framework.Targets[1].Path, framework.Targets[0].Path);
                break;
            case "framework-regions":
                framework.GeneratedRegions[0] = new FrameworkGeneratedRegion { Path = "changed.md", Region = "changed" };
                Assert.Equal("changed.md", framework.GeneratedRegions[0].Path);
                break;
            case "current-packages":
            case "intended-packages":
                var packages = layer == "current-packages" ? current.Packages : intended.Packages;
                packages[0] = packages[1];
                Assert.Equal("toolkit", packages[0].Id);
                break;
            case "current-dependencies":
            case "intended-dependencies":
                var dependencies = layer == "current-dependencies" ? current.Packages[1].Dependencies : intended.Packages[1].Dependencies;
                dependencies[0] = "changed";
                Assert.Equal("changed", dependencies[0]);
                break;
            case "current-paths":
            case "intended-paths":
                var paths = layer == "current-paths" ? current.Packages[1].Paths : intended.Packages[1].Paths;
                paths[0] = "changed.md";
                Assert.Equal("changed.md", paths[0]);
                break;
            case "current-owners":
            case "intended-owners":
                var ownership = layer == "current-owners" ? current.Paths : intended.Paths;
                var path = Assert.Single(ownership, value => value.Path == target);
                path.Owners[0] = "changed";
                Assert.Equal("changed", path.Owners[0]);
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
        Assert.Equal(expected.FrameworkLifecycle.Coverage, actual.FrameworkLifecycle.Coverage);
        Assert.Equal(expected.FrameworkLifecycle.Source.Id, actual.FrameworkLifecycle.Source.Id);
        Assert.Equal(expected.FrameworkLifecycle.Source.Version, actual.FrameworkLifecycle.Source.Version);
        Assert.Equal(expected.FrameworkLifecycle.Source.InventoryFingerprint, actual.FrameworkLifecycle.Source.InventoryFingerprint);
        Assert.Equal(
            expected.FrameworkLifecycle.Targets.Select(value => (value.Path, value.SourceAssetPath, value.Region, value.BaselineFingerprint, value.FingerprintKind)),
            actual.FrameworkLifecycle.Targets.Select(value => (value.Path, value.SourceAssetPath, value.Region, value.BaselineFingerprint, value.FingerprintKind)));
        Assert.Equal(
            expected.FrameworkLifecycle.GeneratedRegions.Select(value => (value.Path, value.Region)),
            actual.FrameworkLifecycle.GeneratedRegions.Select(value => (value.Path, value.Region)));
        AssertExtensions(expected.CurrentLifecycle, actual.CurrentLifecycle);
        AssertExtensions(expected.IntendedLifecycle, actual.IntendedLifecycle);
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

    private static void AssertExtensions(ExtensionLifecycleState expected, ExtensionLifecycleState actual)
    {
        Assert.Equal("complete", actual.Coverage);
        Assert.Equal(["base", "toolkit"], actual.Packages.Select(value => value.Id));
        Assert.Equal(expected.Packages.Length, actual.Packages.Length);
        foreach (var (expectedValue, actualValue) in expected.Packages.Zip(actual.Packages))
        {
            Assert.Equal(expectedValue.Id, actualValue.Id);
            Assert.Equal("1.0.0", actualValue.Version);
            Assert.Equal(expectedValue.Source, actualValue.Source);
            Assert.Equal(expectedValue.Dependencies, actualValue.Dependencies);
            Assert.Equal(expectedValue.Paths, actualValue.Paths);
        }
        Assert.Equal(["base"], actual.Packages[1].Dependencies);
        Assert.Equal([".agents/toolkit/_toolkit.md"], actual.Packages[1].Paths);
        Assert.Equal(expected.Paths.Length, actual.Paths.Length);
        foreach (var (expectedValue, actualValue) in expected.Paths.Zip(actual.Paths))
        {
            Assert.Equal(expectedValue.Path, actualValue.Path);
            Assert.Equal(expectedValue.Owners, actualValue.Owners);
            Assert.Equal(expectedValue.BaselineFingerprint, actualValue.BaselineFingerprint);
            Assert.Equal(expectedValue.FingerprintKind, actualValue.FingerprintKind);
        }
        Assert.Equal(["toolkit"], Assert.Single(actual.Paths, value => value.Path == ".agents/toolkit/_toolkit.md").Owners);
    }

    private static FrameworkLifecycleState CopyFramework(FrameworkLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = value.Source.Id,
                Version = value.Source.Version,
                InventoryFingerprint = value.Source.InventoryFingerprint,
            },
            Targets = [.. value.Targets.Select(target => new FrameworkLifecycleTarget
            {
                Path = target.Path,
                SourceAssetPath = target.SourceAssetPath,
                Region = target.Region,
                BaselineFingerprint = target.BaselineFingerprint,
                FingerprintKind = target.FingerprintKind,
            })],
            GeneratedRegions = [.. value.GeneratedRegions.Select(region => new FrameworkGeneratedRegion
            {
                Path = region.Path,
                Region = region.Region,
            })],
        };

    private static ExtensionLifecycleState CopyExtensions(ExtensionLifecycleState value)
        => new()
        {
            Coverage = value.Coverage,
            Packages = [.. value.Packages.Select(package => new LifecycleExtensionPackageV1
            {
                Id = package.Id,
                Version = package.Version,
                Source = package.Source,
                Dependencies = [.. package.Dependencies],
                Paths = [.. package.Paths],
            })],
            Paths = [.. value.Paths.Select(path => new LifecycleExtensionPathV1
            {
                Path = path.Path,
                Owners = [.. path.Owners],
                BaselineFingerprint = path.BaselineFingerprint,
                FingerprintKind = path.FingerprintKind,
            })],
        };

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(heading, ["Extension"], $"# {heading}\n");
}
