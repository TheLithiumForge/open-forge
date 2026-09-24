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
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ExcludedMappedDestinationsAndFutureChildrenStayUntouched()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("private/review.md");
        workspace.Source("private/future/child.md");
        workspace.Source("public/active.md");
        workspace.RecordAt("docs", "private/review.md", "private/retired.md");
        workspace.Directory("docs/private");
        workspace.Write("docs/private/review.md", "Local review bytes.");
        workspace.Write("docs/private/retired.md", "Local retired bytes.");
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"docs/public\"],\"removedDirectories\":[\"docs/private\"]}");
        var reviewBytes = File.ReadAllBytes(workspace.Absolute("docs/private/review.md"));
        var retiredBytes = File.ReadAllBytes(workspace.Absolute("docs/private/retired.md"));
        var sourceBytes = File.ReadAllBytes(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/private/future/child.md"));

        try
        {
            var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(LibraryApplicationState.Applied, result.Result.Application.State);
            Assert.Equal("docs/public/active.md", Assert.Single(result.Result.Plan.Links).Path);
            Assert.DoesNotContain(result.Result.Plan.Links, link => link.Path.StartsWith("private/", StringComparison.Ordinal));
            Assert.DoesNotContain(result.Result.Plan.Directories, directory => directory.Path.StartsWith("docs/private/", StringComparison.Ordinal));
            Assert.Contains(result.Result.Findings, finding => finding.Code == LibrarySyncFindingCode.PathExcluded);
            Assert.All(result.Result.Findings.Where(finding => finding.Code == LibrarySyncFindingCode.PathExcluded),
                finding => Assert.Equal(CliSemanticStatus.Complete, finding.Status));
            Assert.Equal("Local review bytes.", File.ReadAllText(workspace.Absolute("docs/private/review.md")));
            Assert.Equal("Local retired bytes.", File.ReadAllText(workspace.Absolute("docs/private/retired.md")));
            Assert.Equal(reviewBytes, File.ReadAllBytes(workspace.Absolute("docs/private/review.md")));
            Assert.Equal(retiredBytes, File.ReadAllBytes(workspace.Absolute("docs/private/retired.md")));
            Assert.True(System.IO.Directory.Exists(workspace.Absolute("docs/private")));
            Assert.False(System.IO.Directory.Exists(workspace.Absolute("docs/private/future")));
            Assert.False(File.Exists(workspace.Absolute("docs/private/future/child.md")));
            Assert.Null(new FileInfo(workspace.Absolute("docs/private/review.md")).LinkTarget);
            Assert.Null(new FileInfo(workspace.Absolute("docs/private/retired.md")).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/private/future/child.md")));
            var registration = Assert.Single(Assert.IsType<LibraryRegistrationsDocument>(result.Result.Record.Intended).Libraries);
            Assert.Equal(["private/retired.md", "private/review.md", "public/active.md"], registration.Paths);
        }
        finally
        {
            var linkPath = workspace.Absolute("docs/public/active.md");
            if (new FileInfo(linkPath).LinkTarget is not null)
            {
                File.Delete(linkPath);
            }
            var publicDirectory = workspace.Absolute("docs/public");
            if (System.IO.Directory.Exists(publicDirectory)
                && !System.IO.Directory.EnumerateFileSystemEntries(publicDirectory).Any())
            {
                System.IO.Directory.Delete(publicDirectory);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ExcludedOwnedLinksAndTheirRecordedPathsAreNotPruned()
    {
        const string current = "private/review.md";
        const string retired = "private/retired.md";
        const string destinationRoot = "docs";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(current);
        workspace.Directory("docs/private");
        workspace.Write(".agents/open-forge.json", "{\"removedDirectories\":[\"docs/private\"]}");
        var currentDestination = workspace.Absolute("docs/private/review.md");
        var retiredDestination = workspace.Absolute("docs/private/retired.md");
        var currentSource = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{current}");
        var retiredSource = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{retired}");
        var destinationParent = System.IO.Path.GetDirectoryName(currentDestination)
            ?? throw new InvalidOperationException("The destination parent was not available.");
        var currentTarget = System.IO.Path.GetRelativePath(destinationParent, currentSource).Replace('\\', '/');
        var retiredTarget = System.IO.Path.GetRelativePath(destinationParent, retiredSource).Replace('\\', '/');
        workspace.Link("docs/private/review.md", currentTarget);
        workspace.Link("docs/private/retired.md", retiredTarget);
        workspace.RecordAt(destinationRoot, current, retired);
        var before = workspace.Snapshot();
        var ownershipBytes = File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.RecordPath));
        var sourceBytes = File.ReadAllBytes(currentSource);

        var result = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
            workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(2, result.Result.Findings.Count(finding => finding.Code == LibrarySyncFindingCode.PathExcluded));
        Assert.All(result.Result.Findings.Where(finding => finding.Code == LibrarySyncFindingCode.PathExcluded),
            finding => Assert.Equal(CliSemanticStatus.Complete, finding.Status));
        var intended = Assert.IsType<LibraryRegistrationsDocument>(result.Result.Record.Intended);
        Assert.Equal(
            new[] { retired, current }.Order(StringComparer.Ordinal),
            Assert.Single(intended.Libraries).Paths);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(ownershipBytes, File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
        Assert.Equal(currentTarget, new FileInfo(currentDestination).LinkTarget);
        Assert.Equal(retiredTarget, new FileInfo(retiredDestination).LinkTarget);
        Assert.Equal(sourceBytes, File.ReadAllBytes(currentSource));
        Assert.False(File.Exists(retiredSource));
    }

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
