using OpenForge.Cli.Core.Framework.Sources.Loading;
using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

internal sealed class RouteMoveOutsideWorkspace : IDisposable
{
    internal const string Root = "docs/team";
    internal const string Left = Root + "/left";
    internal const string Right = Root + "/right";
    internal const string Leaf = Left + "/old note.md";
    internal const string Destination = Right + "/new note.md";
    internal const string Tag = "Docs";
    internal const string LeafText = "# Leaf\n\n[Resource](local.txt) and [Shared](../../../shared.md#section).\n";
    internal const string OverwriteText = "Local authored overwrite.\n";
    private readonly TemporaryWorkspace temporary = TemporaryWorkspace.Create("move-outside");
    private readonly HashSet<string> outputFiles = new(StringComparer.Ordinal);
    private readonly HashSet<string> outputDirectories = new(StringComparer.Ordinal);
    internal WorkspaceLockTestStore LockStore { get; } = WorkspaceLockTestStore.Create("move-outside-lock");
    internal CliWorkspace Workspace { get; }
    internal string MainRoot { get; }
    internal string Loader => MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    internal string Anchor { get; }
    internal string LeftCatalogue => WorkspaceRoutePath.CanonicalEntrypointPath(Left);
    internal string RightCatalogue => WorkspaceRoutePath.CanonicalEntrypointPath(Right);

    internal RouteMoveOutsideWorkspace(bool custom, bool compatibility = false, bool listed = false)
    {
        Workspace = new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = LockStore.Track(Workspace);
        MainRoot = custom ? "handbook/context" : WorkspaceLayout.DefaultMainRoot;
        Anchor = compatibility ? Root + "/" + SourceFormClassifier.IndexFileName : WorkspaceRoutePath.CanonicalEntrypointPath(Root);
        var settings = new JsonObject { [WorkspaceSettingsDefinitions.MainRootProperty] = MainRoot, ["future"] = new JsonObject { ["keep"] = 7 } };
        temporary.WriteBytes(WorkspaceSettingsDefinitions.RelativePath,
            WorkspaceSettingsCodec.AddOutsideRoute(Encoding.UTF8.GetBytes(settings.ToJsonString()), Root)
                ?? throw new InvalidOperationException("The seed requires a fresh registration."));
        temporary.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with { Framework = new(new("fixture", null), [], []) }));
        var rootLink = Path.GetRelativePath(MainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
        Write(Loader, OpenForgeDocumentSeed.GeneratedEntries($"- [Notes](notes/_notes.md) - #{Tag}" + (listed ? $"\n- [Team]({rootLink}) - #{Tag}" : string.Empty)));
        Write(MainRoot + "/notes/_notes.md", Catalogue("- [Primary](note.md) - #Docs"));
        Write(MainRoot + "/notes/note.md", Markdown("# Primary\n"));
        Write(Anchor, Catalogue("- [Left](left/_left.md) - #Docs\n- [Right](right/_right.md) - #Docs"));
        Write(LeftCatalogue, Catalogue($"- [First](old%20note.md) - #Docs #{SourceLoadingClosureResolver.LoadNowTag} - applies to `src/**`\n- [Second](old%20note.md) - #Review - applies to `tests/**`\n- [Topic](topic/_topic.md) - #Docs\n- [Incoming](incoming.md) - #Docs"));
        Write(RightCatalogue, Catalogue(string.Empty));
        Write(Leaf, Markdown(LeafText));
        Write(SourceOverwritePath.ReadAdjacentPath(Leaf), OverwriteText);
        Write(Left + "/local.txt", "ordinary resource\n");
        Write(Left + "/incoming.md", Markdown("[Incoming](old%20note.md#leaf)\n"));
        Write("shared.md", "# Shared\n\n## Section\n");
        Write(Left + "/topic/_topic.md", Catalogue("- [Child](child.md) - #Docs"));
        Write(Left + "/topic/child.md", Markdown("# Child\n\n[Local](assets/data.bin)\n"));
        temporary.WriteBytes(Left + "/topic/assets/data.bin", [0, 255, 18, 0]);
    }

    internal static string Markdown(string body) => OpenForgeDocumentSeed.Metadata("Outside fixture", [Tag], body);
    internal static string Catalogue(string rows) => Markdown(OpenForgeDocumentSeed.GeneratedEntries(rows));
    internal string Absolute(string path) => temporary.Combine(path);
    internal string Read(string path) => File.ReadAllText(Absolute(path));
    internal byte[] Bytes(string path) => File.ReadAllBytes(Absolute(path));
    internal IReadOnlyDictionary<string, string> Snapshot() => temporary.SnapshotHashes();
    internal void Write(string path, string text)
    {
        if (File.Exists(Absolute(path))) temporary.ReplaceText(path, text);
        else temporary.WriteText(path, text);
    }
    internal void ChangeSettings(Action<JsonObject> change)
    {
        var settings = JsonNode.Parse(Bytes(WorkspaceSettingsDefinitions.RelativePath)) as JsonObject
            ?? throw new InvalidOperationException("The seed settings require an object.");
        change(settings);
        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, settings.ToJsonString());
    }
    internal RouteMoveRequest Request(string source = Leaf, string destination = Destination, RouteMoveMode mode = RouteMoveMode.DryRun)
        => new(Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + source,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + destination, mode);
    internal void OwnLeaf(string destination = Destination)
    {
        outputFiles.Add(destination);
        outputFiles.Add(SourceOverwritePath.ReadAdjacentPath(destination));
    }
    internal void OwnCategory(string source, string destination)
    {
        var sourceRoot = SourceLogicalPath.ReadParent(source);
        var destinationRoot = SourceLogicalPath.ReadParent(destination);
        foreach (var file in Directory.EnumerateFiles(Absolute(sourceRoot), "*", SearchOption.AllDirectories))
        {
            var path = Path.GetRelativePath(Workspace.LexicalRoot, file).Replace(Path.DirectorySeparatorChar, '/');
            outputFiles.Add(path == source ? destination : destinationRoot + path[sourceRoot.Length..]);
        }
        outputDirectories.Add(destinationRoot);
        foreach (var directory in Directory.EnumerateDirectories(Absolute(sourceRoot), "*", SearchOption.AllDirectories))
            outputDirectories.Add(destinationRoot + Path.GetRelativePath(Workspace.LexicalRoot, directory).Replace(Path.DirectorySeparatorChar, '/')[sourceRoot.Length..]);
    }
    internal TemporaryWorkspace Temporary => temporary;
    public void Dispose()
    {
        foreach (var path in outputFiles)
        {
            var physical = Absolute(path);
            if (!File.Exists(physical)) continue;
            Assert.Null(new FileInfo(physical).LinkTarget);
            File.SetAttributes(physical, FileAttributes.Normal);
            File.Delete(physical);
        }
        foreach (var path in outputDirectories.OrderByDescending(value => value.Length))
            if (Directory.Exists(Absolute(path))) Directory.Delete(Absolute(path), false);
        LockStore.Dispose();
        temporary.Dispose();
    }
}
