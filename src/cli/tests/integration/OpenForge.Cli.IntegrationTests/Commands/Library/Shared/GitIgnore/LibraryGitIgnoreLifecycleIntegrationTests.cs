using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Interaction;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.GitIgnore;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class LibraryGitIgnoreLifecycleIntegrationTests
{
    private const string Begin = "# BEGIN OPEN FORGE LIBRARIES\n";
    private const string End = "# END OPEN FORGE LIBRARIES\n";

    [Fact]
    public static async Task AttachSyncAndDetachKeepExactLeavesAndPreserveSourceAndAuthoredRules()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        const string authored = "# authored\n/docs/a.md\n# BEGIN OPEN FORGE INSTALL\n/.agents/framework\n# END OPEN FORGE INSTALL\n";
        workspace.Write(".gitignore", authored);
        var attached = await Attach(workspace, "team-knowledge", "docs", true);
        Assert.Equal(CliSemanticStatus.Complete, attached.Status);
        Assert.Equal(authored + Begin + "/docs/a.md\n" + End, ReadIgnore(workspace));
        Assert.True(Assert.Single(Ownership(workspace)).GetProperty("gitIgnore").GetBoolean());
        Assert.Equal("../shared/team-knowledge/a.md", new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget);
        Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/a.md")));

        workspace.Source("b.md");
        var synced = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, synced.Status);
        Assert.Equal(authored + Begin + "/docs/a.md\n/docs/b.md\n" + End, ReadIgnore(workspace));
        File.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/a.md"));
        var retired = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, retired.Status);
        Assert.Equal(authored + Begin + "/docs/b.md\n" + End, ReadIgnore(workspace));
        Assert.False(File.Exists(workspace.Absolute("docs/a.md")));

        var detached = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, detached.Status);
        Assert.Equal(authored, ReadIgnore(workspace));
        Assert.Empty(Ownership(workspace));
        Assert.False(File.Exists(workspace.Absolute("docs/b.md")));
        Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/b.md")));
    }

    [Fact]
    public static async Task SharedDirectoryUnionReleasesOnlySelectedLibraryAndOptOutPreservesOtherIntent()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        workspace.SourceAt("shared/other", "b.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "other", "docs", true, "shared/other")).Status);
        Assert.Equal(Begin + "/docs/a.md\n/docs/b.md\n" + End, ReadIgnore(workspace));
        workspace.SourceAt("shared/local", "c.md");
        var ignore = ReadIgnore(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "local", "docs", false, "shared/local")).Status);
        Assert.Equal(ignore, ReadIgnore(workspace));
        Assert.True(Ownership(workspace).Single(library => library.GetProperty("id").GetString() == "other").GetProperty("gitIgnore").GetBoolean());
        Assert.False(Ownership(workspace).Single(library => library.GetProperty("id").GetString() == "local").TryGetProperty("gitIgnore", out _));

        Assert.Equal(CliSemanticStatus.Complete, (await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken)).Status);
        Assert.Equal(Begin + "/docs/b.md\n" + End, ReadIgnore(workspace));
        Assert.True(File.Exists(workspace.Absolute("docs/b.md")));
        Assert.True(File.Exists(workspace.Absolute("docs/c.md")));
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task RootAndSiblingDestinationsReceiveOnlyExactLeaves(bool sibling)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("leaf.md");
        var destination = sibling ? "knowledge" : ".";
        var result = await Attach(workspace, "team-knowledge", destination, true);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Begin + (sibling ? "/knowledge/leaf.md\n" : "/leaf.md\n") + End, ReadIgnore(workspace));
    }

    [Theory]
    [InlineData(false), InlineData(null)]
    public static async Task OptOutAndUnattendedOmissionDoNotReadUnrelatedMalformedOwnedRules(bool? selected)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        const string malformed = Begin + "/authored-unknown\n" + End;
        File.WriteAllText(workspace.Absolute(".gitignore"), malformed);
        workspace.SourceAt("shared/other", "b.md");
        var result = await Attach(workspace, "other", "docs", selected, "shared/other");
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(malformed, ReadIgnore(workspace));
        Assert.False(Ownership(workspace).Single(library => library.GetProperty("id").GetString() == "other").TryGetProperty("gitIgnore", out _));
        var sync = workspace.Sync(LibraryMode.Apply) with { LibraryId = LibraryId.Create("other") };
        Assert.Equal(CliSemanticStatus.Complete, (await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(sync, TestContext.Current.CancellationToken)).Status);
        Assert.Equal(CliSemanticStatus.Complete, (await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach("other", LibraryMode.Apply), TestContext.Current.CancellationToken)).Status);
        Assert.Equal(malformed, ReadIgnore(workspace));
        Assert.True(Assert.Single(Ownership(workspace)).GetProperty("gitIgnore").GetBoolean());
    }

    [Theory]
    [InlineData(true), InlineData(false)]
    public static async Task ChoicePrecedesCompletePermissionAndFinalReview(bool answer)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        var stages = new List<string>();
        var permission = new LibraryPermissionOperation((question, _, _) =>
        {
            stages.Add("permission");
            Assert.Equal(answer, question.Paths.Any(path => path.Path == ".gitignore"));
            return ValueTask.FromResult(CliPromptReply<CliPermissionChoice>.Answered(CliPermissionChoice.Once));
        });
        var operation = new LibraryAttachOperation(permission, (_, plan, _, _) =>
        {
            stages.Add("review");
            Assert.Equal(answer, plan.GitIgnore is not null);
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        }, new(true, _ =>
        {
            stages.Add("choice");
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(answer));
        }));
        var result = await operation.ExecuteAsync(workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { AllowPrompt = true, Automatic = false }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(new[] { "choice", "permission", "review" }, stages);
        Assert.Equal(answer, File.Exists(workspace.Absolute(".gitignore")));
        Assert.Equal(answer, Assert.Single(Ownership(workspace)).TryGetProperty("gitIgnore", out _));
    }

    [Fact]
    public static async Task CancelledChoiceHasNoEffectsAndUnavailableChoiceFallsBackToNo()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        var before = workspace.Snapshot();
        var cancelled = new LibraryAttachOperation(workspace.Permissions, ignoreChoice: new(true, _ => ValueTask.FromResult(CliPromptReply<bool>.Cancelled())));
        var request = workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { AllowPrompt = true, Automatic = false, Allow = ["docs"] };
        var result = await cancelled.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(before, workspace.Snapshot());
        var unavailable = new LibraryAttachOperation(workspace.Permissions, LibraryPermissionTestPrompt.AttachConfirmation(), new(true, _ => ValueTask.FromResult(CliPromptReply<bool>.Unavailable())));
        Assert.Equal(CliSemanticStatus.Complete, (await unavailable.ExecuteAsync(request, TestContext.Current.CancellationToken)).Status);
        Assert.False(File.Exists(workspace.Absolute(".gitignore")));
        Assert.False(Assert.Single(Ownership(workspace)).TryGetProperty("gitIgnore", out _));
    }

    [Theory]
    [InlineData("automatic"), InlineData("redirected"), InlineData("json"), InlineData("dry-run"), InlineData("explicit-false")]
    public static async Task NonPromptingModesAndExplicitNoSkipTheChoice(string mode)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        var operation = new LibraryAttachOperation(workspace.Permissions, LibraryPermissionTestPrompt.AttachConfirmation(), new(mode != "redirected", _ => throw new InvalidOperationException("No ignore question is authorized in this mode.")));
        var request = workspace.Attach("team-knowledge", "docs", mode == "dry-run" ? LibraryMode.DryRun : LibraryMode.Apply) with
        {
            Allow = ["docs"],
            Automatic = mode is "automatic" or "json",
            AllowPrompt = mode != "json",
            GitIgnore = mode == "explicit-false" ? false : null,
        };
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Result.Plan.GitIgnore);
        Assert.False(File.Exists(workspace.Absolute(".gitignore")));
        if (mode == "dry-run") Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
    }

    [Theory]
    [InlineData("unknown"), InlineData("duplicate"), InlineData("missing-end"), InlineData("excluded"), InlineData("no-grant")]
    public static async Task RequiredIgnoreUpdateBlocksBeforeAnyWorkspaceEffects(string unsafeCase)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        switch (unsafeCase)
        {
            case "unknown": workspace.Write(".gitignore", Begin + "/user\n" + End); break;
            case "duplicate": workspace.Write(".gitignore", Begin + "/docs/a.md\n/docs/a.md\n" + End); break;
            case "missing-end": workspace.Write(".gitignore", Begin); break;
            case "excluded": workspace.Write(".agents/open-forge.json", """{"removedFiles":[".gitignore"]}"""); break;
            case "no-grant": break;
            default: throw new ArgumentOutOfRangeException(nameof(unsafeCase));
        }
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { GitIgnore = true, Allow = unsafeCase == "no-grant" ? ["docs"] : ["docs", ".gitignore"] }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
    }

    [Theory]
    [InlineData(true), InlineData(null)]
    public static async Task IgnoreLinkOverlapIsRejectedBeforeAnyQuestion(bool? requested)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source(".gitignore");
        var operation = new LibraryAttachOperation(new((_, _, _) => throw new InvalidOperationException("Overlap must precede permission.")), (_, _, _, _) => throw new InvalidOperationException("Overlap must precede review."), new(true, _ => throw new InvalidOperationException("Overlap must precede choice.")));
        var result = await operation.ExecuteAsync(workspace.Attach(LibraryMode.Apply) with { GitIgnore = requested, AllowPrompt = true, Automatic = false }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.GitIgnoreBlocked);
        Assert.Null(new FileInfo(workspace.Absolute(".gitignore")).LinkTarget);
    }

    [Theory]
    [InlineData(false), InlineData(null)]
    public static async Task OptOutCanProjectOpaqueRootIgnoreContent(bool? requested)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source(".gitignore");
        var result = await Attach(workspace, "team-knowledge", ".", requested);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("shared/team-knowledge/.gitignore", new FileInfo(workspace.Absolute(".gitignore")).LinkTarget);
        Assert.False(Assert.Single(Ownership(workspace)).TryGetProperty("gitIgnore", out _));
    }

    [Theory]
    [InlineData("ignore"), InlineData("permission")]
    public static async Task DriftAfterReviewBlocksBeforeLinksAndOwnership(string changed)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        workspace.Write(".gitignore", "# authored\n");
        workspace.Write(".agents/open-forge.json", """{"allowInstallPaths":["docs",".gitignore"]}""");
        var operation = new LibraryAttachOperation(workspace.Permissions, (_, _, _, _) =>
        {
            if (changed == "ignore") workspace.Replace(".gitignore", "# changed\n");
            else workspace.Replace(".agents/open-forge.json", """{"allowInstallPaths":["docs"]}""");
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        });
        var result = await operation.ExecuteAsync(workspace.Attach("team-knowledge", "docs", LibraryMode.Apply) with { GitIgnore = true, Automatic = false, AllowPrompt = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.False(File.Exists(workspace.Absolute("docs/a.md")));
        Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
    }

    [Theory]
    [InlineData("link"), InlineData("missing"), InlineData("directory")]
    public static async Task RootRemoveReleasesOnlyRemovedLibraryClaims(string target)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        workspace.Source("b.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        if (target == "missing") File.Delete(workspace.Absolute("docs/a.md"));
        var result = await RemovePathOperationFactory.Create().ExecuteAsync(new(workspace.Workspace, target == "directory" ? "docs" : "docs/a.md", RemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Effects, effect => effect.Path == ".gitignore" && effect.Kind == "file" && effect.Action == "persist");
        Assert.Equal(target == "directory" ? string.Empty : Begin + "/docs/b.md\n" + End, ReadIgnore(workspace));
        var record = Assert.Single(Ownership(workspace));
        Assert.True(record.GetProperty("gitIgnore").GetBoolean());
        Assert.Equal(target == "directory" ? [] : new[] { "b.md" }, record.GetProperty("paths").EnumerateArray().Select(path => path.GetString()).ToArray());
    }

    [Fact]
    public static async Task RemovedIgnoreFileIsNotRecreatedByRemovalButBlocksRequiredSync()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        var removed = await RemovePathOperationFactory.Create().ExecuteAsync(new(workspace.Workspace, ".gitignore", RemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, removed.Status);
        Assert.Single(removed.Effects, effect => effect.Path == ".gitignore");
        Assert.False(File.Exists(workspace.Absolute(".gitignore")));
        var before = workspace.Snapshot();
        var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, sync.Status);
        Assert.Equal(before, workspace.Snapshot());
        var detach = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, detach.Status);
        Assert.False(File.Exists(workspace.Absolute(".gitignore")));
        Assert.Empty(Ownership(workspace));
    }

    [Fact]
    public static async Task SyncRecreatesMissingIgnoreWithoutChangingLinksOrOwnership()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        var record = File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath));
        var rawTarget = new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget;
        File.Delete(workspace.Absolute(".gitignore"));
        var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, sync.Status);
        Assert.Equal(Begin + "/docs/a.md\n" + End, ReadIgnore(workspace));
        Assert.Equal(record, File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
        Assert.Equal(rawTarget, new FileInfo(workspace.Absolute("docs/a.md")).LinkTarget);
    }

    [Fact]
    public static async Task EmptyInventoryPersistsTrueWithoutReadingOrManufacturingAnIgnoreSection()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Write(".gitignore", Begin + "/unrelated\n");
        var bytes = File.ReadAllBytes(workspace.Absolute(".gitignore"));
        var result = await Attach(workspace, "team-knowledge", "docs", true);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Null(result.Result.Plan.GitIgnore);
        Assert.Equal(bytes, File.ReadAllBytes(workspace.Absolute(".gitignore")));
        Assert.True(Assert.Single(Ownership(workspace)).GetProperty("gitIgnore").GetBoolean());
    }

    [Theory, Trait("Boundary", "Host")]
    [InlineData("attach"), InlineData("sync"), InlineData("detach")]
    public static async Task UnreadableRequiredIgnoreProducesIncompleteRenderedFindingsBeforeEffects(string command)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This owned-file read denial requires Windows file sharing.");
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        workspace.SourceAt("shared/other", "b.md");
        var before = workspace.Snapshot();
        var arguments = command == "attach"
            ? new[] { "library", "attach", "other", "shared/other", "--to", "docs", "--git-ignore", "true", "--dry-run", "--format=json" }
            : new[] { "library", command, "team-knowledge", "--dry-run", "--format=json" };
        using (var held = new FileStream(workspace.Absolute(".gitignore"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var result = await CliHostCapture.RunAsync(arguments, workspace.Path);
            Assert.Equal(3, result.ExitCode);
            Assert.Equal(string.Empty, result.Error);
            using var document = JsonDocument.Parse(result.Output);
            Assert.Equal("incomplete", document.RootElement.GetProperty("status").GetString());
            var finding = Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == $"library-{command}.git-ignore-unavailable");
            Assert.Equal("warning", finding.GetProperty("severity").GetString());
            Assert.Equal("Library Git-ignore rules are unavailable", finding.GetProperty("title").GetString());
            Assert.Contains("could not", finding.GetProperty("message").GetString(), StringComparison.Ordinal);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    private static async Task<LibraryAttachResult> Attach(LibraryMutationWorkspace workspace, string id, string destination, bool? ignore, string source = LibraryMutationWorkspace.SourceRoot)
        => await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(id, source, destination, LibraryMode.Apply) with { GitIgnore = ignore, Allow = [destination == "." ? "leaf.md" : destination, ".gitignore"] }, TestContext.Current.CancellationToken);

    [Fact]
    public static async Task RootRemoveAgentsLinkUsesConfirmedCompanionWriteWithoutANewIgnoreGrant()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source(".agents/guidance/leaf.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", ".", true)).Status);
        File.WriteAllText(workspace.Absolute(".agents/open-forge.json"), """{"allowInstallPaths":[]}""");
        var result = await RemovePathOperationFactory.Create().ExecuteAsync(new(workspace.Workspace, ".agents/guidance/leaf.md", RemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(string.Empty, ReadIgnore(workspace));
        Assert.True(Assert.Single(Ownership(workspace)).GetProperty("gitIgnore").GetBoolean());
        Assert.Empty(Assert.Single(Ownership(workspace)).GetProperty("paths").EnumerateArray());
        Assert.Equal(LibraryMutationWorkspace.SourceBytes, File.ReadAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents/guidance/leaf.md")));
    }

    [Fact]
    public static async Task RootRemoveKeepsAbsentIgnoreAbsentAndRejectsIgnoreDriftBeforeDeletingLinks()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var cleanup = new LibraryGitIgnoreCleanup(workspace);
        workspace.Source("a.md");
        workspace.Source("b.md");
        Assert.Equal(CliSemanticStatus.Complete, (await Attach(workspace, "team-knowledge", "docs", true)).Status);
        var drifted = await RemovePathOperationFactory.Create(confirmation: (_, _, _, _) =>
        {
            File.WriteAllText(workspace.Absolute(".gitignore"), "# authored drift\n" + Begin + "/docs/a.md\n/docs/b.md\n" + End);
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        }).ExecuteAsync(new(workspace.Workspace, "docs/a.md", RemoveMode.Apply, automatic: false, allowInteractiveConfirmation: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, drifted.Status);
        Assert.True(File.Exists(workspace.Absolute("docs/a.md")));
        File.Delete(workspace.Absolute(".gitignore"));
        var removed = await RemovePathOperationFactory.Create().ExecuteAsync(new(workspace.Workspace, "docs/a.md", RemoveMode.Apply, automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, removed.Status);
        Assert.DoesNotContain(removed.Effects, effect => effect.Path == ".gitignore");
        Assert.False(File.Exists(workspace.Absolute(".gitignore")));
        Assert.True(Assert.Single(Ownership(workspace)).GetProperty("gitIgnore").GetBoolean());
        Assert.Equal("b.md", Assert.Single(Assert.Single(Ownership(workspace)).GetProperty("paths").EnumerateArray()).GetString());
    }

    private static string ReadIgnore(LibraryMutationWorkspace workspace) => File.ReadAllText(workspace.Absolute(".gitignore"));

    private static JsonElement[] Ownership(LibraryMutationWorkspace workspace)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
        return document.RootElement.GetProperty("libraries").EnumerateArray().Select(library => library.Clone()).ToArray();
    }
}
