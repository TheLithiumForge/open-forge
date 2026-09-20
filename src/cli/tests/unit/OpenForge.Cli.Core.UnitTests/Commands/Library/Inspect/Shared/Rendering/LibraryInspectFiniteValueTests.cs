using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectFiniteValueTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library Inspect serializes every accepted comparison relation as finite wire text")]
    [InlineData((int)LibraryComparisonRelation.NotStarted, "not-started")]
    [InlineData((int)LibraryComparisonRelation.Current, "current")]
    [InlineData((int)LibraryComparisonRelation.Added, "added")]
    [InlineData((int)LibraryComparisonRelation.Retired, "retired")]
    [InlineData((int)LibraryComparisonRelation.Missing, "missing")]
    [InlineData((int)LibraryComparisonRelation.Changed, "changed")]
    [InlineData((int)LibraryComparisonRelation.Unavailable, "unavailable")]
    [InlineData((int)LibraryComparisonRelation.Blocked, "blocked")]
    public void RelationVocabulary(int value, string expected)
    {
        var seed = LibraryInspectResultFixture.Create();
        var result = seed with
        {
            Result = seed.Result with
            {
                Projection = seed.Result.Projection with
                {
                    Comparisons = [seed.Result.Projection.Comparisons[0] with { Relation = (LibraryComparisonRelation)value }],
                },
            },
        };
        using var document = JsonDocument.Parse(RenderJson(result));
        Assert.Equal(expected, document.RootElement.GetProperty("data").GetProperty("files")[0].GetProperty("relation").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect rejects undefined comparison relations during rendering")]
    public void UndefinedRelationIsRejected()
    {
        var seed = LibraryInspectResultFixture.Create();
        var result = seed with
        {
            Result = seed.Result with
            {
                Projection = seed.Result.Projection with
                {
                    Comparisons = [seed.Result.Projection.Comparisons[0] with { Relation = (LibraryComparisonRelation)int.MaxValue }],
                },
            },
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => RenderJson(result));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectWording.RelationWire((LibraryComparisonRelation)int.MaxValue));
    }

    private static string RenderJson(LibraryInspectResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryInspectResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryInspectPresentation.Rendering).PrimaryContent;
}
