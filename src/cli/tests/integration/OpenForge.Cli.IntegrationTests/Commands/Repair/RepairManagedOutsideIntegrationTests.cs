using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RepairManagedOutsideIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Readonly navigation metadata does not authorize Repair of managed authored sources or targets")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData("knowledge/main", false), InlineData("knowledge/main", true)]
    public async Task AuthoredProtection(string mainRoot, bool target)
    {
        foreach (var outside in new[] { false, true })
        {
            using var fixture = new Fixture(mainRoot, outside, managedEntrypoint: false);
            fixture.Files.ReplaceBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("fixture", null), [target ? fixture.Target : fixture.Source], []),
            }));
            var before = fixture.Files.SnapshotHashes(includeRuntime: true);
            foreach (var mode in new[] { RepairMode.DryRun, RepairMode.Apply })
            {
                var result = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, mode, true, [], false), Token);
                Assert.Empty(result.Plan?.FileChanges ?? []);
                Assert.Contains(result.Findings, finding => finding.Protection);
                Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
            }
        }
    }

    [Theory(DisplayName = "Readonly outside Repair metadata retains exact state through verification under the workspace lease")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ManagedMetadataChangesUnderLease(string mainRoot)
    {
        using var fixture = new Fixture(mainRoot, outside: true, managedEntrypoint: false);
        var preview = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
        var plan = preview.Plan ?? throw new InvalidOperationException();
        Assert.False(plan.IsBlocked);
        Assert.NotEmpty(plan.Maintenance?.ReadExpectations ?? []);
        var acquired = await OpenForge.Cli.Core.Framework.Mutation.Locking.WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new(fixture.Workspace, RepairDefinitions.CommandIdentity, Guid.NewGuid()), Token);
        await using var lease = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Locking.Models.WorkspaceLockLease>(acquired.Lease);
        var planner = new OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairMaintenancePlanner(
            new OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.PhysicalPathResolver());
        Assert.True(await OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairPlanRevalidator.ValidateLayoutAsync(plan, lease, Token));
        Assert.True(await planner.VerifyAsync(plan, Token));
        fixture.Files.ReplaceText(fixture.Leaf, File.ReadAllText(fixture.Files.Combine(fixture.Leaf)) + "\nAn independent managed edit.\n");
        Assert.False(await planner.VerifyAsync(plan, Token));
    }

    [Theory(DisplayName = "Repair reads managed sibling metadata without completing or changing the sibling in primary and outside trees")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ManagedLeaf(string mainRoot)
        => await CompareAsync(mainRoot, managedEntrypoint: false);

    [Theory(DisplayName = "Repair maintains existing managed entrypoint Entries while preserving authored bytes in primary and outside trees")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/main")]
    public async Task ManagedEntrypoint(string mainRoot)
        => await CompareAsync(mainRoot, managedEntrypoint: true);

    private static async Task CompareAsync(string mainRoot, bool managedEntrypoint)
    {
        foreach (var outside in new[] { false, true })
        {
            using var fixture = new Fixture(mainRoot, outside, managedEntrypoint);
            var before = fixture.Files.SnapshotHashes(includeRuntime: true);
            var managedPath = managedEntrypoint ? fixture.Anchor : fixture.Leaf;
            var managedBytes = File.ReadAllBytes(fixture.Files.Combine(managedPath));
            var parser = new MarkdownDocumentParser();
            var original = parser.Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
            var originalSpan = original.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
            var operation = new RepairOperation();
            var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
            Assert.True(preview.Status == CliSemanticStatus.Complete,
                $"outside={outside}: {string.Join("\n", preview.Findings.Select(finding => $"{finding.Code}: {finding.Cause}"))}");
            Assert.NotNull(preview.Plan?.Maintenance);
            Assert.DoesNotContain(preview.Plan.Maintenance.Intent.Metadata, row => row.Path == managedPath);
            Assert.DoesNotContain(preview.Plan.Maintenance.Intent.Originals.Keys, path => path == managedPath);
            if (outside) Assert.NotEmpty(preview.Plan.Maintenance.ReadExpectations);
            else Assert.Empty(preview.Plan.Maintenance.ReadExpectations);
            Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
            var applied = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), Token);
            Assert.True(applied.Status == CliSemanticStatus.Complete,
                $"outside={outside}: {string.Join("\n", applied.Findings.Select(finding => $"{finding.Code}: {finding.Cause}"))}");
            var current = parser.Parse(File.ReadAllText(fixture.Files.Combine(fixture.Anchor)));
            var currentSpan = current.GeneratedRegion.EntriesBlock?.Span ?? throw new InvalidOperationException();
            Assert.Equal(Encoding.UTF8.GetBytes(original.Source[..originalSpan.Start]), Encoding.UTF8.GetBytes(current.Source[..currentSpan.Start]));
            Assert.Equal(Encoding.UTF8.GetBytes(original.Source[originalSpan.End..]), Encoding.UTF8.GetBytes(current.Source[currentSpan.End..]));
            Assert.Equal(3, SourceGeneratedEntriesParser.Parse(current).Entries.Count);
            if (!managedEntrypoint) Assert.Equal(managedBytes, File.ReadAllBytes(fixture.Files.Combine(managedPath)));
            var source = parser.Parse(File.ReadAllText(fixture.Files.Combine(fixture.Source)));
            Assert.Equal(Fixture.SourceBody.Replace("./guide.md", "guide.md", StringComparison.Ordinal), source.Source[source.Source.IndexOf("# Note", StringComparison.Ordinal)..]);
            var after = fixture.Files.SnapshotHashes();
            var repeat = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), Token);
            Assert.Empty(repeat.Plan?.FileChanges ?? []);
            Assert.Equal(after, fixture.Files.SnapshotHashes());
        }
    }

    private sealed class Fixture : IDisposable
    {
        internal const string SourceBody = "# Note\n\nRead [Guide](./guide.md).\n\nKeep independent authored bytes.\n";
        internal readonly TemporaryWorkspace Files = TemporaryWorkspace.Create("repair-managed-comparison");
        internal readonly CliWorkspace Workspace;
        internal readonly string Anchor;
        internal readonly string Leaf;
        internal readonly string Source;
        internal readonly string Target;

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
            Leaf = directory + "/managed.md";
            Source = directory + "/note.md";
            Target = directory + "/guide.md";
            Files.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
                $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{(outside ? "\"" + directory + "\"" : "")}}]}""");
            Files.CreateFile(WorkspaceLayoutNames.AgentHostFileName, $"# Workspace\n\nRead `{mainRoot}/{WorkspaceLayoutNames.LoaderFileName}`.\n");
            var destination = Path.GetRelativePath(mainRoot, Anchor).Replace(Path.DirectorySeparatorChar, '/');
            Files.CreateFile(mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
                Entries("# Workspace Loader\n\n## Axioms\n\n- Preserve authored content.", $"- [Parent]({destination}) - #Authored"));
            Files.CreateFile(Anchor, Document("Parent", Entries("# Parent\n\n## Axioms\n\n- Keep authored rules.", MarkdownEntriesSectionReader.EmptyEntry)));
            Files.CreateFile(Leaf, Document("Managed note", "# Managed note\n\nKeep this managed file.\n"));
            Files.CreateFile(Source, Document("Note", SourceBody));
            Files.CreateFile(Target, Document("Guide", "# Guide\n"));
            Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("fixture", null), [managedEntrypoint ? Anchor : Leaf], []),
            }));

            string Document(string description, string body)
                => OpenForgeDocumentSeed.Metadata(description, RoutedMetadataDefaults.Tags(category), body);

            static string Entries(string prefix, string entries)
                => OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed { Prefix = prefix, Entries = entries });
        }

        public void Dispose() => Files.Dispose();
    }
}
