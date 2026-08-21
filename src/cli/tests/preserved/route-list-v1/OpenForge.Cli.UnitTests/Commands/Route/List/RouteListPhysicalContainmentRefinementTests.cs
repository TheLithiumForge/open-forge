using OpenForge.Cli.Commands.Route.List.Filesystem;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListPhysicalContainmentRefinementTests
{
    [Theory(DisplayName = "Route-list physical containment states retain logical path resolved component and direct cause invariants"),
     InlineData((int)RouteListPhysicalContainmentState.Contained),
     InlineData((int)RouteListPhysicalContainmentState.LexicalEscape),
     InlineData((int)RouteListPhysicalContainmentState.PhysicalEscape),
     InlineData((int)RouteListPhysicalContainmentState.DanglingLink),
     InlineData((int)RouteListPhysicalContainmentState.Inaccessible),
     InlineData((int)RouteListPhysicalContainmentState.Cycle),
     InlineData((int)RouteListPhysicalContainmentState.Unsupported),
     Trait("Feature", "route-list"), Trait("Refinement", "route-list-acceptance"), Trait("Evidence", "Unit")]
    public void StatesRetainPathAndCauseInvariants(int stateValue)
    {
        var state = (RouteListPhysicalContainmentState)stateValue;
        const string logicalPath = "/workspace/.agents/root/source.md";
        const string resolvedThroughLastSuccessfulComponent = "/workspace/.agents/root";
        const string escapedPhysicalPath = "/outside/source.md";
        const string directCause = "The component could not be proved safe.";

        var result = state switch
        {
            RouteListPhysicalContainmentState.Contained => RouteListPhysicalContainmentResult.Contained(
                logicalPath,
                "/workspace/.agents/root/source.md"),
            RouteListPhysicalContainmentState.LexicalEscape => RouteListPhysicalContainmentResult.LexicalEscape(
                logicalPath,
                directCause),
            RouteListPhysicalContainmentState.PhysicalEscape => RouteListPhysicalContainmentResult.PhysicalEscape(
                logicalPath,
                escapedPhysicalPath,
                directCause),
            RouteListPhysicalContainmentState.DanglingLink => RouteListPhysicalContainmentResult.DanglingLink(
                logicalPath,
                directCause),
            RouteListPhysicalContainmentState.Inaccessible => RouteListPhysicalContainmentResult.Inaccessible(
                logicalPath,
                resolvedThroughLastSuccessfulComponent,
                directCause),
            RouteListPhysicalContainmentState.Cycle => RouteListPhysicalContainmentResult.Cycle(
                logicalPath,
                resolvedThroughLastSuccessfulComponent,
                directCause),
            RouteListPhysicalContainmentState.Unsupported => RouteListPhysicalContainmentResult.Unsupported(
                logicalPath,
                resolvedThroughLastSuccessfulComponent,
                directCause),
            _ => throw new ArgumentOutOfRangeException(nameof(stateValue)),
        };

        Assert.Equal(state, result.State);
        Assert.Equal(logicalPath, result.NormalizedLogicalPath);
        if (state == RouteListPhysicalContainmentState.Contained)
        {
            Assert.Equal("/workspace/.agents/root/source.md", result.ResolvedPhysicalPath);
            Assert.Null(result.DirectCause);
        }
        else
        {
            Assert.Equal(directCause, result.DirectCause);
        }

        if (state is RouteListPhysicalContainmentState.LexicalEscape or RouteListPhysicalContainmentState.DanglingLink)
        {
            Assert.Null(result.ResolvedPhysicalPath);
        }
        else if (state == RouteListPhysicalContainmentState.PhysicalEscape)
        {
            Assert.Equal(escapedPhysicalPath, result.ResolvedPhysicalPath);
        }
        else if (state is RouteListPhysicalContainmentState.Inaccessible
                 or RouteListPhysicalContainmentState.Cycle
                 or RouteListPhysicalContainmentState.Unsupported)
        {
            Assert.Equal(resolvedThroughLastSuccessfulComponent, result.ResolvedPhysicalPath);
        }
    }
}
