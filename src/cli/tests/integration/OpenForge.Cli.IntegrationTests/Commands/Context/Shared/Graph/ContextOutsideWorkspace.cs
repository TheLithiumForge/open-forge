using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Context.Shared.Graph;

internal sealed class ContextOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string Directory = "docs/team";
    internal const string Leaf = Directory + "/rule" + SourceFormClassifier.MarkdownExtension;
    internal const string Unlisted = Directory + "/unlisted" + SourceFormClassifier.MarkdownExtension;
    internal const string Alias = Directory + "/a-alias" + SourceFormClassifier.MarkdownExtension;
    internal const string FirstCondition = "src/**";
    internal const string LeafCondition = "**/*.cs";
    internal const string OtherCondition = "tests/**";
    internal const string LeafBody = "# Rule\r\n\r\nAuthored outside rule.\r\n";
    internal static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    internal static readonly string Nested = WorkspaceRoutePath.CanonicalEntrypointPath(Directory + "/nested");
    internal static readonly string Deep = Directory + "/nested/deep" + SourceFormClassifier.MarkdownExtension;
    internal static readonly string Skills = WorkspaceRoutePath.CanonicalEntrypointPath(WorkspaceLayout.NativeSkillsPath);
    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("context-outside");
    internal WorkspaceLayout Layout { get; }
    private byte[] _settings;

    internal ContextOutsideWorkspace(string mainRoot, bool listed = true, bool alias = false)
    {
        Layout = WorkspaceLayout.Create(new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        _settings = WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, mainRoot) ?? [];
        if (_settings.Length != 0) Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, _settings);
        Files.CreateFile(SourceLogicalPath.WorkspaceEntryPath, "# Workspace\r\n");
        var relative = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        var primary = Layout.SourcePath(WorkspaceRoutePath.CanonicalEntrypointPath("rules"));
        Files.CreateFile(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n', new[]
        {
            Row("Primary", "rules/" + Path.GetFileName(primary)),
            Row("Skills", Layout.IsDefault ? WorkspaceLayoutNames.SkillsDirectoryName + "/" + Path.GetFileName(Skills) : relative + Skills),
            listed ? Row("First outside", relative + Anchor, FirstCondition) : string.Empty,
            listed ? Row("Later outside", relative + Anchor, OtherCondition) : string.Empty,
        })));
        Files.CreateFile(primary, OpenForgeDocumentSeed.Metadata("Primary", [SourceLoadingClosureResolver.LoadNowTag],
            OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        Files.CreateFile(Skills, OpenForgeDocumentSeed.Metadata("Skills", [SourceLoadingClosureResolver.LoadNowTag],
            OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        Files.CreateFile(Anchor, OpenForgeDocumentSeed.Metadata("Team", [], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Team\r\n\r\n[Ordinary](unlisted.md)",
            Entries = string.Join("\r\n", Row("First rule", Path.GetFileName(Leaf), LeafCondition),
                Row("Later rule", alias ? Path.GetFileName(Alias) : Path.GetFileName(Leaf), OtherCondition),
                Row("Nested", "nested/" + Path.GetFileName(Nested), LeafCondition)),
            LineEnding = "\r\n",
        })));
        Files.CreateFile(Leaf, LeafBody);
        Files.CreateFile(Unlisted, OpenForgeDocumentSeed.Metadata("Unlisted", [SourceLoadingClosureResolver.LoadNowTag], "# Unlisted\n"));
        Files.CreateFile(Nested, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n', Row("Deep", Path.GetFileName(Deep)),
            Row("Other parent", "../" + Path.GetFileName(Leaf), OtherCondition))));
        Files.CreateFile(Deep, "# Deep\n");
        if (alias) Files.CreateFileSymbolicLink(Alias, Files.Combine(Leaf));
    }

    internal void Register()
    {
        var existing = _settings.Length != 0;
        _settings = WorkspaceSettingsCodec.AddOutsideRoute(_settings, Directory)
            ?? throw new InvalidOperationException();
        if (existing) Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
        else Files.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
    }

    internal static string Row(string description, string destination, string? condition = null)
        => $"- [{description}]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}"
            + (condition is null ? string.Empty : MarkdownEntryRowFormatter.FormatApplyToSuffix([new ApplyToPattern(condition, [.. condition.Split('/')])]));

    public void Dispose() => Files.Dispose();
}
