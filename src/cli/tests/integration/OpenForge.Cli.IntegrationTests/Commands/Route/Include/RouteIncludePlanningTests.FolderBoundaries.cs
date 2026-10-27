using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludePlanningTests
{
    [Theory(DisplayName = "Folder Include and Exclude preserve no-Loader and invalid-control refusals"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task FolderControlRefusals(bool custom, bool exclude)
    {
        foreach (var invalidControl in new[] { 0, 1, 2 })
        {
            using var workspace = RouteInclusionFolderFixture.Create(custom);
            var loader = RouteInclusionFolderFixture.Loader(workspace);
            if (invalidControl == 0) File.Delete(workspace.Files.Combine(loader));
            if (invalidControl == 1) workspace.Files.ReplaceText(loader, "# Loader without Entries\n");
            if (invalidControl == 2) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, "{");
            var before = workspace.Files.SnapshotHashes();
            var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace, exclude), RouteInclusionFolderFixture.Token);
            Assert.Equal(CliSemanticStatus.Blocked, build.Outcome.Status);
            Assert.Null(build.Plan);
            Assert.Equal(before, workspace.Files.SnapshotHashes());
            Assert.False(workspace.Locks.InfrastructureExists);
        }
    }

    [Theory(DisplayName = "Folder registration cannot restore a missing owned or excluded catalogue"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, 0), InlineData(false, 1), InlineData(false, 2), InlineData(false, 3)]
    [InlineData(true, 0), InlineData(true, 1), InlineData(true, 2), InlineData(true, 3)]
    public async Task MissingProtectedAnchor(bool custom, int protection)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var path = RouteInclusionFolderFixture.Catalogue;
        if (protection == 0)
        {
            var bytes = WorkspaceSettingsCodec.AddRemovals(workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath),
                new WorkspaceRemovalSelection { Files = [path] });
            workspace.Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath, bytes!);
        }
        else
        {
            var ownership = WorkspaceOwnershipDocument.Empty;
            if (protection == 1) ownership = ownership with { Framework = new(new("fixture", null), [path], []) };
            if (protection == 2) ownership = ownership with { Framework = new(new("fixture", null), [], [new(path, WorkspaceOwnershipDefinitions.EntriesRegion)]) };
            if (protection == 3) ownership = ownership with { Libraries = [new("fixture", "source", RouteInclusionFolderFixture.Folder, [path])] };
            workspace.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));
        }
        var before = workspace.Files.SnapshotHashes();
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Blocked, build.Outcome.Status);
        Assert.Null(build.Plan);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Folder Include refuses overlapping registrations in either ancestor direction"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task FolderRegistrationOverlap(bool custom, bool ancestor)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        var existing = ancestor ? "project" : RouteInclusionFolderFixture.Folder + "/child";
        workspace.Files.CreateDirectory(existing);
        workspace.Files.ReplaceBytes(WorkspaceSettingsDefinitions.RelativePath,
            WorkspaceSettingsCodec.AddOutsideRoute(workspace.Bytes(WorkspaceSettingsDefinitions.RelativePath), existing)!);
        var before = workspace.Files.SnapshotHashes();
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace), RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Invalid, build.Outcome.Status);
        Assert.Null(build.Plan);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Folder Include rejects directory and canonical leaf aliases before effects"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderPhysicalAlias(bool anchorAlias)
    {
        using var workspace = RouteInclusionFolderFixture.Create(false);
        if (anchorAlias) workspace.Files.CreateFileSymbolicLink(RouteInclusionFolderFixture.Catalogue, workspace.Files.Combine("guide.md"));
        else workspace.Files.CreateDirectorySymbolicLink("alias", workspace.Files.Combine(RouteInclusionFolderFixture.Folder));
        var before = workspace.Files.SnapshotHashes();
        var request = RouteInclusionFolderFixture.Request(workspace);
        if (!anchorAlias) request = request with { File = "alias" };
        var build = await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token);
        Assert.Equal(CliSemanticStatus.Invalid, build.Outcome.Status);
        Assert.Null(build.Plan);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }
}
