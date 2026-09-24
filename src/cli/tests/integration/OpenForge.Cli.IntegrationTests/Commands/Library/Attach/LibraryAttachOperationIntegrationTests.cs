using System.IO;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
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

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ExcludedLibraryIdBlocksAttachUntilSettingsAreManuallyChanged()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"docs\"],\"removedLibraries\":[\"team-knowledge\"]}");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/README.md");
        var sourceBytes = File.ReadAllBytes(sourcePath);
        var before = workspace.Snapshot();

        var blocked = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply) with { DestinationRoot = LibraryDestinationRoot.Create("docs") },
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Contains(blocked.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.LibraryRemoved);
        Assert.Equal(LibraryApplicationState.NotStarted, blocked.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());

        workspace.Replace(".agents/open-forge.json", "{\"allowInstallPaths\":[\"docs\"],\"removedLibraries\":[]}");
        try
        {
            var restored = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply) with { DestinationRoot = LibraryDestinationRoot.Create("docs") },
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, restored.Status);
            Assert.Equal("docs/README.md", Assert.Single(restored.Result.Plan.Links).Path);
            Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
            Assert.Equal("../shared/team-knowledge/README.md", new FileInfo(workspace.Absolute("docs/README.md")).LinkTarget);
        }
        finally
        {
            var linkPath = workspace.Absolute("docs/README.md");
            if (new FileInfo(linkPath).LinkTarget == "../shared/team-knowledge/README.md")
            {
                File.Delete(linkPath);
            }
            var destinationPath = workspace.Absolute("docs");
            if (System.IO.Directory.Exists(destinationPath)
                && !System.IO.Directory.EnumerateFileSystemEntries(destinationPath).Any())
            {
                System.IO.Directory.Delete(destinationPath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ExcludedMappedDestinationCompletesAsInformationalWithoutCreatingLinks()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        workspace.Write(".agents/open-forge.json", "{\"removedFiles\":[\"docs/README.md\"]}");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/README.md");
        var sourceBytes = File.ReadAllBytes(sourcePath);
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach("team-knowledge", LibraryMutationWorkspace.SourceRoot, "docs", LibraryMode.DryRun),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var excluded = Assert.Single(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.PathExcluded);
        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.Equal("docs/README.md", excluded.Path);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        Assert.Null(new FileInfo(workspace.Absolute("docs/README.md")).LinkTarget);
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task RepeatedAttachLeavesOwnershipLockByteIdentical()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/directives");
        try
        {
            var first = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, first.Status);
            var afterFirst = File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath));

            var second = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Blocked, second.Status);
            Assert.Equal(afterFirst, File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
        }
        finally
        {
            if (new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget
                == "../../shared/team-knowledge/.agents/directives/review.md")
            {
                File.Delete(workspace.Absolute(LibraryMutationWorkspace.Leaf));
            }
            if (File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)))
            {
                File.Delete(workspace.Absolute(LibraryMutationWorkspace.RecordPath));
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("file"), InlineData("directory"), InlineData("different-link"), InlineData("absolute-link")]
    public static async Task EveryNonExactOccupantRefusesAllEffects(string occupant)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        switch (occupant)
        {
            case "file": workspace.Write(LibraryMutationWorkspace.Leaf, "Local owner."); break;
            case "directory": workspace.Directory(LibraryMutationWorkspace.Leaf); break;
            case "different-link": workspace.Link(rawTarget: "../../another-source.md"); break;
            case "absolute-link": workspace.Link(rawTarget: workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}")); break;
            default: throw new ArgumentOutOfRangeException(nameof(occupant));
        }

        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EmptySourceWithoutNamedChildIsComplete()
    {
        using var workspace = new LibraryMutationWorkspace();
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Attach requires an explicit final confirmation before apply"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public static async Task MissingConfirmationCannotAuthorizeApply()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/directives");
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply) with
            {
                AllowPrompt = true,
                Automatic = false,
                Allow = [".agents/directives"],
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.ConfirmationRequired);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Declining or ending attach confirmation leaves the workspace unchanged"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    [InlineData("no\n")]
    [InlineData("")]
    public static async Task FinalConfirmationCancellationLeavesWorkspaceUnchanged(string finalAnswer)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        var destination = LibraryDestinationRoot.Create("docs");
        var before = workspace.Snapshot();
        using var input = new StringReader($"always\n{finalAnswer}");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));

        var result = await new LibraryAttachOperation(
            permissions,
            LibraryPermissionTestPrompt.AttachTerminalConfirmation(input, output, canPrompt: true)).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply) with
                {
                    DestinationRoot = destination,
                    AllowPrompt = true,
                    Automatic = false,
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.Interrupted
            && finding.Cause == "Library attach was cancelled. Nothing was changed.");
        var review = output.ToString();
        var reviewPosition = review.IndexOf("Would register the team-knowledge Library from shared/team-knowledge.", StringComparison.Ordinal);
        var confirmationPosition = review.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(reviewPosition >= 0 && reviewPosition < confirmationPosition);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Cancelling attach permission admission reports attach cancellation"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public static async Task PermissionPromptCancellationReportsAttachCancellation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("README.md");
        workspace.Write(".agents/open-forge.json", "{\"unknown\":true,\"allowInstallPaths\":[]}");
        var before = workspace.Snapshot();
        var sourceBefore = File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/README.md"));
        var settingsBefore = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));
        using var cancellation = new CancellationTokenSource();
        var promptRead = false;
        using var input = LibraryPermissionTestPrompt.CancelOnRead(cancellation, () => promptRead = true);
        using var output = new StringWriter();

        var result = await new LibraryAttachOperation(
            new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true))).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply) with
                {
                    DestinationRoot = LibraryDestinationRoot.Create("docs"),
                    AllowPrompt = true,
                    Automatic = false,
                },
                cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.Interrupted
            && finding.Cause == "Library attach was cancelled. Nothing was changed.");
        Assert.True(promptRead);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Contains("docs   (directory: everything under it)", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("always, once, cancel:", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/README.md")));
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task SourceControlsAndLinksNeverBecomeProjections()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/loader.md", "# Loader");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/directives/_directives.md", "# Directives");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/directives/review.overwrite.md", "# Private override");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/open-forge.json", "{}");
        workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/.agents/open-forge.lock.json", "{}");
        workspace.Link($"{LibraryMutationWorkspace.SourceRoot}/.agents/linked.md", "directives/review.md");
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryMutationWorkspace.Leaf, Assert.Single(result.Result.Source.EligiblePaths).SourcePath);
        Assert.Equal(6, result.Result.Source.ExcludedPaths.Length);
        Assert.Empty(result.Result.Plan.GeneratedRegions);
        Assert.False(File.Exists(workspace.Absolute(".agents/loader.md")));
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task DryRunIncludesParentsAndPreservesLocalSibling()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Source(".agents/guidance/nested/new.md");
        workspace.Write(".agents/directives/local.md", "Local sibling.");
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, result.Result.Plan.Links.Length);
        Assert.Contains(result.Result.Plan.Directories, directory => directory.Path == ".agents/guidance/nested");
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task ContainedDirectoryLinkAncestryStillBlocksLibraryMutation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/local");
        workspace.DirectoryLink(".agents/directives", "local");
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
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
        var before = workspace.Snapshot();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task EligibleJsonIsLinkedAndRecordedWithoutGeneratedNavigation()
    {
        const string jsonPath = ".agents/resources/data.json";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(jsonPath);
        workspace.Directory(".agents/resources");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{jsonPath}");
        var sourceBytes = File.ReadAllBytes(sourcePath);

        try
        {
            var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Source.EligiblePaths).SourcePath);
            Assert.Equal(jsonPath, Assert.Single(result.Result.Plan.Links).Path);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.Equal(LibraryRecordEffect.Create, result.Result.Plan.RecordEffect);
            Assert.Contains(jsonPath, Assert.Single(Assert.IsType<LibraryRegistrationsDocument>(
                result.Result.Record.Intended).Libraries).Paths);
            Assert.NotNull(new FileInfo(workspace.Absolute(jsonPath)).LinkTarget);
            Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
        }
        finally
        {
            File.Delete(workspace.Absolute(jsonPath));
            File.Delete(workspace.Absolute(LibraryMutationWorkspace.RecordPath));
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MalformedOwnershipBlocksAttachAndPreservesWorkspace()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Directory(".agents/directives");
        workspace.Write(LibraryMutationWorkspace.RecordPath, "{ malformed ownership record");
        var sourcePath = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}");
        var sourceBytes = File.ReadAllBytes(sourcePath);
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecordEffect.None, result.Result.Plan.RecordEffect);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.RecordInvalid
            && finding.Status == CliSemanticStatus.Blocked);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBytes, File.ReadAllBytes(sourcePath));
    }
}
