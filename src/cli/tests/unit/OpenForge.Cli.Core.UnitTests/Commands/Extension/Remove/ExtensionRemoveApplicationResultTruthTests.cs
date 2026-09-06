using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationResultTruthTests
{
    [Fact(DisplayName = "Extension Remove effect receipts preserve retained and unknown residual truth"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior")]
    public void EffectReceiptsPreserveResidualTruth()
    {
        var (change, before) = DeleteFixture();
        var effect = Effect();
        var notStarted = FileChangeReceipt.NotStarted(
            change,
            before,
            FilesystemNotStartedReason.ApplicationFailed,
            "The target effect did not start.");
        var unknown = FileChangeReceipt.CompletionUnknown(
            change,
            before,
            after: null,
            "The target effect completion is unknown.");

        var retained = ExtensionRemoveApplicationResultFactory.WithOutcome(
            effect,
            ExtensionRemoveApplicationResultFactory.ReadOutcome(notStarted));
        var uncertain = ExtensionRemoveApplicationResultFactory.WithOutcome(
            effect,
            ExtensionRemoveApplicationResultFactory.ReadOutcome(unknown));

        Assert.Equal(ExtensionRemoveEffectOutcome.NotStarted, retained.Outcome);
        Assert.Equal(ExtensionRemoveEffectResidual.Retained, retained.Residual);
        Assert.Equal(ExtensionRemoveEffectOutcome.CompletionUnknown, uncertain.Outcome);
        Assert.Equal(ExtensionRemoveEffectResidual.Unknown, uncertain.Residual);
    }

    [Fact(DisplayName = "Extension Remove lifecycle receipts preserve cancellation and unknown publication truth"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior")]
    public void LifecycleReceiptsPreserveCancellationAndUnknownTruth()
    {
        var (change, before) = DeleteFixture();
        var cancelled = FileChangeReceipt.NotStarted(
            change,
            before,
            FilesystemNotStartedReason.Cancelled,
            "Lifecycle publication was cancelled.");
        var unknown = FileChangeReceipt.CompletionUnknown(
            change,
            before,
            after: null,
            "Lifecycle publication completion is unknown.");

        Assert.Equal(
            ExtensionRemoveLifecycleOutcome.NotStarted,
            ExtensionRemoveApplicationResultFactory.ReadLifecycleOutcome(cancelled));
        Assert.Equal(
            ExtensionRemoveFindingCode.Interrupted,
            ExtensionRemoveApplicationResultFactory.ReadLifecycleFinding(cancelled));
        Assert.Equal(
            ExtensionRemoveLifecycleOutcome.CompletionUnknown,
            ExtensionRemoveApplicationResultFactory.ReadLifecycleOutcome(unknown));
        Assert.Equal(
            ExtensionRemoveFindingCode.LifecyclePublicationFailed,
            ExtensionRemoveApplicationResultFactory.ReadLifecycleFinding(unknown));
    }

    [Fact(DisplayName = "Extension Remove final verification keeps failed and unknown states distinct"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior")]
    public void FinalVerificationKeepsFailedAndUnknownDistinct()
    {
        var failed = ExtensionRemoveApplicationResultFactory.FailedVerification();
        var unknown = ExtensionRemoveApplicationResultFactory.UnknownVerification();

        Assert.Equal(ExtensionRemoveVerificationState.Failed, failed.Targets);
        Assert.Equal(ExtensionRemoveVerificationState.Failed, failed.Topology);
        Assert.Equal(ExtensionRemoveVerificationState.Failed, failed.ExtensionsLifecycle);
        Assert.Equal(ExtensionRemoveVerificationState.Unknown, unknown.Targets);
        Assert.Equal(ExtensionRemoveVerificationState.Unknown, unknown.Topology);
        Assert.Equal(ExtensionRemoveVerificationState.Unknown, unknown.ExtensionsLifecycle);
        Assert.NotEqual(failed, unknown);
    }

    [Fact(DisplayName = "Extension Remove recovery truth retains a prepared final only after a handled failure"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior")]
    public async Task RecoveryTruthRetainsPreparedFinalAfterFailureAndForgetsItAfterCleanupEscape()
    {
        using var temporary = TemporaryWorkspace.Create("extension-remove-result-truth");
        var preparation = await PrepareRecoveryAsync(temporary);
        try
        {
            var retained = ExtensionRemoveApplicationResultFactory.RecoveryAfterFailure(preparation);
            var unknown = ExtensionRemoveApplicationResultFactory.RecoveryAfterCleanupEscape(preparation);

            Assert.Equal(ExtensionRemoveRecoveryState.Retained, retained.State);
            Assert.Equal(preparation.BundlePath, retained.ResidualPath);
            Assert.Equal(["target.md"], retained.ProtectedPaths);
            Assert.Equal(ExtensionRemoveRecoveryState.Unknown, unknown.State);
            Assert.Null(unknown.ResidualPath);
            Assert.Equal(["target.md"], unknown.ProtectedPaths);
        }
        finally
        {
            DeleteBundle(preparation.BundlePath);
        }
    }

    private static (PlannedFileChange Change, FileStateSnapshot Before) DeleteFixture()
    {
        var path = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            $"open-forge-extension-remove-result-{Guid.NewGuid():N}.md"));
        var before = FileStateSnapshot.File(path, path, "prior"u8);
        return (PlannedFileChange.Delete(before.Expectation), before);
    }

    private static ExtensionRemoveEffect Effect()
        => new(
            ".agents/toolkit.md",
            "toolkit",
            ExtensionRemoveEffectKind.PackageFile,
            ExtensionRemoveEffectAction.Delete,
            ExtensionRemoveEffectOutcome.Planned,
            ExtensionRemoveEffectResidual.None);

    private static async Task<RecoveryBundlePreparation> PrepareRecoveryAsync(
        TemporaryWorkspace temporary)
    {
        var workspace = new CliWorkspace(
            temporary.Path,
            temporary.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var path = temporary.CreateFile("target.md", "prior");
        var before = FileStateSnapshot.File(path, path, "prior"u8);
        var change = PlannedFileChange.Delete(before.Expectation);
        var input = RecoveryBundleInput.Create(
            workspace,
            "extension remove",
            RecoveryBundleAttribution.Create(
                RecoveryBundleProducer.Extension,
                RecoveryBundleOperation.Remove,
                workspace),
            Guid.NewGuid(),
            [RecoveryBundleTarget.Create(change, before)]);

        var result = await new RecoveryBundleStore(new RecoveryBundleReader()).PrepareAsync(
            input,
            CancellationToken.None);
        return Assert.IsType<RecoveryBundlePreparation>(result.Preparation);
    }

    private static void DeleteBundle(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        var directory = Path.GetDirectoryName(path);
        if (directory is not null
            && Directory.Exists(directory)
            && !Directory.EnumerateFileSystemEntries(directory).Any())
        {
            Directory.Delete(directory);
        }
    }
}
