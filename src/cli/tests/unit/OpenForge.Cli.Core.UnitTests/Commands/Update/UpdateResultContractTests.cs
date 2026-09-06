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
        var complete = UpdateDefinitions.ReadNextAction(CliSemanticStatus.Complete, [], false, false, false, UpdateMode.Apply);
        var invalid = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Invalid,
            [new UpdateFinding(UpdateFindingCode.InvalidInput, null, "bad input")],
            false,
            false,
            false,
            UpdateMode.Apply);
        var blocked = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Blocked,
            [new UpdateFinding(UpdateFindingCode.PlanBlocked, null, "blocked")],
            false,
            false,
            false,
            UpdateMode.Apply);
        var incomplete = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Incomplete,
            [new UpdateFinding(UpdateFindingCode.PayloadUnavailable, null, "missing")],
            false,
            false,
            false,
            UpdateMode.Apply);
        var attention = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Attention,
            [new UpdateFinding(UpdateFindingCode.ManagedDivergence, "docs/index.md", "changed")],
            false,
            false,
            false,
            UpdateMode.Apply);
        var failed = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Failed,
            [new UpdateFinding(UpdateFindingCode.OperationFailed, null, "failed")],
            false,
            false,
            false,
            UpdateMode.Apply);
        var interrupted = UpdateDefinitions.ReadNextAction(
            CliSemanticStatus.Interrupted,
            [new UpdateFinding(UpdateFindingCode.Interrupted, null, "stopped")],
            false,
            false,
            false,
            UpdateMode.Apply);

        Assert.Null(complete);
        Assert.Equal("open-forge update --help", invalid?.Command);
        Assert.Equal("open-forge doctor", blocked?.Command);
        Assert.Equal("open-forge doctor", incomplete?.Command);
        Assert.Equal("open-forge update --force", attention?.Command);
        Assert.Equal("open-forge update --verbose", failed?.Command);
        Assert.Equal("open-forge update", interrupted?.Command);
        Assert.All(
            new[] { invalid, blocked, incomplete, attention, failed, interrupted },
            action => Assert.False(string.IsNullOrWhiteSpace(action?.Reason)));
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
