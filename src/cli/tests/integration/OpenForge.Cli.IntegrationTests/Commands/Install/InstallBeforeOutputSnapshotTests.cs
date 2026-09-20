using System.Text;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class InstallBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<InstallResult> Renderers = CommandOutputRenderers<InstallResult>.From(InstallPresentation.Rendering);

    private const string OccupiedPath = ".agents/memory/_memory.md";

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves the force boundary for an occupied generated region")]
    public async Task OccupiedGeneratedRegion()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, force) in new[]
        {
            ("occupied-without-force", false),
            ("occupied-with-force", true),
        })
        {
            using var workspace = InstallOperationWorkspace.Create("install-output-occupied");
            var intended = SeedOccupiedGeneratedRegion(workspace);
            var before = workspace.SnapshotHashes();
            var result = await Execute(workspace, workspace.Request(force: force));
            Assert.Equal(force ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
            if (force)
            {
                Assert.Equal(intended, File.ReadAllText(workspace.Combine(OccupiedPath)));
                Assert.True(File.Exists(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
                Assert.Equal(InstallResultRecoveryState.Removed, result.Facts.Recovery.State);
                Assert.Equal(0, await workspace.ReadRecoveryCandidateCountAsync(TestContext.Current.CancellationToken));
                Assert.Contains(result.Facts.Effects, effect => effect.Path == OccupiedPath
                    && effect.Action == InstallEffectAction.Replace && effect.Outcome == InstallEffectOutcome.Verified);
            }
            else
            {
                Assert.Equal(before, workspace.SnapshotHashes());
                Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.TargetOccupied);
            }

            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves an unavailable recovery store without workspace effects")]
    public async Task RecoveryStoreUnavailable()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-recovery-store");
        SeedOccupiedGeneratedRegion(workspace);
        var root = RecoveryBundlePathIdentity.ResolveStoreRoot(Environment.SpecialFolderOption.None)
            ?? throw new InvalidOperationException("The Install recovery fixture requires a store root.");
        var bucket = RecoveryBundlePathIdentity.WorkspaceDirectory(root, workspace.PhysicalPath);
        Assert.False(File.Exists(bucket));
        Assert.False(Directory.Exists(bucket));
        Directory.CreateDirectory(Path.GetDirectoryName(bucket)!);
        var collision = "owned recovery bucket collision"u8.ToArray();
        File.WriteAllBytes(bucket, collision);
        try
        {
            var before = workspace.SnapshotHashes();
            var result = await Execute(workspace, workspace.Request(force: true));
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.RecoveryUnavailable);
            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(collision, File.ReadAllBytes(bucket));
            Renderers.MatchDetails(result, "recovery-store-unavailable");
        }
        finally
        {
            File.Delete(bucket);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves actual partial effects when a later replacement is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This replacement failure requires Windows file sharing enforcement.");
        }

        using var workspace = InstallOperationWorkspace.Create("install-output-partial");
        SeedOccupiedGeneratedRegion(workspace);
        var target = workspace.Combine(OccupiedPath);
        var before = File.ReadAllBytes(target);
        using var held = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await Execute(workspace, workspace.Request(force: true));
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.WriteFailed);
        Assert.Equal(before, File.ReadAllBytes(target));
        Assert.True(File.Exists(workspace.Combine(".agents/loader.md")));
        Assert.False(File.Exists(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
        Assert.Contains(result.Facts.Effects, effect => effect.Outcome == InstallEffectOutcome.Verified);
        Assert.Contains(result.Facts.Effects, effect => effect.Path == OccupiedPath && effect.Outcome == InstallEffectOutcome.NotStarted);
        Assert.Equal(InstallResultRecoveryState.Retained, result.Facts.Recovery.State);
        var bundle = Assert.IsType<string>(result.Facts.Recovery.ResidualPath);
        Assert.True(File.Exists(bundle));
        Renderers.MatchDetails(result, "write-failed-partial", recoveryBundlePath: bundle);
    }

    private static string SeedOccupiedGeneratedRegion(InstallOperationWorkspace workspace)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload
            ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
        var asset = payload.Find(OccupiedPath)
            ?? throw new InvalidOperationException("The generated Install fixture target is unavailable.");
        var intended = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
        const string current = "- [Accepted knowledge that should remain current](crystallized/_crystallized.md)";
        Assert.Contains(current, intended, StringComparison.Ordinal);
        workspace.WriteText(OccupiedPath, intended.Replace(current, "- [Stale generated entry](stale.md)", StringComparison.Ordinal));
        return intended;
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves invalid arguments before effects")]
    public async Task InvalidInput()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-invalid");
        var before = workspace.SnapshotHashes();
        await new ReadCommandOutputCapture(workspace.PhysicalPath).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["install", "unexpected-operand"],
            ExitCode = 4,
            ShellDiagnostic = true,
        });
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Fresh Install output preserves its applied or previewed result")]
    public async Task FreshDirectory()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, dryRun) in new[]
        {
            ("fresh-directory", false),
            ("fresh-directory-dry-run", true),
        })
        {
            using var workspace = InstallOperationWorkspace.Create("install-output");
            var before = workspace.SnapshotHashes();
            var result = await Execute(workspace, workspace.Request(mode: dryRun ? InstallMode.DryRun : InstallMode.Apply));
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(!dryRun, File.Exists(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
            if (dryRun)
            {
                Assert.Equal(before, workspace.SnapshotHashes());
            }

            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Already installed Framework output preserves the verified no-op")]
    public async Task AlreadyInstalled()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-current");
        Assert.Equal(CliSemanticStatus.Complete, (await Execute(workspace, workspace.Request())).Status);
        var before = workspace.SnapshotHashes();
        var result = await Execute(workspace, workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Empty(result.Facts.Effects);
        Renderers.MatchDetails(result, "already-installed");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves authored AGENTS instructions")]
    public async Task ExistingAgentsDocument()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-agents");
        const string authored = "# My workspace\n\nKeep these instructions.\n";
        workspace.WriteText("AGENTS.md", authored);
        var result = await Execute(workspace, workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.StartsWith(authored, File.ReadAllText(workspace.Combine("AGENTS.md")), StringComparison.Ordinal);
        Renderers.MatchDetails(result, "existing-agents-md");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output explains unavailable confirmation without effects")]
    public async Task ConfirmationUnavailable()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-confirmation");
        var before = workspace.SnapshotHashes();
        var result = await Execute(workspace, workspace.Request(automatic: false));
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ConfirmationRequired);
        Renderers.MatchDetails(result, "confirmation-unavailable");
    }

    private static async Task<InstallResult> Execute(InstallOperationWorkspace workspace, InstallRequest request)
    {
        return await InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot)
            .ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output directs changed Framework content to its owning update command")]
    public async Task ChangedFrameworkFile()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-changed");
        Assert.Equal(CliSemanticStatus.Complete, (await Execute(workspace, workspace.Request())).Status);
        workspace.ReplaceInstalledText(".agents/loader.md", "# User divergence\n");
        var before = workspace.SnapshotHashes();
        var result = await Execute(workspace, workspace.Request(force: true));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "changed-framework-file");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install output preserves cancellation before any effects")]
    public async Task Cancelled()
    {
        using var workspace = InstallOperationWorkspace.Create("install-output-cancelled");
        var before = workspace.SnapshotHashes();
        var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(accepted: false), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "cancelled");
    }
}
