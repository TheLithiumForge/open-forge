using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.MainRoot;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

[Trait("Feature", "install-main-root"), Trait("Evidence", "EndToEnd")]
public sealed class PublishedMainRootLifecycleProcessTests
{
    private const string MainRoot = "docs/forge";
    private const string Notes = "notes";
    private const string First = "first";
    private const string Second = "second";
    private const string External = "external";
    private const string ForeignPath = External + "/guide" + PublishedTokens.Layout.MarkdownExtension;
    private const string SourceRoot = "library-source";
    private const string LibraryId = "team-library";
    private const string LinkedName = "linked" + PublishedTokens.Layout.MarkdownExtension;
    private const string ChecklistName = "checklist" + PublishedTokens.Layout.MarkdownExtension;
    private const string GuideName = "guide" + PublishedTokens.Layout.MarkdownExtension;
    private const string RemovedName = "obsolete.txt";
    private const string PackageId = "team-extension";
    private const string PackageVersion = "1.0.0";
    private const string PackageLeaf = "extension-note" + PublishedTokens.Layout.MarkdownExtension;
    private const string Description = "Team context";
    private const string Tag = "Team";
    private const string PreservedBody = "# Authored context\r\n\r\nPreserve this exact text.\r\n";

    [Theory(DisplayName = "A real custom Install supports command families without reactivating rows or moving literal sources")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    public async Task FullLifecycle(string form)
    {
        using var workspace = new PublishedMainRootWorkspace(MainRoot);
        using var packageSource = TemporaryWorkspace.Create(nameof(FullLifecycle));
        workspace.OwnTree(External);
        workspace.OwnTree(SourceRoot);
        workspace.Write(ForeignPath, PreservedBody);
        var foreign = workspace.Read(ForeignPath);
        await workspace.Install(form);
        using var initialized = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteInit.Command, Notes);
        var firstId = Notes + "/" + First;
        var secondId = Notes + "/" + Second;
        var firstPath = MainRoot + "/" + firstId + PublishedTokens.Layout.MarkdownExtension;
        var secondPath = MainRoot + "/" + secondId + PublishedTokens.Layout.MarkdownExtension;
        using var created = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteCreate.Command, firstId,
            PublishedTokens.RouteCreate.Description, Description, PublishedTokens.RouteCreate.Tag, Tag);
        using var updated = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteUpdate.Command,
            PublishedTokens.Layout.LiteralReferencePrefix + firstPath, PublishedTokens.RouteUpdate.Description, Description);
        using var inspected = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteInspect.Command, firstId);
        using var listed = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteList.Command);
        using var found = await workspace.Run(PublishedTokens.Find.Command);
        using var references = await workspace.Run(PublishedTokens.References.Command, PublishedTokens.Layout.LiteralReferencePrefix + firstPath);
        using var moved = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteMove.Command, firstId, secondId);
        Assert.False(File.Exists(workspace.Combine(firstPath)));
        Assert.True(File.Exists(workspace.Combine(secondPath)));
        using var removed = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteRemove.Command, secondId, PublishedTokens.RouteRemove.Automatic);
        Assert.False(File.Exists(workspace.Combine(secondPath)));
        using var included = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, ForeignPath,
            PublishedTokens.RouteInclude.Description, Description, PublishedTokens.RouteInclude.Tag, PublishedTokens.Metadata.LoadNow);
        using var context = await workspace.Run(PublishedTokens.Context.Command, PublishedTokens.Layout.LiteralReferencePrefix + ForeignPath);
        Assert.Contains(context.RootElement.GetProperty(PublishedTokens.Json.Data).GetProperty(PublishedTokens.ContextData.Sources).EnumerateArray(),
            source => source.GetProperty(PublishedTokens.Json.Id).GetString() == PublishedTokens.Layout.LiteralReferencePrefix + ForeignPath);
        using var excluded = await workspace.Run(PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, ForeignPath);
        Assert.Equal(foreign, workspace.Read(ForeignPath));

        var guidance = PublishedTokens.Layout.GuidanceDirectory + "/" + PublishedTokens.Layout.GuidanceEntrypoint;
        Assert.Contains(workspace.Combine(MainRoot + "/" + guidance), workspace.EntryTargets(workspace.Loader));
        var loader = Encoding.UTF8.GetString(workspace.Read(workspace.Loader));
        workspace.Write(workspace.Loader, string.Join('\n', loader.Split('\n').Where(line => !line.Contains("(" + guidance + ")", StringComparison.Ordinal))));
        using var indexed = await workspace.Run(PublishedTokens.Index.Command);
        AssertUnlisted();
        using var frameworkUpdated = await workspace.Run(PublishedTokens.Update.Command, PublishedTokens.Update.Automatic);
        AssertUnlisted();
        using var configured = await workspace.Run(PublishedTokens.Install.Command, PublishedTokens.Install.Configure,
            PublishedTokens.Install.Frontmatter, form, PublishedTokens.Install.Automatic);
        AssertUnlisted();

        var package = PackageId;
        packageSource.WriteText(package + "/" + PublishedTokens.ExtensionPackage.ManifestFile, $$"""
            {"{{PublishedTokens.ExtensionPackage.Id}}":"{{PackageId}}","{{PublishedTokens.ExtensionPackage.Name}}":"{{Description}}",
             "{{PublishedTokens.ExtensionPackage.Description}}":"{{Description}}","{{PublishedTokens.ExtensionPackage.Version}}":"{{PackageVersion}}","{{PublishedTokens.ExtensionPackage.Dependencies}}":[]}
            """);
        var suffix = Notes + "/" + PackageLeaf;
        packageSource.WriteText(package + "/" + PublishedTokens.ExtensionPackage.ContentDirectory + "/" + PublishedTokens.Sources.AgentsRoot + "/" + suffix,
            OpenForgeDocumentSeed.Metadata(Description, [Tag], PreservedBody));
        using var extensionInstalled = await workspace.Run(PublishedTokens.Extension.Command, PublishedTokens.ExtensionInstall.Command, PackageId,
            PublishedTokens.ExtensionInstall.Source, packageSource.Path, PublishedTokens.ExtensionInstall.Automatic);
        AssertUnlisted();
        Assert.True(File.Exists(workspace.Combine(MainRoot + "/" + suffix)));
        using var extensionListed = await workspace.Run(PublishedTokens.Extension.Command, PublishedTokens.ExtensionList.Command, PublishedTokens.ExtensionList.Installed);
        using var extensionInspected = await workspace.Run(PublishedTokens.Extension.Command, PublishedTokens.ExtensionInspect.Command, PackageId,
            PublishedTokens.ExtensionInspect.Source, packageSource.Path);
        using var extensionUpdated = await workspace.Run(PublishedTokens.Extension.Command, PublishedTokens.ExtensionUpdate.Command, PackageId,
            PublishedTokens.ExtensionUpdate.Source, packageSource.Path, PublishedTokens.ExtensionUpdate.Automatic);
        using var extensionRemoved = await workspace.Run(PublishedTokens.Extension.Command, PublishedTokens.ExtensionRemove.Command, PackageId, PublishedTokens.ExtensionRemove.Automatic);
        Assert.False(File.Exists(workspace.Combine(MainRoot + "/" + suffix)));

        workspace.Write(SourceRoot + "/" + LinkedName, OpenForgeDocumentSeed.Metadata(Description, [Tag], PreservedBody));
        var sourceBytes = workspace.Read(SourceRoot + "/" + LinkedName);
        using var attached = await workspace.Run(PublishedTokens.Library.Command, PublishedTokens.Library.Attach, LibraryId, SourceRoot,
            PublishedTokens.Library.Destination, MainRoot + "/" + Notes, PublishedTokens.Library.Automatic);
        var linked = workspace.Combine(MainRoot + "/" + Notes + "/" + LinkedName);
        Assert.Equal(Path.GetRelativePath(Path.GetDirectoryName(linked)!, workspace.Combine(SourceRoot + "/" + LinkedName)), new FileInfo(linked).LinkTarget);
        Assert.Equal(sourceBytes, File.ReadAllBytes(linked));
        using var libraryListed = await workspace.Run(PublishedTokens.Library.Command, PublishedTokens.Library.List);
        using var libraryInspected = await workspace.Run(PublishedTokens.Library.Command, PublishedTokens.Library.Inspect, LibraryId);
        using var synced = await workspace.Run(PublishedTokens.Library.Command, PublishedTokens.Library.Sync, LibraryId, PublishedTokens.Library.Automatic);
        using var detached = await workspace.Run(PublishedTokens.Library.Command, PublishedTokens.Library.Detach, LibraryId, PublishedTokens.Library.Automatic);
        Assert.False(File.Exists(linked));
        Assert.Equal(sourceBytes, workspace.Read(SourceRoot + "/" + LinkedName));
        var checklist = MainRoot + "/" + Notes + "/" + ChecklistName;
        var guide = MainRoot + "/" + Notes + "/" + GuideName;
        workspace.Write(checklist, OpenForgeDocumentSeed.Metadata(Description, [Tag], $"# Checklist\n\nRead [Guide](./{GuideName}).\n"));
        workspace.Write(guide, OpenForgeDocumentSeed.Metadata(Description, [Tag], PreservedBody));
        var guideBytes = workspace.Read(guide);
        using var indexedRepair = await workspace.Run(PublishedTokens.Index.Command);
        using var repaired = await workspace.Run(PublishedTokens.Repair.Command, PublishedTokens.Repair.Automatic);
        Assert.NotEmpty(repaired.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
        Assert.Equal(guideBytes, workspace.Read(guide));
        using var doctor = await workspace.Run(PublishedTokens.Doctor.Command);
        using var cleanup = await workspace.Run(PublishedTokens.Cleanup.Command, PublishedTokens.Cleanup.DryRun);
        Assert.False(File.Exists(workspace.Combine(PublishedTokens.Sources.LoaderPath)));
        Assert.False(Directory.Exists(workspace.Combine(MainRoot + "/" + PublishedTokens.Layout.SkillsDirectory)));
        workspace.Write(MainRoot + "/" + Notes + "/" + First + PublishedTokens.Layout.MarkdownExtension, PreservedBody);
        var removedPath = MainRoot + "/" + Notes + "/" + RemovedName;
        workspace.Write(removedPath, PreservedBody);
        using var fileRemoved = await workspace.Run(PublishedTokens.Remove.Command,
            PublishedTokens.Layout.LiteralReferencePrefix + removedPath, PublishedTokens.Remove.Automatic);
        Assert.False(File.Exists(workspace.Combine(removedPath)));
        Assert.Equal(foreign, workspace.Read(ForeignPath));
        Assert.Equal(Encoding.UTF8.GetBytes(PreservedBody), workspace.Read(MainRoot + "/" + Notes + "/" + First + PublishedTokens.Layout.MarkdownExtension));
        Assert.Equal(sourceBytes, workspace.Read(SourceRoot + "/" + LinkedName));

        void AssertUnlisted() => Assert.DoesNotContain(workspace.Combine(MainRoot + "/" + guidance), workspace.EntryTargets(workspace.Loader));
    }
}
