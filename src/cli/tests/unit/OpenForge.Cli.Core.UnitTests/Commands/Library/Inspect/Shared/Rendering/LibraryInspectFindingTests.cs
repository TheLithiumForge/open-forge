using OpenForge.Cli.Core.Presentation.Shared.Rendering;
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
    internal static readonly LibraryInspectFindingCode[] AcceptedFindings =
    [
        LibraryInspectFindingCode.InvalidId,
        LibraryInspectFindingCode.UnknownId,
        LibraryInspectFindingCode.RecordInvalid,
        LibraryInspectFindingCode.RecordUnavailable,
        LibraryInspectFindingCode.RecordBlocked,
        LibraryInspectFindingCode.SourceRootInvalid,
        LibraryInspectFindingCode.SourceRootUnavailable,
        LibraryInspectFindingCode.SourceRootBlocked,
        LibraryInspectFindingCode.InventoryIncomplete,
        LibraryInspectFindingCode.PathAdded,
        LibraryInspectFindingCode.PathRetired,
        LibraryInspectFindingCode.LinkMissing,
        LibraryInspectFindingCode.LinkChanged,
        LibraryInspectFindingCode.LinkBlocked,
        LibraryInspectFindingCode.OperationFailed,
        LibraryInspectFindingCode.Interrupted,
        LibraryInspectFindingCode.OwnershipObservation,
        LibraryInspectFindingCode.LayoutUnavailable,
    ];

    public static TheoryData<int> Findings => new(AcceptedFindings.Select(value => (int)value));

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library Inspect serializes every accepted finding code independently")]
    [MemberData(nameof(Findings))]
    public void FindingVocabulary(int value)
    {
        var expected = LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)value);
        var seed = LibraryInspectResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryInspectFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        using var document = JsonDocument.Parse(RenderJson(result));
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray())
            .GetProperty(CliJsonFields.Code).GetString());

        Assert.Equal(expected, LibraryInspectWording.MachineCode((LibraryInspectFindingCode)value));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect finding inventory is closed and rejects undefined runtime values")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(AcceptedFindings.Order(), Enum.GetValues<LibraryInspectFindingCode>().Order());
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectDefinitions.ReadFindingCode((LibraryInspectFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryInspectWording.MachineCode((LibraryInspectFindingCode)int.MaxValue));
    }

    private static string RenderJson(LibraryInspectResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryInspectResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryInspectPresentation.Rendering).PrimaryContent;
}
