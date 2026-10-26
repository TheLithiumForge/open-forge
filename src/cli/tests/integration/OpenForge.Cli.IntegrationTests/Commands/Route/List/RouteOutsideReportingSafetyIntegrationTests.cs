using OpenForge.Cli.Core.Commands.Route.Inspect;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Boundary", "OS"), Trait("Feature", "outside-reporting"), Trait("Evidence", "Integration")]
public sealed class RouteOutsideReportingSafetyIntegrationTests
{
    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task MissingAndUnsafeRegistrationsDoNotPoisonUnrelatedPrimarySelection(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false);
        using var foreign = TemporaryWorkspace.Create("outside-reporting-foreign");
        foreign.WriteText("secret.md", "Independent foreign bytes.\n");
        const string missing = "docs/missing";
        const string unsafePath = "docs/unsafe";
        fixture.Inner.CreateDirectorySymbolicLink(unsafePath, foreign.Path);
        fixture.Inner.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            fixture.Settings(routes: [RouteListOutsideWorkspace.DirectoryPath, missing, unsafePath]));
        var before = fixture.Inner.SnapshotHashes();
        var foreignBefore = foreign.SnapshotHashes();
        var primaryList = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace, "guide", RouteListDepth.All),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, primaryList.Status);
        Assert.Null(Assert.Single(primaryList.Rows).MaintenanceRoot);
        var primaryInspect = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace, "guide", false),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, primaryInspect.Status);
        Assert.Null(primaryInspect.Identity!.MaintenanceRoot);
        var unavailable = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + missing + "/_missing.md", RouteListDepth.All), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, unavailable.Status);
        Assert.Empty(unavailable.Rows);
        Assert.Contains(unavailable.Findings, finding => finding.Code == RouteListFindingCode.ReadUnavailable);
        var unsafeList = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + unsafePath + "/secret.md", RouteListDepth.All), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, unsafeList.Status);
        Assert.Empty(unsafeList.Rows);
        Assert.Contains(unsafeList.Findings, finding => finding.Code == RouteListFindingCode.PhysicalBoundary);
        var unsafeInspect = await new RouteInspectResolver().ResolveAsync(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + unsafePath + "/secret.md", false), TestContext.Current.CancellationToken);
        Assert.Equal(RouteInspectResolutionState.Blocked, unsafeInspect.State);
        Assert.Null(unsafeInspect.Identity);
        Assert.Contains(unsafeInspect.Issues, issue => issue.Code == RouteInspectResolutionIssueCode.UnsafeSource);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        Assert.Equal(foreignBefore, foreign.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task CompatibilityAnchorDoesNotAliasAStaleCanonicalLoaderRow(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false, compatibility: true);
        var destination = Path.GetRelativePath(fixture.Layout.MainRoot, fixture.CanonicalAnchor).Replace('\\', '/');
        fixture.Inner.ReplaceText(fixture.Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(
            $"- [Stale authored root]({destination}) - #LoadNow\n"));
        var before = fixture.Inner.SnapshotHashes();
        var result = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace, null, RouteListDepth.All),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        var missing = Assert.Single(result.Rows);
        Assert.True(missing.IsMissing);
        Assert.Equal(fixture.CanonicalAnchor, missing.Path);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, missing.MaintenanceRoot);
        Assert.DoesNotContain(result.Rows, row => row.Path == fixture.Anchor);
        var selected = await RouteListOperationFactory.Create()(new RouteListRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, RouteListDepth.All), TestContext.Current.CancellationToken);
        Assert.Contains(selected.Rows, row => row.Path == fixture.Anchor && !row.IsMissing);
        Assert.DoesNotContain(selected.Rows, row => row.Path == fixture.CanonicalAnchor);
        var inspect = await new RouteInspectResolver().ResolveAsync(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor, false), TestContext.Current.CancellationToken);
        Assert.NotNull(inspect.Identity);
        Assert.Equal(fixture.Anchor, inspect.Identity.CanonicalWorkspaceRelativePath);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, inspect.Identity.MaintenanceRoot);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteListOutsideWorkspace.CustomRoot)]
    public async Task OrdinalLookalikeAndUnreadableSourceNeverAcquireAnotherCoordinate(string root)
    {
        using var fixture = new RouteListOutsideWorkspace(root, listed: false);
        const string lookalike = "docs/teammate/ordinary.md";
        fixture.Inner.WriteText(lookalike, "Independent ordinary bytes.\n");
        fixture.Inner.ReplaceBytes(RouteListOutsideWorkspace.Child, [0xc3, 0x28]);
        var before = fixture.Inner.SnapshotHashes();
        var selected = await RouteInspectOperationFactory.Create()(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + lookalike, false), TestContext.Current.CancellationToken);
        Assert.Null(selected.Identity?.MaintenanceRoot);
        var unreadable = await new RouteInspectResolver().ResolveAsync(new RouteInspectRequest(fixture.Workspace,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + RouteListOutsideWorkspace.Child, false), TestContext.Current.CancellationToken);
        Assert.Equal(RouteInspectResolutionState.Incomplete, unreadable.State);
        Assert.Equal(RouteListOutsideWorkspace.DirectoryPath, unreadable.Identity!.MaintenanceRoot);
        Assert.Contains(unreadable.Issues, issue => issue.Code == RouteInspectResolutionIssueCode.ReadUnavailable);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }
}
