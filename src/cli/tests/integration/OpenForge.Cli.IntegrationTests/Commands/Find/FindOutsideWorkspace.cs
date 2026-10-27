using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

internal sealed class FindOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string Directory = "docs/team";
    internal const string Leaf = Directory + "/rule" + SourceFormClassifier.MarkdownExtension;
    internal const string Unlisted = Directory + "/unlisted" + SourceFormClassifier.MarkdownExtension;
    internal const string Alias = Directory + "/a-alias" + SourceFormClassifier.MarkdownExtension;
    internal const string RowTag = "RowPolicy";
    internal const string RowDescription = "First authored policy";
    internal const string Condition = "src/**";
    internal const string ChildCondition = "**/*.cs";
    internal static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("find-outside");
    internal WorkspaceLayout Layout { get; }
    private byte[] _settings;

    internal FindOutsideWorkspace(string mainRoot, bool listed = true, bool alias = false)
    {
        Layout = WorkspaceLayout.Create(new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        _settings = WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, mainRoot) ?? [];
        if (_settings.Length != 0) Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, _settings);
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        Files.CreateFile(Layout.LoaderPath, OpenForgeDocumentSeed.Metadata("Loader", [RowTag],
            OpenForgeDocumentSeed.GeneratedEntries(listed ? Row(prefix + Anchor, Condition) : string.Empty)));
        Files.CreateFile(Anchor, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n',
            Row(Path.GetFileName(Leaf), ChildCondition),
            Row(alias ? Path.GetFileName(Alias) : Path.GetFileName(Leaf), "tests/**"))));
        Files.CreateFile(Leaf, "# Rule\r\n\r\nAuthored body.\r\n");
        Files.CreateFile(Unlisted, OpenForgeDocumentSeed.Metadata("Unrouted inventory", [RowTag], "# Unlisted\n"));
        if (alias) Files.CreateFileSymbolicLink(Alias, Files.Combine(Leaf));
    }

    private static string Row(string destination, string condition)
        => $"- [{RowDescription}]({destination}) - #{RowTag} #{SourceLoadingClosureResolver.LoadNowTag}"
            + MarkdownEntryRowFormatter.FormatApplyToSuffix([new ApplyToPattern(condition, [.. condition.Split('/')])]);

    internal void Register()
    {
        var existing = _settings.Length != 0;
        _settings = WorkspaceSettingsCodec.AddOutsideRoute(_settings, Directory) ?? throw new InvalidOperationException();
        if (existing) Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
        else Files.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
    }

    public void Dispose() => Files.Dispose();
}
