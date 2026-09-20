using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;

using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Operational;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal static class RepairOperationalContributorFactory
{
    internal static OperationalContributorCatalogue Create(PhysicalPathResolver physicalPathResolver)
    {
        var sourceSessionReader = new SourceReadSessionReader(physicalPathResolver);
        var routeSourceInspector = new RouteSourceInspector(sourceSessionReader, physicalPathResolver);
        return new OperationalContributorCatalogue(
            new WorkspaceEntryOperationalContributor(new WorkspacePathObserver(physicalPathResolver)),
            new RecoveryResidualOperationalContributor(
                new RecoveryBundleTargetStateReader(physicalPathResolver),
                physicalPathResolver),
            new RouteOperationalContributor(
                new RouteObservationReader(
                    routeSourceInspector,
                    new RouteGeneratedNavigationReader(physicalPathResolver),
                    new RouteDoctorGeneratedNavigationReader())),
            new LocalReferenceOperationalContributor(
                routeSourceInspector,
                new SourceLinkDestinationResolver(
                    (workspace, lexicalPath) => physicalPathResolver.ResolveCandidate(
                        workspace.LexicalRoot,
                        workspace.PhysicalRoot,
                        lexicalPath),
                    StrictUtf8FileReader.ReadAsync,
                    new MarkdownDocumentParser().Parse),
                new LocalReferenceCandidateReader()),
            new FrameworkLifecycleOperationalContributor(
                physicalPathResolver,
                new FrameworkLifecycleTargetReader(physicalPathResolver)),
            new ExtensionLifecycleOperationalContributor(
                physicalPathResolver,
                new ExtensionSourceReader(physicalPathResolver),
                new ExtensionLifecycleTargetReader(physicalPathResolver)),
            new LibraryOperationalContributor());
    }
}
