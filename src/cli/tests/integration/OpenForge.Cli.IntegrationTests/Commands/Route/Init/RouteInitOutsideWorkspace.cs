using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init;

internal sealed class RouteInitOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string Directory = "docs/team";
    internal const string NestedDirectory = Directory + "/design/decisions";
    internal const string Tag = "Fixture";
    internal const string Description = "Authored outside destination";
    internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("route-init-outside");
    internal readonly CliWorkspace Workspace;
    internal readonly WorkspaceLayout Layout;
    internal readonly string Anchor;
    private readonly List<string> _createdFiles = [];
    private readonly List<string> _createdDirectories = [];

    internal RouteInitOutsideWorkspace(string mainRoot, bool listed = true, string directory = Directory)
    {
        Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Layout = WorkspaceLayout.Create(Workspace, mainRoot).WithOutsideRoutes([directory]);
        Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(directory);
        Files.CreateDirectory(directory);
        Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([directory]));
        var destination = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
        Files.WriteText(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(listed
            ? $"- [Authored conditional outside row]({destination}) - #{Tag}" : string.Empty));
        Files.WriteBytes("unrelated.bin", [0, 239, 187, 191, 255, 13, 10]);
    }

    internal string Settings(IEnumerable<string> directories, string? extra = null)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", directories.Select(path => $"\"{path}\""))}}]{{extra}}}""";

    internal static string Document(string entries = MarkdownEntriesSectionReader.EmptyEntry)
        => OpenForgeDocumentSeed.Metadata("Independent authored metadata", [Tag],
            OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
            {
                Prefix = "# Independent authoring\r\n\r\nKeep this text byte-exact.\r\n",
                Entries = entries,
                LineEnding = "\r\n",
            }));

    internal RouteInitRequest Request(string path, RouteInitMode mode = RouteInitMode.Apply,
        RouteInitMetadataInput? metadata = null, RouteInitScaffold scaffold = RouteInitScaffold.Generic)
        => new(Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path, scaffold, mode, metadata ?? RouteInitMetadataInput.None);

    internal async Task<RouteInitPlan> Plan(string path, RouteInitMetadataInput? metadata = null)
    {
        var build = await new RouteInitPlanBuilder().BuildAsync(Request(path, metadata: metadata), TestContext.Current.CancellationToken);
        Assert.True(build.Plan is not null, string.Join(Environment.NewLine, build.Formation.Findings.Select(finding => finding.Cause)));
        var plan = Assert.IsType<RouteInitPlan>(build.Plan);
        Own(plan);
        return plan;
    }

    internal void Own(RouteInitPlan plan)
    {
        _createdFiles.AddRange(plan.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Create).Select(change => change.LogicalPath));
        _createdDirectories.AddRange(plan.DirectoryCreations.Select(change => change.LogicalPath));
    }

    internal async Task<RouteInitResult> Execute(RouteInitRequest request)
    {
        var build = await new RouteInitPlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        if (build.Plan is { } plan) Own(plan);
        return await RouteInitOperationFactory.Create().ExecuteAsync(request, TestContext.Current.CancellationToken);
    }

    internal void AssertPreserved(IReadOnlyDictionary<string, string> before)
    {
        var after = Files.SnapshotHashes();
        foreach (var row in before.Where(row => !row.Key.StartsWith(Layout.OutsideRoutes[0].DirectoryPath + "/", StringComparison.Ordinal)))
            Assert.Equal(row.Value, after[row.Key]);
        Assert.All(after.Keys.Except(before.Keys, StringComparer.Ordinal), path =>
            Assert.StartsWith(Layout.OutsideRoutes[0].DirectoryPath + "/", path, StringComparison.Ordinal));
    }

    public void Dispose()
    {
        foreach (var path in _createdFiles.Distinct(StringComparer.Ordinal))
        {
            if (!File.Exists(path)) continue;
            if ((File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                throw new InvalidOperationException("A created Init test file changed kind before cleanup.");
            File.Delete(path);
        }
        foreach (var path in _createdDirectories.Distinct(StringComparer.Ordinal).OrderByDescending(path => path.Length))
            if (System.IO.Directory.Exists(path)) System.IO.Directory.Delete(path, recursive: false);
        Files.Dispose();
    }
}
