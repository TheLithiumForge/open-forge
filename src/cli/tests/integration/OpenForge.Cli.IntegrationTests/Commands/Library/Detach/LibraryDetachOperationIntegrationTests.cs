using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Effects;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Detach;

public sealed class LibraryDetachOperationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task DetachingOneLibraryRetainsOtherOwnershipWithLeafPathsOnly()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.SourceAt("shared/other", ".agents/directives/survivor.md");
        workspace.Directory(".agents/directives");
        try
        {
            var first = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, first.Status);

            var second = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach("survivor", "shared/other", ".", LibraryMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, second.Status);

            var detached = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Detach("team-knowledge", LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, detached.Status);
            Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
            var library = Assert.Single(ownership.RootElement.GetProperty("libraries").EnumerateArray());
            Assert.Equal("survivor", library.GetProperty("id").GetString());
            Assert.Equal("shared/other", library.GetProperty("sourceRoot").GetString());
            Assert.Equal(".", library.GetProperty("destinationRoot").GetString());
            var paths = library.GetProperty("paths").EnumerateArray().Select(value => value.GetString()).ToArray();
            Assert.Equal([".agents/directives/survivor.md"], paths);
            Assert.All(paths, path => Assert.EndsWith(".md", path, StringComparison.Ordinal));
            Assert.True(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
        }
        finally
        {
            var detachedPath = workspace.Absolute(LibraryMutationWorkspace.Leaf);
            if (new FileInfo(detachedPath).LinkTarget is not null)
            {
                File.Delete(detachedPath);
            }
            var survivorPath = workspace.Absolute(".agents/directives/survivor.md");
            if (new FileInfo(survivorPath).LinkTarget is not null)
            {
                File.Delete(survivorPath);
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
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        var expectedOccupantKind = occupant switch
        {
            "file" => LibraryDetachOccupantKind.OrdinaryFile,
            "directory" => LibraryDetachOccupantKind.Folder,
            "different-link" or "absolute-link" => LibraryDetachOccupantKind.DifferentLink,
            _ => throw new ArgumentOutOfRangeException(nameof(occupant)),
        };
        var finding = Assert.Single(result.Result.Findings, finding => finding.Code == LibraryDetachFindingCode.MappingBlocked);
        Assert.Equal(expectedOccupantKind, finding.OccupantKind);
        if (occupant == "file")
        {
            Assert.Equal(CliSemanticStatus.Attention, result.Status);
            Assert.Equal(CliSemanticStatus.Attention, finding.Status);
            Assert.Equal(LibraryApplicationState.Applied, result.Result.Application.State);
            Assert.Equal(LibraryRecordEffect.Replace, result.Result.Plan.RecordEffect);
            Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.Equal("Local owner.", File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.Leaf)));
            using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
            Assert.Empty(ownership.RootElement.GetProperty("libraries").EnumerateArray());
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
            Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
            Assert.Equal(CliSemanticStatus.Blocked, finding.Status);
            Assert.Equal(before, workspace.Snapshot());
        }
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingSourceRootDoesNotPreventExactDanglingPlan()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        System.IO.Directory.Delete(workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents"));
        System.IO.Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot));
        var before = workspace.Snapshot();
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Result.Identity.SourceIndependent);
        Assert.Equal(LibraryLinkEffectKind.Delete, Assert.Single(result.Result.Plan.Links).Kind);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task MissingRegisteredLeafWarnsAndRemovesOtherExactLink()
    {
        const string other = "docs/other.md";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(other);
        workspace.Directory("docs");
        workspace.Directory(".agents/directives");
        workspace.Link(other);
        workspace.Record(LibraryMutationWorkspace.Leaf, other);
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"docs/other.md\"]}");
        var before = workspace.Snapshot();
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(LibraryApplicationState.Applied, result.Result.Application.State);
        Assert.Equal(other, Assert.Single(result.Result.Plan.Links).Path);
        Assert.Equal(LibraryDetachFindingCode.RegisteredLinkMissing, Assert.Single(result.Result.Findings).Code);
        Assert.Equal(CliSemanticStatus.Attention, Assert.Single(result.Result.Findings).Status);
        Assert.Null(new FileInfo(workspace.Absolute(other)).LinkTarget);
        using var ownership = JsonDocument.Parse(File.ReadAllText(workspace.Absolute(LibraryMutationWorkspace.OwnershipPath)));
        Assert.Empty(ownership.RootElement.GetProperty("libraries").EnumerateArray());
        Assert.NotEqual(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task DryRunMissingLeafWarnsAndPreservesExactLinkPlan()
    {
        const string other = "docs/other.md";
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source(other);
        workspace.Directory("docs");
        workspace.Directory(".agents/directives");
        workspace.Link(other);
        workspace.Record(LibraryMutationWorkspace.Leaf, other);
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\"docs/other.md\"]}");
        System.IO.Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        var before = workspace.Snapshot();

        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(LibraryMode.DryRun),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(other, Assert.Single(result.Result.Plan.Links).Path);
        Assert.Equal(LibraryDetachFindingCode.RegisteredLinkMissing, Assert.Single(result.Result.Findings).Code);
        Assert.Equal(CliSemanticStatus.Attention, Assert.Single(result.Result.Findings).Status);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Detach requires an explicit final confirmation before apply"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public async Task MissingConfirmationCannotAuthorizeApply()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source();
        workspace.Link();
        workspace.Record(LibraryMutationWorkspace.Leaf);
        workspace.Write(".agents/open-forge.json", "{\"allowInstallPaths\":[\".agents/directives\"]}");
        var before = workspace.Snapshot();

        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Detach(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Equal(LibraryDetachFindingCode.ConfirmationRequired, Assert.Single(result.Result.Findings).Code);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Declining or ending detach confirmation leaves the workspace unchanged"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    [InlineData("no\n")]
    [InlineData("")]
    public async Task FinalConfirmationCancellationLeavesWorkspaceUnchanged(string finalAnswer)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("review.md");
        workspace.RecordAt("docs", "review.md");
        workspace.Link("docs/review.md", "../shared/team-knowledge/review.md");
        var before = workspace.Snapshot();
        using var input = new StringReader($"always\n{finalAnswer}");
        using var output = new StringWriter();
        var permissions = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));

        var result = await new LibraryDetachOperation(
            permissions,
            LibraryPermissionTestPrompt.DetachTerminalConfirmation(input, output, canPrompt: true)).ExecuteAsync(
                workspace.Detach(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibraryDetachFindingCode.Interrupted
            && finding.Cause == "Library detach was cancelled. Nothing was changed.");
        var review = output.ToString();
        var reviewPosition = review.IndexOf(
            "Would detach team-knowledge: remove 1 link under docs.",
            StringComparison.Ordinal);
        var confirmationPosition = review.IndexOf("Remove the 1 links listed above? [y/N]", StringComparison.Ordinal);
        Assert.True(reviewPosition >= 0 && reviewPosition < confirmationPosition);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Cancelling detach permission admission reports detach cancellation"), Trait("Feature", "library-interaction"), Trait("Evidence", "Integration")]
    public async Task PermissionPromptCancellationReportsDetachCancellation()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Source("review.md");
        workspace.RecordAt("docs", "review.md");
        workspace.Link("docs/review.md", "../shared/team-knowledge/review.md");
        workspace.Write(".agents/open-forge.json", "{\"unknown\":true,\"allowInstallPaths\":[]}");
        var before = workspace.Snapshot();
        var sourceBefore = File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/review.md"));
        var settingsBefore = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));
        using var cancellation = new CancellationTokenSource();
        var promptRead = false;
        using var input = LibraryPermissionTestPrompt.CancelOnRead(cancellation, () => promptRead = true);
        using var output = new StringWriter();

        var result = await new LibraryDetachOperation(
            new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true))).ExecuteAsync(
                workspace.Detach(LibraryMode.Apply) with { AllowPrompt = true, Automatic = false },
                cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibraryDetachFindingCode.Interrupted
            && finding.Cause == "Library detach was cancelled. Nothing was changed.");
        Assert.True(promptRead);
        Assert.True(cancellation.IsCancellationRequested);
        Assert.Contains("docs/review.md", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("always, once, cancel:", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, File.ReadAllBytes(workspace.Absolute("shared/team-knowledge/review.md")));
        Assert.Equal(settingsBefore, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task UnregisteredMatchingLinkRemainsOutsideDeletionPlan()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Link();
        workspace.Link(".agents/directives/local.md");
        workspace.Record(LibraryMutationWorkspace.Leaf);
        var before = workspace.Snapshot();
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(LibraryMutationWorkspace.Leaf, Assert.Single(result.Result.Plan.Links).Path);
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
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);
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
        var result = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(workspace.Detach(LibraryMode.Apply), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(LibraryRecoveryState.NotRequested, result.Result.Application.Recovery.State);
        Assert.Equal(before, workspace.Snapshot());
    }
}
