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
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.OperationalContributors.Models;

public sealed class OperationalContributorCatalogueTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "The operational catalogue retains its seven concrete typed contributor references"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void CatalogueRetainsTheSevenConcreteTypedContributorReferences()
    {
        var workspaceEntry = new WorkspaceEntryContributor();
        var recoveryResiduals = new RecoveryResidualContributor();
        var routes = new RouteContributor();
        var localReferences = new LocalReferenceContributor();
        var frameworkLifecycle = new FrameworkLifecycleContributor();
        var extensionLifecycle = new ExtensionLifecycleContributor();
        var libraries = new LibraryOperationalContributor();
        var catalogue = new OperationalContributorCatalogue(
            workspaceEntry,
            recoveryResiduals,
            routes,
            localReferences,
            frameworkLifecycle,
            extensionLifecycle,
            libraries);

        Assert.Same(workspaceEntry, catalogue.WorkspaceEntry);
        Assert.Same(recoveryResiduals, catalogue.RecoveryResiduals);
        Assert.Same(routes, catalogue.Routes);
        Assert.Same(localReferences, catalogue.LocalReferences);
        Assert.Same(frameworkLifecycle, catalogue.FrameworkLifecycle);
        Assert.Same(extensionLifecycle, catalogue.ExtensionLifecycle);
        Assert.Same(libraries, catalogue.Libraries);
    }

    private sealed class WorkspaceEntryContributor : IWorkspaceEntryOperationalContributor
    {
        public ValueTask<WorkspaceEntryStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();

        public ValueTask<WorkspaceEntryDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private sealed class RecoveryResidualContributor : IRecoveryResidualOperationalContributor
    {
        public ValueTask<RecoveryResidualStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();

        public ValueTask<RecoveryResidualDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private sealed class RouteContributor : IRouteOperationalContributor
    {
        public ValueTask<RouteStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();

        public ValueTask<RouteDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private sealed class LocalReferenceContributor : ILocalReferenceOperationalContributor
    {
        public ValueTask<LocalReferenceDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private sealed class FrameworkLifecycleContributor : IFrameworkLifecycleOperationalContributor
    {
        public ValueTask<FrameworkLifecycleStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            WorkspaceOwnershipRead ownership,
            CancellationToken cancellationToken)
            => throw NotInvoked();

        public ValueTask<FrameworkLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private sealed class ExtensionLifecycleContributor : IExtensionLifecycleOperationalContributor
    {
        public ValueTask<ExtensionLifecycleStatusView> ReadStatusAsync(
            CliWorkspace workspace,
            WorkspaceOwnershipRead ownership,
            CancellationToken cancellationToken)
            => throw NotInvoked();

        public ValueTask<ExtensionLifecycleDoctorView> ReadDoctorAsync(
            CliWorkspace workspace,
            CancellationToken cancellationToken)
            => throw NotInvoked();
    }

    private static InvalidOperationException NotInvoked()
        => new("Catalogue identity evidence does not invoke contributor behavior.");
}
