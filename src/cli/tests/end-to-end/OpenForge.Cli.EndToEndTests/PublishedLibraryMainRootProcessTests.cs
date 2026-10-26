using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedLibraryMainRootProcessTests
{
    private const string CustomRoot = "docs/forge";
    private const string SourceRoot = "shared/knowledge";
    private const string LibraryId = "knowledge";
    private const string Category = "topics";
    private const string CategoryEntrypoint = "_topics";
    private const string FirstName = "first";
    private const string SecondName = "second";
    private const string Tag = "Knowledge";
    private const string Description = "Library knowledge";
    private const string SourceBody = "# " + Description + "\n\nSource-owned bytes.\n";
    private const string Loader = "# Loader\n\n## " + PublishedTokens.Metadata.Entries
        + "\n- [" + Description + "](" + Category + "/" + CategoryEntrypoint + PublishedTokens.Layout.MarkdownExtension
        + ") - #" + PublishedTokens.Metadata.LoadNow + "\n";

    [Theory]
    [InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "library-mutation"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootLifecyclePreservesLiteralSourcesAndRelativeLinks(string form)
    {
        using var workspace = TemporaryWorkspace.Create("published-library-main-root");
        using var store = PublishedWorkspaceLockStore.Create("published-library-main-root-store");
        store.Track(workspace.Path);
        var target = PublishedExecutableTarget.Discover();
        workspace.WriteText(PublishedTokens.Settings.RelativePath, $$"""
            {"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{CustomRoot}}","{{PublishedTokens.Settings.FrontmatterProperty}}":"{{form}}"}
            """);
        workspace.WriteText(CustomRoot + "/" + PublishedTokens.Layout.LoaderFileName, Loader);
        workspace.WriteText(PublishedTokens.Layout.AgentHost, "# Workspace\n\nRead `" + CustomRoot + "/" + PublishedTokens.Layout.LoaderFileName + "`.\n");
        workspace.WriteText(CustomRoot + "/" + PublishedTokens.Layout.GuideFile, "# Guide\n");
        var first = Category + "/" + FirstName + PublishedTokens.Layout.MarkdownExtension;
        var second = Category + "/" + SecondName + PublishedTokens.Layout.MarkdownExtension;
        var indent = form == PublishedTokens.Settings.RootFrontmatter ? string.Empty : "  ";
        var scope = form == PublishedTokens.Settings.RootFrontmatter ? string.Empty : PublishedTokens.Metadata.Scope + ":\n";
        var body = "---\n" + scope + indent + PublishedTokens.Metadata.Description + ": " + Description
            + "\n" + indent + PublishedTokens.Metadata.Tags + ": [" + Tag + ", " + PublishedTokens.Metadata.LoadNow + "]\n---\n" + SourceBody;
        workspace.WriteText(CustomRoot + "/" + Category + "/" + CategoryEntrypoint + PublishedTokens.Layout.MarkdownExtension,
            body + "\n## " + PublishedTokens.Metadata.Entries + "\n");
        workspace.WriteText(SourceRoot + "/" + first, body);
        var sourceBytes = Encoding.UTF8.GetBytes(body);
        var settingsBytes = File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath));
        try
        {
            using var attached = await Run(PublishedTokens.Library.Attach, LibraryId, SourceRoot,
                PublishedTokens.Library.Destination, CustomRoot, PublishedTokens.Library.Automatic);
            CheckLink(first);
            var readOnlyBefore = workspace.SnapshotHashes();
            var context = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
                [PublishedTokens.Context.Command, Category, PublishedTokens.Context.Content, PublishedTokens.Context.Frontmatter,
                    PublishedTokens.Context.FollowLinks, PublishedTokens.Context.All,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], store.EnvironmentVariables);
            Assert.True(context.ExitCode == PublishedTokens.Statuses.CompleteExitCode, context.StandardOutput + context.StandardError);
            using var loaded = JsonDocument.Parse(context.StandardOutput);
            var loadedSource = Assert.Single(loaded.RootElement.GetProperty(PublishedTokens.Json.Data)
                .GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray(),
                source => source.GetProperty(PublishedTokens.Json.Path).GetString() == CustomRoot + "/" + first);
            Assert.Equal(Category + "/" + FirstName, loadedSource.GetProperty(PublishedTokens.Json.Id).GetString());
            using var inspected = await Run(PublishedTokens.Library.Inspect, LibraryId);
            Assert.True(inspected.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.LibraryData.Current).GetBoolean());
            using var listed = await Run(PublishedTokens.Library.List);
            var row = Assert.Single(listed.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.LibraryData.Libraries).EnumerateArray());
            Assert.Equal(CustomRoot, row.GetProperty(PublishedTokens.LibraryData.DestinationFolder).GetString());
            Assert.Equal(SourceRoot, row.GetProperty(PublishedTokens.LibraryData.SourceFolder).GetString());
            Assert.Equal(readOnlyBefore, workspace.SnapshotHashes());

            workspace.WriteText(SourceRoot + "/" + second, body);
            using var synced = await Run(PublishedTokens.Library.Sync, LibraryId, PublishedTokens.Library.Automatic);
            CheckLink(first);
            CheckLink(second);
            Assert.Equal(settingsBytes, File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
            using var detached = await Run(PublishedTokens.Library.Detach, LibraryId, PublishedTokens.Library.Automatic);
            Assert.False(File.Exists(workspace.Combine(CustomRoot + "/" + first)));
            Assert.False(File.Exists(workspace.Combine(CustomRoot + "/" + second)));
            Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(SourceRoot + "/" + first)));
            Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Combine(SourceRoot + "/" + second)));
            using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(PublishedTokens.Settings.RelativePath)));
            Assert.Equal(CustomRoot, settings.RootElement.GetProperty(PublishedTokens.Settings.MainRootProperty).GetString());
            store.AssertNoRecoveryArtifacts(workspace.Path);
        }
        finally
        {
            foreach (var suffix in new[] { first, second })
            {
                var link = new FileInfo(workspace.Combine(CustomRoot + "/" + suffix));
                if (link.LinkTarget is not null) link.Delete();
            }
            var ownership = workspace.Combine(PublishedTokens.Ownership.RelativePath);
            if (File.Exists(ownership)) File.Delete(ownership);
        }

        void CheckLink(string suffix)
        {
            var destination = workspace.Combine(CustomRoot + "/" + suffix);
            var source = workspace.Combine(SourceRoot + "/" + suffix);
            var expected = Path.GetRelativePath(Path.GetDirectoryName(destination)!, source);
            Assert.Equal(expected, new FileInfo(destination).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(destination));
        }

        async Task<JsonDocument> Run(params string[] arguments)
        {
            var result = await PublishedProcessTestSupport.RunAsync(target, workspace.Path,
                [PublishedTokens.Library.Command, .. arguments, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], store.EnvironmentVariables);
            Assert.True(result.ExitCode == PublishedTokens.Statuses.CompleteExitCode, result.StandardOutput + result.StandardError);
            var document = JsonDocument.Parse(result.StandardOutput);
            Assert.Equal(PublishedTokens.Statuses.Complete, document.RootElement.GetProperty(PublishedTokens.Json.Status).GetString());
            return document;
        }
    }
}
