using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdatePlanProjectorTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update projector forms a complete safe no-op with ordered unchanged paths"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void FormsCompleteSafeNoOpWithOrderedUnchangedPaths()
    {
        var observation = RouteUpdateTestData.Observation(
            RouteUpdateTestData.DescriptionPatch("Before"));
        var destination = RouteUpdateTestData.DestinationPlan() with
        {
            Body = RouteUpdateTestData.BodyPlan(
                observation,
                intendedText: RouteUpdateTestData.TargetText),
            IntendedTargetBytes = ImmutableArray.CreateRange(
                Encoding.UTF8.GetBytes(RouteUpdateTestData.TargetText)),
        };

        var build = Build(destination, observation);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);

        Assert.Same(plan.Preview, build.Formation);
        Assert.True(plan.IsNoOp);
        Assert.Empty(plan.FileChanges);
        Assert.Empty(plan.RecoveryTargets);
        Assert.Equal(RouteUpdatePlanCompleteness.Complete, plan.Preview.Plan.Completeness);
        Assert.Equal(RouteUpdatePlanSafety.Safe, plan.Preview.Plan.Safety);
        Assert.Equal(RouteUpdateBodyState.Preserved, plan.Preview.Plan.Body);
        Assert.Empty(plan.Preview.Effects);
        Assert.Equal(
            [RouteUpdateTestData.ParentPath, RouteUpdateTestData.TargetPath],
            plan.Preview.UnchangedPaths);
        Assert.Equal(RouteUpdateVerificationState.NotRequested, plan.Preview.Verification);
        Assert.Empty(plan.Preview.Findings);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update projector creates only the routed-file effect for responsibility-only change"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void CreatesOnlyRoutedFileEffectForResponsibilityOnlyChange()
    {
        const string expectedText = "---\nopen-forge:\n  description: Before\n  responsibility: After responsibility\n  tags: [Memory, Before]\n  custom: preserve-me\n---\n\n# Authored body\n\nKeep this body.";
        var observation = RouteUpdateTestData.Observation(
            RouteUpdateTestData.ResponsibilityPatch("After responsibility"));
        var metadata = RouteUpdateTestData.MetadataPlan(observation, expectedText);
        var body = RouteUpdateTestData.BodyPlan(
            observation,
            intendedText: expectedText) with
        {
            Metadata = metadata,
            Preview = metadata.Preview,
        };
        var destination = RouteUpdateTestData.DestinationPlan() with
        {
            Body = body,
            IntendedTargetBytes = body.IntendedTargetBytes,
        };

        var build = Build(destination, observation);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        var effect = Assert.Single(plan.Preview.Effects);

        Assert.False(plan.IsNoOp);
        Assert.Single(plan.FileChanges);
        Assert.Equal(RouteUpdateTestData.TargetPath, effect.Path);
        Assert.Equal(RouteUpdateEffectKind.RoutedFile, effect.Kind);
        Assert.Equal(RouteUpdateEffectAction.Replace, effect.Action);
        Assert.NotEqual(effect.Change.Before, effect.Change.Expected);
        Assert.Equal(RouteUpdateEffectOutcome.Planned, effect.Outcome);
        Assert.Equal(RouteUpdateEffectResidual.None, effect.Residual);
        Assert.DoesNotContain(
            plan.Preview.Effects,
            candidate => candidate.Kind == RouteUpdateEffectKind.GeneratedRegion);
        Assert.Equal([RouteUpdateTestData.ParentPath], plan.Preview.UnchangedPaths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Update projector exposes protected Template attention without a change"), Trait("Feature", "route-update"), Trait("Evidence", "UnitBehavior")]
    public void ExposesProtectedTemplateAttentionWithoutChange()
    {
        var observation = RouteUpdateTestData.Observation(
            templateReference: RouteUpdateTestData.TemplateId);
        var body = RouteUpdateTestData.BodyPlan(
            observation,
            RouteUpdateTestData.ProtectedTemplate(),
            RouteUpdateBodyState.AuthoredBodyProtected);
        var destination = RouteUpdateTestData.DestinationPlan() with
        {
            Body = body,
            IntendedTargetBytes = body.IntendedTargetBytes,
        };

        var build = Build(destination, observation);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        var finding = Assert.Single(plan.Preview.Findings);

        Assert.True(plan.IsNoOp);
        Assert.Equal(RouteUpdateFindingCode.TemplateBodyProtected, finding.Code);
        Assert.Equal(CliSemanticStatus.Attention, finding.Status);
        Assert.Equal(RouteUpdateTestData.TargetPath, finding.Target);
        Assert.Equal(
            RouteUpdateTemplateDecision.AuthoredBodyProtected,
            plan.Preview.Template?.Decision);
        Assert.Equal(RouteUpdateBodyState.AuthoredBodyProtected, plan.Preview.Plan.Body);
        Assert.Empty(plan.Preview.Effects);
    }

    private static RouteUpdatePlanBuild Build(
        RouteUpdateDestinationPlan destination,
        RouteUpdateObservation observation)
        => new RouteUpdatePlanProjector().Build(
            new RouteUpdatePlanProjectionInput
            {
                Destination = destination,
                Navigation = RouteUpdateTestData.NavigationPlan(observation),
            });
}
