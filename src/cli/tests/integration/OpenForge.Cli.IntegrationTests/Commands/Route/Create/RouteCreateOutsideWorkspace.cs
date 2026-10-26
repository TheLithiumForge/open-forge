using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

internal sealed class RouteCreateOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string Directory = "docs/team";
    internal const string OtherDirectory = "reference";
    internal const string Description = "Authored destination";
    internal const string Tag = "Fixture";
    internal const string Body = "# Authored rules\r\n\r\nKeep every byte outside Entries.\r\n";
    internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("route-create-outside");
    internal readonly CliWorkspace Workspace;
    internal readonly WorkspaceLayout Layout;
    internal readonly string Anchor;
    private readonly List<string> _createdFiles = [];
    private readonly List<string> _createdDirectories = [];

    internal RouteCreateOutsideWorkspace(string mainRoot, string directory = Directory, bool listed = true)
    {
        Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Layout = WorkspaceLayout.Create(Workspace, mainRoot).WithOutsideRoutes([directory]);
        Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(directory);
        Files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([directory]));
        var destination = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
        Files.WriteText(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(listed
            ? $"- [Authored outside row]({destination}) - #{Tag}" : string.Empty));
        Files.WriteText(Anchor, Document(Body));
    }

    internal string Settings(IEnumerable<string> directories, string? extra = null)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", directories.Select(path => $"\"{path}\""))}}]{{extra}}}""";

    internal static string Document(string body = Body, string entries = "")
        => OpenForgeDocumentSeed.Metadata("Independent parent metadata", [Tag], body + OpenForgeDocumentSeed.GeneratedEntries(entries));

    internal RouteCreateRequest Request(string path, bool optional = false, string? template = null, RouteCreateMode mode = RouteCreateMode.Apply)
        => new(Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path,
            new RouteCreateMetadataInput(optional ? null : Description, optional ? [] : [Tag], null), template, mode);

    internal async Task<RouteCreatePlanBuild> Build(string path, bool optional = false, string? template = null)
        => await new RouteCreatePlanBuilder().BuildAsync(Request(path, optional, template), TestContext.Current.CancellationToken);

    internal void Own(RouteCreatePlan plan)
    {
        _createdFiles.AddRange(plan.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Create).Select(change => change.LogicalPath));
        _createdDirectories.AddRange(plan.DirectoryCreations.Select(change => change.LogicalPath));
    }

    public void Dispose()
    {
        foreach (var path in _createdFiles.Distinct(StringComparer.Ordinal))
        {
            if (!File.Exists(path)) continue;
            if ((File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                throw new InvalidOperationException("A created test file changed kind before cleanup.");
            File.Delete(path);
        }
        foreach (var path in _createdDirectories.Distinct(StringComparer.Ordinal).OrderByDescending(path => path.Length))
            if (System.IO.Directory.Exists(path)) System.IO.Directory.Delete(path, recursive: false);
        Files.Dispose();
    }
}
