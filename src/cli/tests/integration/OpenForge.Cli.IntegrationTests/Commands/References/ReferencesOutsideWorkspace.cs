using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

internal sealed class ReferencesOutsideWorkspace : IDisposable
{
    internal const string CustomRoot = "knowledge/main";
    internal const string Directory = "docs/team";
    internal const string Leaf = Directory + "/links" + SourceFormClassifier.MarkdownExtension;
    internal const string Unlisted = Directory + "/unlisted" + SourceFormClassifier.MarkdownExtension;
    internal const string Opaque = Directory + "/opaque.mdc";
    internal const string Target = "target" + SourceFormClassifier.MarkdownExtension;
    internal const string Destination = "../../" + Target;
    internal static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("references-outside");
    internal WorkspaceLayout Layout { get; }
    private byte[] _settings;

    internal ReferencesOutsideWorkspace(string mainRoot)
    {
        Layout = WorkspaceLayout.Create(new CliWorkspace(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        _settings = WorkspaceSettingsCodec.SetMainRoot(ReadOnlyMemory<byte>.Empty, mainRoot) ?? [];
        if (_settings.Length != 0) Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, _settings);
        var prefix = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length));
        Files.CreateFile(Layout.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n',
            Row(prefix + Anchor), Row(prefix + Target), Row(prefix + Opaque))));
        Files.CreateFile(Anchor, OpenForgeDocumentSeed.GeneratedEntries(string.Join('\n', Row(Path.GetFileName(Leaf)), Row(Destination))));
        Files.CreateFile(Leaf, OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = $"# Links\r\n\r\n[First]({Destination}) [Again]({Destination}#details)\r\n",
            Entries = Row(Destination),
            LineEnding = "\r\n",
        }));
        Files.CreateFile(Unlisted, $"# Unlisted\n\n[Incoming]({Destination})\n");
        Files.CreateFile(Opaque, $"# Opaque\n\n[Ignored]({Destination})\n");
        Files.CreateFile(Target, "# Target\n\n## Details\n");
    }

    private static string Row(string destination)
        => $"- [Included]({destination}) - #{SourceLoadingClosureResolver.LoadNowTag}";

    internal void Register()
    {
        var existing = _settings.Length != 0;
        _settings = WorkspaceSettingsCodec.AddOutsideRoute(_settings, Directory) ?? throw new InvalidOperationException();
        if (existing) Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
        else Files.WriteBytes(WorkspaceSettingsDefinitions.RelativePath, _settings);
    }

    public void Dispose() => Files.Dispose();
}
