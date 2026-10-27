using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

internal sealed class RouteListOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string DirectoryPath = "docs/team";
    internal const string Child = DirectoryPath + "/child.md";
    internal const string Overwrite = DirectoryPath + "/child.overwrite.md";
    internal const string Skill = DirectoryPath + "/" + SourceFormClassifier.NativeSkillFileName;
    internal const string Independent = DirectoryPath + "/independent.md";
    internal const string IndependentOverwrite = DirectoryPath + "/independent.overwrite.md";
    internal static readonly string IndependentDocument = OpenForgeDocumentSeed.Metadata(
        "Independent authored leaf", ["Body"], "# Independent\n\nKeep authored \u03c0 bytes.\n");
    internal const string IndependentOverwriteDocument = "Independent companion bytes\r\n";
    internal const string FirstRow = $"- [First child](child.md) - #{SourceLoadingClosureResolver.LoadNowTag} #Row - applies to `src/**`\n";
    internal const string SecondRow = $"- [Second child](child.md) - #{SourceLoadingClosureResolver.KeepInMindTag} - applies to `tests/**`\n";
    internal TemporaryWorkspace Inner { get; } = TemporaryWorkspace.Create("route-outside-reporting");
    internal CliWorkspace Workspace => new(Inner.Path, Inner.Path, CliWorkspaceSelectionMethod.CurrentDirectory);
    internal WorkspaceLayout Layout { get; }
    internal string Anchor { get; }
    internal string CanonicalAnchor => WorkspaceRoutePath.CanonicalEntrypointPath(DirectoryPath);
    internal string PrimaryAnchor => WorkspaceRoutePath.CanonicalEntrypointPath(Layout.MainRoot + "/guide");

    internal RouteListOutsideWorkspace(string root = WorkspaceLayout.DefaultMainRoot,
        bool listed = true, bool registered = true, bool fileIncluded = false, bool compatibility = false, string? compatibilityName = null,
        bool inventoryOnly = false)
    {
        Layout = WorkspaceLayout.Create(Workspace, root).WithOutsideRoutes(registered ? [DirectoryPath] : []);
        Anchor = compatibilityName is not null ? DirectoryPath + "/" + compatibilityName
            : compatibility ? DirectoryPath + "/" + SourceFormClassifier.IndexFileName : CanonicalAnchor;
        Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(registered, fileIncluded));
        Inner.WriteText(SourceLogicalPath.WorkspaceEntryPath, "# Workspace\n\nIndependent root bytes.\n");
        Inner.WriteText(PrimaryAnchor, Document("Primary", MarkdownEntriesSectionReader.EmptyEntry));
        var relative = Path.GetRelativePath(Layout.MainRoot, Anchor).Replace('\\', '/');
        Inner.WriteText(Layout.LoaderPath, GeneratedLoaderDocumentBuilder.Build(
            $"- [Primary](guide/_guide.md) - #{SourceLoadingClosureResolver.LoadNowTag}\n" + (listed ? $"- [Team authored]({relative}) - #{SourceLoadingClosureResolver.LoadNowTag}\n" : string.Empty)));
        Inner.WriteText(Anchor, Document("Authored team", inventoryOnly ? MarkdownEntriesSectionReader.EmptyEntry
            : FirstRow + SecondRow + "- [Outside skill](SKILL.md) - #Private\n"));
        if (inventoryOnly)
        {
            Inner.WriteText(Independent, IndependentDocument);
            Inner.WriteText(IndependentOverwrite, IndependentOverwriteDocument);
            return;
        }
        Inner.WriteText(Child, OpenForgeDocumentSeed.Metadata("Authored child", ["Body"], "# Child\n\nBody π.\n"));
        Inner.WriteText(Overwrite, "Independent overwrite bytes\r\n");
        Inner.WriteText(Skill, "# Ordinary outside skill\n");
    }

    internal string Settings(bool registered = true, bool fileIncluded = false, IEnumerable<string>? routes = null)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{Layout.MainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", (routes ?? (registered ? [DirectoryPath] : [])).Select(path => $"\"{path}\""))}}],"{{WorkspaceSettingsDefinitions.RootsProperty}}":[{{(fileIncluded ? $"\"{Anchor}\"" : string.Empty)}}],"independent":{"preserve":true} }""";

    internal static string Document(string description, string rows)
        => OpenForgeDocumentSeed.Metadata(description, ["Authored"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Independent catalogue\n\nKeep authored bytes.",
            Entries = rows,
            LineEnding = "\n",
        }));

    public void Dispose() => Inner.Dispose();
}
