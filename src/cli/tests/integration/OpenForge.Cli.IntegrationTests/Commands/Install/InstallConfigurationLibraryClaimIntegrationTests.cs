using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallConfigurationLibraryClaimIntegrationTests
{
    [Fact(DisplayName = "Configuration blocks a missing default recorded at an interpreted Library destination")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task MissingLibraryDestinationBlocksWholePlan()
    {
        using var workspace = InstallOperationWorkspace.Create("install-config-library-claim");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(Request(workspace, InstallPreset.Essentials, false),
            TestContext.Current.CancellationToken)).Status);
        workspace.WriteText("vendor/core/_guidance.md", "# External guidance\n");
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        var registered = read.Document with { Libraries = [new LibraryOwnership("core", "vendor/core", ".agents/guidance", ["_guidance.md"])] };
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(registered));
        var before = workspace.SnapshotHashes();

        var result = await operation.ExecuteAsync(Request(workspace, InstallPreset.FullCore, true), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.OwnershipConflict
            && finding.Subject == ".agents/guidance/_guidance.md");
        Assert.Empty(result.Facts.Effects);
        Assert.False(workspace.Exists(".agents/guidance/_guidance.md"));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.RecoveryDirectoryExists());
    }

    [Fact(DisplayName = "A changed Library destination registration invalidates confirmed configuration before effects")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task RegistrationDriftBlocksBeforeApply()
    {
        using var workspace = InstallOperationWorkspace.Create("install-config-library-drift");
        var initial = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await initial.ExecuteAsync(Request(workspace, InstallPreset.Essentials, false),
            TestContext.Current.CancellationToken)).Status);
        workspace.WriteText("vendor/core/_guidance.md", "# External guidance\n");
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        var original = read.Document with { Libraries = [new LibraryOwnership("core", "vendor/core", "external/guidance", ["_guidance.md"])] };
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(original));
        var before = workspace.SnapshotHashes();
        var changed = WorkspaceOwnershipCodec.Write(original with
        { Libraries = [new LibraryOwnership("core", "vendor/core", ".agents/guidance", ["_guidance.md"])] });
        var confirmations = 0;
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(observe: (_, _) =>
        {
            confirmations++;
            File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), changed);
        }), workspace.LockStoreRoot);
        var request = new InstallRequest(workspace.Workspace, InstallMode.Apply, false, false, true)
        { Setup = new(true, InstallPreset.FullCore, []) };

        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(1, confirmations);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.All(result.Facts.Effects, effect => Assert.Equal(InstallEffectOutcome.NotStarted, effect.Outcome));
        Assert.False(workspace.Exists(".agents/guidance/_guidance.md"));
        var after = workspace.SnapshotHashes();
        Assert.Equal(before.Where(pair => pair.Key != InstallOperationWorkspace.OwnershipPath),
            after.Where(pair => pair.Key != InstallOperationWorkspace.OwnershipPath));
        Assert.Equal(changed, File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
        Assert.False(workspace.RecoveryDirectoryExists());
    }

    private static InstallRequest Request(InstallOperationWorkspace workspace, InstallPreset preset, bool configure)
        => new(workspace.Workspace, InstallMode.Apply, false, true, false) { Setup = new(configure, preset, []) };
}
