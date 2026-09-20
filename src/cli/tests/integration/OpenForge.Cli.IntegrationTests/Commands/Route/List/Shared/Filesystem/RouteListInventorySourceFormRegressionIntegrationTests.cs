using OpenForge.Cli.Core.Commands.Route.List;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List.Shared.Filesystem;

public sealed class RouteListInventorySourceFormRegressionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route-list inventory retains accepted findings for root and suffix-only source forms"),
        InlineData(".agents/SKILL.md", "---\nname: root-skill\ndescription: Root skill\n---\n"),
        InlineData(".agents/index.md", "---\nopen-forge:\n  description: Root index\n  tags: [Route]\n---\n"),
        InlineData(".agents/.overwrite.md", "orphan overwrite")]
    [Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EdgeSourceFormsRetainOneAuthoredFormFinding(string path, string contents)
    {
        using var workspace = RouteListFilesystemIntegrationWorkspace.Create();
        workspace.Write(path, contents);
        var before = workspace.SnapshotHashes();

        var facts = await workspace.ReadAsync(TestContext.Current.CancellationToken);

        Assert.Empty(facts.Sources);
        var finding = Assert.Single(facts.Findings);
        Assert.Equal(RouteListFindingCode.AuthoredForm, finding.Code);
        Assert.Equal(path, finding.CanonicalLogicalSubject);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
