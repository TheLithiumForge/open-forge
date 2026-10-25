using System.Text;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.RoutedMaintenance;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RepairOutsideIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    internal const string SourceBody = "# Note\n\nRead [Guide](./guide.md).\n\nIndependent authored bytes.\n";
    internal static readonly string Target = OutsideMaintenanceWorkspace.Route + "/nested/guide.md";

    [Theory(DisplayName = "Incomplete global diagnosis refuses Repair without entering an unrelated missing or unsafe registration")]
    [InlineData(false), InlineData(true)]
    public async Task UnrelatedRegistration(bool wrongKind)
    {
        using var fixture = Create();
        const string unrelated = "unavailable";
        fixture.Files.ReplaceText(OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{OpenForge.Cli.Core.Framework.Settings.WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{OutsideMaintenanceWorkspace.Route}}","{{unrelated}}"]}""");
        if (wrongKind) fixture.Files.CreateFile(unrelated, OutsideMaintenanceWorkspace.Body);
        var before = fixture.Files.SnapshotHashes(includeRuntime: true);
        foreach (var mode in new[] { RepairMode.DryRun, RepairMode.Apply })
        {
            var result = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, mode, true, [], false), Token);
            Assert.Equal(mode == RepairMode.DryRun ? CliSemanticStatus.Incomplete : CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisIncomplete);
            Assert.Equal(before, fixture.Files.SnapshotHashes(includeRuntime: true));
            Assert.False(Directory.Exists(OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime.WorkspaceRuntimePaths.DirectoryPath(fixture.Workspace)));
        }
    }

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
            foreach (var mode in new[] { RepairMode.DryRun, RepairMode.Apply })
            {
                var result = await new RepairOperation().ExecuteAsync(new(workspace, mode, true, [], false), Token);
                Assert.Equal(CliSemanticStatus.Blocked, result.Status);
                Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.DiagnosisBlocked);
                Assert.Equal(before, files.SnapshotHashes(includeRuntime: true));
                Assert.False(Directory.Exists(OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime.WorkspaceRuntimePaths.DirectoryPath(workspace)));
            }
        }
    }

    [Fact(DisplayName = "Repair retains outside source and target expectations through the physical workspace lease")]
    public async Task TargetChangesUnderLease()
    {
        using var fixture = Create();
        var preview = await new RepairOperation().ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), Token);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan>(preview.Plan);
        var reader = new OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairTargetReader();
        var expectations = await reader.ReadAsync(plan, Token) ?? throw new InvalidOperationException();
        var acquired = await OpenForge.Cli.Core.Framework.Mutation.Locking.WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new(fixture.Workspace, RepairDefinitions.CommandIdentity, Guid.NewGuid()), Token);
        await using var lease = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Locking.Models.WorkspaceLockLease>(acquired.Lease);
        Assert.True(await OpenForge.Cli.Core.Commands.Repair.Shared.Planning.RepairPlanRevalidator.ValidateLayoutAsync(plan, lease, Token));
        fixture.Files.ReplaceText(Target, "# Independently changed target\n");
        Assert.False(await reader.ValidateAsync(plan, expectations, Token));
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
