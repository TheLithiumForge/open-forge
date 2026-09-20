using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateResultContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update result requires ordered properties and initialized arrays"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RequiresOrderedCompleteResultPropertiesAndNonNullArrays()
    {
        var result = new UpdateResult(CompleteFormation() with
        {
            Effects = [Effect("docs/z.md"), Effect("docs/a.md")],
            GeneratedNavigation = new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions =
                [
                    new UpdateGeneratedNavigationRegion
                    {
                        Path = "docs/z.md",
                        State = UpdateGeneratedNavigationRegionState.Changed,
                    },
                    new UpdateGeneratedNavigationRegion
                    {
                        Path = "docs/a.md",
                        State = UpdateGeneratedNavigationRegionState.New,
                    },
                ],
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.Retained,
                ProtectedPaths =
                [
                    WorkspaceOwnershipDefinitions.RelativePath,
                    "docs/z.md",
                    "docs/a.md",
                ],
                ResidualPath = "recovery.zip",
            },
        });
        Assert.Empty(result.Comparisons);
        Assert.Equal(["docs/a.md", "docs/z.md"], result.Effects.Select(effect => effect.Path));
        Assert.Equal(["docs/a.md", "docs/z.md"], result.GeneratedNavigation?.Regions.Select(region => region.Path));
        Assert.Equal(
            ["docs/a.md", "docs/z.md", WorkspaceOwnershipDefinitions.RelativePath],
            result.Recovery.ProtectedPaths);
        Assert.Empty(result.Findings);
        Assert.NotNull(result.Lifecycle);
        Assert.NotNull(result.Recovery);
        Assert.Throws<ArgumentException>(() => new UpdateResult(CompleteFormation() with
        {
            Effects = [Effect("docs/a.md"), Effect("docs/a.md")],
        }));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update plan review projects Apply results to DryRun without changing authority or planned facts"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PlanReviewProjectionPreservesPlannedFactsAndAuthority()
    {
        var applied = new UpdateResult(CompleteFormation() with
        {
            Mode = UpdateMode.Apply,
            Force = true,
            Prune = true,
            Automatic = false,
        });

        var review = applied.ForPlanReview();

        Assert.Equal(UpdateMode.DryRun, review.Mode);
        Assert.Equal(applied.Force, review.Force);
        Assert.Equal(applied.Prune, review.Prune);
        Assert.Equal(applied.Automatic, review.Automatic);
        Assert.Equal(applied.Source, review.Source);
        Assert.Equal(applied.Comparisons, review.Comparisons);
        Assert.Equal(applied.GeneratedNavigation, review.GeneratedNavigation);
        Assert.Equal(applied.Effects, review.Effects);
        Assert.Equal(applied.Lifecycle, review.Lifecycle);
        Assert.Equal(applied.Recovery.State, review.Recovery.State);
        Assert.Equal(
            applied.Recovery.ProtectedPaths,
            review.Recovery.ProtectedPaths,
            StringComparer.Ordinal);
        Assert.Equal(applied.Recovery.ResidualPath, review.Recovery.ResidualPath);
        Assert.Equal(applied.Verification, review.Verification);
        Assert.Equal(applied.Findings, review.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update coalesces authored and generated logical changes into one physical effect"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void CoalescesAuthoredAndGeneratedLogicalChangesIntoOnePhysicalEffect()
    {
        var managed = new UpdateLogicalChange(
            UpdateComparisonTargetKind.ManagedRegion,
            UpdateLogicalChangeAction.Replace,
            "entries",
            "assets/index.md");
        var generated = new UpdateLogicalChange(
            UpdateComparisonTargetKind.GeneratedRegion,
            UpdateLogicalChangeAction.Replace,
            "entries",
            null);
        var effect = new UpdatePhysicalEffect(
            "docs/index.md",
            UpdatePhysicalEffectAction.Replace,
            [
                generated,
                managed,
            ],
            UpdatePhysicalEffectOutcome.Planned,
            UpdatePhysicalEffectResidual.None);

        Assert.Equal("docs/index.md", effect.Path);
        Assert.Equal(2, effect.Changes.Count);
        Assert.Equal(UpdateComparisonTargetKind.ManagedRegion, effect.Changes[0].Kind);
        Assert.Equal(UpdateComparisonTargetKind.GeneratedRegion, effect.Changes[1].Kind);
        Assert.Null(effect.Changes[1].SourceAssetPath);
        Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            UpdateComparisonTargetKind.File,
            UpdateLogicalChangeAction.Replace,
            "entries",
            "assets/index.md"));
        Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            UpdateComparisonTargetKind.ManagedRegion,
            UpdateLogicalChangeAction.Replace,
            "entries",
            null));
        Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            UpdateComparisonTargetKind.GeneratedRegion,
            UpdateLogicalChangeAction.Replace,
            "entries",
            "assets/index.md"));
        Assert.Throws<ArgumentException>(() => new UpdatePhysicalEffect(
            "docs/index.md",
            UpdatePhysicalEffectAction.Replace,
            [managed, managed],
            UpdatePhysicalEffectOutcome.Planned,
            UpdatePhysicalEffectResidual.None));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update exposes the finite finding codes"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ExposesFiniteFindingCodes()
    {
        Assert.Equal(29, UpdateDefinitions.FindingCodes.Count);
        var finding = new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/index.md", "managed bytes differ");
        Assert.Equal("docs/index.md", finding.Target);
        var longCause = new string('x', 300);
        var exactFinding = new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/index.md", longCause);
        Assert.Equal(longCause, exactFinding.Cause);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update maps every status to its exact next command and reason"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MapsEveryStatusToTheExactNextCommandAndReason()
    {
        var complete = UpdateDefinitions.ReadNextAction(CliSemanticStatus.Complete, [], CompleteFormation());
        var invalid = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Invalid,
            [new UpdateFinding(UpdateFindingCode.InvalidInput, null, "bad input")],
            CompleteFormation());
        var blocked = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Blocked,
            [new UpdateFinding(UpdateFindingCode.PlanBlocked, null, "blocked")],
            CompleteFormation());
        var incomplete = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Incomplete,
            [new UpdateFinding(UpdateFindingCode.PayloadUnavailable, null, "missing")],
            CompleteFormation());
        var attention = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            [new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/index.md", "changed")],
            CompleteFormation());
        var failed = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Failed,
            [new UpdateFinding(UpdateFindingCode.OperationFailed, null, "failed")],
            CompleteFormation());
        var interrupted = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Interrupted,
            [new UpdateFinding(UpdateFindingCode.Interrupted, null, "stopped")],
            CompleteFormation());

        Assert.Null(complete);
        Assert.Equal("open-forge update --help", invalid?.Command);
        Assert.Equal("open-forge doctor", blocked?.Command);
        Assert.Equal("open-forge doctor", incomplete?.Command);
        Assert.Equal("open-forge update --prune --dry-run", attention?.Command);
        Assert.Equal("open-forge update --detail debug", failed?.Command);
        Assert.Equal("open-forge update", interrupted?.Command);
        Assert.Equal("Correct the named Update input, then rerun the request.", invalid?.Reason);
        Assert.Equal(
            "Inspect the blocked workspace, lifecycle, ownership, projection, or safety boundary before rerunning Update.",
            blocked?.Reason);
        Assert.Equal(
            "Inspect the unavailable workspace, source, lifecycle, projection, or recovery facts before relying on Update.",
            incomplete?.Reason);
        Assert.Equal(
            "Preview deleting the retained files before applying the prune.",
            attention?.Reason);
        Assert.Equal("Report the failure and retry the same Update request with bounded diagnostics.", failed?.Reason);
        Assert.Equal("Rerun the same Update request.", interrupted?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update confirmation requests automatic apply while preserving force and prune"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ConfirmationRequestsAutomaticApplyAndPreservesAuthority()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = true,
            Prune = true,
            Automatic = false,
            Findings =
            [
                new UpdateFinding(UpdateFindingCode.InvalidInput, null, "An input needs attention."),
                new UpdateFinding(UpdateFindingCode.ConfirmationRequired, null, "Explicit confirmation is required."),
            ],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Invalid,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --force --prune --automatic", action?.Command);
        Assert.Equal("Rerun the same Update request with explicit automatic mode.", action?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update retired attention adds prune without granting force"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RetiredAttentionAddsPruneWithoutForce()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = false,
            Prune = false,
            Automatic = true,
            Findings = [new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/retired.md", "Retired content remains.")],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --prune --dry-run", action?.Command);
        Assert.Equal(
            "Preview deleting the retained files before applying the prune.",
            action?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update retained-artifact attention prefers cleanup over mixed preservation findings"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void RetainedArtifactAttentionPrefersCleanupOverPreservation()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = true,
            Prune = true,
            Automatic = true,
            Findings =
            [
                new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/changed.md", "Managed bytes differ."),
                new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/retired.md", "Retired content remains."),
                new UpdateFinding(UpdateFindingCode.RecoveryArtifactRetained, "recovery.zip", "The verified artifact remains."),
            ],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            formation.Findings,
            formation);

        Assert.Equal("open-forge cleanup", action?.Command);
        Assert.Equal(
            "Review and remove the reported recovery artifact after confirming the verified Update result.",
            action?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Failed Update preserves every request flag before verbose despite a retained artifact"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void FailedRequestPreservesOptionsBeforeVerboseDespiteRetainedArtifact()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = true,
            Prune = true,
            Automatic = true,
            Findings =
            [
                new UpdateFinding(UpdateFindingCode.RecoveryArtifactRetained, "recovery.zip", "The recovery artifact remains."),
                new UpdateFinding(UpdateFindingCode.OperationFailed, null, "The operation failed."),
            ],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Failed,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --force --prune --automatic --dry-run --detail debug", action?.Command);
        Assert.Equal(
            "Report the failure and retry the same Update request with bounded diagnostics.",
            action?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Interrupted Update preserves the selected prune automatic and dry-run options"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void InterruptedRequestPreservesSelectedOptions()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = false,
            Prune = true,
            Automatic = true,
            Findings = [new UpdateFinding(UpdateFindingCode.Interrupted, null, "The request stopped.")],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Interrupted,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --prune --automatic --dry-run", action?.Command);
        Assert.Equal("Rerun the same Update request.", action?.Reason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update result coordinates preserve admitted syntax without normalization"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesLogicalPhysicalRecoveryAndNavigationCoordinateSyntax()
    {
        var change = new UpdateLogicalChange(
            kind: UpdateComparisonTargetKind.ManagedRegion,
            action: UpdateLogicalChangeAction.Replace,
            region: "docs/name. ",
            sourceAssetPath: "docs/cafe\u0301.md");
        var effect = new UpdatePhysicalEffect(
            path: "docs/CON.md",
            action: UpdatePhysicalEffectAction.Replace,
            changes: [change],
            outcome: UpdatePhysicalEffectOutcome.Planned,
            residual: UpdatePhysicalEffectResidual.None);
        var result = new UpdateResult(CompleteFormation() with
        {
            Effects = [effect],
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.Retained,
                ProtectedPaths = ["docs/a?.md", "docs/a:b.md", "docs/CON.md"],
                ResidualPath = "recovery.zip",
            },
            GeneratedNavigation = new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions =
                [
                    new UpdateGeneratedNavigationRegion
                    {
                        Path = "é:x",
                        State = UpdateGeneratedNavigationRegionState.Changed,
                    },
                ],
            },
        });

        Assert.Equal("docs/name. ", change.Region);
        Assert.Equal("docs/cafe\u0301.md", change.SourceAssetPath);
        Assert.Equal("docs/CON.md", Assert.Single(result.Effects).Path);
        Assert.Equal(["docs/CON.md", "docs/a:b.md", "docs/a?.md"], result.Recovery.ProtectedPaths);
        var navigation = Assert.IsType<UpdateGeneratedNavigation>(result.GeneratedNavigation);
        Assert.Equal("é:x", Assert.Single(navigation.Regions).Path);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update source validates identity then lowercase inventory hash then count"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void PreservesSourceIdentityFingerprintAndCountValidationOrder()
    {
        var source = new UpdateSource
        {
            Id = "framework",
            Version = null,
            InventoryFingerprint = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            AssetCount = 0,
        };

        source.Validate();
        var identity = Assert.Throws<ArgumentException>(() => (source with
        {
            Id = "other",
            InventoryFingerprint = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            AssetCount = -1,
        }).Validate());
        var fingerprint = Assert.Throws<ArgumentException>(() => (source with
        {
            InventoryFingerprint = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
            AssetCount = -1,
        }).Validate());
        var count = Assert.Throws<ArgumentOutOfRangeException>(() => (source with
        {
            AssetCount = -1,
        }).Validate());

        Assert.Equal("framework", source.Id);
        Assert.Null(source.Version);
        Assert.Equal("0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef", source.InventoryFingerprint);
        Assert.Equal(0, source.AssetCount);
        Assert.Equal("Id", identity.ParamName);
        Assert.Equal("Update source identity must be framework. (Parameter 'Id')", identity.Message);
        Assert.Equal("InventoryFingerprint", fingerprint.ParamName);
        Assert.Equal(
            "Update source inventory identity must be lowercase SHA-256. (Parameter 'InventoryFingerprint')",
            fingerprint.Message);
        Assert.Equal("AssetCount", count.ParamName);
        Assert.Equal(-1, count.ActualValue);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update logical changes preserve caller whitespace and coordinate precedence"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void KeepsLogicalCoordinateWhitespaceAdmissionAndPrecedence()
    {
        var region = Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            kind: UpdateComparisonTargetKind.ManagedRegion,
            action: UpdateLogicalChangeAction.Replace,
            region: " ",
            sourceAssetPath: " "));
        var managed = Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            kind: UpdateComparisonTargetKind.ManagedRegion,
            action: UpdateLogicalChangeAction.Replace,
            region: "entries",
            sourceAssetPath: " "));
        var file = Assert.Throws<ArgumentException>(() => new UpdateLogicalChange(
            kind: UpdateComparisonTargetKind.File,
            action: UpdateLogicalChangeAction.Replace,
            region: null,
            sourceAssetPath: " "));

        Assert.Equal("region", region.ParamName);
        Assert.Equal(
            "A managed or generated logical change requires one canonical region identity. (Parameter 'region')",
            region.Message);
        Assert.Equal("sourceAssetPath", managed.ParamName);
        Assert.Equal(
            "A managed logical change requires canonical source provenance. (Parameter 'sourceAssetPath')",
            managed.Message);
        Assert.Equal("sourceAssetPath", file.ParamName);
        Assert.Equal(
            "An authored file logical change requires canonical source provenance. (Parameter 'sourceAssetPath')",
            file.Message);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update physical effects reject whitespace paths before empty changes"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void KeepsPhysicalPathAdmissionBeforeLogicalChangeValidation()
    {
        var exception = Assert.Throws<ArgumentException>(() => new UpdatePhysicalEffect(
            path: " ",
            action: UpdatePhysicalEffectAction.Replace,
            changes: [],
            outcome: UpdatePhysicalEffectOutcome.Planned,
            residual: UpdatePhysicalEffectResidual.None));

        Assert.Equal("path", exception.ParamName);
        Assert.Equal(
            "Physical effect paths must be canonical workspace-relative paths. (Parameter 'path')",
            exception.Message);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Update result validates recovery whitespace before generated navigation"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void KeepsRecoveryAndNavigationWhitespaceAdmissionOrder()
    {
        var formation = CompleteFormation() with
        {
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [" "],
                ResidualPath = null,
            },
            GeneratedNavigation = new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions =
                [
                    new UpdateGeneratedNavigationRegion
                    {
                        Path = " ",
                        State = UpdateGeneratedNavigationRegionState.Unchanged,
                    },
                ],
            },
        };
        var recovery = Assert.Throws<ArgumentException>(() => new UpdateResult(formation));
        var navigation = Assert.Throws<ArgumentException>(() => new UpdateResult(formation with
        {
            Recovery = CompleteFormation().Recovery,
        }));

        Assert.Equal("ProtectedPaths", recovery.ParamName);
        Assert.Equal(
            "Update recovery protected paths must be canonical workspace-relative paths. (Parameter 'ProtectedPaths')",
            recovery.Message);
        Assert.Null(navigation.ParamName);
        Assert.Equal("Generated navigation paths must be canonical workspace-relative paths.", navigation.Message);
    }

    private static UpdateResultFormation CompleteFormation()
        => new()
        {
            Workspace = null,
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = false,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = UpdateVerificationState.NotRequested,
            Findings = [],
        };

    private static UpdatePhysicalEffect Effect(string path)
        => new(
            path,
            UpdatePhysicalEffectAction.Replace,
            [new UpdateLogicalChange(
                UpdateComparisonTargetKind.ManagedRegion,
                UpdateLogicalChangeAction.Replace,
                "entries",
                $"assets/{path[6..]}")],
            UpdatePhysicalEffectOutcome.Planned,
            UpdatePhysicalEffectResidual.None);

}
