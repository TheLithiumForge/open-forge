using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Framework;
using OpenForge.Cli.IntegrationTests.Commands.Route.Init.Generic;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

[Trait("Feature", "route-init"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitFrontmatterIntegrationTests
{
    [Theory(DisplayName = "Route Init writes generic scaffolds in the effective workspace form")]
    [InlineData("root", true), InlineData("scoped", false), InlineData(null, false)]
    public async Task ScaffoldUsesWorkspaceForm(string? setting, bool root)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-scaffold-form");
        if (setting is not null)
        {
            workspace.WriteText(".agents/open-forge.json", $"{{\"frontmatter\":\"{setting}\"}}");
        }

        var result = await RouteInitOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(
            workspace.Request("documents/design"), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        foreach (var path in new[] { ".agents/documents/_documents.md", ".agents/documents/design/_design.md" })
        {
            var facts = ReadMetadata(Encoding.UTF8.GetBytes(workspace.ReadText(path)));
            Assert.Equal(root ? FrontmatterForm.Root : FrontmatterForm.Scoped, facts.Syntax.AuthoredForm);
            Assert.Equal(["NeedsAuthoring"], facts.Metadata?.Tags);
        }
    }

    [Fact(DisplayName = "Route Init renders embedded entrypoints and restoration documents in root form")]
    public async Task EmbeddedEntrypointsAndRestorationRenderInRootForm()
    {
        using var workspace = await RouteInitFrameworkIntegrationWorkspace.CreateTrustedAsync(
            "route-init-root-payload", TestContext.Current.CancellationToken);
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"root\"}");
        var sparse = await PlanAsync(workspace.Request("memory/team/working"));
        var copied = Assert.Single(sparse.FileChanges.Where(change => change.LogicalPath == workspace.Combine(".agents/memory/team/working/_working.md")));
        Assert.Equal(FrontmatterForm.Root, ReadMetadata(copied.IntendedBytes.AsSpan()).Syntax.AuthoredForm);
        var scope = Assert.Single(sparse.FileChanges.Where(change => change.LogicalPath == workspace.Combine(".agents/memory/team/_team.md")));
        Assert.Equal(FrontmatterForm.Root, ReadMetadata(scope.IntendedBytes.AsSpan()).Syntax.AuthoredForm);

        workspace.Delete(".agents/skills");
        var restoration = await PlanAsync(workspace.Request("skills"));
        var payload = Assert.IsType<OpenForge.Cli.Core.Framework.Distribution.Models.FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var renderedCount = 0;
        foreach (var change in restoration.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Create))
        {
            var path = Path.GetRelativePath(workspace.PhysicalPath, change.LogicalPath).Replace('\\', '/');
            var asset = payload.Find(path);
            if (asset is null || !path.EndsWith(".md", StringComparison.Ordinal) || path.EndsWith("/SKILL.md", StringComparison.Ordinal))
            {
                continue;
            }
            var source = ReadMetadata(asset.Bytes.AsSpan());
            if (source.Syntax.AuthoredForm != FrontmatterForm.Scoped)
            {
                continue;
            }

            Assert.Equal(FrontmatterForm.Root, ReadMetadata(change.IntendedBytes.AsSpan()).Syntax.AuthoredForm);
            renderedCount++;
            if (restoration.Restoration?.Files.SingleOrDefault(file => file.Asset.Path == path) is { } ordinary)
            {
                Assert.Equal(Body(asset.Bytes.AsSpan()), Body(change.IntendedBytes.AsSpan()));
                Assert.Equal(ordinary.IntendedBytes.ToArray(), change.IntendedBytes.ToArray());
            }
        }
        Assert.True(renderedCount > 1);
    }

    [Fact(DisplayName = "Route Init preserves existing metadata authored in the other form")]
    public async Task ExistingOtherFormIsPreserved()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-preserve-authored-form");
        workspace.WriteText(".agents/open-forge.json", "{}");
        var operation = RouteInitOperationFactory.Create(workspace.LockStoreRoot);
        var initial = await operation.ExecuteAsync(workspace.Request("documents"), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, initial.Status);
        const string existingPath = ".agents/documents/_documents.md";
        var before = workspace.ReadText(existingPath);
        File.WriteAllText(workspace.Absolute(".agents/open-forge.json"), "{\"frontmatter\":\"root\"}");

        var plan = await PlanAsync(workspace.Request("documents/design"));
        var parent = Assert.Single(plan.FileChanges.Where(change => change.LogicalPath == workspace.Absolute(existingPath)));
        Assert.Equal(FrontmatterForm.Scoped, ReadMetadata(parent.IntendedBytes.AsSpan()).Syntax.AuthoredForm);
        var beforeDocument = new MarkdownDocumentParser().Parse(before);
        var afterDocument = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(parent.IntendedBytes.AsSpan()));
        var beforeSpan = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(beforeDocument.Frontmatter.BlockSpan);
        var afterSpan = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(afterDocument.Frontmatter.BlockSpan);
        Assert.Equal(before[..beforeSpan.End], afterDocument.Source[..afterSpan.End]);
        Assert.Equal(before, workspace.ReadText(existingPath));
    }

    [Theory(DisplayName = "Route Init invalidates plans after a preference or settings observation change")]
    [InlineData("{\"frontmatter\":\"root\"}"), InlineData("{\"frontmatter\":\"scoped\"}")]
    public async Task SettingsChangeInvalidatesPlan(string changedSettings)
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-settings-revalidation");
        var builder = new RouteInitPlanBuilder();
        var plan = Assert.IsType<RouteInitPlan>((await builder.BuildAsync(workspace.Request("documents"), TestContext.Current.CancellationToken)).Plan);
        workspace.WriteText(".agents/open-forge.json", changedSettings);
        var before = workspace.SnapshotHashes();

        var result = await new RouteInitPlanRevalidator(builder).RevalidateAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(RouteInitPlanRevalidationState.Changed, result.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Init rejects invalid workspace settings before planning effects")]
    public async Task InvalidSettingsStopPlanning()
    {
        using var workspace = GenericRouteInitIntegrationWorkspace.Create("route-init-invalid-settings");
        workspace.WriteText(".agents/open-forge.json", "{\"frontmatter\":\"unknown\"}");
        var before = workspace.SnapshotHashes();

        var build = await new RouteInitPlanBuilder().BuildAsync(workspace.Request("documents"), TestContext.Current.CancellationToken);

        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteInitFindingCode.InvalidInput);
        Assert.Empty(build.Formation.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static async Task<RouteInitPlan> PlanAsync(RouteInitRequest request)
        => Assert.IsType<RouteInitPlan>((await new RouteInitPlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken)).Plan);

    private static FrameworkDocumentMetadataFacts ReadMetadata(ReadOnlySpan<byte> bytes)
    {
        var facts = new FrameworkDocumentMetadataParser().Parse(
            new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes)), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(FrameworkDocumentMetadataState.Complete, facts.State);
        return facts;
    }

    private static byte[] Body(ReadOnlySpan<byte> bytes)
    {
        var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(bytes));
        var span = Assert.IsType<OpenForge.Cli.Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(document.BodySpan);
        return Encoding.UTF8.GetBytes(document.Source.Substring(span.Start, span.Length));
    }
}
