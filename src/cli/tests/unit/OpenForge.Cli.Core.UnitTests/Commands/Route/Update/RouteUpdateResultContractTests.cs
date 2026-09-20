using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdateResultContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update request preserves the accepted typed facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequestPreservesAcceptedTypedFacts()
    {
        var workspace = RouteUpdateTestData.Workspace();
        var patch = RouteUpdateTestData.TagsPatch("Memory", "Decision");

        var request = new RouteUpdateRequest(
            workspace,
            RouteUpdateTestData.TargetId,
            patch,
            RouteUpdateTestData.TemplateId,
            RouteUpdateMode.DryRun);

        Assert.Same(workspace, request.Workspace);
        Assert.Equal(RouteUpdateTestData.TargetId, request.SourceReference);
        Assert.Same(patch, request.Patch);
        Assert.Equal(RouteUpdateTestData.TemplateId, request.TemplateReference);
        Assert.Equal(RouteUpdateMode.DryRun, request.Mode);
        Assert.True(request.IsDryRun);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update freezes the selected exact source without mutating the requested operand"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequestFreezesSelectedExactSource()
    {
        var patch = RouteUpdateTestData.DescriptionPatch("After");
        var request = new RouteUpdateRequest(
            RouteUpdateTestData.Workspace(),
            RouteUpdateTestData.TargetId,
            patch,
            null,
            RouteUpdateMode.Apply,
            allowInteractiveSourceSelection: true);

        var frozen = request.FreezeSource(RouteUpdateTestData.TargetPath);

        Assert.Equal(RouteUpdateTestData.TargetId, request.SourceReference);
        Assert.Null(request.FrozenSourceReference);
        Assert.Equal(RouteUpdateTestData.TargetId, request.ResolutionReference);
        Assert.Equal(RouteUpdateTestData.TargetId, frozen.SourceReference);
        Assert.Equal(RouteUpdateTestData.TargetPath, frozen.FrozenSourceReference);
        Assert.Equal(RouteUpdateTestData.TargetPath, frozen.ResolutionReference);
        Assert.True(frozen.AllowInteractiveSourceSelection);
        Assert.Same(patch, frozen.Patch);
        Assert.Equal(request.Mode, frozen.Mode);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update request rejects missing and undefined required facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequestRejectsMissingAndUndefinedRequiredFacts()
    {
        var workspace = RouteUpdateTestData.Workspace();
        var patch = RouteUpdateTestData.DescriptionPatch("After");

        Assert.Throws<ArgumentException>(() => new RouteUpdateRequest(
            workspace,
            " ",
            patch,
            null,
            RouteUpdateMode.Apply));
        Assert.Throws<ArgumentNullException>(() => new RouteUpdateRequest(
            workspace,
            RouteUpdateTestData.TargetId,
            null!,
            null,
            RouteUpdateMode.Apply));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RouteUpdateRequest(
            workspace,
            RouteUpdateTestData.TargetId,
            patch,
            null,
            (RouteUpdateMode)int.MaxValue));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update complete no-op retains exact public facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void CompleteNoOpRetainsExactPublicFacts()
    {
        var result = RouteUpdateTestData.Result();

        Assert.Equal("route update", result.Command);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateMode.Apply, result.Mode);
        Assert.Equal(RouteUpdateTestData.TargetId, result.Target.Id);
        Assert.Equal(RouteUpdatePatchState.Unchanged, result.Patch.Description.State);
        Assert.Null(result.Template);
        Assert.Equal(RouteUpdatePlanCompleteness.Complete, result.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.Safe, result.Plan.Safety);
        Assert.Equal(RouteUpdateBodyState.Preserved, result.Plan.Body);
        Assert.Empty(result.Effects);
        Assert.Equal(
            [RouteUpdateTestData.ParentPath, RouteUpdateTestData.TargetPath],
            result.UnchangedPaths);
        Assert.Equal(RouteUpdateRecoveryState.NotRequired, result.Recovery.State);
        Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
        Assert.Empty(result.Findings);
        Assert.Null(result.Next);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update requested unresolved Template retains nullable unavailable facts"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RequestedUnresolvedTemplateRetainsNullableUnavailableFacts()
    {
        var template = new RouteUpdateTemplate
        {
            Requested = RouteUpdateTestData.TemplateId,
            Id = null,
            Path = null,
            Classification = null,
            BodyByteLength = null,
            Decision = RouteUpdateTemplateDecision.Unresolved,
        };
        var formation = RouteUpdateTestData.VerifiedNoOpFormation(
            template,
            findings:
            [
                RouteUpdateTestData.Finding(
                    RouteUpdateFindingCode.TemplateUnavailable,
                    target: RouteUpdateTestData.TemplateId),
            ]);

        var result = RouteUpdateTestData.Result(formation);

        var actual = Assert.IsType<RouteUpdateTemplate>(result.Template);
        Assert.Same(template, actual);
        Assert.Null(actual.Id);
        Assert.Null(actual.Path);
        Assert.Null(actual.Classification);
        Assert.Null(actual.BodyByteLength);
        Assert.Equal(RouteUpdateTemplateDecision.Unresolved, actual.Decision);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update result rejects duplicate and out-of-order effects"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void ResultRejectsDuplicateAndOutOfOrderEffects()
    {
        var target = RouteUpdateTestData.Effect();
        var parent = RouteUpdateTestData.Effect(
            RouteUpdateTestData.ParentPath,
            RouteUpdateEffectKind.GeneratedRegion);

        Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
            ChangedFormation([target, target])));
        Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
            ChangedFormation([parent, target])));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Update effect change requires both hashes"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    [InlineData("", "expected-hash")]
    [InlineData("before-hash", "")]
    public void EffectChangeRequiresBothHashes(string before, string expected)
    {
        var effect = RouteUpdateTestData.Effect() with
        {
            Change = new RouteUpdateEffectChange
            {
                Before = before,
                Expected = expected,
            },
        };

        Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
            ChangedFormation([effect])));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update unchanged paths are initialized ordered unique and disjoint"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void UnchangedPathsAreInitializedOrderedUniqueAndDisjoint()
    {
        var effect = RouteUpdateTestData.Effect();
        var cases = new ImmutableArray<string>[]
        {
            default,
            [RouteUpdateTestData.TargetPath, RouteUpdateTestData.ParentPath],
            [RouteUpdateTestData.ParentPath, RouteUpdateTestData.ParentPath],
            [RouteUpdateTestData.TargetPath],
        };

        foreach (var unchanged in cases)
        {
            Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
                ChangedFormation([effect]) with
                {
                    UnchangedPaths = unchanged,
                }));
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update recovery residual nullability follows state"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void RecoveryResidualNullabilityFollowsState()
    {
        Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
            RouteUpdateTestData.VerifiedNoOpFormation() with
            {
                Recovery = new RouteUpdateRecovery
                {
                    State = RouteUpdateRecoveryState.Retained,
                    ResidualPath = null,
                },
            }));
        Assert.Throws<ArgumentException>(() => RouteUpdateTestData.Result(
            RouteUpdateTestData.VerifiedNoOpFormation() with
            {
                Recovery = new RouteUpdateRecovery
                {
                    State = RouteUpdateRecoveryState.Removed,
                    ResidualPath = RouteUpdateTestData.RecoveryPath("recovery.zip"),
                },
            }));
    }

    private static RouteUpdateResultFormation ChangedFormation(
        ImmutableArray<RouteUpdateEffect> effects)
        => RouteUpdateTestData.VerifiedNoOpFormation() with
        {
            Effects = effects,
            UnchangedPaths = [],
        };

}
