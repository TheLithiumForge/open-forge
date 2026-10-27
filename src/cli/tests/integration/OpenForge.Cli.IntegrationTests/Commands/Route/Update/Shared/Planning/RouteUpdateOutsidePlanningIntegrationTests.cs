using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update.Shared.Planning;

[Trait("Feature", "route-update-outside"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteUpdateOutsidePlanningIntegrationTests
{
    [Theory(DisplayName = "Outside Update patches exact base bytes with bounded navigation and repeat no-op")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false), InlineData(WorkspaceLayout.DefaultMainRoot, false, true)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false, false)]
    public async Task OrdinaryPatchPreservesIndependentBytes(string root, bool listed, bool overwrite)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root, listed);
        if (overwrite) workspace.Files.WriteText(workspace.Overwrite, "Independent overwrite bytes\r\n");
        var request = workspace.Request(overwrite ? workspace.Overwrite : workspace.Target);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var parentBefore = workspace.Bytes(workspace.Parent);
        var preview = await workspace.Execute(workspace.Request(overwrite ? workspace.Overwrite : workspace.Target, mode: RouteUpdateMode.DryRun));
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + workspace.Target, preview.Target.Id);
        Assert.Equal(overwrite ? RouteUpdateTargetSelection.OverwritePath : RouteUpdateTargetSelection.BasePath, preview.Target.SelectedBy);
        Assert.Equal([workspace.Target, workspace.Parent], preview.Effects.Select(item => item.Path));
        var applied = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, applied.Verification);
        Assert.Equal(RouteUpdateOutsideWorkspace.ChangedDocument(), workspace.Bytes(workspace.Target));
        AssertBoundedEntries(parentBefore, workspace.Bytes(workspace.Parent));
        var entry = Assert.Single(SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(workspace.Text(workspace.Parent))).Entries);
        Assert.Equal(RouteUpdateOutsideWorkspace.After, entry.Description);
        Assert.Equal(SourceLogicalPath.ReadFileName(workspace.Target), entry.Destination);
        Assert.Equal(new[] { "Before", RouteUpdateOutsideWorkspace.Tag }.Order(StringComparer.Ordinal), entry.Tags.Order(StringComparer.Ordinal));
        Assert.Equal(["src/**"], entry.ApplyTo.Select(item => item.Text));
        workspace.AssertPreserved(before);
        if (overwrite) Assert.Equal(Encoding.UTF8.GetBytes("Independent overwrite bytes\r\n"), workspace.Bytes(workspace.Overwrite));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeated = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Update patches registered canonical and compatibility anchors without touching Loader policy")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, null), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, null)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreIndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreReferencesFileName)]
    public async Task RegisteredAnchorsKeepActualNames(string root, string? compatibilityFile)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var target = workspace.Anchor;
        if (compatibilityFile is not null)
        {
            File.Delete(workspace.Files.Combine(target));
            target = workspace.DirectoryPath + "/" + compatibilityFile;
            workspace.Files.WriteText(target, RouteUpdateOutsideWorkspace.Entrypoint());
        }
        var before = workspace.Files.SnapshotHashes();
        var body = Body(workspace.Text(target));
        var applied = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, applied.Verification);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + target, applied.Target.Id);
        Assert.Equal(target, Assert.Single(applied.Effects).Path);
        Assert.Equal(body, Body(workspace.Text(target)));
        if (compatibilityFile is not null) Assert.False(File.Exists(workspace.Files.Combine(workspace.Anchor)));
        workspace.AssertPreserved(before);
    }

    [Theory(DisplayName = "Outside entrypoint field patches preserve their authored body and own Entries exactly")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task AuthoredEntrypointBodyIsNotRefreshed(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root, listed: false);
        workspace.Files.ReplaceText(workspace.Parent, RouteUpdateOutsideWorkspace.Entrypoint(
            $"- [Independent child policy](note.md) - #{SourceLoadingClosureResolver.LoadNowTag} #Fixture - applies to `authored/**`"));
        var before = workspace.Files.SnapshotHashes();
        var body = Body(workspace.Text(workspace.Parent));
        var result = await workspace.Execute(workspace.Request(workspace.Parent));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(body, Body(workspace.Text(workspace.Parent)));
        Assert.Equal(before[workspace.Target], workspace.Files.SnapshotHashes()[workspace.Target]);
        Assert.All(result.Effects, item => Assert.StartsWith(workspace.DirectoryPath + "/", item.Path, StringComparison.Ordinal));
        workspace.AssertPreserved(before);
    }

    [Theory(DisplayName = "Outside Update clears only explicitly selected responsibility and local applicability fields")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task ExplicitClearsPreserveEveryOtherByte(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var patch = RouteUpdateIntegrationWorkspace.Patch(
            responsibility: RouteUpdateIntegrationWorkspace.ResponsibilityPatch(null).Responsibility,
            applyTo: RouteUpdateIntegrationWorkspace.ClearApplyToPatch().ApplyTo);
        var expected = Encoding.UTF8.GetBytes(RouteUpdateOutsideWorkspace.AuthoredDocument
            .Replace("  responsibility: Keep the authoring\r\n", string.Empty, StringComparison.Ordinal)
            .Replace("  applyTo: [\"src/**\"]\r\n", string.Empty, StringComparison.Ordinal));
        var request = workspace.Request(patch: patch);
        var result = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expected, workspace.Bytes(workspace.Target));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        Assert.Empty((await workspace.Execute(request)).Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside metadata creation uses the retained workspace form without assigning category semantics")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Memory, false)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, WorkspaceLayoutNames.SkillsDirectoryName, true)]
    public async Task CreationKeepsWorkspaceFormAndGenericTags(string root, string folder, bool rootForm)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root, directory: "outside/" + folder);
        if (rootForm) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            workspace.Settings([workspace.DirectoryPath], $",\"{WorkspaceSettingsDefinitions.FrontmatterProperty}\":\"{WorkspaceSettingsDefinitions.FrontmatterRootName}\""));
        const string authored = "---\r\nforeign: {keep: true}\r\n---\r\n\r\n# Authored content\r\n";
        workspace.Files.ReplaceText(workspace.Target, authored);
        var patch = RouteUpdateIntegrationWorkspace.Patch(description: RouteUpdateIntegrationWorkspace.DescriptionPatch(RouteUpdateOutsideWorkspace.After).Description,
            tags: RouteUpdateIntegrationWorkspace.TagsPatch(RouteUpdateOutsideWorkspace.Tag).Tags);
        var result = await workspace.Execute(workspace.Request(patch: patch));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var text = workspace.Text(workspace.Target);
        var facts = new FrameworkDocumentMetadataParser().Parse(new MarkdownDocumentParser().Parse(text), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal([RouteUpdateOutsideWorkspace.Tag], facts.Metadata?.Tags);
        Assert.Equal(Body(authored), Body(text));
    }

    [Theory(DisplayName = "Outside SKILL filenames remain ordinary Markdown and add no native metadata")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task SkillFilenameIsOrdinary(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var target = workspace.ParentDirectory + "/" + SourceFormClassifier.NativeSkillFileName;
        workspace.Files.WriteText(target, RouteUpdateOutsideWorkspace.AuthoredDocument);
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateTargetForm.OrdinaryMarkdown, result.Target.Form);
        Assert.Equal(RouteUpdateOutsideWorkspace.ChangedDocument(), workspace.Bytes(target));
    }

    [Theory(DisplayName = "Outside Update reuses Template classification and preserves authored body without provenance")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task TemplateCompletionAndProtectionRemainExplicit(string root, bool emptyBody)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        workspace.Files.WriteText(workspace.Template, RouteUpdateIntegrationWorkspace.TemplateDocument());
        if (emptyBody) workspace.Files.ReplaceText(workspace.Target,
            OpenForgeDocumentSeed.MetadataFrontmatter(RouteUpdateOutsideWorkspace.Before, [RouteUpdateOutsideWorkspace.Tag]) + "\r\n\r\n ");
        var template = workspace.Bytes(workspace.Template);
        var originalBody = Body(workspace.Text(workspace.Target));
        var request = workspace.Request(template: SourceWorkspaceRelativePath.LiteralReferencePrefix + workspace.Template);
        var result = await workspace.Execute(request);
        Assert.Equal(emptyBody ? CliSemanticStatus.Complete : CliSemanticStatus.Attention, result.Status);
        Assert.Equal(emptyBody ? RouteUpdateTemplateDecision.Copied : RouteUpdateTemplateDecision.AuthoredBodyProtected, result.Template?.Decision);
        Assert.Equal(template, workspace.Bytes(workspace.Template));
        if (emptyBody) Assert.Equal(Encoding.UTF8.GetBytes(RouteUpdateIntegrationWorkspace.TemplateBody), Body(workspace.Text(workspace.Target)));
        else Assert.Equal(originalBody, Body(workspace.Text(workspace.Target)));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeated = await workspace.Execute(request);
        Assert.Equal(CliSemanticStatus.Attention, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    internal static byte[] Body(string text)
    {
        var body = new MarkdownDocumentParser().Parse(text).BodySpan ?? throw new InvalidOperationException();
        return Encoding.UTF8.GetBytes(text[body.Start..body.End]);
    }

    internal static void AssertBoundedEntries(byte[] before, byte[] after)
    {
        var original = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(before));
        var changed = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(after));
        var oldSpan = original.GeneratedRegion.ContentSpan ?? throw new InvalidOperationException();
        var newSpan = changed.GeneratedRegion.ContentSpan ?? throw new InvalidOperationException();
        Assert.Equal(Encoding.UTF8.GetBytes(original.Source[..oldSpan.Start]), Encoding.UTF8.GetBytes(changed.Source[..newSpan.Start]));
        Assert.Equal(Encoding.UTF8.GetBytes(original.Source[oldSpan.End..]), Encoding.UTF8.GetBytes(changed.Source[newSpan.End..]));
    }
}
