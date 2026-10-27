using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create.Shared.Planning;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteCreateOutsideSafetyIntegrationTests
{
    private const string FileName = "note" + SourceFormClassifier.MarkdownExtension;
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    public enum Protection { RemovedFile, RemovedDirectory, ManagedTarget, LibraryAnchor, PrivateRoute }

    [Theory(DisplayName = "Outside Create rejects excluded, managed target and Library navigation effects without changing bytes or infrastructure")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Protection.RemovedFile), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Protection.RemovedFile)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Protection.RemovedDirectory), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Protection.RemovedDirectory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Protection.ManagedTarget), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Protection.ManagedTarget)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Protection.LibraryAnchor), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Protection.LibraryAnchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Protection.PrivateRoute), InlineData(RouteCreateOutsideWorkspace.CustomRoot, Protection.PrivateRoute)]
    public async Task ProtectedEffects(string mainRoot, Protection protection)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var path = RouteCreateOutsideWorkspace.Directory + "/scope/" + FileName;
        switch (protection)
        {
            case Protection.RemovedFile:
                fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings([RouteCreateOutsideWorkspace.Directory],
                    $",\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[\"{path}\"]"));
                break;
            case Protection.RemovedDirectory:
                fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings([RouteCreateOutsideWorkspace.Directory],
                    $",\"{WorkspaceSettingsDefinitions.RemovedDirectoriesProperty}\":[\"{RouteCreateOutsideWorkspace.Directory}/scope\"]"));
                break;
            case Protection.ManagedTarget:
                fixture.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
                { Extensions = [new("fixture-owner", null, null, [], [path], [])] }));
                break;
            case Protection.LibraryAnchor:
                fixture.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
                { Libraries = [new("fixture-library", "library-source", RouteCreateOutsideWorkspace.Directory, [fixture.Anchor])] }));
                break;
            case Protection.PrivateRoute:
                fixture.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
                { Framework = new(new("fixture-owner", null), [], []) { GitIgnoredRoutes = [new(RouteCreateOutsideWorkspace.Directory, fixture.Anchor)] } }));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(protection));
        }
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(path);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.TargetUnsafe);
        Assert.Empty(build.Formation.Effects);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(fixture.Workspace)));
    }

    [Theory(DisplayName = "Outside Create cannot follow a contained directory alias into another tree or reserved runtime storage")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, true)]
    public async Task DirectoryAliases(string mainRoot, bool reserved)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var targetDirectory = reserved ? WorkspaceRuntimePaths.DirectoryName : "foreign";
        fixture.Files.CreateDirectory(targetDirectory);
        var alias = RouteCreateOutsideWorkspace.Directory + "/alias";
        fixture.Files.CreateDirectorySymbolicLink(alias, fixture.Files.Combine(targetDirectory));
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(alias + "/missing/" + FileName);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.TargetUnsafe);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
        Assert.False(File.Exists(fixture.Files.Combine(targetDirectory + "/missing/" + FileName)));
    }

    [Theory(DisplayName = "Outside Create rejects a readable file alias instead of treating it as an identical target")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task LeafAlias(string mainRoot)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var path = RouteCreateOutsideWorkspace.Directory + "/" + FileName;
        fixture.Files.CreateFileSymbolicLink(path, fixture.Files.Combine(fixture.Anchor));
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(path);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code is RouteCreateFindingCode.TargetUnsafe or RouteCreateFindingCode.IdentityCollision);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Outside Create rechecks an existing directory mapping under the lease before any effect")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task DirectoryMappingDrift(string mainRoot)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var directory = RouteCreateOutsideWorkspace.Directory + "/scope";
        fixture.Files.CreateDirectory(directory);
        fixture.Files.CreateDirectory("foreign");
        var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(directory + "/" + FileName)).Plan);
        var physical = fixture.Files.Combine(directory);
        Directory.Delete(physical);
        Directory.CreateSymbolicLink(physical, fixture.Files.Combine("foreign"));
        try
        {
            var before = fixture.Files.SnapshotHashes();
            var result = await new RouteCreateApplicationOperation().ExecuteAsync(plan, Token);
            Assert.Equal(RouteCreateFindingCode.TargetChanged, Assert.Single(result.Findings).Code);
            Assert.Equal(before, fixture.Files.SnapshotHashes());
            Assert.False(File.Exists(fixture.Files.Combine("foreign/" + FileName)));
        }
        finally
        {
            Directory.Delete(physical);
            Directory.CreateDirectory(physical);
        }
    }

    [Theory(DisplayName = "An unreadable selected outside anchor stops Create before target snapshots and effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task UnavailableAnchor(string mainRoot)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var before = fixture.Files.SnapshotHashes();
        using (File.Open(fixture.Files.Combine(fixture.Anchor), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var build = await fixture.Build(RouteCreateOutsideWorkspace.Directory + "/" + FileName);
            Assert.Null(build.Plan);
            Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.InspectionIncomplete);
        }
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "An unavailable unrelated registration does not become a primary-root failure or a Create effect")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task UnavailableUnrelatedAnchor(string mainRoot)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        var otherAnchor = WorkspaceRoutePath.CanonicalEntrypointPath(RouteCreateOutsideWorkspace.OtherDirectory);
        fixture.Files.WriteText(otherAnchor, RouteCreateOutsideWorkspace.Document());
        fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            fixture.Settings([RouteCreateOutsideWorkspace.Directory, RouteCreateOutsideWorkspace.OtherDirectory]));
        var before = fixture.Files.SnapshotHashes();
        using (File.Open(fixture.Files.Combine(otherAnchor), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var path = RouteCreateOutsideWorkspace.Directory + "/" + FileName;
            var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
            Assert.Equal(new[] { path, fixture.Anchor }, plan.Preview.Effects.Select(effect => effect.Path));
        }
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }
}
