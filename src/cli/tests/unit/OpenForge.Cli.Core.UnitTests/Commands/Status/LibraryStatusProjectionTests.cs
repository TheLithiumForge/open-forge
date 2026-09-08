using System.Text;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Status.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class LibraryStatusProjectionTests
{
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Current", "Complete", "trusted"), InlineData("Missing", "Attention", "trusted")]
    [InlineData("Changed", "Attention", "trusted"), InlineData("Blocked", "Blocked", "blocked")]
    [InlineData("Unavailable", "Incomplete", "incomplete")]
    public void BoundedMappingFactsSelectStatusWithoutInventingInventory(
        string mappingState,
        string status,
        string publicState)
    {
        var state = Enum.Parse<LibraryMappingObservationState>(mappingState);
        var observation = Observation(state);
        var result = StatusLibraryAggregator.Build(observation);
        Assert.Equal(Enum.Parse<CliSemanticStatus>(status), result.State);
        Assert.Same(observation, result.Observation);
        var registration = Assert.Single(result.Records);
        Assert.Equal("team-knowledge", registration.Id.Value);
        Assert.Equal(1, registration.Registered.Value);
        var link = Assert.Single(registration.Links);
        Assert.Equal("directives/review", link.SourceId);
        Assert.Same(observation.Mappings[0], link.Observation);
        Assert.Equal(1, result.Counts.Registered.Value);
        Assert.Equal(mappingState == "Current" ? 1 : 0, result.Counts.Current.Value);
        Assert.Equal(mappingState == "Missing" ? 1 : 0, result.Counts.Missing.Value);
        Assert.Equal(mappingState == "Changed" ? 1 : 0, result.Counts.Changed.Value);
        Assert.Equal(mappingState == "Blocked" ? 1 : 0, result.Counts.Blocked.Value);
        Assert.Equal(mappingState == "Unavailable" ? 1 : 0, result.Counts.Unavailable.Value);
        Assert.Equal(publicState, StatusLibraryPresentation.Project(result).State);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Current", "current"), InlineData("Missing", "missing"), InlineData("Changed", "changed")]
    [InlineData("Blocked", "blocked"), InlineData("Unavailable", "unavailable")]
    public void JsonPreservesEveryBoundedLinkStateAndIndependentSourceId(string mappingState, string wire)
    {
        var result = Result(Enum.Parse<LibraryMappingObservationState>(mappingState));
        var json = StatusLibraryPresentation.Project(result);
        Assert.Equal(".agents/open-forge.libraries.json", json.Record.Path);
        var registration = Assert.Single(json.Records);
        Assert.Equal("team-knowledge", registration.Id);
        Assert.Equal("shared/team-knowledge", registration.SourceRoot);
        var link = Assert.Single(registration.RegisteredLinks.Links);
        Assert.Equal(wire, link.State);
        Assert.Equal("directives/review", link.SourceId);
        Assert.Equal(".agents/directives/review.md", link.DestinationPath);
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", link.ExpectedRelativeLink);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void HumanRetainsLibraryIdentityAndBoundedDrift()
    {
        var builder = new StringBuilder();
        StatusLibraryPresentation.Append(builder, Result(LibraryMappingObservationState.Missing));
        var text = builder.ToString();
        Assert.Contains("State: trusted", text, StringComparison.Ordinal);
        Assert.Contains("Record: complete", text, StringComparison.Ordinal);
        Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
        Assert.Contains("source-root=available", text, StringComparison.Ordinal);
        Assert.Contains("source=available", text, StringComparison.Ordinal);
        Assert.Contains("registered=1", text, StringComparison.Ordinal);
        Assert.Contains("current=0, missing=1, changed=0, blocked=0, unavailable=0", text, StringComparison.Ordinal);
        Assert.Contains("missing", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("shared/team-knowledge", text, StringComparison.Ordinal);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void AbsentRecordIsCompleteZeroWithoutAdoption()
    {
        var observation = Observation(LibraryMappingObservationState.Current) with
        {
            Record = LibraryMutationPlanningData.MissingRecord(),
            Sources = [],
            Mappings = [],
        };
        var result = StatusLibraryAggregator.Build(observation);
        Assert.Equal(CliSemanticStatus.Complete, result.State);
        Assert.Equal("absent", StatusLibraryPresentation.Project(result).State);
        Assert.Empty(result.Records);
        Assert.Equal(0, result.Counts.Registered.Value);
        Assert.Empty(result.Findings);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UndefinedSemanticStatusIsRejectedByBothRenderers()
    {
        var invalid = Result(LibraryMappingObservationState.Current) with { State = (CliSemanticStatus)int.MaxValue };
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusLibraryPresentation.Project(invalid));
        Assert.Throws<ArgumentOutOfRangeException>(() => StatusLibraryPresentation.Append(new StringBuilder(), invalid));
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Malformed", "Blocked", "blocked", "invalid")]
    [InlineData("Unavailable", "Incomplete", "incomplete", "unavailable")]
    [InlineData("Blocked", "Blocked", "blocked", "blocked")]
    public void RecordBoundaryCannotBeConvertedToAnEmptyTrustedLibrarySet(
        string recordState,
        string status,
        string publicState,
        string publicRecordState)
    {
        var view = Observation(LibraryMappingObservationState.Current);
        view = view with
        {
            State = recordState == "Unavailable" ? OperationalViewState.Incomplete : OperationalViewState.Blocked,
            Record = view.Record with
            {
                State = Enum.Parse<LibrariesRecordReadState>(recordState),
                Record = null,
                Snapshot = null,
                Cause = "Required record boundary unavailable or unsafe.",
            },
            Sources = [],
            Mappings = [],
        };
        var result = StatusLibraryAggregator.Build(view);
        Assert.Equal(Enum.Parse<CliSemanticStatus>(status), result.State);
        Assert.Same(view, result.Observation);
        Assert.NotEmpty(result.Findings);
        var projected = StatusLibraryPresentation.Project(result);
        Assert.Equal(publicState, projected.State);
        Assert.Equal(publicRecordState, projected.Record.State);
    }

    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    public void UnavailableRegisteredSourceRemainsIncompleteEvenWithAnExactLink()
    {
        var view = Observation(LibraryMappingObservationState.Current);
        view = view with
        {
            State = OperationalViewState.Incomplete,
            Sources = [view.Sources[0] with
            {
                State = LibrarySourceRootState.Unavailable, PhysicalSourceRoot = null, PhysicalAgentsDirectory = null,
                PhysicallyContained = null, PhysicallyDisjoint = null, Cause = "Required source root unavailable.",
            }],
        };
        var result = StatusLibraryAggregator.Build(view);
        Assert.Equal(CliSemanticStatus.Incomplete, result.State);
        Assert.Equal(OperationalSourceAvailability.Unavailable, Assert.Single(result.Records).SourceAvailability);
        Assert.Equal(1, result.Counts.Registered.Value);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Missing", "missing"), InlineData("Complete", "complete"), InlineData("Malformed", "invalid")]
    [InlineData("Unavailable", "unavailable"), InlineData("Blocked", "blocked")]
    public void JsonMapsEveryLibraryRecordStateToItsOwnedVocabulary(string state, string wire)
    {
        var value = Result(LibraryMappingObservationState.Current);
        value = value with
        {
            Observation = value.Observation with
            {
                Record = value.Observation.Record with { State = Enum.Parse<LibrariesRecordReadState>(state) },
            },
        };

        Assert.Equal(wire, StatusLibraryPresentation.Project(value).Record.State);
    }

    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Unit")]
    [InlineData("Available", "available"), InlineData("Missing", "missing"), InlineData("Invalid", "invalid")]
    [InlineData("Inaccessible", "unavailable"), InlineData("Unavailable", "unavailable"), InlineData("Blocked", "blocked")]
    public void JsonMapsEveryLibrarySourceRootStateToItsOwnedVocabulary(string state, string wire)
    {
        var value = Result(LibraryMappingObservationState.Current);
        value = value with
        {
            Records = [value.Records[0] with { SourceRootState = Enum.Parse<LibrarySourceRootState>(state) }],
        };

        Assert.Equal(wire, Assert.Single(StatusLibraryPresentation.Project(value).Records).SourceRootState);
    }

    private static LibraryStatusView Observation(LibraryMappingObservationState state)
        => new()
        {
            State = state switch
            {
                LibraryMappingObservationState.Blocked => OperationalViewState.Blocked,
                LibraryMappingObservationState.Unavailable => OperationalViewState.Incomplete,
                _ => OperationalViewState.Complete,
            },
            Ownership = LibraryMutationPlanningData.Ownership(),
            LinkCapability = null,
            Record = LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf),
            Sources = [LibraryMutationPlanningData.Inventory(LibraryMutationPlanningData.Leaf).Source],
            Mappings = [LibraryMutationPlanningData.Mapping(LibraryMutationPlanningData.Leaf, state)],
        };

    private static StatusLibrary Result(LibraryMappingObservationState state)
    {
        var observation = Observation(state);
        var counts = new StatusLibraryCounts
        {
            Registered = Count(1),
            Current = Count(state == LibraryMappingObservationState.Current ? 1 : 0),
            Missing = Count(state == LibraryMappingObservationState.Missing ? 1 : 0),
            Changed = Count(state == LibraryMappingObservationState.Changed ? 1 : 0),
            Blocked = Count(state == LibraryMappingObservationState.Blocked ? 1 : 0),
            Unavailable = Count(state == LibraryMappingObservationState.Unavailable ? 1 : 0),
        };
        return new StatusLibrary
        {
            State = state switch
            {
                LibraryMappingObservationState.Current => CliSemanticStatus.Complete,
                LibraryMappingObservationState.Blocked => CliSemanticStatus.Blocked,
                LibraryMappingObservationState.Unavailable => CliSemanticStatus.Incomplete,
                _ => CliSemanticStatus.Attention,
            },
            Observation = observation,
            Counts = counts,
            Findings = [],
            Records = [new StatusLibraryRegistration
            {
                Id = observation.Record.Record!.Libraries[0].Id,
                SourceRoot = observation.Record.Record.Libraries[0].SourceRoot,
                SourceRootState = observation.Sources[0].State,
                SourceAvailability = OperationalSourceAvailability.Available, Registered = Count(1), Counts = counts,
                Links = [new StatusLibraryLink(observation.Mappings[0], "directives/review")],
            }],
        };
    }

    private static StatusIntegerValue Count(int value) => new(OperationalValueState.Available, value);
}
