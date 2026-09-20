using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Detach;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class LibraryDetachBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<LibraryDetachResult> Renderers = CommandOutputRenderers<LibraryDetachResult>.From(LibraryDetachPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library detach output preserves exact link removal and ownership safety")]
    [InlineData("detached", (int)CliSemanticStatus.Complete)]
    [InlineData("detached-no-links", (int)CliSemanticStatus.Complete)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("unknown-id", (int)CliSemanticStatus.Invalid)]
    [InlineData("registered-link-gone", (int)CliSemanticStatus.Attention)]
    [InlineData("changed-occupant", (int)CliSemanticStatus.Attention)]
    [InlineData("destination-protected", (int)CliSemanticStatus.Blocked)]
    [InlineData("permission-required", (int)CliSemanticStatus.Blocked)]
    [InlineData("no-ownership-record", (int)CliSemanticStatus.Complete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    [InlineData("write-failed-partial", (int)CliSemanticStatus.Failed)]
    public async Task Detach(string situation, int status)
    {
        if (situation == "write-failed-partial" && !OperatingSystem.IsWindows())
            Assert.Skip("This deterministic record replacement failure requires Windows file sharing.");
        using var workspace = new LibraryMutationWorkspace();
        using var artifacts = new LibraryOutputArtifacts(workspace);
        var target = situation switch
        {
            "permission-required" => "docs/review.md",
            "destination-protected" => ".agents/loader.md",
            _ => LibraryMutationWorkspace.Leaf,
        };
        workspace.Directory(situation switch
        {
            "permission-required" => "docs",
            "destination-protected" => LibraryMutationWorkspace.SourceRoot,
            _ => ".agents/directives",
        });
        workspace.Source(target);
        if (situation != "no-ownership-record")
        {
            if (situation == "destination-protected")
            {
                workspace.Record(target);
            }
            else
            {
                workspace.Record(situation == "detached-no-links" ? [] : [target]);
            }
        }
        if (situation == "changed-occupant") workspace.Write(target, "local changed occupant\n");
        else if (situation is not ("detached-no-links" or "registered-link-gone" or "no-ownership-record" or "destination-protected")) workspace.Link(target);
        artifacts.OwnLink(target);
        var before = workspace.Snapshot();
        var changedOccupantBefore = situation == "changed-occupant"
            ? File.ReadAllBytes(workspace.Absolute(target))
            : null;
        var sourceBefore = File.ReadAllBytes(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{target}"));
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        var request = workspace.Detach(situation == "dry-run" ? LibraryMode.DryRun : LibraryMode.Apply) with
        {
            LibraryId = LibraryId.Create(situation == "unknown-id" ? "unknown" : "team-knowledge"),
        };
        LibraryDetachResult result;
        using (var held = situation == "lock-held" ? artifacts.HoldLock() : null)
        using (var denied = situation == "write-failed-partial"
                   ? File.Open(workspace.Absolute(LibraryMutationWorkspace.RecordPath), FileMode.Open, FileAccess.Read, FileShare.Read) : null)
        {
            result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(request, cancellation.Token);
        }
        artifacts.OwnRecovery(result.Result.Application.Recovery);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "detached" or "detached-no-links" or "write-failed-partial") Assert.Null(new FileInfo(workspace.Absolute(target)).LinkTarget);
        else if (situation is "registered-link-gone" or "changed-occupant")
        {
            var after = workspace.Snapshot();
            Assert.Equal(LibraryApplicationState.Applied, result.Result.Application.State);
            Assert.Empty(result.Result.Plan.Links);
            Assert.Equal(
                before
                    .Where(pair => !string.Equals(pair.Key, LibraryMutationWorkspace.RecordPath, StringComparison.Ordinal))
                    .ToArray(),
                after
                    .Where(pair => !string.Equals(pair.Key, LibraryMutationWorkspace.RecordPath, StringComparison.Ordinal))
                    .ToArray());
            if (situation == "registered-link-gone")
            {
                Assert.False(File.Exists(workspace.Absolute(target)));
            }
            else
            {
                Assert.NotNull(changedOccupantBefore);
                Assert.Null(new FileInfo(workspace.Absolute(target)).LinkTarget);
                Assert.Equal(changedOccupantBefore, File.ReadAllBytes(workspace.Absolute(target)));
            }

            using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            Assert.Empty(ownership.RootElement.GetProperty("libraries").EnumerateArray());
        }
        else Assert.Equal(before, workspace.Snapshot());
        if (situation == "write-failed-partial")
        {
            Assert.Equal(before[LibraryMutationWorkspace.RecordPath], workspace.Snapshot()[LibraryMutationWorkspace.RecordPath]);
            Assert.NotNull(result.Result.Application.Recovery.Path);
        }
        Assert.Equal(sourceBefore, File.ReadAllBytes(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{target}")));
        Renderers.MatchDetails(
            result,
            situation,
            result.Result.Application.Recovery.Path,
            testName: $"{nameof(Detach)}_{situation}");
    }
}
