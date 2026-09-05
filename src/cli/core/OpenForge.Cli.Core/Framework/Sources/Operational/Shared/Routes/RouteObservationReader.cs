using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteObservationReader
{
    private readonly RouteGeneratedNavigationReader _generatedNavigationReader;
    private readonly RouteSourceInspector _sourceInspector;

    internal RouteObservationReader(
        RouteSourceInspector sourceInspector,
        RouteGeneratedNavigationReader generatedNavigationReader)
    {
        _sourceInspector = sourceInspector;
        _generatedNavigationReader = generatedNavigationReader;
    }

    internal async ValueTask<RouteStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read();
        var inspection = await _sourceInspector.ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var context = new RouteContextReader().Read(payload, inspection);
        var generated = _generatedNavigationReader.Read(workspace, payload, inspection);
        var state = ReadState(inspection.State, context.IsIncomplete);
        return new RouteStatusView
        {
            State = state,
            SourceInventory = ReadSourceInventory(inspection),
            InitialStartup = context.InitialStartup,
            CurrentStartup = context.CurrentStartup,
            TotalAvailable = context.TotalAvailable,
            Continuity = context.Continuity,
            ContinuitySources = context.ContinuitySources,
            InitialRootCategories = context.InitialRootCategories,
            CurrentRootCategories = context.CurrentRootCategories,
            GeneratedNavigation = generated,
        };
    }

    internal async ValueTask<RouteDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var payload = EmbeddedFrameworkPayloadReader.Read();
        var inspection = await _sourceInspector.ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var generated = _generatedNavigationReader.Read(workspace, payload, inspection);
        return new RouteDoctorView
        {
            State = inspection.State,
            Catalogue = inspection.Catalogue,
            Routes = inspection.Routes,
            Metadata = inspection.Sources.Select(source => new RouteMetadataObservation(
                source.Source.Identity.CanonicalBasePath,
                source.FrameworkMetadata)).ToArray(),
            GeneratedNavigation = generated,
        };
    }

    private static OperationalViewState ReadState(
        OperationalViewState inspection,
        bool contextIncomplete)
    {
        if (!contextIncomplete
            || inspection is OperationalViewState.Blocked or OperationalViewState.Interrupted)
        {
            return inspection;
        }

        return OperationalViewState.Incomplete;
    }

    private static RouteSourceInventoryState ReadSourceInventory(
        RouteSourceInspection inspection)
    {
        if (inspection.State == OperationalViewState.Interrupted)
        {
            return RouteSourceInventoryState.Interrupted;
        }

        if (inspection.State == OperationalViewState.Blocked)
        {
            return RouteSourceInventoryState.Blocked;
        }

        if (RouteSourceInspectionPolicy.IsSafelyAbsentSourceInventory(
                inspection.Catalogue))
        {
            return RouteSourceInventoryState.SafelyAbsent;
        }

        return RouteSourceInspectionPolicy.IsCatalogueComplete(inspection.Catalogue)
            ? RouteSourceInventoryState.Present
            : RouteSourceInventoryState.Incomplete;
    }
}
