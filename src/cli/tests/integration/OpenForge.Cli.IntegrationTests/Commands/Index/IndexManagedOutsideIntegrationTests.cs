using System.Text;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class IndexManagedOutsideIntegrationTests
{
    [Theory(DisplayName = "Readonly managed metadata acquisition retains byte expectations for lease validation")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ManagedMetadataChangesUnderLease(string mainRoot)
    {
        using var fixture = new Fixture(mainRoot, outside: true, managedEntrypoint: false);
        var request = new IndexRequest(fixture.Workspace, [SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor], IndexMode.DryRun);
        var reader = new OpenForge.Cli.Core.Commands.Index.Shared.Projection.IndexProjectionReader(
            new OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.PhysicalPathResolver());
        var projection = (await reader.ReadAsync(request, TestContext.Current.CancellationToken)).Projection ?? throw new InvalidOperationException();
        var plan = new OpenForge.Cli.Core.Commands.Index.Shared.Planning.IndexPlanBuilder().Build(new()
        { Request = request, Projection = projection, LayoutRead = projection.LayoutRead ?? throw new InvalidOperationException() });
        var acquired = await OpenForge.Cli.Core.Framework.Mutation.Locking.WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new(fixture.Workspace, IndexDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Locking.Models.WorkspaceLockLease>(acquired.Lease);
        Assert.True(await reader.VerifyMaintenanceAsync(plan, TestContext.Current.CancellationToken));
        fixture.Files.ReplaceText(fixture.Leaf, File.ReadAllText(fixture.Files.Combine(fixture.Leaf)) + "\nAn independent managed edit.\n");
        Assert.False(await reader.VerifyMaintenanceAsync(plan, TestContext.Current.CancellationToken));
    }

    [Theory(DisplayName = "Managed leaf metadata remains readable for parent and sibling navigation in primary and outside trees")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", false), InlineData("knowledge/main", true)]
    public async Task ManagedLeaf(string mainRoot, bool sibling)
    {
        foreach (var outside in new[] { false, true })
        {
            using var fixture = new Fixture(mainRoot, outside, managedEntrypoint: false);
            var before = File.ReadAllBytes(fixture.Files.Combine(fixture.Leaf));
            var selected = sibling ? fixture.Sibling : fixture.Anchor;
            var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
                [SourceWorkspaceRelativePath.LiteralReferencePrefix + selected], IndexMode.Apply), TestContext.Current.CancellationToken);
            Assert.True(result.Status == CliSemanticStatus.Complete,
                $"outside={outside}: {string.Join("\n", result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}"))}");
            Assert.Equal(before, File.ReadAllBytes(fixture.Files.Combine(fixture.Leaf)));
            Assert.DoesNotContain(result.Metadata, row => row.Path == fixture.Leaf);
            var document = new MarkdownDocumentParser().Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
            Assert.Contains(SourceGeneratedEntriesParser.Parse(document).Entries,
                row => row.Destination == Path.GetFileName(fixture.Leaf));
        }
    }

    [Theory(DisplayName = "Managed recognized entrypoints retain authored bytes while their existing Entries region is maintained")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ManagedEntrypoint(string mainRoot)
    {
        foreach (var outside in new[] { false, true })
        {
            using var fixture = new Fixture(mainRoot, outside, managedEntrypoint: true);
            var parser = new MarkdownDocumentParser();
            var before = parser.Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
            var beforeSpan = before.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
            var result = await IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace,
                [SourceWorkspaceRelativePath.LiteralReferencePrefix + fixture.Anchor], IndexMode.Apply), TestContext.Current.CancellationToken);
            Assert.True(result.Status == CliSemanticStatus.Complete,
                $"outside={outside}: {string.Join("\n", result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}"))}");
            Assert.DoesNotContain(result.Metadata, row => row.Path == fixture.Anchor);
            var after = parser.Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
            var afterSpan = after.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
            Assert.Equal(Encoding.UTF8.GetBytes(before.Source[..beforeSpan.Start]), Encoding.UTF8.GetBytes(after.Source[..afterSpan.Start]));
            Assert.Equal(Encoding.UTF8.GetBytes(before.Source[beforeSpan.End..]), Encoding.UTF8.GetBytes(after.Source[afterSpan.End..]));
            Assert.Equal(2, SourceGeneratedEntriesParser.Parse(after).Entries.Count);
        }
    }

    private sealed class Fixture : IDisposable
    {
        internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("index-managed-comparison");
        internal readonly CliWorkspace Workspace;
        internal readonly string Anchor;
        internal readonly string Leaf;
        internal readonly string Sibling;

        internal Fixture(string mainRoot, bool outside, bool managedEntrypoint)
        {
            Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
            var directory = mainRoot + "/" + WorkspaceCategoryNames.Guidance;
            var category = RoutedMetadataCategory.Guidance;
            if (outside)
            {
                directory = "docs/team";
                category = RoutedMetadataCategory.Workspace;
            }
            Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(directory);
            Leaf = directory + "/managed" + SourceFormClassifier.MarkdownExtension;
            Sibling = directory + "/ordinary" + SourceFormClassifier.MarkdownExtension;
            Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
                $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{(outside ? "\"" + directory + "\"" : "")}}]}""");
            var destination = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
            Files.CreateFile(mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
                GeneratedLoaderDocumentBuilder.Build($"- [Parent]({destination}) - #Authored") + "\n");
            Files.CreateFile(Anchor, Document("Parent", GeneratedLoaderDocumentBuilder.Build(MarkdownEntriesSectionReader.EmptyEntry)
                + "\n\n## Authored\n\nKeep these exact rules.\n"));
            Files.CreateFile(Leaf, Document("Managed note", "# Managed note\n\nKeep the complete managed file.\n"));
            Files.CreateFile(Sibling, Document("Ordinary note", "# Ordinary note\n"));
            var ownership = WorkspaceOwnershipDocument.Empty with
            {
                Extensions = [new("fixture-owner", null, null, [], [managedEntrypoint ? Anchor : Leaf], [])],
            };
            Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));

            string Document(string description, string body)
                => OpenForgeDocumentSeed.Metadata(description, RoutedMetadataDefaults.Tags(category), body);
        }

        public void Dispose() => Files.Dispose();
    }
}
