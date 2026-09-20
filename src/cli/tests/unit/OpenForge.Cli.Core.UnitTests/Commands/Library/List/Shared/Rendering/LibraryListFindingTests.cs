using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Presentation.Library.List.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListFindingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List serializes every accepted finding code independently")]
    [InlineData((int)LibraryListFindingCode.InvalidInput, "library-list.invalid-input")]
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
    [InlineData((int)LibraryListFindingCode.OwnershipObservation, "library-list.ownership-observation")]
    public void FindingVocabulary(int value, string expected)
    {
        var seed = LibraryListResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryListFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        using var document = JsonDocument.Parse(RenderJson(result));
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray())
            .GetProperty("code").GetString());
        Assert.Equal(expected, LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)value));
        Assert.Equal(expected, LibraryListWording.MachineCode((LibraryListFindingCode)value));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List finding inventory is closed and rejects undefined runtime values")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(14, Enum.GetValues<LibraryListFindingCode>().Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListWording.MachineCode((LibraryListFindingCode)int.MaxValue));
    }

    private static string RenderJson(LibraryListResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
}
