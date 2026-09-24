using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Models;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Permissions;

[Trait("Feature", "library-permissions"), Trait("Evidence", "Integration")]
public sealed class LibraryPermissionOperationIntegrationTests
{
    private const string PermissionPath = ".agents/open-forge.json";

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task LiveLeavesProposeImmediateParentsAndExactRootLeafWithoutWriting()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader("always\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var request = (await RequestAsync(workspace, allowPrompt: true)) with
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
        Assert.DoesNotContain(approval.ProposedScopes, scope => scope.Path is "." or ".apm" or ".apm/agents");
        Assert.Contains("everything under it", output.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("always, once, cancel:", output.ToString(), StringComparison.Ordinal);
        Assert.Contains(".apm/agents/team", output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain(".apm/agents/team/nested/c.md", output.ToString(), StringComparison.Ordinal);
        Assert.Equal("sentinel", input.ReadLine());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task RetiredLeavesRequestOnlyExactGrantsAndNeverRequireSourceAvailability()
    {
        using var workspace = new LibraryMutationWorkspace();
        Directory.Delete(workspace.Absolute(LibraryMutationWorkspace.SourceRoot), recursive: true);
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(TextReader.Null, output, canPrompt: false));
        var before = workspace.Snapshot();
        var request = (await RequestAsync(workspace, allowPrompt: false)) with
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

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Explicit grants are staged for empty targets without writing settings")]
    public async Task ExplicitGrantIsStagedForEmptyTargetsWithoutWritingSettings()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader("always\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: false));
        var before = workspace.Snapshot();
        var stage = await operation.DetermineAsync((await RequestAsync(workspace, allowPrompt: false)) with
        {
            Targets = [],
            ExplicitGrantPaths = ["docs", "docs/empty-target.md"],
        }, TestContext.Current.CancellationToken);

        Assert.Null(stage.Failure);
        Assert.Equal(WorkspacePermissionDecision.NotRequired, stage.Result.Decision);
        Assert.Equal(WorkspacePermissionAction.Create, stage.Result.Action);
        Assert.NotNull(stage.Change);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(string.Empty, output.ToString());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("no\n"), InlineData("\n"), InlineData("")]
    public static async Task DeclineCannotRememberGrantOrConsumeExtraInput(string answer)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader(answer);
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();

        var stage = await operation.DetermineAsync(await RequestAsync(workspace, allowPrompt: true), TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPermissionFailure.Declined, stage.Failure);
        Assert.Empty(Assert.IsType<LibraryPermissionApproval>(stage.Approval).ApprovedScopes);
        Assert.Null(stage.Change);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false, true), InlineData(true, false)]
    public static async Task NonInteractiveAdmissionDoesNotConsumeApprovalInput(bool allowPrompt, bool canPrompt)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader("always\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt));
        var before = workspace.Snapshot();

        var stage = await operation.DetermineAsync(await RequestAsync(workspace, allowPrompt), TestContext.Current.CancellationToken);

        Assert.Equal(LibraryPermissionFailure.Required, stage.Failure);
        Assert.Equal("always", input.ReadLine());
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("team-knowledge")]
    [InlineData("other")]
    public static async Task SharedGrantIgnoresLibraryIdentity(string id)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Write(PermissionPath, """{"allowInstallPaths":["docs"]}""");
        workspace.Write(".agents/open-forge.permissions.json", "{ malformed retired file");
        using var input = new StringReader("unread\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var request = (await RequestAsync(workspace, allowPrompt: true)) with
        {
            LibraryId = LibraryId.Create(id),
        };
        var stage = await operation.DetermineAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(stage.Failure);
        Assert.Equal(WorkspacePermissionDecision.Granted, stage.Result.Decision);
        var extensionPrompt = LibraryPermissionTestPrompt.Create(input, output, canPrompt: true);
        var extension = await new ExtensionPermissionOperation(extensionPrompt)
            .DetermineAsync(new(workspace.Workspace, [new("docs/a.md", ExtensionPermissionEffect.Copy)], "different-extension-source", true), TestContext.Current.CancellationToken);
        Assert.Null(extension.Failure);
        Assert.Equal(WorkspacePermissionDecision.Granted, extension.Result.Decision);
        Assert.Null(extension.Change);
        Assert.Null(stage.Change);
        Assert.Equal("unread", input.ReadLine());
        Assert.Equal(string.Empty, output.ToString());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task UnderLeasePermissionChangeInvalidatesObservationWithoutEffects(bool originallyMissing)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-permission-revalidation");
        if (!originallyMissing)
        {
            workspace.Write(PermissionPath, """
                {"allowInstallPaths":["docs"]}
                """);
        }
        var operation = workspace.Permissions;
        var observation = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
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
            GrantChange = null,
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
            workspace.Write(PermissionPath, """{"allowInstallPaths":[]}""");
        }
        else
        {
            workspace.Replace(PermissionPath, """{"allowInstallPaths":[]}""");
        }
        var before = workspace.Snapshot();

        var unchanged = await LibraryPermissionOperation.RevalidateAsync(lease, stage, TestContext.Current.CancellationToken);

        Assert.False(unchanged);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public static async Task UnderLeaseRemovalChangeInvalidatesObservationEvenWithoutGrantOrPlannedWrite()
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("library-removal-revalidation");
        workspace.Write(PermissionPath, "{\"removedLibraries\":[]}");
        var observation = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        var stage = new LibraryPermissionStage
        {
            Observation = observation,
            Approval = null,
            Result = WorkspacePermissionResult.NotEvaluated,
            Change = null,
            GrantChange = null,
            RecoveryTarget = null,
            Failure = null,
        };
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new WorkspaceLockRequest(workspace.Workspace, "library sync", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);

        workspace.Replace(PermissionPath, "{\"removedLibraries\":[\"team-knowledge\"]}");
        var before = workspace.Snapshot();

        var unchanged = await LibraryPermissionOperation.RevalidateAsync(lease, stage, TestContext.Current.CancellationToken);

        Assert.False(unchanged);
        Assert.Null(stage.Change);
        Assert.Null(stage.GrantChange);
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public static async Task AlwaysGrantAndLibraryRemovalUseOneChangeFromExactOriginalSnapshot()
    {
        using var workspace = new LibraryMutationWorkspace();
        const string original = "{\"unknown\":{\"keep\":true},\"allowInstallPaths\":[],\"removedLibraries\":[]}";
        workspace.Write(PermissionPath, original);
        var originalBytes = File.ReadAllBytes(workspace.Absolute(PermissionPath));
        using var input = new StringReader("always\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var request = (await RequestAsync(workspace, allowPrompt: true)) with
        {
            Targets = [new("docs/retired.md", LibraryPermissionTargetUse.Retired) { Effect = LibraryPermissionEffect.RemoveLink }],
            RemovalSelection = new WorkspaceRemovalSelection { Libraries = ["team-knowledge"] },
        };

        var stage = await operation.DetermineAsync(request, TestContext.Current.CancellationToken);

        Assert.Null(stage.Failure);
        Assert.Equal(WorkspacePermissionDecision.Approved, stage.Result.Decision);
        Assert.NotNull(stage.GrantChange);
        var change = Assert.IsType<PlannedFileChange>(stage.Change);
        Assert.Equal(originalBytes, stage.RecoveryTarget?.Before.Bytes.ToArray());
        using var intended = JsonDocument.Parse(change.IntendedBytes.ToArray());
        Assert.True(intended.RootElement.GetProperty("unknown").GetProperty("keep").GetBoolean());
        Assert.Equal("docs/retired.md", Assert.Single(intended.RootElement.GetProperty("allowInstallPaths").EnumerateArray()).GetString());
        Assert.Equal("team-knowledge", Assert.Single(intended.RootElement.GetProperty("removedLibraries").EnumerateArray()).GetString());
        Assert.Equal("sentinel", input.ReadLine());
        Assert.Equal(originalBytes, File.ReadAllBytes(workspace.Absolute(PermissionPath)));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task UndefinedTargetUseCannotBecomeAnAutomaticDirectoryProposal()
    {
        using var workspace = new LibraryMutationWorkspace();
        var request = (await RequestAsync(workspace, allowPrompt: false)) with
        {
            Targets = [new("docs/a.md", (LibraryPermissionTargetUse)int.MaxValue) { Effect = LibraryPermissionEffect.CreateLink }],
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await workspace.Permissions.DetermineAsync(request, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("once", true, false)]
    [InlineData("always", true, true)]
    [InlineData("cancel", false, false)]
    public static async Task ApprovalChoiceSeparatesOperationAdmissionFromPersistentGrant(string answer, bool approved, bool persistent)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var input = new StringReader(answer + "\nsentinel\n");
        using var output = new StringWriter();
        var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
        var before = workspace.Snapshot();
        var stage = await operation.DetermineAsync(await RequestAsync(workspace, allowPrompt: true), TestContext.Current.CancellationToken);
        Assert.Equal(approved ? WorkspacePermissionDecision.Approved : WorkspacePermissionDecision.Declined, stage.Result.Decision);
        Assert.Equal(persistent, stage.Change is not null);
        Assert.Equal(persistent, stage.RecoveryTarget is not null);
        Assert.Equal(persistent ? WorkspacePermissionAction.Create : WorkspacePermissionAction.None, stage.Result.Action);
        Assert.Equal("sentinel", input.ReadLine());
        Assert.Equal(before, workspace.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false, false), InlineData(false, true)]
    [InlineData(true, false), InlineData(true, true)]
    public static async Task AlwaysGrantCannotPublishBeforeRecovery(bool extension, bool existing)
    {
        using var workspace = new LibraryMutationWorkspace();
        using var locks = WorkspaceLockTestStore.Create("shared-grant-recovery");
        if (existing)
        {
            workspace.Write(PermissionPath, "{\"keep\":true}");
        }
        using var input = new StringReader("always\n");
        using var output = new StringWriter();
        var interaction = LibraryPermissionTestPrompt.Create(input, output, canPrompt: true);
        await using var lease = Assert.IsType<WorkspaceLockLease>((await locks.AcquireAsync(
            new(workspace.Workspace, "permission test", Guid.NewGuid()), TestContext.Current.CancellationToken)).Lease);
        var before = workspace.Snapshot();
        if (extension)
        {
            var operation = new ExtensionPermissionOperation(interaction);
            var stage = await operation.DetermineAsync(new(workspace.Workspace, [new("docs/a.md", ExtensionPermissionEffect.Copy)], null, true), TestContext.Current.CancellationToken);
            Assert.NotNull(stage.Change);
            Assert.NotNull(stage.RecoveryTarget);
            var result = await operation.ApplyAsync(lease, stage, recovery: null, TestContext.Current.CancellationToken);
            Assert.Equal(ExtensionPermissionFailure.WriteFailed, result.Failure);
            Assert.Equal(WorkspacePermissionOutcome.NotStarted, result.Result.Outcome);
        }
        else
        {
            var operation = new LibraryPermissionOperation(LibraryPermissionTestPrompt.Create(input, output, canPrompt: true));
            var stage = await operation.DetermineAsync(await RequestAsync(workspace, allowPrompt: true), TestContext.Current.CancellationToken);
            Assert.NotNull(stage.Change);
            Assert.NotNull(stage.RecoveryTarget);
            var result = await LibraryPermissionOperation.ApplyAsync(lease, stage, recovery: null, TestContext.Current.CancellationToken);
            Assert.Equal(LibraryPermissionFailure.WriteFailed, result.Failure);
            Assert.Equal(WorkspacePermissionOutcome.NotStarted, result.Result.Outcome);
        }
        Assert.Equal(before, workspace.Snapshot());
    }

    private static async ValueTask<LibraryPermissionRequest> RequestAsync(LibraryMutationWorkspace workspace, bool allowPrompt)
        => new()
        {
            Workspace = workspace.Workspace,
            LibraryId = LibraryId.Create("team-knowledge"),
            SettingsObservation = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken),
            Targets = [new("docs/a.md", LibraryPermissionTargetUse.Live) { Effect = LibraryPermissionEffect.CreateLink }],
            AllowPrompt = allowPrompt,
        };
}
