using OpenForge.Cli.Core.Presentation.Shared.Rendering;
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
    internal static readonly LibraryListFindingCode[] AcceptedFindings =
    [
        LibraryListFindingCode.InvalidInput,
        LibraryListFindingCode.InvalidRecord,
        LibraryListFindingCode.RecordUnavailable,
        LibraryListFindingCode.RecordBlocked,
        LibraryListFindingCode.SourceRootInvalid,
        LibraryListFindingCode.SourceRootUnavailable,
        LibraryListFindingCode.SourceRootBlocked,
        LibraryListFindingCode.LinkMissing,
        LibraryListFindingCode.LinkChanged,
        LibraryListFindingCode.LinkUnavailable,
        LibraryListFindingCode.LinkBlocked,
        LibraryListFindingCode.OperationFailed,
        LibraryListFindingCode.Interrupted,
        LibraryListFindingCode.OwnershipObservation,
        LibraryListFindingCode.LayoutUnavailable,
    ];

    public static TheoryData<int> Findings => new(AcceptedFindings.Select(value => (int)value));

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library List serializes every accepted finding code independently")]
    [MemberData(nameof(Findings))]
    public void FindingVocabulary(int value)
    {
        var expected = LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)value);
        var seed = LibraryListResultFixture.Create();
        var finding = seed.Result.Findings[0] with { Code = (LibraryListFindingCode)value };
        var result = seed with { Result = seed.Result with { Findings = [finding] } };
        using var document = JsonDocument.Parse(RenderJson(result));
        Assert.Equal(expected, Assert.Single(document.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray())
            .GetProperty(CliJsonFields.Code).GetString());

        Assert.Equal(expected, LibraryListWording.MachineCode((LibraryListFindingCode)value));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List finding inventory is closed and rejects undefined runtime values")]
    public void FindingInventoryIsClosed()
    {
        Assert.Equal(AcceptedFindings.Order(), Enum.GetValues<LibraryListFindingCode>().Order());
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListDefinitions.ReadFindingCode((LibraryListFindingCode)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => LibraryListWording.MachineCode((LibraryListFindingCode)int.MaxValue));
    }

    private static string RenderJson(LibraryListResult result)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Json, CliDetail.Standard, null)),
            LibraryListPresentation.Rendering).PrimaryContent;
}
