using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;

internal sealed class OutsideMaintenanceWorkspace : IDisposable
{
    internal const string Route = "docs/team";
    internal const string Other = "reference";
    internal const string CustomRoot = "knowledge/main";
    internal const string Body = "# Note\r\n\r\nIndependent authored bytes.\r\n";
    internal static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Route);
    internal static readonly string Note = Route + "/nested/note" + SourceFormClassifier.MarkdownExtension;
    internal static readonly string OtherNote = Other + "/note" + SourceFormClassifier.MarkdownExtension;
    internal readonly TemporaryWorkspace Files;
    internal readonly CliWorkspace Workspace;
    internal readonly string Loader;
    private readonly bool _anchor;

    internal OutsideMaintenanceWorkspace(string mainRoot = WorkspaceLayout.DefaultMainRoot, bool loader = true, bool listed = false, bool anchor = false)
    {
        _anchor = anchor;
        Files = TemporaryWorkspace.Create("outside-index");
        Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Route}}","{{Other}}"]}""");
        Files.CreateFile(Note, Body);
        Files.CreateFile(OtherNote, Body);
        if (anchor) Files.CreateFile(Anchor, GeneratedLoaderDocumentBuilder.Build(MarkdownEntriesSectionReader.EmptyEntry));
        if (loader)
        {
            var destination = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
            Files.CreateFile(Loader, GeneratedLoaderDocumentBuilder.Build(listed
                ? $"- [Authored team policy]({destination}) - #Authored - applies to `src/**`"
                : MarkdownEntriesSectionReader.EmptyEntry) + "\n");
        }
    }

    internal byte[] Read(string path) => File.ReadAllBytes(Files.Combine(path));
    public void Dispose()
    {
        foreach (var path in new[] { Anchor, WorkspaceRoutePath.CanonicalEntrypointPath(Route + "/nested") })
        {
            if (path == Anchor && _anchor) continue;
            var physical = Files.Combine(path);
            if (!File.Exists(physical)) continue;
            Assert.Equal((FileAttributes)0, File.GetAttributes(physical) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            File.Delete(physical);
        }
        Files.Dispose();
    }
}
