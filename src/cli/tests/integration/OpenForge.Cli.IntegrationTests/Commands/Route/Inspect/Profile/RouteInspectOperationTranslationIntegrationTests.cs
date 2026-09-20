using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Shared.Profile;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect.Profile;

public sealed class RouteInspectOperationTranslationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect translates a Loader subject to a typed invalid result without forming a profile")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task LoaderSubjectIsInvalidWithoutProfile()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader([]);
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            ".agents/loader.md",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(RouteInspectReferenceKind.SourcePath, result.Selection.ReferenceKind);
        Assert.Equal(RouteInspectSelectionMethod.Unresolved, result.Selection.SelectionMethod);
        Assert.Equal(".agents/loader.md", result.Selection.RequestedReference);
        Assert.Empty(result.Selection.CandidatePaths);
        Assert.Null(result.Identity);
        Assert.Null(result.Profile);
        Assert.Empty(result.Observations);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.LoaderSubject, condition.Code);
        Assert.Equal(CliSemanticStatus.Invalid, condition.Status);
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.LoaderSubject);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route inspect translates an unresolved ambiguous identity to a typed blocked result without a profile")]
    [Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task AmbiguousIdentityIsBlockedWithoutProfile()
    {
        using var workspace = RouteInspectProfileIntegrationWorkspace.Create();
        workspace.WriteLoader(
            [RouteInspectProfileIntegrationWorkspace.Entry("Root", "root/_root.md", "Root")]);
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/_root.md",
                Description = "Root",
                Tags = ["Root"],
            });
        workspace.WriteRoutedMarkdown(
            ".agents/root/collision.md",
            "Collision leaf",
            ["Route"],
            "# Collision leaf\n\nA collision candidate.\n");
        workspace.WriteEntrypoint(
            new RouteInspectProfileIntegrationEntrypoint
            {
                RelativePath = ".agents/root/collision/_collision.md",
                Description = "Collision entrypoint",
                Tags = ["Route"],
            });
        var before = workspace.Snapshot();

        var result = await workspace.InspectAsync(
            "root/collision",
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInspectReferenceKind.SourceId, result.Selection.ReferenceKind);
        Assert.Equal(RouteInspectSelectionMethod.Unresolved, result.Selection.SelectionMethod);
        Assert.Equal("root/collision", result.Selection.RequestedReference);
        Assert.Equal(
            [
                ".agents/root/collision.md",
                ".agents/root/collision/_collision.md",
            ],
            result.Selection.CandidatePaths);
        Assert.Null(result.Identity);
        Assert.Null(result.Profile);
        Assert.Empty(result.Observations);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(RouteInspectConditionCode.AmbiguousSource, condition.Code);
        Assert.Equal(CliSemanticStatus.Blocked, condition.Status);
        Assert.Equal(result.Selection.CandidatePaths, condition.Paths);
        RouteInspectProfileIntegrationAssertions.AssertSemanticConditionAndNext(
            result,
            RouteInspectConditionCode.AmbiguousSource);
        RouteInspectProfileIntegrationAssertions.AssertNoWriteOrInspectionState(before, workspace.Snapshot());
    }
}
