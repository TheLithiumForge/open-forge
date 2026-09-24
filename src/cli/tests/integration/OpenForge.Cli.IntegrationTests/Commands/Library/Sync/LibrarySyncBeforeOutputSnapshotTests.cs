using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Sync;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class LibrarySyncBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<LibrarySyncResult> Renderers = CommandOutputRenderers<LibrarySyncResult>.From(LibrarySyncPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library sync output preserves an inaccessible source inventory without effects")]
    public async Task SourceUnreadable()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This source enumeration boundary uses an owned Windows directory ACL.");
            return;
        }
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Link();
        var before = workspace.Snapshot();
        LibrarySyncResult result;
        using (var denied = WindowsDirectoryEnumerationDenial.Create(workspace.Path,
                   workspace.Absolute(LibraryMutationWorkspace.SourceRoot + "/.agents")))
        {
            result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(before, workspace.Snapshot());
        Renderers.MatchDetails(result, "source-unreadable");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library sync output preserves link additions, removals, previews and safety boundaries")]
    [InlineData("up-to-date", (int)CliSemanticStatus.Complete)]
    [InlineData("links-added", (int)CliSemanticStatus.Complete)]
    [InlineData("links-removed", (int)CliSemanticStatus.Complete)]
    [InlineData("both", (int)CliSemanticStatus.Complete)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("unknown-id", (int)CliSemanticStatus.Invalid)]
    [InlineData("changed-occupant", (int)CliSemanticStatus.Blocked)]
    [InlineData("registered-link-gone", (int)CliSemanticStatus.Attention)]
    [InlineData("permission-required", (int)CliSemanticStatus.Blocked)]
    [InlineData("no-ownership-record", (int)CliSemanticStatus.Complete)]
    [InlineData("removed-library-id", (int)CliSemanticStatus.Blocked)]
    [InlineData("excluded-destination", (int)CliSemanticStatus.Complete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    [InlineData("write-failed-partial", (int)CliSemanticStatus.Failed)]
    public async Task Synchronize(string situation, int status)
    {
        if (situation == "write-failed-partial" && !OperatingSystem.IsWindows())
            Assert.Skip("This deterministic record replacement failure requires Windows file sharing.");
        using var workspace = new LibraryMutationWorkspace();
        using var artifacts = new LibraryOutputArtifacts(workspace);
        const string added = ".agents/directives/added.md";
        var target = situation == "permission-required" ? "docs/review.md" : LibraryMutationWorkspace.Leaf;
        workspace.Directory(situation == "permission-required" ? "docs" : ".agents/directives");
        if (situation is not ("links-removed" or "both")) workspace.Source(target);
        if (situation != "no-ownership-record") workspace.Record(situation is "links-added" or "write-failed-partial" ? [] : [target]);
        if (situation == "removed-library-id") workspace.Write(".agents/open-forge.json", "{\"removedLibraries\":[\"team-knowledge\"]}");
        else if (situation == "excluded-destination") workspace.Write(".agents/open-forge.json", "{\"removedFiles\":[\".agents/directives/review.md\"]}");
        if (situation == "changed-occupant") workspace.Write(target, "local changed occupant\n");
        else if (situation is not ("links-added" or "registered-link-gone" or "no-ownership-record" or "write-failed-partial")) workspace.Link(target);
        if (situation is "both" or "dry-run" or "lock-held") workspace.Source(added);
        artifacts.OwnLink(target);
        artifacts.OwnLink(added);
        var before = workspace.Snapshot();
        var sourceBefore = before.Where(pair => pair.Key.StartsWith(LibraryMutationWorkspace.SourceRoot + "/", StringComparison.Ordinal)).ToArray();
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        var request = workspace.Sync(situation == "dry-run" ? LibraryMode.DryRun : LibraryMode.Apply) with
        {
            LibraryId = LibraryId.Create(situation == "unknown-id" ? "unknown" : "team-knowledge"),
        };
        LibrarySyncResult result;
        using (var held = situation == "lock-held" ? artifacts.HoldLock() : null)
        using (var denied = situation == "write-failed-partial"
                   ? File.Open(workspace.Absolute(LibraryMutationWorkspace.RecordPath), FileMode.Open, FileAccess.Read, FileShare.Read) : null)
        {
            result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(request, cancellation.Token);
        }
        artifacts.OwnRecovery(result.Result.Application.Recovery);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "links-added" or "registered-link-gone" or "write-failed-partial") Assert.NotNull(new FileInfo(workspace.Absolute(target)).LinkTarget);
        else if (situation == "links-removed") Assert.Null(new FileInfo(workspace.Absolute(target)).LinkTarget);
        else if (situation == "both")
        {
            Assert.Null(new FileInfo(workspace.Absolute(target)).LinkTarget);
            Assert.NotNull(new FileInfo(workspace.Absolute(added)).LinkTarget);
        }
        else Assert.Equal(before, workspace.Snapshot());
        if (situation == "write-failed-partial")
        {
            Assert.Equal(before[LibraryMutationWorkspace.RecordPath], workspace.Snapshot()[LibraryMutationWorkspace.RecordPath]);
            Assert.NotNull(result.Result.Application.Recovery.Path);
        }
        Assert.Equal(sourceBefore, workspace.Snapshot().Where(pair => pair.Key.StartsWith(LibraryMutationWorkspace.SourceRoot + "/", StringComparison.Ordinal)).ToArray());
        Renderers.MatchDetails(
            result,
            situation,
            result.Result.Application.Recovery.Path,
            testName: $"{nameof(Synchronize)}_{situation}");
    }
}
