using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove.Shared.Planning;

public sealed class ExtensionRemovePlanOwnershipIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Remove revalidation retains accepted caller decisions and target bytes"), Trait("Feature", "extension-remove"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AcceptedInputsRemainEqualToTheObservedPlan(bool replaceDecision)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-remove-plan-ownership");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-remove-plan-ownership-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", OpenForgeDocumentSeed.Metadata("Toolkit", ["Extension"], "# Toolkit\n")));
        var install = await workspace.RunAsync(["extension", "install", "toolkit", "--source", source.Path, "--automatic", "--format", "json"]);
        Assert.Equal(0, install.ExitCode);
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionRemovePlanner(
            selectionPrompt: ExtensionInteractionTestFactory.UnavailableSelection,
            selectionQuestion: ExtensionRemoveWording.Selection(),
            physicalPathResolver: resolver);
        var build = await planner.BuildAsync(
            new ExtensionRemoveRequest(workspace.Workspace, ExtensionRemoveMode.DryRun, ["toolkit"], true, false),
            CancellationToken.None);
        var baseline = Assert.IsType<ExtensionRemovePlan>(build.Plan);
        Assert.All(baseline.Planning.Decisions, decision => Assert.NotNull(decision.Path.LibraryBoundary));
        var decisions = baseline.Planning.Decisions.ToList();
        var targets = baseline.Topology.IntendedTargetBytes.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
        var accepted = ExtensionRemovePlan.Create(new ExtensionRemovePlanInput
        {
            Request = baseline.Request,
            SettingsObservation = baseline.SettingsObservation,
            RemovalSelection = baseline.RemovalSelection,
            SettingsChange = baseline.SettingsChange,
            SettingsRecoveryTarget = baseline.SettingsRecoveryTarget,
            SettingsEffect = baseline.SettingsEffect,
            Selection = baseline.Selection,
            Dependencies = baseline.Dependencies,
            Planning = baseline.Planning with { Decisions = decisions },
            Topology = new ExtensionRemoveTopology
            {
                IntendedTargetBytes = targets,
                GeneratedEntries = baseline.Topology.GeneratedEntries,
                ProtectedPaths = baseline.Topology.ProtectedPaths,
            },
            Effects = baseline.Effects,
            OwnershipChange = baseline.OwnershipChange,
            OwnershipRecoveryTarget = baseline.OwnershipRecoveryTarget,
        });
        Assert.True(ExtensionRemovePlanComparer.Matches(baseline, accepted));

        if (replaceDecision)
        {
            decisions[0] = decisions[0] with { Disposition = ExtensionRemovePlanningDisposition.Blocked };
            Assert.Equal(ExtensionRemovePlanningDisposition.Blocked, decisions[0].Disposition);
        }
        else
        {
            Assert.Contains(".agents/loader.md", targets.Keys);
            targets[".agents/loader.md"][0] = 0;
            Assert.Equal((byte)0, targets[".agents/loader.md"][0]);
        }

        Assert.True(ExtensionRemovePlanComparer.Matches(baseline, accepted));
        Assert.Equal(["toolkit"], accepted.Selection.Ids);
        Assert.Equal(baseline.Topology.IntendedTargetBytes[".agents/loader.md"], accepted.Topology.IntendedTargetBytes[".agents/loader.md"]);
        Assert.False(accepted.Planning.IsBlocked);
    }
}
