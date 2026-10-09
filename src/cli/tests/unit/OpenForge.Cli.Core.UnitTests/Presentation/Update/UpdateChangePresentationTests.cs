using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Update;

public sealed class UpdateChangePresentationTests
{
    [Theory(DisplayName = "Update distinguishes missing whole-file restoration and creation from Entries refreshes")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "update-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MissingFileLabels(bool preview)
    {
        var outcome = preview ? UpdatePhysicalEffectOutcome.Planned : UpdatePhysicalEffectOutcome.Verified;
        var result = Result(preview,
        [
            new(".agents/loader.md", UpdatePhysicalEffectAction.Create,
            [
                new(UpdateComparisonTargetKind.File, UpdateLogicalChangeAction.Restore, region: null, sourceAssetPath: "loader.md"),
                new(UpdateComparisonTargetKind.GeneratedRegion, UpdateLogicalChangeAction.Replace, region: "entries", sourceAssetPath: null),
            ], outcome, UpdatePhysicalEffectResidual.None),
            new(".agents/memory/_memory.md", UpdatePhysicalEffectAction.Create,
                [new(UpdateComparisonTargetKind.GeneratedRegion, UpdateLogicalChangeAction.Create, region: "entries", sourceAssetPath: null)],
                outcome, UpdatePhysicalEffectResidual.None),
            new("AGENTS.md", UpdatePhysicalEffectAction.Create,
                [new(UpdateComparisonTargetKind.ManagedRegion, UpdateLogicalChangeAction.Restore, region: "open-forge", sourceAssetPath: "AGENTS.md")],
                outcome, UpdatePhysicalEffectResidual.None),
        ]);
        var report = UpdatePresentation.Rendering.Selector(result, new CliSelection(CliDetail.Standard));
        Assert.Equal(0, report.Data.ReplacedFiles);
        Assert.Equal(preview ? "would be restored" : "restored", report.Data.TextRows[0].Wording);
        Assert.Equal(preview ? "would be created" : "created", report.Data.TextRows[1].Wording);
        Snapshot(result, preview ? "preview" : "applied");
    }

    [Theory(DisplayName = "Update describes Loader Entries and host sections once in preview and verified results")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "update-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void SectionRefreshLabels(bool preview)
    {
        var outcome = preview ? UpdatePhysicalEffectOutcome.Planned : UpdatePhysicalEffectOutcome.Verified;
        var effects = new[]
        {
            Effect(".agents/loader.md", UpdateComparisonTargetKind.GeneratedRegion, UpdateLogicalChangeAction.Replace, outcome),
            Effect("AGENTS.md", UpdateComparisonTargetKind.ManagedRegion, UpdateLogicalChangeAction.Replace, outcome),
            Effect("CLAUDE.md", UpdateComparisonTargetKind.ManagedRegion, UpdateLogicalChangeAction.Restore, outcome),
        };
        var result = Result(preview, effects);
        var report = UpdatePresentation.Rendering.Selector(result, new CliSelection(CliDetail.Standard));
        Assert.Equal(0, report.Data.ReplacedFiles);
        Assert.Equal(effects.Length + 1, report.Data.TextRows.Count);
        Assert.Single(report.Data.TextRows, row => row.Path == ".agents/loader.md");
        Snapshot(result, preview ? "preview" : "applied");
    }

    [Theory(DisplayName = "Update counts a mixed whole-file and Entries write as one replacement")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "update-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MixedWholeFileReplacement(bool preview)
    {
        var effect = new UpdatePhysicalEffect(
            path: ".agents/loader.md",
            action: UpdatePhysicalEffectAction.Replace,
            changes:
            [
                new(UpdateComparisonTargetKind.File, UpdateLogicalChangeAction.Replace, region: null, sourceAssetPath: "loader.md"),
                new(UpdateComparisonTargetKind.GeneratedRegion, UpdateLogicalChangeAction.Replace, region: "entries", sourceAssetPath: null),
            ],
            outcome: preview ? UpdatePhysicalEffectOutcome.Planned : UpdatePhysicalEffectOutcome.Verified,
            residual: UpdatePhysicalEffectResidual.None);
        var result = Result(preview, [effect]);
        var report = UpdatePresentation.Rendering.Selector(result, new CliSelection(CliDetail.Standard));
        Assert.Equal(1, report.Data.ReplacedFiles);
        Assert.Single(report.Data.TextRows, row => row.Path == effect.Path);
        Snapshot(result, preview ? "preview" : "applied");
    }

    [Theory(DisplayName = "Update partial failure and cancellation count only verified mutations")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "update-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void PartialResultsCountVerifiedMutations(bool cancelled)
    {
        var effects = new List<UpdatePhysicalEffect>();
        foreach (var outcome in new[]
        {
            UpdatePhysicalEffectOutcome.Verified,
            UpdatePhysicalEffectOutcome.NotStarted,
            UpdatePhysicalEffectOutcome.VerificationFailed,
            UpdatePhysicalEffectOutcome.CompletionUnknown,
        })
        {
            foreach (var action in new[] { UpdateLogicalChangeAction.Replace, UpdateLogicalChangeAction.Restore, UpdateLogicalChangeAction.Create, UpdateLogicalChangeAction.Delete })
            {
                var physicalAction = action switch
                {
                    UpdateLogicalChangeAction.Replace => UpdatePhysicalEffectAction.Replace,
                    UpdateLogicalChangeAction.Restore or UpdateLogicalChangeAction.Create => UpdatePhysicalEffectAction.Create,
                    UpdateLogicalChangeAction.Delete => UpdatePhysicalEffectAction.Delete,
                    _ => throw new ArgumentOutOfRangeException(nameof(action)),
                };
                effects.Add(new($".agents/{outcome}-{action}.md", physicalAction,
                    [new(UpdateComparisonTargetKind.File, action, region: null, sourceAssetPath: "fixture.md")],
                    outcome, UpdatePhysicalEffectResidual.None));
            }
        }
        effects.Add(Effect(".agents/loader.md", UpdateComparisonTargetKind.GeneratedRegion,
            UpdateLogicalChangeAction.Replace, UpdatePhysicalEffectOutcome.NotStarted));
        var result = Result(false, effects, cancelled ? UpdateFindingCode.Interrupted : UpdateFindingCode.WriteFailed);
        Assert.Equal(cancelled ? CliSemanticStatus.Interrupted : CliSemanticStatus.Failed, result.Status);
        var report = UpdatePresentation.Rendering.Selector(result, new(CliDetail.Full));
        Assert.Equal(1, report.Data.ReplacedFiles);
        Assert.Equal(1, report.Data.RestoredFiles);
        Assert.Equal(2, report.Data.CreatedFiles);
        Assert.Equal(1, report.Data.DeletedFiles);
        Assert.Equal(0, report.Data.UpdatedSections);
        Assert.Equal(4, report.Data.ChangedFiles);
        Assert.Equal(1, Assert.Single(report.Counts, count => count.Name == "filesReplaced").Value);
    }

    [Theory(DisplayName = "Update replacement rows follow the observed recovery disposition")]
    [InlineData((int)UpdateRecoveryState.Retained, "replaced (your previous file is in the recovery bundle)")]
    [InlineData((int)UpdateRecoveryState.Removed, "replaced")]
    [InlineData((int)UpdateRecoveryState.Unknown, "replaced")]
    [Trait("Feature", "update-presentation"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ReplacementRecoveryLabels(int recoveryValue, string expected)
    {
        var effect = Effect(".agents/file.md", UpdateComparisonTargetKind.File,
            UpdateLogicalChangeAction.Replace, UpdatePhysicalEffectOutcome.Verified);
        var report = UpdatePresentation.Rendering.Selector(Result(false, [effect], recoveryState: (UpdateRecoveryState)recoveryValue), new(CliDetail.Minimal));
        Assert.Equal(expected, Assert.Single(report.Data.TextRows).Wording);
    }

    private static void Snapshot(UpdateResult result, string name, [global::System.Runtime.CompilerServices.CallerMemberName] string testName = "")
        => CommandOutputSnapshot.MatchSnapshot(
            CliRenderingStage.Render(new CliPresentationRequest<UpdateResult>(
                result, new(CliFormat.Text, CliDetail.Standard, null)), UpdatePresentation.Rendering).PrimaryContent,
            name, testName: testName);

    private static UpdatePhysicalEffect Effect(string path, UpdateComparisonTargetKind kind, UpdateLogicalChangeAction action, UpdatePhysicalEffectOutcome outcome)
        => new(path, UpdatePhysicalEffectAction.Replace,
            [new(kind, action, region: kind == UpdateComparisonTargetKind.File ? null : "entries",
                sourceAssetPath: kind == UpdateComparisonTargetKind.GeneratedRegion ? null : path)],
            outcome, UpdatePhysicalEffectResidual.None);

    private static UpdateResult Result(bool preview, IReadOnlyList<UpdatePhysicalEffect> effects,
        UpdateFindingCode? failure = null, UpdateRecoveryState recoveryState = UpdateRecoveryState.NotRequired)
        => new(new UpdateResultFormation
        {
            Workspace = null,
            Mode = preview ? UpdateMode.DryRun : UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = true,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions = [new() { Path = ".agents/loader.md", State = UpdateGeneratedNavigationRegionState.Changed }],
            },
            Effects = effects,
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.Trusted,
                Coverage = UpdateLifecycleCoverage.Complete,
                Action = UpdateLifecycleAction.Publish,
                Outcome = preview ? UpdateLifecycleOutcome.Planned : UpdateLifecycleOutcome.Verified,
            },
            Recovery = new UpdateRecovery
            {
                State = recoveryState,
                ProtectedPaths = [],
                ResidualPath = recoveryState == UpdateRecoveryState.Retained ? Path.Combine(Path.GetTempPath(), "update-recovery-fixture.zip") : null,
            },
            Verification = preview ? UpdateVerificationState.NotRequested : UpdateVerificationState.Verified,
            Findings = failure is { } code ? [new(code, target: null, cause: "The operation stopped.")] : [],
        });
}
