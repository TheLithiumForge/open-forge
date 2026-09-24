using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update"), Trait("Evidence", "IntegrationRestoration")]
public sealed class UpdateDirectoryRestorationIntegrationTests
{
    private static readonly CommandOutputRenderers<Core.Commands.Update.Models.Result.UpdateResult> Renderers =
        CommandOutputRenderers<Core.Commands.Update.Models.Result.UpdateResult>.From(UpdatePresentation.Rendering);

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update recreates a removed Core category after its exclusion is cleared")]
    public async Task RestoresRemovedGuidanceDirectory()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-restore-guidance");

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Core.Commands.Update.Models.Result.UpdateVerificationState.Verified, result.Verification);
        Assert.True(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
        Assert.True(workspace.Exists(UpdateIntegrationWorkspace.RetiredCandidatePath));
        var restoredDirectory = Assert.Single(result.Effects, effect =>
            effect.Kind == UpdatePhysicalEffectKind.Directory
            && effect.Path == ".agents/guidance");
        Assert.Equal(UpdatePhysicalEffectAction.Create, restoredDirectory.Action);
        Assert.Equal(UpdatePhysicalEffectOutcome.Verified, restoredDirectory.Outcome);
        Assert.Equal(UpdatePhysicalEffectResidual.None, restoredDirectory.Residual);
        Assert.Contains(result.Effects, effect => effect.Kind == UpdatePhysicalEffectKind.File
            && effect.Path.StartsWith(".agents/guidance/", StringComparison.Ordinal));
        Renderers.MatchDetails(
            result,
            "category-directory-restored",
            result.Recovery.ResidualPath);
    }

    [Trait("Boundary", "Projection")]
    [Fact(DisplayName = "Update directory residuals follow the observed after-state")]
    public async Task DirectoryResidualFollowsObservedAfterState()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-directory-residual");
        var execution = await BuildExecutionAsync(workspace);
        var creation = Assert.Single(execution.DirectoryCreations);
        var intendedPath = IntendedPhysicalPath(execution, creation);
        var before = FileStateSnapshot.Missing(creation.LogicalPath);

        var absent = DirectoryCreationReceipt.VerificationFailed(
            creation,
            before,
            intendedPath,
            FileStateSnapshot.Missing(creation.LogicalPath),
            "The directory is absent after verification failed.");
        Assert.Equal(UpdatePhysicalEffectResidual.None, ProjectDirectoryEffect(execution, absent).Residual);

        var unexpectedFile = DirectoryCreationReceipt.VerificationFailed(
            creation,
            before,
            intendedPath,
            FileStateSnapshot.File(
                creation.LogicalPath,
                intendedPath,
                Encoding.UTF8.GetBytes("unexpected entry")),
            "A different entry remains at the directory target.");
        Assert.Equal(UpdatePhysicalEffectResidual.Retained, ProjectDirectoryEffect(execution, unexpectedFile).Residual);

        var unavailable = DirectoryCreationReceipt.VerificationUnavailable(
            creation,
            before,
            intendedPath,
            "The final directory state could not be observed.");
        Assert.Equal(UpdatePhysicalEffectResidual.Unknown, ProjectDirectoryEffect(execution, unavailable).Residual);
    }

    [Trait("Boundary", "Projection")]
    [Fact(DisplayName = "Update projects completion-unknown directory receipts from their observed state")]
    public async Task ProjectsCompletionUnknownDirectoryReceipts()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-unknown-directory-completion");
        var execution = await BuildExecutionAsync(workspace);
        var creation = Assert.Single(execution.DirectoryCreations);
        var intendedPath = IntendedPhysicalPath(execution, creation);
        var before = FileStateSnapshot.Missing(creation.LogicalPath);

        var unavailable = DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            intendedPath,
            after: null,
            "The attempted directory creation has no observable final state.");
        Assert.Equal(FilesystemEffectState.Unknown, unavailable.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, unavailable.VerificationState);
        var unavailableEffect = ProjectDirectoryEffect(execution, unavailable);
        Assert.Equal(UpdatePhysicalEffectOutcome.CompletionUnknown, unavailableEffect.Outcome);
        Assert.Equal(UpdatePhysicalEffectResidual.Unknown, unavailableEffect.Residual);

        var changedEntry = DirectoryCreationReceipt.CompletionUnknown(
            creation,
            before,
            intendedPath,
            FileStateSnapshot.File(
                creation.LogicalPath,
                intendedPath,
                Encoding.UTF8.GetBytes("changed entry")),
            "A different entry is observed after the attempted directory creation.");
        Assert.Equal(FilesystemEffectState.Unknown, changedEntry.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, changedEntry.VerificationState);
        var changedEntryEffect = ProjectDirectoryEffect(execution, changedEntry);
        Assert.Equal(UpdatePhysicalEffectOutcome.CompletionUnknown, changedEntryEffect.Outcome);
        Assert.Equal(UpdatePhysicalEffectResidual.Retained, changedEntryEffect.Residual);
    }

    [Trait("Boundary", "Projection")]
    [Fact(DisplayName = "Update projects partial directory application without publishing ownership")]
    public async Task ProjectsPartialDirectoryApplicationWithoutOwnershipPublication()
    {
        using var workspace = await RemoveAndClearExclusionsAsync(".agents/memory", "update-partial-directory-application");
        var execution = await BuildExecutionAsync(workspace);
        Assert.True(execution.DirectoryCreations.Count > 1);
        Assert.NotNull(execution.OwnershipChange);

        var earlierCreation = execution.DirectoryCreations[0];
        var failedCreation = execution.DirectoryCreations[1];
        Assert.Equal(Path.GetDirectoryName(failedCreation.LogicalPath), earlierCreation.LogicalPath);
        var earlierPath = IntendedPhysicalPath(execution, earlierCreation);
        var failedPath = IntendedPhysicalPath(execution, failedCreation);

        // Model the typed receipt prefix returned when a later planned directory fails.
        var attempt = new UpdateApplicationAttempt(
            [
                DirectoryCreationReceipt.Verified(
                    earlierCreation,
                    FileStateSnapshot.Missing(earlierCreation.LogicalPath),
                    earlierPath,
                    FileStateSnapshot.Directory(earlierCreation.LogicalPath, earlierPath)),
                DirectoryCreationReceipt.VerificationFailed(
                    failedCreation,
                    FileStateSnapshot.Missing(failedCreation.LogicalPath),
                    failedPath,
                    FileStateSnapshot.Missing(failedCreation.LogicalPath),
                    "The later planned directory is absent after verification failed."),
            ],
            [],
            OwnershipReceipt: null,
            Finding: new UpdateFinding(
                UpdateFindingCode.VerificationFailed,
                RelativePath(execution, failedCreation.LogicalPath),
                "The later planned directory is absent after verification failed."));

        var effects = UpdateApplicationResultProjector.Effects(execution, attempt);
        var directories = effects.Where(effect => effect.Kind == UpdatePhysicalEffectKind.Directory).ToArray();
        Assert.Equal(execution.DirectoryCreations.Count, directories.Length);
        Assert.Equal(UpdatePhysicalEffectOutcome.Verified, directories[0].Outcome);
        Assert.Equal(UpdatePhysicalEffectResidual.Retained, directories[0].Residual);
        Assert.Equal(UpdatePhysicalEffectOutcome.VerificationFailed, directories[1].Outcome);
        Assert.Equal(UpdatePhysicalEffectResidual.None, directories[1].Residual);
        Assert.All(directories.Skip(2), effect =>
        {
            Assert.Equal(UpdatePhysicalEffectOutcome.NotStarted, effect.Outcome);
            Assert.Equal(UpdatePhysicalEffectResidual.None, effect.Residual);
        });
        Assert.All(effects.Where(effect => effect.Kind == UpdatePhysicalEffectKind.File), effect =>
            Assert.Equal(UpdatePhysicalEffectOutcome.NotStarted, effect.Outcome));
        Assert.Null(attempt.OwnershipReceipt);
        Assert.Equal(
            UpdateLifecycleOutcome.NotStarted,
            UpdateApplicationResultProjector.Lifecycle(execution, attempt).Outcome);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update restores a missing directory with interpretable older Framework ownership metadata")]
    public async Task RestoresDirectoryWithOlderOwnershipSchemaAndDescription()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-restore-older-ownership");
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        ownership["schemaVersion"] = 0;
        ownership["description"] = "Older Framework registration with descriptive metadata.";
        workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.True(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
        Assert.Contains(result.Effects, effect =>
            effect.Kind == UpdatePhysicalEffectKind.Directory
            && effect.Path == ".agents/guidance"
            && effect.Outcome == UpdatePhysicalEffectOutcome.Verified);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == UpdateFindingCode.OwnershipObservation);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update previews missing parent creation without filesystem effects")]
    public async Task PreviewsMissingDirectoryWithoutWrites()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-preview-guidance");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(mode: UpdateMode.DryRun));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
        var plannedDirectory = Assert.Single(result.Effects, effect =>
            effect.Kind == UpdatePhysicalEffectKind.Directory
            && effect.Path == ".agents/guidance");
        Assert.Equal(UpdatePhysicalEffectOutcome.Planned, plannedDirectory.Outcome);
        Renderers.MatchDetails(result, "category-directory-restoration-preview");
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update keeps an excluded removed Core directory absent")]
    public async Task RemovedDirectoryExclusionStillSkipsRestoration()
    {
        using var workspace = await RemoveGuidanceAsync("update-excluded-guidance");
        var exclusionSettings = workspace.ReadText(".agents/open-forge.json");

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Core.Commands.Update.Models.Result.UpdateVerificationState.Verified, result.Verification);
        Assert.Equal(exclusionSettings, workspace.ReadText(".agents/open-forge.json"));
        Assert.False(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
        Assert.DoesNotContain(result.Effects, effect =>
            effect.Path == ".agents/guidance"
                || effect.Path.StartsWith(".agents/guidance/", StringComparison.Ordinal));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update plans a nested missing parent chain before restoring Core files")]
    public async Task RestoresNestedMissingParentChain()
    {
        using var workspace = await RemoveAndClearExclusionsAsync(".agents/memory", "update-restore-memory");

        var result = await workspace.ExecuteAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Core.Commands.Update.Models.Result.UpdateVerificationState.Verified, result.Verification);
        var directories = result.Effects
            .Where(effect => effect.Kind == UpdatePhysicalEffectKind.Directory)
            .ToArray();
        Assert.Contains(directories, effect => effect.Path == ".agents/memory");
        Assert.Contains(directories, effect => effect.Path == ".agents/memory/working");
        Assert.True(directories.Length > 1);
        Assert.All(directories, effect =>
        {
            Assert.Equal(UpdatePhysicalEffectAction.Create, effect.Action);
            Assert.Equal(UpdatePhysicalEffectOutcome.Verified, effect.Outcome);
        });
        Assert.True(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "memory", "working")));
        Assert.Contains(result.Effects, effect => effect.Kind == UpdatePhysicalEffectKind.File
            && effect.Path.StartsWith(".agents/memory/", StringComparison.Ordinal));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update refuses a parent changed after confirmation before writing payload files")]
    public async Task RefusesParentChangedAfterPreview()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-changed-guidance");
        IReadOnlyDictionary<string, string>? afterParentChange = null;

        var result = await workspace.ExecutePromptedAsync(
            workspace.Request(automatic: false, allowsInteractiveConfirmation: true),
            () =>
            {
                workspace.CreateDirectory(".agents/guidance");
                afterParentChange = workspace.SnapshotHashes();
            });

        Assert.NotNull(afterParentChange);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(afterParentChange, workspace.SnapshotHashes());
        Assert.Empty(Directory.EnumerateFileSystemEntries(
            Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update refuses a linked parent after confirmation without following it")]
    public async Task RefusesLinkedParentAfterPreview()
    {
        using var workspace = await RemoveGuidanceAndClearExclusionsAsync("update-linked-guidance");
        var link = Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance");
        var target = Path.Combine(workspace.Workspace.LexicalRoot, "linked-guidance-target");
        workspace.RegisterTestCleanupPath(".agents/guidance");
        workspace.RegisterTestCleanupPath("linked-guidance-target");
        IReadOnlyDictionary<string, string>? afterLink = null;
        Exception? linkFailure = null;
        var result = await workspace.ExecutePromptedAsync(
            workspace.Request(automatic: false, allowsInteractiveConfirmation: true),
            () =>
            {
                try
                {
                    Directory.CreateDirectory(target);
                    Directory.CreateSymbolicLink(link, target);
                    afterLink = workspace.SnapshotHashes();
                }
                catch (Exception exception) when (exception is UnauthorizedAccessException
                    or IOException
                    or NotSupportedException
                    or PlatformNotSupportedException)
                {
                    linkFailure = exception;
                }
            });

        if (linkFailure is not null)
        {
            Assert.Skip($"Directory symlinks are unavailable on this test host: {linkFailure.GetType().Name}.");
        }

        Assert.NotNull(afterLink);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == Core.Commands.Update.UpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(afterLink, workspace.SnapshotHashes());
        Assert.Empty(Directory.EnumerateFileSystemEntries(target));
    }

    private static async Task<UpdateIntegrationWorkspace> RemoveGuidanceAndClearExclusionsAsync(string purpose)
        => await RemoveAndClearExclusionsAsync(".agents/guidance", purpose);

    private static async Task<UpdatePlanExecution> BuildExecutionAsync(UpdateIntegrationWorkspace workspace)
    {
        var resolution = await UpdatePlanBuilder.Create().BuildExecutionAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);
        return Assert.IsType<UpdatePlanExecution>(resolution.Execution);
    }

    private static UpdatePhysicalEffect ProjectDirectoryEffect(
        UpdatePlanExecution execution,
        DirectoryCreationReceipt receipt)
        => Assert.Single(
            UpdateApplicationResultProjector.Effects(
                execution,
                new UpdateApplicationAttempt([receipt], [], OwnershipReceipt: null, Finding: null)),
            effect => effect.Kind == UpdatePhysicalEffectKind.Directory);

    private static string IntendedPhysicalPath(
        UpdatePlanExecution execution,
        PlannedDirectoryCreation creation)
        => Path.GetFullPath(Path.Combine(
            execution.Request.Workspace.PhysicalRoot,
            Path.GetRelativePath(execution.Request.Workspace.LexicalRoot, creation.LogicalPath)));

    private static string RelativePath(UpdatePlanExecution execution, string logicalPath)
        => Path.GetRelativePath(execution.Request.Workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');

    private static async Task<UpdateIntegrationWorkspace> RemoveAndClearExclusionsAsync(
        string target,
        string purpose)
    {
        var workspace = UpdateIntegrationWorkspace.Create(purpose);
        try
        {
            workspace.RegisterTestCleanupPath(target.Replace('/', Path.DirectorySeparatorChar));
            workspace.WriteText(".agents/open-forge.json", "{}\n");
            await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
            var remove = await workspace.RunRootAsync(["remove", target, "--automatic", "--format", "json"]);
            Assert.Equal(0, remove.ExitCode);
            Assert.False(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, target.Replace('/', Path.DirectorySeparatorChar))));
            workspace.ReplaceText(".agents/open-forge.json", "{}\n");
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task<UpdateIntegrationWorkspace> RemoveGuidanceAsync(string purpose)
    {
        var workspace = UpdateIntegrationWorkspace.Create(purpose);
        try
        {
            workspace.RegisterTestCleanupPath(".agents/guidance");
            workspace.WriteText(".agents/open-forge.json", "{}\n");
            await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
            var remove = await workspace.RunRootAsync(
                ["remove", ".agents/guidance", "--automatic", "--format", "json"]);
            Assert.Equal(0, remove.ExitCode);
            Assert.False(Directory.Exists(Path.Combine(workspace.Workspace.LexicalRoot, ".agents", "guidance")));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }
}
