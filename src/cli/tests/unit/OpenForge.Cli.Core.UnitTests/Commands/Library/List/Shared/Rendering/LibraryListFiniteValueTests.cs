using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListFiniteValueTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List serializes every accepted record state as finite wire text")]
    [InlineData((int)LibraryRecordViewState.NotStarted, "not-started")]
    [InlineData((int)LibraryRecordViewState.Missing, "missing")]
    [InlineData((int)LibraryRecordViewState.Complete, "complete")]
    [InlineData((int)LibraryRecordViewState.Invalid, "invalid")]
    [InlineData((int)LibraryRecordViewState.Unavailable, "unavailable")]
    [InlineData((int)LibraryRecordViewState.Blocked, "blocked")]
    [InlineData((int)LibraryRecordViewState.Failed, "failed")]
    [InlineData((int)LibraryRecordViewState.Interrupted, "interrupted")]
    public void RecordVocabulary(int value, string expected)
    {
        var seed = LibraryListResultFixture.Create();
        var result = seed with
        {
            Result = seed.Result with { Record = seed.Result.Record with { State = (LibraryRecordViewState)value } },
        };
        using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Full));
        Assert.Equal(expected, document.RootElement.GetProperty("data").GetProperty("recordCoverage").GetProperty("state").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List serializes every accepted link state as finite wire text")]
    [InlineData((int)LibraryLinkViewState.NotStarted, "not-started")]
    [InlineData((int)LibraryLinkViewState.Current, "current")]
    [InlineData((int)LibraryLinkViewState.Missing, "missing")]
    [InlineData((int)LibraryLinkViewState.Changed, "changed")]
    [InlineData((int)LibraryLinkViewState.Unavailable, "unavailable")]
    [InlineData((int)LibraryLinkViewState.Blocked, "blocked")]
    public void LinkVocabulary(int value, string expected)
    {
        var seed = LibraryListResultFixture.Create();
        var library = seed.Result.Libraries[0];
        var result = seed with
        {
            Result = seed.Result with
            {
                Libraries = [library with { Paths = [library.Paths[0] with { State = (LibraryLinkViewState)value }] }],
            },
        };
        using var document = JsonDocument.Parse(RenderJson(result, CliDetail.Standard));
        Assert.Equal(expected, document.RootElement.GetProperty("data").GetProperty("libraries")[0]
            .GetProperty("links")[0].GetProperty("state").GetString());
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List wording rejects undefined native observation values")]
    [InlineData("record")]
    [InlineData("source")]
    [InlineData("link")]
    public void UndefinedObservationIsRejected(string field)
        => Assert.Throws<ArgumentOutOfRangeException>(() => field switch
        {
            "record" => LibraryListWording.RecordState((LibraryRecordViewState)int.MaxValue),
            "source" => LibraryListWording.SourceRootState((LibrarySourceRootViewState)int.MaxValue),
            "link" => LibraryListWording.LinkState((LibraryLinkViewState)int.MaxValue),
            _ => throw new ArgumentOutOfRangeException(nameof(field)),
        });

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List finding inventory is closed and includes invalid input")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(14, Enum.GetValues<LibraryListFindingCode>().Length);
        Assert.Equal("library-list.invalid-input", LibraryListWording.MachineCode(LibraryListFindingCode.InvalidInput));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListWording.MachineCode((LibraryListFindingCode)int.MaxValue));
    }

    private static string RenderJson(LibraryListResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, detail, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
}
