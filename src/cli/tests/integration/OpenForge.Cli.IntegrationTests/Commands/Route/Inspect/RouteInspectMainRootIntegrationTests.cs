using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Inspect;

public sealed class RouteInspectMainRootIntegrationTests
{
    private const string MainRoot = "manual/context";
    private const string Route = "review";
    private const string RouteFile = "_" + Route + SourceFormClassifier.MarkdownExtension;
    private const string RoutePath = MainRoot + "/" + Route + "/" + RouteFile;
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string InvalidJson = "{";
    private const string RouteBody = $"# Review\n\n## Axioms\n\n- Keep the source.\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n{MarkdownEntriesSectionReader.EmptyEntry}\n";

    [Fact(DisplayName = "Route Inspect retains the selected layout and identities when ownership is unavailable"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task RetainsLayoutAndLoaderFactsWithUnreadableOwnership()
    {
        using var workspace = CreateWorkspace();
        workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath, InvalidJson);
        var resolution = await ResolveAsync(workspace, Route);

        Assert.Equal(RouteInspectResolutionState.Resolved, resolution.State);
        Assert.Equal(MainRoot, resolution.ReadGraph().Layout.MainRoot);
        Assert.Equal(Route, resolution.ReadIdentity().Id);
        Assert.Equal(RoutePath, resolution.ReadIdentity().CanonicalWorkspaceRelativePath);
        var profile = new RouteInspectProfileBuilder().Build(resolution, TestContext.Current.CancellationToken);
        Assert.True(profile.Reading.TaskStart.ReadValue());
        var reading = Assert.Single(profile.Reading.Automatic.ReadValue().Reasons);
        Assert.True(reading.RelatedSourceIsLoader);
        var loaderId = Path.GetFileNameWithoutExtension(WorkspaceLayoutNames.LoaderFileName);
        Assert.Equal(loaderId, reading.RelatedSourceId);
        Assert.Equal(new[] { loaderId }, profile.Axioms.ReadValue().Inherited.ReadValue().SourceIds);
    }

    [Fact(DisplayName = "Route Inspect never falls back to the default root when settings are invalid"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task InvalidSettingsHaveNoFallbackLayout()
    {
        using var workspace = CreateWorkspace();
        workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, InvalidJson);
        workspace.WriteText(SourceLogicalPath.LoaderPath, GeneratedLoaderDocumentBuilder.Build($"- [Review]({Route}/{RouteFile})"));
        workspace.WriteText(WorkspaceLayout.DefaultMainRoot + "/" + Route + "/" + RouteFile, OpenForgeDocumentSeed.MetadataFrontmatter(Route, [SourceLoadingClosureResolver.LoadNowTag]) + "\n" + RouteBody);
        var resolution = await ResolveAsync(workspace, Route);

        Assert.Equal(RouteInspectResolutionState.Blocked, resolution.State);
        Assert.Null(resolution.Graph);
        Assert.Null(resolution.Identity);
        var issue = Assert.Single(resolution.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.UnsafeSource, issue.Code);
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, issue.Subject);
    }

    [Fact(DisplayName = "Route Inspect keeps an orphan overwrite's related custom-root identity"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task OrphanOverwriteKeepsRelatedIdentity()
    {
        using var workspace = CreateWorkspace();
        var orphanPath = MainRoot + "/" + SourceOverwritePath.ReadAdjacentPath(Route + SourceFormClassifier.MarkdownExtension);
        workspace.WriteText(orphanPath, "# Orphan\n");
        var resolution = await ResolveAsync(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + orphanPath);

        Assert.Equal(RouteInspectResolutionState.Blocked, resolution.State);
        var issue = Assert.Single(resolution.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.OrphanOverwrite, issue.Code);
        Assert.Contains(RoutePath, issue.Paths);
    }

    [Fact(DisplayName = "Route Inspect keeps literal default-root operands literal in a custom layout"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Integration")]
    public async Task DefaultRootOperandIsNotRedirected()
    {
        using var workspace = CreateWorkspace();
        var path = WorkspaceLayout.DefaultMainRoot + "/" + Route + "/" + RouteFile;
        var resolution = await ResolveAsync(workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path);

        Assert.Equal(RouteInspectResolutionState.Invalid, resolution.State);
        var issue = Assert.Single(resolution.Issues);
        Assert.Equal(RouteInspectResolutionIssueCode.MissingSource, issue.Code);
        Assert.Equal(path, issue.Subject);
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create(nameof(RouteInspectMainRootIntegrationTests));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(LoaderPath, $"# Context\n\n## Axioms\n\n- Preserve evidence.\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n- [Review]({Route}/{RouteFile}) - #{SourceLoadingClosureResolver.LoadNowTag}\n");
        workspace.WriteText(RoutePath, OpenForgeDocumentSeed.MetadataFrontmatter(Route, [SourceLoadingClosureResolver.LoadNowTag]) + "\n" + RouteBody);
        return workspace;
    }

    private static ValueTask<RouteInspectResolution> ResolveAsync(TemporaryWorkspace workspace, string reference)
        => new RouteInspectResolver().ResolveAsync(
            new RouteInspectRequest(new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory), reference, false),
            TestContext.Current.CancellationToken);
}
