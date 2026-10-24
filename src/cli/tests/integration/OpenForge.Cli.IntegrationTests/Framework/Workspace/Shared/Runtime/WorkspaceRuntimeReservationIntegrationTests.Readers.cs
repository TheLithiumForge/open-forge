using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    [Theory(DisplayName = "Registered runtime files are rejected while other registrations and their children are read")]
    [InlineData(RuntimeFile), InlineData(RuntimeAliasHistory), InlineData(RuntimeAliasFile)]
    [InlineData(RuntimeLinkedSafe)]
    public async Task RegisteredRuntimeFilesAreRejected(string target)
    {
        using var files = TemporaryWorkspace.Create("runtime-registered");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, FixtureSettings(target, SafeFile));
        files.ReplaceText(SourceLogicalPath.LoaderPath, $"# Loader\n\n## Entries\n\n- [Runtime](../{target}) - #LoadNow\n- [Safe](../{SafeFile}) - #LoadNow\n");
        files.ReplaceText(SafeFile, $"# Safe\n\n## Entries\n\n- [Child]({SafeChildFile}) - #LoadNow\n");
        files.CreateFile(SafeChildFile, "# Safe child\n");
        var workspace = Workspace(files);
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(paths).ReadAsync(workspace, Token);
        var inspection = await new RouteSourceInspector(new SourceReadSessionReader(paths), paths).ReadAsync(Assert.IsType<WorkspaceLayout>(observation.Layout), Token);

        var result = await new RouteIncludedFileReader(inspection.Catalogue.Layout).ReadAsync(inspection, observation.Settings, Token);

        Assert.NotNull(result);
        var rejected = Assert.Single(result.Files, file => file.Path == target);
        Assert.True(rejected.Registered);
        Assert.True(rejected.Listed);
        MatchSnapshot(Assert.IsType<string>(rejected.Rejection), "rejection.content");
        var safe = Assert.Single(result.Files, file => file.Path == SafeFile);
        Assert.True(safe.Registered);
        Assert.Null(safe.Rejection);
        var child = Assert.Single(result.Files, file => file.Path == SafeChildFile);
        Assert.True(child.FileExists);
        Assert.Null(child.Rejection);
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    [Fact(DisplayName = "Included overwrite companions cannot read runtime bytes through a file link")]
    public async Task IncludedRuntimeCompanionIsOmitted()
    {
        using var files = TemporaryWorkspace.Create("runtime-companion");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, FixtureSettings(SafeFile));
        files.ReplaceText(SourceLogicalPath.LoaderPath, $"# Loader\n\n## Entries\n\n- [Safe](../{SafeFile}) - #LoadNow\n");
        const string content = SafeDocumentContent;
        files.CreateFile(SafeFile, content);
        Assert.True(files.TryCreateFileSymbolicLink(SafeCompanionFile, files.Combine(RuntimeFile), out var cause), cause);
        var workspace = Workspace(files);
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(paths).ReadAsync(workspace, Token);
        var inspection = await new RouteSourceInspector(new SourceReadSessionReader(paths), paths).ReadAsync(Assert.IsType<WorkspaceLayout>(observation.Layout), Token);

        var result = await new RouteIncludedFileReader(inspection.Catalogue.Layout).ReadAsync(inspection, observation.Settings, Token);

        Assert.NotNull(result);
        Assert.Equal(1L, result.Total.Files.Value);
        Assert.Equal(content.Length, result.Total.Utf8Bytes.Value);
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    [Theory(DisplayName = "Reference fragments reject runtime targets before acquiring any target bytes")]
    [InlineData(RuntimeFile), InlineData(RuntimeAliasHistory), InlineData(RuntimeAliasFile)]
    [InlineData(RuntimeLinkedSafe)]
    public async Task ReferenceFragmentsNeverReadRuntime(string target)
    {
        using var files = TemporaryWorkspace.Create("runtime-fragment");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        var workspace = Workspace(files);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(workspace, Token);
        var reads = new List<string>();
        var resolver = new SourceLinkDestinationResolver(new PhysicalPathResolver(), (physical, logical, token) =>
        {
            reads.Add(logical);
            return StrictUtf8FileReader.ReadAsync(physical, logical, token);
        }, new MarkdownDocumentParser().Parse);
        var input = new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = session.Catalogue,
            SourceCanonicalPath = RootCatalogue,
            RawDestination = $"../../{target}{HistoryFragment}",
        };

        var result = await resolver.ResolveAsync(input, Token);

        Assert.Equal(SourceLinkTargetResolution.Unreadable, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnreadable, result.Finding?.Code);
        MatchSnapshot(Assert.IsType<string>(result.Finding?.Cause), "rejection.content");
        Assert.Null(result.Target.PhysicalPath);
        Assert.Empty(reads);
        var safe = await resolver.ResolveAsync(input with { RawDestination = SafeDestination }, Token);
        Assert.Equal(SourceLinkTargetResolution.Complete, safe.Target.Resolution);
        Assert.Equal([SafeFile], reads);
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    [Theory(DisplayName = "Context leaves runtime links unresolved and still expands ordinary links")]
    [InlineData(RuntimeFile, ""), InlineData(RuntimeFile, HistoryFragment)]
    [InlineData(RuntimeAliasHistory, ""), InlineData(RuntimeAliasHistory, HistoryFragment)]
    [InlineData(RuntimeAliasFile, ""), InlineData(RuntimeAliasFile, HistoryFragment)]
    public async Task ContextDoesNotExpandRuntimeLinks(string target, string fragment)
    {
        using var files = TemporaryWorkspace.Create("runtime-context");
        SeedWorkspace(files);
        SeedRuntimeLinks(files);
        files.CreateFile(SourceLogicalPath.WorkspaceEntryPath, $"Read `{SourceLogicalPath.LoaderPath}`.\n");
        files.ReplaceText(RootLeaf, $"---\nopen-forge:\n  description: Leaf\n  tags: [Docs]\n---\n# Leaf\n\n[History](../../{target}{fragment})\n\n[Safe](../../{SafeFile})\n");

        var result = await CliHostCapture.RunAsync([ContextDefinitions.CommandIdentity, LeafReference, ContextDefinitions.FollowLinks.Name, ContextDefinitions.All, CliSyntaxDefinitions.Format.Name, JsonFormat], files.Path);

        Assert.Equal(3, result.ExitCode);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Contains(document.RootElement.GetProperty(CliJsonFields.Findings).EnumerateArray(),
            finding => finding.GetProperty(CliJsonFields.Code).GetString() == ContextDefinitions.Read(ContextFindingCode.TargetUnreadable).Code);
        MatchSnapshot(CommandOutputNormalization.ReplaceDelimitedPath(result.Output, files.Path, "<workspace>"), $"{target.Replace('/', '-')}{fragment}.json.{CliPresentationDefinitions.Minimal}");
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
    }

    private static string FixtureSettings(params string[] roots)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteNumber(WorkspaceSettingsDefinitions.SchemaVersionProperty, FixtureSettingsVersion);
            writer.WriteStartArray(WorkspaceSettingsDefinitions.RootsProperty);
            foreach (var root in roots) writer.WriteStringValue(root);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void SeedRuntimeLinks(TemporaryWorkspace files)
    {
        files.CreateFile(RuntimeFile, "# History\n\nRUNTIME-CONTENT-PROBE\n");
        files.CreateFile(SafeFile, "# Safe\n\nSAFE-CONTENT-PROBE\n");
        Assert.True(files.TryCreateDirectorySymbolicLink(RuntimeAliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var directoryCause), directoryCause);
        Assert.True(files.TryCreateFileSymbolicLink(RuntimeAliasFile, files.Combine(RuntimeFile), out var fileCause), fileCause);
        Assert.True(files.TryCreateFileSymbolicLink(RuntimeLinkedSafe, files.Combine(SafeFile), out var lexicalCause), lexicalCause);
    }
}
