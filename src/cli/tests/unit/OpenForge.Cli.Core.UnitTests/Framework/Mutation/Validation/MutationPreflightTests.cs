using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Validation;

public sealed class MutationPreflightTests
{
    [Fact(DisplayName = "Mutation preflight accepts no-op and blocks duplicate and lock-target sets")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task PreflightBlocksStructurallyInvalidChangeSetsBeforeObservation()
    {
        var workspace = Workspace();
        var preflight = Preflight();
        var path = Path.Combine(workspace.LexicalRoot, "document.md");
        var change = PlannedFileChange.Create(FileExpectation.Missing(path), "content"u8);

        var empty = await preflight.ValidateAsync(workspace, [], CancellationToken.None);
        var duplicate = await preflight.ValidateAsync(
            workspace,
            [change, change],
            CancellationToken.None);
        var lockTarget = await preflight.ValidateAsync(
            workspace,
            [PlannedFileChange.Create(
                FileExpectation.Missing(
                    Path.Combine(workspace.LexicalRoot, WorkspaceLockRequest.RelativePath)),
                "lock"u8)],
            CancellationToken.None);

        Assert.Equal(MutationValidationState.Valid, empty.State);
        Assert.Equal(MutationValidationState.Blocked, duplicate.State);
        Assert.Equal(MutationValidationState.Blocked, lockTarget.State);
    }

    [Fact(DisplayName = "Mutation preflight blocks duplicate expected physical identities")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task PreflightBlocksDuplicateExpectedPhysicalIdentityBeforeObservation()
    {
        var workspace = Workspace();
        var hash = FileExpectation.Hash("before"u8);
        var physicalPath = Path.Combine(workspace.PhysicalRoot, "shared.md");
        var changes = new[]
        {
            PlannedFileChange.Delete(
                FileExpectation.File(
                    Path.Combine(workspace.LexicalRoot, "first.md"),
                    physicalPath,
                    hash)),
            PlannedFileChange.Delete(
                FileExpectation.File(
                    Path.Combine(workspace.LexicalRoot, "second.md"),
                    physicalPath,
                    hash)),
        };

        var result = await Preflight().ValidateAsync(
            workspace,
            changes,
            CancellationToken.None);

        Assert.Equal(MutationValidationState.Blocked, result.State);
        Assert.Empty(result.Checks);
    }

    [Fact(DisplayName = "Mutation preflight returns cancelled before observing a nonempty plan"), Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task PreflightReturnsCancelledBeforeObservation()
    {
        var workspace = Workspace();
        var path = Path.Combine(workspace.LexicalRoot, "document.md");
        var change = PlannedFileChange.Create(FileExpectation.Missing(path), "content"u8);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await Preflight().ValidateAsync(
            workspace,
            [change],
            cancellation.Token);

        Assert.Equal(MutationValidationState.Cancelled, result.State);
        Assert.Empty(result.Checks);
    }

    private static MutationPreflight Preflight()
        => new(new FileExpectationValidator(new PhysicalPathResolver()));

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "mutation-preflight-unit"));
        return new CliWorkspace(
            lexicalRoot: root,
            physicalRoot: root,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
