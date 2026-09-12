using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteObservationReader
{
    private readonly RouteGeneratedNavigationReader _generatedNavigationReader;
    private readonly RouteDoctorGeneratedNavigationReader _doctorGeneratedNavigationReader;
    private readonly RouteSourceInspector _sourceInspector;

    internal RouteObservationReader(
        RouteSourceInspector sourceInspector,
        RouteGeneratedNavigationReader generatedNavigationReader,
        RouteDoctorGeneratedNavigationReader doctorGeneratedNavigationReader)
    {
        _sourceInspector = sourceInspector;
        _generatedNavigationReader = generatedNavigationReader;
        _doctorGeneratedNavigationReader = doctorGeneratedNavigationReader;
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
        var inspection = await _sourceInspector.ReadAsync(workspace, cancellationToken)
            .ConfigureAwait(false);
        var generated = _doctorGeneratedNavigationReader.Read(inspection);
        return new RouteDoctorView
        {
            State = inspection.State,
            SourceInventory = ReadSourceInventory(inspection),
            Catalogue = inspection.Catalogue,
            Routes = inspection.Routes,
            Metadata = inspection.Sources.Select(source => new RouteMetadataObservation(
                source.Source.Identity.CanonicalBasePath,
                source.FrameworkMetadata)).ToArray(),
            WorkspaceEntry = inspection.WorkspaceEntry,
            Sources = inspection.Sources,
            GeneratedNavigation = generated,
            DeclaredRoots = RouteDoctorFactReader.ReadDeclaredRoots(inspection.Routes),
            Shape = RouteDoctorFactReader.ReadShape(inspection),
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
