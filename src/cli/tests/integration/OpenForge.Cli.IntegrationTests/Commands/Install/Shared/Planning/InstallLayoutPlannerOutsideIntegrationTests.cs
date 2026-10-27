using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Planning;

[Trait("Feature", "install-layout"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallLayoutPlannerOutsideIntegrationTests
{
    private const string Outside = "docs/team";
    private const string Anchor = Outside + "/_team.md";
    private const string CustomRoot = "knowledge/main";
    private const string AnchorText = "# Independent catalogue\n\n## Entries\n\n- none - No entries - #Empty\n";

    [Theory(DisplayName = "Initial Install retains outside declarations and available missing or unsafe observations")]
    [InlineData(false, (int)WorkspaceOutsideRouteReadState.Available), InlineData(true, (int)WorkspaceOutsideRouteReadState.Available)]
    [InlineData(false, (int)WorkspaceOutsideRouteReadState.Missing), InlineData(true, (int)WorkspaceOutsideRouteReadState.Missing)]
    [InlineData(false, (int)WorkspaceOutsideRouteReadState.Unsafe), InlineData(true, (int)WorkspaceOutsideRouteReadState.Unsafe)]
    public async Task RetainsOutsideObservation(bool custom, int stateValue)
    {
        var state = (WorkspaceOutsideRouteReadState)stateValue;
        using var workspace = InstallOperationWorkspace.Create(nameof(RetainsOutsideObservation), custom ? CustomRoot : null);
        var settings = Settings([Outside]);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, settings);
        if (state == WorkspaceOutsideRouteReadState.Available) workspace.WriteText(Anchor, AnchorText);
        if (state == WorkspaceOutsideRouteReadState.Unsafe) workspace.WriteText(Outside, "Independent blocking file\n");
        var before = workspace.SnapshotHashes();
        var paths = new PhysicalPathResolver();
        var original = await new WorkspaceLayoutReader(paths).ReadAsync(workspace.Workspace, TestContext.Current.CancellationToken);
        var selected = await new InstallLayoutPlanner(paths).ReadAsync(workspace.Request() with
        {
            RequestedMainRoot = custom ? CustomRoot : null,
        }, TestContext.Current.CancellationToken);

        Assert.Null(selected.Boundary);
        var intended = Assert.IsType<WorkspaceLayout>(selected.IntendedLayout);
        Assert.Equal(custom ? CustomRoot : WorkspaceLayout.DefaultMainRoot, intended.MainRoot);
        Assert.Equal(Outside, Assert.Single(intended.OutsideRoutes).DirectoryPath);
        var observed = Assert.Single(selected.Observation.OutsideRoutes);
        Assert.Equal(state, observed.State);
        Assert.Equal(Outside, observed.Descriptor.DirectoryPath);
        Assert.Equal(Anchor, observed.Descriptor.CataloguePath);
        Assert.Equal(original.State, selected.Observation.State);
        Assert.Equal(original.Settings.State, selected.Observation.Settings.State);
        Assert.Equal(original.Ownership.State, selected.Observation.Ownership.State);
        Assert.Equal(original.Issues, selected.Observation.Issues);
        Assert.All(original.Expectations, expected => Assert.Contains(expected, selected.Observation.Expectations));
        Assert.Equal(Encoding.UTF8.GetBytes(settings), selected.Observation.Settings.Snapshot!.Bytes);
        if (state == WorkspaceOutsideRouteReadState.Available)
            Assert.Equal(Encoding.UTF8.GetBytes(AnchorText), observed.CatalogueSnapshot!.Bytes);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    [Theory(DisplayName = "Initial requested main root conflicts use the typed input boundary without losing registration")]
    [InlineData(Outside), InlineData("docs"), InlineData(Outside + "/new"), InlineData("DOCS/TEAM")]
    public async Task ConflictingInitialRootIsRefused(string requested)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(ConflictingInitialRootIsRefused));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings([Outside]));
        var before = workspace.SnapshotHashes();
        var result = await new InstallLayoutPlanner(new PhysicalPathResolver()).ReadAsync(
            workspace.Request() with { RequestedMainRoot = requested }, TestContext.Current.CancellationToken);

        Assert.Null(result.IntendedLayout);
        Assert.Equal(InstallFindingCode.InvalidInput, result.Boundary?.Code);
        Assert.Equal(Outside, Assert.Single(result.Observation.Layout!.OutsideRoutes).DirectoryPath);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    [Theory(DisplayName = "Empty outside registrations preserve initial default and custom setup")]
    [InlineData(false), InlineData(true)]
    public async Task EmptyRegistrationControl(bool custom)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(EmptyRegistrationControl), custom ? CustomRoot : null);
        var selected = await new InstallLayoutPlanner(new PhysicalPathResolver()).ReadAsync(workspace.Request() with
        {
            RequestedMainRoot = custom ? CustomRoot : null,
        }, TestContext.Current.CancellationToken);
        Assert.Null(selected.Boundary);
        Assert.Empty(Assert.IsType<WorkspaceLayout>(selected.IntendedLayout).OutsideRoutes);
        Assert.Empty(selected.Observation.OutsideRoutes);
        Assert.Empty(workspace.SnapshotHashes());
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    private static string Settings(string[] routes)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{string.Join(",", routes.Select(path => $"\"{path}\""))}}]}""";
}
