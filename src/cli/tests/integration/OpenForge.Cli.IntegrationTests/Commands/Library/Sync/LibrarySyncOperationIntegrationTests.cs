using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Sync;

public sealed class LibrarySyncOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("file"), InlineData("directory"), InlineData("different-link"), InlineData("absolute-link")]
    public static async Task EveryNonExactOccupantRefusesAllEffects(string occupant)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        switch (occupant)
        {
            case "file": workspace.Write(LibraryMutationWorkspace.Leaf, "Local owner."); break;
            case "directory": workspace.Directory(LibraryMutationWorkspace.Leaf); break;
            case "different-link": workspace.Link(rawTarget: "../../another-source.md"); break;
            case "absolute-link": workspace.Link(rawTarget: workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")); break;
            default: throw new ArgumentOutOfRangeException(nameof(occupant));
        }

        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ChangedOrdinaryDestinationPreservesBytesAndCreatesIndependentLink()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source("three.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Write(LibraryMutationWorkspace.Leaf, "Local owner.");
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"three.md\"]}");

        try
        {
            var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(LibraryApplicationState.Applied, result.Result.Application.State);
            Assert.Equal("three.md", Assert.Single(result.Result.Plan.Links).Path);
            Assert.Equal(LibraryRecordEffect.Replace, result.Result.Plan.RecordEffect);
            Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.Equal("Local owner.", File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.Leaf)));
            Assert.NotNull(new FileInfo(workspace.Absolute("three.md")).LinkTarget);
            var registration = Assert.Single(Assert.IsType<LibraryRegistrationsDocument>(result.Result.Record.Intended).Libraries);
            Assert.Equal(
                [LibraryMutationWorkspace.Leaf, "three.md"],
                registration.Paths);
        }
        finally
        {
            if (new FileInfo(workspace.Absolute("three.md")).LinkTarget
                == "shared/team-knowledge/three.md")
            {
                File.Delete(workspace.Absolute("three.md"));
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task UnavailableSourceNeverRetiresProjection()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        System.IO.Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRetiredProjectionBlocksCompleteSourceInventory()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Sync requires an explicit final confirmation before apply"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public async Task MissingConfirmationCannotAuthorizeApply()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source("next.md");
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"next.md\",\".agents/directives\"]}");
        var before = workspace.Snapshot();

        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(LibrarySyncFindingCode.ConfirmationRequired, Assert.Single(result.Result.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Declining or ending sync confirmation leaves the workspace unchanged"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    [InlineData("no\n")]
    [InlineData("")]
    public async Task FinalConfirmationCancellationLeavesWorkspaceUnchanged(string finalAnswer)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source("next.md");
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        using var input = new StringReader($"always\n{finalAnswer}");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));

        var result = await new LibrarySyncOperation(
            permissions,
            LibraryPermissionTestPrompt.SyncTerminalConfirmation(input, output, canPrompt: true)).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibrarySyncFindingCode.Interrupted
            && finding.Cause == "Library sync was cancelled. Nothing was changed.");
        var review = output.ToString();
        var reviewPosition = review.IndexOf("Would synchronize team-knowledge:", StringComparison.Ordinal);
        var confirmationPosition = review.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(reviewPosition >= 0 && reviewPosition < confirmationPosition);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Cancelling sync permission admission reports sync cancellation"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public async Task PermissionPromptCancellationReportsSyncCancellation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("new.md");
        workspace.Source("kept.md");
        workspace.RecordAt("docs", "kept.md", "old.md");
        workspace.Link("docs/kept.md", "../shared/team-knowledge/kept.md");
        workspace.Link("docs/old.md", "../shared/team-knowledge/old.md");
        workspace.Write(".agents/open-forge.json", "{\"unknown\":true,\"allowInstallPaths\":[]}");
        var before = workspace.Snapshot();
        var sourceBefore = File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/new.md"));
        var settingsBefore = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));
        using var cancellation = new CancellationTokenSource();
        var promptRead = false;
        using var input = LibraryPermissionTestPrompt.CancelOnRead(cancellation, () => promptRead = true);
        using var output = new StringWriter();

        var result = await new LibrarySyncOperation(
            new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true))).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
                cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibrarySyncFindingCode.Interrupted
            && finding.Cause == "Library sync was cancelled. Nothing was changed.");
        Assert.True(promptRead);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Contains("docs   (directory: everything under it)", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("always, once, cancel:", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/new.md")));
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task SourceByteChangesDoNotReplaceLiveProjection()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.OwnershipAt(".", LibraryMutationWorkspace.Leaf);
        File.WriteAllText(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}"), "Changed source bytes.");
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryApplicationState.NoOp, result.Result.Application.State);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(LibraryRecordEffect.None, result.Result.Plan.RecordEffect);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContainedDirectoryLinkAncestryStillBlocksLibraryMutation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/local");
        workspace.Link(".agents/local/review.md", "../../shared/team-knowledge/.agents/directives/review.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.DirectoryLink(".agents/directives", "local");
        var before = workspace.Snapshot();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationReturnsInterruptedWithoutEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(workspace.Sync(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EligibleJsonIsAddedAndRecordedWithoutGeneratedNavigation()
    {
        const string jsonPath = ".agents/resources/data.json";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source(jsonPath);
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Directory(".agents/resources");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{jsonPath}");
        var sourceBytes = File.ReadAllBytes(sourcePath);

        try
        {
            var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Contains(result.Result.Source.EligiblePaths, path => path.SourcePath == jsonPath);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Plan.Links).Path);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.Equal(LibraryRecordEffect.Replace, result.Result.Plan.RecordEffect);
            Assert.Contains(jsonPath, Assert.Single(Assert.IsType<LibraryRegistrationsDocument>(
                result.Result.Record.Intended).Libraries).Paths);
            Assert.NotNull(new FileInfo(workspace.Absolute(jsonPath)).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        }
        finally
        {
            File.Delete(workspace.Absolute(jsonPath));
        }
    }
}
