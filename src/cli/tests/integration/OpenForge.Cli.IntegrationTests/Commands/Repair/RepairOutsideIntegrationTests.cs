using System.Text;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RepairOutsideIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    internal const string SourceBody = "# Note\n\nRead [Guide](./guide.md).\n\nIndependent authored bytes.\n";
    internal static readonly string Target = OutsideMaintenanceWorkspace.Route + "/nested/guide.md";

    [Theory(DisplayName = "Known unrelated absence permits selected Repair while a wrong-kind registration refuses writes")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false), InlineData(WorkspaceLayout.DefaultMainRoot, true, false)]
    [InlineData(OutsideMaintenanceWorkspace.CustomRoot, false, false), InlineData(OutsideMaintenanceWorkspace.CustomRoot, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true), InlineData(OutsideMaintenanceWorkspace.CustomRoot, false, true)]
    public async Task UnrelatedRegistration(string mainRoot, bool wrongKind, bool authoredRows)
    {
        using var fixture = Create(mainRoot);
        const string unrelated = "unavailable";
        RegisterUnrelated(fixture, mainRoot, unrelated);
        if (wrongKind) fixture.Files.CreateFile(unrelated, OutsideMaintenanceWorkspace.Body);
        var destination = Path.GetRelativePath(mainRoot, OutsideMaintenanceWorkspace.Note).Replace('\\', '/');
        var alias = destination.Replace("/note.md", "/./note.md", StringComparison.Ordinal);
        if (authoredRows)
            fixture.Files.ReplaceText(fixture.Loader, GeneratedLoaderDocumentBuilder.Build(
                $"- [First authored policy]({destination}) - #First - applies to `src/**`\n"
                + $"- [Second authored policy]({alias}) - #Second - applies to `tests/**`"));
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var settings = fixture.Read(WorkspaceSettingsDefinitions.RelativePath);
        var loader = fixture.Read(fixture.Loader);
        var other = fixture.Read(OutsideMaintenanceWorkspace.OtherNote);
        var expectedPaths = new[] { OutsideMaintenanceWorkspace.Note, Target, OutsideMaintenanceWorkspace.Anchor,
            WorkspaceRoutePath.CanonicalEntrypointPath(SourceLogicalPath.ReadParent(Target)) }.Order(StringComparer.Ordinal).ToArray();
        if (authoredRows)
        {
            var components = RepairOperationFactory.CreateDefaultComponents();
            var diagnosis = await components.DiagnosisReader.ReadAsync(new DoctorRequest(fixture.Workspace), Token);
            var read = await components.CatalogueReader.ReadAsync(diagnosis.Observation.LayoutRead!, diagnosis.Observation.LocalReferences, Token);
            var included = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(read.Catalogue!.Layout, Token);
            Assert.NotSame(read.LayoutRead!.Layout, included.Catalogue.Layout);
            var rowPlan = RepairPlanner.Build(new(fixture.Workspace, RepairMode.DryRun, true, [], false), read.Proposals);
            var composed = await components.MaintenancePlanner!.CompleteAsync(rowPlan, read with { Catalogue = included.Catalogue }, Token);
            Assert.False(composed.IsBlocked);
            var retained = Assert.IsType<RepairMaintenancePlan>(composed.Maintenance).Intent.Formation.Catalogue;
            Assert.Same(read.LayoutRead.Layout, retained.Layout);
            Assert.Equal(included.Catalogue.IncludedRows, retained.IncludedRows);
            Assert.Equal(included.Catalogue.Aliases, retained.Aliases);
            Assert.Same(included.Catalogue.FindByPath(OutsideMaintenanceWorkspace.Note), retained.FindByPath(OutsideMaintenanceWorkspace.Note));
            var rows = retained.IncludedRows.Where(row => row.ParentPath == fixture.Loader).ToArray();
            Assert.Equal(new[] { destination, alias }, rows.Select(row => row.Entry.Destination));
            Assert.Equal(new[] { "First authored policy", "Second authored policy" }, rows.Select(row => row.Entry.Description));
            Assert.Equal(new[] { "First", "Second" }, rows.Select(row => Assert.Single(row.Entry.Tags)));
            Assert.Equal(new[] { "src/**", "tests/**" }, rows.Select(row => Assert.Single(row.Entry.ApplyTo).Text));
            Assert.All(rows, row => Assert.Equal(OutsideMaintenanceWorkspace.Note, row.Path));
            Assert.Same(rows[0].Entry, retained.FindByPath(OutsideMaintenanceWorkspace.Note)!.IncludedEntry);
            Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        }
        foreach (var mode in new[] { RepairMode.DryRun, RepairMode.Apply })
        {
            var result = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, mode, true, [], false), Token);
            Assert.Equal(wrongKind ? CliSemanticStatus.Blocked : CliSemanticStatus.Complete, result.Status);
            Assert.Equal(wrongKind ? RepairCoverageState.Blocked : RepairCoverageState.Complete, result.Diagnosis.SelectedScope);
            Assert.DoesNotContain(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisIncomplete);
            var plan = Assert.IsType<RepairPlan>(result.Plan);
            var selected = Assert.Single(plan.Selection.Selected);
            Assert.Equal(OutsideMaintenanceWorkspace.Note, selected.Proposal.SourceCanonicalPath);
            var effect = Assert.Single(plan.Effects);
            Assert.Equal(OutsideMaintenanceWorkspace.Note, effect.SourceCanonicalPath);
            var change = Assert.Single(effect.Changes);
            Assert.Equal("./guide.md", change.ExpectedDestination);
            Assert.Equal("guide.md", change.IntendedDestination);
            var observation = Assert.Single(plan.LayoutRead!.OutsideRoutes, route => route.Descriptor.DirectoryPath == unrelated);
            Assert.Equal(wrongKind ? WorkspaceOutsideRouteReadState.Unsafe : WorkspaceOutsideRouteReadState.Missing, observation.State);
            if (wrongKind)
            {
                Assert.Single(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisBlocked);
                Assert.Equal(RepairPreflightState.NotRequested, result.Preflight.State);
                Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
                Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
                Assert.Equal(Encoding.UTF8.GetBytes(OutsideMaintenanceWorkspace.Body), fixture.Read(unrelated));
            }
            else
            {
                Assert.DoesNotContain(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisBlocked);
                Assert.False(plan.IsBlocked);
                Assert.Equal(expectedPaths, plan.FileChanges.Select(file => Path.GetRelativePath(fixture.Files.Path, file.LogicalPath)
                    .Replace('\\', '/')).Order(StringComparer.Ordinal));
                var maintenance = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairMaintenancePlan>(plan.Maintenance);
                var catalogue = maintenance.Intent.Formation.Catalogue;
                Assert.Same(plan.LayoutRead.Layout, catalogue.Layout);
                Assert.Equal(new[] { OutsideMaintenanceWorkspace.Route, OutsideMaintenanceWorkspace.Other, unrelated },
                    catalogue.Layout.OutsideRoutes.Select(route => route.DirectoryPath));
                Assert.Equal(RepairPreflightState.Ready, result.Preflight.State);
                if (mode == RepairMode.DryRun)
                {
                    Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
                    Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
                }
                else
                {
                    Assert.Equal(RepairApplicationState.Applied, result.Application.State);
                    Assert.Equal(RepairVerificationState.Verified, result.Verification.ResultingBytes);
                    Assert.Equal(RepairVerificationState.Verified, result.Verification.Targets);
                    Assert.Equal(RepairPostDiagnosisState.Complete, result.PostDiagnosis.State);
                    Assert.Equal(1, result.Counts.AppliedEffects);
                    var after = fixture.Files.SnapshotHashes();
                    Assert.Equal(expectedPaths, before.Keys.Union(after.Keys, StringComparer.Ordinal)
                        .Where(path => !before.TryGetValue(path, out var oldHash) || !after.TryGetValue(path, out var currentHash) || oldHash != currentHash)
                        .Order(StringComparer.Ordinal));
                    var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(fixture.Read(OutsideMaintenanceWorkspace.Note)));
                    var metadata = new SourceAuthoredMetadataParser().Parse(document, SourceDocumentForm.Markdown);
                    Assert.Equal(SourceAuthoredMetadataState.Complete, metadata.State);
                    Assert.Equal("Note", metadata.Description);
                    Assert.Equal(SourceBody.Replace("./guide.md", "guide.md", StringComparison.Ordinal),
                        document.Source[document.Source.IndexOf("# Note", StringComparison.Ordinal)..]);
                    var guide = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(fixture.Read(Target)));
                    var guideMetadata = new SourceAuthoredMetadataParser().Parse(guide, SourceDocumentForm.Markdown);
                    Assert.Equal(SourceAuthoredMetadataState.Complete, guideMetadata.State);
                    Assert.Equal("Guide", guideMetadata.Description);
                    Assert.Equal("# Guide\n\nKeep this target.\n", guide.Source[guide.Source.IndexOf("# Guide", StringComparison.Ordinal)..]);
                    var rootEntries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(
                        Encoding.UTF8.GetString(fixture.Read(OutsideMaintenanceWorkspace.Anchor))));
                    Assert.Equal(new[] { "nested/_nested.md" }, rootEntries.Entries.Select(entry => entry.Destination));
                    var nestedPath = WorkspaceRoutePath.CanonicalEntrypointPath(SourceLogicalPath.ReadParent(Target));
                    var nestedEntries = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(
                        Encoding.UTF8.GetString(fixture.Read(nestedPath))));
                    Assert.Equal(new[] { "guide.md", "note.md" }, nestedEntries.Entries.Select(entry => entry.Destination));
                    var repeated = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), Token);
                    Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
                    Assert.Empty(Assert.IsType<RepairPlan>(repeated.Plan).FileChanges);
                    Assert.Equal(0, repeated.Counts.AppliedEffects);
                    Assert.Equal(after, fixture.Files.SnapshotHashes());
                }
                Assert.False(File.Exists(fixture.Files.Combine(unrelated)));
            }
            Assert.False(Directory.Exists(fixture.Files.Combine(unrelated)));
            Assert.Equal(settings, fixture.Read(WorkspaceSettingsDefinitions.RelativePath));
            Assert.Equal(loader, fixture.Read(fixture.Loader));
            Assert.Equal(other, fixture.Read(OutsideMaintenanceWorkspace.OtherNote));
            if (wrongKind || mode == RepairMode.DryRun)
                Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(fixture.Workspace)));
        }
    }

    private static void RegisterUnrelated(OutsideMaintenanceWorkspace fixture, string mainRoot, string unrelated)
        => fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}","{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{OutsideMaintenanceWorkspace.Route}}","{{OutsideMaintenanceWorkspace.Other}}","{{unrelated}}"]}""");

    [Theory(DisplayName = "Primary and outside Repair preserve no-Loader diagnosis refusal in preview and apply")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(OutsideMaintenanceWorkspace.CustomRoot)]
    public async Task NoLoaderPreservation(string mainRoot)
    {
        using var primary = OpenForge.Cli.TestSupport.TemporaryWorkspace.Create("repair-primary-no-loader");
        primary.CreateFile(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}"}""");
        primary.CreateFile(WorkspaceLayoutNames.AgentHostFileName, $"# Workspace\n\nRead `{mainRoot}/{WorkspaceLayoutNames.LoaderFileName}`.\n");
        var directory = mainRoot + "/" + WorkspaceCategoryNames.Guidance;
        primary.CreateFile(directory + "/note.md", SourceBody);
        primary.CreateFile(directory + "/guide.md", "# Guide\n");
        using var outside = Create(mainRoot, loader: false);
        await Check(primary, new(primary.Path, primary.Path, OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspaceSelectionMethod.ExplicitWorkspace));
        await Check(outside.Files, outside.Workspace);

        async Task Check(OpenForge.Cli.TestSupport.TemporaryWorkspace files, OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace workspace)
        {
            var before = files.SnapshotHashes(includeRuntime: true);
            var components = RepairOperationFactory.CreateDefaultComponents();
            var diagnosis = await components.DiagnosisReader.ReadAsync(new DoctorRequest(workspace), Token);
            Assert.Equal(OperationalViewState.Incomplete, diagnosis.Observation.WorkspaceEntry.State);
            foreach (var mode in new[] { RepairMode.DryRun, RepairMode.Apply })
            {
                var result = await new RepairOperation().ExecuteAsync(new(workspace, mode, true, [], false), Token);
                Assert.Equal(CliSemanticStatus.Blocked, result.Status);
                Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisBlocked);
                Assert.Equal(RepairCoverageState.Blocked, result.Diagnosis.WorkspaceAndPath);
                Assert.Equal(RepairPreflightState.NotRequested, result.Preflight.State);
                Assert.Equal(RepairApplicationState.NotRequested, result.Application.State);
                Assert.Equal(0, result.Counts.AppliedEffects);
                Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
                Assert.False(Directory.Exists(OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime.WorkspaceRuntimePaths.DirectoryPath(workspace)));
            }
        }
    }

    [Fact(DisplayName = "Repair retains outside source and target expectations through the physical workspace lease")]
    public async Task TargetChangesUnderLease()
    {
        using var fixture = Create();
        RegisterUnrelated(fixture, WorkspaceLayout.DefaultMainRoot, "unavailable");
        var components = RepairOperationFactory.CreateDefaultComponents();
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan>(preview.Plan);
        var reader = new OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairTargetReader();
        var expectations = await reader.ReadAsync(plan, Token) ?? throw new InvalidOperationException();
        var acquired = await OpenForge.Cli.Core.Framework.Mutation.Locking.WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new(fixture.Workspace, RepairDefinitions.CommandIdentity, Guid.NewGuid()), Token);
        await using var lease = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Locking.Models.WorkspaceLockLease>(acquired.Lease);
        Assert.True(await OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairPlanRevalidator.ValidateLayoutAsync(plan, lease, Token));
        var revalidator = new OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairPlanRevalidator(
            components.DiagnosisReader, components.CatalogueReader, components.MaintenancePlanner);
        Assert.True(await revalidator.ValidateAsync(plan, Token));
        fixture.Files.ReplaceText(Target, "# Independently changed target\n");
        Assert.False(await reader.ValidateAsync(plan, expectations, Token));
        Assert.False(await revalidator.ValidateAsync(plan, Token));
        fixture.Files.ReplaceText(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath, "{}");
        Assert.False(await OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairPlanRevalidator.ValidateLayoutAsync(plan, lease, Token));
    }

    [Fact(DisplayName = "Repair never offers a physical alias target as an outside maintenance destination")]
    public async Task AliasTargetIsReadOnly()
    {
        using var fixture = Create();
        var alias = OutsideMaintenanceWorkspace.Route + "/nested/alias.md";
        fixture.Files.ReplaceText(OutsideMaintenanceWorkspace.Note, SourceBody.Replace("./guide.md", "./alias.md", StringComparison.Ordinal));
        fixture.Files.CreateFileSymbolicLink(alias, fixture.Files.Combine(OutsideMaintenanceWorkspace.OtherNote));
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var result = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
        Assert.Empty(result.Plan?.Effects ?? []);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Repair maintains its selected registered closure while retaining primary Loader admission")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(OutsideMaintenanceWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(OutsideMaintenanceWorkspace.CustomRoot, true)]
    public async Task SelectedClosure(string mainRoot, bool listed)
    {
        using var fixture = Create(mainRoot, loader: true, listed: listed);
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        var other = fixture.Read(OutsideMaintenanceWorkspace.OtherNote);
        var loaderBytes = fixture.Read(fixture.Loader);
        var operation = new RepairOperation();
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
        Assert.NotNull(preview.Plan);
        Assert.Single(preview.Plan.Selection.Selected);
        Assert.False(preview.Plan.IsBlocked);
        Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
        var applied = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), Token);
        Assert.True(applied.Status != CliSemanticStatus.Blocked, string.Join("\n", applied.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.NotEqual(CliSemanticStatus.Failed, applied.Status);
        Assert.True(File.Exists(fixture.Files.Combine(OutsideMaintenanceWorkspace.Anchor)));
        Assert.Equal(other, fixture.Read(OutsideMaintenanceWorkspace.OtherNote));
        Assert.Equal(loaderBytes, fixture.Read(fixture.Loader));
        var document = new OpenForge.Cli.Core.Framework.Documents.Markdown.MarkdownDocumentParser()
            .Parse(Encoding.UTF8.GetString(fixture.Read(OutsideMaintenanceWorkspace.Note)));
        Assert.Equal(SourceBody.Replace("./guide.md", "guide.md", StringComparison.Ordinal), document.Source[document.Source.IndexOf("# Note", StringComparison.Ordinal)..]);
        var after = fixture.Files.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), Token);
        Assert.Empty(repeat.Plan?.Effects ?? []);
        Assert.Equal(after, fixture.Files.SnapshotHashes());
    }

    internal static OutsideMaintenanceWorkspace Create(string mainRoot = WorkspaceLayout.DefaultMainRoot, bool loader = true, bool listed = false)
    {
        var fixture = new OutsideMaintenanceWorkspace(mainRoot, loader, listed, anchor: listed);
        fixture.Files.CreateFile(WorkspaceLayoutNames.AgentHostFileName, $"# Workspace\n\nRead `{fixture.Loader}`.\n");
        fixture.Files.ReplaceText(OutsideMaintenanceWorkspace.Note, SourceBody);
        fixture.Files.CreateFile(Target, "# Guide\n\nKeep this target.\n");
        return fixture;
    }
}
