using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Presentation.Extension.Remove.Shared.Wording;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

public sealed class ExtensionRemovePriorRecoveryIntegrationTests
{
    private const string Target = ".agents/toolkit.md";

    [Trait("Boundary", "OS")]
    [Fact(
        DisplayName = "Extension Remove preserves a valid prior recovery final while retaining its own recovery")]
    public async Task ValidPriorRecoveryFinalDoesNotBlockIndependentRemoval()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-prior-recovery-valid");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-prior-recovery-valid-source");
        source.AddPackage("toolkit", [], (Target, Document("Toolkit v1")));
        await InstallAsync(workspace, source);

        source.ReplacePayload("toolkit", Target, Document("Toolkit v2"));
        var sourceBefore = source.Snapshot();
        var update = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, update.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, update.Status);
        Assert.Equal(string.Empty, update.StandardError);
        using var updateDocument = JsonDocument.Parse(update.StandardOutput);
        var priorPath = updateDocument.RootElement
            .GetProperty("recovery")
            .GetProperty("path")
            .GetString();
        Assert.NotNull(priorPath);
        var priorBytes = File.ReadAllBytes(priorPath!);
        var priorRead = await RecoveryBundleReader.ReadFinalAsync(
            RecoveryStoreLocator.Local(workspace.Workspace),
            priorPath!,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, priorRead.State);

        var remove = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, remove.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, remove.Status);
        Assert.Equal(string.Empty, remove.StandardError);
        using var removeDocument = JsonDocument.Parse(remove.StandardOutput);
        Assert.Empty(removeDocument.RootElement.GetProperty("findings").EnumerateArray());
        var removePath = removeDocument.RootElement
            .GetProperty("recovery")
            .GetProperty("path")
            .GetString();
        Assert.NotNull(removePath);
        Assert.NotEqual(priorPath, removePath);
        Assert.True(File.Exists(removePath));
        Assert.Equal(priorBytes, File.ReadAllBytes(priorPath!));
        var removeRead = await RecoveryBundleReader.ReadFinalAsync(
            RecoveryStoreLocator.Local(workspace.Workspace),
            removePath!,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, removeRead.State);
        Assert.False(File.Exists(workspace.Combine(Target)));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(
        DisplayName = "Extension Remove warns about malformed or draft recovery candidates and completes removal")]
    [InlineData("malformed")]
    [InlineData("draft")]
    public async Task UnverifiedPriorRecoveryCandidateAllowsRemoval(string candidateKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-remove-prior-recovery-{candidateKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-remove-prior-recovery-{candidateKind}-source");
        source.AddPackage("toolkit", [], (Target, Document("Toolkit")));
        await InstallAsync(workspace, source);

        var directory = RecoveryBundleStoreIntegrationTests.WorkspaceDirectory(workspace.Workspace);
        Directory.CreateDirectory(directory);
        var operationId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var fileName = candidateKind == "draft"
            ? RecoveryBundleFormatV1.DraftFileName(operationId)
            : RecoveryBundleFormatV1.FinalFileName(operationId);
        var candidatePath = Path.Combine(directory, fileName);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        File.WriteAllBytes(candidatePath, candidateBytes);
        var beforeSource = source.Snapshot();

        await ExtensionRecoveryHistoryFixture.MatchHistoryAsync(workspace, source.Path,
            new ReadOutputScenario
            {
                Situation = candidateKind,
                Arguments = ["extension", "remove", "toolkit", "--dry-run"],
                ExitCode = 2,
            }, testName: $"UnverifiedHistory.{candidateKind}");

        var run = await workspace.RunAsync(
        [
            "extension", "remove", "toolkit",
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(2, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString()
                == "extension-remove.recovery-history");
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(candidateBytes, File.ReadAllBytes(candidatePath));
        Assert.False(File.Exists(workspace.Combine(Target)));
    }

    [Theory, InlineData(false), InlineData(true)]
    public async Task MissingOrChangedOwnPreparationStillBlocksRemoval(bool missing)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-remove-own-preparation");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-remove-own-preparation-source");
        source.AddPackage("toolkit", [], (Target, Document("Toolkit")));
        await InstallAsync(workspace, source);
        var history = ExtensionRecoveryHistoryFixture.CandidatePath(workspace.Workspace,
            Guid.NewGuid(), RecoveryBundleCandidateKind.Draft);
        File.WriteAllText(history, "prior draft");
        var request = new ExtensionRemoveRequest(workspace.Workspace, ExtensionRemoveMode.Apply,
            ["toolkit"], automatic: true, allowInteraction: false);
        var planner = new ExtensionRemovePlanner(ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionRemoveWording.Selection(), new PhysicalPathResolver());
        var build = await planner.BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionRemovePlan>(build.Plan);
        var execution = ExtensionRemoveExecutionPlanComposer.Compose(plan,
            new ExtensionPermissionStage(null, WorkspacePermissionResult.NotEvaluated, null, null, null));
        var prepared = await ExtensionRemoveRecoveryApplication.PrepareAsync(execution, Guid.NewGuid(),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var legacyCopy = await ExtensionRecoveryHistoryFixture.CopyToLegacyAsync(preparation, TestContext.Current.CancellationToken);
        var legacyBytes = File.ReadAllBytes(legacyCopy);
        try
        {
            var valid = await planner.BuildForVerificationAsync(request, preparation, TestContext.Current.CancellationToken);
            Assert.NotNull(valid.Plan);
            Assert.DoesNotContain(valid.Result.Findings, finding => finding.Target == preparation.BundlePath);
            if (missing) File.Delete(preparation.BundlePath);
            else File.WriteAllText(preparation.BundlePath, "changed preparation");
            var before = workspace.Snapshot();

            var verification = await planner.BuildForVerificationAsync(request, preparation, TestContext.Current.CancellationToken);

            Assert.Null(verification.Plan);
            var failure = Assert.Single(verification.Result.Findings);
            Assert.Equal(ExtensionRemoveFindingCode.RecoveryConflict, failure.Code);
            Assert.Equal(preparation.BundlePath, failure.Target);
            Assert.Equal(before, workspace.Snapshot());
            Assert.Equal("prior draft", File.ReadAllText(history));
            Assert.True(File.Exists(workspace.Combine(Target)));
            Assert.Equal(legacyBytes, File.ReadAllBytes(legacyCopy));
        }
        finally
        {
            File.Delete(legacyCopy);
        }
    }

    private static async Task InstallAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
