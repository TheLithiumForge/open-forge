using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Mutation.Validation;

public sealed class DirectoryMutationValidationIntegrationTests : IDisposable
{
    private readonly WorkspaceLockTestStore lockStore = WorkspaceLockTestStore.Create(
        "directory-validation-lock-store");

    [Fact(DisplayName = "Combined preflight preserves explicit parent-first directory and file order")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CombinedPreflightAcceptsExplicitParentFirstClosure()
    {
        using var temporary = TemporaryWorkspace.Create("directory-validation-parent-first");
        var parentPath = temporary.Combine("routes");
        var childPath = temporary.Combine("routes/child");
        var filePath = temporary.Combine("routes/child/_child.md");
        var directories = new[]
        {
            Creation(parentPath),
            Creation(childPath),
        };
        var files = new[]
        {
            PlannedFileChange.Create(FileExpectation.Missing(filePath), "entrypoint"u8),
        };

        var result = await Preflight().ValidateAsync(
            Workspace(temporary),
            directories,
            files,
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, result.State);
        Assert.Collection(
            result.Checks,
            check => Assert.Equal(parentPath, check.Expectation.LogicalPath),
            check => Assert.Equal(childPath, check.Expectation.LogicalPath),
            check => Assert.Equal(filePath, check.Expectation.LogicalPath));
        Assert.All(result.Checks, check =>
            Assert.Equal(FileExpectationValidationState.Matched, check.State));
        Assert.False(Directory.Exists(parentPath));
        Assert.False(File.Exists(filePath));
    }

    [Fact(DisplayName = "Combined preflight blocks reversed or omitted missing directory parents")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CombinedPreflightRequiresExplicitParentFirstClosure()
    {
        using var temporary = TemporaryWorkspace.Create("directory-validation-parent-closure");
        var parent = Creation(temporary.Combine("routes"));
        var child = Creation(temporary.Combine("routes/child"));
        var preflight = Preflight();
        var workspace = Workspace(temporary);

        var reversed = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [child, parent],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        var omitted = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [child],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        var omittedFileParent = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [],
            fileChanges:
            [
                PlannedFileChange.Create(
                    FileExpectation.Missing(temporary.Combine("missing/document.md")),
                    "content"u8),
            ],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Blocked, reversed.State);
        Assert.Equal(MutationValidationState.Blocked, omitted.State);
        Assert.Equal(MutationValidationState.Blocked, omittedFileParent.State);
        Assert.False(Directory.Exists(parent.LogicalPath));
        Assert.False(Directory.Exists(child.LogicalPath));
    }

    [Fact(DisplayName = "File-only preflight preserves its prospective missing-parent behavior")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task FileOnlyPreflightBehaviorRemainsUnchanged()
    {
        using var temporary = TemporaryWorkspace.Create("directory-validation-file-regression");
        var target = temporary.Combine("missing/document.md");
        var change = PlannedFileChange.Create(
            FileExpectation.Missing(target),
            "content"u8);

        var result = await Preflight().ValidateAsync(
            Workspace(temporary),
            changes: [change],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, result.State);
        Assert.Equal(target, Assert.Single(result.Checks).PhysicalPath);
        Assert.False(Directory.Exists(temporary.Combine("missing")));
    }

    [Fact(DisplayName = "Combined preflight requires .agents as an ordinary parent-first directory effect")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CombinedPreflightRequiresExactAgentsParentClosure()
    {
        using var temporary = TemporaryWorkspace.Create("directory-validation-agents-bootstrap");
        var agentsPath = temporary.Combine(".agents");
        var routesPath = temporary.Combine(".agents/routes");
        var nestedPath = temporary.Combine(".agents/routes/child");
        var workspace = Workspace(temporary);
        var preflight = Preflight();

        var descendant = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [Creation(agentsPath), Creation(routesPath)],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        var omittedIntermediate = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [Creation(agentsPath), Creation(nestedPath)],
            fileChanges: [],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, descendant.State);
        Assert.Equal(2, descendant.Checks.Count);
        Assert.Equal(agentsPath, descendant.Checks[0].PhysicalPath);
        Assert.Equal(routesPath, descendant.Checks[1].PhysicalPath);
        Assert.Equal(MutationValidationState.Blocked, omittedIntermediate.State);
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
    }

    [Fact(DisplayName = "Combined preflight rejects cross-effect physical aliases and external aliases")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CombinedPreflightEnforcesResolvedPhysicalUniquenessAndContainment()
    {
        using var outside = TemporaryWorkspace.Create("directory-validation-outside");
        using var temporary = TemporaryWorkspace.Create("directory-validation-aliases");
        var shared = temporary.CreateDirectory("shared");
        var firstAlias = temporary.CreateDirectorySymbolicLink("alias-a", shared);
        var secondAlias = temporary.CreateDirectorySymbolicLink("alias-b", shared);
        var externalAlias = temporary.CreateDirectorySymbolicLink("external", outside.Path);
        var workspace = Workspace(temporary);
        var preflight = Preflight();
        var directoryPath = Path.Combine(firstAlias, "target");
        var filePath = Path.Combine(secondAlias, "target");

        var safe = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [Creation(directoryPath)],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        var duplicate = await preflight.ValidateAsync(
            workspace,
            directoryCreations: [Creation(directoryPath)],
            fileChanges:
            [
                PlannedFileChange.Create(FileExpectation.Missing(filePath), "content"u8),
            ],
            TestContext.Current.CancellationToken);
        var external = await preflight.ValidateAsync(
            workspace,
            directoryCreations:
            [
                Creation(Path.Combine(externalAlias, "target")),
            ],
            fileChanges: [],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, safe.State);
        Assert.Equal(Path.Combine(shared, "target"), Assert.Single(safe.Checks).PhysicalPath);
        Assert.Equal(MutationValidationState.Blocked, duplicate.State);
        Assert.Equal(2, duplicate.Checks.Count);
        Assert.Equal(MutationValidationState.Blocked, external.State);
        Assert.False(Directory.Exists(Path.Combine(shared, "target")));
        Assert.False(Directory.Exists(Path.Combine(outside.Path, "target")));
    }

    [Fact(DisplayName = "Combined revalidation requires a live matching lease and a nonempty plan")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Integration")]
    public async Task CombinedRevalidationRequiresLiveLeaseAndNonemptyPlan()
    {
        using var temporary = TemporaryWorkspace.Create("directory-revalidation-lease");
        var workspace = Workspace(temporary);
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var creation = Creation(temporary.Combine("route"));
        await using var lease = await AcquireAsync(workspace);

        var valid = await revalidator.ValidateAsync(
            lease,
            directoryCreations: [creation],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        var empty = await revalidator.ValidateAsync(
            lease,
            directoryCreations: [],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        await lease.DisposeAsync();
        var disposed = await revalidator.ValidateAsync(
            lease,
            directoryCreations: [creation],
            fileChanges: [],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, valid.State);
        Assert.Single(valid.Checks);
        Assert.Equal(MutationValidationState.Blocked, empty.State);
        Assert.Equal(MutationValidationState.Blocked, disposed.State);
        Assert.False(Directory.Exists(creation.LogicalPath));
    }

    private static PlannedDirectoryCreation Creation(string path)
        => PlannedDirectoryCreation.Create(FileExpectation.Missing(path));

    private static MutationPreflight Preflight()
        => new(new FileExpectationValidator(new PhysicalPathResolver()));

    private async ValueTask<WorkspaceLockLease> AcquireAsync(CliWorkspace workspace)
    {
        var result = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(workspace, "directory validation", Guid.NewGuid()),
            TestContext.Current.CancellationToken);
        return Assert.IsType<WorkspaceLockLease>(result.Lease);
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(
            lexicalRoot: temporary.Path,
            physicalRoot: temporary.Path,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);

    public void Dispose() => lockStore.Dispose();
}
