using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectFiniteValueTests
{
    [Theory(DisplayName = "Library Inspect serializes every accepted observation state without enum names or numbers"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("inventory", (int)LibraryInventoryViewState.NotStarted, "not-started")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Complete, "complete")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Incomplete, "incomplete")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Invalid, "invalid")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Blocked, "blocked")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Failed, "failed")]
    [InlineData("inventory", (int)LibraryInventoryViewState.Interrupted, "interrupted")]
    [InlineData("relation", (int)LibraryComparisonRelation.NotStarted, "not-started")]
    [InlineData("relation", (int)LibraryComparisonRelation.Current, "current")]
    [InlineData("relation", (int)LibraryComparisonRelation.Added, "added")]
    [InlineData("relation", (int)LibraryComparisonRelation.Retired, "retired")]
    [InlineData("relation", (int)LibraryComparisonRelation.Missing, "missing")]
    [InlineData("relation", (int)LibraryComparisonRelation.Changed, "changed")]
    [InlineData("relation", (int)LibraryComparisonRelation.Unavailable, "unavailable")]
    [InlineData("relation", (int)LibraryComparisonRelation.Blocked, "blocked")]
    public void ObservationVocabulary(string field, int value, string expected)
    {
        using var document = JsonDocument.Parse(Render(field, value));
        var payload = document.RootElement.GetProperty("result");
        var actual = field == "inventory"
            ? payload.GetProperty("source").GetProperty("state")
            : payload.GetProperty("projection").GetProperty("comparisons")[0].GetProperty("relation");
        Assert.Equal(expected, actual.GetString());
    }

    [Theory(DisplayName = "Library Inspect rejects undefined observation values during rendering"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("inventory")]
    [InlineData("relation")]
    public void UndefinedObservationIsRejected(string field)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Render(field, int.MaxValue));

    private static string Render(string field, int value)
    {
        var seed = LibraryInspectResultFixture.Create();
        var payload = field switch
        {
            "inventory" => seed.Result with { Source = seed.Result.Source with { State = (LibraryInventoryViewState)value } },
            "relation" => seed.Result with
            {
                Projection = seed.Result.Projection with
                {
                    Comparisons = [seed.Result.Projection.Comparisons[0] with { Relation = (LibraryComparisonRelation)value }],
                },
            },
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        return LibraryInspectPresentation.RenderJson(new(seed with { Result = payload }, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
    }
}
