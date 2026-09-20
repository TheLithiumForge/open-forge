using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

internal sealed class DoctorOperationTestSupport
{
    internal DoctorOperationTestSupport(List<string>? calls = null)
    {
        Calls = calls ?? [];
        WorkspaceEntry = new RecordingWorkspaceEntryContributor(Calls);
        RecoveryResiduals = new RecordingRecoveryResidualContributor(Calls);
        Routes = new RecordingRouteContributor(Calls);
        LocalReferences = new RecordingLocalReferenceContributor(Calls);
        FrameworkLifecycle = new RecordingFrameworkLifecycleContributor(Calls);
        ExtensionLifecycle = new RecordingExtensionLifecycleContributor(Calls);
        Catalogue = new OperationalContributorCatalogue(
            WorkspaceEntry,
            RecoveryResiduals,
            Routes,
            LocalReferences,
            FrameworkLifecycle,
            ExtensionLifecycle,
            new LibraryOperationalContributor());
    }

    internal List<string> Calls { get; }

    internal RecordingWorkspaceEntryContributor WorkspaceEntry { get; }

    internal RecordingRecoveryResidualContributor RecoveryResiduals { get; }

    internal RecordingRouteContributor Routes { get; }

    internal RecordingLocalReferenceContributor LocalReferences { get; }

    internal RecordingFrameworkLifecycleContributor FrameworkLifecycle { get; }

    internal RecordingExtensionLifecycleContributor ExtensionLifecycle { get; }

    internal OperationalContributorCatalogue Catalogue { get; }

    internal sealed class RecordingWorkspaceEntryContributor(List<string> calls)
        : IWorkspaceEntryOperationalContributor
    {
        internal Exception? Failure { get; set; }

        public ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Doctor evidence must not invoke Status views.");

        public ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("workspace-entry");
            if (Failure is { } failure)
            {
                throw failure;
            }

            return ValueTask.FromResult(new WorkspaceEntryDoctorView(
                new WorkspaceEntryDoctorSummary(
                    OperationalViewState.Complete,
                    OperationalInstallationState.Uninstalled,
                    null,
                    null),
                new WorkspacePathObservationSet(
                    WorkspacePathObservation.Contained(
                        workspace.LexicalRoot,
                        PathComponentState.Ordinary,
                        FileAttributes.Directory),
                    WorkspacePathObservation.Unresolved(
                        ".agents",
                        PhysicalPathState.Missing),
                    WorkspacePathObservation.Unresolved(
                        ".agents/AGENTS.md",
                        PhysicalPathState.Missing),
                    WorkspacePathObservation.Unresolved(
                        ".agents/loader.md",
                        PhysicalPathState.Missing))));
        }
    }

    internal sealed class RecordingRecoveryResidualContributor(List<string> calls)
        : IRecoveryResidualOperationalContributor
    {
        internal RecoveryResidualDoctorView View { get; set; } = new(
            OperationalViewState.Complete,
            [],
            Cause: null);

        public ValueTask<RecoveryResidualStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Doctor evidence must not invoke Status views.");

        public ValueTask<RecoveryResidualDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("recovery-residuals");
            return ValueTask.FromResult(View);
        }
    }

    internal sealed class RecordingRouteContributor(List<string> calls)
        : IRouteOperationalContributor
    {
        internal RouteSourceInventoryState SourceInventory { get; set; } =
            RouteSourceInventoryState.SafelyAbsent;

        public ValueTask<RouteStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Doctor evidence must not invoke Status views.");

        public ValueTask<RouteDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("routes");
            return ValueTask.FromResult(new RouteDoctorView
            {
                State = OperationalViewState.Complete,
                SourceInventory = SourceInventory,
                Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
                Routes = new SourceRouteFacts(
                    new SourceRouteTopology([], []),
                    [],
                    [],
                    areLoaderRootFactsComplete: true,
                    isCancelled: false),
                Metadata = [],
                WorkspaceEntry = new RouteSourceLayerObservation(
                    "AGENTS.md",
                    FileReadState.Missing,
                    Text: null),
                Sources = [],
                GeneratedNavigation = [],
                DeclaredRoots = [],
                Shape = [],
            });
        }
    }

    internal sealed class RecordingLocalReferenceContributor(List<string> calls)
        : ILocalReferenceOperationalContributor
    {
        public ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("local-references");
            return ValueTask.FromResult(new LocalReferenceDoctorView(
                OperationalViewState.Complete,
                [],
                LocalReferenceCandidateScanState.Complete,
                [],
                []));
        }
    }

    internal sealed class RecordingFrameworkLifecycleContributor(List<string> calls)
        : IFrameworkLifecycleOperationalContributor
    {
        public ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            WorkspaceOwnershipRead ownership,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Doctor evidence must not invoke Status views.");

        public ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("framework-lifecycle");
            var ownership = WorkspaceOwnershipRead.Absent(
                Path.Combine(workspace.LexicalRoot, ".agents/open-forge.lock.json"));
            return ValueTask.FromResult(FrameworkLifecycleDoctorView.Create(
                FrameworkLifecycleDoctorAssessment.Create(
                    OperationalViewState.Incomplete,
                    OperationalLifecycleState.Incomplete,
                    OperationalSourceAvailability.Unavailable,
                    FrameworkManagedSetState.Empty),
                ownership,
                FrameworkPayloadReadResult.Unavailable("The unit fixture has no payload."),
                []));
        }
    }

    internal sealed class RecordingExtensionLifecycleContributor(List<string> calls)
        : IExtensionLifecycleOperationalContributor
    {
        internal ExtensionLifecycleDoctorView View { get; set; } = CreateDefaultView();

        public ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            WorkspaceOwnershipRead ownership,
            CancellationToken cancellationToken)
            => throw new InvalidOperationException("Doctor evidence must not invoke Status views.");

        public ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
        {
            calls.Add("extension-lifecycle");
            return ValueTask.FromResult(View);
        }

        private static ExtensionLifecycleDoctorView CreateDefaultView()
            => ExtensionLifecycleDoctorView.Create(
                ExtensionLifecycleDoctorAssessment.Create(
                    OperationalViewState.Incomplete,
                    ExtensionLifecycleSectionState.Unavailable,
                    ExtensionManagedSetState.Empty,
                    OperationalLifecycleState.Incomplete,
                    OperationalSourceAvailability.Unavailable),
                [],
                CreateCompleteOwnership(),
                ExtensionLifecycleDoctorFacts.Create([], []));
    }

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-doctor-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static WorkspaceOwnershipRead CreateCompleteOwnership()
        => new(WorkspaceOwnershipReadState.Complete,
            OpenForge.Cli.Core.Framework.Ownership.Models.Document.WorkspaceOwnershipDocument.Empty,
            Path.Combine(Workspace().LexicalRoot, ".agents/open-forge.lock.json"), null, null);

    internal static ExtensionSourceReadResult Source(
        ExtensionSourceReadState state,
        ExtensionSourceKind? kind,
        string identity,
        string? cause,
        ExtensionSourceFailureKind failureKind = ExtensionSourceFailureKind.None)
        => new(
            state,
            kind,
            identity,
            [],
            cause,
            failureKind);
}
