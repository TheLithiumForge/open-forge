using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListFiniteValueTests
{
    [Theory(DisplayName = "Library List serializes every accepted observation state without enum names or numbers"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("record", (int)LibraryRecordViewState.NotStarted, "not-started")]
    [InlineData("record", (int)LibraryRecordViewState.Missing, "missing")]
    [InlineData("record", (int)LibraryRecordViewState.Complete, "complete")]
    [InlineData("record", (int)LibraryRecordViewState.Invalid, "invalid")]
    [InlineData("record", (int)LibraryRecordViewState.Unavailable, "unavailable")]
    [InlineData("record", (int)LibraryRecordViewState.Blocked, "blocked")]
    [InlineData("record", (int)LibraryRecordViewState.Failed, "failed")]
    [InlineData("record", (int)LibraryRecordViewState.Interrupted, "interrupted")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.NotStarted, "not-started")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.Available, "available")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.Missing, "missing")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.Unavailable, "unavailable")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.Invalid, "invalid")]
    [InlineData("sourceRoot", (int)LibrarySourceRootViewState.Blocked, "blocked")]
    [InlineData("link", (int)LibraryLinkViewState.NotStarted, "not-started")]
    [InlineData("link", (int)LibraryLinkViewState.Current, "current")]
    [InlineData("link", (int)LibraryLinkViewState.Missing, "missing")]
    [InlineData("link", (int)LibraryLinkViewState.Changed, "changed")]
    [InlineData("link", (int)LibraryLinkViewState.Unavailable, "unavailable")]
    [InlineData("link", (int)LibraryLinkViewState.Blocked, "blocked")]
    [InlineData("coverage", (int)LibraryCoverage.NotStarted, "not-started")]
    [InlineData("coverage", (int)LibraryCoverage.Complete, "complete")]
    [InlineData("coverage", (int)LibraryCoverage.Incomplete, "incomplete")]
    [InlineData("coverage", (int)LibraryCoverage.Blocked, "blocked")]
    [InlineData("coverage", (int)LibraryCoverage.Failed, "failed")]
    [InlineData("coverage", (int)LibraryCoverage.Interrupted, "interrupted")]
    [InlineData("inventory", (int)LibraryListInventoryState.NotStarted, "not-started")]
    [InlineData("inventory", (int)LibraryListInventoryState.NotRequested, "not-requested")]
    public void ObservationVocabulary(string field, int value, string expected)
    {
        using var document = JsonDocument.Parse(Render(field, value));
        var payload = document.RootElement.GetProperty("result");
        var actual = field switch
        {
            "record" => payload.GetProperty("record").GetProperty("state"),
            "sourceRoot" => payload.GetProperty("libraries")[0].GetProperty("sourceRootState"),
            "link" => payload.GetProperty("libraries")[0].GetProperty("paths")[0].GetProperty("state"),
            _ => payload.GetProperty(field),
        };
        Assert.Equal(expected, actual.GetString());
    }

    [Theory(DisplayName = "Library List rejects undefined observation values during rendering"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData("record")]
    [InlineData("sourceRoot")]
    [InlineData("link")]
    [InlineData("coverage")]
    [InlineData("inventory")]
    public void UndefinedObservationIsRejected(string field)
        => Assert.Throws<ArgumentOutOfRangeException>(() => Render(field, int.MaxValue));

    private static string Render(string field, int value)
    {
        var seed = LibraryListResultFixture.Create();
        var library = seed.Result.Libraries[0];
        var payload = field switch
        {
            "record" => seed.Result with { Record = seed.Result.Record with { State = (LibraryRecordViewState)value } },
            "sourceRoot" => seed.Result with { Libraries = [library with { SourceRootState = (LibrarySourceRootViewState)value }] },
            "link" => seed.Result with { Libraries = [library with { Paths = [library.Paths[0] with { State = (LibraryLinkViewState)value }] }] },
            "coverage" => seed.Result with { Coverage = (LibraryCoverage)value },
            "inventory" => seed.Result with { Inventory = (LibraryListInventoryState)value },
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        };
        return LibraryListPresentation.RenderJson(new(seed with { Result = payload }, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
    }
}
