using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Create;

public sealed class ExtensionCreatePlanningTests
{
    [Theory(DisplayName = "Extension Create destination failures map every terminal state explicitly"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateDestinationState.Unavailable, CliSemanticStatus.Incomplete, ExtensionCreateFindingCode.CatalogueUnavailable)]
    [InlineData(ExtensionCreateDestinationState.Unsafe, CliSemanticStatus.Blocked, ExtensionCreateFindingCode.CatalogueUnsafe)]
    [InlineData(ExtensionCreateDestinationState.Collision, CliSemanticStatus.Blocked, ExtensionCreateFindingCode.DestinationCollision)]
    public void DestinationFailuresMapTerminalStatesExplicitly(
        object stateValue,
        object statusValue,
        object codeValue)
    {
        var state = Assert.IsType<ExtensionCreateDestinationState>(stateValue);
        var expectedStatus = Assert.IsType<CliSemanticStatus>(statusValue);
        var expectedCode = Assert.IsType<ExtensionCreateFindingCode>(codeValue);
        var outcome = ExtensionCreatePlanner.DestinationFailure(
            ResolvedRequest(),
            "/catalogue",
            "/catalogue/development-toolkit",
            new ExtensionCreateDestinationObservation(state, physicalIdentity: null, cause: "observed cause"));

        var result = Assert.IsType<ExtensionCreateResult>(outcome.Result);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == expectedCode);
    }

    [Theory(DisplayName = "Extension Create refuses nonfailure destination states at failure formation"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    [InlineData(ExtensionCreateDestinationState.Absent)]
    [InlineData(ExtensionCreateDestinationState.Exact)]
    public void DestinationFailureRejectsNonfailureStates(object stateValue)
    {
        var state = Assert.IsType<ExtensionCreateDestinationState>(stateValue);

        Assert.Throws<InvalidOperationException>(
            () => ExtensionCreatePlanner.DestinationFailure(
                ResolvedRequest(),
                "/catalogue",
                "/catalogue/development-toolkit",
                new ExtensionCreateDestinationObservation(state, physicalIdentity: null, cause: null)));
    }

    [Fact(DisplayName = "Extension Create rejects an undefined destination state at failure formation"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void DestinationFailureRejectsUndefinedState()
    {
        var undefined = (ExtensionCreateDestinationState)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionCreatePlanner.DestinationFailure(
                ResolvedRequest(),
                "/catalogue",
                "/catalogue/development-toolkit",
                new ExtensionCreateDestinationObservation(undefined, physicalIdentity: null, cause: null)));
    }

    [Fact(DisplayName = "Extension Create destination failure cases cover every named destination state"), Trait("Feature", "extension-create"), Trait("Evidence", "Unit")]
    public void DestinationFailureCasesCoverEveryNamedState()
    {
        ExtensionCreateDestinationState[] specified =
        [
            ExtensionCreateDestinationState.Absent,
            ExtensionCreateDestinationState.Exact,
            ExtensionCreateDestinationState.Collision,
            ExtensionCreateDestinationState.Unsafe,
            ExtensionCreateDestinationState.Unavailable,
        ];

        Assert.Equal(Enum.GetValues<ExtensionCreateDestinationState>(), specified);
    }

    private static ExtensionCreateResolvedRequest ResolvedRequest()
        => new()
        {
            StableId = "development-toolkit",
            CataloguePath = "/catalogue",
            Manifest = new ExtensionCreateManifest
            {
                Id = "development-toolkit",
                Name = "Development Toolkit",
                Description = "Open Forge Extension package development-toolkit.",
                Version = "0.1.0",
                Dependencies = [],
            },
            ManifestBytes = ReadOnlyMemory<byte>.Empty,
            Mode = ExtensionCreateMode.Apply,
        };
}
