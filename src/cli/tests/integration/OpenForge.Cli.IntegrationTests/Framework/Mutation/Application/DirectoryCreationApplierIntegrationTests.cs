using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Application;

public sealed class DirectoryCreationApplierIntegrationTests : IDisposable
{
    private readonly WorkspaceLockTestStore lockStore = WorkspaceLockTestStore.Create(
        "directory-creation-applier-lock-store");

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier creates and exactly verifies one target without support artifacts")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CreatesOneExactDirectoryWithoutRecoveryOrLifecycleArtifacts()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-one");
        var target = temporary.Combine("route");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var check = await CheckAsync(workspace, creation, validator);
        await using var lease = await AcquireAsync(workspace);

        try
        {
            var receipt = await Applier(validator, resolver).ApplyAsync(
                lease,
                creation,
                check,
                TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.Applied, receipt.EffectState);
            Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
            Assert.Equal(target, receipt.IntendedPhysicalPath);
            Assert.Equal(target, receipt.After?.PhysicalPath);
            Assert.Equal(FileExpectationKind.Directory, receipt.After?.Kind);
            Assert.True(Directory.Exists(target));
            Assert.DoesNotContain(
                Directory.EnumerateFiles(temporary.Path, "*", SearchOption.AllDirectories),
                path => Path.GetFileName(path).Contains("recovery", StringComparison.OrdinalIgnoreCase)
                    || Path.GetFileName(path).Contains("stage", StringComparison.OrdinalIgnoreCase)
                    || Path.GetFileName(path).Contains("lifecycle", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (Directory.Exists(target))
            {
                Directory.Delete(target);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation loop retains a verified parent when a later child stops")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task NestedCreationRetainsFirstResidualWhenLaterTargetStops()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-nested-residual");
        var parentPath = temporary.Combine("routes");
        var childPath = temporary.Combine("routes/child");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var parent = Creation(parentPath);
        var child = Creation(childPath);
        var parentCheck = await CheckAsync(workspace, parent, validator);
        var childCheck = await CheckAsync(workspace, child, validator);
        await using var lease = await AcquireAsync(workspace);

        try
        {
            var applier = Applier(validator, resolver);
            var parentReceipt = await applier.ApplyAsync(
                lease,
                parent,
                parentCheck,
                TestContext.Current.CancellationToken);
            await File.WriteAllTextAsync(
                childPath,
                "racing file",
                TestContext.Current.CancellationToken);
            var childReceipt = await applier.ApplyAsync(
                lease,
                child,
                childCheck,
                TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemVerificationState.Verified, parentReceipt.VerificationState);
            Assert.Equal(FilesystemEffectState.NotStarted, childReceipt.EffectState);
            Assert.Equal(FilesystemNotStartedReason.TargetChanged, childReceipt.NotStartedReason);
            Assert.True(Directory.Exists(parentPath));
            Assert.Equal("racing file", await File.ReadAllTextAsync(
                childPath,
                TestContext.Current.CancellationToken));
        }
        finally
        {
            if (File.Exists(childPath))
            {
                File.Delete(childPath);
            }

            if (Directory.Exists(parentPath))
            {
                Directory.Delete(parentPath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier creates an explicit nested plan one target at a time")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CreatesNestedDirectoriesOneAtATime()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-nested");
        var parentPath = temporary.Combine("routes");
        var childPath = temporary.Combine("routes/child");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var parent = Creation(parentPath);
        var child = Creation(childPath);
        var parentCheck = await CheckAsync(workspace, parent, validator);
        var childCheck = await CheckAsync(workspace, child, validator);
        await using var lease = await AcquireAsync(workspace);

        try
        {
            var applier = Applier(validator, resolver);
            var receipts = new[]
            {
                await applier.ApplyAsync(
                    lease,
                    parent,
                    parentCheck,
                    TestContext.Current.CancellationToken),
                await applier.ApplyAsync(
                    lease,
                    child,
                    childCheck,
                    TestContext.Current.CancellationToken),
            };

            Assert.All(receipts, receipt =>
                Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState));
            Assert.True(Directory.Exists(parentPath));
            Assert.True(Directory.Exists(childPath));
        }
        finally
        {
            if (Directory.Exists(childPath))
            {
                Directory.Delete(childPath);
            }

            if (Directory.Exists(parentPath))
            {
                Directory.Delete(parentPath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier rejects disposed and foreign workspace leases")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task RequiresLiveLeaseForTheSelectedWorkspace()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-lease");
        using var foreign = TemporaryWorkspace.Create("directory-apply-foreign-lease");
        var target = temporary.Combine("route");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var check = await CheckAsync(workspace, creation, validator);
        await using var disposedLease = await AcquireAsync(workspace);
        await disposedLease.DisposeAsync();
        await using var foreignLease = await AcquireAsync(Workspace(foreign));
        var applier = Applier(validator, resolver);

        var disposed = await applier.ApplyAsync(
            disposedLease,
            creation,
            check,
            TestContext.Current.CancellationToken);
        var foreignResult = await applier.ApplyAsync(
            foreignLease,
            creation,
            check,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, disposed.EffectState);
        Assert.Equal(FilesystemNotStartedReason.ContractRejected, disposed.NotStartedReason);
        Assert.Equal(FilesystemEffectState.NotStarted, foreignResult.EffectState);
        Assert.Equal(FilesystemNotStartedReason.ContractRejected, foreignResult.NotStartedReason);
        Assert.False(Directory.Exists(target));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier observes cancellation before the target effect")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task PreCancellationReturnsNotStartedWithoutCreation()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-cancel");
        var target = temporary.Combine("route");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var check = await CheckAsync(workspace, creation, validator);
        await using var lease = await AcquireAsync(workspace);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var receipt = await Applier(validator, resolver).ApplyAsync(
            lease,
            creation,
            check,
            cancellation.Token);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.NotStarted, receipt.VerificationState);
        Assert.Equal(FilesystemNotStartedReason.Cancelled, receipt.NotStartedReason);
        Assert.False(Directory.Exists(target));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier rejects a target collision after planning")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CollisionAfterPlanningReturnsNotStartedAndPreservesOccupant()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-collision");
        var target = temporary.Combine("route");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var check = await CheckAsync(workspace, creation, validator);
        await File.WriteAllTextAsync(
            target,
            "racing file",
            TestContext.Current.CancellationToken);
        await using var lease = await AcquireAsync(workspace);

        try
        {
            var receipt = await Applier(validator, resolver).ApplyAsync(
                lease,
                creation,
                check,
                TestContext.Current.CancellationToken);

            Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
            Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
            Assert.Equal("racing file", await File.ReadAllTextAsync(
                target,
                TestContext.Current.CancellationToken));
        }
        finally
        {
            if (File.Exists(target))
            {
                File.Delete(target);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier rejects a planned target whose parent alias changes identity")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ParentAliasIdentityChangeReturnsNotStartedWithoutCreation()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-identity-change");
        var first = temporary.CreateDirectory("first");
        var second = temporary.CreateDirectory("second");
        var alias = temporary.CreateDirectorySymbolicLink("alias", first);
        var target = Path.Combine(alias, "route");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var check = await CheckAsync(workspace, creation, validator);
        await using var lease = await AcquireAsync(workspace);
        DeleteDirectoryLink(alias);
        Directory.CreateSymbolicLink(alias, second);

        var receipt = await Applier(validator, resolver).ApplyAsync(
            lease,
            creation,
            check,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.False(Directory.Exists(Path.Combine(first, "route")));
        Assert.False(Directory.Exists(Path.Combine(second, "route")));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier rejects a forged matched missing check")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task ForgedMissingPhysicalTargetIsRejectedWithoutEffects()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-forged-check");
        var other = temporary.CreateDirectory("other");
        var target = temporary.Combine("route");
        var forgedTarget = Path.Combine(other, "forged");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var creation = Creation(target);
        var forgedCheck = FileExpectationValidationResult.Matched(
            creation.Expectation,
            FileStateSnapshot.Missing(target),
            forgedTarget);
        await using var lease = await AcquireAsync(workspace);

        var receipt = await Applier(validator, resolver).ApplyAsync(
            lease,
            creation,
            forgedCheck,
            TestContext.Current.CancellationToken);

        Assert.Equal(FilesystemEffectState.NotStarted, receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, receipt.NotStartedReason);
        Assert.False(Directory.Exists(target));
        Assert.False(Directory.Exists(forgedTarget));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Directory creation applier rejects a check for a different planned target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task MismatchedCheckAndCreationAreRejectedWithoutEffects()
    {
        using var temporary = TemporaryWorkspace.Create("directory-apply-check-coherence");
        var first = Creation(temporary.Combine("first"));
        var second = Creation(temporary.Combine("second"));
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var firstCheck = await CheckAsync(workspace, first, validator);
        await using var lease = await AcquireAsync(workspace);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await Applier(validator, resolver).ApplyAsync(
                lease,
                second,
                firstCheck,
                TestContext.Current.CancellationToken));
        Assert.False(Directory.Exists(first.LogicalPath));
        Assert.False(Directory.Exists(second.LogicalPath));
    }

    private static DirectoryCreationApplier Applier(
        FileExpectationValidator validator,
        PhysicalPathResolver resolver)
        => new(
            new MutationRevalidator(validator),
            validator);

    private static PlannedDirectoryCreation Creation(string path)
        => PlannedDirectoryCreation.Create(FileExpectation.Missing(path));

    private static async ValueTask<FileExpectationValidationResult> CheckAsync(
        CliWorkspace workspace,
        PlannedDirectoryCreation creation,
        FileExpectationValidator validator)
    {
        var check = await validator.ValidateAsync(
            workspace,
            creation.Expectation,
            TestContext.Current.CancellationToken);
        Assert.Equal(FileExpectationValidationState.Matched, check.State);
        return check;
    }

    private async ValueTask<WorkspaceLockLease> AcquireAsync(CliWorkspace workspace)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "directory apply", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    private static void DeleteDirectoryLink(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Directory.Delete(path);
        }
        else
        {
            File.Delete(path);
        }
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    public void Dispose() => lockStore.Dispose();
}
