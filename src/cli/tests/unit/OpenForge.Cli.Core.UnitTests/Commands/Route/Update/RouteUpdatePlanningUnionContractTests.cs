using System.Reflection;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

public sealed class RouteUpdatePlanningUnionContractTests
{
    [Fact(DisplayName = "Route Update planning unions reject incomplete and contradictory states"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void PlanningUnionsRejectIncompleteAndContradictoryStates()
    {
        var observation = RouteUpdateTestData.Observation();
        var boundary = new RouteUpdatePlanningBoundary
        {
            Formation = RouteUpdateTestData.VerifiedNoOpFormation(),
        };
        var selection = new RouteUpdateSourceSelection
        {
            Request = observation.Request,
            Target = observation.Target,
            Catalogue = observation.Catalogue,
            Source = observation.TargetSource,
        };
        var layout = new RouteUpdateMetadataLayout
        {
            Source = "open-forge:\n  description: Before\n  tags: [Memory]\n",
            Description = "Before",
            Tags = ["Memory"],
            Responsibility = null,
            DescriptionMember = null,
            ResponsibilityMember = null,
            TagsMember = null,
        };
        var editPlan = new RouteUpdateMetadataEditPlan
        {
            Edits = [],
            Preview = [],
        };

        AssertRejectsUnion(
            typeof(RouteUpdateSourceSelectionBuild),
            success: null,
            failure: null);
        AssertRejectsUnion(
            typeof(RouteUpdateSourceSelectionBuild),
            selection,
            boundary);
        AssertRejectsUnion(
            typeof(RouteUpdateMetadataLayoutRead),
            success: null,
            failure: null);
        AssertRejectsUnion(
            typeof(RouteUpdateMetadataLayoutRead),
            layout,
            "unsafe");
        AssertRejectsUnion(
            typeof(RouteUpdateMetadataEditPlanBuild),
            success: null,
            failure: null);
        AssertRejectsUnion(
            typeof(RouteUpdateMetadataEditPlanBuild),
            editPlan,
            "unsafe");
    }

    [Fact(DisplayName = "Route Update observed layers reject partial failure evidence beside success"), Trait("Feature", "route-update"), Trait("Evidence", "UnitContract")]
    public void ObservedLayersRejectPartialFailureEvidenceBesideSuccess()
    {
        var observation = RouteUpdateTestData.Observation();
        var layer = new RouteUpdateObservedLayer
        {
            Text = observation.TargetText,
            Snapshot = observation.TargetSnapshot,
        };

        AssertRejects(
            typeof(RouteUpdateObservedLayerBuild),
            layer,
            RouteUpdateFindingCode.TargetUnsafe,
            null,
            false);
        AssertRejects(
            typeof(RouteUpdateObservedLayerBuild),
            layer,
            null,
            "unsafe",
            false);
    }

    private static void AssertRejectsUnion(
        Type type,
        object? success,
        object? failure)
        => AssertRejects(type, success, failure);

    private static void AssertRejects(Type type, params object?[] arguments)
    {
        var constructor = Assert.Single(type.GetConstructors(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        var exception = Assert.Throws<TargetInvocationException>(
            () => constructor.Invoke(arguments));

        Assert.IsType<ArgumentException>(exception.InnerException);
    }
}
