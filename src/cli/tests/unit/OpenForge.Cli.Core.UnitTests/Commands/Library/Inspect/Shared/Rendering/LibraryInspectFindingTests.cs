using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Presentation.Library.Inspect.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectFindingTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library Inspect serializes every accepted finding code independently")]
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
    [InlineData((int)LibraryInspectFindingCode.OwnershipObservation, "library-inspect.ownership-observation")]
    public void FindingVocabulary(int value, string expected)
    {
        var seed = LibraryInspectResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryInspectFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        using var document = JsonDocument.Parse(RenderJson(result));
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray())
            .GetProperty("code").GetString());
        Assert.Equal(expected, LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)value));
        Assert.Equal(expected, LibraryInspectWording.MachineCode((LibraryInspectFindingCode)value));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect finding inventory is closed and rejects undefined runtime values")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(17, Enum.GetValues<LibraryInspectFindingCode>().Length);
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectWording.MachineCode((LibraryInspectFindingCode)int.MaxValue));
    }

    private static string RenderJson(LibraryInspectResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryInspectResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryInspectPresentation.Rendering).PrimaryContent;
}
