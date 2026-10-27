using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "outside-route-journey"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedOutsideRouteProcessTests
{
    private const string MainRoot = "knowledge/main";
    private const string Team = "docs/team";
    private const string Anchor = Team + "/_team.md";
    private const string FixtureTag = "HJourney";

    [Fact]
    public async Task CustomSetupIncludesCreatesIndexesAndReadsExactOutsideIdentities()
    {
        using var fixture = new Fixture();
        string[] install = [PublishedTokens.Install.Command, PublishedTokens.Install.MainRoot, MainRoot,
            PublishedTokens.Install.Preset, PublishedTokens.Install.MinimalPreset, PublishedTokens.Install.Frontmatter,
            PublishedTokens.Settings.ScopedFrontmatter, PublishedTokens.Install.Automatic];
        await fixture.Preview(install, PublishedTokens.Install.DryRun);
        await fixture.Apply(install);
        fixture.Write(Team + "/kept.txt", "Independent resource bytes.\r\n");
        var kept = fixture.Bytes(Team + "/kept.txt");
        Assert.False(File.Exists(fixture.PathFor(Anchor)));
        string[] include = [PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, Team,
            PublishedTokens.RouteInclude.Description, "Team index", PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow];
        await fixture.Preview(include, PublishedTokens.RouteInclude.DryRun);
        await fixture.Apply(include);
        Assert.True(File.Exists(fixture.PathFor(Anchor)));
        fixture.AssertRegistrations(Team);
        const string leaf = Team + "/handoff.md";
        string[] create = [PublishedTokens.Route.Command, PublishedTokens.RouteCreate.Command, Reference(leaf),
            PublishedTokens.RouteCreate.Description, "Handoff", PublishedTokens.RouteCreate.Tag, FixtureTag];
        await fixture.Preview(create, PublishedTokens.RouteCreate.DryRun);
        await fixture.Apply(create);
        Assert.Equal(Encoding.UTF8.GetBytes(OpenForgeDocumentSeed.MetadataFrontmatter("Handoff", [FixtureTag]) + "\n"), fixture.Bytes(leaf));
        var authored = Document("Handoff", "# Handoff\n\n[Team](_team.md)\n\nIndependent π bytes.\r\n");
        fixture.Write(leaf, authored);
        string[] index = [PublishedTokens.Index.Command, Reference(Anchor)];
        await fixture.Preview(index, PublishedTokens.Index.DryRun);
        await fixture.Apply(index);
        await fixture.NoOp(index);
        Assert.Equal(Encoding.UTF8.GetBytes(authored), fixture.Bytes(leaf));
        Assert.Equal(kept, fixture.Bytes(Team + "/kept.txt"));
        using var context = await fixture.Read([PublishedTokens.Context.Command, Reference(leaf)]);
        Assert.Contains(Data(context).GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.Json.Path).GetString() == leaf
                && row.GetProperty(PublishedTokens.Json.Id).GetString() == Reference(leaf));
        using var found = await fixture.Read([PublishedTokens.Find.Command, PublishedTokens.Find.Tag, FixtureTag]);
        Assert.Contains(Data(found).GetProperty(PublishedTokens.FindData.Matches).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.Json.Path).GetString() == leaf
                && row.GetProperty(PublishedTokens.Json.Id).GetString() == Reference(leaf));
        using var references = await fixture.Read([PublishedTokens.References.Command, Reference(leaf)]);
        Assert.Equal(Reference(leaf), Data(references).GetProperty(PublishedTokens.ReferencesData.Source).GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Contains(Data(references).GetProperty(PublishedTokens.ReferencesData.Outgoing).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.ReferencesData.ResolvedPath).GetString() == Anchor);
        using var listed = await fixture.Read([PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, Reference(Anchor),
            PublishedTokens.RouteList.Depth, PublishedTokens.RouteList.AllDepth]);
        Assert.Contains(Data(listed).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.Json.Id).GetString() == Reference(leaf));
        using var inspected = await fixture.Read([PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command, Reference(leaf)]);
        Assert.Equal(Reference(leaf), Data(inspected).GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(Team, Data(inspected).GetProperty(PublishedTokens.RouteInspectData.MaintenanceRoot).GetString());
        await fixture.AssertOutside(listed: true);
    }

    [Fact]
    public async Task ConditionedAuthoredRowsStayUnlistedThroughBareExplicitIndexAndExclude()
    {
        using var fixture = new Fixture();
        fixture.Seed();
        var rows = $"- [First label](child.md) - #{PublishedTokens.Metadata.LoadNow} #Policy - applies to `src/**`\n"
            + $"- [Second label](child.md) - #{PublishedTokens.Metadata.KeepInMind} - applies to `tests/**`";
        var anchor = Document("Team", OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        { Prefix = "# Team\n\nIndependent authored prefix.", Entries = rows, LineEnding = "\n" }));
        fixture.Write(Anchor, anchor);
        fixture.Write(Team + "/child.md", Document("Child", "# Child\n\nKeep body bytes.\r\n"));
        string[] include = [PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, Team,
            PublishedTokens.RouteInclude.Description, "Conditional team", PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow,
            PublishedTokens.RouteInclude.ApplyTo, "src/**"];
        await fixture.Preview(include, PublishedTokens.RouteInclude.DryRun);
        await fixture.Apply(include);
        Assert.Equal(Encoding.UTF8.GetBytes(anchor), fixture.Bytes(Anchor));
        fixture.AssertRegistrations(Team);
        await fixture.AssertOutside(listed: true);
        fixture.Write(Loader, GeneratedLoaderDocumentBuilder.Build(PublishedTokens.Metadata.EmptyEntry) + "\n");
        var loader = fixture.Bytes(Loader);
        var fresh = Document("Fresh", "# Fresh\n\nIndependent addition.\n");
        fixture.Write(Team + "/fresh.md", fresh);
        await fixture.Preview([PublishedTokens.Index.Command], PublishedTokens.Index.DryRun);
        await fixture.Apply([PublishedTokens.Index.Command]);
        Assert.Equal(Encoding.UTF8.GetBytes(anchor), fixture.Bytes(Anchor));
        await fixture.AssertOutside(listed: false);
        string[] index = [PublishedTokens.Index.Command, Reference(Anchor)];
        await fixture.Preview(index, PublishedTokens.Index.DryRun);
        await fixture.Apply(index);
        Assert.Contains(rows, Encoding.UTF8.GetString(fixture.Bytes(Anchor)), StringComparison.Ordinal);
        Assert.NotEqual(Encoding.UTF8.GetBytes(anchor), fixture.Bytes(Anchor));
        Assert.Equal(Encoding.UTF8.GetBytes(fresh), fixture.Bytes(Team + "/fresh.md"));
        Assert.Equal(loader, fixture.Bytes(Loader));
        await fixture.NoOp(index);
        using var inventory = await fixture.Read([PublishedTokens.Route.Command, PublishedTokens.RouteList.Command, Reference(Anchor),
            PublishedTokens.RouteList.Depth, PublishedTokens.RouteList.AllDepth]);
        Assert.Contains(Data(inventory).GetProperty(PublishedTokens.RouteListData.Rows).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.Json.Id).GetString() == Reference(Team + "/fresh.md"));
        using var profile = await fixture.Read([PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command, Reference(Anchor),
            PublishedTokens.RouteInspect.WorkingPath, "src/app.cs"]);
        Assert.Equal(Team, Data(profile).GetProperty(PublishedTokens.RouteInspectData.MaintenanceRoot).GetString());
        Assert.Equal(JsonValueKind.Null, Data(profile).GetProperty(PublishedTokens.RouteInspectData.Read).GetProperty(PublishedTokens.RouteInspectData.AtStart).ValueKind);
        var beforeExclude = fixture.Bytes(Anchor);
        string[] exclude = [PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, Team];
        await fixture.Preview(exclude, PublishedTokens.RouteExclude.DryRun);
        await fixture.Apply(exclude);
        fixture.AssertRegistrations();
        Assert.Equal(beforeExclude, fixture.Bytes(Anchor));
        Assert.Equal(loader, fixture.Bytes(Loader));
        await fixture.Apply([PublishedTokens.Index.Command]);
        Assert.Equal(beforeExclude, fixture.Bytes(Anchor));
        Assert.Equal(loader, fixture.Bytes(Loader));
    }

    [Fact]
    public async Task MoveAndDescendantRemovalStayBoundedAndReportRecoveryCleanup()
    {
        using var fixture = new Fixture();
        const string other = "docs/other";
        const string category = Team + "/topic";
        const string categoryAnchor = category + "/_topic.md";
        const string oldLeaf = category + "/old.md";
        const string newLeaf = category + "/renamed.md";
        fixture.Seed(Team, other);
        fixture.Write(PublishedTokens.Ownership.RelativePath, new JsonObject
        {
            [PublishedTokens.Ownership.Framework] = new JsonObject
            {
                [PublishedTokens.Ownership.Source] = new JsonObject { [PublishedTokens.Ownership.Id] = "independent-fixture" },
            },
        }.ToJsonString());
        fixture.Write(Anchor, Document("Team", OpenForgeDocumentSeed.GeneratedEntries("- [Topic](topic/_topic.md) - #HJourney")));
        fixture.Write(other + "/_other.md", Document("Other", OpenForgeDocumentSeed.GeneratedEntries(PublishedTokens.Metadata.EmptyEntry)));
        var entries = Document("Topic", OpenForgeDocumentSeed.GeneratedEntries("- [Old](old.md) - #HJourney\n- [Reader](reader.md) - #HJourney"));
        var original = Document("Old", "# Old\n\nIndependent moved bytes.\r\n");
        var reader = Document("Reader", "# Reader\n\nKeep [visible label](old.md).\n");
        fixture.Write(categoryAnchor, entries);
        fixture.Write(oldLeaf, original);
        fixture.Write(category + "/reader.md", reader);
        byte[] resource = [0, 1, 2, 255];
        File.WriteAllBytes(fixture.PathFor(category + "/asset.bin"), resource);
        var otherBytes = fixture.Bytes(other + "/_other.md");
        var loader = fixture.Bytes(Loader);
        string[] move = [PublishedTokens.Route.Command, PublishedTokens.RouteMove.Command, Reference(oldLeaf), Reference(newLeaf)];
        await fixture.Preview(move, PublishedTokens.RouteMove.DryRun);
        Assert.Empty(fixture.RecoveryFiles());
        await fixture.Apply(move, recoveryRemoved: true);
        Assert.False(File.Exists(fixture.PathFor(oldLeaf)));
        Assert.Equal(Encoding.UTF8.GetBytes(original), fixture.Bytes(newLeaf));
        Assert.Equal(Encoding.UTF8.GetBytes(reader.Replace("(old.md)", "(renamed.md)", StringComparison.Ordinal)), fixture.Bytes(category + "/reader.md"));
        Assert.Equal(Encoding.UTF8.GetBytes(entries.Replace("(old.md)", "(renamed.md)", StringComparison.Ordinal)), fixture.Bytes(categoryAnchor));
        Assert.Empty(fixture.RecoveryFiles());
        await fixture.Refuse([PublishedTokens.Route.Command, PublishedTokens.RouteMove.Command, Reference(newLeaf), Reference(other + "/renamed.md")]);
        foreach (var protectedPath in new[] { Team, Anchor })
            await fixture.Refuse([PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, Reference(protectedPath), PublishedTokens.RouteRemove.Automatic], protectedTarget: true);
        string[] remove = [PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, Reference(categoryAnchor), PublishedTokens.RouteRemove.Automatic];
        await fixture.Preview(remove, PublishedTokens.RouteRemove.DryRun);
        await fixture.Apply(remove, recoveryCleared: true);
        Assert.False(Directory.Exists(fixture.PathFor(category)));
        Assert.True(File.Exists(fixture.PathFor(Anchor)));
        Assert.Equal(otherBytes, fixture.Bytes(other + "/_other.md"));
        Assert.Equal(loader, fixture.Bytes(Loader));
        fixture.AssertRegistrations(Team, other);
        using var settings = JsonDocument.Parse(fixture.Bytes(PublishedTokens.Settings.RelativePath));
        Assert.Contains(category, settings.RootElement.GetProperty(PublishedTokens.Settings.RemovedDirectoriesProperty).EnumerateArray().Select(row => row.GetString()));
        Assert.False(settings.RootElement.TryGetProperty(PublishedTokens.Settings.RemovedCategoriesProperty, out var categories) && categories.GetArrayLength() > 0);
        Assert.Empty(fixture.RecoveryFiles());
        using var status = await fixture.Read([PublishedTokens.Status.Command], allowIncomplete: true);
        var store = Assert.Single(Data(status).GetProperty(PublishedTokens.StatusRecovery.Recovery).GetProperty(PublishedTokens.StatusRecovery.Stores).EnumerateArray(),
            row => row.GetProperty(PublishedTokens.StatusRecovery.Kind).GetString() == PublishedTokens.Recovery.WorkspaceStore);
        Assert.Empty(store.GetProperty(PublishedTokens.StatusRecovery.Candidates).EnumerateArray());
    }

    private static string Reference(string path) => PublishedTokens.Layout.LiteralReferencePrefix + path;
    private static string Loader => MainRoot + "/" + PublishedTokens.Layout.LoaderFileName;
    private static string Document(string description, string body) => OpenForgeDocumentSeed.Metadata(description, [FixtureTag], body);
    private static JsonElement Data(JsonDocument document) => document.RootElement.GetProperty(PublishedTokens.Json.Data);

    private sealed class Fixture : IDisposable
    {
        private readonly TemporaryWorkspace _files = TemporaryWorkspace.Create("published-outside-route");
        private readonly PublishedWorkspaceLockStore _locks = PublishedWorkspaceLockStore.Create("published-outside-route-locks");
        private readonly PublishedExecutableTarget _target = PublishedExecutableTarget.Discover();
        private readonly string _evidence = Path.Combine(PublishedExecutableTarget.FindRepositoryRoot(), "artifacts", "coordinator",
            "task87-h-process", Guid.NewGuid().ToString("N"));
        private int _step;
        internal string Root { get; }

        internal Fixture()
        {
            Root = _files.CreateDirectory("workspace");
            _locks.Track(Root);
        }

        internal string PathFor(string path) => _files.Combine("workspace", path);
        internal byte[] Bytes(string path) => File.ReadAllBytes(PathFor(path));
        internal void Write(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PathFor(path))!);
            File.WriteAllText(PathFor(path), text, new UTF8Encoding(false));
        }

        internal void Seed(params string[] registrations)
        {
            Write(PublishedTokens.Settings.RelativePath, new JsonObject
            {
                [PublishedTokens.Settings.SchemaVersionProperty] = PublishedTokens.Settings.SchemaVersion,
                [PublishedTokens.Settings.MainRootProperty] = MainRoot,
                [PublishedTokens.Settings.OutsideRoutesProperty] = new JsonArray(registrations.Select(value => JsonValue.Create(value)).ToArray()),
            }.ToJsonString());
            Write(PublishedTokens.Layout.AgentHost, "# Independent workspace\n");
            Write(Loader, GeneratedLoaderDocumentBuilder.Build(PublishedTokens.Metadata.EmptyEntry) + "\n");
        }

        internal void AssertRegistrations(params string[] expected)
        {
            using var settings = JsonDocument.Parse(Bytes(PublishedTokens.Settings.RelativePath));
            var actual = settings.RootElement.TryGetProperty(PublishedTokens.Settings.OutsideRoutesProperty, out var rows)
                ? rows.EnumerateArray().Select(row => row.GetString()).ToArray() : [];
            Assert.Equal(expected, actual);
            Assert.Equal(MainRoot, settings.RootElement.GetProperty(PublishedTokens.Settings.MainRootProperty).GetString());
        }

        private IReadOnlyDictionary<string, string> Snapshot()
            => Directory.EnumerateFileSystemEntries(Root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal)
                .ToDictionary(path => Path.GetRelativePath(Root, path), path => Directory.Exists(path) ? "directory" : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), StringComparer.Ordinal);

        private async Task<ProcessRunResult> Run(string[] arguments, bool readOnly)
        {
            string[] argv = [.. arguments, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat,
                PublishedTokens.Syntax.DetailOption, PublishedTokens.Presentation.FullDetail];
            var result = readOnly ? await PublishedProcessTestSupport.RunWithoutWritesAsync(_target, Root, Snapshot, argv, _locks.EnvironmentVariables)
                : await PublishedProcessTestSupport.RunAsync(_target, Root, argv, _locks.EnvironmentVariables);
            Directory.CreateDirectory(_evidence);
            File.WriteAllText(Path.Combine(_evidence, $"{_step++:D2}.json"), new JsonObject
            {
                [nameof(PublishedExecutableTarget.ExecutablePath)] = _target.ExecutablePath,
                [nameof(PublishedExecutableTarget.ExpectedVersion)] = _target.ExpectedVersion,
                [nameof(arguments)] = new JsonArray(argv.Select(value => JsonValue.Create(value)).ToArray()),
                [nameof(readOnly)] = readOnly,
                [nameof(ProcessRunResult.ExitCode)] = result.ExitCode,
                [nameof(ProcessRunResult.StandardOutput)] = result.StandardOutput,
                [nameof(ProcessRunResult.StandardError)] = result.StandardError,
            }.ToJsonString(), new UTF8Encoding(false));
            return result;
        }

        internal async Task Apply(string[] arguments, bool recoveryRemoved = false, bool recoveryCleared = false)
        {
            var run = await Run(arguments, readOnly: false);
            Assert.True(run.ExitCode == PublishedTokens.Statuses.CompleteExitCode, run.StandardOutput + run.StandardError);
            using var json = JsonDocument.Parse(run.StandardOutput);
            Assert.Equal(PublishedTokens.Statuses.Complete, json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
            if (recoveryRemoved)
            {
                Assert.Equal(PublishedTokens.Recovery.RemovedDisposition,
                    json.RootElement.GetProperty(PublishedTokens.Json.Recovery).GetProperty(PublishedTokens.Json.Disposition).GetString());
                Assert.True(Directory.Exists(PathFor(PublishedTokens.Runtime.DirectoryName)));
                Assert.Empty(RecoveryFiles());
            }
            if (recoveryCleared)
            {
                Assert.Equal(JsonValueKind.Null, json.RootElement.GetProperty(PublishedTokens.Json.Recovery).ValueKind);
                Assert.True(Directory.Exists(PathFor(PublishedTokens.Runtime.DirectoryName)));
                Assert.Empty(RecoveryFiles());
            }
        }

        internal async Task Preview(string[] arguments, string option)
        {
            var run = await Run([.. arguments, option], readOnly: true);
            Assert.True(run.ExitCode == PublishedTokens.Statuses.CompleteExitCode, run.StandardOutput + run.StandardError);
        }

        internal async Task NoOp(string[] arguments)
        {
            using var json = await Read(arguments);
            Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        }

        internal async Task<JsonDocument> Read(string[] arguments, bool allowIncomplete = false)
        {
            var run = await Run(arguments, readOnly: true);
            var accepted = allowIncomplete ? new[] { PublishedTokens.Statuses.CompleteExitCode, PublishedTokens.Statuses.AttentionExitCode, PublishedTokens.Statuses.IncompleteExitCode }
                : [PublishedTokens.Statuses.CompleteExitCode];
            Assert.True(accepted.Contains(run.ExitCode), run.StandardOutput + run.StandardError);
            return JsonDocument.Parse(run.StandardOutput);
        }

        internal async Task Refuse(string[] arguments, bool protectedTarget = false)
        {
            var run = await Run(arguments, readOnly: true);
            Assert.True(run.ExitCode == (protectedTarget ? PublishedTokens.Statuses.BlockedExitCode : PublishedTokens.Statuses.InvalidExitCode), run.StandardOutput + run.StandardError);
            using var json = JsonDocument.Parse(run.StandardOutput);
            Assert.Equal(protectedTarget ? PublishedTokens.Statuses.Blocked : PublishedTokens.Statuses.Invalid,
                json.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
            if (protectedTarget) Assert.Contains(json.RootElement.GetProperty(PublishedTokens.Json.Findings).EnumerateArray(),
                finding => finding.GetProperty(PublishedTokens.Json.Code).GetString() == PublishedTokens.RouteRemove.ProtectedTargetCode);
            Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        }

        internal async Task AssertOutside(bool listed)
        {
            using var status = await Read([PublishedTokens.Status.Command], allowIncomplete: true);
            var row = Assert.Single(Data(status).GetProperty(PublishedTokens.StatusOutsideRoutes.Collection).EnumerateArray());
            Assert.Equal(Team, row.GetProperty(PublishedTokens.Json.Path).GetString());
            Assert.Equal(PublishedTokens.StatusOutsideRoutes.Available, row.GetProperty(PublishedTokens.StatusOutsideRoutes.State).GetString());
            Assert.Equal(listed, row.GetProperty(PublishedTokens.StatusOutsideRoutes.Listed).GetBoolean());
        }

        internal string[] RecoveryFiles()
            => Directory.Exists(PathFor(PublishedTokens.Runtime.DirectoryName))
                ? Directory.GetFiles(PathFor(PublishedTokens.Runtime.DirectoryName), "*" + PublishedTokens.RecoveryFormat.FinalFileExtension, SearchOption.AllDirectories) : [];

        public void Dispose()
        {
            DeleteChildren(Root);
            _locks.Dispose();
            _files.Dispose();
        }

        private void DeleteChildren(string directory)
        {
            Assert.True(Path.GetFullPath(directory) == Root || Path.GetFullPath(directory).StartsWith(Root + Path.DirectorySeparatorChar, StringComparison.Ordinal));
            Assert.Equal(FileAttributes.Directory, File.GetAttributes(directory) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(entry);
                Assert.False(attributes.HasFlag(FileAttributes.ReparsePoint));
                if (attributes.HasFlag(FileAttributes.Directory)) { DeleteChildren(entry); Directory.Delete(entry); }
                else File.Delete(entry);
            }
        }
    }
}
