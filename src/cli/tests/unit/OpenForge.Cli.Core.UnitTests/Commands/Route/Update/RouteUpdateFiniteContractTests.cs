using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateFiniteContractTests
{
    [Fact(DisplayName = "Route Update request, target, patch, and Template mappings are exhaustive"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequestTargetPatchAndTemplateMappingsAreExhaustive()
    {
        AssertMapping<RouteUpdateMode>(["apply", "dry-run"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateTargetSelection>(["source-id", "base-path", "overwrite-path"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateTargetForm>(["ordinary-markdown", "canonical-entrypoint", "compatibility-entrypoint"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdatePatchState>(["not-requested", "unresolved", "unchanged", "changed"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateResponsibilityOperation>(["not-requested", "set", "remove"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateTemplateClassification>(["template"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateTemplateDecision>(["unresolved", "copied", "authored-body-protected"], RouteUpdateDefinitions.ReadMachineName);

        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateMode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateTargetSelection)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateTargetForm)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdatePatchState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateResponsibilityOperation)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateTemplateClassification)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateTemplateDecision)int.MaxValue));
    }

    [Fact(DisplayName = "Route Update plan, effect, recovery, and verification mappings are exhaustive"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void PlanEffectRecoveryAndVerificationMappingsAreExhaustive()
    {
        AssertMapping<RouteUpdatePlanCompleteness>(["not-established", "incomplete", "complete"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdatePlanSafety>(["not-established", "safe", "blocked"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateBodyState>(["not-established", "preserved", "template-copied", "authored-body-protected"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateEffectKind>(["routed-file", "generated-region"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateEffectAction>(["replace"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdatePreviewKind>(["metadata-field", "template-body", "generated-region"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateEffectOutcome>(["planned", "not-started", "verified", "verification-failed", "completion-unknown"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateEffectResidual>(["none", "retained", "unknown"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateRecoveryState>(["not-required", "not-created", "removed", "retained", "unknown"], RouteUpdateDefinitions.ReadMachineName);
        AssertMapping<RouteUpdateVerificationState>(["not-requested", "verified", "failed", "unknown"], RouteUpdateDefinitions.ReadMachineName);

        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdatePlanCompleteness)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdatePlanSafety)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateBodyState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateEffectKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateEffectAction)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdatePreviewKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateEffectOutcome)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateEffectResidual)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateRecoveryState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateVerificationState)int.MaxValue));
    }

    [Fact(DisplayName = "Route Update finding names and statuses match the complete independent oracle"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void FindingNamesAndStatusesMatchIndependentOracle()
    {
        Assert.Equal(ExpectedFindingNames, Enum.GetValues<RouteUpdateFindingCode>().Select(RouteUpdateDefinitions.ReadMachineName));
        Assert.Equal(ExpectedFindingStatuses, Enum.GetValues<RouteUpdateFindingCode>().Select(RouteUpdateDefinitions.ReadStatus));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadMachineName((RouteUpdateFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => RouteUpdateDefinitions.ReadStatus((RouteUpdateFindingCode)int.MaxValue));
    }

    private static void AssertMapping<T>(
        IReadOnlyList<string> expected,
        Func<T, string> read)
        where T : struct, Enum
        => Assert.Equal(expected, Enum.GetValues<T>().Select(read));

    private static readonly string[] ExpectedFindingNames =
    [
        "route-update.invalid-input",
        "route-update.invalid-target",
        "route-update.invalid-patch",
        "route-update.invalid-template",
        "route-update.workspace-unsafe",
        "route-update.target-unsafe",
        "route-update.route-ambiguous",
        "route-update.identity-collision",
        "route-update.frontmatter-unsafe",
        "route-update.metadata-preservation-unsafe",
        "route-update.template-unsafe",
        "route-update.generated-region-unsafe",
        "route-update.workspace-lock-unavailable",
        "route-update.target-changed",
        "route-update.recovery-conflict",
        "route-update.workspace-unavailable",
        "route-update.inspection-incomplete",
        "route-update.template-unavailable",
        "route-update.projection-incomplete",
        "route-update.recovery-unavailable",
        "route-update.template-body-protected",
        "route-update.recovery-artifact-retained",
        "route-update.target-changed-during-apply",
        "route-update.write-failed",
        "route-update.verification-failed",
        "route-update.recovery-failed",
        "route-update.operation-failed",
        "route-update.interrupted",
    ];

    private static readonly CliSemanticStatus[] ExpectedFindingStatuses =
    [
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Attention,
        CliSemanticStatus.Attention,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
    ];
}
