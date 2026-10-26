using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Shared.Planning;

[Trait("Feature", "route-update-outside"), Trait("Evidence", "IntegrationSafety"), Trait("Boundary", "OS")]
public sealed class RouteUpdateOutsideSafetyIntegrationTests
{
    public enum Refusal { MissingAnchor, MissingParent, AmbiguousAnchor, AmbiguousParent, Unregistered, Ordinal, Directory, MissingFile }

    [Theory(DisplayName = "Outside Update requires exact registration and one existing bounded route chain")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.MissingAnchor), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.MissingAnchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.MissingParent), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.MissingParent)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.AmbiguousAnchor), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.AmbiguousAnchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.AmbiguousParent), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.AmbiguousParent)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.Unregistered), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.Unregistered)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.Ordinal), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.Ordinal)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.Directory), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.Directory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Refusal.MissingFile), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Refusal.MissingFile)]
    public async Task UnsafeSelectionHasNoEffects(string root, Refusal refusal)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var path = workspace.Target;
        switch (refusal)
        {
            case Refusal.MissingAnchor: File.Delete(workspace.Files.Combine(workspace.Anchor)); break;
            case Refusal.MissingParent: File.Delete(workspace.Files.Combine(workspace.Parent)); break;
            case Refusal.AmbiguousAnchor:
                workspace.Files.WriteText(workspace.DirectoryPath + "/" + SourceFormClassifier.IndexFileName, RouteUpdateOutsideWorkspace.Entrypoint()); break;
            case Refusal.AmbiguousParent:
                workspace.Files.WriteText(workspace.ParentDirectory + "/" + SourceFormClassifier.IndexFileName, RouteUpdateOutsideWorkspace.Entrypoint()); break;
            case Refusal.Unregistered:
                workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([])); break;
            case Refusal.Ordinal: path = path.Replace("docs/", "Docs/", StringComparison.Ordinal); break;
            case Refusal.Directory: path = workspace.ParentDirectory; break;
            case Refusal.MissingFile: path = workspace.ParentDirectory + "/absent" + SourceFormClassifier.MarkdownExtension; break;
            default: throw new ArgumentOutOfRangeException(nameof(refusal));
        }
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(path));
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.NotEmpty(result.Findings);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside short IDs and Loader single-file inclusion grant no maintenance authority")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task InclusionAndShortIdsDoNotGrantMaintenance(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        const string foreign = "foreign/single.md";
        workspace.Files.WriteText(foreign, RouteUpdateOutsideWorkspace.AuthoredDocument);
        workspace.Files.ReplaceText(workspace.Layout.LoaderPath, RouteUpdateOutsideWorkspace.Entrypoint(
            "- [Single file](../../foreign/single.md) - #Fixture"));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var included = await workspace.Execute(workspace.Request(foreign));
        Assert.Empty(included.Effects);
        Assert.NotEqual(CliSemanticStatus.Complete, included.Status);
        var shortId = new RouteUpdateRequest(workspace.Workspace, "team/design/note",
            RouteUpdateIntegrationWorkspace.DescriptionPatch(RouteUpdateOutsideWorkspace.After), null, RouteUpdateMode.Apply);
        Assert.Empty((await workspace.Execute(shortId)).Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Unavailable unrelated outside routes do not block primary or selected outside Update")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task UnrelatedUnavailableRouteDoesNotBroadenReadiness(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([workspace.DirectoryPath, "unavailable/registered"]));
        var outside = await workspace.Execute(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, outside.Status);
        var primary = await workspace.Execute(new RouteUpdateRequest(workspace.Workspace, "ordinary/note",
            RouteUpdateIntegrationWorkspace.DescriptionPatch(RouteUpdateOutsideWorkspace.After), null, RouteUpdateMode.Apply));
        Assert.Equal(CliSemanticStatus.Complete, primary.Status);
        Assert.Equal("ordinary/note", primary.Target.Id);
    }

    [Theory(DisplayName = "Outside protected metadata targets remain readonly")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task OwnershipDoesNotGrantTargetPermission(string root, bool library)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        WriteOwnership(workspace, workspace.Target, library);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request());
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside supporting ownership is independent from bounded Entries permission")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false, true)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true, true)]
    public async Task ReadonlyInputsAndEntriesHaveSeparateAuthority(string root, bool parent, bool library)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var sibling = workspace.ParentDirectory + "/readonly" + SourceFormClassifier.MarkdownExtension;
        workspace.Files.WriteText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument);
        WriteOwnership(workspace, parent ? workspace.Parent : sibling, library);
        var before = workspace.Files.SnapshotHashes();
        var parentBefore = workspace.Bytes(workspace.Parent);
        var result = await workspace.Execute(workspace.Request());
        if (parent && library)
        {
            Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetUnsafe);
            Assert.Empty(result.Effects);
            Assert.Equal(before, workspace.Files.SnapshotHashes());
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(before[sibling], workspace.Files.SnapshotHashes()[sibling]);
            RouteUpdateOutsidePlanningIntegrationTests.AssertBoundedEntries(parentBefore, workspace.Bytes(workspace.Parent));
            workspace.AssertPreserved(before);
        }
    }

    [Theory(DisplayName = "Outside Update refuses leaf links and ancestor aliases without modifying either coordinate")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task PhysicalAliasesDoNotRedirectEffects(string root, bool directory)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        using var foreign = TemporaryWorkspace.Create("route-update-foreign");
        foreign.WriteText("note.md", RouteUpdateOutsideWorkspace.AuthoredDocument);
        string path;
        if (directory)
        {
            path = workspace.DirectoryPath + "/alias/note.md";
            Assert.True(workspace.Files.TryCreateDirectorySymbolicLink(workspace.DirectoryPath + "/alias", foreign.Path, out _));
        }
        else
        {
            path = workspace.ParentDirectory + "/alias.md";
            Assert.True(workspace.Files.TryCreateFileSymbolicLink(path, foreign.Combine("note.md"), out _));
        }
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var foreignBefore = foreign.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(path));
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.Equal(foreignBefore, foreign.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside explicit removals exclude target metadata and generated Entries effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task ExclusionsRemainAuthoritative(string root, bool parent)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([workspace.DirectoryPath],
            $",\"{WorkspaceSettingsDefinitions.RemovedFilesProperty}\":[\"{(parent ? workspace.Parent : workspace.Target)}\"]"));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request());
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    internal static void WriteOwnership(RouteUpdateOutsideWorkspace workspace, string path, bool library)
    {
        var ownership = library
            ? WorkspaceOwnershipDocument.Empty with { Libraries = [new("fixture-library", "library-source", workspace.DirectoryPath, [path])] }
            : WorkspaceOwnershipDocument.Empty with { Extensions = [new("fixture-owner", null, null, [], [path], [])] };
        workspace.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));
    }

    [Theory(DisplayName = "Outside private descendants remain excluded without granting metadata or navigation authority")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task PrivateDescendantsRemainProtected(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var ownership = WorkspaceOwnershipDocument.Empty with
        {
            Framework = new(new("fixture-framework", null), [], [])
            { GitIgnoredRoutes = [new(workspace.DirectoryPath, workspace.Anchor)] },
        };
        workspace.Files.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(ownership));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request());
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside authored leaving rows do not expand the maintained metadata closure")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task LeavingRowsRemainIndependent(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        const string foreign = "foreign/independent.md";
        workspace.Files.WriteText(foreign, "# Intentionally incomplete metadata\r\n");
        workspace.Files.ReplaceText(workspace.Parent, RouteUpdateOutsideWorkspace.Entrypoint(
            "- [Independent foreign row](../../../foreign/independent.md) - #LoadNow #Fixture - applies to `authored/**`"));
        var body = workspace.Bytes(workspace.Parent);
        var before = workspace.Files.SnapshotHashes();
        var plan = await workspace.Plan();
        Assert.DoesNotContain(plan.Navigation.ReadonlySnapshots, snapshot => snapshot.Expectation.LogicalPath == workspace.Files.Combine(foreign));
        var result = await workspace.Execute(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var oldDocument = new OpenForge.Cli.Core.Framework.Documents.Markdown.MarkdownDocumentParser().Parse(System.Text.Encoding.UTF8.GetString(body));
        var newDocument = new OpenForge.Cli.Core.Framework.Documents.Markdown.MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent));
        var originalRow = Assert.Single(OpenForge.Cli.Core.Framework.Sources.Loading.SourceGeneratedEntriesParser.Parse(oldDocument).Entries);
        var retainedRow = Assert.Single(OpenForge.Cli.Core.Framework.Sources.Loading.SourceGeneratedEntriesParser.Parse(newDocument).Entries,
            entry => entry.Destination == originalRow.Destination);
        Assert.Equal(oldDocument.Source[originalRow.Span.Start..originalRow.Span.End], newDocument.Source[retainedRow.Span.Start..retainedRow.Span.End]);
        workspace.AssertPreserved(before);
    }

    [Theory(DisplayName = "Outside merged physical identities cannot select another logical coordinate")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task MergedIdentityIsRefused(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        Assert.True(workspace.Files.TryCreateFileSymbolicLink(workspace.ParentDirectory + "/alias.md", workspace.Files.Combine(workspace.Target), out _));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request());
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.IdentityCollision);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Template completion requires existing Template classification and a readable bounded source")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task TemplateEligibilityIsNotInferred(string root, bool missing)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        if (!missing) workspace.Files.WriteText(workspace.Template, RouteUpdateIntegrationWorkspace.TemplateDocument(tags: [RouteUpdateOutsideWorkspace.Tag]));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(template: SourceWorkspaceRelativePath.LiteralReferencePrefix + workspace.Template));
        Assert.NotEqual(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Update refuses a registered directory replaced by a cross-boundary alias")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task RegisteredBoundaryAliasIsRefused(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        const string alias = "docs/registered-alias";
        Assert.True(workspace.Files.TryCreateDirectorySymbolicLink(alias, workspace.Files.Combine(workspace.DirectoryPath), out _));
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([workspace.DirectoryPath, alias]));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await workspace.Execute(workspace.Request(alias + "/design/note.md"));
        Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Update refuses an unavailable registered anchor before snapshots or effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task UnavailableAnchorIsIncomplete(string root)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        using (var held = File.Open(workspace.Files.Combine(workspace.Anchor), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var result = await workspace.Execute(workspace.Request());
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == RouteUpdateFindingCode.InspectionIncomplete);
            Assert.Empty(result.Effects);
        }
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside field changes preserve independently authored parent loading policy even in a readonly Library input")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task AuthoredParentPolicyStaysReadonly(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        workspace.Files.ReplaceText(workspace.Parent, RouteUpdateOutsideWorkspace.Entrypoint(
            "- [Independent row metadata](note.md) - #LoadNow #Fixture - applies to `policy/**`"));
        WriteOwnership(workspace, workspace.Parent, library: true);
        var parent = workspace.Bytes(workspace.Parent);
        var before = workspace.Files.SnapshotHashes();
        var result = await workspace.Execute(workspace.Request());
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(workspace.Target, Assert.Single(result.Effects).Path);
        Assert.Equal(parent, workspace.Bytes(workspace.Parent));
        Assert.Equal(RouteUpdateOutsideWorkspace.ChangedDocument(), workspace.Bytes(workspace.Target));
        workspace.AssertPreserved(before);
    }
}
