using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Sharing;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;

internal static class DoctorMaintenanceRouteProjection
{
    internal static RouteDoctorView Read(RouteDoctorView routes, DoctorMaintenanceObservation maintenance, SourceSharing? sharing)
    {
        if (maintenance.MalformedMetadata.IsEmpty) return routes;
        var sources = routes.Sources.Select(source => maintenance.MalformedMetadata.ContainsKey(source.Source.Identity.CanonicalBasePath)
            ? source with { AuthoredMetadata = SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed) } : source).ToArray();
        var navigation = new RouteDoctorGeneratedNavigationReader().Read(new()
        {
            Catalogue = routes.Catalogue,
            Routes = routes.Routes,
            Sources = sources,
            State = routes.State,
            WorkspaceEntry = routes.WorkspaceEntry,
        }, sharing);
        return routes with { Sources = sources, GeneratedNavigation = navigation };
    }
}
