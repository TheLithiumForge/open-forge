using System.Text.Json;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Boundary", "Host"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
public sealed class RouteRemoveOutsideReviewIntegrationTests
{
    public static IEnumerable<object[]> MissingEntrypoints()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteRemoveOutsideWorkspace.CustomRoot })
            foreach (var compatibility in new[] { false, true })
                foreach (var mode in new[] { "default", RemoveDefinitions.PathKind, RemoveDefinitions.RouteKind })
                    yield return [root, compatibility, mode];
    }

    [Theory, MemberData(nameof(MissingEntrypoints))]
    public async Task MissingEntrypointKeepsPathModeRefusal(string root, bool compatibility, string mode)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root);
        var path = DescendantEntrypoint(compatibility);
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(
            $",\"{WorkspaceSettingsDefinitions.RemovedDirectoriesProperty}\":[\"{RouteRemoveOutsideWorkspace.Category}\"]"));
        var before = fixture.Inner.SnapshotHashes();
        using var output = new StringWriter();
        using var error = new StringWriter();
        string[] kind = mode == "default" ? [] : [RemoveDefinitions.KindOption, mode];
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + path, .. kind, RemoveDefinitions.AutomaticOption,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(mode == RemoveDefinitions.RouteKind ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Empty(report.RootElement.GetProperty(CliJsonFields.Effects).EnumerateArray());
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public async Task ClassifyThenCreateCannotExpandEntrypointFileIntoCategoryDeletion(bool compatibility)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var path = DescendantEntrypoint(compatibility);
        var dispatch = await RemoveTargetResolver.ResolveAsync(fixture.Inner.Workspace,
            RemoveSelection.Selected(RemoveKind.Path, path), TestContext.Current.CancellationToken);
        SeedDescendant(fixture, compatibility);
        var before = fixture.Inner.SnapshotHashes();
        if (!dispatch.IsEntrypointFile && dispatch.Kind == RemoveKind.Route)
        {
            var result = await RouteRemoveOperationFactory.Create(fixture.Inner.LockStoreRoot).ExecuteAsync(
                new RouteRemoveRequest(fixture.Inner.Workspace,
                    dispatch.RouteReference ?? throw new InvalidOperationException("A delegated route needs its exact reference."),
                    RouteRemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
        }
        Assert.True(dispatch.IsEntrypointFile);
        Assert.Equal(RemoveKind.Path, dispatch.Kind);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        Assert.True(File.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Child)));
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Theory]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task ExplicitRouteAndCategoryDirectoryRetainCategoryRemoval(bool compatibility, bool explicitRoute)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        SeedDescendant(fixture, compatibility);
        var loader = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        var target = explicitRoute ? DescendantEntrypoint(compatibility) : RouteRemoveOutsideWorkspace.Category;
        string[] kind = explicitRoute ? [RemoveDefinitions.KindOption, RemoveDefinitions.RouteKind] : [];
        using var output = new StringWriter();
        using var error = new StringWriter();
        var result = await fixture.Inner.RunAsync([RemoveDefinitions.CommandIdentity,
            SourceWorkspaceRelativePath.LiteralReferencePrefix + target, .. kind, RemoveDefinitions.AutomaticOption,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat], output, error);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(Directory.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Category)));
        Assert.Equal(loader, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
    }

    public static IEnumerable<object[]> AnchorChanges()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteRemoveOutsideWorkspace.CustomRoot })
            foreach (var compatibility in new[] { false, true })
                foreach (var nested in new[] { false, true })
                    yield return [root, compatibility, nested];
    }

    [Theory, MemberData(nameof(AnchorChanges))]
    public async Task CompetingAnchorAfterRecoveryPreventsEveryEffect(string root, bool compatibility, bool nested)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, compatibility: compatibility);
        var target = nested ? SeedNestedLeaf(fixture) : RouteRemoveOutsideWorkspace.Leaf;
        var plan = await fixture.Plan(target);
        var operationId = Guid.NewGuid();
        await using var lease = await fixture.Inner.AcquireLeaseAsync(operationId);
        var revalidated = await RouteRemoveOperationFactory.CreatePlanRevalidator().RevalidateAsync(plan, lease, TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemovePlanRevalidationState.Exact, revalidated.State);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(new RouteRemoveHeldApplication(plan, operationId, lease), TestContext.Current.CancellationToken);
        Assert.NotNull(prepared.Preparation);
        var competing = nested ? RouteRemoveOutsideWorkspace.DirectoryPath + "/design/" + SourceFormClassifier.IndexFileName
            : compatibility ? WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.DirectoryPath)
                : RouteRemoveOutsideWorkspace.DirectoryPath + "/" + SourceFormClassifier.IndexFileName;
        fixture.Inner.WriteText(competing, RouteRemoveOutsideWorkspace.Entry(MarkdownEntriesSectionReader.EmptyEntry));
        var before = fixture.Inner.SnapshotHashes();
        var progress = await Apply(fixture, plan, lease, prepared);
        AssertAllNotStarted(progress);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        Assert.True(File.Exists(prepared.Preparation.BundlePath));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteRemoveOutsideWorkspace.CustomRoot)]
    public async Task PreparedCompatibilityAnchorWithStaleCanonicalLoaderRemainsUsable(string root)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, compatibility: true);
        var target = SeedNestedLeaf(fixture);
        var loader = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        var result = await fixture.Execute(target);
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.False(File.Exists(fixture.Inner.Combine(target)));
        Assert.Equal(loader, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
        Assert.False(File.Exists(fixture.Inner.Combine(WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.DirectoryPath))));
    }

    [Fact]
    public async Task RegisteredRootMustStillBeAnOrdinaryDirectory()
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var plan = await fixture.Plan();
        var original = fixture.Inner.Combine(RouteRemoveOutsideWorkspace.DirectoryPath);
        var saved = fixture.Inner.Combine(RouteRemoveOutsideWorkspace.DirectoryPath + "-saved");
        Directory.Move(original, saved);
        File.WriteAllText(original, "Substituted ordinary file\n");
        try
        {
            Assert.NotNull(RouteRemoveOutsideBoundaryInspector.Validate(plan.LayoutRead,
                RouteRemoveOutsideWorkspace.DirectoryPath, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(original);
            Directory.Move(saved, original);
        }
    }

    public static IEnumerable<object[]> UnavailableRowHosts()
    {
        foreach (var root in new[] { WorkspaceLayout.DefaultMainRoot, RouteRemoveOutsideWorkspace.CustomRoot })
            foreach (var ignoredProse in new[] { false, true })
                foreach (var ordinaryCatalogue in new[] { false, true })
                    yield return [root, ignoredProse, ordinaryCatalogue];
    }

    [Theory, MemberData(nameof(UnavailableRowHosts))]
    public async Task UnavailableRequiredEntriesCannotProveAbsentLeaf(string root, bool ignoredProse, bool ordinaryCatalogue)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root);
        var host = ordinaryCatalogue ? RouteRemoveOutsideWorkspace.DirectoryPath + "/reading.md" : fixture.Anchor;
        SeedAbsentLeaf(fixture, ignoredProse, ignoredPath: host);
        var authored = RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.RemovedRow
            + "- [Malformed survivor](keep.md) - unsupported suffix\r\n");
        Assert.Equal(SourceGeneratedEntriesState.Unavailable, SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(authored)).State);
        fixture.Inner.WriteText(host, authored);
        if (ordinaryCatalogue) fixture.Inner.WriteText(fixture.Anchor, RouteRemoveOutsideWorkspace.Entry(
            "- [Reading catalogue](reading.md) - #Fixture\r\n" + RouteRemoveOutsideWorkspace.SurvivingRows));
        var before = fixture.Inner.SnapshotHashes();
        var result = await fixture.Execute();
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.NotEqual(RouteRemoveVerificationState.Verified, result.Verification);
        Assert.NotEqual(RouteRemoveCoverage.Complete, result.GeneratedNavigation.Coverage);
        Assert.NotEmpty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteRemoveOutsideWorkspace.CustomRoot)]
    public async Task ParsedAbsenceKeepsOrdinaryAndUnrelatedMalformedSourcesSeparate(string root)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root);
        SeedAbsentLeaf(fixture, false, [RouteRemoveOutsideWorkspace.DirectoryPath, "docs/other"]);
        fixture.Inner.WriteText(fixture.Anchor, RouteRemoveOutsideWorkspace.Entry(MarkdownEntriesSectionReader.EmptyEntry));
        fixture.Inner.WriteText("ordinary.md", "Ordinary source without Entries\n");
        var malformed = RouteRemoveOutsideWorkspace.Entry("- [Other](missing.md) - unsupported suffix");
        fixture.Inner.WriteText(fixture.Layout.SourcePath("unrelated/_unrelated.md"), malformed);
        fixture.Inner.WriteText(WorkspaceRoutePath.CanonicalEntrypointPath("docs/other"), malformed);
        var before = fixture.Inner.SnapshotHashes();
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(RouteRemoveVerificationState.Verified, result.Verification);
        Assert.Empty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    private static string DescendantEntrypoint(bool compatibility)
        => compatibility ? RouteRemoveOutsideWorkspace.Category + "/" + SourceFormClassifier.IndexFileName
            : WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category);

    private static void SeedDescendant(RouteRemoveOutsideWorkspace fixture, bool compatibility)
    {
        fixture.SeedCategory();
        if (!compatibility) return;
        fixture.Inner.DeleteFile(WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category));
        fixture.Inner.WriteText(DescendantEntrypoint(true), RouteRemoveOutsideWorkspace.Entry("- [Child](child.md) - #Fixture"));
        fixture.Inner.WriteText(fixture.Anchor, RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.SurvivingRows
            + "- [Topics](topics/index.md) - #Authored\r\n"));
    }

    private static string SeedNestedLeaf(RouteRemoveOutsideWorkspace fixture)
    {
        const string directory = RouteRemoveOutsideWorkspace.DirectoryPath + "/design";
        fixture.Inner.WriteText(fixture.Anchor, RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.RemovedRow
            + RouteRemoveOutsideWorkspace.SurvivingRows + "- [Design](design/_design.md) - #Fixture\r\n"));
        fixture.Inner.WriteText(WorkspaceRoutePath.CanonicalEntrypointPath(directory), RouteRemoveOutsideWorkspace.Entry("- [Nested](old.md) - #Fixture"));
        const string target = directory + "/old.md";
        fixture.Inner.WriteText(target, "# Nested ordinary source\n");
        return target;
    }

    private static void SeedAbsentLeaf(RouteRemoveOutsideWorkspace fixture, bool ignoredProse, IEnumerable<string>? routes = null,
        string? ignoredPath = null)
    {
        fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Leaf);
        fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Overwrite);
        var ignore = ignoredProse ? $",\"{WorkspaceSettingsDefinitions.IgnoreLinksFromProperty}\":[\"{ignoredPath ?? fixture.Anchor}\"]" : string.Empty;
        fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings(
            $",\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[\"{RouteRemoveOutsideWorkspace.Leaf}\",\"{RouteRemoveOutsideWorkspace.Overwrite}\"]{ignore}", routes));
    }

    private static ValueTask<RouteRemoveApplicationProgress> Apply(RouteRemoveOutsideWorkspace fixture, RouteRemovePlan plan,
        OpenForge.Cli.Core.Framework.Mutation.Locking.Models.WorkspaceLockLease lease, RouteRemoveRecoveryPreparationResult prepared)
        => RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(new RouteRemoveEffectApplicationInput
        {
            Plan = plan,
            Lease = lease,
            RecoveryPreparation = prepared,
        }, TestContext.Current.CancellationToken);

    private static void AssertAllNotStarted(RouteRemoveApplicationProgress progress)
    {
        Assert.NotEmpty(progress.Receipts);
        Assert.NotNull(progress.SettingsReceipt);
        Assert.Equal(FilesystemEffectState.NotStarted, progress.SettingsReceipt.Receipt.EffectState);
        Assert.All(progress.Receipts, receipt => Assert.Equal(FilesystemEffectState.NotStarted,
            receipt switch
            {
                RouteRemoveFileChangeReceipt file => file.Receipt.EffectState,
                RouteRemoveDirectoryDeletionReceipt directory => directory.Receipt.State.EffectState,
                _ => throw new InvalidOperationException("Unknown route removal receipt."),
            }));
        Assert.Equal(RouteRemoveRecoveryState.Retained, progress.Recovery.State);
    }
}
