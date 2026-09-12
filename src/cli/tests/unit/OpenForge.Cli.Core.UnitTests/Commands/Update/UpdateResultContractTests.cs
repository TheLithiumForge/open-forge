using System.Text.Json;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update;

public sealed class UpdateResultContractTests
{
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
                    LifecycleSchema.RelativePath,
                    "docs/z.md",
                    "docs/a.md",
                ],
                ResidualPath = "recovery.zip",
            },
        });
        var names = typeof(UpdateJsonResult)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.Equal(
            [
                nameof(UpdateJsonResult.Mode),
                nameof(UpdateJsonResult.Force),
                nameof(UpdateJsonResult.Prune),
                nameof(UpdateJsonResult.Automatic),
                nameof(UpdateJsonResult.Source),
                nameof(UpdateJsonResult.Comparisons),
                nameof(UpdateJsonResult.GeneratedNavigation),
                nameof(UpdateJsonResult.Effects),
                nameof(UpdateJsonResult.Lifecycle),
                nameof(UpdateJsonResult.Recovery),
                nameof(UpdateJsonResult.Verification),
                nameof(UpdateJsonResult.Findings),
            ],
            names);
        Assert.Empty(result.Comparisons);
        Assert.Equal(["docs/a.md", "docs/z.md"], result.Effects.Select(effect => effect.Path));
        Assert.Equal(["docs/a.md", "docs/z.md"], result.GeneratedNavigation?.Regions.Select(region => region.Path));
        Assert.Equal(
            ["docs/a.md", "docs/z.md", LifecycleSchema.RelativePath],
            result.Recovery.ProtectedPaths);
        Assert.Empty(result.Findings);
        Assert.NotNull(result.Lifecycle);
        Assert.NotNull(result.Recovery);
        Assert.Throws<ArgumentException>(() => new UpdateResult(CompleteFormation() with
        {
            Effects = [Effect("docs/a.md"), Effect("docs/a.md")],
        }));
    }

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

    [Fact(DisplayName = "Update exposes exactly thirty findings with the three-member shape"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void ExposesExactlyThirtyFindingCodesAndThreeMemberFindingShape()
    {
        Assert.Equal(30, UpdateDefinitions.FindingCodes.Count);
        var finding = new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/index.md", "managed bytes differ");
        var names = typeof(UpdateJsonFinding)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();

        Assert.Equal(
            [nameof(UpdateJsonFinding.Code), nameof(UpdateJsonFinding.Target), nameof(UpdateJsonFinding.Cause)],
            names);
        Assert.Equal(UpdateFindingCode.ManagedDivergence, finding.Code);
        Assert.Equal("docs/index.md", finding.Target);
        var longCause = new string('x', 300);
        var exactFinding = new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/index.md", longCause);
        Assert.Equal(longCause, exactFinding.Cause);
    }

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
            [new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/index.md", "changed")],
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
        Assert.Equal("open-forge update --force", attention?.Command);
        Assert.Equal("open-forge update --verbose", failed?.Command);
        Assert.Equal("open-forge update", interrupted?.Command);
        Assert.Equal("Correct the named Update input, then rerun the request.", invalid?.Reason);
        Assert.Equal(
            "Inspect the blocked workspace, lifecycle, ownership, projection, or safety boundary before rerunning Update.",
            blocked?.Reason);
        Assert.Equal(
            "Inspect the unavailable workspace, source, lifecycle, projection, or recovery facts before relying on Update.",
            incomplete?.Reason);
        Assert.Equal(
            "Review the preserved Update divergence, then rerun with the named explicit authority.",
            attention?.Reason);
        Assert.Equal("Report the failure and retry the same Update request with bounded diagnostics.", failed?.Reason);
        Assert.Equal("Rerun the same Update request.", interrupted?.Reason);
    }

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

    [Fact(DisplayName = "Update missing-target attention adds force while preserving prune and dry-run"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MissingTargetAttentionAddsForceAndPreservesSelectedOptions()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.DryRun,
            Force = false,
            Prune = true,
            Automatic = false,
            Findings = [new UpdateFinding(UpdateFindingCode.ManagedTargetMissing, "docs/missing.md", "Managed target is absent.")],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --force --prune --dry-run", action?.Command);
        Assert.Equal(
            "Review the preserved Update divergence, then rerun with the named explicit authority.",
            action?.Reason);
    }

    [Fact(DisplayName = "Update divergence attention adds force while preserving automatic mode"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void DivergenceAttentionAddsForceAndPreservesAutomaticMode()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = true,
            Findings = [new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/changed.md", "Managed bytes differ.")],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --force --automatic", action?.Command);
        Assert.Equal(
            "Review the preserved Update divergence, then rerun with the named explicit authority.",
            action?.Reason);
    }

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

        Assert.Equal("open-forge update --prune --automatic --dry-run", action?.Command);
        Assert.Equal(
            "Review the preserved Update divergence, then rerun with the named explicit authority.",
            action?.Reason);
    }

    [Fact(DisplayName = "Update mixed missing and retired attention requires both independent authorities"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void MixedPreservationRequiresBothAuthorities()
    {
        var formation = CompleteFormation() with
        {
            Mode = UpdateMode.Apply,
            Force = false,
            Prune = false,
            Automatic = false,
            Findings =
            [
                new UpdateFinding(UpdateFindingCode.RetiredContentPreserved, "docs/retired.md", "Retired content remains."),
                new UpdateFinding(UpdateFindingCode.ManagedTargetMissing, "docs/missing.md", "Managed target is absent."),
            ],
        };

        var action = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            formation.Findings,
            formation);

        Assert.Equal("open-forge update --force --prune", action?.Command);
        Assert.Equal(
            "Review the preserved Update divergence, then rerun with the named explicit authority.",
            action?.Reason);
    }

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
                new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/changed.md", "Managed bytes differ."),
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

        Assert.Equal("open-forge update --force --prune --automatic --dry-run --verbose", action?.Command);
        Assert.Equal(
            "Report the failure and retry the same Update request with bounded diagnostics.",
            action?.Reason);
    }

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

    [Fact(DisplayName = "Update serializes schema-v1 envelope and command result in exact order"), Trait("Feature", "update"), Trait("Evidence", "UnitContract")]
    public void SerializesSchemaV1EnvelopeAndCommandResultWithExactOrder()
    {
        var document = new UpdateJsonDocument
        {
            SchemaVersion = 1,
            Command = "update",
            Status = "complete",
            Workspace = null,
            Result = JsonResult(),
            Next = null,
        };

        using var parsed = JsonDocument.Parse(
            JsonSerializer.Serialize(document, UpdateJsonContext.Default.UpdateJsonDocument));
        var root = parsed.RootElement;
        var result = root.GetProperty("result");

        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(result, "mode", "force", "prune", "automatic", "source", "comparisons", "generatedNavigation", "effects", "lifecycle", "recovery", "verification", "findings");
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Empty(result.GetProperty("comparisons").EnumerateArray());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
    }

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

    private static UpdateJsonResult JsonResult()
        => new()
        {
            Mode = "apply",
            Force = false,
            Prune = false,
            Automatic = false,
            Source = null,
            Comparisons = [],
            GeneratedNavigation = null,
            Effects = [],
            Lifecycle = new UpdateJsonLifecycle
            {
                Trust = "not-requested",
                Coverage = "not-requested",
                Action = "none",
                Outcome = "not-requested",
            },
            Recovery = new UpdateJsonRecovery
            {
                State = "not-required",
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = "not-requested",
            Findings = [],
        };

    private static void AssertPropertyOrder(JsonElement element, params string[] expected)
        => Assert.Equal(expected, element.EnumerateObject().Select(property => property.Name));
}
