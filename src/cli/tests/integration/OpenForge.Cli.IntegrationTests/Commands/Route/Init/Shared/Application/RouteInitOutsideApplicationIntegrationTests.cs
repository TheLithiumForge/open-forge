using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Shared.Application;

[Trait("Feature", "route-init-outside"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitOutsideApplicationIntegrationTests
{
    public enum Drift { Settings, Target, Directory, Sibling, Physical, Boundary }

    [Theory(DisplayName = "Outside Init final verification rejects changed readonly bytes after directory effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task ReadonlyPostconditionCannotBeReplacedByFreshMetadata(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        var sibling = RouteInitOutsideWorkspace.Directory + "/sibling" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(sibling, RouteInitOutsideWorkspace.Document());
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.Directory + "/child");
        var plan = await workspace.Plan(target);
        var concurrent = RouteInitOutsideWorkspace.Document() + "\nConcurrent authored suffix.\n";
        var operation = RouteInitOperationFactory.Create(directoryCreationObserver: _ => workspace.Files.ReplaceText(sibling, concurrent));
        var outcome = await operation.ApplicationOperation.ExecuteAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteInitVerificationState.Failed, outcome.Formation.Verification);
        Assert.Contains(outcome.Formation.Findings, finding => finding.Code == RouteInitFindingCode.VerificationFailed);
        Assert.Equal(Encoding.UTF8.GetBytes(concurrent), File.ReadAllBytes(workspace.Files.Combine(sibling)));
        Assert.True(File.Exists(workspace.Files.Combine(target)));
    }

    [Theory(DisplayName = "Outside Init preserves authored leaving links and never acquires foreign metadata for its bounded projection")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task LeavingLinksDoNotExpandTheClosure(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        const string foreign = "foreign/missing-metadata" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(foreign, "# Intentionally outside the maintained closure\r\n");
        const string row = "- [Authored foreign policy](../../foreign/missing-metadata.md) - #Fixture - applies to `src/**`";
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document(row));
        var before = workspace.Files.SnapshotHashes();
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory);
        var plan = await workspace.Plan(target);
        Assert.DoesNotContain(plan.SourceSnapshots, snapshot => snapshot.LogicalPath == workspace.Files.Combine(foreign));
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        workspace.AssertPreserved(before);
        var document = new MarkdownDocumentParser().Parse(File.ReadAllText(workspace.Files.Combine(workspace.Anchor)));
        var retained = Assert.Single(OpenForge.Cli.Core.Framework.Sources.Loading.SourceGeneratedEntriesParser.Parse(document).Entries,
            entry => entry.Destination == "../../" + foreign);
        Assert.Equal(Encoding.UTF8.GetBytes(row), Encoding.UTF8.GetBytes(document.Source.Substring(retained.Span.Start, retained.Span.Length)));
    }

    [Theory(DisplayName = "Outside Init can read Library-owned metadata but cannot change Library-owned Entries")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteInitOutsideWorkspace.CustomRoot, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteInitOutsideWorkspace.CustomRoot, false)]
    public async Task LibraryInputsDoNotGrantEntriesAuthority(string root, bool libraryAnchor)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document());
        var sibling = RouteInitOutsideWorkspace.Directory + "/library" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(sibling, RouteInitOutsideWorkspace.Document());
        workspace.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            { Libraries = [new("fixture-library", "library-source", RouteInitOutsideWorkspace.Directory, [libraryAnchor ? workspace.Anchor : sibling])] }));
        var before = workspace.Files.SnapshotHashes();
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory);
        var result = await workspace.Execute(workspace.Request(target));
        if (libraryAnchor)
        {
            Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.TargetUnsafe);
            Assert.Equal(before, workspace.Files.SnapshotHashes());
        }
        else
        {
            Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
            Assert.Equal(before[sibling], workspace.Files.SnapshotHashes()[sibling]);
            workspace.AssertPreserved(before);
        }
    }

    [Theory(DisplayName = "Outside Init revalidates settings, leaf, ancestry and readonly metadata bytes under its lease")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Settings), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Settings)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Target), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Target)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Directory), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Directory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Sibling), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Sibling)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Physical), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Physical)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Boundary), InlineData(RouteInitOutsideWorkspace.CustomRoot, Drift.Boundary)]
    public async Task StaleFactsPreventEffects(string root, Drift drift)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        using var other = OpenForge.Cli.TestSupport.TemporaryWorkspace.Create("route-init-replacement");
        var sibling = RouteInitOutsideWorkspace.Directory + "/sibling" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(sibling, RouteInitOutsideWorkspace.Document());
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.Directory + "/child");
        var plan = await workspace.Plan(target);
        switch (drift)
        {
            case Drift.Settings: workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([])); break;
            case Drift.Target: workspace.Files.WriteText(target, RouteInitOutsideWorkspace.Document()); break;
            case Drift.Directory: workspace.Files.CreateDirectory(SourceLogicalPath.ReadParent(target)); break;
            case Drift.Sibling: workspace.Files.ReplaceText(sibling, RouteInitOutsideWorkspace.Document() + "\nConcurrent suffix.\n"); break;
            case Drift.Physical:
                Assert.True(workspace.Files.TryCreateDirectorySymbolicLink(SourceLogicalPath.ReadParent(target), other.Path, out _));
                break;
            case Drift.Boundary:
                File.Delete(workspace.Files.Combine(sibling));
                Directory.Delete(workspace.Files.Combine(RouteInitOutsideWorkspace.Directory));
                break;
            default: throw new ArgumentOutOfRangeException(nameof(drift));
        }
        var before = workspace.Files.SnapshotHashes();
        var foreignBefore = other.SnapshotHashes();
        var outcome = await RouteInitOperationFactory.Create().ApplicationOperation.ExecuteAsync(plan, TestContext.Current.CancellationToken);
        Assert.Contains(outcome.Formation.Findings, finding => finding.Code == RouteInitFindingCode.TargetChanged);
        Assert.All(outcome.Formation.Effects, effect => Assert.Equal(RouteInitEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.Equal(foreignBefore, other.SnapshotHashes());
    }

    [Theory(DisplayName = "Outside Init reads managed metadata and updates only permitted entrypoint Entries")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteInitOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteInitOutsideWorkspace.CustomRoot, true)]
    public async Task ManagedInputsRemainSeparateFromEntries(string root, bool managedAnchor)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document());
        var sibling = RouteInitOutsideWorkspace.Directory + "/managed" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(sibling, RouteInitOutsideWorkspace.Document());
        var ownership = WorkspaceOwnershipDocument.Empty with
        { Extensions = [new("fixture-owner", null, null, [], [managedAnchor ? workspace.Anchor : sibling], [])] };
        workspace.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));
        var siblingBytes = File.ReadAllBytes(workspace.Files.Combine(sibling));
        var before = workspace.Files.SnapshotHashes();
        var original = new MarkdownDocumentParser().Parse(File.ReadAllText(workspace.Files.Combine(workspace.Anchor)));
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory);
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(siblingBytes, File.ReadAllBytes(workspace.Files.Combine(sibling)));
        var after = new MarkdownDocumentParser().Parse(File.ReadAllText(workspace.Files.Combine(workspace.Anchor)));
        var oldSpan = original.GeneratedRegion.ContentSpan ?? throw new InvalidOperationException();
        var newSpan = after.GeneratedRegion.ContentSpan ?? throw new InvalidOperationException();
        Assert.Equal(Encoding.UTF8.GetBytes(original.Source[..oldSpan.Start]), Encoding.UTF8.GetBytes(after.Source[..newSpan.Start]));
        Assert.Equal(Encoding.UTF8.GetBytes(original.Source[oldSpan.End..]), Encoding.UTF8.GetBytes(after.Source[newSpan.End..]));
        workspace.AssertPreserved(before);
        Assert.Equal(RouteInitRecoveryState.Removed, result.Recovery.State);
    }

    [Theory(DisplayName = "Outside Init preserves excluded missing anchors without granting scaffold permission")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task ExcludedAnchorIsNotRestored(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([RouteInitOutsideWorkspace.Directory],
            $",\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[\"{workspace.Anchor}\"]"));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(workspace.Anchor));
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Fact(DisplayName = "Outside Init retains created chain and recovery after a later anchor replacement fails")]
    public async Task PartialFailureRetainsRecoveryAndConverges()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = new RouteInitOutsideWorkspace(RouteInitOutsideWorkspace.CustomRoot);
        workspace.Files.WriteText(workspace.Anchor, RouteInitOutsideWorkspace.Document());
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory);
        var plan = await workspace.Plan(target);
        var anchorBefore = File.ReadAllBytes(workspace.Files.Combine(workspace.Anchor));
        using (File.Open(workspace.Files.Combine(workspace.Anchor), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var outcome = await RouteInitOperationFactory.Create().ApplicationOperation.ExecuteAsync(plan, TestContext.Current.CancellationToken);
            Assert.Contains(outcome.Formation.Findings, finding => finding.Code == RouteInitFindingCode.WriteFailed);
            Assert.Equal(anchorBefore, File.ReadAllBytes(workspace.Files.Combine(workspace.Anchor)));
            Assert.True(Directory.Exists(workspace.Files.Combine(SourceLogicalPath.ReadParent(target))));
            Assert.Equal(RouteInitRecoveryState.Retained, outcome.Formation.Recovery.State);
            Assert.True(File.Exists(outcome.Formation.Recovery.ResidualPath));
        }
        var retry = await workspace.Execute(workspace.Request(target));
        Assert.Equal(RouteInitVerificationState.Verified, retry.Verification);
        var repeat = await workspace.Execute(workspace.Request(target));
        Assert.Empty(repeat.Effects);
    }
}
