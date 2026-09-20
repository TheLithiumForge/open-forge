using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Detach.Shared.Planning;

public sealed class LibraryDetachConsumerBoundaryTests
{
    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("unobserved"), InlineData("ancestor-gap")]
    public void CoverageGapNeverMeansMissingDirectory(string scenario)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        input = input with
        {
            ConsumerBoundary = input.ConsumerBoundary with
            {
                ConsumerRoot = scenario == "unobserved" ? null : input.ConsumerBoundary.ConsumerRoot,
                Ancestors = [],
                IsComplete = false,
            }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Incomplete, plan.State);
        Assert.Empty(plan.Directories);
    }

    [Trait("Boundary", "Processing")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("missing-root"), InlineData("file-root"), InlineData("linked-parent")]
    public void PositiveUnsafeBoundaryBlocksCompletePlan(string scenario)
    {
        var input = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var absolute = LibraryMutationPlanningData.Absolute(scenario == "linked-parent" ? ".agents/directives" : ".agents");
        var leaf = scenario switch
        {
            "missing-root" => NoFollowLeafObservation.Missing(absolute),
            "file-root" => NoFollowLeafObservation.OrdinaryFile(absolute),
            _ => NoFollowLeafObservation.Classified(absolute, NoFollowLeafState.ReparsePoint),
        };
        var containment = scenario == "missing-root"
            ? PhysicalPathResolution.Classified(PhysicalPathState.Missing, absolute)
            : PhysicalPathResolution.Contained(absolute, absolute);
        var observation = new LibraryConsumerDirectoryObservation(leaf, containment);
        input = input with
        {
            ConsumerBoundary = scenario == "linked-parent"
            ? input.ConsumerBoundary with { Ancestors = [observation] }
            : input.ConsumerBoundary with { ConsumerRoot = observation }
        };
        var plan = LibraryDetachPlanner.Plan(input, TestContext.Current.CancellationToken);
        Assert.Equal(LibraryPlanState.Blocked, plan.State);
        Assert.NotEmpty(plan.Findings);
    }
}
