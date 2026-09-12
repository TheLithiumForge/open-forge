using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Document;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Reading;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Models.Planning;

public sealed class ExtensionInstallPlanOwnershipTests
{
    private const string TargetFingerprint = "34a04005bcaf206eec990bd9637d9fdb6725e0a0c0d4aebf003f17f4c956eb5c";

    [Fact(DisplayName = "Install Plan and Foundation isolate caller topology and lifecycle inputs"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void PlanAndFoundationIsolateAcceptedInputs()
    {
        var targetBytes = "target"u8.ToArray();
        var generatedBytes = "generated"u8.ToArray();
        var targets = new Dictionary<string, byte[]>(StringComparer.Ordinal) { [".agents/toolkit.md"] = targetBytes };
        var generated = new Dictionary<string, byte[]>(StringComparer.Ordinal) { [".agents/loader.md"] = generatedBytes };
        var regions = new List<ExtensionInstallGeneratedRegion> { new(".agents/loader.md", ExtensionInstallGeneratedRegionState.Unchanged) };
        var protectedPaths = new HashSet<string>(StringComparer.Ordinal) { ".agents/authored.md" };
        var forcePaths = new HashSet<string>(StringComparer.Ordinal) { ".agents/toolkit.md" };
        var topology = ExtensionInstallTopology.Create(targets, generated, regions, protectedPaths, forcePaths);
        var input = PlanInput(topology);
        var plan = ExtensionInstallPlan.Create(input);
        var foundation = new ExtensionInstallFoundation(
            input.FrameworkPayload,
            input.FrameworkLifecycle,
            LifecycleRead(input.Request.Workspace, input.CurrentLifecycle),
            input.CurrentLifecycle,
            topology);

        targetBytes[0] = (byte)'x';
        generatedBytes[0] = (byte)'x';
        targets[".agents/toolkit.md"] = "replacement"u8.ToArray();
        generated[".agents/loader.md"] = "replacement"u8.ToArray();
        regions.Clear();
        protectedPaths.Clear();
        forcePaths.Clear();
        input.FrameworkLifecycle.Targets[0] = new FrameworkLifecycleTarget
        {
            Path = "CLAUDE.md",
            SourceAssetPath = "CLAUDE.md",
            Region = null,
            BaselineFingerprint = new string('b', 64),
            FingerprintKind = "exact-bytes",
        };
        input.CurrentLifecycle.Packages[0].Paths[0] = ".agents/changed.md";
        input.IntendedLifecycle.Paths[0].Owners[0] = "changed";

        Assert.Equal((byte)'x', targetBytes[0]);
        Assert.Equal((byte)'x', generatedBytes[0]);
        Assert.Equal("replacement"u8.ToArray(), targets[".agents/toolkit.md"]);
        Assert.Equal("replacement"u8.ToArray(), generated[".agents/loader.md"]);
        Assert.Empty(regions);
        Assert.Empty(protectedPaths);
        Assert.Empty(forcePaths);
        foreach (var accepted in new[] { plan.Topology, foundation.Topology })
        {
            Assert.Equal("target"u8.ToArray(), accepted.IntendedTargetBytes[".agents/toolkit.md"]);
            Assert.Equal("generated"u8.ToArray(), accepted.GeneratedTargetBytes[".agents/loader.md"]);
            var region = Assert.Single(accepted.Regions);
            Assert.Equal(".agents/loader.md", region.Path);
            Assert.Equal(ExtensionInstallGeneratedRegionState.Unchanged, region.State);
            Assert.Equal(".agents/authored.md", Assert.Single(accepted.ProtectedPaths));
            Assert.Equal(".agents/toolkit.md", Assert.Single(accepted.InitialForceEligiblePaths));
        }
        Assert.Equal("CLAUDE.md", input.FrameworkLifecycle.Targets[0].Path);
        Assert.Equal(".agents/changed.md", input.CurrentLifecycle.Packages[0].Paths[0]);
        Assert.Equal("changed", input.IntendedLifecycle.Paths[0].Owners[0]);
        Assert.Equal("AGENTS.md", plan.FrameworkLifecycle.Targets[0].Path);
        Assert.Equal("AGENTS.md", foundation.FrameworkLifecycle.Targets[0].Path);
        Assert.Equal(".agents/toolkit.md", plan.CurrentLifecycle.Packages[0].Paths[0]);
        Assert.Equal(".agents/toolkit.md", foundation.CurrentExtensions.Packages[0].Paths[0]);
        Assert.Equal("toolkit", plan.IntendedLifecycle.Paths[0].Owners[0]);
        Assert.Equal("1.0.0", plan.FrameworkLifecycle.Source.Version);
        Assert.True(plan.IsNoOp);
    }

    [Fact(DisplayName = "Install TargetState isolates dictionary entries bytes owners occupants and lifecycle inputs"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void TargetStateIsolatesAcceptedInputs()
    {
        var bytes = "target"u8.ToArray();
        var owners = new List<string> { "toolkit" };
        var path = new ExtensionInstallIntendedPath(".agents/toolkit.md", bytes, TargetFingerprint, "exact-bytes", owners);
        var intendedPaths = new Dictionary<string, ExtensionInstallIntendedPath>(StringComparer.Ordinal) { [path.Path] = path };
        var logical = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-install-ownership", ".agents", "toolkit.md"));
        var observations = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal)
        {
            [path.Path] = FileStateSnapshot.File(logical, logical, "before"u8),
        };
        var occupants = new List<string> { path.Path };
        var lifecycle = Extensions();
        var state = new ExtensionInstallTargetState(observations, intendedPaths, occupants, lifecycle);

        bytes[0] = (byte)'x';
        owners[0] = "changed";
        var returnedBytes = path.Bytes;
        returnedBytes[0] = (byte)'y';
        intendedPaths[path.Path] = new ExtensionInstallIntendedPath(path.Path, "replacement"u8.ToArray(), new string('b', 64), "semantic", ["changed"]);
        observations[path.Path] = FileStateSnapshot.Missing(logical);
        occupants.Clear();
        lifecycle.Paths[0].Owners[0] = "changed";

        Assert.Equal((byte)'x', bytes[0]);
        Assert.Equal((byte)'y', returnedBytes[0]);
        Assert.Equal("changed", owners[0]);
        Assert.Equal("replacement"u8.ToArray(), intendedPaths[path.Path].Bytes);
        Assert.Equal(FileExpectationKind.Missing, observations[path.Path].Kind);
        Assert.Empty(occupants);
        Assert.Equal("changed", lifecycle.Paths[0].Owners[0]);
        var accepted = state.IntendedPaths[".agents/toolkit.md"];
        Assert.Equal(".agents/toolkit.md", accepted.Path);
        Assert.Equal("target"u8.ToArray(), accepted.Bytes);
        Assert.Equal(TargetFingerprint, accepted.Fingerprint);
        Assert.Equal("exact-bytes", accepted.FingerprintKind);
        Assert.Equal(["toolkit"], accepted.Owners);
        Assert.Equal("before"u8.ToArray(), state.Observations[path.Path].Bytes);
        Assert.Equal([".agents/toolkit.md"], state.EligibleOccupants);
        Assert.Equal("toolkit", state.IntendedLifecycle.Paths[0].Owners[0]);
    }

    private static ExtensionInstallPlanInput PlanInput(ExtensionInstallTopology topology)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-install-ownership"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var payload = FrameworkPayload.Create(
        [
            FrameworkPayloadAsset.Create("AGENTS.md", "agents"u8),
            FrameworkPayloadAsset.Create("CLAUDE.md", "claude"u8),
            FrameworkPayloadAsset.Create(".agents/loader.md", "loader"u8),
        ]);
        var package = ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact { Id = "toolkit", Name = "Toolkit", Description = "Toolkit package.", Version = "1.0.0", Dependencies = [] },
            new ExtensionPackageContentsFact
            {
                ManifestPath = "extension.json",
                Payload =
                [
                    ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                    {
                        Path = "content/.agents/toolkit.md",
                        TargetPath = ".agents/toolkit.md",
                        State = ExtensionPackageFileReadState.Available,
                        Bytes = "target"u8.ToArray(),
                        ByteLength = 6,
                        Sha256 = FileExpectation.Hash("target"u8),
                    }),
                ],
            });
        var selection = new ExtensionInstallSelection(ExtensionInstallSelectionKind.ExplicitIds, ["toolkit"]);
        return new ExtensionInstallPlanInput
        {
            Request = new ExtensionInstallRequest(workspace, ExtensionInstallMode.DryRun, ["toolkit"], false, null, false, true, false),
            SourceRead = new ExtensionSourceReadResult(ExtensionSourceReadState.Complete, ExtensionSourceKind.EmbeddedCatalogue, "embedded catalogue", [package], null),
            SourceSignature = "c581f9a147032eff59736cf45cbdee8708213aa0b4630f49c437a6130d75e3af",
            InferredRootId = null,
            Selection = selection,
            Packages = [package],
            FrameworkPayload = payload,
            FrameworkLifecycle = new FrameworkLifecycleState
            {
                Coverage = "complete",
                Source = new FrameworkLifecycleSource { Id = "embedded-framework", Version = "1.0.0", InventoryFingerprint = payload.InventoryFingerprint },
                Targets =
                [
                    new FrameworkLifecycleTarget
                    {
                        Path = "AGENTS.md", SourceAssetPath = "AGENTS.md", Region = null,
                        BaselineFingerprint = "8c70b25cbbbe8e7935bbc70516181551934ae5e917b40d25c4a72bbbd865f2e4", FingerprintKind = "exact-bytes",
                    },
                ],
                GeneratedRegions = [],
            },
            CurrentLifecycle = Extensions(),
            IntendedLifecycle = Extensions(),
            Topology = topology,
            Facts = new ExtensionInstallResultFacts
            {
                Selection = selection,
                Source = new ExtensionInstallSource(ExtensionInstallSourceKind.Embedded, null, "embedded catalogue", 1),
                Packages = [new ExtensionInstallPackage("toolkit", true, [])],
                Framework = new ExtensionInstallFramework(payload.InventoryFingerprint, 1, 0),
                Footprint = new ExtensionInstallFootprint(1, [".agents/toolkit.md"], [".agents/loader.md"], []),
                Effects = [],
                GeneratedNavigation = new ExtensionInstallGeneratedNavigation(topology.Regions),
                Lifecycle = new ExtensionInstallLifecycle(ExtensionInstallLifecycleAction.Preserve, ExtensionInstallLifecycleOutcome.AlreadyCurrent),
                Recovery = new ExtensionInstallRecovery(ExtensionInstallRecoveryState.NotRequired, [], null),
                Verification = new ExtensionInstallVerification(
                    ExtensionInstallVerificationState.Verified, ExtensionInstallVerificationState.Verified,
                    ExtensionInstallVerificationState.Verified, ExtensionInstallVerificationState.Verified),
            },
            Effects = [],
            LifecycleChange = null,
            LifecycleRecoveryTarget = null,
        };
    }

    private static ExtensionLifecycleState Extensions()
        => new()
        {
            Coverage = "complete",
            Packages =
            [
                new LifecycleExtensionPackageV1
                {
                    Id = "toolkit", Version = "1.0.0", Source = "embedded catalogue", Dependencies = [], Paths = [".agents/toolkit.md"],
                },
            ],
            Paths =
            [
                new LifecycleExtensionPathV1
                {
                    Path = ".agents/toolkit.md", Owners = ["toolkit"], BaselineFingerprint = TargetFingerprint, FingerprintKind = "exact-bytes",
                },
            ],
        };

    private static LifecycleStoreReadResult LifecycleRead(CliWorkspace workspace, ExtensionLifecycleState extensions)
    {
        var envelope = new LifecycleEnvelopeV1
        {
            SchemaVersion = 1,
            FingerprintPolicy = "open-forge-markdown-v1",
            WorkspacePath = workspace.LexicalRoot,
            Extensions = JsonSerializer.SerializeToElement(extensions, LifecycleJsonContext.Default.ExtensionLifecycleState),
        };
        var path = Path.Combine(workspace.LexicalRoot, ".agents/open-forge.lifecycle.json");
        return LifecycleStoreReadResult.Available(
            workspace, LifecycleSection.Extensions,
            FileStateSnapshot.File(path, path, JsonSerializer.SerializeToUtf8Bytes(envelope, LifecycleJsonContext.Default.LifecycleEnvelopeV1)),
            envelope, null, extensions);
    }
}
