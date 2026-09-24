using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install.Models.Planning;

public sealed class ExtensionInstallPlanOwnershipTests
{
    private const string TargetFingerprint = "34a04005bcaf206eec990bd9637d9fdb6725e0a0c0d4aebf003f17f4c956eb5c";

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Install Plan and Foundation isolate caller mutable topology inputs"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
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
            WorkspaceOwnershipRead.Absent(
                Path.Combine(input.Request.Workspace.LexicalRoot, ".agents", "open-forge.lock.json")),
            topology);

        targetBytes[0] = (byte)'x';
        generatedBytes[0] = (byte)'x';
        targets[".agents/toolkit.md"] = "replacement"u8.ToArray();
        generated[".agents/loader.md"] = "replacement"u8.ToArray();
        regions.Clear();
        protectedPaths.Clear();
        forcePaths.Clear();
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
        Assert.True(plan.IsNoOp);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Install TargetState isolates dictionary entries bytes owners occupants and ownership receipts"), Trait("Feature", "extension-install"), Trait("Evidence", "Unit")]
    public void TargetStateIsolatesAcceptedInputs()
    {
        var bytes = "target"u8.ToArray();
        var owners = new List<string> { "toolkit" };
        var path = new ExtensionInstallIntendedPath(".agents/toolkit.md", bytes, TargetFingerprint, MarkdownFingerprintState.ExactBytes, owners);
        var intendedPaths = new Dictionary<string, ExtensionInstallIntendedPath>(StringComparer.Ordinal) { [path.Path] = path };
        var logical = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "extension-install-ownership", ".agents", "toolkit.md"));
        var observations = new Dictionary<string, FileStateSnapshot>(StringComparer.Ordinal)
        {
            [path.Path] = FileStateSnapshot.File(logical, logical, "before"u8),
        };
        var occupants = new List<string> { path.Path };
        var extensions = Extensions();
        var state = new ExtensionInstallTargetState(observations, intendedPaths, occupants, extensions);

        bytes[0] = (byte)'x';
        owners[0] = "changed";
        var returnedBytes = path.Bytes;
        returnedBytes[0] = (byte)'y';
        intendedPaths[path.Path] = new ExtensionInstallIntendedPath(path.Path, "replacement"u8.ToArray(), new string('b', 64), MarkdownFingerprintState.Semantic, ["changed"]);
        observations[path.Path] = FileStateSnapshot.Missing(logical);
        occupants.Clear();

        Assert.Equal((byte)'x', bytes[0]);
        Assert.Equal((byte)'y', returnedBytes[0]);
        Assert.Equal("changed", owners[0]);
        Assert.Equal("replacement"u8.ToArray(), intendedPaths[path.Path].Bytes);
        Assert.Equal(FileExpectationKind.Missing, observations[path.Path].Kind);
        Assert.Empty(occupants);
        var accepted = state.IntendedPaths[".agents/toolkit.md"];
        Assert.Equal(".agents/toolkit.md", accepted.Path);
        Assert.Equal("target"u8.ToArray(), accepted.Bytes);
        Assert.Equal(TargetFingerprint, accepted.Fingerprint);
        Assert.Equal(MarkdownFingerprintState.ExactBytes, accepted.FingerprintKind);
        Assert.Equal(["toolkit"], accepted.Owners);
        Assert.Equal("before"u8.ToArray(), state.Observations[path.Path].Bytes);
        Assert.Equal([".agents/toolkit.md"], state.EligibleOccupants);
        Assert.Equal("toolkit", state.IntendedExtensions[0].Id);
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
            SettingsObservation = WorkspaceSettingsRead.Absent(Path.Combine(root, ".agents", "open-forge.json")),
            SourceRead = new ExtensionSourceReadResult(ExtensionSourceReadState.Complete, ExtensionSourceKind.EmbeddedCatalogue, "embedded catalogue", [package], null),
            SourceSignature = "c581f9a147032eff59736cf45cbdee8708213aa0b4630f49c437a6130d75e3af",
            InferredRootId = null,
            Selection = selection,
            Packages = [package],
            FrameworkPayload = payload,
            FrameworkOwnership = null,
            Ownership = WorkspaceOwnershipRead.Absent(Path.Combine(root, ".agents", "open-forge.lock.json")),
            IntendedExtensions = Extensions(),
            Topology = topology,
            Facts = new ExtensionInstallResultFacts
            {
                Selection = selection,
                Source = new ExtensionInstallSource(ExtensionInstallSourceKind.Embedded, null, "embedded catalogue", 1),
                Packages = [new ExtensionInstallPackage("toolkit", "1.0.0", true, [])],
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
            OwnershipChange = null,
            OwnershipRecoveryTarget = null,
        };
    }

    private static ImmutableArray<ExtensionOwnership> Extensions()
        => [new("toolkit", "1.0.0", "embedded catalogue", [], [".agents/toolkit.md"], [])];
}
