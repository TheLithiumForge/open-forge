using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class UpdateBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<UpdateResult> Renderers = CommandOutputRenderers<UpdateResult>.From(UpdatePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update output preserves invalid arguments before effects")]
    public async Task InvalidInput()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-invalid");
        var before = workspace.SnapshotHashes();
        await new ReadCommandOutputCapture(workspace.Workspace.PhysicalRoot).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["update", "unexpected-operand"],
            ExitCode = 4,
            ShellDiagnostic = true,
        });
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Current Framework update output preserves its no-op")]
    public async Task UpToDate()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-current");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "up-to-date");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update output observes a missing ownership record without claiming prior ownership")]
    public async Task NoOwnershipRecord()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-no-ownership");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.RemoveFile(UpdateIntegrationWorkspace.OwnershipPath);
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.OwnershipObservation);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "no-ownership-record");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Framework update output preserves the real replacement or restoration")]
    [InlineData("changed-file-replaced", false)]
    [InlineData("missing-file-restored", true)]
    public async Task ManagedFileChanged(string situation, bool missing)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-change");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var expected = workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath);
        if (missing)
        {
            workspace.RemoveManagedContent();
        }
        else
        {
            workspace.MutateManagedContent();
        }

        var result = await workspace.ExecuteAsync(workspace.Request(force: true));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expected, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        Assert.NotEmpty(result.Effects);
        AssertRecovery(result);
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, testName: situation);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Framework update output preserves retired files unless pruning is selected")]
    [InlineData("retired-kept", false)]
    [InlineData("retired-pruned", true)]
    public async Task RetiredFile(string situation, bool prune)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-retired");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.SeedHistoricalRetiredTarget();
        var result = await workspace.ExecuteAsync(workspace.Request(prune: prune));
        Assert.Equal(prune ? CliSemanticStatus.Complete : CliSemanticStatus.Attention, result.Status);
        Assert.Equal(!prune, workspace.Exists(UpdateIntegrationWorkspace.HistoricalTargetPath));
        AssertRecovery(result);
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, testName: situation);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update dry-run output preserves the preview without effects")]
    public async Task DryRunChanges()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-preview");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request(mode: UpdateMode.DryRun));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "dry-run-changes");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update output explains unavailable confirmation without effects")]
    public async Task ConfirmationUnavailable()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-confirmation");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request(automatic: false));
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "confirmation-unavailable");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update output preserves cancellation before effects")]
    public async Task Cancelled()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-cancelled");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.MutateManagedContent();
        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(
            workspace.Request(automatic: false, allowsInteractiveConfirmation: true), canPrompt: true, input: "n\n");
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "cancelled");
    }

    private static void AssertRecovery(UpdateResult result)
    {
        if (result.Recovery.ResidualPath is { } path)
        {
            Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
            Assert.True(File.Exists(path));
            Assert.NotEmpty(result.Recovery.ProtectedPaths);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Framework update output preserves partial progress after a later Windows replacement is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = UpdateIntegrationWorkspace.Create("update-output-partial");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        const string firstPath = ".agents/guidance/_guidance.md";
        var firstExpected = workspace.ReadBytes(firstPath);
        workspace.ReplaceText(firstPath, workspace.ReadText(firstPath) + "\nLocal change.\n");
        workspace.MutateManagedContent();
        var blockedBefore = workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath);
        UpdateResult result;
        using (var held = File.Open(Path.Combine(workspace.Workspace.PhysicalRoot, UpdateIntegrationWorkspace.ManagedPath),
                   FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await workspace.ExecuteAsync(workspace.Request(force: true));
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(firstExpected, workspace.ReadBytes(firstPath));
        Assert.Equal(blockedBefore, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        Assert.Contains(result.Effects, effect => effect.Path == firstPath && effect.Outcome == Core.Commands.Update.Models.Effects.UpdatePhysicalEffectOutcome.Verified);
        Assert.Equal(UpdateRecoveryState.Retained, result.Recovery.State);
        AssertRecovery(result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }
}
