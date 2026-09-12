using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Lifecycle.Operational;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Workspace.Operational;

namespace OpenForge.Cli.Core.Framework.OperationalContributors.Models;

internal sealed class OperationalContributorCatalogue(
    IWorkspaceEntryOperationalContributor workspaceEntry,
    IRecoveryResidualOperationalContributor recoveryResiduals,
    IRouteOperationalContributor routes,
    ILocalReferenceOperationalContributor localReferences,
    IFrameworkLifecycleOperationalContributor frameworkLifecycle,
    IExtensionLifecycleOperationalContributor extensionLifecycle,
    ILibraryOperationalContributor libraries)
{
    internal ILibraryOperationalContributor Libraries { get; } = libraries;

    internal IWorkspaceEntryOperationalContributor WorkspaceEntry { get; } = workspaceEntry;

    internal IRecoveryResidualOperationalContributor RecoveryResiduals { get; } = recoveryResiduals;

    internal IRouteOperationalContributor Routes { get; } = routes;

    internal ILocalReferenceOperationalContributor LocalReferences { get; } = localReferences;

    internal IFrameworkLifecycleOperationalContributor FrameworkLifecycle { get; } = frameworkLifecycle;

    internal IExtensionLifecycleOperationalContributor ExtensionLifecycle { get; } = extensionLifecycle;
}
