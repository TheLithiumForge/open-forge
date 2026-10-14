using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;
using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.IntegrationTests.Commands.Repair;
using OpenForge.Cli.Core.Presentation.Index;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Index;

[Trait("Feature", "index-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
public sealed class IndexCompletionIntegrationTests
{
    [Fact(DisplayName = "Index displays a proven Git navigation rename once and preserves authored rules")]
    public async Task GitNavigationRenameHasOneHumanRow()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/billing/_billing.md"] = "---\nopen-forge:\n  description: Billing\n  tags: [Guidance]\n---\n\n# Billing\n\n## Axioms\n\n- Keep exact rules.\n\n## Entries\n",
            [".agents/guidance/billing/rounding.md"] = "---\nopen-forge:\n  description: Rounding\n  tags: [Guidance]\n---\n\n# Rounding\n\nKeep amounts.\n",
        });
        var evidence = new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), new GitRenameEvidenceReader("git", fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync));
        var operation = IndexOperationFactory.Create(renameEvidence: evidence);
        await operation.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        await fixture.RecordTreeAsync();
        Directory.Move(fixture.PathFor(".agents/guidance/billing"), fixture.PathFor(".agents/guidance/invoices"));
        foreach (var preview in new[] { true, false })
        {
            var result = await operation.ExecuteAsync(new(fixture.Workspace, [], preview ? IndexMode.DryRun : IndexMode.Apply), TestContext.Current.CancellationToken);
            foreach (var detail in Enum.GetValues<CliDetail>())
            {
                var selection = new CliSelection(detail);
                var rendering = IndexPresentation.Rendering;
                var report = rendering.SelectText!(CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
                var text = CliTextRenderer.Render(report, CliTextStyle.Plain, rendering.DataTextRenderer).Content;
                Assert.StartsWith(preview ? "Would rename 1 navigation file." : "Renamed 1 navigation file.", text, StringComparison.Ordinal);
                Assert.Equal(0, Assert.Single(report.Report.Counts, count => count.Name == "entrypointsCreated").Value);
                Assert.Equal(1, Assert.Single(report.Report.Counts, count => count.Name == "navigationFilesRenamed").Value);
                Assert.Contains(preview ? "would be renamed from _billing.md, your rules would be kept" : "renamed from _billing.md, your rules were kept", text);
                Assert.DoesNotContain("deleted", text);
                Assert.DoesNotContain("entrypoint created", text);
                Assert.Equal(2, report.Report.Effects.Count(effect => effect.Kind == OpenForge.Cli.Core.Presentation.Shared.Models.CliEffectKind.File
                    && effect.Action is OpenForge.Cli.Core.Presentation.Shared.Models.CliEffectAction.Created or OpenForge.Cli.Core.Presentation.Shared.Models.CliEffectAction.Deleted));
            }
        }
        Assert.False(File.Exists(fixture.PathFor(".agents/guidance/invoices/_billing.md")));
        Assert.Contains("- Keep exact rules.", File.ReadAllText(fixture.PathFor(".agents/guidance/invoices/_invoices.md")));
    }

    [Fact(DisplayName = "Index derives Memory state tags from the completed intended chain")]
    public async Task IntendedMemoryState()
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-memory-state");
        workspace.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Memory](memory/_memory.md) - #Memory\n");
        workspace.CreateFile(".agents/memory/_memory.md", "---\ndescription: Memory\ntags: [Memory]\n---\n\n# Memory\n\n## Entries\n\n- none - No entries - #Empty\n");
        workspace.CreateFile(".agents/memory/working/team/note.md", "# Note\n");
        using var created = new CreatedFiles(workspace, ".agents/memory/working/_working.md", ".agents/memory/working/team/_team.md");
        var result = await CliHostCapture.RunAsync(["index"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        foreach (var path in new[] { ".agents/memory/working/_working.md", ".agents/memory/working/team/_team.md", ".agents/memory/working/team/note.md" })
            Assert.Contains("tags: [Memory, Working, Contextual]", File.ReadAllText(workspace.Combine(path)), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Index consumes injected folder identity proof or retains the old navigation file")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task FolderRename(bool proven, bool partial)
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-rename");
        using var created = new CreatedFiles(workspace, ".agents/guidance/invoices/_invoices.md");
        Seed(workspace);
        const string oldPath = ".agents/guidance/invoices/_billing.md";
        const string complete = "---\ndescription: Billing rules\ntags: [Guidance]\n---\n\n# Billing\n\n## Axioms\n\n- Keep rounding rules.\n\n## Entries\n\n- stale\n\n## Authored\n\nKeep exact.\n";
        var old = partial ? complete.Replace("tags: [Guidance]\n", "", StringComparison.Ordinal) : complete;
        workspace.CreateFile(oldPath, old);
        workspace.CreateFile(".agents/guidance/invoices/note.md", "# Note\n");
        var request = new IndexRequest(new(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), [], IndexMode.DryRun);
        var reader = new IndexProjectionReader(new PhysicalPathResolver(), proven ? new RenameEvidence() : null);
        var preview = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        var projection = preview.Projection ?? throw new InvalidOperationException();
        var plan = new IndexPlanBuilder().Build(new() { Request = request, Projection = projection });
        Assert.Equal(proven, plan.Updates.Any(change => change.Kind == PlannedFileChangeKind.Delete));
        Assert.Equal(proven ? 0 : 1, projection.Maintenance?.OldNavigation.Length);
        Assert.Equal(old, File.ReadAllText(workspace.Combine(oldPath)));
        var result = await IndexOperationFactory.Create(renameEvidence: proven ? new RenameEvidence() : null).ExecuteAsync(
            new(request.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(OpenForge.Cli.Core.Shell.Definitions.CliSemanticStatus.Complete, result.Status);
        Assert.Equal(!proven, File.Exists(workspace.Combine(oldPath)));
        var canonical = File.ReadAllText(workspace.Combine(".agents/guidance/invoices/_invoices.md"));
        Assert.Contains(proven ? "- Keep rounding rules." : "- inherited - No local axioms; loaded ancestor axioms remain active.", canonical, StringComparison.Ordinal);
        if (proven) Assert.EndsWith("## Authored\n\nKeep exact.\n", canonical, StringComparison.Ordinal);
        if (partial)
        {
            Assert.Contains("tags: [Guidance]", canonical, StringComparison.Ordinal);
            Assert.Contains(result.Metadata, item => item.Path == ".agents/guidance/invoices/_invoices.md");
        }
    }

    private sealed class RenameEvidence : IRoutedMaintenanceRenameEvidence
    {
        public ValueTask<RouteScaffoldRenameProof?> ReadAsync(CliWorkspace workspace, RouteScaffoldRenameCandidate candidate, CancellationToken cancellationToken)
            => ValueTask.FromResult<RouteScaffoldRenameProof?>(candidate.OldSource is null ? null : new()
            {
                OldCanonicalPath = ".agents/guidance/billing/_billing.md",
                CurrentCanonicalPath = candidate.OldSource.Identity.CanonicalBasePath,
                CurrentContentHash = candidate.OldSnapshot?.ContentHash ?? throw new InvalidOperationException(),
                Basis = RouteScaffoldRenameBasis.CommittedContent,
            });
        public ValueTask<bool> VerifyAsync(CliWorkspace workspace, RouteScaffoldRenameProof proof, CancellationToken cancellationToken) => ValueTask.FromResult(true);
    }

    [Fact(DisplayName = "Index coalesces a partial entrypoint with Entries and guards original metadata and settings")]
    public async Task CoalescesAndGuardsOriginals()
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-coalesce");
        Seed(workspace);
        const string root = ".agents/guidance/_guidance.md";
        const string authored = "---\ndescription: Authored words # keep comment\ncustom: kept\n---\n\n# Guidance\n\n## Entries\n\n- stale\n\n## Authored\n\nKeep my body.\n";
        workspace.ReplaceText(root, authored);
        workspace.CreateFile(".agents/guidance/note.md", "# Note\nKeep this note.\n");
        var request = new IndexRequest(new(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), [], IndexMode.Apply);
        var reader = new IndexProjectionReader(new PhysicalPathResolver());
        var read = await reader.ReadAsync(request, TestContext.Current.CancellationToken);
        var projection = read.Projection ?? throw new InvalidOperationException();
        var plan = new IndexPlanBuilder().Build(new() { Request = request, Projection = projection });
        var effect = Assert.Single(plan.Updates, change => change.LogicalPath == workspace.Combine(root));
        Assert.Equal(OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files.PlannedFileChangeKind.Replace, effect.Kind);
        Assert.Equal(3, plan.RecoveryTargets.Count);
        Assert.Single(plan.RecoveryTargets, target => target.Change.LogicalPath == workspace.Combine(root));
        workspace.CreateFile(".agents/open-forge.json", "{\"schemaVersion\":1,\"frontmatter\":\"root\"}");
        Assert.False(await reader.VerifyOwnershipAsync(plan, TestContext.Current.CancellationToken));
        var apply = await CliHostCapture.RunAsync(["index"], workspace.Path);
        Assert.True(apply.ExitCode == 0, apply.Output + apply.Error);
        var final = File.ReadAllText(workspace.Combine(root));
        Assert.Contains("description: Authored words # keep comment\ncustom: kept\n", final, StringComparison.Ordinal);
        Assert.EndsWith("## Authored\n\nKeep my body.\n", final, StringComparison.Ordinal);
        Assert.Contains("[Note](note.md) - #Guidance", final, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Malformed entrypoints leave dependent navigation exact while independent roots complete")]
    public async Task MalformedEntrypointLeavesDependents()
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-bad-entrypoint");
        Seed(workspace);
        const string bad = "---\ntags: 42\n---\n\n# Guidance\n\n## Entries\n\n- stale\n";
        workspace.ReplaceText(".agents/guidance/_guidance.md", bad);
        workspace.CreateFile(".agents/guidance/team/note.md", "# Dependent\nKeep untouched.\n");
        workspace.CreateFile(".agents/maps/_maps.md", "---\ndescription: Maps\ntags: [Map]\n---\n\n# Maps\n\n## Entries\n\n- stale\n");
        workspace.CreateFile(".agents/maps/note.md", "# Independent\n");
        var result = await CliHostCapture.RunAsync(["index", "--format", "json"], workspace.Path);
        Assert.Equal(3, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal(bad, File.ReadAllText(workspace.Combine(".agents/guidance/_guidance.md")));
        Assert.Equal("# Dependent\nKeep untouched.\n", File.ReadAllText(workspace.Combine(".agents/guidance/team/note.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/guidance/team/_team.md")));
        Assert.Contains("[Independent](note.md) - #Map", File.ReadAllText(workspace.Combine(".agents/maps/_maps.md")), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Index excludes unknown removed empty package and overwrite-only chains")]
    public async Task ExcludesChains()
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-exclusions");
        Seed(workspace);
        workspace.CreateFile(".agents/open-forge.json", "{\"schemaVersion\":1,\"removedDirectories\":[\".agents/guidance/removed\"]}");
        workspace.CreateFile(".agents/unknown/deep/note.md", "# Unknown\n");
        workspace.CreateFile(".agents/guidance/removed/deep/note.md", "# Removed\n");
        workspace.CreateFile(".agents/guidance/native/SKILL.md", "---\nname: native\ndescription: Native\n---\n\n# Native\n");
        workspace.CreateFile(".agents/guidance/native/references/deep/note.md", "# Package resource\n");
        workspace.CreateFile(".agents/guidance/orphan/deep/note.overwrite.md", "# Overwrite\n");
        workspace.CreateFile(".agents/guidance/resources/deep/file.txt", "Resource\n");
        var excluded = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["index"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        foreach (var path in excluded.Keys.Where(path => path.EndsWith("note.md", StringComparison.Ordinal)))
            Assert.Equal(excluded[path], workspace.SnapshotHashes()[path]);
        Assert.False(File.Exists(workspace.Combine(".agents/guidance/native/references/_references.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/guidance/removed/_removed.md")));
        Assert.False(File.Exists(workspace.Combine(".agents/unknown/_unknown.md")));
    }

    [Theory(DisplayName = "Index previews and completes a selected deep plain note in one final plan")]
    [InlineData("scoped")]
    [InlineData("root")]
    public async Task DeepSelectedNote(string form)
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-deep");
        using var created = new CreatedFiles(workspace, ".agents/guidance/team/_team.md", ".agents/guidance/team/deep/_deep.md");
        Seed(workspace);
        workspace.CreateFile(".agents/open-forge.json", $"{{\"schemaVersion\":1,\"frontmatter\":\"{form}\"}}");
        const string path = ".agents/guidance/team/deep/note.md";
        const string body = "# Release notes\r\n\r\nKeep my words.\r\n";
        workspace.CreateFile(path, body);
        workspace.CreateFile(".agents/guidance/unselected/deep/note.md", "# Other\n");
        var before = workspace.SnapshotHashes();
        var direct = await new IndexProjectionReader(new PhysicalPathResolver()).ReadAsync(new(new CliWorkspace(workspace.Path, workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace), [path], IndexMode.DryRun), TestContext.Current.CancellationToken);
        Assert.NotNull(direct.Projection);
        var plan = new IndexPlanBuilder().Build(new() { Request = new(new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), [path], IndexMode.DryRun), Projection = direct.Projection });
        Assert.Equal(plan.Updates.Count, plan.Updates.Select(item => item.LogicalPath).Distinct(StringComparer.Ordinal).Count());
        var preview = await CliHostCapture.RunAsync(["index", path, "--dry-run", "--format", "json"], workspace.Path);
        Assert.Equal(0, preview.ExitCode);
        using (var json = JsonDocument.Parse(preview.Output))
        {
            Assert.Equal("Would create 2 entrypoints and complete metadata in 1 file.", json.RootElement.GetProperty("summary").GetProperty("headline").GetString());
            Assert.Single(json.RootElement.GetProperty("data").GetProperty("metadata").EnumerateArray());
            Assert.Equal(2, json.RootElement.GetProperty("data").GetProperty("entrypoints").GetArrayLength());
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        var apply = await CliHostCapture.RunAsync(["index", path], workspace.Path);
        Assert.Equal(0, apply.ExitCode);
        Assert.Contains("Created 2 entrypoints and completed metadata in 1 file.", apply.Output, StringComparison.Ordinal);
        Assert.Contains("metadata completed, your content was kept", apply.Output, StringComparison.Ordinal);
        Assert.Contains("entrypoint created", apply.Output, StringComparison.Ordinal);
        Assert.EndsWith(body, File.ReadAllText(workspace.Combine(path)), StringComparison.Ordinal);
        Assert.Contains("[Release notes](note.md) - #Guidance", File.ReadAllText(workspace.Combine(".agents/guidance/team/deep/_deep.md")), StringComparison.Ordinal);
        Assert.Contains("[Notes in guidance/team](team/_team.md) - #Guidance", File.ReadAllText(workspace.Combine(".agents/guidance/_guidance.md")), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(".agents/guidance/unselected/_unselected.md")));
        var completed = workspace.SnapshotHashes();
        var repeat = await CliHostCapture.RunAsync(["index", path], workspace.Path);
        Assert.Equal(0, repeat.ExitCode);
        Assert.Contains("Nothing to do.", repeat.Output, StringComparison.Ordinal);
        Assert.Equal(completed, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Malformed ordinary metadata stays exact and admits same-parent and independent Index work")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task MalformedContinuation(bool sibling, bool deep)
    {
        using var workspace = TemporaryWorkspace.Create("index-completion-malformed");
        Seed(workspace);
        const string bad = "---\nopen-forge:\n  description: [unclosed\n";
        var path = deep ? ".agents/guidance/team/deep/bad.md" : ".agents/guidance/bad.md";
        using var created = new CreatedFiles(workspace, ".agents/guidance/team/_team.md", ".agents/guidance/team/deep/_deep.md");
        workspace.CreateFile(path, bad);
        if (sibling) workspace.CreateFile(".agents/guidance/good.md", "# Good\nKeep body.\n");
        var args = deep ? new[] { "index", path, "--format", "json", "--detail", "standard" } : ["index", "--format", "json", "--detail", "standard"];
        var result = await CliHostCapture.RunAsync(args, workspace.Path);
        Assert.Equal(3, result.ExitCode);
        using var json = JsonDocument.Parse(result.Output);
        var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Equal("index.metadata-skipped", finding.GetProperty("code").GetString());
        Assert.Equal($"{path} has invalid frontmatter. It was left unchanged. Other files were indexed.", finding.GetProperty("message").GetString());
        Assert.Contains($"Fix the frontmatter in {path}, then run open-forge index.", json.RootElement.GetProperty("next").GetProperty("reason").GetString(), StringComparison.Ordinal);
        Assert.Equal(bad, File.ReadAllText(workspace.Combine(path)));
        var parent = File.ReadAllText(workspace.Combine(".agents/guidance/_guidance.md"));
        Assert.DoesNotContain("bad.md", parent, StringComparison.Ordinal);
        if (sibling) Assert.Contains("[Good](good.md) - #Guidance", parent, StringComparison.Ordinal);
    }

    internal static void Seed(TemporaryWorkspace workspace)
    {
        workspace.CreateFile(".agents/loader.md", "# Loader\n\n## Entries\n\n- [Guidance](guidance/_guidance.md) - #Guidance\n");
        workspace.CreateFile(".agents/guidance/_guidance.md", "---\nopen-forge:\n  description: Guidance\n  tags: [Guidance]\n---\n\n# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- none - No entries - #Empty\n");
    }

    private sealed class CreatedFiles(TemporaryWorkspace workspace, params string[] paths) : IDisposable
    {
        public void Dispose()
        {
            foreach (var path in paths.Select(path => workspace.Combine(path)))
            {
                if (!File.Exists(path)) continue;
                Assert.Equal((FileAttributes)0, File.GetAttributes(path) & (FileAttributes.Directory | FileAttributes.ReparsePoint));
                File.Delete(path);
            }
        }
    }
}
