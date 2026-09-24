using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemovePathOperationRevalidationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Root Remove lease revalidation notices settings, ownership, or target changes after confirmation")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    [InlineData("settings")]
    [InlineData("ownership")]
    [InlineData("target")]
    public async Task ChangedInputsBlockBeforeRecoveryOrDeletion(string changedInput)
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create($"remove-root-revalidate-{changedInput}");
        workspace.WriteText("target.txt", "Original target bytes.\n");
        var confirmation = Confirm(() =>
        {
            switch (changedInput)
            {
                case "settings":
                    workspace.WriteText(".agents/open-forge.json", "{\"schemaVersion\":1,\"removedFiles\":[]}");
                    break;
                case "ownership":
                    workspace.WriteText(".agents/open-forge.lock.json", "{\"schemaVersion\":1,\"framework\":null,\"extensions\":[],\"libraries\":[]}");
                    break;
                case "target":
                    File.WriteAllText(workspace.Combine("target.txt"), "Changed target bytes.\n");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(changedInput), changedInput, "The test mutation is not defined.");
            }
        });

        var result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot, confirmation)
            .ExecuteAsync(
                new RemovePathRequest(workspace.Workspace, "target.txt", RemoveMode.Apply, automatic: false,
                    allowInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal("remove", result.Command);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.TargetChanged);
        Assert.True(File.Exists(workspace.Combine("target.txt")));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove cancellation stops before the workspace lease and recovery preparation")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ConfirmationCancellationDoesNotWrite()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-confirm-cancel");
        workspace.WriteText("target.txt", "Keep this target.\n");
        var before = workspace.SnapshotHashes();
        var operation = RemovePathOperationFactory.Create(
            workspace.LockStoreRoot,
            (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Cancelled()));

        var result = await operation.ExecuteAsync(
            new RemovePathRequest(workspace.Workspace, "target.txt", RemoveMode.Apply, automatic: false,
                allowInteractiveConfirmation: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal("remove", result.Command);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove lease revalidation notices a newly inventoried directory child")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ChangedDirectoryInventoryBlocksBeforeRecovery()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-revalidate-directory");
        workspace.WriteText("tree/first.txt", "First child.\n");
        var confirmation = Confirm(() => workspace.WriteText("tree/late.txt", "Added after initial inventory.\n"));

        var result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot, confirmation)
            .ExecuteAsync(
                new RemovePathRequest(workspace.Workspace, "tree", RemoveMode.Apply, automatic: false,
                    allowInteractiveConfirmation: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.TargetChanged);
        Assert.Equal("First child.\n", File.ReadAllText(workspace.Combine("tree/first.txt")));
        Assert.Equal("Added after initial inventory.\n", File.ReadAllText(workspace.Combine("tree/late.txt")));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Root Remove reports completed target deletions and retains recovery after a later blocked deletion")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task WriteFailureIsPartialAndDoesNotRollBackSettings()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-partial-write");
        var removedBytes = new byte[] { 4, 0, 255, 13, 10 };
        var lockedBytes = new byte[] { 0, 255, 13, 10, 7 };
        workspace.WriteBytes("batch/a-first.bin", removedBytes);
        workspace.WriteBytes("batch/z-locked.bin", lockedBytes);
        FileStream? held = null;
        var confirmation = Confirm(() =>
        {
            held = new FileStream(
                workspace.Combine("batch/z-locked.bin"),
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read | FileShare.Write);
        });

        RemoveResult result;
        try
        {
            result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot, confirmation)
                .ExecuteAsync(
                    new RemovePathRequest(workspace.Workspace, "batch", RemoveMode.Apply, automatic: false,
                        allowInteractiveConfirmation: true),
                    TestContext.Current.CancellationToken);
        }
        finally
        {
            held?.Dispose();
        }

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.WriteFailed);
        Assert.False(File.Exists(workspace.Combine("batch/a-first.bin")));
        Assert.True(File.Exists(workspace.Combine("batch/z-locked.bin")));
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.json")));
        Assert.Equal(["batch/a-first.bin"], result.Removed);
        Assert.Equal("retained", result.RecoveryDisposition);
        Assert.NotNull(result.RecoveryPath);
        Assert.Equal(lockedBytes, File.ReadAllBytes(workspace.Combine("batch/z-locked.bin")));
        var finalRead = await OpenForge.Cli.Core.Framework.Recovery.RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            result.RecoveryPath!,
            TestContext.Current.CancellationToken);
        Assert.Equal(OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue.RecoveryBundleReadState.Valid, finalRead.State);
        var entries = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue.RecoveryBundleVerifiedRead>(finalRead.Verified).Entries;
        Assert.Contains(entries, entry => entry.TargetPath == "batch/a-first.bin"
            && entry.Prior.OrdinaryFile?.Sha256 == Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(removedBytes)).ToLowerInvariant());
        Assert.Contains(entries, entry => entry.TargetPath == "batch/z-locked.bin"
            && entry.Prior.OrdinaryFile?.Sha256 == Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(lockedBytes)).ToLowerInvariant());
    }

    private static CliPlanConfirmation<RemoveResult, RemoveConfirmationQuestion> Confirm(Action beforeAnswer)
        => (_, _, _, _) =>
        {
            beforeAnswer();
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        };
}
