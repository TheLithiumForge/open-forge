using OpenForge.Cli.Core.Framework.Sources.Loading;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.List;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

[Trait("Boundary", "Host"), Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
public sealed class RouteInspectOutsideIntegrationTests
{
    public static IEnumerable<object[]> CompatibilityAnchors()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteListOutsideWorkspace.CustomRoot })
            foreach (var name in new[] { SourceFormClassifier.IndexFileName, SourceFormClassifier.UnderscoreIndexFileName,
                SourceFormClassifier.ReferencesFileName, SourceFormClassifier.UnderscoreReferencesFileName }) yield return [root, name];
    }

    [Theory, MemberData(nameof(CompatibilityAnchors))]
    public async Task EachCompatibilityAnchorRetainsItsActualFilename(string root, string name)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false, compatibilityName: name);
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, false, ["src/Planned.cs"]), TestContext.Current.CancellationToken);
        Assert.Equal(RouteInspectSourceForm.CompatibilityEntrypoint, result.Identity!.Form);
        Assert.Equal(RouteInspectRouteState.Detached, result.Identity.RouteState);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, result.Identity.Id);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, result.Identity.MaintenanceRoot);
        Assert.False(File.Exists(fixture.Inner.Combine(fixture.CanonicalAnchor)));
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task FirstReachingRowsControlInheritedLoadingAcrossStructuralParentsAndAliases(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root);
        const string nested = RouteListOutsideWorkspace.DirectoryPath + "/nested/_nested.md";
        const string child = RouteListOutsideWorkspace.DirectoryPath + "/nested/child.md";
        const string alias = RouteListOutsideWorkspace.DirectoryPath + "/alias.md";
        fixture.Inner.WriteText(nested, RouteListOutsideWorkspace.Document("Structural parent",
            $"- [Later child](child.md) - #{SourceLoadingClosureResolver.KeepInMindTag} - applies to `tests/**`\n"));
        fixture.Inner.WriteText(child, "# Independent nested child\n");
        fixture.Inner.CreateFileSymbolicLink(alias, fixture.Inner.Combine(child));
        fixture.Inner.ReplaceText(fixture.Anchor, RouteListOutsideWorkspace.Document("First reaching parent",
            $"- [Nested parent](nested/_nested.md) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `tests/**`\n"
            + $"- [First direct child](nested/child.md) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `src/**`\n"
            + "- [Alias child](alias.md) - #Private - applies to `docs/**`\n"));
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + child, false, ["src/Planned.cs"]), TestContext.Current.CancellationToken);
        Assert.Equal(RouteInspectApplicabilityState.Matched, result.Applicability!.State);
        Assert.Equal([child], result.Applicability.Conditions.Select(condition => condition.Source));
        Assert.True(result.Profile!.Reading.TaskStart.ReadValue());
        var resolution = await new OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution.RouteInspectResolver()
            .ResolveAsync(new RouteInspectRequest(fixture.Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + child, false),
                TestContext.Current.CancellationToken);
        var catalogue = resolution.Graph!.Catalogue!;
        Assert.Equal(fixture.Anchor, catalogue.FindByPath(child)!.IncludedParentPath);
        Assert.Equal(child, catalogue.Aliases[alias]);
        Assert.Contains(catalogue.IncludedRows, row => row.Path == alias && row.AliasOf == child && row.Entry.Tags.Contains("Private"));
        Assert.Contains(catalogue.IncludedRows, row => row.Path == child && row.ParentPath == nested && row.Entry.Description == "Later child");
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    public static IEnumerable<object[]> Conditions()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteListOutsideWorkspace.CustomRoot })
            foreach (var state in new[] { RouteInspectApplicabilityState.Pending, RouteInspectApplicabilityState.Matched,
                RouteInspectApplicabilityState.Unmatched }) yield return [root, (int)state];
    }

    [Theory, MemberData(nameof(Conditions))]
    public async Task RetainedRowConditionsControlLoadingWithoutChangingAuthoredMetadata(string root, int expectedValue)
    {
        var expected = (RouteInspectApplicabilityState)expectedValue;
        using var fixture = new RouteListOutsideWorkspace(root);
        var before = fixture.Inner.SnapshotHashes();
        string[]? paths = expected switch
        {
            RouteInspectApplicabilityState.Pending => null,
            RouteInspectApplicabilityState.Matched => ["src/Planned.cs"],
            _ => ["tests/Planned.cs"],
        };
        var result = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + RouteListOutsideWorkspace.Child, false, paths), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Identity);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, result.Identity.MaintenanceRoot);
        Assert.Equal(new[] { "Body" }, result.Identity.Tags);
        Assert.NotNull(result.Applicability);
        Assert.Equal(expected, result.Applicability.State);
        var condition = Assert.Single(result.Applicability.Conditions);
        Assert.Equal(RouteListOutsideWorkspace.Child, condition.Source);
        Assert.Equal(["src/**"], condition.Patterns);
        Assert.NotNull(result.Profile);
        if (expected == RouteInspectApplicabilityState.Pending)
        {
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(RouteInspectFactState.Unavailable, result.Profile.Reading.TaskStart.State);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(expected == RouteInspectApplicabilityState.Matched, result.Profile.Reading.TaskStart.ReadValue());
        }
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task PlainLinkedMarkdownKeepsRowKeepInMindWithoutFileMetadata(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root);
        fixture.Inner.ReplaceText(fixture.Anchor, RouteListOutsideWorkspace.Document("Authored parent",
            $"- [Standing outside file](SKILL.md) - #{SourceLoadingClosureResolver.KeepInMindTag}\n"));
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + RouteListOutsideWorkspace.Skill, false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteInspectSourceKind.Markdown, result.Identity!.Kind);
        Assert.Empty(result.Identity.Tags);
        Assert.True(result.Profile!.Reading.TaskStart.ReadValue());
        Assert.True(result.Profile.Reading.Later.ReadValue().MayBeReadAgain);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(RouteListOutsideWorkspace.CustomRoot, true)]
    public async Task UnlistedCompatibilityAndOutsideSkillKeepTheirActualIdentity(string root, bool compatibility)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false, compatibility: compatibility);
        var before = fixture.Inner.SnapshotHashes();
        foreach (var path in new[] { fixture.Anchor, RouteListOutsideWorkspace.Overwrite, RouteListOutsideWorkspace.Skill })
        {
            var result = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
                SourceWorkspaceRelativePath.LiteralReferencePrefix + path, false, ["src/Planned.cs"]), TestContext.Current.CancellationToken);
            Assert.NotNull(result.Identity);
            var expected = path == RouteListOutsideWorkspace.Overwrite ? RouteListOutsideWorkspace.Child : path;
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + expected, result.Identity.Id);
            Assert.Equal(expected, result.Identity.CanonicalWorkspaceRelativePath);
            Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, result.Identity.MaintenanceRoot);
            if (path == RouteListOutsideWorkspace.Skill) Assert.Equal(RouteInspectSourceKind.Markdown, result.Identity.Kind);
            if (path == RouteListOutsideWorkspace.Overwrite) Assert.Equal(2, result.Identity.PhysicalLayers.Count);
        }
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task JsonCoordinateIsDeclarationOnlyAtEveryDetail(bool registered)
    {
        using var fixture = new RouteListOutsideWorkspace(registered: registered, fileIncluded: true);
        var before = fixture.Inner.SnapshotHashes();
        foreach (var detail in new[] { CliPresentationDefinitions.Minimal, CliPresentationDefinitions.Standard,
            CliPresentationDefinitions.Full, CliPresentationDefinitions.Debug })
        {
            var run = await CliHostCapture.RunAsync([RouteDefinitions.RouteGroup.Name, RouteInspectDefinitions.InspectCommand.Name,
                SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor,
                CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat,
                CliSyntaxDefinitions.Detail.Name, detail], fixture.Inner.Path);
            using var report = JsonDocument.Parse(run.Output);
            var data = report.RootElement.GetProperty(CliJsonFields.Data);
            var present = data.TryGetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(RouteInspectData.MaintenanceRoot)), out var coordinate);
            Assert.Equal(registered, present);
            if (present) Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, coordinate.GetString());
        }
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }
}
