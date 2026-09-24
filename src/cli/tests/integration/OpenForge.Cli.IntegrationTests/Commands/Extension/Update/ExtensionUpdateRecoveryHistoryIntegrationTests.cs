using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Presentation.Extension.Update.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Recovery;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateRecoveryHistoryIntegrationTests
{
    private const string Target = ".agents/toolkit.txt";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update retains prior verified recovery history while verifying its own preparation")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task VerifiedPriorHistoryDoesNotBlockUpdatePreparation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-verified-recovery-history");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-verified-recovery-history-source");
        source.AddPackage("toolkit", [], (Target, "version one\n"));
        await InstallAsync(workspace, source);
        var prior = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
            workspace,
            ExtensionInstallDefinitions.CommandIdentity,
            RecoveryBundleOperation.Install,
            TestContext.Current.CancellationToken);
        var priorBytes = await File.ReadAllBytesAsync(prior.BundlePath, TestContext.Current.CancellationToken);
        source.ReplacePayload("toolkit", Target, "version two\n");
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal("version two\n", workspace.ReadText(Target));
        using var document = JsonDocument.Parse(run.StandardOutput);
        var ownBundlePath = Assert.IsType<string>(document.RootElement.GetProperty("recovery").GetProperty("path").GetString());
        Assert.NotEqual(prior.BundlePath, ownBundlePath);
        Assert.Equal(priorBytes, await File.ReadAllBytesAsync(prior.BundlePath, TestContext.Current.CancellationToken));
        var priorRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            prior.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, priorRead.State);
        var ownRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            ownBundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, ownRead.State);
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update blocks unverified recovery history at its exact path")]
    [InlineData("malformed")]
    [InlineData("draft")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task UnverifiedRecoveryHistoryBlocksWriteFree(string candidateKind)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            $"extension-update-unverified-recovery-{candidateKind}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            $"extension-update-unverified-recovery-{candidateKind}-source");
        source.AddPackage("toolkit", [], (Target, "version one\n"));
        await InstallAsync(workspace, source);
        var kind = candidateKind == "draft"
            ? RecoveryBundleCandidateKind.Draft
            : RecoveryBundleCandidateKind.Final;
        var candidatePath = ExtensionRecoveryHistoryFixture.CandidatePath(
            workspace.Workspace,
            Guid.NewGuid(),
            kind);
        var candidateBytes = candidateKind == "draft"
            ? "incomplete draft"u8.ToArray()
            : [0x01, 0x02, 0x03, 0x04];
        await File.WriteAllBytesAsync(candidatePath, candidateBytes, TestContext.Current.CancellationToken);
        source.ReplacePayload("toolkit", Target, "version two\n");
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("extension-update.recovery-conflict", finding.GetProperty("code").GetString());
        Assert.Equal(candidatePath, finding.GetProperty("subject").GetProperty("path").GetString());
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(candidateBytes, await File.ReadAllBytesAsync(candidatePath, TestContext.Current.CancellationToken));
        Assert.Equal("version one\n", workspace.ReadText(Target));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update reports a mismatched prepared recovery final at its exact path")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task MismatchedPreparedRecoveryFinalBlocksVerificationWriteFree()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-mismatched-prepared-recovery");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-mismatched-prepared-recovery-source");
        source.AddPackage("toolkit", [], (Target, "version one\n"));
        await InstallAsync(workspace, source);
        var retained = await ExtensionRecoveryHistoryFixture.CreateVerifiedFinalAsync(
            workspace,
            ExtensionInstallDefinitions.CommandIdentity,
            RecoveryBundleOperation.Install,
            TestContext.Current.CancellationToken);
        var retainedBytes = await File.ReadAllBytesAsync(retained.BundlePath, TestContext.Current.CancellationToken);
        source.ReplacePayload("toolkit", Target, "version two\n");
        var request = new ExtensionUpdateRequest(
            workspace.Workspace,
            ExtensionUpdateMode.Apply,
            ["toolkit"],
            all: false,
            sourcePath: source.Path,
            force: false,
            prune: false,
            automatic: true,
            allowInteraction: false);
        var resolver = new PhysicalPathResolver();
        var planner = new ExtensionUpdatePlanner(
            new OpenForge.Cli.Core.Framework.Extensions.ExtensionSourceReader(resolver),
            new FileExpectationValidator(resolver),
            resolver,
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionUpdateWording.Selection());
        var build = await planner.BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionUpdatePlan>(build.Plan);
        var execution = new ExtensionUpdateExecutionPlan(
            plan,
            new ExtensionPermissionStage(
                null,
                WorkspacePermissionResult.NotEvaluated,
                null,
                null,
                null));
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create("extension-update-mismatched-recovery-locks");
        var acquired = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, ExtensionUpdateDefinitions.CommandIdentity, operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await ExtensionUpdateRecoveryApplication.PrepareAsync(
            execution,
            operationId,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var observed = await RecoveryFinalIdentityFixture.ObserveAsync(workspace.Workspace, preparation);
        var changedAttribution = RecoveryBundleAttribution.Create(
            RecoveryBundleProducer.Repair,
            RecoveryBundleOperation.Repair,
            workspace.Workspace);
        await observed.ChangeAttributionAsync(changedAttribution);
        _ = await observed.AssertAdmissionAsync(preparation.Command, changedAttribution);
        var beforeVerification = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var revalidated = await planner.BuildForVerificationAsync(
            request,
            preparation,
            TestContext.Current.CancellationToken);

        Assert.Null(revalidated.Plan);
        var finding = Assert.Single(revalidated.Result.Findings);
        Assert.Equal(ExtensionUpdateFindingCode.RecoveryConflict, finding.Code);
        Assert.Equal(preparation.BundlePath, finding.Target);
        Assert.Empty(revalidated.Result.Effects);
        Assert.Equal(beforeVerification, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
        Assert.Equal(retainedBytes, await File.ReadAllBytesAsync(retained.BundlePath, TestContext.Current.CancellationToken));
        Assert.Equal("version one\n", workspace.ReadText(Target));
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
}
