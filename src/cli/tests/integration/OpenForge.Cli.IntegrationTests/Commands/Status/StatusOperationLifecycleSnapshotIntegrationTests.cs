using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

public sealed class StatusOperationLifecycleSnapshotIntegrationTests
{
    [Fact(DisplayName = "Status forwards one immutable lifecycle observation to both lifecycle contributors and refreshes the next invocation")]
    [Trait("Feature", "status-command"), Trait("Evidence", "Integration")]
    public async Task StatusForwardsOneImmutableLifecycleObservationToBothContributorsAndRefreshesTheNextInvocation()
    {
        using var workspace = StatusIntegrationWorkspace.Create("status-operation-lifecycle-snapshot");
        var initialBytes = "{\"snapshot\":\"initial\"}"u8.ToArray();
        var replacementBytes = "{\"snapshot\":\"replacement\"}"u8.ToArray();
        workspace.WriteBytes(StatusIntegrationWorkspace.LifecyclePath, initialBytes);
        var recordings = new StatusOperationRecordings(() =>
            workspace.ReplaceBytes(StatusIntegrationWorkspace.LifecyclePath, replacementBytes));
        var operation = new StatusOperation(
            recordings.Catalogue,
            new LifecycleDocumentSnapshotReader(new PhysicalPathResolver()));
        using var firstCancellation = new CancellationTokenSource();
        using var secondCancellation = new CancellationTokenSource();

        var firstResult = await operation.ExecuteAsync(new(workspace.Workspace), firstCancellation.Token);

        Assert.Same(workspace.Workspace, firstResult.Workspace);
        AssertWorkspaceInvocation(recordings.WorkspaceEntry.Invocations, 0, workspace.Workspace, firstCancellation.Token);
        AssertWorkspaceInvocation(recordings.RecoveryResiduals.Invocations, 0, workspace.Workspace, firstCancellation.Token);
        AssertWorkspaceInvocation(recordings.Routes.Invocations, 0, workspace.Workspace, firstCancellation.Token);
        var firstFramework = Assert.Single(recordings.FrameworkLifecycle.Invocations);
        var firstExtension = Assert.Single(recordings.ExtensionLifecycle.Invocations);
        Assert.Same(firstFramework.Snapshot, firstExtension.Snapshot);
        AssertLifecycleInvocation(firstFramework, workspace.Workspace, firstCancellation.Token, initialBytes);
        AssertLifecycleInvocation(firstExtension, workspace.Workspace, firstCancellation.Token, initialBytes);
        Assert.Equal(replacementBytes, File.ReadAllBytes(workspace.Combine(StatusIntegrationWorkspace.LifecyclePath)));
        Assert.Equal(0, recordings.LocalReferences.Calls);

        var secondResult = await operation.ExecuteAsync(new(workspace.Workspace), secondCancellation.Token);

        Assert.Same(workspace.Workspace, secondResult.Workspace);
        AssertWorkspaceInvocation(recordings.WorkspaceEntry.Invocations, 1, workspace.Workspace, secondCancellation.Token);
        AssertWorkspaceInvocation(recordings.RecoveryResiduals.Invocations, 1, workspace.Workspace, secondCancellation.Token);
        AssertWorkspaceInvocation(recordings.Routes.Invocations, 1, workspace.Workspace, secondCancellation.Token);
        Assert.Equal(2, recordings.FrameworkLifecycle.Invocations.Count);
        Assert.Equal(2, recordings.ExtensionLifecycle.Invocations.Count);
        var secondFramework = recordings.FrameworkLifecycle.Invocations[1];
        var secondExtension = recordings.ExtensionLifecycle.Invocations[1];
        Assert.Same(secondFramework.Snapshot, secondExtension.Snapshot);
        Assert.NotSame(firstFramework.Snapshot, secondFramework.Snapshot);
        AssertLifecycleInvocation(secondFramework, workspace.Workspace, secondCancellation.Token, replacementBytes);
        AssertLifecycleInvocation(secondExtension, workspace.Workspace, secondCancellation.Token, replacementBytes);
        Assert.NotEqual(firstFramework.Snapshot.File?.ContentHash, secondFramework.Snapshot.File?.ContentHash);
        Assert.Equal(0, recordings.LocalReferences.Calls);
    }

    private static void AssertWorkspaceInvocation(
        IReadOnlyList<StatusWorkspaceInvocation> invocations,
        int index,
        CliWorkspace expectedWorkspace,
        CancellationToken expectedToken)
    {
        Assert.Equal(index + 1, invocations.Count);
        Assert.Same(expectedWorkspace, invocations[index].Workspace);
        Assert.Equal(expectedToken, invocations[index].CancellationToken);
    }

    private static void AssertLifecycleInvocation(
        StatusLifecycleInvocation invocation,
        CliWorkspace expectedWorkspace,
        CancellationToken expectedToken,
        byte[] expectedBytes)
    {
        Assert.Equal(LifecycleDocumentSnapshotState.Available, invocation.Snapshot.State);
        Assert.Same(expectedWorkspace, invocation.Snapshot.Workspace);
        Assert.Equal(expectedToken, invocation.CancellationToken);
        var file = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.FileStateSnapshot>(
            invocation.Snapshot.File);
        Assert.Equal(expectedBytes, file.Bytes.ToArray());
    }
}
