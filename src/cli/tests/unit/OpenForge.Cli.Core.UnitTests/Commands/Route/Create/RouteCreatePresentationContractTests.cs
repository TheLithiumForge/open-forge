using OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Create;

public sealed class RouteCreatePresentationContractTests
{
    [Fact(DisplayName = "Route Create JSON projection preserves the accepted typed envelope"), Trait("Feature", "route-create"), Trait("Evidence", "UnitContract")]
    public void JsonProjectionPreservesAcceptedTypedEnvelope()
    {
        var projected = RouteCreateJsonProjection.Create(RouteCreateTestData.Result());

        Assert.Equal(1, projected.SchemaVersion);
        Assert.Equal("route create", projected.Command);
        Assert.Equal("complete", projected.Status);
        Assert.Equal(RouteCreateTestData.Workspace().LexicalRoot, projected.Workspace?.Path);
        Assert.Equal("explicit-workspace", projected.Workspace?.SelectedBy);
        Assert.Equal("apply", projected.Result.Mode);
        Assert.Equal(RouteCreateTestData.TargetId, projected.Result.Target.Requested);
        Assert.Equal(RouteCreateTestData.TargetId, projected.Result.Target.Id);
        Assert.Equal(RouteCreateTestData.TargetPath, projected.Result.Target.Path);
        Assert.Equal(RouteCreateTestData.ParentId, projected.Result.Parent?.Id);
        Assert.Equal(RouteCreateTestData.ParentPath, projected.Result.Parent?.Path);
        Assert.Equal("canonical", projected.Result.Parent?.Form);
        Assert.Equal("Project overview", projected.Result.Metadata.Description);
        Assert.Equal("Explains the project", projected.Result.Metadata.Responsibility);
        Assert.Equal(["Docs", "Overview"], projected.Result.Metadata.Tags);
        Assert.Null(projected.Result.Template);
        Assert.Equal("complete", projected.Result.Plan.Completeness);
        Assert.Equal("safe", projected.Result.Plan.Safety);
        var effect = Assert.Single(projected.Result.Effects);
        Assert.Equal(RouteCreateTestData.TargetPath, effect.Path);
        Assert.Equal("routed-file", effect.Kind);
        Assert.Equal("create", effect.Action);
        Assert.Equal("verified", effect.Outcome);
        Assert.Equal("none", effect.Residual);
        Assert.Equal([".agents/loader.md"], projected.Result.UnchangedPaths);
        Assert.Equal("not-required", projected.Result.Recovery.State);
        Assert.Null(projected.Result.Recovery.ResidualPath);
        Assert.Equal("verified", projected.Result.Verification);
        Assert.Empty(projected.Result.Findings);
        Assert.Null(projected.Next);
    }
}
