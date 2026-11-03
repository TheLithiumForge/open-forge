using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexOutsideRefreshIntegrationTests
{
    private const string Route = "docs/team";
    private const string FirstTag = "Alpha";
    private const string AddedTag = "Gamma";
    private const string AuthoredTag = "Authored";
    private const string Condition = "src/**";
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Route);
    private static readonly string Leaf = Route + "/file" + SourceFormClassifier.MarkdownExtension;
    private static readonly string ScopeAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(Route + "/scope");
    private static readonly string Foreign = "reference/note" + SourceFormClassifier.MarkdownExtension;

    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Index rebuilds an outside catalogue's rows after its routed children change metadata, keeps authored rows and then stays current")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ChangedMetadataRefreshesRows(string mainRoot)
    {
        using var fixture = new Fixture(mainRoot);
        var operation = IndexOperationFactory.Create();
        string[] exact = [SourceWorkspaceRelativePath.LiteralReferencePrefix + Anchor];
        Complete(await operation.ExecuteAsync(new(fixture.Workspace, exact, IndexMode.Apply), Token));
        var loader = fixture.Read(fixture.Loader);
        var leaving = fixture.RowBytes(fixture.LeavingDestination);
        fixture.WriteChildren(Revision.Second);

        var refreshed = await operation.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), Token);
        Complete(refreshed);
        Assert.Equal(Anchor, Assert.Single(refreshed.Regions, region => region.Action == IndexRegionAction.Update).Source.Path);
        var entries = fixture.Entries();
        foreach (var child in new[] { Leaf, ScopeAnchor })
        {
            var row = Assert.Single(entries, entry => entry.Destination == Destination(child));
            Assert.Equal(Description(child, Revision.Second), row.Description);
            Assert.Equal(Tags(Revision.Second), row.Tags);
        }

        Assert.Equal(leaving, fixture.RowBytes(fixture.LeavingDestination));
        Assert.Equal(loader, fixture.Read(fixture.Loader));
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["refreshed-catalogue.content"] = Encoding.UTF8.GetString(fixture.Read(Anchor)),
        });

        var current = fixture.Files.SnapshotHashes();
        foreach (var references in new string[][] { [], exact })
        {
            var repeat = await operation.ExecuteAsync(new(fixture.Workspace, references, IndexMode.Apply), Token);
            Complete(repeat);
            Assert.Equal(0, repeat.Counts.Updates);
            Assert.Equal(current, fixture.Files.SnapshotHashes());
        }
    }

    private static void Complete(IndexResult result)
        => Assert.True(result.Status == CliSemanticStatus.Complete,
            string.Join("\n", result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));

    private static string Destination(string child) => child[(Route.Length + 1)..];

    private static string Description(string child, Revision revision) => $"{revision} {Path.GetFileNameWithoutExtension(child)} description";

    private static string[] Tags(Revision revision) => revision == Revision.First ? [FirstTag] : [FirstTag, AddedTag];

    private enum Revision
    {
        First,
        Second,
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("index-outside-refresh");
        internal readonly CliWorkspace Workspace;
        internal readonly string Loader;
        internal readonly string LeavingDestination = Path.GetRelativePath(Route, Foreign).Replace(Path.DirectorySeparatorChar, '/');

        internal Fixture(string mainRoot)
        {
            Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
            Loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
            Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
                $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Route}}"]}""");
            var listing = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
            Files.CreateFile(Loader, GeneratedLoaderDocumentBuilder.Build(
                $"- [{Route}]({listing}) - #{SourceLoadingClosureResolver.LoadNowTag} - applies to `{Condition}`") + "\n");
            Files.CreateFile(Foreign, "# Note\n");
            Files.CreateFile(Anchor, OpenForgeDocumentSeed.Metadata(Route, [FirstTag], "\n" + OpenForgeDocumentSeed.GeneratedEntries(
                $"- [{Foreign}]({LeavingDestination}) - #{AuthoredTag} - applies to `{Condition}`")));
            WriteChildren(Revision.First);
        }

        internal void WriteChildren(Revision revision)
        {
            Write(Leaf, OpenForgeDocumentSeed.Metadata(Description(Leaf, revision), Tags(revision), "# File\n\nBody.\n"));
            Write(ScopeAnchor, OpenForgeDocumentSeed.Metadata(Description(ScopeAnchor, revision), Tags(revision),
                "\n" + OpenForgeDocumentSeed.GeneratedEntries(MarkdownEntriesSectionReader.EmptyEntry)));

            void Write(string path, string text)
            {
                if (revision == Revision.First) Files.CreateFile(path, text);
                else Files.ReplaceText(path, text);
            }
        }

        internal byte[] Read(string path) => File.ReadAllBytes(Files.Combine(path));

        internal IReadOnlyList<SourceGeneratedEntry> Entries()
            => SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(Read(Anchor)))).Entries;

        internal byte[] RowBytes(string destination)
        {
            var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(Read(Anchor)));
            var row = Assert.Single(SourceGeneratedEntriesParser.Parse(document).Entries, entry => entry.Destination == destination);
            return Encoding.UTF8.GetBytes(document.Source[row.Span.Start..row.Span.End]);
        }

        public void Dispose() => Files.Dispose();
    }
}
