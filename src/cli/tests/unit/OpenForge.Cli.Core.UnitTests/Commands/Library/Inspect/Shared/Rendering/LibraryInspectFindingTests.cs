using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectFindingTests
{
    [Theory(DisplayName = "Library Inspect serializes every accepted finding code independently"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)LibraryInspectFindingCode.InvalidId, "library-inspect.invalid-id")]
    [InlineData((int)LibraryInspectFindingCode.UnknownId, "library-inspect.unknown-id")]
    [InlineData((int)LibraryInspectFindingCode.RecordInvalid, "library-inspect.record-invalid")]
    [InlineData((int)LibraryInspectFindingCode.RecordUnavailable, "library-inspect.record-unavailable")]
    [InlineData((int)LibraryInspectFindingCode.RecordBlocked, "library-inspect.record-blocked")]
    [InlineData((int)LibraryInspectFindingCode.SourceRootInvalid, "library-inspect.source-root-invalid")]
    [InlineData((int)LibraryInspectFindingCode.SourceRootUnavailable, "library-inspect.source-root-unavailable")]
    [InlineData((int)LibraryInspectFindingCode.SourceRootBlocked, "library-inspect.source-root-blocked")]
    [InlineData((int)LibraryInspectFindingCode.InventoryIncomplete, "library-inspect.inventory-incomplete")]
    [InlineData((int)LibraryInspectFindingCode.PathAdded, "library-inspect.path-added")]
    [InlineData((int)LibraryInspectFindingCode.PathRetired, "library-inspect.path-retired")]
    [InlineData((int)LibraryInspectFindingCode.LinkMissing, "library-inspect.link-missing")]
    [InlineData((int)LibraryInspectFindingCode.LinkChanged, "library-inspect.link-changed")]
    [InlineData((int)LibraryInspectFindingCode.LinkBlocked, "library-inspect.link-blocked")]
    [InlineData((int)LibraryInspectFindingCode.OperationFailed, "library-inspect.operation-failed")]
    [InlineData((int)LibraryInspectFindingCode.Interrupted, "library-inspect.interrupted")]
    public void FindingVocabulary(int value, string expected)
    {
        var seed = LibraryInspectResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryInspectFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        var json = LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
        using var document = JsonDocument.Parse(json);
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal(expected, LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)value));
    }

    [Fact(DisplayName = "Library Inspect finding inventory is closed and rejects undefined runtime values"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(16, Enum.GetValues<LibraryInspectFindingCode>().Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)int.MaxValue));
    }
}
