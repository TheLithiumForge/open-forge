using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Shared.Completion;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Completion;

public sealed class LibraryMutationCompletionProjectionTests
{
    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void AttachObservationProjectsEligibleUnregisteredPath()
    {
        var observation = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var result = LibraryMutationCompletionProjection.Projection(observation, LibraryMutationPlanningData.Id, LibraryPlanState.Complete, sourceIndependent: false);

        var mapping = Assert.Single(result.Mappings);
        Assert.Equal(LibraryMutationPlanningData.Leaf, mapping.SourcePath);
        Assert.Equal(LibraryComparisonRelation.Added, mapping.Relation);
        Assert.Equal(LibraryLinkViewState.Missing, mapping.State);
        Assert.Equal("directives/review", mapping.SourceId);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void SyncObservationDistinguishesCurrentAndRetiredPaths()
    {
        const string retired = ".agents/directives/retired.md";
        var observation = LibraryMutationPlanningData.Sync([LibraryMutationPlanningData.Leaf], [LibraryMutationPlanningData.Leaf, retired]);
        var result = LibraryMutationCompletionProjection.Projection(observation, LibraryMutationPlanningData.Id, LibraryPlanState.Complete, sourceIndependent: false);

        Assert.Equal([retired, LibraryMutationPlanningData.Leaf], result.Mappings.Select(mapping => mapping.SourcePath));
        Assert.Equal([LibraryComparisonRelation.Retired, LibraryComparisonRelation.Current], result.Mappings.Select(mapping => mapping.Relation));
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void DetachObservationHasNoSourceAndRetiresTheRegisteredLink()
    {
        ILibraryMutationObservation observation = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        Assert.Null(observation.Source);

        var result = LibraryMutationCompletionProjection.Projection(observation, LibraryMutationPlanningData.Id, LibraryPlanState.Complete, sourceIndependent: true);

        var mapping = Assert.Single(result.Mappings);
        Assert.Equal(LibraryComparisonRelation.Retired, mapping.Relation);
        Assert.Equal(LibraryLinkViewState.Current, mapping.State);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", mapping.ObservedRelativeLink);
    }

    [Trait("Boundary", "Processing")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void AbsentObservationDoesNotInventMappingsOrOwnership()
    {
        var result = LibraryMutationCompletionProjection.Projection(null, null, LibraryPlanState.NotStarted, sourceIndependent: false);

        Assert.Equal(LibraryPlanState.NotStarted, result.State);
        Assert.Empty(result.Mappings);
        Assert.Empty(result.Ownership);
    }
}
