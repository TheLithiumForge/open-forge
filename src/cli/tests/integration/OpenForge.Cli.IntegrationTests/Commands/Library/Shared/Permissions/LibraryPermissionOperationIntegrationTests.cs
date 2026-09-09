using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Permissions.Models.Planning;
using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using OpenForge.Cli.Core.Framework.Permissions.Shared.Observation;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Integration")]
public sealed class LibraryPermissionOperationIntegrationTests
{
    private const string PermissionPath = ".agents/open-forge.permissions.json";

    [Fact]
    public async Task LiveLeavesProposeImmediateParentsAndExactRootLeafWithoutWriting()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader("yes\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var request = Request(workspace, allowPrompt: true) with
        {
            Targets =
            [
                new("README.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
                new(".apm/agents/team/a.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
                new(".apm/agents/team/b.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
                new(".apm/agents/team/nested/c.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
                new("docs/guide.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
                new(".agents/directives/internal.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink },
            ],
        };

        var stage = await operation.DetermineAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(stage.Failure);
        Assert.Equal(WorkspacePermissionDecision.Approved, stage.Result.Decision);
        var approval = Assert.IsType<LibraryPermissionApproval>(stage.Approval);
        Assert.Equal(5, approval.Leaves.Required.Length);
        Assert.Equal([".apm/agents/team", "README.md", "docs"], approval.ProposedScopes.Select(scope => scope.Path));
        Assert.Equal(approval.ProposedScopes, approval.ApprovedScopes);
        Assert.Equal(LibraryPermissionScopeKind.File, Assert.Single(approval.ApprovedScopes, scope => scope.Path == "README.md").Kind);
        Assert.All(approval.ApprovedScopes.Where(scope => scope.Path != "README.md"),
            scope => Assert.Equal(LibraryPermissionScopeKind.Directory, scope.Kind));
        Assert.All(approval.ApprovedScopes, scope => Assert.Equal(LibraryMutationWorkspace.SourceRoot, scope.Subject.SourceRoot));
        Assert.DoesNotContain(approval.ProposedScopes, scope => scope.Path is "." or ".apm" or ".apm/agents");
        Assert.Contains("future", output.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("descendants", output.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".apm/agents/team", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(".apm/agents/team/nested/c.md", output.ToString(), StringComparison.Ordinal);
        Assert.Equal("sentinel", input.ReadLine());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact]
    public async Task RetiredLeavesRequestOnlyExactGrantsAndNeverRequireSourceAvailability()
    {
        using var workspace = new LibraryMutationWorkspace();
        Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(new CliInteractiveSession(TextReader.Null, output, canPrompt: false));
        var before = workspace.Snapshot();
        var request = Request(workspace, allowPrompt: false) with
        {
            Targets = [new("docs/retired.md", LibraryPermissionTargetUse.Retired) { Effect = LibraryPermissionEffect.RemoveLink }],
        };

        var stage = await operation.DetermineAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPermissionFailure.Required, stage.Failure);
        var approval = Assert.IsType<LibraryPermissionApproval>(stage.Approval);
        var scope = Assert.Single(approval.ProposedScopes);
        Assert.Equal(LibraryPermissionScopeKind.File, scope.Kind);
        Assert.Equal("docs/retired.md", scope.Path);
        Assert.Empty(approval.ApprovedScopes);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory]
    [InlineData("no\n"), InlineData("\n"), InlineData("")]
    public static async Task DeclineCannotRememberGrantOrConsumeExtraInput(string answer)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader(answer);
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt: true));
        var before = workspace.Snapshot();

        var stage = await operation.DetermineAsync(Request(workspace, allowPrompt: true), TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPermissionFailure.Declined, stage.Failure);
        Assert.Empty(Assert.IsType<LibraryPermissionApproval>(stage.Approval).ApprovedScopes);
        Assert.Null(stage.Change);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory]
    [InlineData(false, true), InlineData(true, false)]
    public static async Task NonInteractiveAdmissionDoesNotConsumeApprovalInput(bool allowPrompt, bool canPrompt)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader("yes\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt));
        var before = workspace.Snapshot();

        var stage = await operation.DetermineAsync(Request(workspace, allowPrompt), TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPermissionFailure.Required, stage.Failure);
        Assert.Equal("yes", input.ReadLine());
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task RebindingDisclosesOldAndNewRootsAndRequiresExplicitApproval(bool approve)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Write(PermissionPath, """
            {"schemaVersion":1,"extensions":[{"id":"team-knowledge","paths":["extension.txt"]}],
             "libraries":[{"id":"other","sourceRoot":"shared/other","paths":["keep.txt"],"directories":[]},
              {"id":"team-knowledge","sourceRoot":"shared/old","paths":[],"directories":["docs"]}]}
            """);
        using var input = new StringReader(approve ? "yes\nsentinel\n" : "no\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(new CliInteractiveSession(input, output, canPrompt: true));
        var before = workspace.Snapshot();

        var stage = await operation.DetermineAsync(Request(workspace, allowPrompt: true), TestContext.Current.CancellationToken);

        var approval = Assert.IsType<LibraryPermissionApproval>(stage.Approval);
        var rebinding = Assert.IsType<LibraryPermissionRebinding>(approval.Rebinding);
        Assert.Equal("shared/old", rebinding.PreviousSourceRoot);
        Assert.Equal(LibraryMutationWorkspace.SourceRoot, rebinding.SourceRoot);
        Assert.Contains("shared/old", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(LibraryMutationWorkspace.SourceRoot, output.ToString(), StringComparison.Ordinal);
        Assert.Contains("replace", output.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(approve ? WorkspacePermissionDecision.Approved : WorkspacePermissionDecision.Declined, stage.Result.Decision);
        Assert.Equal(approve ? 1 : 0, approval.ApprovedScopes.Length);
        Assert.Equal("sentinel", input.ReadLine());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task UnderLeasePermissionChangeInvalidatesObservationWithoutEffects(bool originallyMissing)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-permission-revalidation");
        if (!originallyMissing)
        {
            workspace.Write(PermissionPath, """
                {"schemaVersion":1,"extensions":[],"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","paths":[],"directories":["docs"]}]}
                """);
        }
        var operation = workspace.Permissions;
        var observation = await WorkspacePermissionReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        if (!originallyMissing)
        {
            Assert.NotNull(observation.Document);
        }
        Assert.NotNull(observation.Snapshot);
        var stage = new LibraryPermissionStage
        {
            Observation = observation,
            Approval = null,
            Result = WorkspacePermissionResult.NotEvaluated,
            Change = null,
            RecoveryTarget = null,
            Failure = null,
        };
        var acquired = await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, "library sync", Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceLockState.Acquired, acquired.State);
        Assert.NotNull(acquired.Lease);
        await using var lease = acquired.Lease;
        if (originallyMissing)
        {
            workspace.Write(PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
        }
        else
        {
            workspace.Replace(PermissionPath, """{"schemaVersion":1,"extensions":[],"libraries":[]}""");
        }
        var before = workspace.Snapshot();

        var unchanged = await LibraryPermissionOperation.RevalidateAsync(lease, stage, TestContext.Current.CancellationToken);

        Assert.False(unchanged);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Fact]
    public async Task UndefinedTargetUseCannotBecomeAnAutomaticDirectoryProposal()
    {
        using var workspace = new LibraryMutationWorkspace();
        var request = Request(workspace, allowPrompt: false) with
        {
            Targets = [new("docs/a.md", (LibraryPermissionTargetUse)int.MaxValue) { Effect = LibraryPermissionEffect.CreateLink }],
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await workspace.Permissions.DetermineAsync(request, TestContext.Current.CancellationToken));
    }

    private static LibraryPermissionRequest Request(LibraryMutationWorkspace workspace, bool allowPrompt)
        => new()
        {
            Workspace = workspace.Workspace,
            Library = LibraryRecord.Create(LibraryId.Create("team-knowledge"),
                WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot),
                LibraryDestinationRoot.Create("docs"), [SourceRelativeEligiblePath.Create("a.md")]),
            Targets = [new("docs/a.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink }],
            AllowPrompt = allowPrompt,
        };
}
