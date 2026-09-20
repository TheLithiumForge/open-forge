using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RepairBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<RepairResult> Renderers = CommandOutputRenderers<RepairResult>.From(RepairPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves a real Library recovery step and its original evidence bundle")]
    public async Task LibraryRecoveryStep()
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create");
        var sourcesBefore = workspace.Sources();
        var original = workspace.Preparation.BundlePath;
        var bundleBefore = File.ReadAllBytes(original);
        var result = await new RepairOperation().ExecuteAsync(
            new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.NotEmpty(result.PostDiagnosis.Findings);
        var execution = Assert.IsType<Core.Commands.Repair.Models.Application.RepairLibraryExecution>(result.LibraryExecution);
        var receipt = Assert.Single(execution.LibraryReceipts);
        Assert.Equal(original, receipt.OriginalResidual.BundlePath);
        Assert.Null(new FileInfo(workspace.TargetPath).LinkTarget);
        Assert.False(File.Exists(workspace.TargetPath));
        Assert.Equal(sourcesBefore, workspace.Sources());
        Assert.Equal(bundleBefore, File.ReadAllBytes(original));
        var paths = new List<string> { original };
        if (execution.ForwardPreparation is { } forward)
        {
            Assert.Equal(Path.GetDirectoryName(original), Path.GetDirectoryName(forward.BundlePath));
            Assert.False(File.Exists(forward.BundlePath));
            paths.Add(forward.BundlePath);
        }
        CommandOutputRenderers<RepairResult>.From(
            RepairPresentation.Rendering,
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths),
            output => CommandOutputNormalization.NormalizeRecoveryPaths(output, paths))
            .MatchDetails(result, "library-recovery-step");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves an earlier repaired file when a later replacement is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = RepairIntegrationWorkspace.Create("repair-output-partial", includeGuided: false);
        workspace.AddEarlierSafeSource();
        var sourceBefore = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
        RepairResult result;
        using (var held = File.Open(workspace.Combine(RepairIntegrationWorkspace.SourcePath), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await new RepairOperation().ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Contains("[Safe](guide.md)", workspace.ReadText(".agents/docs/aaa.md"), StringComparison.Ordinal);
        Assert.Equal(sourceBefore, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
        Assert.Equal(RepairResidualState.Retained, result.Recovery.Residual);
        Assert.True(File.Exists(result.Recovery.ResidualPath));
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves a held workspace lock without changing source bytes")]
    public async Task LockHeld()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-output-lock");
        var lockPath = Assert.IsType<string>(workspace.LockPath);
        Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
        var sourceBefore = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
        var unrelatedBefore = workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath);
        RepairResult result;
        using (var held = new FileStream(lockPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
        {
            result = await new RepairOperation().ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.WorkspaceLockUnavailable);
        Assert.Equal(sourceBefore, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
        Assert.Equal(unrelatedBefore, workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath));
        Renderers.MatchDetails(result, "lock-held");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves automatic selection, two-link changes and previews")]
    public async Task AutomaticSelection()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("nothing-to-repair", CliSemanticStatus.Complete),
            ("automatic-two-links", CliSemanticStatus.Complete),
            ("automatic-nothing-safe-two-guided", CliSemanticStatus.Attention),
            ("dry-run-automatic", CliSemanticStatus.Complete),
        })
        {
            var safe = situation is "automatic-two-links" or "dry-run-automatic";
            using var workspace = RepairIntegrationWorkspace.Create("repair-output-automatic", safe, situation == "automatic-nothing-safe-two-guided");
            if (safe) workspace.AddSecondSafeLink();
            if (situation == "automatic-nothing-safe-two-guided") workspace.AddSecondGuidedLink();
            var before = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
            var unrelated = workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath);
            var result = await new RepairOperation().ExecuteAsync(workspace.Request(
                situation == "dry-run-automatic" ? RepairMode.DryRun : RepairMode.Apply, automatic: true), TestContext.Current.CancellationToken);
            Assert.Equal(status, result.Status);
            if (situation == "automatic-two-links")
            {
                Assert.Equal(2, result.Selection!.Selected.Count);
                Assert.DoesNotContain("./guide.md", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
            }
            else
            {
                Assert.Equal(before, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
            }

            Assert.Equal(unrelated, workspace.ReadBytes(RepairIntegrationWorkspace.UnrelatedPath));
            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves explicit relinks and refusal of stale or contradictory requests")]
    public async Task ExplicitRelink()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, status) in new[]
        {
            ("relink-one", CliSemanticStatus.Complete),
            ("relink-invalid", CliSemanticStatus.Blocked),
        })
        {
            using var workspace = RepairIntegrationWorkspace.Create("repair-output-relink", false, true);
            var relink = workspace.GuidedRelink(expectedDestination: situation == "relink-invalid" ? "changed.md" : "missing.md");
            var before = workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath);
            var result = await new RepairOperation().ExecuteAsync(workspace.Request(relinks: [relink]), TestContext.Current.CancellationToken);
            Assert.Equal(status, result.Status);
            if (situation == "relink-one") Assert.Contains("[Guided](replacement.md)", workspace.ReadText(RepairIntegrationWorkspace.SourcePath), StringComparison.Ordinal);
            else Assert.Equal(before, workspace.ReadBytes(RepairIntegrationWorkspace.SourcePath));
            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves missing selection authority and pre-effect cancellation")]
    public async Task SelectionBoundary()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, cancelled) in new[]
        {
            ("non-interactive-no-selection", false),
            ("cancelled", true),
        })
        {
            using var workspace = RepairIntegrationWorkspace.Create("repair-output-boundary");
            var before = workspace.SnapshotState();
            using var cancellation = new CancellationTokenSource();
            if (cancelled) cancellation.Cancel();
            var result = await new RepairOperation().ExecuteAsync(workspace.Request(automatic: cancelled), cancellation.Token);
            Assert.Equal(cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(before, workspace.SnapshotState());
            Renderers.MatchDetails(result, situation, snapshotCollector: snapshots);
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output preserves invalid operands without effects")]
    public async Task InvalidInput()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-output-invalid");
        var before = workspace.SnapshotState();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["repair", "unexpected-operand"],
            ExitCode = 4,
            ShellDiagnostic = true,
        });
        Assert.Equal(before, workspace.SnapshotState());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Repair output rejects contradictory relinks at the real binding boundary")]
    public async Task ContradictoryRelinks()
    {
        using var workspace = RepairIntegrationWorkspace.Create("repair-output-contradictory", false, true);
        var relink = workspace.GuidedRelink();
        var location = relink.SourceLocation.ToString();
        var before = workspace.SnapshotState();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "contradictory-relinks",
            Arguments = ["repair", "--relink", location, "missing.md", RepairIntegrationWorkspace.GuidedTargetPath,
                "--relink", location, "missing.md", RepairIntegrationWorkspace.SafeTargetPath],
            ExitCode = 4,
        });
        Assert.Equal(before, workspace.SnapshotState());
    }
}
