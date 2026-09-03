using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMoveRequestAndResultContractTests
{
    [Fact(DisplayName = "Route Move request retains one exact source destination workspace and mode")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void RequestRetainsCompleteTypedInput()
    {
        var request = RouteMoveTestData.Request(RouteMoveMode.DryRun);

        Assert.Equal(RouteMoveTestData.SourceId, request.SourceReference);
        Assert.Equal(RouteMoveTestData.DestinationPath, request.DestinationTarget);
        Assert.Equal(RouteMoveMode.DryRun, request.Mode);
        Assert.True(request.IsDryRun);
        Assert.Equal(RouteMoveTestData.Workspace(), request.Workspace);
    }

    [Fact(DisplayName = "Route Move request rejects absent and undefined required input")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void RequestRejectsInvalidRequiredInput()
    {
        var workspace = RouteMoveTestData.Workspace();

        Assert.Throws<ArgumentException>(() => new RouteMoveRequest(
            workspace,
            " ",
            RouteMoveTestData.DestinationPath,
            RouteMoveMode.Apply));
        Assert.Throws<ArgumentException>(() => new RouteMoveRequest(
            workspace,
            RouteMoveTestData.SourceId,
            " ",
            RouteMoveMode.Apply));
        Assert.Throws<ArgumentOutOfRangeException>(() => new RouteMoveRequest(
            workspace,
            RouteMoveTestData.SourceId,
            RouteMoveTestData.DestinationPath,
            (RouteMoveMode)int.MaxValue));
    }

    [Theory(DisplayName = "Route Move path state accepts only exact missing file and directory facts"),
        InlineData((int)RouteMovePathStateKind.Missing, null),
        InlineData((int)RouteMovePathStateKind.Directory, null),
        InlineData((int)RouteMovePathStateKind.File, "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void PathStateRetainsExactTypedFacts(int kindValue, string? fingerprint)
    {
        var state = new RouteMovePathState((RouteMovePathStateKind)kindValue, fingerprint);

        Assert.Equal((RouteMovePathStateKind)kindValue, state.Kind);
        Assert.Equal(fingerprint, state.ContentSha256);
    }

    [Fact(DisplayName = "Route Move result requires initialized ordered unique semantic collections")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void ResultRejectsDuplicateSemanticPaths()
    {
        var formation = RouteMoveTestData.Formation() with
        {
            UnchangedPaths = [RouteMoveTestData.SourcePath, RouteMoveTestData.SourcePath],
        };

        Assert.Throws<ArgumentException>(() => new RouteMoveResult(
            formation,
            OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Complete,
            next: null));
    }

    [Fact(DisplayName = "Route Move leaf subjects keep complete inventory out of category-only items")]
    [Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void LeafSubjectCarriesLayersWithoutCategoryItems()
    {
        var formation = RouteMoveTestData.Formation();

        Assert.Equal(RouteMoveSubjectKind.Leaf, formation.Subject.Kind);
        Assert.Single(formation.Subject.Layers);
        Assert.Empty(formation.Subject.Items);
    }
}
