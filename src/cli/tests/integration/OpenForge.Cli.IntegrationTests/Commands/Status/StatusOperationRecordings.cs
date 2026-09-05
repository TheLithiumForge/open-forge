using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Operational;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed record StatusWorkspaceInvocation(
    CliWorkspace Workspace,
    CancellationToken CancellationToken);

internal sealed record StatusLifecycleInvocation(
    LifecycleDocumentSnapshot Snapshot,
    CancellationToken CancellationToken);

internal sealed class StatusOperationRecordings
{
    private readonly Action _onFirstLifecycleObservation;
    private int _lifecycleObservations;

    internal StatusOperationRecordings(Action onFirstLifecycleObservation)
    {
        _onFirstLifecycleObservation = onFirstLifecycleObservation;
        WorkspaceEntry = new RecordingWorkspaceEntryContributor();
        RecoveryResiduals = new RecordingRecoveryResidualContributor();
        Routes = new RecordingRouteContributor();
        LocalReferences = new RecordingLocalReferenceContributor();
        FrameworkLifecycle = new RecordingFrameworkLifecycleContributor(ObserveLifecycle);
        ExtensionLifecycle = new RecordingExtensionLifecycleContributor(ObserveLifecycle);
        Catalogue = new OperationalContributorCatalogue(
            WorkspaceEntry,
            RecoveryResiduals,
            Routes,
            LocalReferences,
            FrameworkLifecycle,
            ExtensionLifecycle);
    }

    internal RecordingWorkspaceEntryContributor WorkspaceEntry { get; }

    internal RecordingRecoveryResidualContributor RecoveryResiduals { get; }

    internal RecordingRouteContributor Routes { get; }

    internal RecordingLocalReferenceContributor LocalReferences { get; }

    internal RecordingFrameworkLifecycleContributor FrameworkLifecycle { get; }

    internal RecordingExtensionLifecycleContributor ExtensionLifecycle { get; }

    internal OperationalContributorCatalogue Catalogue { get; }

    private void ObserveLifecycle()
    {
        if (Interlocked.Increment(ref _lifecycleObservations) == 1)
        {
            _onFirstLifecycleObservation();
        }
    }

    internal abstract class RecordingWorkspaceStatusContributor
    {
        internal List<StatusWorkspaceInvocation> Invocations { get; } = [];

        protected void Record(CliWorkspace workspace, CancellationToken cancellationToken)
            => Invocations.Add(new StatusWorkspaceInvocation(workspace, cancellationToken));
    }

    internal abstract class RecordingLifecycleStatusContributor(Action onObservation)
    {
        internal List<StatusLifecycleInvocation> Invocations { get; } = [];

        protected void Record(
            LifecycleDocumentSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            Invocations.Add(new StatusLifecycleInvocation(snapshot, cancellationToken));
            onObservation();
        }
    }

    internal sealed class RecordingWorkspaceEntryContributor
        : RecordingWorkspaceStatusContributor, IWorkspaceEntryOperationalContributor
    {
        public ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            Record(workspace, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.WorkspaceEntry());
        }

        public ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingRecoveryResidualContributor
        : RecordingWorkspaceStatusContributor, IRecoveryResidualOperationalContributor
    {
        public ValueTask<RecoveryResidualStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            Record(workspace, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.RecoveryResiduals());
        }

        public ValueTask<RecoveryResidualDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingRouteContributor
        : RecordingWorkspaceStatusContributor, IRouteOperationalContributor
    {
        public ValueTask<RouteStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            Record(workspace, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.Routes());
        }

        public ValueTask<RouteDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingLocalReferenceContributor : ILocalReferenceOperationalContributor
    {
        internal int Calls { get; private set; }

        public ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            Calls++;
            throw new InvalidOperationException("Status must not invoke local-reference facts.");
        }
    }

    internal sealed class RecordingFrameworkLifecycleContributor(Action onObservation)
        : RecordingLifecycleStatusContributor(onObservation), IFrameworkLifecycleOperationalContributor
    {
        public ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
            LifecycleDocumentSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            Record(snapshot, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.FrameworkLifecycle());
        }

        public ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingExtensionLifecycleContributor(Action onObservation)
        : RecordingLifecycleStatusContributor(onObservation), IExtensionLifecycleOperationalContributor
    {
        public ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
            LifecycleDocumentSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            Record(snapshot, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.ExtensionLifecycle());
        }

        public ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    private static InvalidOperationException DoctorViewException()
        => new("Status evidence never invokes a Doctor view.");
}
