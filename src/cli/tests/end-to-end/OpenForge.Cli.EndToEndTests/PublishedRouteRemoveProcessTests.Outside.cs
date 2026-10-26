using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteRemoveProcessTests
{
    [Theory]
    [InlineData(PublishedTokens.Sources.AgentsRoot, false)]
    [InlineData("knowledge/main", true)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "EndToEnd")]
    public async Task OutsideRouteAndRootCompositionPreservesAuthoredPolicy(string root, bool compatibility)
    {
        var executable = PublishedExecutableTarget.Discover();
        using var files = TemporaryWorkspace.Create("published-remove-outside");
        using var locks = PublishedWorkspaceLockStore.Create("published-remove-outside-locks");
        locks.Track(files.Path);
        const string directory = "docs/team";
        var anchor = directory + "/" + (compatibility ? PublishedTokens.Layout.IndexFile : "_team" + PublishedTokens.Layout.MarkdownExtension);
        var leaf = directory + "/old" + PublishedTokens.Layout.MarkdownExtension;
        var keep = directory + "/keep" + PublishedTokens.Layout.MarkdownExtension;
        var loader = root + "/" + PublishedTokens.Layout.LoaderFileName;
        const string removedRow = "- [Old policy](old.md) - #LoadNow #Old - applies to `old/**`\r\n";
        const string survivingRows = "- [Keep one](keep.md) - #Authored - applies to `src/**`\r\n- [Keep two](keep.md) - #Other - applies to `test/**`\r\n";
        var parent = OpenForgeDocumentSeed.Metadata("Parent", ["Fixture"], OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Prefix = "# Independent parent\r\n\r\nPreserve authored policy.",
            Entries = removedRow + survivingRows,
            LineEnding = "\r\n",
        }));
        files.WriteText(PublishedTokens.Settings.RelativePath,
            $$"""{"{{PublishedTokens.Settings.SchemaVersionProperty}}":{{PublishedTokens.Settings.SchemaVersion}},"{{PublishedTokens.Settings.MainRootProperty}}":"{{root}}","{{PublishedTokens.Settings.OutsideRoutesProperty}}":["{{directory}}"]}""");
        files.WriteText(loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        files.WriteText(anchor, parent);
        files.WriteText(leaf, OpenForgeDocumentSeed.Metadata("Old", ["Fixture"], "# Old\r\n"));
        files.WriteText(keep, OpenForgeDocumentSeed.Metadata("Keep", ["Fixture"], "# Keep\r\n"));
        var loaderBefore = File.ReadAllBytes(files.Combine(loader));
        var keepBefore = File.ReadAllBytes(files.Combine(keep));
        try
        {
            var before = files.SnapshotHashes();
            var preview = await PublishedProcessTestSupport.RunAsync(executable, files.Path,
                [PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, PublishedTokens.Layout.LiteralReferencePrefix + leaf,
                    PublishedTokens.RouteRemove.DryRun, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], locks.EnvironmentVariables);
            Assert.True(preview.ExitCode == 0, preview.StandardOutput + preview.StandardError);
            Assert.Equal(before, files.SnapshotHashes());
            locks.AssertNoInfrastructure();
            var applied = await PublishedProcessTestSupport.RunAsync(executable, files.Path,
                [PublishedTokens.Remove.Command, PublishedTokens.Layout.LiteralReferencePrefix + leaf, PublishedTokens.Remove.Automatic,
                    PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], locks.EnvironmentVariables);
            Assert.True(applied.ExitCode == 0, applied.StandardOutput + applied.StandardError);
            using (var report = JsonDocument.Parse(applied.StandardOutput))
            {
                Assert.Equal(PublishedTokens.Remove.Command, report.RootElement.GetProperty(PublishedTokens.Json.Command).GetString());
                Assert.Contains(report.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray(),
                    item => item.GetProperty(PublishedTokens.Json.Path).GetString() == leaf);
            }
            Assert.False(File.Exists(files.Combine(leaf)));
            Assert.Equal(parent.Replace(removedRow, string.Empty, StringComparison.Ordinal), File.ReadAllText(files.Combine(anchor)));
            Assert.Equal(loaderBefore, File.ReadAllBytes(files.Combine(loader)));
            Assert.Equal(keepBefore, File.ReadAllBytes(files.Combine(keep)));
            var after = files.SnapshotHashes();
            var repeated = await PublishedProcessTestSupport.RunAsync(executable, files.Path,
                [PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, PublishedTokens.Layout.LiteralReferencePrefix + leaf,
                    PublishedTokens.RouteRemove.Automatic, PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat], locks.EnvironmentVariables);
            Assert.Equal(0, repeated.ExitCode);
            using var noOp = JsonDocument.Parse(repeated.StandardOutput);
            Assert.Empty(noOp.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
            Assert.Equal(after, files.SnapshotHashes());
        }
        finally
        {
            locks.RemoveRecoveryArtifacts(files.Path);
        }
    }
}
