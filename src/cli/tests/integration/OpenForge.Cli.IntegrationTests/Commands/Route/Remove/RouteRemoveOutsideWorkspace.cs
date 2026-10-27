using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

internal sealed class RouteRemoveOutsideWorkspace : IDisposable
{
    internal const string DirectoryPath = "docs/team";
    internal const string CustomRoot = "knowledge/main";
    internal const string Leaf = DirectoryPath + "/old.md";
    internal const string Overwrite = DirectoryPath + "/old.overwrite.md";
    internal const string Survivor = DirectoryPath + "/keep.md";
    internal const string Category = DirectoryPath + "/topics";
    internal const string Child = Category + "/child.md";
    internal const string Resource = Category + "/assets/data.bin";
    internal const string RemovedRow = $"- [Independent old label](old.md) - #{SourceLoadingClosureResolver.LoadNowTag} #Authored - applies to `old/**`\r\n";
    internal const string SurvivingRows = "- [Keep one](keep.md) - #Keep #First - applies to `src/**`\r\n- [Keep two](keep.md) - #Second - applies to `test/**`\r\n";
    internal RouteRemoveIntegrationWorkspace Inner { get; } = RouteRemoveIntegrationWorkspace.Create("route-remove-outside");
    internal WorkspaceLayout Layout { get; }
    internal string Anchor { get; }

    internal RouteRemoveOutsideWorkspace(string root = WorkspaceLayout.DefaultMainRoot, bool listed = true, bool compatibility = false)
    {
        Layout = WorkspaceLayout.Create(Inner.Workspace, root).WithOutsideRoutes([DirectoryPath]);
        Anchor = compatibility ? DirectoryPath + "/index.md" : WorkspaceRoutePath.CanonicalEntrypointPath(DirectoryPath);
        if (root != WorkspaceLayout.DefaultMainRoot) Inner.DeleteFile(RouteRemoveIntegrationWorkspace.LoaderPath);
        Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings());
        var destination = Path.GetRelativePath(root, WorkspaceRoutePath.CanonicalEntrypointPath(DirectoryPath)).Replace('\\', '/');
        Inner.WriteText(Layout.LoaderPath, Entry(listed ? $"- [Outside authored]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag} #Policy\r\n" : MarkdownEntriesSectionReader.EmptyEntry));
        Inner.WriteText(Anchor, Entry(RemovedRow + SurvivingRows));
        Inner.WriteText(Leaf, OpenForgeDocumentSeed.Metadata("Old", ["Fixture"], "# Old\r\n\r\nExact body Ï€\r\n"));
        Inner.WriteText(Overwrite, "Independent overwrite\r\n");
        Inner.WriteText(Survivor, OpenForgeDocumentSeed.Metadata("Keep", ["Fixture"], "# Keep\r\n\r\nKeep body unchanged.\r\n"));
    }

    internal string Settings(string? extra = null, IEnumerable<string>? routes = null)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", (routes ?? [DirectoryPath]).Select(path => $"\"{path}\""))}}],"independent":{"preserve":true}{{extra}}}""";

    internal static string Entry(string rows)
        => OpenForgeDocumentSeed.Metadata("Independent parent", ["Fixture"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Independent parent\r\n\r\nPreserve introduction and row policy.",
            Entries = rows,
            LineEnding = "\r\n",
        }));

    internal void SeedCategory()
    {
        Inner.WriteText(Anchor, Entry(SurvivingRows + "- [Topics](topics/_topics.md) - #Authored\r\n"));
        Inner.WriteText(WorkspaceRoutePath.CanonicalEntrypointPath(Category), Entry("- [Child](child.md) - #Fixture"));
        Inner.WriteText(Child, "# Child\n");
        Inner.WriteBytes(Resource, [0, 255, 13, 10]);
    }

    internal RouteRemoveRequest Request(string path = Leaf, RouteRemoveMode mode = RouteRemoveMode.Apply)
        => new(Inner.Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, mode, automatic: true);

    internal async Task<RouteRemovePlan> Plan(string path = Leaf)
    {
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(Request(path), TestContext.Current.CancellationToken);
        Assert.True(build.Plan is not null, string.Join(Environment.NewLine, build.Formation.Findings.Select(finding => finding.Cause)));
        return Assert.IsType<RouteRemovePlan>(build.Plan);
    }

    internal ValueTask<RouteRemoveResult> Execute(string path = Leaf, RouteRemoveMode mode = RouteRemoveMode.Apply)
        => RouteRemoveOperationFactory.Create(Inner.LockStoreRoot).ExecuteAsync(Request(path, mode), TestContext.Current.CancellationToken);

    public void Dispose() => Inner.Dispose();
}
