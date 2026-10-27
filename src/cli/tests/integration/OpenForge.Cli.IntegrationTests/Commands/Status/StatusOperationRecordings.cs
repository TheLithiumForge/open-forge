using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Status;

internal sealed record StatusWorkspaceInvocation(
    CliWorkspace Workspace,
    CancellationToken CancellationToken);

internal sealed record StatusLifecycleInvocation(
    CliWorkspace Workspace,
    WorkspaceOwnershipRead Ownership,
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
            ExtensionLifecycle,
            new LibraryOperationalContributor());
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
            CliWorkspace workspace,
            WorkspaceOwnershipRead ownership,
            CancellationToken cancellationToken)
        {
            Invocations.Add(new StatusLifecycleInvocation(workspace, ownership, cancellationToken));
            onObservation();
        }
    }

    internal sealed class RecordingWorkspaceEntryContributor
        : RecordingWorkspaceStatusContributor, IWorkspaceEntryOperationalContributor
    {
        public ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
            CliWorkspace workspace, WorkspaceLayoutRead observation,
            CancellationToken cancellationToken)
        {

            Record(workspace, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.WorkspaceEntry());
        }

        public ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
            CliWorkspace workspace, WorkspaceLayoutRead observation,
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
        public ValueTask<RouteOutsideRoutesObservation> ReadOutsideAsync(WorkspaceLayoutRead observation, CancellationToken cancellationToken)
            => throw new InvalidOperationException("This recording requires the existing empty-registration path.");

        public ValueTask<RouteStatusView> ReadStatusAsync(WorkspaceLayoutRead observation, SourceSharing? sharing,
            CancellationToken cancellationToken, RouteOutsideRoutesObservation retained)
            => throw new InvalidOperationException("This recording requires the existing empty-registration path.");

        public ValueTask<RouteDoctorView> ReadDoctorAsync(WorkspaceLayoutRead observation, SourceSharing? sharing,
            CancellationToken cancellationToken, RouteOutsideRoutesObservation retained)
            => throw DoctorViewException();

        public ValueTask<RouteStatusView> ReadStatusAsync(
            WorkspaceLayoutRead observation,
            SourceSharing? sharing,
            CancellationToken cancellationToken)
        {
            var workspace = observation.Layout?.Workspace ?? throw new InvalidOperationException();

            Record(workspace, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.Routes());
        }

        public ValueTask<RouteDoctorView> ReadDoctorAsync(
            WorkspaceLayoutRead observation,
            SourceSharing? sharing,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingLocalReferenceContributor : ILocalReferenceOperationalContributor
    {
        internal int Calls { get; private set; }

        public ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
            WorkspaceLayoutRead observation,
            CancellationToken cancellationToken,
            RouteDoctorView? routes = null)
        {
            var workspace = observation.Layout?.Workspace ?? throw new InvalidOperationException();

            Calls++;
            throw new InvalidOperationException("Status must not invoke local-reference facts.");
        }
    }

    internal sealed class RecordingFrameworkLifecycleContributor(Action onObservation)
        : RecordingLifecycleStatusContributor(onObservation), IFrameworkLifecycleOperationalContributor
    {
        public ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
            WorkspaceLayoutRead observation,
            CancellationToken cancellationToken)
        {
            var workspace = observation.Layout?.Workspace ?? throw new InvalidOperationException();
            var ownership = observation.Ownership;

            Record(workspace, ownership, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.FrameworkLifecycle());
        }

        public ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
            WorkspaceLayoutRead observation,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    internal sealed class RecordingExtensionLifecycleContributor(Action onObservation)
        : RecordingLifecycleStatusContributor(onObservation), IExtensionLifecycleOperationalContributor
    {
        public ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
            WorkspaceLayoutRead observation,
            CancellationToken cancellationToken)
        {
            var workspace = observation.Layout?.Workspace ?? throw new InvalidOperationException();
            var ownership = observation.Ownership;

            Record(workspace, ownership, cancellationToken);
            return ValueTask.FromResult(StatusOperationViewSeeds.ExtensionLifecycle());
        }

        public ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
            WorkspaceLayoutRead observation,
            CancellationToken cancellationToken)
            => throw DoctorViewException();
    }

    private static InvalidOperationException DoctorViewException()
        => new("Status evidence never invokes a Doctor view.");
}
