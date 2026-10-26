using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

[Trait("Feature", "route-update-outside"), Trait("Evidence", "IntegrationSafety"), Trait("Boundary", "OS")]
public sealed class RouteUpdateOutsideApplicationIntegrationTests
{
    public enum Drift { Target, Settings, Anchor, Parent, Sibling, Overwrite, Template, Physical, Boundary }

    [Theory(DisplayName = "Outside Update revalidates retained target settings physical and readonly inputs before effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Target), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Target)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Settings), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Settings)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Anchor), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Anchor)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Parent), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Parent)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Sibling), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Sibling)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Overwrite), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Overwrite)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Template), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Template)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Physical), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Physical)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, Drift.Boundary), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, Drift.Boundary)]
    public async Task StalePlanHasNoEffects(string root, Drift drift)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        using var foreign = TemporaryWorkspace.Create("route-update-stale-foreign");
        var sibling = workspace.ParentDirectory + "/sibling.md";
        workspace.Files.WriteText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument);
        workspace.Files.WriteText(workspace.Overwrite, "Independent overwrite\n");
        workspace.Files.WriteText(workspace.Template, RouteUpdateIntegrationWorkspace.TemplateDocument());
        var request = workspace.Request(template: SourceWorkspaceRelativePath.LiteralReferencePrefix + workspace.Template);
        var plan = await workspace.Plan(request);
        var changedPath = drift switch
        {
            Drift.Target => workspace.Target,
            Drift.Anchor => workspace.Anchor,
            Drift.Parent => workspace.Parent,
            Drift.Sibling => sibling,
            Drift.Overwrite => workspace.Overwrite,
            Drift.Template => workspace.Template,
            _ => null,
        };
        var originalDirectory = workspace.Files.Combine(workspace.DirectoryPath);
        var backupDirectory = workspace.Files.Combine("retained-original");
        var moved = drift is Drift.Physical or Drift.Boundary;
        if (changedPath is not null) workspace.Files.ReplaceText(changedPath, workspace.Text(changedPath) + "\nConcurrent authored suffix.\n");
        else if (drift == Drift.Settings) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([]));
        else
        {
            Directory.Move(originalDirectory, backupDirectory);
            if (drift == Drift.Physical) Directory.CreateSymbolicLink(originalDirectory, foreign.Path);
        }
        try
        {
            var before = workspace.Files.SnapshotHashes();
            var foreignBefore = foreign.SnapshotHashes();
            var result = await workspace.Apply(plan);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == (drift == Drift.Physical
                ? RouteUpdateFindingCode.TargetUnsafe : RouteUpdateFindingCode.TargetChanged));
            Assert.All(result.Effects, effect => Assert.Equal(RouteUpdateEffectOutcome.NotStarted, effect.Outcome));
            Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
            Assert.Equal(before, workspace.Files.SnapshotHashes());
            Assert.Equal(foreignBefore, foreign.SnapshotHashes());
        }
        finally
        {
            if (moved)
            {
                if (drift == Drift.Physical) Directory.Delete(originalDirectory);
                Directory.Move(backupDirectory, originalDirectory);
            }
        }
    }

    [Theory(DisplayName = "Outside Update cancellation before planning creates no bytes or runtime infrastructure")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task CancelledBeforePlanningIsReadOnly(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await OpenForge.Cli.Core.Commands.Route.Update.RouteUpdateOperationFactory.Create()
            .ExecuteAsync(workspace.Request(), cancellation.Token);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Empty(result.Effects);
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Update checks supporting inputs before each effect and retains truthful partial recovery")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteUpdateOutsideWorkspace.CustomRoot)]
    public async Task LaterInputRaceRetainsOnlyVerifiedReceipt(string root)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var sibling = workspace.ParentDirectory + "/readonly.md";
        workspace.Files.WriteText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument);
        var plan = await workspace.Plan();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.Lease(operationId);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        var validation = await revalidator.ValidateAsync(lease, plan.FileChanges, TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validation.State);
        var prepared = await RouteUpdateRecoveryPreparer.PrepareAsync(new() { Plan = plan, OperationId = operationId.ToString("D") }, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation.BundlePath);
        var parent = workspace.Bytes(workspace.Parent);
        var loader = workspace.Bytes(workspace.Layout.LoaderPath);
        var applier = new FileChangeApplier(revalidator, validator);
        var progress = await new RouteUpdateEffectApplication(async (currentLease, change, expected, recovery, token) =>
        {
            var receipt = await applier.ApplyAsync(currentLease, change, expected, recovery, token);
            workspace.Files.ReplaceText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument + "Concurrent body suffix.\r\n");
            return receipt;
        }).ApplyAsync(new() { Plan = plan, Preparation = preparation, Lease = lease, Validation = validation }, TestContext.Current.CancellationToken);
        Assert.Equal(2, progress.Receipts.Length);
        Assert.Equal(FilesystemEffectState.Applied, progress.Receipts[0].EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, progress.Receipts[0].VerificationState);
        Assert.Equal(FilesystemEffectState.NotStarted, progress.Receipts[1].EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, progress.Receipts[1].NotStartedReason);
        Assert.Null(progress.UncertainAttempt);
        Assert.Equal(RouteUpdateRecoveryState.Retained, progress.Recovery.State);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(RouteUpdateOutsideWorkspace.ChangedDocument(), workspace.Bytes(workspace.Target));
        Assert.Equal(parent, workspace.Bytes(workspace.Parent));
        Assert.Equal(loader, workspace.Bytes(workspace.Layout.LoaderPath));
    }

    [Theory(DisplayName = "Outside Update semantic verification rejects readonly and settings drift after all effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteUpdateOutsideWorkspace.CustomRoot, true)]
    public async Task FinalVerificationRetainsOriginalReadonlyExpectations(string root, bool settings)
    {
        using var workspace = new RouteUpdateOutsideWorkspace(root);
        var sibling = workspace.ParentDirectory + "/readonly.md";
        workspace.Files.WriteText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument);
        var plan = await workspace.Plan();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.Lease(operationId);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        var validation = await revalidator.ValidateAsync(lease, plan.FileChanges, TestContext.Current.CancellationToken);
        var prepared = await RouteUpdateRecoveryPreparer.PrepareAsync(new() { Plan = plan, OperationId = operationId.ToString("D") }, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation.BundlePath);
        var progress = await new RouteUpdateEffectApplication(new FileChangeApplier(revalidator, validator).ApplyAsync)
            .ApplyAsync(new() { Plan = plan, Preparation = preparation, Lease = lease, Validation = validation }, TestContext.Current.CancellationToken);
        Assert.All(progress.Receipts, receipt => Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState));
        if (settings) workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings([]));
        else workspace.Files.ReplaceText(sibling, RouteUpdateOutsideWorkspace.AuthoredDocument + "Concurrent suffix.\r\n");
        var before = workspace.Files.SnapshotHashes();
        var verification = await new RouteUpdateAppliedVerifier(RouteUpdateIntegrationWorkspace.CreatePlanBuilder(), validator)
            .VerifyAsync(new() { Plan = plan, Progress = progress, Lease = lease }, TestContext.Current.CancellationToken);
        Assert.Equal(RouteUpdateAppliedVerificationState.Failed, verification.State);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }
}
