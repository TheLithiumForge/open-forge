using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Mutation.Validation;

public sealed class DirectoryMutationPreflightTests
{
    [Fact(DisplayName = "Combined mutation preflight accepts an empty directory and file plan")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task CombinedPreflightAcceptsEmptyPlans()
    {
        var result = await Preflight().ValidateAsync(
            Workspace(),
            directoryCreations: [],
            fileChanges: [],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Valid, result.State);
        Assert.Empty(result.Checks);
    }

    [Fact(DisplayName = "Combined mutation preflight blocks duplicate directory logical targets")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task CombinedPreflightBlocksDuplicateDirectoryTargets()
    {
        var workspace = Workspace();
        var creation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(Path.Combine(workspace.LexicalRoot, "route")));

        var result = await Preflight().ValidateAsync(
            workspace,
            directoryCreations: [creation, creation],
            fileChanges: [],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Blocked, result.State);
        Assert.Empty(result.Checks);
    }

    [Fact(DisplayName = "Combined mutation preflight blocks a logical target shared by directory and file effects")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task CombinedPreflightBlocksCrossEffectLogicalTargets()
    {
        var workspace = Workspace();
        var path = Path.Combine(workspace.LexicalRoot, "target");

        var result = await Preflight().ValidateAsync(
            workspace,
            directoryCreations:
            [
                PlannedDirectoryCreation.Create(FileExpectation.Missing(path)),
            ],
            fileChanges:
            [
                PlannedFileChange.Create(FileExpectation.Missing(path), "content"u8),
            ],
            TestContext.Current.CancellationToken);

        Assert.Equal(MutationValidationState.Blocked, result.State);
        Assert.Empty(result.Checks);
    }

    [Fact(DisplayName = "Combined mutation preflight reserves the workspace root")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task CombinedPreflightBlocksReservedDirectoryEffects()
    {
        var workspace = Workspace();
        var rootCreation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(workspace.LexicalRoot));
        var rootResult = await Preflight().ValidateAsync(
            workspace,
            directoryCreations: [rootCreation],
            fileChanges: [],
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Blocked, rootResult.State);
        Assert.Empty(rootResult.Checks);
    }

    [Fact(DisplayName = "Combined mutation preflight observes cancellation before any target")]
    [Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
    public async Task CombinedPreflightReturnsCancelledBeforeObservation()
    {
        var workspace = Workspace();
        var creation = PlannedDirectoryCreation.Create(
            FileExpectation.Missing(Path.Combine(workspace.LexicalRoot, "route")));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await Preflight().ValidateAsync(
            workspace,
            directoryCreations: [creation],
            fileChanges: [],
            cancellation.Token);

        Assert.Equal(MutationValidationState.Cancelled, result.State);
        Assert.Empty(result.Checks);
    }

    private static MutationPreflight Preflight()
        => new(new FileExpectationValidator(new PhysicalPathResolver()));

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "directory-mutation-preflight-unit"));
        return new CliWorkspace(
            lexicalRoot: root,
            physicalRoot: root,
            selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }
}
