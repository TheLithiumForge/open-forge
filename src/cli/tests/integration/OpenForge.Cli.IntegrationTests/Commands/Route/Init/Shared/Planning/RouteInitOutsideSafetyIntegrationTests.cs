using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Shared.Planning;

[Trait("Feature", "route-init-outside"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitOutsideSafetyIntegrationTests
{
    [Theory(DisplayName = "Primary Init retains missing compatibility-name exact-path refusal without effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.ReferencesFileName)]
    public async Task PrimaryMissingCompatibilityCanonicalPathsRemainInvalid(string root, string compatibilityFile)
    {
        using var workspace = PrimaryWorkspace(root);
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(root + "/" + Path.GetFileNameWithoutExtension(compatibilityFile));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        foreach (var mode in new[] { RouteInitMode.DryRun, RouteInitMode.Apply })
        {
            var result = await workspace.Execute(workspace.Request(target, mode));
            Assert.Equal(CliSemanticStatus.Invalid, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.InvalidTarget);
            Assert.Empty(result.Effects);
            Assert.False(File.Exists(workspace.Files.Combine(target)));
            Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        }
    }

    [Theory(DisplayName = "Primary Init retains compatibility-name short-ID initialization and repeat no-op")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.ReferencesFileName)]
    public async Task PrimaryCompatibilityNameShortIdsRemainUsable(string root, string compatibilityFile)
    {
        using var workspace = PrimaryWorkspace(root);
        var id = Path.GetFileNameWithoutExtension(compatibilityFile);
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(workspace.Layout.SourcePath(id));
        var request = new RouteInitRequest(workspace.Workspace, id, RouteInitScaffold.Generic, RouteInitMode.Apply, RouteInitMetadataInput.None);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var preview = await workspace.Execute(new RouteInitRequest(workspace.Workspace, id,
            RouteInitScaffold.Generic, RouteInitMode.DryRun, RouteInitMetadataInput.None));
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(target, preview.Target.Path);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        var applied = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteInitVerificationState.Verified, applied.Verification);
        Assert.Equal(target, applied.Target.Path);
        Assert.True(File.Exists(workspace.Files.Combine(target)));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeated = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Primary Init retains an existing exact compatibility-name canonical entrypoint byte-for-byte")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.ReferencesFileName)]
    public async Task PrimaryExistingCompatibilityCanonicalPathsRemainUsable(string root, string compatibilityFile)
    {
        using var workspace = PrimaryWorkspace(root);
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(root + "/" + Path.GetFileNameWithoutExtension(compatibilityFile));
        workspace.Files.WriteText(target, RouteInitOutsideWorkspace.Document());
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var authored = File.ReadAllBytes(workspace.Files.Combine(target));
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Effects, effect => effect.Path == target);
        Assert.Equal(authored, File.ReadAllBytes(workspace.Files.Combine(target)));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        foreach (var row in before.Where(row => row.Key != workspace.Layout.LoaderPath))
            Assert.Equal(row.Value, after[row.Key]);
        var repeated = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Primary Init maps shared automatic-ID entrypoint collisions to typed ambiguity without effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, true), InlineData(RouteInitOutsideWorkspace.CustomRoot, false)]
    public async Task PrimaryCatalogueEntrypointCollisionsRemainBlocked(string root, bool canonical)
    {
        using var workspace = PrimaryWorkspace(root);
        var directory = root + "/ordinary";
        workspace.Files.WriteText(canonical
            ? WorkspaceRoutePath.CanonicalEntrypointPath(directory)
            : directory + "/" + SourceFormClassifier.ReferencesFileName, RouteInitOutsideWorkspace.Document());
        workspace.Files.WriteText(directory + "/" + SourceFormClassifier.IndexFileName, RouteInitOutsideWorkspace.Document());
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(WorkspaceRoutePath.CanonicalEntrypointPath(directory + "/child")));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.RouteAmbiguous);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    private static RouteInitOutsideWorkspace PrimaryWorkspace(string root)
    {
        var workspace = new RouteInitOutsideWorkspace(root, listed: false);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, root == WorkspaceLayout.DefaultMainRoot
            ? "{}" : $$"""{"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}""");
        return workspace;
    }

    [Theory(DisplayName = "Init short IDs retain primary mapping and cannot select an outside folder by coincidence")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task ShortIdsRemainPrimary(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        var request = new RouteInitRequest(workspace.Workspace, RouteInitOutsideWorkspace.Directory,
            RouteInitScaffold.Generic, RouteInitMode.DryRun, RouteInitMetadataInput.None);
        var build = await new OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning.RouteInitPlanBuilder()
            .BuildAsync(request, TestContext.Current.CancellationToken);
        Assert.NotNull(build.Plan);
        Assert.Equal(WorkspaceRoutePath.CanonicalEntrypointPath(workspace.Layout.SourcePath(RouteInitOutsideWorkspace.Directory)), build.Formation.Target.Path);
        Assert.All(build.Formation.Entrypoints, entry => Assert.StartsWith(root + "/", entry.Path, StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Init retains native Skill package refusal independently from outside registration")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task NativeSkillPackageRemainsProtected(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        var package = WorkspaceLayout.NativeSkillsPath + "/package";
        workspace.Files.WriteText(package + "/" + SourceFormClassifier.NativeSkillFileName,
            OpenForgeDocumentSeed.Skill("package", "Independent native Skill"));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(WorkspaceRoutePath.CanonicalEntrypointPath(package + "/child")));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.IdentityCollision);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init refuses incomplete direct-child metadata without completing authored fields")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task MissingReadonlyMetadataRemainsAnIncompleteBoundary(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.WriteText(RouteInitOutsideWorkspace.Directory + "/plain" + SourceFormClassifier.MarkdownExtension, "# Unfinished authored note\r\n");
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(workspace.Anchor));
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.MetadataIncomplete);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init refuses an in-workspace physical transition into another registered boundary")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task OtherRegisteredBoundaryDoesNotGrantPhysicalAdmission(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        const string other = "other-boundary";
        workspace.Files.CreateDirectory(other);
        workspace.Files.WriteText(other + "/kept" + SourceFormClassifier.MarkdownExtension, RouteInitOutsideWorkspace.Document());
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([RouteInitOutsideWorkspace.Directory, other]));
        var link = RouteInitOutsideWorkspace.Directory + "/alias";
        Assert.True(workspace.Files.TryCreateDirectorySymbolicLink(link, workspace.Files.Combine(other), out _));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(WorkspaceRoutePath.CanonicalEntrypointPath(link + "/child")));
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.TargetUnsafe);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init retains exact entrypoint grammar and ordinal registration authority")]
    [InlineData(RouteInitOutsideWorkspace.Directory)]
    [InlineData(RouteInitOutsideWorkspace.Directory + "/ordinary" + SourceFormClassifier.MarkdownExtension)]
    [InlineData(RouteInitOutsideWorkspace.Directory + "/" + SourceFormClassifier.IndexFileName)]
    [InlineData("docs/TEAM/_TEAM" + SourceFormClassifier.MarkdownExtension)]
    [InlineData("docs/team-other/_team-other" + SourceFormClassifier.MarkdownExtension)]
    [InlineData("unregistered/_unregistered" + SourceFormClassifier.MarkdownExtension)]
    [InlineData(RouteInitOutsideWorkspace.Directory + "/" + SourceFormClassifier.NativeSkillFileName)]
    public async Task InvalidExactFormsHaveNoEffects(string target)
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteInitOutsideWorkspace.CustomRoot })
        {
            using var workspace = new RouteInitOutsideWorkspace(root);
            var before = workspace.Files.SnapshotHashes(includeRuntime: true);
            var result = await workspace.Execute(workspace.Request(target));
            Assert.Equal(CliSemanticStatus.Invalid, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.InvalidTarget);
            Assert.Empty(result.Effects);
            Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        }
    }

    [Theory(DisplayName = "Outside Init refuses competing entrypoints without choosing a canonical preference")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task CompetingEntrypointsAreAmbiguous(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document());
        workspace.Files.WriteText(RouteInitOutsideWorkspace.Directory + "/" + SourceFormClassifier.IndexFileName, RouteInitOutsideWorkspace.Document());
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory)));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.RouteAmbiguous);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init refuses Framework scaffolding and missing registered directories before effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteInitOutsideWorkspace.CustomRoot, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteInitOutsideWorkspace.CustomRoot, false)]
    public async Task RequiredOutsideBoundary(string root, bool framework)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        if (!framework) Directory.Delete(workspace.Files.Combine(RouteInitOutsideWorkspace.Directory));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(workspace.Anchor,
            scaffold: framework ? RouteInitScaffold.Framework : RouteInitScaffold.Generic));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == (framework
            ? RouteInitFindingCode.FrameworkAlignmentBlocked : RouteInitFindingCode.TargetUnsafe));
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init blocks physical aliases across boundaries and linked final leaves")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteInitOutsideWorkspace.CustomRoot, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteInitOutsideWorkspace.CustomRoot, false)]
    public async Task PhysicalAliasesAreReadOnly(string root, bool directoryAlias)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        using var other = TemporaryWorkspace.Create("route-init-foreign");
        var target = workspace.Anchor;
        if (directoryAlias)
        {
            var link = RouteInitOutsideWorkspace.Directory + "/escape";
            Assert.True(workspace.Files.TryCreateDirectorySymbolicLink(link, other.Path, out _));
            target = WorkspaceRoutePath.CanonicalEntrypointPath(link + "/child");
        }
        else
        {
            var foreign = other.CreateFile("foreign" + SourceFormClassifier.MarkdownExtension, RouteInitOutsideWorkspace.Document());
            Assert.True(workspace.Files.TryCreateFileSymbolicLink(target, foreign, out _));
        }
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var foreignBefore = other.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.TargetUnsafe);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.Equal(foreignBefore, other.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Unrelated missing outside registration does not expand or block primary or selected outside Init")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task UnrelatedRegistrationStaysOutsideClosure(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([RouteInitOutsideWorkspace.Directory, "unavailable"]));
        var outside = await workspace.Plan(workspace.Anchor);
        Assert.Single(outside.Preview.Entrypoints);
        var primaryPath = WorkspaceRoutePath.CanonicalEntrypointPath(root + "/ordinary");
        var primary = await workspace.Plan(primaryPath);
        Assert.Single(primary.Preview.Entrypoints);
        Assert.Equal(primaryPath, primary.Preview.Entrypoints[0].Path);
        Assert.DoesNotContain(primary.Preview.Effects, effect => effect.Path.StartsWith("unavailable/", StringComparison.Ordinal));
    }

    [Theory(DisplayName = "File inclusion alone does not grant outside Init maintenance")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task FileRegistrationDoesNotAdmitInit(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document());
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([],
            $",\"{WorkspaceSettingsDefinitions.RootsProperty}\":[\"{workspace.Anchor}\"]"));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(workspace.Anchor));
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }
}
