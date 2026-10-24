using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Framework.Workspace.Shared.Runtime;

public sealed partial class WorkspaceRuntimeReservationIntegrationTests
{
    private const string MoveSourceScenario = "move-source";
    private const string MoveDestinationScenario = "move-destination";
    private const string RemoveScenario = "remove-target";

    [Theory(DisplayName = "Remove selection and inventories reject direct and linked runtime targets")]
    [InlineData(WorkspaceRuntimePaths.DirectoryName), InlineData(AliasDirectory)]
    public async Task RemoveRejectsRuntime(string root)
    {
        using var files = TemporaryWorkspace.Create("runtime-remove");
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(AliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var workspace = Workspace(files);
        foreach (var target in new[] { root, $"{root}/{RecoveryHistory}", $"{root}/{MissingFile}" })
        {
            var selected = await RemoveTargetResolver.ResolveAsync(workspace, RemoveSelection.Selected(RemoveKind.Path, target), Token);
            Assert.NotNull(selected.Cause);
        }
        var inventory = new RemovePathInventory(new PhysicalPathResolver());
        Assert.IsType<RemovePathDirectoryInventory.Unavailable>(await inventory.InventoryDirectoryAsync(workspace, files.Combine(root), null, Token));
        Assert.IsType<RemovePathFileCapture.Unavailable>(await inventory.CaptureFileAsync(workspace, files.Combine($"{root}/{RecoveryHistory}"), Token));
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(RuntimeFile)));
    }

    [Theory(DisplayName = "Route Create and Init reject new targets below a runtime alias")]
    [InlineData(false), InlineData(true)]
    public async Task RouteCreationRejectsRuntimeAlias(bool existing)
    {
        using var files = TemporaryWorkspace.Create("runtime-create");
        SeedWorkspace(files);
        files.CreateDirectory(WorkspaceRuntimePaths.DirectoryName);
        if (existing) files.CreateFile(RuntimeNewLeaf, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(SourceAliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var workspace = Workspace(files);
        var create = await new RouteCreateTargetInspector().InspectAsync(new RouteCreateRequest(workspace, SourceAliasNewFile,
            new RouteCreateMetadataInput(description: "New", tags: ["Docs"], responsibility: null), templateReference: null, RouteCreateMode.DryRun), Token);
        Assert.Equal(RouteCreateFindingCode.TargetUnsafe, create.Boundary?.Finding.Code);
        var init = new RouteInitTargetPlanner().Resolve(new RouteInitRequest(workspace, SourceAliasNewCatalogue,
            RouteInitScaffold.Generic, RouteInitMode.DryRun, RouteInitMetadataInput.None));
        Assert.Equal(RouteInitTargetResolutionState.Invalid, init.State);
        Assert.False(File.Exists(files.Combine(RuntimeNewCatalogue)));
    }

    [Theory(DisplayName = "Route selectors refuse runtime aliases without changing their contents")]
    [InlineData(MoveSourceScenario), InlineData(MoveDestinationScenario), InlineData(RemoveScenario)]
    public async Task RouteSelectorsRejectRuntimeAliases(string operation)
    {
        using var files = TemporaryWorkspace.Create("runtime-route-selectors");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, RetainedHistory);
        Assert.True(files.TryCreateDirectorySymbolicLink(SourceAliasDirectory, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var arguments = operation switch
        {
            MoveSourceScenario => new[] { RouteDefinitions.RouteGroup.Name, RouteMoveDefinitions.MoveCommand.Name, SourceAliasHistory, RootMoved, RouteMoveDefinitions.DryRun.Name },
            MoveDestinationScenario => [RouteDefinitions.RouteGroup.Name, RouteMoveDefinitions.MoveCommand.Name, RootLeaf, SourceAliasNewFile, RouteMoveDefinitions.DryRun.Name],
            RemoveScenario => [RouteDefinitions.RouteGroup.Name, RouteRemoveDefinitions.RemoveCommand.Name, SourceAliasHistory, RouteRemoveDefinitions.DryRun.Name],
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        var result = await CliHostCapture.RunAsync(arguments, files.Path);
        Assert.True(result.ExitCode != 0, result.Output + result.Error);
        Assert.Equal(RetainedHistory, File.ReadAllText(files.Combine(RuntimeFile)));
        Assert.False(File.Exists(files.Combine(RuntimeNewLeaf)));
        Assert.False(File.Exists(files.Combine(RootMoved)));
    }

    [Fact(DisplayName = "Index ignores runtime content and aliases when no ignore file exists")]
    public async Task IndexIgnoresRuntimeContent()
    {
        using var files = TemporaryWorkspace.Create("runtime-index");
        SeedWorkspace(files);
        files.CreateFile(RuntimeFile, new byte[] { 255 });
        Assert.True(files.TryCreateDirectorySymbolicLink(RootRuntime, files.Combine(WorkspaceRuntimePaths.DirectoryName), out var cause), cause);
        var result = await CliHostCapture.RunAsync([IndexDefinitions.CommandIdentity, IndexDefinitions.DryRun.Name, CliSyntaxDefinitions.Format.Name, JsonFormat], files.Path);
        Assert.Equal(0, result.ExitCode);
        MatchSnapshot(CommandOutputNormalization.ReplaceDelimitedPath(result.Output, files.Path, "<workspace>"), $"result.json.{CliPresentationDefinitions.Minimal}");
        Assert.False(File.Exists(WorkspaceRuntimePaths.IgnorePath(Workspace(files))));
        Assert.Equal(new byte[] { 255 }, File.ReadAllBytes(files.Combine(RuntimeFile)));
    }

    [Fact(DisplayName = "Removing a Framework workspace file preserves runtime recovery bytes")]
    public async Task FrameworkFileRemovalPreservesRuntimeRecovery()
    {
        using var files = TemporaryWorkspace.Create("runtime-removal-preservation");
        SeedWorkspace(files);
        files.CreateFile(RootScratch, "remove me");
        files.CreateFile(RuntimeFile, RetainedHistory);
        var result = await CliHostCapture.RunAsync([RemoveDefinitions.CommandIdentity, RootScratch, RemoveDefinitions.KindOption, RemoveDefinitions.PathKind, RemoveDefinitions.AutomaticOption], files.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.False(File.Exists(files.Combine(RootScratch)));
        Assert.Equal(RetainedHistory, File.ReadAllText(files.Combine(RuntimeFile)));
    }

    private static void SeedWorkspace(TemporaryWorkspace files)
    {
        files.CreateFile(SourceLogicalPath.LoaderPath, $"# Loader\n\n## Entries\n\n- [Root]({RootCatalogueRelative}) - #Docs\n");
        files.CreateFile(WorkspaceSettingsDefinitions.RelativePath, FixtureSettings());
        files.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("framework", null), [], []),
        }));
        files.CreateFile(RootCatalogue, $"---\nopen-forge:\n  description: Root\n  tags: [Docs]\n---\n# Root\n\n## Entries\n\n- [Leaf]({LeafName}.md) - #Docs\n");
        files.CreateFile(RootLeaf, "---\nopen-forge:\n  description: Leaf\n  tags: [Docs]\n---\n# Leaf\n");
    }
}
