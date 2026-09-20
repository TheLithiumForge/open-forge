using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Ownership;

[Trait("Feature", "workspace-ownership"), Trait("Evidence", "Unit")]
public sealed class WorkspaceOwnershipStoreTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An absent lock plans a create")]
    public void AbsentLockPlansCreate()
    {
        var path = LockPath();
        var current = new WorkspaceOwnershipRead(
            WorkspaceOwnershipReadState.Absent,
            WorkspaceOwnershipDocument.Empty,
            path,
            FileStateSnapshot.Missing(path),
            Cause: null);

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        Assert.Equal(OwnershipWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(ExpectedDocumentBytes(IntendedFramework()), change.IntendedBytes.ToArray());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A readable lock plans a replacement")]
    public void ReadableLockPlansReplace()
    {
        var path = LockPath();
        var currentDocument = Document(
            new FrameworkOwnership(new OwnedSource("open-forge", "0.9.0"), ["old.md"], []));
        var current = Read(
            WorkspaceOwnershipReadState.Complete,
            currentDocument,
            FileStateSnapshot.File(path, path, WorkspaceOwnershipCodec.Write(currentDocument)),
            cause: null);

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        Assert.Equal(OwnershipWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(ExpectedDocumentBytes(IntendedFramework()), change.IntendedBytes.ToArray());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A readable invalid lock is rebuilt from empty")]
    public void InvalidReadableLockRebuildsFromEmpty()
    {
        var path = LockPath();
        var current = Read(
            WorkspaceOwnershipReadState.Invalid,
            WorkspaceOwnershipDocument.Empty,
            FileStateSnapshot.File(path, path, "{ invalid"u8),
            cause: "The lock is invalid.");

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        Assert.Equal(OwnershipWritePlanState.Planned, result.State);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        var written = Assert.IsType<WorkspaceOwnershipDocument>(
            WorkspaceOwnershipCodec.Read(change.IntendedBytes.ToArray()).Document);
        Assert.Equal(IntendedFramework().Source, written.Framework?.Source);
        Assert.Empty(written.Extensions);
        Assert.Empty(written.Libraries);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An unreadable lock skips its write")]
    public void UnreadableLockSkipsWrite()
    {
        var current = Read(
            WorkspaceOwnershipReadState.Unavailable,
            WorkspaceOwnershipDocument.Empty,
            snapshot: null,
            cause: "The lock could not be read.");

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        Assert.Equal(OwnershipWritePlanState.Skipped, result.State);
        Assert.Null(result.Change);
        Assert.Equal("The lock could not be read.", result.Cause);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Writing Framework ownership preserves Extension and Library ownership")]
    public void WritingFrameworkPreservesOtherSections()
    {
        var path = LockPath();
        var extension = new ExtensionOwnership("planning", "1.0.0", "bundled", [], ["plan.md"], []);
        var library = new LibraryOwnership("shared", "source", "destination", ["shared.md"]);
        var currentDocument = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            new FrameworkOwnership(new OwnedSource("open-forge", "0.9.0"), ["old.md"], []),
            [extension],
            [library]);
        var current = Read(
            WorkspaceOwnershipReadState.Complete,
            currentDocument,
            FileStateSnapshot.File(path, path, WorkspaceOwnershipCodec.Write(currentDocument)),
            cause: null);

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var written = Assert.IsType<WorkspaceOwnershipDocument>(
            WorkspaceOwnershipCodec.Read(change.IntendedBytes.ToArray()).Document);
        Assert.Equal(IntendedFramework().Source, written.Framework?.Source);
        var writtenExtension = Assert.Single(written.Extensions);
        Assert.Equal(extension.Id, writtenExtension.Id);
        Assert.Equal(extension.Version, writtenExtension.Version);
        Assert.Equal(extension.Source, writtenExtension.Source);
        Assert.Equal(extension.Dependencies, writtenExtension.Dependencies);
        Assert.Equal(extension.Paths, writtenExtension.Paths);
        Assert.Equal(extension.Regions, writtenExtension.Regions);
        var writtenLibrary = Assert.Single(written.Libraries);
        Assert.Equal(library.Id, writtenLibrary.Id);
        Assert.Equal(library.SourceRoot, writtenLibrary.SourceRoot);
        Assert.Equal(library.DestinationRoot, writtenLibrary.DestinationRoot);
        Assert.Equal(library.Paths, writtenLibrary.Paths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Writing Extension ownership preserves Framework and Library ownership")]
    public void WritingExtensionPreservesOtherSections()
    {
        var path = LockPath();
        var framework = new FrameworkOwnership(
            new OwnedSource("open-forge", "0.9.0"),
            ["old.md"],
            [new OwnedRegion("old.md", "entries")]);
        var library = new LibraryOwnership("shared", "source", "destination", ["shared.md"]);
        var currentDocument = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            framework,
            [new ExtensionOwnership("planning", "0.9.0", "bundled", [], ["old-plan.md"], [])],
            [library]);
        var current = Read(
            WorkspaceOwnershipReadState.Complete,
            currentDocument,
            FileStateSnapshot.File(path, path, WorkspaceOwnershipCodec.Write(currentDocument)),
            cause: null);

        var result = new WorkspaceOwnershipStore().PlanExtensionOwnership(current, [IntendedExtension()]);

        var change = Assert.IsType<PlannedFileChange>(result.Change);
        var written = Assert.IsType<WorkspaceOwnershipDocument>(
            WorkspaceOwnershipCodec.Read(change.IntendedBytes.ToArray()).Document);
        var writtenFramework = Assert.IsType<FrameworkOwnership>(written.Framework);
        Assert.Equal(framework.Source, writtenFramework.Source);
        Assert.Equal(framework.Paths, writtenFramework.Paths);
        Assert.Equal(framework.Regions, writtenFramework.Regions);
        var writtenExtension = Assert.Single(written.Extensions);
        Assert.Equal(IntendedExtension().Id, writtenExtension.Id);
        Assert.Equal(IntendedExtension().Version, writtenExtension.Version);
        Assert.Equal(IntendedExtension().Source, writtenExtension.Source);
        Assert.Equal(IntendedExtension().Dependencies, writtenExtension.Dependencies);
        Assert.Equal(IntendedExtension().Paths, writtenExtension.Paths);
        Assert.Equal(IntendedExtension().Regions, writtenExtension.Regions);
        var writtenLibrary = Assert.Single(written.Libraries);
        Assert.Equal(library.Id, writtenLibrary.Id);
        Assert.Equal(library.SourceRoot, writtenLibrary.SourceRoot);
        Assert.Equal(library.DestinationRoot, writtenLibrary.DestinationRoot);
        Assert.Equal(library.Paths, writtenLibrary.Paths);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "A byte-identical Framework ownership write is unchanged")]
    public void IdenticalWriteIsUnchanged()
    {
        var path = LockPath();
        var currentDocument = Document(IntendedFramework());
        var current = Read(
            WorkspaceOwnershipReadState.Complete,
            currentDocument,
            FileStateSnapshot.File(path, path, WorkspaceOwnershipCodec.Write(currentDocument)),
            cause: null);

        var result = new WorkspaceOwnershipStore().PlanFrameworkOwnership(current, IntendedFramework());

        Assert.Equal(OwnershipWritePlanState.Unchanged, result.State);
        Assert.Null(result.Change);
    }

    private static WorkspaceOwnershipRead Read(
        WorkspaceOwnershipReadState state,
        WorkspaceOwnershipDocument document,
        FileStateSnapshot? snapshot,
        string? cause)
        => new(state, document, LockPath(), snapshot, cause);

    private static WorkspaceOwnershipDocument Document(FrameworkOwnership framework)
        => new(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            framework,
            [],
            []);

    private static FrameworkOwnership IntendedFramework()
        => new(
            new OwnedSource("open-forge", "1.0.0"),
            [".agents/loader.md"],
            [new OwnedRegion(".agents/directives/_directives.md", "entries")]);

    private static ExtensionOwnership IntendedExtension()
        => new(
            "planning",
            "1.0.0",
            "bundled",
            ["framework"],
            ["extensions/planning.md"],
            [new OwnedRegion("extensions/_extensions.md", "entries")]);

    private static byte[] ExpectedDocumentBytes(FrameworkOwnership framework)
        => WorkspaceOwnershipCodec.Write(Document(framework));

    private static string LockPath()
        => Path.Combine(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-ownership-store")),
            WorkspaceOwnershipDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar));
}
