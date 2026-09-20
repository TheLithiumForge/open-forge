using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies a leaf pair with label-preserving external detachment"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task LeafApplicationDetachesIncomingLinkAndPreservesLifecycle()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-leaf-apply");
        var lifecycleBefore = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var outsideBefore = workspace.ReadText("notes.md");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Contains("Removed .agents/guidance/old guide.md", output.ToString(), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
        Assert.Equal(
            "Prefix Old guide suffix.\n",
            File.ReadAllText(workspace.Combine("README.md")));
        Assert.Equal(outsideBefore, workspace.ReadText("notes.md"));
        Assert.DoesNotContain(
            "Old guide",
            workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath),
            StringComparison.Ordinal);
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove applies one complete category with ordered nonrecursive deletion"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task CategoryApplicationRemovesEveryContainedItem()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-category-apply");
        var resourceBefore = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.CategoryResourcePath);
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.CategoryId, "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Contains("Removed the route guidance/topics  (4 files)", output.ToString(), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryChildPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryNotesPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryResourcePath)));
        Assert.False(Directory.Exists(workspace.Combine(".agents/guidance/topics")));
        Assert.DoesNotContain(
            "Topics",
            workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath),
            StringComparison.Ordinal);
        Assert.Equal(4, resourceBefore.Length);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove composes authored detachment and generated projection in one parent file"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task OneParentDocumentRetainsAuthoredDetachmentAndGeneratedProjection()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-coalesced-parent");
        var parent = workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath);
        workspace.WriteText(
            RouteRemoveIntegrationWorkspace.ParentPath,
            parent.Replace(
                "# Guidance\n",
                "# Guidance\n\n[Authored old guide](old%20guide.md#part)\n",
                StringComparison.Ordinal));
        var before = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.ParentPath);
        var parentPath = workspace.Combine(RouteRemoveIntegrationWorkspace.ParentPath);
        var plan = await workspace.BuildApplicationPlanAsync();

        var change = Assert.Single(
            plan.Projection.FileChanges,
            candidate => candidate.LogicalPath == parentPath);
        Assert.Equal(PlannedFileChangeKind.ReplaceGeneratedRegion, change.Kind);
        var recovery = Assert.Single(
            plan.Projection.RecoveryTargets,
            target => target.Change.LogicalPath == parentPath);
        Assert.Equal(before, recovery.Before.Bytes.ToArray());
        Assert.Contains(
            plan.Preview.References.Detachments,
            detachment => detachment.SourcePath == RouteRemoveIntegrationWorkspace.ParentPath
                && detachment.VisibleLabel == "Authored old guide");

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        var applied = workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath);
        Assert.Contains("# Guidance\n\nAuthored old guide\n", applied, StringComparison.Ordinal);
        Assert.DoesNotContain("old%20guide.md", applied, StringComparison.Ordinal);
        Assert.DoesNotContain("- [Old guide]", applied, StringComparison.Ordinal);
        Assert.Contains("- [Topics](topics/_topics.md) - #Route", applied, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Remove never releases lifecycle ownership while applying an unmanaged subject"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ApplicationDoesNotRewriteLifecycleOwnership()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-lifecycle-identity");
        var lifecycleBefore = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath);
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Contains("Removed .agents/guidance/old guide.md", output.ToString(), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
        Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafOverwritePath)));
        Assert.Equal(lifecycleBefore, workspace.ReadBytes(RouteRemoveIntegrationWorkspace.OwnershipPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove applies generated, reference, file, and directory effects in one ordered plan"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task ApplicationPreservesExactEffectPhaseOrder()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-effect-order");
        var plan = await workspace.BuildApplicationPlanAsync(RouteRemoveIntegrationWorkspace.CategoryId);
        var expectedDirectories = new[]
        {
            workspace.Combine(".agents/guidance/topics/assets"),
            workspace.Combine(".agents/guidance/topics"),
        };
        Assert.Equal(
            expectedDirectories,
            plan.Projection.DirectoryDeletions.Select(deletion => deletion.LogicalPath));
        var phases = plan.Projection.FileChanges
            .Select(change => change.Kind switch
            {
                PlannedFileChangeKind.ReplaceGeneratedRegion => 0,
                PlannedFileChangeKind.Replace => 1,
                PlannedFileChangeKind.Delete => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(change.Kind), change.Kind, "Unexpected Route Remove file effect."),
            })
            .Concat(plan.Projection.DirectoryDeletions.Select(_ => 3))
            .ToArray();

        Assert.NotEmpty(phases);
        Assert.Contains(0, phases);
        Assert.Contains(1, phases);
        Assert.Contains(2, phases);
        Assert.Contains(3, phases);
        Assert.Equal(phases.OrderBy(value => value), phases);

        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(
            new RouteRemoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        Assert.NotNull(prepared.Preparation);

        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(
            new RouteRemoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            },
            TestContext.Current.CancellationToken);

        var expectedPaths = plan.Projection.FileChanges
            .Select(change => change.LogicalPath)
            .Concat(plan.Projection.DirectoryDeletions.Select(deletion => deletion.LogicalPath))
            .ToArray();
        Assert.Equal(expectedPaths, progress.Receipts.Select(ReadReceiptPath));
        Assert.Equal(
            expectedDirectories,
            progress.Receipts
                .OfType<RouteRemoveDirectoryDeletionReceipt>()
                .Select(receipt => receipt.Receipt.Deletion.LogicalPath));
        Assert.All(progress.Receipts, AssertVerifiedReceipt);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove retains prior receipts and recovery after a real later-target expected-state race"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task LaterTargetRaceStopsNewEffectsAndRetainsRecovery()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-later-target-race");
        var plan = await workspace.BuildApplicationPlanAsync(RouteRemoveIntegrationWorkspace.CategoryId);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(
            new RouteRemoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        var laterChange = Assert.Single(
            plan.Projection.FileChanges,
            change => change.LogicalPath == workspace.Combine("notes.md"));
        var laterIndex = plan.Projection.FileChanges.IndexOf(laterChange);
        Assert.True(laterIndex > 0);
        const string concurrentContents = "Concurrent later-target edit.\n";
        workspace.WriteText("notes.md", concurrentContents);

        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(
            new RouteRemoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(
            plan.Projection.FileChanges.Length + plan.Projection.DirectoryDeletions.Length,
            progress.Receipts.Length);
        Assert.NotEmpty(progress.Receipts.Take(laterIndex));
        Assert.All(progress.Receipts.Take(laterIndex), AssertVerifiedReceipt);
        var raced = Assert.IsType<RouteRemoveFileChangeReceipt>(progress.Receipts[laterIndex]);
        Assert.Equal(FilesystemEffectState.NotStarted, raced.Receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, raced.Receipt.NotStartedReason);
        Assert.All(
            progress.Receipts.Skip(laterIndex + 1),
            receipt => AssertNotStarted(receipt, FilesystemNotStartedReason.ApplicationFailed));
        Assert.Contains(
            progress.Findings,
            finding => finding.Code == RouteRemoveFindingCode.TargetChangedDuringApply);
        Assert.Equal(RouteRemoveRecoveryState.Retained, progress.Recovery.State);
        Assert.NotEqual(RouteRemoveVerificationState.Verified, progress.Verification);
        Assert.Equal(concurrentContents, workspace.ReadText("notes.md"));
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.True(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.CategoryPath)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove retains recovery when a late source restores an incoming reference"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsLateIncomingReferenceAndRetainsRecovery()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-late-reference");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(
            new RouteRemoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(
            new RouteRemoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            },
            TestContext.Current.CancellationToken);
        Assert.Empty(progress.Findings);
        Assert.NotEmpty(progress.Receipts);
        Assert.Equal(
            plan.Projection.FileChanges.Length + plan.Projection.DirectoryDeletions.Length,
            progress.Receipts.Length);
        Assert.All(progress.Receipts, AssertVerifiedReceipt);
        var absence = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAbsenceAsync(
            plan,
            TestContext.Current.CancellationToken);
        Assert.Null(absence.Plan);
        Assert.Empty(absence.Formation.Findings);
        Assert.Equal(RouteRemoveCoverage.Complete, absence.Formation.References.Coverage);
        Assert.True(absence.Formation.References.OccurrenceCount > 0);
        Assert.Equal(RouteRemovePlanCompleteness.Complete, absence.Formation.Plan.Completeness);
        Assert.Equal(RouteRemovePlanSafety.Safe, absence.Formation.Plan.Safety);
        Assert.Equal(RouteRemoveVerificationState.Verified, absence.Formation.Verification);
        Assert.Equal(
            "[Topics](.agents/guidance/topics/_topics.md) remains.\n",
            workspace.ReadText("notes.md"));
        const string lateReference = "[Late old guide](.agents/guidance/old%20guide.md#part)\n";
        workspace.WriteText("late.md", lateReference);

        var completion = await new RouteRemoveApplicationCompletion(
            RouteRemoveOperationFactory.CreateAppliedVerifier()).CompleteAsync(
                new RouteRemoveHeldApplication(plan, operationId, lease),
                prepared,
                progress,
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteRemoveVerificationState.Failed, completion.Verification);
        var finding = Assert.Single(completion.Findings);
        Assert.Equal(RouteRemoveFindingCode.VerificationFailed, finding.Code);
        Assert.Equal(CliSemanticStatus.Failed, finding.Status);
        Assert.Equal(RouteRemoveRecoveryState.Retained, completion.Recovery.State);
        Assert.Equal(plan.Preview.Recovery.ProtectedPaths, completion.Recovery.ProtectedPaths);
        Assert.Equal(preparation.BundlePath, completion.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(lateReference, workspace.ReadText("late.md"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove retains recovery when late topology invalidates a generated projection"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsLateSiblingProjectionAndRetainsRecovery()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-late-sibling");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(
            new RouteRemoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(
            new RouteRemoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            },
            TestContext.Current.CancellationToken);
        Assert.Empty(progress.Findings);
        Assert.NotEmpty(progress.Receipts);
        Assert.Equal(
            plan.Projection.FileChanges.Length + plan.Projection.DirectoryDeletions.Length,
            progress.Receipts.Length);
        Assert.All(progress.Receipts, AssertVerifiedReceipt);
        var parentBytes = workspace.ReadBytes(RouteRemoveIntegrationWorkspace.ParentPath);
        const string siblingPath = ".agents/guidance/late-sibling.md";
        var siblingText = OpenForgeDocumentSeed.Metadata(
            "Late sibling",
            ["Route"],
            "# Late sibling\n\nLate body.\n");
        workspace.WriteText(siblingPath, siblingText);
        var siblingBytes = workspace.ReadBytes(siblingPath);
        Assert.DoesNotContain(
            "Late sibling",
            workspace.ReadText(RouteRemoveIntegrationWorkspace.ParentPath),
            StringComparison.Ordinal);

        var completion = await new RouteRemoveApplicationCompletion(
            RouteRemoveOperationFactory.CreateAppliedVerifier()).CompleteAsync(
                new RouteRemoveHeldApplication(plan, operationId, lease),
                prepared,
                progress,
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteRemoveVerificationState.Failed, completion.Verification);
        var finding = Assert.Single(completion.Findings);
        Assert.Equal(RouteRemoveFindingCode.VerificationFailed, finding.Code);
        Assert.Equal(CliSemanticStatus.Failed, finding.Status);
        Assert.Equal(RouteRemoveRecoveryState.Retained, completion.Recovery.State);
        Assert.Equal(plan.Preview.Recovery.ProtectedPaths, completion.Recovery.ProtectedPaths);
        Assert.Equal(preparation.BundlePath, completion.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(parentBytes, workspace.ReadBytes(RouteRemoveIntegrationWorkspace.ParentPath));
        Assert.Equal(siblingBytes, workspace.ReadBytes(siblingPath));
        Assert.Equal(siblingText, workspace.ReadText(siblingPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Remove retains recovery when a late unrelated reference changes complete facts"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsLateUnrelatedReferenceCountDriftAndRetainsRecovery()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-late-unrelated-reference");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.PrepareAsync(
            new RouteRemoveHeldApplication(Plan: plan, OperationId: operationId, Lease: lease),
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            prepared.Preparation);
        var progress = await RouteRemoveOperationFactory.CreateEffectApplication().ApplyAsync(
            new RouteRemoveEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                RecoveryPreparation = prepared,
            },
            TestContext.Current.CancellationToken);
        Assert.Empty(progress.Findings);
        Assert.NotEmpty(progress.Receipts);
        Assert.All(progress.Receipts, AssertVerifiedReceipt);
        const string latePath = "late-unrelated.md";
        const string lateText = "[Retained topics](.agents/guidance/topics/_topics.md)\n";
        workspace.WriteText(latePath, lateText);

        var completion = await new RouteRemoveApplicationCompletion(
            RouteRemoveOperationFactory.CreateAppliedVerifier()).CompleteAsync(
                new RouteRemoveHeldApplication(plan, operationId, lease),
                prepared,
                progress,
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteRemoveVerificationState.Failed, completion.Verification);
        var finding = Assert.Single(completion.Findings);
        Assert.Equal(RouteRemoveFindingCode.VerificationFailed, finding.Code);
        Assert.Equal(CliSemanticStatus.Failed, finding.Status);
        Assert.Equal(RouteRemoveRecoveryState.Retained, completion.Recovery.State);
        Assert.Equal(plan.Preview.Recovery.ProtectedPaths, completion.Recovery.ProtectedPaths);
        Assert.Equal(preparation.BundlePath, completion.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(lateText, workspace.ReadText(latePath));
    }

    private static string ReadReceiptPath(RouteRemoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteRemoveFileChangeReceipt file => file.Receipt.Change.LogicalPath,
            RouteRemoveDirectoryDeletionReceipt directory => directory.Receipt.Deletion.LogicalPath,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Remove receipt."),
        };

    private static void AssertVerifiedReceipt(RouteRemoveApplicationReceipt receipt)
    {
        switch (receipt)
        {
            case RouteRemoveFileChangeReceipt file:
                Assert.Equal(FilesystemEffectState.Applied, file.Receipt.EffectState);
                Assert.Equal(FilesystemVerificationState.Verified, file.Receipt.VerificationState);
                break;
            case RouteRemoveDirectoryDeletionReceipt directory:
                Assert.Equal(FilesystemEffectState.Applied, directory.Receipt.State.EffectState);
                Assert.Equal(FilesystemVerificationState.Verified, directory.Receipt.State.VerificationState);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Remove receipt.");
        }
    }

    private static void AssertNotStarted(
        RouteRemoveApplicationReceipt receipt,
        FilesystemNotStartedReason reason)
    {
        switch (receipt)
        {
            case RouteRemoveFileChangeReceipt file:
                Assert.Equal(FilesystemEffectState.NotStarted, file.Receipt.EffectState);
                Assert.Equal(reason, file.Receipt.NotStartedReason);
                break;
            case RouteRemoveDirectoryDeletionReceipt directory:
                Assert.Equal(FilesystemEffectState.NotStarted, directory.Receipt.State.EffectState);
                Assert.Equal(reason, directory.Receipt.State.NotStartedReason);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Remove receipt.");
        }
    }
}
