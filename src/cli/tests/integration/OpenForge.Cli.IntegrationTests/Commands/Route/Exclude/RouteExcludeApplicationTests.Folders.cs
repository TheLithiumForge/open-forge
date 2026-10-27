using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Exclude;

public sealed partial class RouteExcludeApplicationTests
{
    [Theory(DisplayName = "Folder and catalogue file registration remain independent in both removal directions"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task IndependentFolderAndFile(bool custom, bool folderFirst)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        var include = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var folder = new RouteIncludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder };
        var file = folder with { File = RouteInclusionFolderFixture.Catalogue, Description = "Listed file" };
        foreach (var request in folderFirst ? new[] { folder, file } : new[] { file, folder })
            Assert.True((await include.ExecuteAsync(request, RouteInclusionFolderFixture.Token)).Outcome.Verified);
        var registered = RouteInclusionFolderFixture.Settings(workspace);
        Assert.Equal([RouteInclusionFolderFixture.Folder], registered.OutsideRoutes);
        Assert.Equal([RouteInclusionFolderFixture.Catalogue], registered.Roots);
        var bytes = workspace.Bytes(RouteInclusionFolderFixture.Catalogue);
        var exclude = new RouteExcludeOperation(workspace.Locks.StoreRoot);
        var removedPath = folderFirst ? RouteInclusionFolderFixture.Folder : RouteInclusionFolderFixture.Catalogue;
        var result = await exclude.ExecuteAsync(new RouteExcludeRequest { Workspace = workspace.Workspace, File = removedPath }, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.RegisteredBefore);
        Assert.False(result.Outcome.RegisteredAfter);
        Assert.True(result.Outcome.Verified);
        var settings = RouteInclusionFolderFixture.Settings(workspace);
        Assert.Equal(folderFirst ? [] : new[] { RouteInclusionFolderFixture.Folder }, settings.OutsideRoutes);
        Assert.Equal(folderFirst ? new[] { RouteInclusionFolderFixture.Catalogue } : [], settings.Roots);
        Assert.Empty(settings.RemovedFiles);
        Assert.Empty(settings.RemovedDirectories);
        var layout = WorkspaceLayout.Create(workspace.Workspace, settings.MainRoot).WithOutsideRoutes(settings.OutsideRoutes);
        Assert.Empty(SourceLoaderEntriesParser.Parse(layout, workspace.Read(layout.LoaderPath)).Destinations);
        Assert.Equal(bytes, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
        Assert.True((await exclude.ExecuteAsync(new RouteExcludeRequest { Workspace = workspace.Workspace, File = removedPath }, RouteInclusionFolderFixture.Token)).Outcome.NoOp);
        if (!folderFirst)
        {
            Assert.True((await include.ExecuteAsync(folder, RouteInclusionFolderFixture.Token)).Outcome.Verified);
            Assert.Equal([RouteInclusionFolderFixture.Folder], RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
            Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).Roots);
        }
    }

    [Theory(DisplayName = "Folder Exclude removes a missing registration without traversing or recreating its tree"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task MissingRegisteredDirectory(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var include = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        Assert.True((await include.ExecuteAsync(new RouteIncludeRequest { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, RouteInclusionFolderFixture.Token)).Outcome.Verified);
        File.Delete(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue));
        Directory.Delete(workspace.Files.Combine(RouteInclusionFolderFixture.Folder));
        var result = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteExcludeRequest
        { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.Verified);
        Assert.Equal(RouteInclusionTargetKind.Folder, result.Outcome.TargetKind);
        Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        Assert.False(Directory.Exists(workspace.Files.Combine(RouteInclusionFolderFixture.Folder)));
        Assert.DoesNotContain(result.Outcome.Effects, effect => effect.Kind == RouteInclusionEffectKind.Catalogue);
    }

    [Theory(DisplayName = "Folder Exclude removes an unavailable registration without requiring its content to become readable"), Trait("Feature", "route-exclude"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task UnavailableRegisteredDirectory(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        var token = RouteInclusionFolderFixture.Token;
        var included = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = RouteInclusionFolderFixture.Folder }, token);
        Assert.True(included.Outcome.Verified);
        var descendant = RouteInclusionFolderFixture.Folder + "/nested/note" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(descendant, "# Unavailable content\r\n");
        var anchorBytes = workspace.Bytes(RouteInclusionFolderFixture.Catalogue);
        var descendantBytes = workspace.Bytes(descendant);
        var directory = workspace.Files.Combine(RouteInclusionFolderFixture.Folder);
        FileStream? held = null;
        FileStream? descendantHeld = null;
        UnixFileMode? mode = null;
        try
        {
            if (OperatingSystem.IsWindows())
            {
                held = File.Open(workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue), FileMode.Open, FileAccess.Read, FileShare.None);
                descendantHeld = File.Open(workspace.Files.Combine(descendant), FileMode.Open, FileAccess.Read, FileShare.None);
            }
            else
            {
                mode = File.GetUnixFileMode(directory);
                File.SetUnixFileMode(directory, UnixFileMode.None);
            }
            var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace, exclude: true), token);
            var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
            Assert.Equal(WorkspaceOutsideRouteReadState.Unavailable, Assert.Single(plan.LayoutRead.OutsideRoutes).State);
            Assert.DoesNotContain(plan.Changes, change => change.LogicalPath == workspace.Files.Combine(RouteInclusionFolderFixture.Catalogue));
            var outcome = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, token);
            Assert.Equal(CliSemanticStatus.Complete, outcome.Status);
            Assert.True(outcome.Verified);
            Assert.False(outcome.RegisteredAfter);
            Assert.DoesNotContain(outcome.Effects, effect => effect.Kind == RouteInclusionEffectKind.Catalogue);
            Assert.Empty(RouteInclusionFolderFixture.Settings(workspace).OutsideRoutes);
        }
        finally
        {
            descendantHeld?.Dispose();
            held?.Dispose();
            if (!OperatingSystem.IsWindows() && mode is { } prior) File.SetUnixFileMode(directory, prior);
        }
        Assert.Equal(anchorBytes, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
        Assert.Equal(descendantBytes, workspace.Bytes(descendant));
    }
}
