using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Remove;

public sealed class ExtensionRemoveApplicationResultTruthTests
{
    [Fact(DisplayName = "Extension Remove effect receipts preserve retained and unknown residual truth"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior"), Trait("Boundary", "Processing")]
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

    [Fact(DisplayName = "Extension Remove lifecycle receipts preserve cancellation and unknown publication truth"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior"), Trait("Boundary", "Processing")]
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

    [Fact(DisplayName = "Extension Remove final verification keeps failed and unknown states distinct"), Trait("Feature", "extension-remove"), Trait("Evidence", "UnitBehavior"), Trait("Boundary", "Processing")]
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

}
