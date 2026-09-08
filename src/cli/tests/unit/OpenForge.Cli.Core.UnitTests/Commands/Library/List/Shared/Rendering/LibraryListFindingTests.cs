using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListFindingTests
{
    [Theory(DisplayName = "Library List serializes every accepted finding code independently"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)LibraryListFindingCode.InvalidRecord, "library-list.invalid-record")]
    [InlineData((int)LibraryListFindingCode.RecordUnavailable, "library-list.record-unavailable")]
    [InlineData((int)LibraryListFindingCode.RecordBlocked, "library-list.record-blocked")]
    [InlineData((int)LibraryListFindingCode.SourceRootInvalid, "library-list.source-root-invalid")]
    [InlineData((int)LibraryListFindingCode.SourceRootUnavailable, "library-list.source-root-unavailable")]
    [InlineData((int)LibraryListFindingCode.SourceRootBlocked, "library-list.source-root-blocked")]
    [InlineData((int)LibraryListFindingCode.LinkMissing, "library-list.link-missing")]
    [InlineData((int)LibraryListFindingCode.LinkChanged, "library-list.link-changed")]
    [InlineData((int)LibraryListFindingCode.LinkUnavailable, "library-list.link-unavailable")]
    [InlineData((int)LibraryListFindingCode.LinkBlocked, "library-list.link-blocked")]
    [InlineData((int)LibraryListFindingCode.OperationFailed, "library-list.operation-failed")]
    [InlineData((int)LibraryListFindingCode.Interrupted, "library-list.interrupted")]
    public void FindingVocabulary(int value, string expected)
    {
        var seed = LibraryListResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryListFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        var json = LibraryListPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
        using var document = JsonDocument.Parse(json);
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(expected, LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)value));
    }

    [Fact(DisplayName = "Library List finding inventory is closed and rejects undefined runtime values"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(12, Enum.GetValues<LibraryListFindingCode>().Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)int.MaxValue));
    }
}
